using GoLinks.Database;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoLinks.Models.Views
{
    public class BrowseModel
    {
        [BindProperty]
        public List<GoLink> GoLinks { get; set; } = new();

        public BrowseModel(GoLinkContext dbContext, string queryString = "")
        {
            var goLinks = dbContext.GoLinks;

            var paramKvps = HttpUtility.ParseQueryString(queryString);
            if (string.IsNullOrEmpty(queryString))
            {
                
                GoLinks = goLinks.OrderBy(x => x.NumUses).Take(50).ToList();
            }
            else if (paramKvps.Count > 0)
            {
                int page = 1;
                int limit = 100;
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
                        goLinks.Where(x => x.Owner.StartsWith(value));
                    }
                    else if (param == nameof(GoLink.ShortLink).ToLowerInvariant())
                    {
                        goLinks.Where(x => x.ShortLink.StartsWith(value));
                    }
                    else if (param == "page")
                    {
                        _ = int.TryParse(value, out page);
                    }
                }

                GoLinks = goLinks
                    .OrderBy(x => x.NumUses)
                    .Skip(limit * (page - 1))
                    .Take(limit)
                    .ToList();
            }
        }
    }
}
