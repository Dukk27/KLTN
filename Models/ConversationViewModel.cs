namespace KLTN.Models
{
    public class ConversationViewModel
    {
        public string ConversationId { get; set; } 
        public string UserName { get; set; } // Tên người đang chat với mình
        public string LastMessage { get; set; } // Tin nhắn gần nhất
        public DateTime? LastMessageTime { get; set; } // Thời gian tin nhắn cuối
        public int LastSenderId { get; set; } 
        public string LastSenderName { get; set; }
        public bool HasUnreadMessages { get; set; } // Xác định tin nhắn chưa đọc
        public int UnreadMessagesCount { get; set; } 
    }
}
