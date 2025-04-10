using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KLTN.Hubs
{
    public class ChatHub : Hub
    {
        private readonly KLTNContext _context;

        public ChatHub(KLTNContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            Console.WriteLine(
                $"[DEBUG] Nhận tin nhắn từ {senderId} gửi đến {receiverId}: {message}"
            );

            var senderName =
                _context
                    .Accounts.Where(a => a.IdUser == senderId)
                    .Select(a => a.UserName)
                    .FirstOrDefault() ?? "Ẩn danh";

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.Now,
            };

            try
            {
                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Tin nhắn đã được lưu vào DB!");

                string formattedTimestamp = newMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss");

                // Gửi tin nhắn với đủ 4 tham số: senderId, senderName, message, timestamp
                await Clients
                    .Group(senderId.ToString())
                    .SendAsync("ReceiveMessage", senderId, senderName, message, formattedTimestamp);

                await Clients
                    .Group(receiverId.ToString())
                    .SendAsync("ReceiveMessage", senderId, senderName, message, formattedTimestamp);

                await Clients
                    .Group(receiverId.ToString())
                    .SendAsync("ReceiveNotification", senderId, senderName, message);
                    
                await Clients.Group(receiverId.ToString()).SendAsync("UpdateUnreadMessages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi lưu tin nhắn vào DB: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Session.GetInt32("UserId")?.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"[DEBUG] Người dùng {userId} đã kết nối vào nhóm chat.");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.GetHttpContext()?.Session.GetInt32("UserId")?.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"[DEBUG] Người dùng {userId} đã rời khỏi nhóm chat.");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
