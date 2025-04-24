using System;
using System.Linq;
using System.Security.Claims;
using KLTN.Helpers;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepo;
        private readonly KLTNContext _context;

        public ReportController(IReportRepository reportRepo, KLTNContext context)
        {
            _reportRepo = reportRepo;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ReportHouse(int houseId, string reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy version hiện tại của bài đăng
            var house = await _context.Houses.FirstOrDefaultAsync(h => h.IdHouse == houseId);
            if (house == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
                return RedirectToAction("Detail", "Home", new { id = houseId });
            }

            int currentVersion = house.ReportVersion;

            // Kiểm tra người dùng đã báo cáo phiên bản này chưa
            bool alreadyReported = await _context.Reports.AnyAsync(r =>
                r.UserId == userId.Value
                && r.HouseId == houseId
                && r.ReportVersion == currentVersion
            ); 

            if (alreadyReported)
            {
                TempData["ErrorMessage"] = "Bạn đã báo cáo bài đăng này trước đó.";
                return RedirectToAction("Detail", "Home", new { id = houseId });
            }

            // Tạo báo cáo mới
            var report = new Report
            {
                UserId = userId.Value,
                HouseId = houseId,
                Reason = reason,
                CreatedAt = DateTime.Now,
                IsApproved = false,
                ReportVersion =
                    currentVersion // Gán version 
                ,
            };

            await _reportRepo.AddReportAsync(report);

            var adminNotification = new Notification
            {
                UserId = 1, // Admin
                Message =
                    $"Một báo cáo mới đã được gửi về bài đăng '{house.NameHouse}' với lý do: {reason}",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(adminNotification);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn! Báo cáo đã được gửi.";
            return RedirectToAction("Detail", "Home", new { id = houseId });
        }
    }
}
