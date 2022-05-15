using Microsoft.AspNetCore.Mvc;
using LiteDB;
using System.Linq;

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

        public void OnPost()
        {
            if (!LinkRequest.IsValid(out string errorMessage))
            {
                ErrorMessage = errorMessage;
                SuccessMessage = null;
                return;
            }

            using (LiteDatabase db = new("Data/Links.db"))
            {
                var linkCollection = db.GetCollection<GoLink>();
                linkCollection.EnsureIndex(nameof(GoLink.ShortLink), unique: true);
                linkCollection.EnsureIndex(nameof(GoLink.Owner), unique: false);
                linkCollection.EnsureIndex(nameof(GoLink.NumUses), unique: false);

                var existingLink = linkCollection
                    .Find(link => link.ShortLink == LinkRequest.ShortLink)
                    .FirstOrDefault();

                if (!(existingLink is null))
                {
                    ErrorMessage = 
                        $"Go link <a href=\"/Links/Edit?{existingLink.Id}\">go/{existingLink.ShortLink}</a> " +
                        $"already exists.";
                    SuccessMessage = null;
                    return;
                }

                linkCollection.Insert(new GoLink(LinkRequest));
            }


            SuccessMessage = $"Success! go/{LinkRequest.ShortLink} now points to {LinkRequest.DestinationLink}.";
            ErrorMessage = null;
        }
    }
}
