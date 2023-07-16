using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GoLinks.Database;

namespace GoLinks.Models.Views
{
    public class NewModel
    {
        [BindProperty]
        public NewGoLinkRequest LinkRequest { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        public NewModel()
        {
            LinkRequest = new NewGoLinkRequest();
            SuccessMessage = null;
            ErrorMessage = null;
        }

        public void OnPost(GoLinkContext dbContext)
        {
            if (!LinkRequest.IsValid(out string errorMessage))
            {
                ErrorMessage = errorMessage;
                SuccessMessage = null;
                return;
            }

            var goLinks = dbContext.GoLinks;

            var existingLink = goLinks
                .Where(link => link.ShortLink == LinkRequest.ShortLink)
                .FirstOrDefault();

            if (!(existingLink is null))
            {
                ErrorMessage = 
                    $"Go link <a href=\"/Links/Edit?{existingLink.Id}\">go/{existingLink.ShortLink}</a> " +
                    $"already exists.";
                SuccessMessage = null;
                return;
            }

            goLinks.Add(new GoLink(LinkRequest));
            dbContext.SaveChanges();

            SuccessMessage = $"Success! go/{LinkRequest.ShortLink} now points to {LinkRequest.DestinationLink}.";
            ErrorMessage = null;
        }
    }
}
