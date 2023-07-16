using Microsoft.AspNetCore.Mvc;
using GoLinks.Models;
using GoLinks.Database;
using System.Linq;

namespace GoLinks.Controllers.Edit
{
    public class LinksController : Controller
    {
        private readonly GoLinkContext dbContext;
        public LinksController(GoLinkContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("Links/Edit/View/{id?}")]
        public IActionResult Edit(int? id)
        {
            if (id is null)
            {
                return RerouteFromEditToNew();
            }

            // If the link they were trying to edit is invalid, reroute them to New.
            /*if (!TryGetIdFromQueryString(out int id))
            {
                return RerouteFromEditToNew();
            }*/

            GoLink? linkToEdit = dbContext.GoLinks
                .Where(link => link.Id == id)
                .FirstOrDefault();

            if (linkToEdit is null)
            {
                return RerouteFromEditToNew();
            }

            return View(new Models.Views.EditModel(linkToEdit));
        }

        public IActionResult Apply(int? id, Models.Views.EditModel model)
        {
            if (id is null)
            {
                return RerouteFromEditToNew();
            }

            model.ApplyEdit(dbContext, id.Value);
            return View("Edit", model);
        }

        [HttpPost]
        [Route("Links/Edit/Delete/{id?}")]
        public IActionResult Delete(int? id, Models.Views.EditModel model)
        {
            if (id is null)
            {
                return RerouteFromEditToNew();
            }

            if (model.Delete(dbContext, id.Value))
            {
                return View("Browse", new Models.Views.BrowseModel(dbContext));
            }
            model.ErrorMessage = $"Unable to delete link #{id}, unknown ID.";
            model.SuccessMessage = null;
            return View(model);
        }

        private bool TryGetIdFromQueryString(out int id)
        {
            return int.TryParse(Request.QueryString.Value?.Trim('?') ?? "", out id);
        }

        private IActionResult RerouteFromEditToNew()
        {
            var newModel = new Models.Views.NewModel()
            {
                ErrorMessage = $"Invalid link to edit \"{Request.QueryString.Value}\"."
            };
            return View("New", newModel);
        }
    }
}
