using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GoLinks.Models
{
    [Index(nameof(Id), nameof(ShortLink), nameof(Owner), nameof(NumUses))]
    public class GoLink
    {
        /// <summary>
        /// The primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        [BindProperty, Required]
        public string Owner { get; set; }

        [BindProperty, Required, DisplayName("Short Link Path")]
        public string ShortLink { get; set; }

        [BindProperty, Required, DisplayName("Destination Link")]
        public string DestinationLink { get; set; }

        [BindProperty]
        public int NumUses { get; set; } = 0;

        [Obsolete("Should only be called via reflection")]
        public GoLink() { }

        public GoLink(NewGoLinkRequest newRequest)
        {
            Owner = newRequest.Owner
                ?? throw new ArgumentException($"{nameof(newRequest.Owner)} not present");
            ShortLink = newRequest.ShortLink
                ?? throw new ArgumentException($"{nameof(newRequest.ShortLink)} not present");
            DestinationLink = newRequest.DestinationLink
                ?? throw new ArgumentException($"{nameof(newRequest.DestinationLink)} not present");
        }
    }

    public class NewGoLinkRequest
    {
        /// <summary>
        /// A regex that matches the scheme portion of a URL. Note that we allow for more than just
        /// http:// and https://, which gives us the flexibility to link to applications opened on
        /// the desktop, such as Slack or other shared custom schemes.
        /// </summary>
        /// <remarks>
        /// See https://www.ietf.org/rfc/rfc1738.txt#:~:text=The%20main%20parts%20of%20URLs.
        /// </remarks>
        private readonly Regex schemeRegex = new Regex(@"^[a-z\+\.-]+://");

        // TODO: Have a better lookup for this elsewhere.
        private static readonly HashSet<string> InvalidNames = new()
        {
            // Home Page
            "",
            // The pages used to modify or view links
            "Links",
        };

        [BindProperty, Required]
        public string? Owner { get; set; }

        [BindProperty, Required, DisplayName("Short Link Path")]
        public string? ShortLink { get; set; }


        private string? destinationLink;

        [BindProperty, Required, DisplayName("Destination Link")]
        public string? DestinationLink
        { 
            get
            {
                return destinationLink;
            }
            set
            {
                destinationLink = value;
                // Add http:// by default if users forget to put it there. If it's https,
                // they can always go back later and edit it.
                if (!(destinationLink is null) && !schemeRegex.IsMatch(destinationLink))
                {
                    destinationLink = $"http://{value}";
                    return;
                }
                destinationLink = value;
            }
        }

        public NewGoLinkRequest()
        {
        }

        public NewGoLinkRequest(GoLink goLink)
        {
            Owner = goLink.Owner;
            ShortLink = goLink.ShortLink;
            DestinationLink = goLink.DestinationLink;
        }

        public bool IsValid([NotNullWhen(false)] out string errorMessage)
        {
            if (Owner is null)
            {
                errorMessage = $"{nameof(Owner)} is missing from the link.";
                return false;
            }

            if (ShortLink is null)
            {
                errorMessage = $"{nameof(ShortLink)} is missing from the link.";
                return false;
            }

            string firstPart = ShortLink.Split('/')[0];
            if (InvalidNames.Contains(firstPart))
            {
                errorMessage = $"{ShortLink} cannot start with \"{firstPart}/\"";
                return false;
            }

            if (DestinationLink is null)
            {
                errorMessage = $"{nameof(DestinationLink)} is missing from the link.";
                return false;
            }

            string cleanedDestinationLink = Regex.Replace(DestinationLink, "\\^{[^{}]+}", "placeholder");

            if (!Uri.IsWellFormedUriString(cleanedDestinationLink, UriKind.RelativeOrAbsolute))
            {
                errorMessage = $"{DestinationLink} must be URL encoded (aside from formatting strings).";
                return false;
            }

            // TODO: Check if the Owner actually exists (don't want a misspelling).

            errorMessage = "";
            return true;
        }
    }
}
