using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly KLTNContext _context;

        public NotificationController(KLTNContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách thông báo của user
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = _context
                .Notifications.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        // Tạo thông báo mới
        [HttpPost]
        public IActionResult Create([FromBody] Notification notification)
        {
            if (notification == null || string.IsNullOrEmpty(notification.Message))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            notification.CreatedAt = DateTime.Now;
            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thông báo đã được tạo." });
        }

        // Đánh dấu thông báo là đã đọc
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            notification.IsRead = true;
            _context.SaveChanges();

            return Json(new { success = true, message = "Thông báo đã được đánh dấu là đã đọc." });
        }

        // Xóa thông báo
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            _context.Notifications.Remove(notification);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thông báo đã bị xóa." });
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var notifications = _context
                .Notifications.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    id = n.Id,
                    message = n.Message,
                    isRead = n.IsRead,
                })
                .ToList();

            return Json(notifications);
        }

        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { count = 0 });
            }

            var unreadCount = _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);

            return Json(new { count = unreadCount });
        }

        // Đánh dấu tất cả thông báo là đã đọc
        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var notifications = _context
                .Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .ToList();
            if (!notifications.Any())
            {
                return Json(new { success = false, message = "Không có thông báo chưa đọc." });
            }

            notifications.ForEach(n => n.IsRead = true);
            _context.SaveChanges();

            return Json(
                new { success = true, message = "Tất cả thông báo đã được đánh dấu là đã đọc." }
            );
        }

        // Xóa tất cả thông báo của người dùng
        [HttpPost]
        public IActionResult DeleteAll()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var notifications = _context.Notifications.Where(n => n.UserId == userId).ToList();
            if (!notifications.Any())
            {
                return Json(new { success = false, message = "Không có thông báo để xóa." });
            }

            _context.Notifications.RemoveRange(notifications);
            _context.SaveChanges();

            return Json(new { success = true, message = "Tất cả thông báo đã bị xóa." });
        }
    }
}
