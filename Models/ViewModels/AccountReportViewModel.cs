using KLTN.Models;

namespace KLTN.ViewModels
{
    public class AccountReportViewModel
    {
        public int IdUser { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int TotalPosts { get; set; }
        public int ApprovedPosts { get; set; }
        public int RejectedPosts { get; set; }
        public int ActivePosts { get; set; }
        public int PendingPosts { get; set; }
        public int HiddenPosts { get; set; }
    }
}
