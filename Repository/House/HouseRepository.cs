// HouseRepository.cs
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Repositories
{
    public class HouseRepository : IHouseRepository
    {
        private readonly KLTNContext _context;

        public HouseRepository(KLTNContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<House>> GetAllHousesAsync()
        {
            return await _context
                .Houses.Include(h => h.HouseDetails)
                .Include(h => h.HouseType)
                .Include(h => h.IdAmenities)
                .Include(h => h.IdUserNavigation)
                .ToListAsync();
        }

        public async Task<House> GetHouseWithDetailsAsync(int id)
        {
            return await _context
                .Houses.Include(h => h.HouseDetails)
                .Include(h => h.IdAmenities)
                .Include(h => h.HouseType)
                .Include(h => h.IdUserNavigation)
                .Include(h => h.Reviews)
                .ThenInclude(review => review.IdUserNavigation)
                .FirstOrDefaultAsync(h => h.IdHouse == id);
        }

        public async Task AddAsync(House house)
        {
            try
            {
                await _context.Houses.AddAsync(house);
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
                Console.WriteLine($"General error adding house: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public async Task UpdateAsync(House house)
        {
            if (_context.Entry(house).State == EntityState.Detached)
            {
                _context.Houses.Attach(house);
            }

            _context.Entry(house).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa các bản ghi UserPosts liên quan
                    var userPosts = await _context
                        .UserPosts.Where(up => up.HouseId == id)
                        .ToListAsync();
                    if (userPosts.Any())
                    {
                        _context.UserPosts.RemoveRange(userPosts);
                        await _context.SaveChangesAsync();
                    }

                    // Xóa các bản ghi Payments liên quan
                    var payments = await _context
                        .Payments.Where(p => p.HouseId == id)
                        .ToListAsync();
                    if (payments.Any())
                    {
                        _context.Payments.RemoveRange(payments);
                        await _context.SaveChangesAsync();
                    }

                    // Xóa các bản ghi Reviews liên quan
                    var reviews = await _context.Reviews.Where(r => r.IdHouse == id).ToListAsync();
                    if (reviews.Any())
                    {
                        _context.Reviews.RemoveRange(reviews);
                        await _context.SaveChangesAsync();
                    }

                    // Xóa các bản ghi HouseDetails liên quan
                    var houseDetails = await _context
                        .HouseDetails.Where(hd => hd.IdHouse == id)
                        .ToListAsync();
                    if (houseDetails.Any())
                    {
                        _context.HouseDetails.RemoveRange(houseDetails);
                        await _context.SaveChangesAsync();
                    }

                    // Xóa ngôi nhà
                    var house = await _context.Houses.FindAsync(id);
                    if (house != null)
                    {
                        _context.Houses.Remove(house);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error deleting house: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw; // Re-throw the exception to handle it at a higher level
                }
            }
        }

        public async Task<List<House>> GetHousesByUserId(int userId)
        {
            return await _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.IdUser == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<House>> GetHousesByTypeAsync(int houseTypeId)
        {
            return await _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.HouseTypeId == houseTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<House>> GetFilteredHousesAsync(
            string searchString,
            string priceRange,
            string sortBy,
            string roomType,
            List<string> amenities,
            double? minArea,
            double? maxArea
        )
        {
            var query = _context
                .Houses.Include(h => h.HouseDetails)
                .Include(h => h.HouseType)
                .Include(h => h.IdAmenities)
                .Include(h => h.IdUserNavigation)
                .AsQueryable();

            // if (!string.IsNullOrEmpty(searchString))
            // {
            //     query = query.Where(h =>
            //         h.HouseDetails.Any(hd =>
            //             hd.Address.Contains(searchString) || h.NameHouse.Contains(searchString)
            //         )
            //     );
            // }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h =>
                    h.HouseDetails.Any(hd => EF.Functions.Like(hd.Address, $"%{searchString}%"))
                    || EF.Functions.Like(h.NameHouse, $"%{searchString}%")
                );
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var ranges = priceRange.Split('-');
                if (
                    ranges.Length == 2
                    && int.TryParse(ranges[0], out int minPrice)
                    && int.TryParse(ranges[1], out int maxPrice)
                )
                {
                    query = query.Where(h =>
                        h.HouseDetails.Any(hd => hd.Price >= minPrice && hd.Price <= maxPrice)
                    );
                }
                else if (ranges.Length == 1 && int.TryParse(ranges[0], out int minOnlyPrice))
                {
                    query = query.Where(h => h.HouseDetails.Any(hd => hd.Price >= minOnlyPrice));
                }
            }

            // Filter by area (DienTich)
            if (minArea.HasValue || maxArea.HasValue)
            {
                query = query.Where(h =>
                    h.HouseDetails.Any(hd =>
                        (!minArea.HasValue || hd.DienTich >= minArea.Value)
                        && (!maxArea.HasValue || hd.DienTich <= maxArea.Value)
                    )
                );
            }

            // Filter by room type
            if (!string.IsNullOrEmpty(roomType))
            {
                Console.WriteLine($"Filtering by room type: {roomType}");

                var houseTypes = query.Select(h => h.HouseType.Name).ToList();
                Console.WriteLine("Available room types:");
                foreach (var type in houseTypes)
                {
                    Console.WriteLine(type);
                }

                query = query.Where(h => h.HouseType.Name.ToLower() == roomType.ToLower());
            }

            // Filter by amenities
            if (amenities != null && amenities.Any())
            {
                query = query.Where(h => h.IdAmenities.Any(a => amenities.Contains(a.Name)));
            }

            // // Sorting
            // if (!string.IsNullOrEmpty(sortBy))
            // {
            //     query = sortBy switch
            //     {
            //         "priceLowHigh" => query.OrderBy(h => h.HouseDetails.Min(hd => hd.Price)),
            //         "priceHighLow" => query.OrderByDescending(h =>
            //             h.HouseDetails.Max(hd => hd.Price)
            //         ),
            //         _ => query.OrderByDescending(h =>
            //             h.IdHouse
            //         ) // Default to newest
            //         ,
            //     };
            // }

            // // return await query.ToListAsync();
            // return query.ToList(); // Chuyển sang client-side evaluation để sử dụng NormalizeAddress

            // Sắp xếp theo lựa chọn
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "priceLowHigh":
                        query = query.OrderBy(h =>
                            h.HouseDetails.Any() ? h.HouseDetails.Min(hd => hd.Price) : 0
                        );
                        break;
                    case "priceHighLow":
                        query = query.OrderByDescending(h =>
                            h.HouseDetails.Any() ? h.HouseDetails.Max(hd => hd.Price) : 0
                        );
                        break;
                    case "oldest":
                        query = query.OrderBy(h =>
                            h.HouseDetails.FirstOrDefault() != null
                                ? h.HouseDetails.FirstOrDefault().TimePost
                                : DateTime.MaxValue
                        );
                        break;
                    default:
                        query = query.OrderByDescending(h =>
                            h.HouseDetails.FirstOrDefault() != null
                                ? h.HouseDetails.FirstOrDefault().TimePost
                                : DateTime.MinValue
                        );
                        break;
                }
            }
            else
            {
                // Nếu không có sortBy, mặc định sắp xếp theo TimeUpdate mới nhất
                query = query.OrderByDescending(h =>
                    h.HouseDetails.FirstOrDefault() != null
                        ? h.HouseDetails.FirstOrDefault().TimeUpdate
                        : DateTime.MinValue
                );
            }

            return await query.ToListAsync();
        }

        public static string NormalizeAddress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Chuyển về dạng FormD để tách dấu ra khỏi ký tự
            var normalizedString = input.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                // Bỏ các ký tự dấu (ký tự Unicode loại "NonSpacingMark")
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC) // Chuẩn hóa lại
                .ToLower(); // Chuyển về chữ thường để tiện so sánh
        }
    }
}
