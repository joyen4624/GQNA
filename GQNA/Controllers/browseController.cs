using GQNA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GQNA.Controllers
{
    public class browseController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();
        // GET: browse
        public ActionResult home()
        {
            return View();
        }
        public ActionResult detail(int? id)
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


            if (project.project_time_end <= DateTime.Now)
            {
                // Cập nhật trạng thái của dự án là đã kết thúc trong database
                project.status = "Kết thúc";
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }

            tbl_DT partner = project.tbl_DT;
            ViewBag.PartnerId = partner.id_DT;
            ViewBag.PartnerName = partner.name_DT;
            ViewBag.PartnerBack = partner.background_DT;
            ViewBag.PartnerLogo = partner.avatar_DT;

            var contributors = db.tbl_QG
       .Where(qg => qg.id_project == id)
       .OrderByDescending(qg => qg.NgayQuyenGop)
       .Take(5) // Lấy 5 người quyên góp gần đây nhất
       .Select(qg => new DoanterView
       {
           TenNguoiQuyenGop = qg.tbl_NHT.name_NHT,
           SoTienQuyenGop = qg.ThanhTien ?? 0,
           SoDienThoai = qg.tbl_NHT.phone_NHT
       })
       .ToList();

            TimeSpan remainingDays = (TimeSpan)(project.project_time_end - DateTime.Now);
            int daysLeft = (int)Math.Ceiling(remainingDays.TotalDays);

            decimal totalAmountContributed = contributors.Sum(c => c.SoTienQuyenGop);
            decimal percentContributed = (decimal)((totalAmountContributed / project.project_amount) * 100);
            percentContributed = Math.Round(percentContributed, 2);

            string formattedPercentContributed = percentContributed.ToString("N2"); // Format lại thành chuỗi với hai chữ số sau dấu thập phân

            string formattedAmount = string.Format("{0:N0}VNĐ", project.project_amount);
            ViewBag.FormattedAmount = formattedAmount;
            ViewBag.Contributors = contributors;
            ViewBag.TotalAmountContributed = totalAmountContributed;
            ViewBag.PercentContributed = formattedPercentContributed;
            ViewBag.DaysLeft = daysLeft;

           


            return View("Detail", project);
        }




        public ActionResult projectSlider()
        {
            List<Project> projects = db.Projects.ToList();

            foreach (var project in projects)
            {
                // Tính tổng tiền đã nhận quyên góp
                decimal totalContributed = db.tbl_QG
                    .Where(qg => qg.id_project == project.id_project)
                    .Sum(qg => qg.ThanhTien) ?? 0;

                // Tính phần trăm quyên góp
                int percentContributed = 0;

                // Kiểm tra xem project.project_amount có giá trị không
                if (project.project_amount.HasValue && project.project_amount.Value != 0)
                {
                    // Tính phần trăm quyên góp
                    percentContributed = (int)((totalContributed / project.project_amount.Value) * 100);
                }

                // Tính số ngày còn lại
                TimeSpan remainingDays = (TimeSpan)(project.project_time_end - DateTime.Now);
                
                int numberOfContributions = db.tbl_QG
           .Count(qg => qg.id_project == project.id_project);

                // Gán các giá trị vào ViewBag
                project.TotalAmount = project.project_amount;
                project.TotalContributed = totalContributed;
                project.PercentContributed = percentContributed;
                project.RemainingDays = remainingDays.Days;
                project.totalSL = numberOfContributions;
            }

            return PartialView(projects);
        }

       


        public ActionResult SliderPage()
        {
            List<tbl_img> images = db.tbl_img.ToList();

            // Trộn ngẫu nhiên danh sách ảnh
            Random random = new Random();
            List<tbl_img> shuffledImages = images.OrderBy(x => random.Next()).Take(5).ToList();

            return PartialView(shuffledImages);
        }

        public ActionResult ChooseProject()
        {
            return PartialView();
        }

        public ActionResult LastEvent()
        {
            return PartialView();
        }

        public ActionResult List(int id_CD, int page = 1, int pageSize = 6)
        {
            // Tính toán số lượng bản ghi bỏ qua (skip)
            int skip = (page - 1) * pageSize;

            List<tbl_get_CD> projects = db.tbl_get_CD
                .Where(cd => cd.id_CD == id_CD)
                .OrderByDescending(cd => cd.Project.id_project)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Chuyển đổi từ tbl_get_CD sang ProjectView
            List<ProjectView> projectViews = projects.Select(cd => new ProjectView
            {
                Id = cd.Project.id_project,
                Name = cd.Project.name_project,
                back = cd.Project.img_project,
                TotalAmount = cd.Project.project_amount,
                TotalContributed = db.tbl_QG
                    .Where(qg => qg.id_project == cd.Project.id_project)
                    .Sum(qg => qg.ThanhTien) ?? 0,
                PercentContributed = CalculatePercentContributed(cd.Project.id_project),
                RemainingDays = CalculateRemainingDays(cd.Project.project_time_end),
                TotalSL = db.tbl_QG.Count(qg => qg.id_project == cd.Project.id_project)
            }).ToList();

            // Tính toán tổng số trang
            int totalProjects = db.tbl_get_CD.Count(cd => cd.id_CD == id_CD);
            int totalPages = (int)Math.Ceiling((double)totalProjects / pageSize);

            // Thêm các thông tin vào ViewBag
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.id_CD = id_CD;
            ViewBag.TotalPages = totalPages;

            // Kiểm tra xem có trang tiếp theo hay không
            ViewBag.HasNextPage = (page * pageSize) < totalProjects;

            return View(projectViews);
        }

        // Helper method to calculate percent contributed
        private int CalculatePercentContributed(int projectId)
        {
            Project project = db.Projects.Find(projectId);
            decimal totalContributed = db.tbl_QG
                .Where(qg => qg.id_project == projectId)
                .Sum(qg => qg.ThanhTien) ?? 0;

            if (project.project_amount.HasValue && project.project_amount.Value != 0)
            {
                return (int)((totalContributed / project.project_amount.Value) * 100);
            }

            return 0;
        }

        // Helper method to calculate remaining days
        private int CalculateRemainingDays(DateTime? projectEndDate)
        {
            if (projectEndDate.HasValue)
            {
                TimeSpan remainingDays = (TimeSpan)(projectEndDate.Value - DateTime.Now);
                return remainingDays.Days;
            }

            return 0;
        }


        public ActionResult tatcaduan(int page = 1, int pageSize = 6)
        {
            // Tính toán số lượng bản ghi bỏ qua (skip)
            int skip = (page - 1) * pageSize;

            // Lấy danh sách tất cả dự án từ bảng Project
            List<Project> projects = db.Projects
                .OrderByDescending(p => p.id_project) // Sắp xếp theo thứ tự giảm dần để lấy những dự án mới nhất
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tính toán tổng số trang
            int totalProjects = db.Projects.Count();
            int totalPages = (int)Math.Ceiling((double)totalProjects / pageSize);

            // Thêm các thông tin vào ViewBag
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;

            // Kiểm tra xem có trang tiếp theo hay không
            ViewBag.HasNextPage = (page * pageSize) < totalProjects;

            return View(projects);
        }



        public ActionResult fact()
        {
            int totalProjects = db.Projects.Count();
            ViewBag.TotalProjects = totalProjects;

            decimal totalSuccessPayment = db.tbl_QG
                .Where(qg => qg.TrangThai == "Thành công" && qg.ThanhTien != null)
                .Sum(qg => qg.ThanhTien) ?? 0;

            var formattedAmount = FormatAmount(totalSuccessPayment);

            ViewBag.TotalSuccessPaymentValue = formattedAmount.Item1;
            ViewBag.TotalSuccessPaymentUnit = formattedAmount.Item2;

            int totalSuccessContributions = db.tbl_QG
       .Count(qg => qg.TrangThai == "Thành công");

            ViewBag.TotalSuccessContributions = totalSuccessContributions;

            return PartialView();
        }

        public static Tuple<string, string> FormatAmount(decimal amount)
        {
            if (amount >= 1000000000)
            {
                return Tuple.Create(string.Format("{0:#.##}", amount / 1000000000), "tỷ");
            }
            else if (amount >= 1000000)
            {
                return Tuple.Create(string.Format("{0:#.##}", amount / 1000000), "triệu");
            }
            else if (amount >= 1000)
            {
                return Tuple.Create(string.Format("{0:#.##}", amount / 1000), "k");
            }
            else
            {
                return Tuple.Create(string.Format("{0:N0}", amount), "VNĐ");
            }
        }

        public ActionResult RelatedProject()
        {
            // Lấy dự án đang diễn ra (chưa kết thúc)
            var ongoingProject = db.Projects
                .Where(p => p.project_time_end > DateTime.Now)
                .OrderByDescending(p => p.project_time_start)
                .FirstOrDefault();

            return PartialView(ongoingProject);
        }


        public ActionResult ListCD()
        {
            List<tbl_CD> chuDes = db.tbl_CD.ToList();

            return PartialView(chuDes);
        }

        public ActionResult DetailDT(int? id_DT)
        {

            if (id_DT == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tbl_DT doitac = db.tbl_DT.Find(id_DT);

            if (doitac == null)
            {
                return HttpNotFound();
            }

            return View("DetailDT", doitac);
        }

        public ActionResult SlideDT()
        {
            var logos = db.tbl_DT.ToList();
            return PartialView(logos);
        }

        public ActionResult detaildoitac(int? id_DT)
        {
            if (id_DT == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tbl_DT doitac = db.tbl_DT.Find(id_DT);

            if (doitac == null)
            {
                return HttpNotFound();
            }

            return View(doitac);
        }

        public ActionResult doitacdonghanh()
        {
            var partners = db.tbl_DT.ToList(); 
            return View(partners);
        }

        private class ProjectDetailViewModel
        {
            public Project Project { get; set; }
            public List<Project> RelatedProjects { get; set; }
            public List<DoanterView> Contributors { get; set; }
        }
    }
}