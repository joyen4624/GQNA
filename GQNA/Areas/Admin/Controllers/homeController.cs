using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GQNA.Areas.Admin.Controllers
{
    public class homeController : Controller
    {
        // GET: Admin/home
        public ActionResult Index()
        {
            return View();
        }
    }
}