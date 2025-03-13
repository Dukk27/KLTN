using System;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repository.House
{
    public class HouseTypeRepository : IHouseTypeRepository
    {
        private readonly KLTNContext _context;

        public HouseTypeRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HouseType>> GetAllHouseTypes()
        {
            return await _context.HouseType.ToListAsync();
        }

        public String GetHouseTypeName(int id)
        {
            return _context.HouseType.FirstOrDefault(e => e.IdHouseType == id).Name;
        }

        public async Task<HouseType> GetHouseTypeByIdAsync(int id)
        {
            return await _context.HouseType.FirstOrDefaultAsync(a => a.IdHouseType == id);
        }

        public async Task<bool> CreateHouseTypeAsync(HouseType houseType)
        {
            try
            {
                await _context.HouseType.AddAsync(houseType);
                int affectedRows = await _context.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    Console.WriteLine("Không có bản ghi nào được thêm vào cơ sở dữ liệu.");
                    return false;
                }
                else
                {
                    Console.WriteLine($"Đã thêm {affectedRows} bản ghi vào cơ sở dữ liệu.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding HouseType: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateHouseTypeAsync(HouseType houseType)
        {
            try
            {
                if (_context.Entry(houseType).State == EntityState.Detached)
                {
                    _context.HouseType.Attach(houseType);
                }
                _context.Entry(houseType).State = EntityState.Modified;

                int affectedRows = await _context.SaveChangesAsync();
                Console.WriteLine($"Đã cập nhật {affectedRows} bản ghi trong cơ sở dữ liệu.");
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating HouseType: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteHouseTypeAsync(int id)
        {
            try
            {
                var houseType = await _context.HouseType.FindAsync(id);
                if (houseType != null)
                {
                    _context.HouseType.Remove(houseType);
                    int affectedRows = await _context.SaveChangesAsync();
                    Console.WriteLine($"Đã xóa {affectedRows} bản ghi từ cơ sở dữ liệu.");
                    return affectedRows > 0;
                }
                else
                {
                    Console.WriteLine("Không tìm thấy bản ghi để xóa.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting HouseType: {ex.Message}");
                return false;
            }
        }
    }
}
