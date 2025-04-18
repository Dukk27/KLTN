using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repositories
{
    public interface IReportRepository
    {
        Task<bool> HasUserReportedAsync(int userId, int houseId);
        Task AddReportAsync(Report report);
        Task<int> CountApprovedReportsForHouseAsync(int houseId);
    }
}
