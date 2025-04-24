using System;
using System.Threading;
using System.Threading.Tasks;
using KLTN.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class ExpiredPostNotificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ExpiredPostNotificationJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService =
                    scope.ServiceProvider.GetRequiredService<NotificationService>();
                notificationService.NotifyExpiredPosts();
            }

            // Chờ 1 ngày trước khi lặp lại
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
