using System;
using System.Linq;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Helpers
{
    public class NotificationService
    {
        private readonly KLTNContext _context;

        public NotificationService(KLTNContext context)
        {
            _context = context;
        }

        public void NotifyExpiredPosts()
        {
            Console.WriteLine("===> ĐANG KIỂM TRA BÀI ĐĂNG QUÁ 30 NGÀY...");

            var houses = _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.HouseDetails.Any())
                .ToList();

            Console.WriteLine($"===> Đã lấy {houses.Count} nhà có thông tin chi tiết.");

            foreach (var house in houses)
            {
                var houseDetail = house.HouseDetails.FirstOrDefault();
                if (houseDetail == null)
                    continue;

                var timePost = houseDetail.TimePost;

                if ((DateTime.Now - timePost).TotalDays > 30)
                {
                    Console.WriteLine(
                        $"===> Nhà '{house.NameHouse}' đã quá 30 ngày, đang kiểm tra notification..."
                    );

                    var today = DateTime.Today;
                    var exists = _context.Notifications.Any(n =>
                        n.UserId == house.IdUser
                        && n.Message.Contains(house.NameHouse)
                        && EF.Functions.DateDiffDay(n.CreatedAt, today) == 0
                    );

                    if (exists)
                    {
                        Console.WriteLine("===> Notification đã tồn tại hôm nay.");
                        continue;
                    }

                    var notification = new Notification
                    {
                        UserId = house.IdUser,
                        Message =
                            $"Bài đăng của bạn có tiêu đề: {house.NameHouse} đã quá 30 ngày. Hệ thống đã ẩn bài đăng của bạn. Vui lòng cập nhật lại",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                    };

                    _context.Notifications.Add(notification);

                    if (house.Status != HouseStatus.Pending)
                    {
                        house.Status = HouseStatus.Pending;
                    }
                    Console.WriteLine("===> Notification mới đã được thêm.");
                }
            }

            _context.SaveChanges();
            Console.WriteLine("===> Lưu thay đổi vào DB xong.");
        }
    }
}
