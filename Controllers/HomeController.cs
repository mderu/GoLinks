using GoLinks.Database;
using GoLinks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace GoLinks.Controllers
{
    public class HomeController : Controller
    {
        private readonly GoLinkContext dbContext;
        public HomeController(GoLinkContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        // Our Redirect route. Note that we allow slashes to allow users to create directories.
        [HttpGet]
        [Route("/{a}")]
        [Route("/{a}/{b}")]
        [Route("/{a}/{b}/{c}")]
        [Route("/{a}/{b}/{c}/{d}")]
        [Route("/{a}/{b}/{c}/{d}/{e}")]
        [Route("/{a}/{b}/{c}/{d}/{e}/{f}")]
        [Route("/{a}/{b}/{c}/{d}/{e}/{f}/{g}")]
        public IActionResult HandleUrl(
            [FromRoute] string a,
            [FromRoute] string b,
            [FromRoute] string c,
            [FromRoute] string d,
            [FromRoute] string e,
            [FromRoute] string f,
            [FromRoute] string g
        )
        {
            string queryString = Request.QueryString.Value ?? "";
            string fullPath = string.Join(
                "/",
                new List<string> { a, b, c, d, e, f, g }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            );

            GoLink? link;
            link = dbContext.GoLinks.FirstOrDefault(l => l.ShortLink == fullPath);

            if (link is null)
            {
                int lastSlash = fullPath.LastIndexOf('/');
                lastSlash = lastSlash < 0 ? fullPath.Length : lastSlash;
                string prefix = fullPath[..lastSlash];

                string newUrl = QueryHelpers.AddQueryString("/Links/Browse", nameof(GoLink.ShortLink), prefix);
                return Redirect(newUrl);
            }

            link.NumUses += 1;
            dbContext.GoLinks.Update(link);
            dbContext.SaveChanges();

            return Redirect(ApplyFormatting(link.DestinationLink, queryString));

        }

        private string ApplyFormatting(string destinationUrl, string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                return destinationUrl;
            }
            string[] paramArray = queryString.Length > 0
                ? queryString[1..].Split('&')
                : Array.Empty<string>();
            var paramKvps = HttpUtility.ParseQueryString(queryString);

            bool hasFormattingChange = false;
            while (true)
            {
                var match = Regex.Match(destinationUrl, "\\^{(?<Query>[^{}]+)}");
                if (!match.Success)
                {
                    break;
                }
                hasFormattingChange = true;
                string query = match.Groups["Query"].Value;
                string replacement;
                if (query == "*")
                {
                    replacement = queryString.Trim('?');
                }
                else if (int.TryParse(query, out int result))
                {
                    try
                    {
                        replacement = paramArray[result];
                    }
                    catch (Exception)
                    {
                        replacement = "";
                    }
                }
                else
                {
                    try
                    {
                        replacement = paramKvps[query] ?? "";
                    }
                    catch (Exception)
                    {
                        replacement = "";
                    }
                }
                destinationUrl = destinationUrl.Replace(
                    match.Groups[0].Value,
                    replacement,
                    comparisonType: StringComparison.OrdinalIgnoreCase);
            }
            if (!hasFormattingChange)
            {
                destinationUrl += queryString;
            }
            return destinationUrl;
        }
    }
}
