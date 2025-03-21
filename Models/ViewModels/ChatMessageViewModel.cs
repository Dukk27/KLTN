namespace KLTN.ViewModels
{
    public class ChatMessageViewModel
    {
        public string Sender { get; set; } // Tên người gửi
        public string Content { get; set; } // Nội dung tin nhắn
        public DateTime Timestamp { get; set; } // Thời gian gửi
    }
}
