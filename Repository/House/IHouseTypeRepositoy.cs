using KLTN.Models;

namespace KLTN.Repositories
{
    public interface IHouseTypeRepository
    {
        Task<IEnumerable<HouseType>> GetAllHouseTypes();
        String GetHouseTypeName(int id);
        Task<HouseType> GetHouseTypeByIdAsync(int id);
        Task<bool> CreateHouseTypeAsync(HouseType houseType);
        Task<bool> UpdateHouseTypeAsync(HouseType houseType);
        Task<bool> DeleteHouseTypeAsync(int id);
    }
}
