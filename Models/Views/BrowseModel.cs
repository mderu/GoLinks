using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Web;

namespace GoLinks.Models.Views
{
    public class BrowseModel
    {
        [BindProperty]
        public List<GoLink> GoLinks { get; set; } = new();

        public BrowseModel(string queryString = "")
        {
            using LiteDatabase db = new("Data/Links.db");
            var linkCollection = db.GetCollection<GoLink>();

            var paramKvps = HttpUtility.ParseQueryString(queryString);
            if (string.IsNullOrEmpty(queryString))
            {
                GoLinks = linkCollection.Query().OrderBy(x => x.NumUses).Limit(50).ToList();
            }
            else if (paramKvps.Count > 0)
            {
                int page = 1;
                int limit = 100;
                var query = linkCollection.Query();
                foreach (var paramKvp in paramKvps)
                {
                    string param = paramKvp.ToString() ?? "";
                    string? value = paramKvps[param];
                    if (value is null)
                    {
                        continue;
                    }
                    param = param.ToLowerInvariant();

                    if (param == nameof(GoLink.Owner).ToLowerInvariant())
                    {
                        query.Where(x => x.Owner.StartsWith(value));
                    }
                    else if (param == nameof(GoLink.ShortLink).ToLowerInvariant())
                    {
                        query.Where(x => x.ShortLink.StartsWith(value));
                    }
                    else if (param == "page")
                    {
                        _ = int.TryParse(value, out page);
                    }
                }

                GoLinks = query
                    .OrderBy(x => x.NumUses)
                    .Offset(limit * (page - 1))
                    .Limit(limit)
                    .ToList();
            }
        }
    }
}
