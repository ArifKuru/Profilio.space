using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CvBuilder.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        // GET: Landing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult gdpr()
        {
            return View();
        }

        public ActionResult privacy()
        {
            return View();
        }


        public ActionResult terms()
        {
            return View();
        }

        public ActionResult disclaimer()
        {
            return View();
        }

    }
}