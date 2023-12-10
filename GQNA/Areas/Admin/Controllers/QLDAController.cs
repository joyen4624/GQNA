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
    public class QLDAController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();

        // GET: Admin/QLDA
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        // GET: Admin/QLDA/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Admin/QLDA/Create
        public ActionResult Create()
        {
            Project project = new Project(); // Hoặc lấy dữ liệu từ database
            ViewBag.ListNHT = new SelectList(db.tbl_DT, "id_DT", "name_DT");
            ViewBag.ListTopics = new MultiSelectList(db.tbl_CD, "id_CD", "name_CD");
            return View(project);
        }

        // POST: Admin/QLDA/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "id_project,name_project,img_project,des_project,project_time_start,project_time_end,id_DT,project_amount,SelectedTopics")] Project project, HttpPostedFileBase img_project, List<HttpPostedFileBase> imgFiles)
        {
            ViewBag.ListNHT = new SelectList(db.tbl_DT, "id_DT", "name_DT", project.id_DT);
            ViewBag.ListTopics = new MultiSelectList(db.tbl_CD, "id_CD", "name_CD", project.SelectedTopics);
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý tải lên ảnh nếu có
                    if (img_project != null && img_project.ContentLength > 0)
                    {
                        // Đường dẫn lưu trữ ảnh
                        string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgProject"), Path.GetFileName(img_project.FileName));

                        // Lưu ảnh vào thư mục
                        img_project.SaveAs(imagePath);

                        // Lưu đường dẫn ảnh vào trường img_project của đối tượng Project
                        project.img_project = "/Content/images/ImgProject/" + Path.GetFileName(img_project.FileName);
                    }
                   
                    // Lưu dự án vào cơ sở dữ liệu
                    db.Projects.Add(project);
                    db.SaveChanges();
                        
                    // Thêm hình ảnh cho dự án
                    foreach (var file in imgFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgProject"), Path.GetFileName(file.FileName));
                            file.SaveAs(imagePath);

                            // Lưu đường dẫn ảnh vào bảng tbl_img
                            tbl_img img = new tbl_img
                            {
                                id_project = project.id_project,
                                url_img = "/Content/images/ImgProject/" + Path.GetFileName(file.FileName)
                            };

                            db.tbl_img.Add(img);
                            db.SaveChanges();
                        }
                    }

                    if (project.SelectedTopics != null && project.SelectedTopics.Any())
                    {
                        foreach (var topicId in project.SelectedTopics)
                        {
                            tbl_get_CD getCD = new tbl_get_CD
                            {
                                id_Project = project.id_project,
                                id_CD = topicId
                            };

                            db.tbl_get_CD.Add(getCD);
                        }

                        db.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Xử lý nếu có lỗi trong quá trình lưu dự án hoặc ảnh
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dự án: " + ex.Message);
                }
            }

            return View(project);
        }


        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    // Đường dẫn lưu trữ ảnh
                    string imagePath = Path.Combine(Server.MapPath("~/Content/images/ImgProject"), Path.GetFileName(upload.FileName));

                    // Lưu ảnh vào thư mục
                    upload.SaveAs(imagePath);

                    // Trả về đường dẫn ảnh mới để CKEditor hiển thị
                    string imageUrl = "/Content/images/ImgProject/" + Path.GetFileName(upload.FileName);
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


        // GET: Admin/QLDA/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Admin/QLDA/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_project,name_project,img_project,des_project,project_time_start,project_time_end,id_NHT,project_amount")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Admin/QLDA/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Admin/QLDA/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
