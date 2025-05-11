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
                        a.AppointmentDate < now
                        && a.Status == AppointmentStatus.Pending
                        && !a.IsNotified
                    )
                    .Include(a => a.User)
                    .Include(a => a.House)
                    .ThenInclude(h => h.HouseDetails)
                    .ToListAsync();

                foreach (var appointment in expiredAppointments)
                {
                    var houseAddress =
                        appointment.House?.HouseDetails?.FirstOrDefault()?.Address
                        ?? "Không có địa chỉ";
                    // Gửi cho người đặt lịch
                    var notification = new Notification
                    {
                        UserId = appointment.UserId,
                        Message =
                            $"Lịch hẹn vào {appointment.AppointmentDate:HH:mm dd/MM/yyyy} tại bài đăng có tiêu đề: {appointment.House?.NameHouse} (Địa chỉ: {houseAddress}) đã quá hạn mà chưa được xác nhận.",
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
                            <p>Lịch hẹn xem nhà của bạn vào <b>{appointment.AppointmentDate:HH:mm dd/MM/yyyy}</b> tại bài đăng có tiêu đề: <b>{appointment.House?.NameHouse}</b> đã quá hạn và chưa được xác nhận.</p>
                            <p><b>Địa chỉ:</b> {houseAddress}.</p>
                            <p>Vui lòng liên hệ chủ trọ để xác nhận lại lịch hẹn.</p>
                            <br>
                            <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                        await emailService.SendEmailAsync("Appointment", toEmail, subject, body);
                    }

                    // Gửi cho chủ trọ
                    var houseOwner = await context.Accounts.FindAsync(appointment.House.IdUser);
                    if (houseOwner != null)
                    {
                        var ownerNotification = new Notification
                        {
                            UserId = houseOwner.IdUser,
                            Message =
                                $"Lịch hẹn với khách vào {appointment.AppointmentDate:HH:mm dd/MM/yyyy} tại bài đăng có tiêu đề: {appointment.House?.NameHouse} (Địa chỉ: {houseAddress}) đã quá hạn mà chưa được xác nhận.",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                        };

                        context.Notifications.Add(ownerNotification);

                        if (!string.IsNullOrWhiteSpace(houseOwner.Email))
                        {
                            string toEmail = houseOwner.Email;
                            string subject = "Lịch hẹn với khách đã quá hạn!";
                            string body =
                                $@"
                                    <p>Xin chào <b>{houseOwner.UserName}</b>,</p>
                                    <p>Lịch hẹn với khách vào <b>{appointment.AppointmentDate:HH:mm dd/MM/yyyy}</b> tại bài đăng có tiêu đề: <b>{appointment.House?.NameHouse}</b> đã quá hạn và chưa được xác nhận.</p>
                                    <p><b>Địa chỉ:</b> {houseAddress}.</p>
                                    <p>Vui lòng kiểm tra lại để đảm bảo quá trình đặt lịch không bị gián đoạn.</p>
                                    <br>
                                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                            await emailService.SendEmailAsync(
                                "Appointment",
                                toEmail,
                                subject,
                                body
                            );
                        }
                    }

                    appointment.IsNotified = true; // Đánh dấu là đã thông báo
                }

                // Gửi thông báo cho chủ trọ trước 1 ngày
                var upcomingAppointments = await context
                    .Appointments.Where(a =>
                        a.AppointmentDate > now
                        && a.AppointmentDate <= now.AddDays(1)
                        && a.Status == AppointmentStatus.Pending
                        && !a.IsReminderSent
                    )
                    .Include(a => a.House)
                    .ThenInclude(h => h.HouseDetails) //thông tin địa chỉ
                    .ToListAsync();

                foreach (var appointment in upcomingAppointments)
                {
                    var houseOwner = await context.Accounts.FindAsync(appointment.House.IdUser);
                    if (houseOwner != null)
                    {
                        var houseAddress =
                            appointment.House?.HouseDetails?.FirstOrDefault()?.Address
                            ?? "Không có địa chỉ";

                        var notification = new Notification
                        {
                            UserId = houseOwner.IdUser,
                            Message =
                                $"Bạn có lịch hẹn với khách lúc {appointment.AppointmentDate:HH:mm dd/MM/yyyy} tại bài đăng có tiêu đề: {appointment.House?.NameHouse} (Địa chỉ: {houseAddress}).",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                        };
                        context.Notifications.Add(notification);

                        if (!string.IsNullOrWhiteSpace(houseOwner.Email))
                        {
                            string toEmail = houseOwner.Email;
                            string subject = "Nhắc nhở lịch hẹn sắp tới!";
                            string body =
                                $@"
                                    <p>Xin chào <b>{houseOwner.UserName}</b>,</p>
                                    <p>Bạn có lịch hẹn với khách vào <b>{appointment.AppointmentDate:HH:mm dd/MM/yyyy}</b> tại bài đăng có tiêu đề: <b>{appointment.House?.NameHouse}</b>.</p>
                                    <p><b>Địa chỉ:</b> {houseAddress}.</p>
                                    <p>Vui lòng chuẩn bị tiếp đón khách đúng giờ.</p>
                                    <br>
                                    <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

                            await emailService.SendEmailAsync(
                                "Appointment",
                                toEmail,
                                subject,
                                body
                            );
                        }

                        // Đánh dấu là đã nhắc
                        appointment.IsReminderSent = true;
                    }
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
