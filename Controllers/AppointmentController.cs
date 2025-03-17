using System;
using System.Linq;
using System.Security.Claims;
using KLTN.Helpers;
using KLTN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly KLTNContext _context;
        private readonly EmailService _emailService;

        public AppointmentController(KLTNContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.House)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Lịch hẹn không tồn tại!" });
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                return Json(new { success = false, message = "Lịch hẹn này không thể xác nhận!" });
            }

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            // Gửi email thông báo xác nhận lịch hẹn
            if (appointment.User != null)
            {
                string toEmail = appointment.User.Email;
                string subject = "✅ Lịch hẹn của bạn đã được xác nhận!";
                string body = $@"
                    <p>Xin chào <b>{appointment.User.UserName}</b>,</p>
                    <p>Lịch hẹn xem nhà của bạn vào ngày <b>{appointment.AppointmentDate:dd/MM/yyyy}</b> 
                    tại nhà trọ <b>{appointment.House?.NameHouse}</b> đã được xác nhận.</p>
                    <p>Vui lòng đến đúng giờ!</p>
                    <br>
                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                await _emailService.SendEmailAsync("Appointment", toEmail, subject, body);
            }

            return Json(new { success = true, message = "Lịch hẹn đã được xác nhận và email đã gửi thành công!" });
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

        [HttpPost]
        [Authorize]
        public IActionResult DeleteSelected([FromBody] List<int> appointmentIds)
        {
            if (appointmentIds == null || !appointmentIds.Any())
            {
                return Json(new { success = false, message = "Không có lịch hẹn nào được chọn." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Bạn cần đăng nhập để thực hiện thao tác này.",
                    }
                );
            }

            var appointmentsToDelete = _context
                .Appointments.Include(a => a.House)
                .Where(a => a.House.IdUser == userId && appointmentIds.Contains(a.AppointmentId))
                .ToList();

            if (!appointmentsToDelete.Any())
            {
                return Json(
                    new { success = false, message = "Không tìm thấy lịch hẹn hợp lệ để xóa." }
                );
            }

            _context.Appointments.RemoveRange(appointmentsToDelete);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa các lịch hẹn đã chọn!" });
        }
    }
}
