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

            var user = _context.Accounts.FirstOrDefault(u => u.IdUser == userId.Value);

            appointment.UserId = userId.Value;
            appointment.Status = AppointmentStatus.Pending;

            // Kiểm tra lịch hẹn phải ít nhất sau 1 tiếng so với thời điểm hiện tại
            if (appointment.AppointmentDate < DateTime.Now.AddHours(1))
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Thời gian hẹn gặp phải cách thời điểm hiện tại ít nhất 1 tiếng.",
                    }
                );
            }

            // Gửi thông báo cho chủ nhà
            var house = _context
                .Houses.Include(h => h.IdUserNavigation) // để lấy thông tin email chủ nhà
                .Include(h => h.HouseDetails) // để lấy địa chỉ
                .FirstOrDefault(h => h.IdHouse == appointment.HouseId);

            if (house != null)
            {
                var notification = new Notification
                {
                    UserId = house.IdUser,
                    Message =
                        $"{user.UserName} đã đặt lịch hẹn vào {appointment.AppointmentDate:HH:mm dd/MM/yyyy} cho bài đăng có tiêu đề: {house.NameHouse}.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Gửi email cho chủ nhà
            if (house?.IdUserNavigation != null)
            {
                string toEmail = house.IdUserNavigation.Email;
                string subject = "Bạn có một lịch hẹn mới!";
                string body =
                    $@"
                    <p>Xin chào <b>{house.IdUserNavigation.UserName}</b>,</p>
                    <p>Người dùng <b>{user.UserName}</b> vừa đặt lịch hẹn xem nhà.</p>
                    <p><b>Thời gian:</b> {appointment.AppointmentDate:HH:mm dd/MM/yyyy}</p>
                    <p><b>Bài đăng:</b> {house.NameHouse}</p>
                    <p><b>Địa chỉ:</b> {house .HouseDetails.FirstOrDefault() ?.Address ?? "Không có địa chỉ"}</p>
                    <p>Vui lòng đăng nhập hệ thống để xác nhận hoặc từ chối lịch hẹn.</p>
                    <br>
                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                _emailService.SendEmailAsync("Appointment", toEmail, subject, body);
            }

            return Json(
                new
                {
                    success = true,
                    message = "Đặt lịch hẹn thành công! Vui lòng chờ người đăng tin xác nhận.",
                }
            );
        }

        [Authorize]
        public IActionResult Index(string filter = "valid")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var now = DateTime.Now;

            IQueryable<Appointment> query = _context
                .Appointments.Include(a => a.User)
                .Include(a => a.House)
                .Where(a => a.House.IdUser == userId);

            if (filter == "expired")
            {
                query = query.Where(a => a.AppointmentDate < now);
            }
            else // default: valid
            {
                query = query.Where(a => a.AppointmentDate >= now);
            }

            var appointments = query
                .OrderBy(a => a.Status == AppointmentStatus.Pending ? 0 : 1) // Pending lên đầu
                .ThenBy(a => a.AppointmentDate)
                .ToList();

            ViewBag.Filter = filter;
            return View(appointments);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context
                .Appointments.Include(a => a.User)
                .Include(a => a.House)
                .ThenInclude(h => h.HouseDetails) // Dùng housedetail để lấy thông tin địa chỉ
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

            // Tạo thông báo cho người đặt lịch
            var houseAddress =
                appointment.House?.HouseDetails.FirstOrDefault()?.Address ?? "Không có địa chỉ";

            var notification = new Notification
            {
                UserId = appointment.UserId, // Người đặt lịch nhận thông báo
                Message =
                    $"Lịch hẹn vào {appointment.AppointmentDate:HH:mm dd/MM/yyyy} tại bài đăng có tiêu đề: {appointment.House?.NameHouse} đã được xác nhận (Địa chỉ: {houseAddress}).",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi email thông báo xác nhận lịch hẹn
            if (appointment.User != null)
            {
                string toEmail = appointment.User.Email;
                string subject = "Lịch hẹn của bạn đã được xác nhận!";
                string body =
                    $@"
                    <p>Xin chào <b>{appointment.User.UserName}</b>,</p>
                    <p>Lịch hẹn xem nhà của bạn vào <b>{appointment.AppointmentDate:HH:mm dd/MM/yyyy}</b> 
                    tại bài đăng có tiêu đề: <b>{appointment .House ?.NameHouse}</b> đã được xác nhận.</p>
                    <p><b>Địa chỉ:</b> {appointment .House?.HouseDetails.FirstOrDefault() ?.Address ?? "Không có địa chỉ"}</p>
                    <p>Vui lòng đến đúng giờ!</p>
                    <br>
                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                await _emailService.SendEmailAsync("Appointment", toEmail, subject, body);
            }

            return Json(
                new
                {
                    success = true,
                    message = "Lịch hẹn đã được xác nhận và email đã gửi thành công!",
                }
            );
        }

        // Hủy lịch hẹn và gửi thông báo cho người đặt lịch
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context
                .Appointments.Include(a => a.User)
                .Include(a => a.House)
                .ThenInclude(h => h.HouseDetails) // Dùng housedetail để lấy thông tin địa chỉ
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Lịch hẹn không tồn tại!" });
            }

            // Cập nhật trạng thái lịch hẹn
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Tạo thông báo cho người đặt lịch
            var houseAddress =
                appointment.House?.HouseDetails.FirstOrDefault()?.Address ?? "Không có địa chỉ";

            var notification = new Notification
            {
                UserId = appointment.UserId, // Gửi cho người đặt lịch
                Message =
                    $"Lịch hẹn vào {appointment.AppointmentDate:HH:mm dd/MM/yyyy} tại bài đăng có tiêu đề: {appointment.House?.NameHouse} (Địa chỉ: {houseAddress}) đã bị hủy.",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi email cho người đặt lịch hẹn
            if (appointment.User != null)
            {
                string toEmail = appointment.User.Email;
                string subject = "Lịch hẹn của bạn đã bị từ chối!";
                string body =
                    $@"
                    <p>Xin chào <b>{appointment.User.UserName}</b>,</p>
                    <p>Lịch hẹn xem nhà của bạn vào <b>{appointment.AppointmentDate:HH:mm dd/MM/yyyy}</b> 
                    tại bài đăng có tiêu đề: <b>{appointment .House ?.NameHouse}</b> đã bị từ chối.</p>
                    <p>Vui lòng liên hệ với chủ nhà để sắp xếp lịch hẹn khác.</p>
                    <br>
                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                await _emailService.SendEmailAsync("Appointment", toEmail, subject, body);
            }

            return Json(
                new { success = true, message = "Lịch hẹn đã bị từ chối và thông báo đã được gửi!" }
            );
        }

        // Hiển thị lịch hẹn của người dùng đã đặt
        [Authorize]
        public IActionResult MyAppointments(string filter = "valid")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var now = DateTime.Now;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Appointment> query = _context
                .Appointments.Include(a => a.House)
                .ThenInclude(h => h.IdUserNavigation)
                .Where(a => a.UserId == userId); // Người đặt lịch là chính user

            if (filter == "expired")
            {
                query = query.Where(a => a.AppointmentDate < now);
            }
            else
            {
                query = query.Where(a => a.AppointmentDate >= now);
            }

            var appointments = query.OrderBy(a => a.AppointmentDate).ToList();

            ViewBag.Filter = filter;
            return View("MyAppointments", appointments);
        }
    }
}
