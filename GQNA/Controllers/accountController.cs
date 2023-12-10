using GQNA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GQNA.Controllers
{
    public class accountController : Controller
    {
        private GQNAEntities3 db = new GQNAEntities3();
        // GET: account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult login()
        {
            return View(new tbl_NHT());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login(tbl_NHT user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.tbl_NHT
                    .Where(x => x.user_NHT.Equals(user.user_NHT) && x.pass_NHT.Equals(user.pass_NHT))
                    .FirstOrDefault();

                if (existingUser != null)
                {
                    // Lưu thông tin đăng nhập vào phiên
                    Session["isIDUser"] = existingUser.id_NHT.ToString();
                    Session["isUserName"] = existingUser.name_NHT;
                    Session["isUserPass"] = existingUser.pass_NHT;
                    Session["isUserMail"] = existingUser.email_NHT;
                    Session["TaiKhoan"] = existingUser;

                    TempData["LoginSuccessMessage"] = "Chào mừng người dùng " + existingUser.name_NHT + " quay trở lại!";
                    return RedirectToAction("home", "browse");
                }
                else
                {
                    ViewBag.LoginFaildMessage = "Tên tài khoản hoặc mật khẩu không hợp lệ.";
                }
            }

            // Trả về view với mô hình người dùng để giữ lại dữ liệu đã nhập và hiển thị thông báo lỗi
            return View(user);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        public ActionResult Register([Bind(Include = "user_NHT,pass_NHT,email_NHT,confirmPassword")] tbl_NHT user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email có đúng định dạng không
                if (!IsValidEmail(user.email_NHT))
                {
                    TempData["EmailVError"] = "Email không đúng định dạng.";
                    return View(user); // Trả về trang đăng ký để người dùng nhập lại
                }

                // Xác minh mật khẩu
                if (user.pass_NHT != user.confirmPassword)
                {
                    TempData["PasswordMismatchError"] = "Mật khẩu nhập lại không khớp.";
                    return View(user); // Trả về trang đăng ký để người dùng nhập lại
                }

                // Kiểm tra xem đã tồn tại người dùng với cùng địa chỉ email
                var existingUser = db.tbl_NHT.FirstOrDefault(u => u.email_NHT == user.email_NHT);
                if (existingUser != null)
                {
                    TempData["EmailError"] = "Email đã tồn tại.";
                    return View(user); // Trả về trang đăng ký để người dùng nhập lại
                }

                // Tạo người dùng mới và thêm vào cơ sở dữ liệu
                tbl_NHT newUser = new tbl_NHT
                {
                    user_NHT = user.user_NHT,
                    email_NHT = user.email_NHT,
                    pass_NHT = user.pass_NHT
                };

                db.tbl_NHT.Add(newUser);
                db.SaveChanges();
                // Điều hướng đến trang đăng nhập sau khi đăng ký thành công
                return RedirectToAction("login");
            }
            return View(user);
        }


        [HttpGet]
        public ActionResult setting()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["isIDUser"] == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("login");
            }

            // Lấy thông tin người dùng từ session
            int userId = Convert.ToInt32(Session["isIDUser"]);
            var user = db.tbl_NHT.Find(userId);

            if (user == null)
            {
                // Nếu không tìm thấy người dùng, đăng xuất và chuyển hướng đến trang đăng nhập
                Session.Clear();
                return RedirectToAction("login");
            }

            var paymentHistory = db.tbl_QG.Where(q => q.id_NHT == userId).ToList();
            ViewBag.PaymentHistory = paymentHistory;

            // Trả về view để người dùng có thể xem thông tin cá nhân
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult setting(tbl_NHT updatedUser, ChangePasswordViewModel changePasswordModel)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["isIDUser"] == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("login");
            }

            // Lấy thông tin người dùng từ session
            int userId = Convert.ToInt32(Session["isIDUser"]);
            var user = db.tbl_NHT.Find(userId);

            if (user == null)
            {
                // Nếu không tìm thấy người dùng, đăng xuất và chuyển hướng đến trang đăng nhập
                Session.Clear();
                return RedirectToAction("login");
            }

            // Kiểm tra xem người dùng có muốn đổi mật khẩu không
            if (changePasswordModel != null)
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra xem mật khẩu hiện tại có đúng không
                    if (changePasswordModel.CurrentPassword != user.pass_NHT)
                    {
                        ModelState.AddModelError("changePasswordModel.CurrentPassword", "Mật khẩu hiện tại không đúng");
                    }
                    else
                    {
                        // Cập nhật mật khẩu mới
                        user.pass_NHT = changePasswordModel.NewPassword;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["PasswordChangeSuccess"] = "Mật khẩu đã được đổi thành công.";
                    }
                }
            }
            else
            {
                // Người dùng không muốn đổi mật khẩu, tiếp tục xử lý cập nhật thông tin khác
                if (ModelState.IsValid)
                {
                    // Cập nhật thông tin người dùng với dữ liệu mới
                    user.user_NHT = updatedUser.user_NHT;
                    user.email_NHT = updatedUser.email_NHT;
                    user.phone_NHT = updatedUser.phone_NHT;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    // Cập nhật thông tin trong session
                    Session["isUserName"] = user.name_NHT;
                    Session["isUserMail"] = user.email_NHT;

                    TempData["ProfileUpdateSuccess"] = "Thông tin cá nhân đã được cập nhật thành công.";
                }
            }

            // Trả về view để người dùng có thể xem thông tin cá nhân và hiển thị thông báo cập nhật
            return View(user);
        }





    }
}