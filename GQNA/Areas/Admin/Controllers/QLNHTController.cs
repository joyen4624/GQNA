using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GQNA.Models;

namespace GQNA.Areas.Admin.Controllers
{
    public class QLNHTController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();

        // GET: Admin/QLNHT
        public ActionResult Index()
        {
            return View(db.tbl_DT.ToList());
        }

        // GET: Admin/QLNHT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_NHT = db.tbl_DT.Find(id);
            if (tbl_NHT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_NHT);
        }

        // GET: Admin/QLNHT/Create
        public ActionResult Create()
        {
            tbl_DT tbl_NHT = new tbl_DT(); // Hoặc lấy dữ liệu từ database
            return View(tbl_NHT);
        }

        // POST: Admin/QLNHT/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_DT,name_DT,avatar_DT,background_DT,des_DT")] tbl_DT tbl_NHT, HttpPostedFileBase img_nht, HttpPostedFileBase back_nht)
        {
            if (ModelState.IsValid)
            {
                if (img_nht != null && img_nht.ContentLength > 0)
                {
                    // Đường dẫn lưu trữ ảnh
                    string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgNHT"), Path.GetFileName(img_nht.FileName));

                    // Lưu ảnh vào thư mục
                    img_nht.SaveAs(imagePath);

                    // Lưu đường dẫn ảnh vào trường img_project của đối tượng Project
                    tbl_NHT.avatar_DT = "/Content/images/ImgNHT/" + Path.GetFileName(img_nht.FileName);
                }

                if (back_nht != null && back_nht.ContentLength > 0)
                {
                    // Đường dẫn lưu trữ ảnh
                    string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgNHT"), Path.GetFileName(back_nht.FileName));

                    // Lưu ảnh vào thư mục
                    back_nht.SaveAs(imagePath);

                    // Lưu đường dẫn ảnh vào trường img_project của đối tượng Project
                    tbl_NHT.background_DT = "/Content/images/ImgNHT/" + Path.GetFileName(back_nht.FileName);
                }

                db.tbl_DT.Add(tbl_NHT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_NHT);
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    // Đường dẫn lưu trữ ảnh
                    string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgNHT"), Path.GetFileName(upload.FileName));

                    // Lưu ảnh vào thư mục
                    upload.SaveAs(imagePath);

                    // Trả về đường dẫn ảnh mới để CKEditor hiển thị
                    string imageUrl = "/Content/images/ImgNHT/" + Path.GetFileName(upload.FileName);
                    return Json(new { uploaded = 1, fileName = upload.FileName, url = imageUrl });
                }
                else
                {
                    return Json(new { uploaded = 0, error = new { message = "Không có tệp tin được tải lên." } });
                }
            }
            catch (Exception ex)
            {
                return Json(new { uploaded = 0, error = new { message = ex.Message } });
            }
        }

        // GET: Admin/QLNHT/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_NHT = db.tbl_DT.Find(id);
            if (tbl_NHT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_NHT);
        }

        // POST: Admin/QLNHT/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_NHT,name_NHT,avatar_NHT,background_NHT,des_NHT")] tbl_DT tbl_NHT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_NHT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_NHT);
        }

        // GET: Admin/QLNHT/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_DT tbl_NHT = db.tbl_DT.Find(id);
            if (tbl_NHT == null)
            {
                return HttpNotFound();
            }
            return View(tbl_NHT);
        }

        // POST: Admin/QLNHT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_DT tbl_NHT = db.tbl_DT.Find(id);
            db.tbl_DT.Remove(tbl_NHT);
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
