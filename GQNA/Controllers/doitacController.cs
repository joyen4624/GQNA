using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GQNA.Models;

namespace GQNA.Controllers
{
    public class doitacController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();

        // GET: doitac
        public ActionResult Index()
        {
            return View(db.tbl_DT.ToList());
        }

        // GET: doitac/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_DT = db.tbl_DT.Find(id);
            if (tbl_DT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_DT);
        }

        public ActionResult chitiet(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_DT = db.tbl_DT.Find(id);
            tbl_DT doiTac = db.tbl_DT.Include("Projects").SingleOrDefault(p => p.id_DT == id);
            if (tbl_DT == null)
            {
                return HttpNotFound();
            }
            return View(doiTac);
        }

        // GET: doitac/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: doitac/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_DT,name_DT,avatar_DT,background_DT,des_DT")] tbl_DT tbl_DT)
        {
            if (ModelState.IsValid)
            {
                db.tbl_DT.Add(tbl_DT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_DT);
        }

        // GET: doitac/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_DT = db.tbl_DT.Find(id);
            if (tbl_DT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_DT);
        }

        // POST: doitac/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_DT,name_DT,avatar_DT,background_DT,des_DT")] tbl_DT tbl_DT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_DT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_DT);
        }

        // GET: doitac/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_DT = db.tbl_DT.Find(id);
            if (tbl_DT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_DT);
        }

        // POST: doitac/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_DT tbl_DT = db.tbl_DT.Find(id);
            db.tbl_DT.Remove(tbl_DT);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
