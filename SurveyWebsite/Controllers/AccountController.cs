using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyWebsite.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SurveyWebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly SurveyDbContext _context;

        public AccountController(SurveyDbContext context)
        {
            _context = context;
        }

        // ====== Mã hóa mật khẩu ======
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // ====== LOGIN ======
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            // Tạo claims cho người dùng đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) // Thêm dòng này
            };


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Đăng nhập bằng cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("Username", user.Username); // Hoặc user.FullName nếu bạn có
            HttpContext.Session.SetString("FullName", user.FullName); // Hoặc user.FullName nếu bạn có
            HttpContext.Session.SetInt32("UserID", user.UserId); // Nếu chưa có

            TempData["SuccessMessage"] = $"Đăng nhập thành công, xin chào {user.FullName}!";

            return RedirectToAction("Index", "Home");
        }

        //TAI KHOAN NHAP SAI MAT KHAU 3 LAN SE BI KHOA 15P
        /* [HttpPost]
         public async Task<IActionResult> Login(string username, string password)
         {
             var user = _context.Users.FirstOrDefault(u => u.Username == username);

             if (user == null)
             {
                 ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                 return View();
             }

             // Kiểm tra trạng thái khóa tài khoản
             if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.Now)
             {
                 var minutesLeft = (user.LockoutUntil.Value - DateTime.Now).Minutes;
                 ViewBag.Error = $"Tài khoản của bạn đã bị khóa tạm thời. Vui lòng thử lại sau {minutesLeft} phút.";
                 return View();
             }

             // Mã hóa mật khẩu để so sánh
             var hashedPassword = HashPassword(password);

             if (user.Password != hashedPassword)
             {
                 // Tăng số lần đăng nhập sai
                 user.FailedLoginAttempts++;
                 if (user.FailedLoginAttempts >= 3)
                 {
                     user.LockoutUntil = DateTime.Now.AddMinutes(15); // Khóa tài khoản trong 15 phút
                 }
                 _context.SaveChanges();
                 ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                 return View();
             }

             // Đặt lại số lần đăng nhập sai nếu đăng nhập thành công
             user.FailedLoginAttempts = 0;
             user.LockoutUntil = null;
             _context.SaveChanges();

             // Tạo claims cho người dùng đăng nhập
             var claims = new List<Claim>
             {
                 new Claim(ClaimTypes.Name, user.Username),
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
             };

             var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

             // Đăng nhập bằng cookie
             await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                 new ClaimsPrincipal(claimsIdentity));

             TempData["SuccessMessage"] = $"Đăng nhập thành công, xin chào {user.FullName}!";

             return RedirectToAction("Index", "Home");
         }*/

        // ====== REGISTER ======

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // Kiểm tra username đã tồn tại chưa
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View(user); // Giữ nguyên dữ liệu đã nhập nếu lỗi
            }

            // Lưu tài khoản vào DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Chuyển sang trang đăng nhập
            return RedirectToAction("Login");
        }

        //MA HOA VA DAT DIEU KIEN MAT KHAU
        /*[HttpPost]
        public async Task<IActionResult> Register(string Password, string ConfirmPassword, User user)
        {
            // Kiểm tra mật khẩu và mật khẩu xác nhận
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu và mật khẩu xác nhận không khớp.";
                return View(user); // Trả lại thông tin đã nhập để người dùng sửa
            }

            // Kiểm tra username đã tồn tại chưa
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View(user); // Trả lại thông tin đã nhập
            }

            // Kiểm tra điều kiện mật khẩu (ít nhất 6 ký tự, bao gồm chữ, số và ký tự đặc biệt)
            var passwordPattern = @"^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).{6,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(Password, passwordPattern))
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ, số và ký tự đặc biệt.";
                return View(user);
            }

            // Mã hóa mật khẩu
            user.Password = HashPassword(Password);

            // Lưu tài khoản vào DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }*/


        // ====== LOGOUT ======

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
