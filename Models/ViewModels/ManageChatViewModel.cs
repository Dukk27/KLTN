namespace KLTN.ViewModels
{
    public class ManageChatViewModel
    {
        public string ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public List<ChatMessageViewModel> Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string ConversationName => $"{SenderName} - {ReceiverName}";
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>(); // Danh sách tin nhắn
    }
}
