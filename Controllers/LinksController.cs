using Microsoft.AspNetCore.Mvc;

namespace GoLinks.Controllers
{
    public class LinksController : Controller
    {
        [HttpGet]
        [Route("Links/Browse")]
        public IActionResult Browse()
        {
            return View("Browse", new Models.Views.BrowseModel(Request.QueryString.Value ?? ""));
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
            model.OnPost();
            return View(model);
        }
    }
}
