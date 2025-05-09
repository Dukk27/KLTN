using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KLTN.Models;

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

    // Xóa bình luận
    public async Task DeleteReviewAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
