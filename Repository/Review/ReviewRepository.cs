using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
    private readonly KLTNContext _context;

    public ReviewRepository(KLTNContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetReviewsByHouseIdAsync(int houseId)
    {
        return await _context
            .Reviews.Where(r => r.IdHouse == houseId)
            .Include(r => r.IdUserNavigation)
            .ToListAsync();
    }

    public async Task AddReviewAsync(Review review)
    {
        try
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding review: " + ex.Message);
            throw;
        }
    }

    // Lấy tất cả bình luận
    public async Task<IEnumerable<Review>> GetAllReviewsAsync()
    {
        return await _context
            .Reviews.Include(r => r.IdUserNavigation) // Include thông tin người dùng
            .Include(r => r.IdHouseNavigation) // Include thông tin nhà
            .OrderByDescending(r => r.ReviewDate) // Sắp xếp theo thời gian mới nhất
            .ToListAsync();
    }

    // Lấy bình luận theo ID
    public async Task<Review> GetReviewByIdAsync(int id)
    {
        return await _context
            .Reviews.Include(r => r.IdUserNavigation) // Include thông tin người dùng
            .Include(r => r.IdHouseNavigation) // Include thông tin nhà
            .FirstOrDefaultAsync(r => r.IdReview == id);
    }

    // Cập nhật bình luận
    public async Task UpdateReviewAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReviewsAsync(List<int> ids)
    {
        var reviewsToDelete = await _context
            .Reviews.Where(r => ids.Contains(r.IdReview))
            .ToListAsync();
        if (reviewsToDelete.Any())
        {
            _context.Reviews.RemoveRange(reviewsToDelete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Review>> GetReviewsByIdsAsync(List<int> ids)
    {
        var reviews = await _context
            .Reviews.Where(r => ids.Contains(r.IdReview))
            .Include(r => r.IdUserNavigation)
            .Include(r => r.IdHouseNavigation)
            .ToListAsync();
        return reviews;
    }
}
