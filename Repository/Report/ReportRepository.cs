using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly KLTNContext _context;

        public ReportRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUserReportedAsync(int userId, int houseId)
        {
            return await _context.Reports.AnyAsync(r => r.UserId == userId && r.HouseId == houseId);
        }

        public async Task AddReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountApprovedReportsForHouseAsync(int houseId)
        {
            return await _context
                .Reports.Where(r => r.HouseId == houseId && r.IsApproved)
                .CountAsync();
        }
    }
}
