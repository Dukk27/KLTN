using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KLTN.Helpers;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KLTN.Helpers
{
    public class ExpiredAppointmentCheckerService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public ExpiredAppointmentCheckerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // CheckExpiredAppointments(null); // Gọi lần đầu tiên ngay khi service khởi động

            _timer = new Timer(
                CheckExpiredAppointments,
                null,
                TimeSpan.FromMinutes(0), // Bắt đầu ngay lập tức
                TimeSpan.FromMinutes(1) // Kiểm tra mỗi phút
            );
            return Task.CompletedTask;
        }

        private async void CheckExpiredAppointments(object state)
        {
            using (var scope = _serviceProvider.CreateScope()) // Tạo scope mới để lấy các dịch vụ scoped
            {
                var context = scope.ServiceProvider.GetRequiredService<KLTNContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                var now = DateTime.Now;

                var expiredAppointments = await context
                    .Appointments.Where(a =>
                        a.AppointmentDate < now && a.Status == AppointmentStatus.Pending && !a.IsNotified
                    )
                    .Include(a => a.User)
                    .Include(a => a.House)
                    .ToListAsync();

                foreach (var appointment in expiredAppointments)
                {
                    var notification = new Notification
                    {
                        UserId = appointment.UserId,
                        Message =
                            $"❗ Lịch hẹn vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} tại bài đăng có tiêu đề {appointment.House?.NameHouse} đã quá hạn mà chưa được xác nhận.",
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                    };

                    context.Notifications.Add(notification);

                    if (appointment.User != null)
                    {
                        string toEmail = appointment.User.Email;
                        string subject = "Lịch hẹn của bạn đã quá hạn!";
                        string body =
                            $@"
                            <p>Xin chào <b>{appointment.User.UserName}</b>,</p>
                            <p>Lịch hẹn xem nhà của bạn vào ngày <b>{appointment.AppointmentDate:dd/MM/yyyy HH:mm}</b> tại nhà trọ <b>{appointment.House?.NameHouse}</b> đã quá hạn và chưa được xác nhận.</p>
                            <p>Vui lòng liên hệ để xác nhận lại hoặc hủy lịch hẹn nếu không còn nhu cầu.</p>
                            <br>
                            <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                        await emailService.SendEmailAsync("Appointment", toEmail, subject, body);
                    }

                    appointment.IsNotified = true; // Đánh dấu là đã thông báo
                }

                await context.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
