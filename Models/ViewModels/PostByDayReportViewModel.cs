using KLTN.Models;

namespace KLTN.ViewModels
{
    public class PostByDayReportViewModel
    {
        public DateTime Date { get; set; }
        public int TotalPosts { get; set; }
        public int IdHouse { get; set; }
        public string Address { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public DateTime TimePost { get; set; }
        public int IdUser { get; set; }
        public string UserName { get; set; }
    }
}
