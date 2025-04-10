using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetReviewsByHouseIdAsync(int houseId);
    Task AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetAllReviewsAsync(); // Lấy tất cả bình luận
    Task<Review> GetReviewByIdAsync(int id); // Lấy bình luận theo ID
    Task UpdateReviewAsync(Review review); // Cập nhật bình luận
    Task DeleteReviewsAsync(List<int> ids);
    Task<List<Review>> GetReviewsByIdsAsync(List<int> ids);
}
