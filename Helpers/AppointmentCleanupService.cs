// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using KLTN.Models;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;

// namespace KLTN.Helpers
// {
//     public class AppointmentCleanupService : IHostedService, IDisposable
//     {
//         private readonly IServiceScopeFactory _scopeFactory;
//         private readonly ILogger<AppointmentCleanupService> _logger;
//         private Timer _timer;

//         public AppointmentCleanupService(
//             IServiceScopeFactory scopeFactory,
//             ILogger<AppointmentCleanupService> logger
//         )
//         {
//             _scopeFactory = scopeFactory;
//             _logger = logger;
//         }

//         public Task StartAsync(CancellationToken cancellationToken)
//         {
//             _timer = new Timer(CleanUpAppointments, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
//             return Task.CompletedTask;
//         }

//         private async void CleanUpAppointments(object state)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var context = scope.ServiceProvider.GetRequiredService<KLTNContext>();
//             var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

//             var now = DateTime.Now;

//             var appointmentsToDelete = context
//                 .Appointments.Where(a =>
//                     (
//                         a.Status == AppointmentStatus.Confirmed
//                         || a.Status == AppointmentStatus.Cancelled
//                     )
//                     && a.AppointmentDate.AddMinutes(1) < now
//                 )
//                 .ToList();

//             appointmentsToDelete.AddRange(
//                 context
//                     .Appointments.Where(a =>
//                         a.Status == AppointmentStatus.Pending && a.AppointmentDate.AddMinutes(1) < now
//                     )
//                     .ToList()
//             );

//             if (appointmentsToDelete.Any())
//             {
//                 context.Appointments.RemoveRange(appointmentsToDelete);
//                 await context.SaveChangesAsync();

//                 _logger.LogInformation(
//                     $"🧹 Đã xóa {appointmentsToDelete.Count} lịch hẹn hết hạn lúc {DateTime.Now}"
//                 );

//                 foreach (var appointment in appointmentsToDelete)
//                 {
//                     // Gửi thông báo cho người dùng
//                     if (appointment.User != null)
//                     {
//                         var notification = new Notification
//                         {
//                             UserId = appointment.UserId,
//                             Message =
//                                 $"❗ Lịch hẹn vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} tại bài đăng có tiêu đề {appointment.House?.NameHouse} đã hết hạn và bị xóa.",
//                             CreatedAt = DateTime.Now,
//                             IsRead = false,
//                         };

//                         context.Notifications.Add(notification);
//                         await context.SaveChangesAsync();

//                         if (!string.IsNullOrWhiteSpace(appointment.User.Email))
//                         {
//                             string toEmail = appointment.User.Email;
//                             string subject = "Lịch hẹn đã bị xóa!";
//                             string body =
//                                 $@"
//                                 <p>Xin chào <b>{appointment.User.UserName}</b>,</p>
//                                 <p>Lịch hẹn xem nhà của bạn vào ngày <b>{appointment.AppointmentDate:dd/MM/yyyy HH:mm}</b> tại nhà trọ <b>{appointment.House?.NameHouse}</b> đã hết hạn và bị xóa.</p>
//                                 <p>Vui lòng kiểm tra lại các lịch hẹn của bạn trên hệ thống.</p>
//                                 <br>
//                                 <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

//                             await emailService.SendEmailAsync("Appointment", toEmail, subject, body);
//                         }
//                     }

//                     // Gửi thông báo cho chủ trọ
//                     if (appointment.House != null)
//                     {
//                         var houseOwner = await context.Accounts.FindAsync(appointment.House.IdUser);
//                         if (houseOwner != null)
//                         {
//                             var ownerNotification = new Notification
//                             {
//                                 UserId = houseOwner.IdUser,
//                                 Message =
//                                     $"⚠️ Lịch hẹn với khách vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} tại bài đăng '{appointment.House?.NameHouse}' đã hết hạn và bị xóa.",
//                                 CreatedAt = DateTime.Now,
//                                 IsRead = false,
//                             };

//                             context.Notifications.Add(ownerNotification);
//                             await context.SaveChangesAsync();

//                             if (!string.IsNullOrWhiteSpace(houseOwner.Email))
//                             {
//                                 string toEmail = houseOwner.Email;
//                                 string subject = "Lịch hẹn với khách đã bị xóa!";
//                                 string body =
//                                     $@"
//                                     <p>Xin chào <b>{houseOwner.UserName}</b>,</p>
//                                     <p>Lịch hẹn với khách vào ngày <b>{appointment.AppointmentDate:dd/MM/yyyy HH:mm}</b> tại nhà trọ <b>{appointment.House?.NameHouse}</b> đã hết hạn và bị xóa.</p>
//                                     <p>Vui lòng kiểm tra lại các lịch hẹn của bạn trên hệ thống.</p>
//                                     <br>
//                                     <p>Trân trọng,<br>Hệ Thống Đặt Lịch Hẹn</p>";

//                                 await emailService.SendEmailAsync("Appointment", toEmail, subject, body);
//                             }
//                         }
//                     }
//                 }

//                 await context.SaveChangesAsync();
//             }
//         }

//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             _timer?.Change(Timeout.Infinite, 0);
//             return Task.CompletedTask;
//         }

//         public void Dispose()
//         {
//             _timer?.Dispose();
//         }
//     }
// }
