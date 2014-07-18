using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeathRay.Controllers
{
    [Authorize]
    [RequireHttps]
    public class MinionWorkController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}