using Microsoft.AspNetCore.Mvc;
using System;
using GoLinks.Database;
using System.Linq;

namespace GoLinks.Models.Views
{
    public class EditModel
    {
        [BindProperty]
        public GoLink LinkToEdit { get; private set; }

        [BindProperty]
        public NewGoLinkRequest LinkModification { get; private set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [Obsolete("Should only be called via reflection")]
        public EditModel()
        {
            LinkModification = new();
        }

        public EditModel(GoLink linkToEdit)
        {
            LinkToEdit = linkToEdit;
            LinkModification = new NewGoLinkRequest(linkToEdit);
            SuccessMessage = null;
            ErrorMessage = null;
        }

        public void ApplyEdit(GoLinkContext dbContext, int id)
        {
            if (!LinkModification.IsValid(out string errorMessage))
            {
                ErrorMessage = errorMessage;
                SuccessMessage = null;
                return;
            }

            // The actual LinkToEdit originally passed into the model is not passed back and forth from the server
            // to client and back between the GET and POST calls. We'll need to fetch this link again.
            //
            // In a way, this limitation forces us to implement a feature that allows us to edit a link without
            // knowing the exact details of what the link's properties are.
            var goLinks = dbContext.GoLinks;
            LinkToEdit = goLinks
                .Where(link => link.Id == id)
                .First();
            if (LinkToEdit is null)
            {
                SuccessMessage = null;
                ErrorMessage = $"Unable to find link with id \"{id}\".";
                return;
            }

            // Forgivenesses: Validation above has already guaranteed these are not null.
            string modifications = (LinkToEdit.Owner != LinkModification.Owner!) 
                ? $"Owner: {LinkToEdit.Owner} => {LinkModification.Owner}\n" : "";
            modifications += (LinkToEdit.ShortLink != LinkModification.ShortLink!)
                ? $"Short Link: {LinkToEdit.ShortLink} => {LinkModification.ShortLink}\n" : "";
            modifications += (LinkToEdit.DestinationLink != LinkModification.DestinationLink!)
                ? $"Destination Link: {LinkToEdit.DestinationLink} => {LinkModification.DestinationLink}" : "";

            if (modifications.Length == 0)
            {
                ErrorMessage = "User input matches the link in the database. No changes were made.";
                SuccessMessage = null;
                return;
            }

            LinkToEdit.Owner = LinkModification.Owner!;
            LinkToEdit.ShortLink = LinkModification.ShortLink!;
            LinkToEdit.DestinationLink = LinkModification.DestinationLink!;
            goLinks.Update(LinkToEdit);
            dbContext.SaveChanges();

            SuccessMessage = $"Success! go/{LinkToEdit.ShortLink} has the following edits: {modifications.Trim()}.";
            ErrorMessage = null;
        }

        public bool Delete(GoLinkContext dbContext, int id)
        {
            GoLink? foundElement = dbContext.GoLinks.FirstOrDefault(goLink => goLink.Id == id);
            if (foundElement is not null)
            {
                dbContext.GoLinks.Remove(foundElement);
                dbContext.SaveChanges();
            }
            return foundElement is not null;
        }
    }
}
