using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApp.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}