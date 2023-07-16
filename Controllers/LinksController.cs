using GoLinks.Database;
using Microsoft.AspNetCore.Mvc;

namespace GoLinks.Controllers
{
    public class LinksController : Controller
    {
        private readonly GoLinkContext dbContext;

        public LinksController(GoLinkContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("Links/Browse")]
        public IActionResult Browse()
        {
            return View("Browse", new Models.Views.BrowseModel(dbContext, Request.QueryString.Value ?? ""));
        }

        [HttpGet]
        [Route("Links/New")]
        public IActionResult New()
        {
            return View(new Models.Views.NewModel());
        }

        [HttpPost]
        [Route("Links/New")]
        public IActionResult New(Models.Views.NewModel model)
        {
            model.OnPost(dbContext);
            return View(model);
        }
    }
}
