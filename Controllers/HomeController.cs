using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IHouseTypeRepository _houseTypeRepository;

        private readonly IAmenityRepository _amenityRepository;
        private readonly KLTNContext _context;

        public HomeController(
            IHouseRepository houseRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository,
            KLTNContext context
        )
        {
            _houseRepository = houseRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchString,
            string priceRange,
            string sortBy,
            string roomType,
            List<string> amenities
        )
        {
            // Lấy danh sách Amenities và HouseTypes
            var amenitiesList = (await _amenityRepository.GetAllAmenitiesAsync()).ToList();
            var houseTypesList = (await _houseTypeRepository.GetAllHouseTypes())
                .Select(ht => new SelectListItem
                {
                    Value = ht.IdHouseType.ToString(),
                    Text = ht.Name,
                })
                .ToList();

            // ViewBag values cho các bộ lọc
            ViewBag.Keyword = searchString;
            ViewBag.PriceRange = priceRange;
            ViewBag.SortBy = sortBy;
            ViewBag.RoomType = roomType;
            ViewBag.SelectedAmenities = amenities;

            // Truy vấn tìm kiếm theo địa chỉ
            var houses = await _houseRepository.GetFilteredHousesAsync(
                searchString,
                priceRange,
                sortBy,
                roomType,
                amenities
            );

            houses = houses
                .Where(h => h.Status == HouseStatus.Active || h.Status == HouseStatus.Approved)
                .ToList();

            // Truy vấn danh sách house types
            var houseTypes = await _houseTypeRepository.GetAllHouseTypes();

            // Tạo ViewModel để truyền dữ liệu vào View
            var viewModel = new HomeViewModel
            {
                Houses = houses,
                HouseTypes = houseTypes,
                Amenities = amenitiesList,
                IsChuTro = User.IsInRole("ChuTro"),
                IsAdmin = User.IsInRole("Admin"),
            };

            return View(viewModel);
        }

        public async Task<IActionResult> HousesByType(int id)
        {
            var houses = await _houseRepository.GetHousesByTypeAsync(id);

            if (houses == null || !houses.Any())
            {
                // Trả về một thông báo cho người dùng khi không có nhà trọ trong danh mục
                TempData["Message"] =
                    "Không có nhà trọ nào thuộc danh mục này. Vui lòng chọn danh mục khác.";
            }
            var selectedHouseType = await _context.HouseType.FirstOrDefaultAsync(ht =>
                ht.IdHouseType == id
            );
            if (selectedHouseType == null)
            {
                return NotFound();
            }
            var houseTypes = await _houseTypeRepository.GetAllHouseTypes();

            var viewModel = new HomeViewModel
            {
                Houses = houses,
                HouseTypes = houseTypes,
                SelectedHouseType = selectedHouseType,
                IsChuTro = User.IsInRole("ChuTro"),
                IsAdmin = User.IsInRole("Admin"),
            };

            return View("HousesListPartial", viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var house = await _houseRepository.GetHouseWithDetailsAsync(id);

            if (house == null)
            {
                return NotFound();
            }

            int userRole = 2; // Mặc định là người tìm phòng
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    userRole = 0;
                else if (User.IsInRole("ChuTro"))
                    userRole = 1;
            }

            ViewBag.UserRole = userRole;

            return PartialView("HouseDetails", house);
        }
    }
}
