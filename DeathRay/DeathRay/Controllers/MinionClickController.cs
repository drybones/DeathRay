using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

using DeathRay.Models;

namespace DeathRay.Controllers
{
    public class MinionClickController : Controller
    {
        // GET: MinionClick
        [Authorize]
        [RequireHttps]
        public ActionResult Click()
        {
            var minionClick = new MinionClick() { Minion = User.Identity.GetUserName(), Timestamp = DateTime.Now };
            return Json(minionClick);
        }
    }
}