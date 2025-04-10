using Microsoft.AspNetCore.Mvc;
using SurveyWebsite.Models;
using System.Linq;

public class UserController : Controller
{
    private readonly SurveyDbContext _context;

    public UserController(SurveyDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(); // Giao diện Razor dùng AJAX
    }
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        // Regular expression để kiểm tra số điện thoại
        string pattern = @"^0\d{9}$"; // Bắt đầu bằng số 0 và có 9 chữ số tiếp theo (tổng 10 ký tự)
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
    }

    [HttpGet]
    public JsonResult GetUsers()
    {
        return Json(_context.Users.ToList());
    }

    [HttpPost]
    public JsonResult Create([FromBody] User user)
    {
        if (ModelState.IsValid)
        {
            if (!IsValidPhoneNumber(user.SDT))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ! Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0." });
            }
            _context.Users.Add(user);
            _context.SaveChanges();
            return Json(new { success = true, message = "Thêm thành công!" });
        }
        return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
    }

    [HttpGet]
    public JsonResult GetUserById(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);
        if (user != null)
        {
            return Json(new { success = true, data = user });
        }
        return Json(new { success = false, message = "Người dùng không tồn tại" });
    }


    [HttpPost]
    public JsonResult Edit([FromBody] User user)
    {
        var u = _context.Users.Find(user.UserId);
        if (u != null)
        {
            if (!IsValidPhoneNumber(user.SDT))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ! Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0." });
            }
            u.Username = user.Username;
            u.Password = user.Password;
            u.FullName = user.FullName;
            u.SDT = user.SDT;
            _context.SaveChanges();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }
        return Json(new { success = false, message = "Không tìm thấy người dùng!" });
    }

    [HttpPost]
    public JsonResult Delete(int id)
    {
        var u = _context.Users.Find(id);
        if (u != null)
        {
            _context.Users.Remove(u);
            _context.SaveChanges();
            return Json(new { success = true, message = "Xoá thành công!" });
        }
        return Json(new { success = false, message = "Không tìm thấy người dùng!" });
    }
}
