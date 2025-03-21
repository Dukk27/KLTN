using System;
using System.Collections.Generic;
using System.Linq;
using KLTN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class ChatController : Controller
    {
        private readonly KLTNContext _context;

        public ChatController(KLTNContext context)
        {
            _context = context;
        }

        public IActionResult Chat(string conversationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra conversationId hợp lệ
            if (string.IsNullOrEmpty(conversationId) || !conversationId.Contains("-"))
            {
                return RedirectToAction("Error", "Home"); // Trang lỗi
            }

            // Tách conversationId thành 2 userId
            var ids = conversationId.Split('-');
            if (
                ids.Length != 2
                || !int.TryParse(ids[0], out int id1)
                || !int.TryParse(ids[1], out int id2)
            )
            {
                return RedirectToAction("Error", "Home");
            }

            // Xác định ReceiverId
            int receiverId = (userId.Value == id1) ? id2 : id1;
            if (receiverId == userId.Value)
            {
                return RedirectToAction("Error", "Home"); // Tránh lỗi tự chat với mình
            }

            // Kiểm tra xem ReceiverId có tồn tại không
            var receiverExists = _context.Accounts.Any(a => a.IdUser == receiverId);
            if (!receiverExists)
            {
                return RedirectToAction("Error", "Home"); // Nếu không tồn tại, tránh lỗi SQL
            }

            try
            {
                // Lấy danh sách tin nhắn giữa hai người
                var messages = _context
                    .Messages.Where(m =>
                        (m.SenderId == userId && m.ReceiverId == receiverId)
                        || (m.SenderId == receiverId && m.ReceiverId == userId)
                    )
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new
                    {
                        SenderId = m.SenderId,
                        SenderName = _context
                            .Accounts.Where(a => a.IdUser == m.SenderId)
                            .Select(a => a.UserName)
                            .FirstOrDefault(),
                        Content = m.Content,
                        Timestamp = m.Timestamp,
                    })
                    .ToList();

                ViewBag.CurrentUserId = userId.Value;
                ViewBag.ReceiverId = receiverId;
                ViewBag.Messages = messages;
                ViewBag.ConversationId = conversationId;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy tin nhắn: " + ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        public static string GenerateConversationId(int userId1, int userId2)
        {
            return userId1 < userId2 ? $"{userId1}-{userId2}" : $"{userId2}-{userId1}";
        }

        public IActionResult ChatList()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var conversations = new List<ConversationViewModel>();

            var messages = _context
                .Messages.Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            var conversationGroups = messages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .ToList();

            foreach (var group in conversationGroups)
            {
                var lastMessage = group.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                var otherUser = _context.Accounts.FirstOrDefault(u => u.IdUser == group.Key);

                if (otherUser != null && lastMessage != null)
                {
                    conversations.Add(
                        new ConversationViewModel
                        {
                            ConversationId = GenerateConversationId(
                                currentUserId.Value,
                                otherUser.IdUser
                            ),
                            UserName = otherUser.UserName,
                            LastMessage = lastMessage.Content,
                            LastMessageTime = lastMessage.Timestamp,
                            LastSenderId = lastMessage.SenderId,
                            LastSenderName =
                                _context
                                    .Accounts.Where(a => a.IdUser == lastMessage.SenderId)
                                    .Select(a => a.UserName)
                                    .FirstOrDefault() ?? "Không rõ",
                        }
                    );
                }
            }

            conversations = conversations.OrderByDescending(c => c.LastMessageTime).ToList();

            ViewBag.CurrentUserId = currentUserId.Value; // Đảm bảo truyền ID xuống View
            return PartialView("_ChatList", conversations);
        }

        [HttpPost]
        public IActionResult DeleteConversation([FromForm] string conversationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập!" });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản của bạn!" });
            }

            if (string.IsNullOrEmpty(conversationId) || !conversationId.Contains("-"))
            {
                return Json(new { success = false, message = "ConversationId không hợp lệ!" });
            }

            var ids = conversationId.Split('-');
            if (
                ids.Length != 2
                || !int.TryParse(ids[0], out int id1)
                || !int.TryParse(ids[1], out int id2)
            )
            {
                return Json(new { success = false, message = "Lỗi khi phân tích ConversationId!" });
            }

            int receiverId = (userId.Value == id1) ? id2 : id1;

            // Lấy tất cả tin nhắn trong cuộc hội thoại
            var messages = _context
                .Messages.Where(m =>
                    (m.SenderId == userId && m.ReceiverId == receiverId)
                    || (m.SenderId == receiverId && m.ReceiverId == userId)
                )
                .ToList();

            if (!messages.Any())
            {
                return Json(new { success = false, message = "Không có tin nhắn để xóa!" });
            }

            // Xóa tin nhắn khỏi database
            _context.Messages.RemoveRange(messages);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa hội thoại thành công!" });
        }
    }
}
