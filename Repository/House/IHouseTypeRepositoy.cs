using KLTN.Models;

namespace KLTN.Repositories
{
    public interface IHouseTypeRepository
    {
        Task<IEnumerable<HouseType>> GetAllHouseTypes();
        String GetHouseTypeName(int id);
    }
}
