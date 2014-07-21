using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackExchange.Redis;
using Microsoft.AspNet.Identity;

using DeathRay.Helpers;

namespace DeathRay.Controllers
{
    [Authorize]
    [RequireHttps]
    public class MinionWorkController : Controller
    {
        public ActionResult Index()
        {
            var minionClickTotal = MinionClickTotalHelper.GetMinionClickTotal(User.Identity.GetUserName());
            return View(minionClickTotal.ClickTotal);
        }
    }
}