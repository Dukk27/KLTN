using System;
using System.Collections.Generic;
using System.Linq;
using KLTN.Models;
using KLTN.ViewModels;
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

            // Kiểm tra xem user hiện tại có thuộc cuộc trò chuyện không
            if (userId.Value != id1 && userId.Value != id2)
            {
                return RedirectToAction("Error", "Home"); // Chặn truy cập
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
                var messages = _context
                    .Messages.Where(m =>
                        (m.SenderId == userId && m.ReceiverId == receiverId)
                        || (m.SenderId == receiverId && m.ReceiverId == userId)
                    )
                    .OrderBy(m => m.Timestamp)
                    .ToList(); // Lấy tất cả tin nhắn

                // Cập nhật trạng thái đã đọc
                var unreadMessages = messages
                    .Where(m => m.ReceiverId == userId && !m.IsRead)
                    .ToList();
                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.IsRead = true;
                    }
                    _context.SaveChanges();
                }

                // Thêm SenderName vào từng tin nhắn
                var messagesWithSenderName = messages
                    .Select(m => new
                    {
                        m.SenderId,
                        SenderName = _context
                            .Accounts.Where(a => a.IdUser == m.SenderId)
                            .Select(a => a.UserName)
                            .FirstOrDefault(),
                        m.Content,
                        m.Timestamp,
                        m.ReceiverId,
                        m.IsRead,
                    })
                    .ToList();

                ViewBag.CurrentUserId = userId.Value;
                ViewBag.ReceiverId = receiverId;
                ViewBag.Messages = messagesWithSenderName; // Truyền dữ liệu đã có SenderName
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

        public IActionResult ChatList(string conversationId)
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
                    // Kiểm tra xem có tin nhắn chưa đọc từ người khác không
                    bool hasUnreadMessages = _context.Messages.Any(m =>
                        m.ReceiverId == currentUserId && m.SenderId == otherUser.IdUser && !m.IsRead
                    );

                    // Đếm số tin nhắn chưa đọc từ người khác
                    var unreadMessagesCount = _context.Messages.Count(m =>
                        m.ReceiverId == currentUserId && m.SenderId == otherUser.IdUser && !m.IsRead
                    );

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
                            HasUnreadMessages = hasUnreadMessages,
                            UnreadMessagesCount = unreadMessagesCount,
                        }
                    );
                }
            }

            conversations = conversations.OrderByDescending(c => c.LastMessageTime).ToList();

            // hiển thị nội dung chat khi có conversationId
            ManageChatViewModel currentConversation = null;
            if (!string.IsNullOrEmpty(conversationId))
            {
                var ids = conversationId.Split('-');
                if (
                    ids.Length == 2
                    && int.TryParse(ids[0], out int id1)
                    && int.TryParse(ids[1], out int id2)
                )
                {
                    int receiverId = (currentUserId.Value == id1) ? id2 : id1;

                    // Cập nhật tin nhắn chưa đọc thành đã đọc
                    var unreadMessages = _context
                        .Messages.Where(m =>
                            m.SenderId == receiverId && m.ReceiverId == currentUserId && !m.IsRead
                        )
                        .ToList();

                    if (unreadMessages.Any())
                    {
                        foreach (var msg in unreadMessages)
                        {
                            msg.IsRead = true;
                        }
                        _context.SaveChanges();
                    }

                    var user = _context.Accounts.FirstOrDefault(a => a.IdUser == receiverId);
                    if (user != null)
                    {
                        // Truyền trạng thái khoá
                        ViewBag.IsReceiverLocked = user.IsLocked;

                        var chatMessages = _context
                            .Messages.Where(m =>
                                (m.SenderId == currentUserId && m.ReceiverId == receiverId)
                                || (m.SenderId == receiverId && m.ReceiverId == currentUserId)
                            )
                            .OrderBy(m => m.Timestamp)
                            .ToList();

                        currentConversation = new ManageChatViewModel
                        {
                            ReceiverId = receiverId,
                            ReceiverName = user.UserName,
                            ConversationId = conversationId,
                            Messages = chatMessages
                                .Select(m => new ChatMessageViewModel
                                {
                                    Sender =
                                        _context
                                            .Accounts.FirstOrDefault(a => a.IdUser == m.SenderId)
                                            ?.UserName ?? "Không rõ",
                                    Content = m.Content,
                                    Timestamp = m.Timestamp,
                                })
                                .ToList(),
                        };
                    }
                }
            }

            ViewBag.CurrentConversation = currentConversation;
            ViewBag.CurrentUserName = _context
                .Accounts.FirstOrDefault(a => a.IdUser == currentUserId)
                ?.UserName;
            ViewBag.CurrentUserId = currentUserId.Value;

            return View(conversations);
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

        // API endpoint để lấy số tin nhắn chưa đọc
        public IActionResult GetUnreadMessagesCount()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return Json(new { count = 0 });
            }

            var unreadCount = _context.Messages.Count(m =>
                m.ReceiverId == currentUserId.Value && !m.IsRead
            );
            return Json(new { count = unreadCount });
        }
    }
}
