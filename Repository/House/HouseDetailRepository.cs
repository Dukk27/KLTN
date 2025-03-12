using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KLTN.Models;

namespace KLTN.Repositories
{
    public class HouseDetailRepository : IHouseDetailRepository
    {
        private readonly KLTNContext _context;

        public HouseDetailRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HouseDetail>> GetAllHouseDetailsAsync()
        {
            return await _context.HouseDetails.Include(h => h.IdHouseNavigation).ToListAsync();
        }

        public async Task<HouseDetail?> GetHouseDetailByIdAsync(int id)
        {
            var houseDetail = await _context
                .HouseDetails.Include(h => h.IdHouseNavigation)
                .FirstOrDefaultAsync(h => h.IdHouseDetail == id);

            return houseDetail;
        }

        public async Task AddAsync(HouseDetail houseDetail)
        {
            try
            {
                await _context.HouseDetails.AddAsync(houseDetail);
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
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update error: {dbEx.Message}");
                Console.WriteLine(dbEx.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error adding HouseDetail: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public async Task UpdateAsync(HouseDetail houseDetail)
        {
            try
            {
                if (_context.Entry(houseDetail).State == EntityState.Detached)
                {
                    _context.HouseDetails.Attach(houseDetail);
                }
                _context.Entry(houseDetail).State = EntityState.Modified;

                int affectedRows = await _context.SaveChangesAsync();
                Console.WriteLine($"Đã cập nhật {affectedRows} bản ghi trong cơ sở dữ liệu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating HouseDetail: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var houseDetail = await _context.HouseDetails.FindAsync(id);
                if (houseDetail != null)
                {
                    _context.HouseDetails.Remove(houseDetail);
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
                Console.WriteLine($"Error deleting HouseDetail: {ex.Message}");
            }
        }
    }
}
