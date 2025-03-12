using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly KLTNContext _context;

        public AmenityRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Amenity>> GetAllAmenitiesAsync()
        {
            return await _context.Amenities.ToListAsync();
        }

        public async Task<Amenity> GetAmenityByIdAsync(int id)
        {
            return await _context.Amenities.FirstOrDefaultAsync(a => a.IdAmenity == id);
        }

        public async Task AddAsync(Amenity amenity)
        {
            try
            {
                await _context.Amenities.AddAsync(amenity);
                int affectedRows = await _context.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    Console.WriteLine("Không có bản ghi nào được thêm vào cơ sở dữ liệu.");
                }
                else
                {
                    Console.WriteLine($"Đã thêm {affectedRows} bản ghi vào cơ sở dữ liệu.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding Amenity: {ex.Message}");
            }
        }

        public async Task UpdateAsync(Amenity amenity)
        {
            try
            {
                if (_context.Entry(amenity).State == EntityState.Detached)
                {
                    _context.Amenities.Attach(amenity);
                }
                _context.Entry(amenity).State = EntityState.Modified;

                int affectedRows = await _context.SaveChangesAsync();
                Console.WriteLine($"Đã cập nhật {affectedRows} bản ghi trong cơ sở dữ liệu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Amenity: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var amenity = await _context.Amenities.FindAsync(id);
                if (amenity != null)
                {
                    _context.Amenities.Remove(amenity);
                    int affectedRows = await _context.SaveChangesAsync();
                    Console.WriteLine($"Đã xóa {affectedRows} bản ghi từ cơ sở dữ liệu.");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy bản ghi để xóa.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting Amenity: {ex.Message}");
            }
        }
    }
}
