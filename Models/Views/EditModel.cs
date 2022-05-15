using Microsoft.AspNetCore.Mvc;
using LiteDB;
using System;

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

        public void ApplyEdit(int id)
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
            using LiteDatabase db = new("Data/Links.db");
            var linkCollection = db.GetCollection<GoLink>();
            LinkToEdit = linkCollection
                .Query()
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
            linkCollection.Update(LinkToEdit.Id, LinkToEdit);

            SuccessMessage = $"Success! go/{LinkToEdit.ShortLink} has the following edits: {modifications.Trim()}.";
            ErrorMessage = null;
        }

        public bool Delete(int id)
        {
            using LiteDatabase db = new("Data/Links.db");
            var linkCollection = db.GetCollection<GoLink>();
            return linkCollection.Delete(id);
        }
    }
}
