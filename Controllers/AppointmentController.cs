using System;
using System.Linq;
using System.Security.Claims;
using KLTN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly KLTNContext _context;

        public AppointmentController(KLTNContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] Appointment appointment)
        {
            if (appointment == null || appointment.HouseId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đặt lịch." });
            }

            appointment.UserId = userId.Value; // Lấy giá trị int từ Session
            appointment.Status = AppointmentStatus.Pending;
            appointment.AppointmentDate = appointment.AppointmentDate.Date;

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return Json(
                new
                {
                    success = true,
                    message = "Đặt lịch thành công! Vui lòng chờ người đăng tin xác nhận.",
                }
            );
        }

        // Hiển thị danh sách lịch hẹn
        [Authorize]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var appointments = _context
                .Appointments.Include(a => a.User)
                .Include(a => a.House)
                .Where(a => a.House.IdUser == userId) // Lọc theo chủ bài đăng
                .ToList();

            return View(appointments);
        }

        // Xác nhận lịch hẹn
        [HttpPost]
        [Authorize]
        public IActionResult Confirm(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Confirmed;
            _context.SaveChanges();
            return Json(new { success = true, message = "Lịch hẹn đã được xác nhận." });
        }

        // Hủy lịch hẹn
        [HttpPost]
        [Authorize]
        public IActionResult Cancel(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Cancelled;
            _context.SaveChanges();
            return Json(new { success = true, message = "Lịch hẹn đã bị hủy." });
        }
    }
}
