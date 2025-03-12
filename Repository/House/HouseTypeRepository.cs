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
    }
}
