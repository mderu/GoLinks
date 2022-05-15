using GoLinks.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace GoLinks.Models.Views
{
    public class BrowseModel
    {
        [BindProperty]
        public List<GoLink> GoLinks { get; set; } = new();

        public BrowseModel()
        {
            using LiteDatabase db = new("Data/Links.db");
            var linkCollection = db.GetCollection<GoLink>();
            GoLinks = linkCollection.Query().OrderBy(x => x.NumUses).Limit(50).ToList();
        }
    }
}
