using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GQNA.Models;
using GQNA.Models.Payment;

namespace GQNA.Controllers
{
    public class quyengopController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();

        public int ConvertDecimalToInteger(decimal decimalValue)
        {
            // Ép kiểu số DECIMAL thành số nguyên và trả về kết quả
            return (int)decimalValue;
        }

        // GET: tbl_QG
        public ActionResult Index()
        {
            var tbl_QG = db.tbl_QG.Include(t => t.Project).Include(t => t.tbl_NHT);
            return View(tbl_QG.ToList());
        }

        public ActionResult thanhtoan(int id, decimal amountT)
        {
          
            var nguoidung = Session["TaiKhoan"] as tbl_NHT;
                int makh = nguoidung.id_NHT;
                var order = db.Projects.FirstOrDefault(u => u.id_project == id);

                tbl_QG dh = new tbl_QG
                {
                    id_HD = DateTime.Now.Ticks,
                    id_NHT = makh,
                    id_project = id,
                    NgayQuyenGop = DateTime.Now,
                    ThanhTien = amountT
                };

                db.tbl_QG.Add(dh);
                db.SaveChanges();

                var urlPayment = "";
                var orders = db.tbl_QG.FirstOrDefault(x => x.id_project == id);
                Session["MaDH"] = orders.id_HD;
                //Get Config Info
                string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

                //Get payment input
                //Save order to db

                //Build URL for VNPAY
                VNPAY_Libary vnpay = new VNPAY_Libary();
                long amount = ConvertDecimalToInteger((decimal)orders.ThanhTien) * 1000;
                Debug.WriteLine(amount);
                vnpay.AddRequestData("vnp_Version", VNPAY_Libary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                long madonhang = (long)orders.id_HD;
                vnpay.AddRequestData("vnp_CreateDate", orders.NgayQuyenGop.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());

                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + madonhang);
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", madonhang.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                //Add Params of 2.1.0 Version
                //Billing

                urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return Redirect(urlPayment);
            
        }

        public ActionResult Return()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VNPAY_Libary vnpay = new VNPAY_Libary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        long code = (long)Session["MaDH"];
                        var nguoidung = Session["TaiKhoan"] as tbl_NHT;
                        var ordersList = new List<tbl_QG>();
                        var orders = db.tbl_QG.FirstOrDefault(x => x.id_HD == code);
                
                        if (orders != null)
                        {
                            orders.TrangThai = "Thành công";
                            db.Entry(orders).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            orders.TrangThai = "Thất bại";
                            db.Entry(orders).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        return View(ordersList);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                }
            }

            return RedirectToAction("Failed");
        }


        // GET: tbl_QG/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_QG tbl_QG = db.tbl_QG.Find(id);
            if (tbl_QG == null)
            {
                return HttpNotFound();
            }
            return View(tbl_QG);
        }

        // GET: tbl_QG/Create
        public ActionResult Create()
        {
            ViewBag.id_project = new SelectList(db.Projects, "id_project", "name_project");
            ViewBag.id_NHT = new SelectList(db.tbl_NHT, "id_NHT", "user_NHT");
            return View();
        }

        // POST: tbl_QG/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_HD,id_NHT,id_project,ThanhTien,NgayQuyenGop,TrangThai")] tbl_QG tbl_QG)
        {
            if (ModelState.IsValid)
            {
                db.tbl_QG.Add(tbl_QG);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_project = new SelectList(db.Projects, "id_project", "name_project", tbl_QG.id_project);
            ViewBag.id_NHT = new SelectList(db.tbl_NHT, "id_NHT", "user_NHT", tbl_QG.id_NHT);
            return View(tbl_QG);
        }

        // GET: tbl_QG/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_QG tbl_QG = db.tbl_QG.Find(id);
            if (tbl_QG == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_project = new SelectList(db.Projects, "id_project", "name_project", tbl_QG.id_project);
            ViewBag.id_NHT = new SelectList(db.tbl_NHT, "id_NHT", "user_NHT", tbl_QG.id_NHT);
            return View(tbl_QG);
        }

        // POST: tbl_QG/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_HD,id_NHT,id_project,ThanhTien,NgayQuyenGop,TrangThai")] tbl_QG tbl_QG)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_QG).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_project = new SelectList(db.Projects, "id_project", "name_project", tbl_QG.id_project);
            ViewBag.id_NHT = new SelectList(db.tbl_NHT, "id_NHT", "user_NHT", tbl_QG.id_NHT);
            return View(tbl_QG);
        }

        // GET: tbl_QG/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_QG tbl_QG = db.tbl_QG.Find(id);
            if (tbl_QG == null)
            {
                return HttpNotFound();
            }
            return View(tbl_QG);
        }

        // POST: tbl_QG/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_QG tbl_QG = db.tbl_QG.Find(id);
            db.tbl_QG.Remove(tbl_QG);
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
