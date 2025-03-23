using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.ViewModels;
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
        private readonly IAccountRepository _accountRepository;
        private readonly KLTNContext _context;

        public HomeController(
            IHouseRepository houseRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository,
            IAccountRepository accountRepository,
            KLTNContext context
        )
        {
            _houseRepository = houseRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
            _accountRepository = accountRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchString,
            string priceRange,
            string sortBy,
            string roomType,
            List<string> amenities,
            int pageNumber = 1, // Trang hiện tại (mặc định là 1)
            int pageSize = 8 // Số lượng phần tử mỗi trang
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
                .Where(h => h.HouseDetails.Any(d => d.Status == "Chưa cho thuê"))
                .ToList();

            int totalHouses = houses.Count();
            int totalPages =
                totalHouses > 0 ? (int)Math.Ceiling((double)totalHouses / pageSize) : 1;

            // Đảm bảo trang hợp lệ
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageNumber = pageNumber > totalPages ? totalPages : pageNumber;

            // Lấy danh sách theo trang hiện tại
            var pagedHouses = houses.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Truy vấn danh sách house types
            var houseTypes = await _houseTypeRepository.GetAllHouseTypes();

            // Tạo ViewModel để truyền dữ liệu vào View
            var viewModel = new HomeViewModel
            {
                Houses = pagedHouses,
                HouseTypes = houseTypes,
                Amenities = amenitiesList,
                IsChuTro = User.IsInRole("ChuTro"),
                IsAdmin = User.IsInRole("Admin"),
                CurrentPage = pageNumber,
                TotalPages = totalPages,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> HousesByType(int id)
        {
            var houses = await _houseRepository.GetHousesByTypeAsync(id);

            if (houses == null || !houses.Any())
            {
                // Trả về thông báo cho người dùng khi không có nhà trọ trong danh mục
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
            int? currentUserId = null;

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    userRole = 0;
                else if (User.IsInRole("ChuTro"))
                    userRole = 1;

                // Lấy ID người dùng hiện tại từ Database
                currentUserId = await _accountRepository.GetUserIdByUsername(User.Identity.Name);
            }

            ViewBag.UserRole = userRole;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OwnerId = house.IdUserNavigation?.IdUser;

            // Tạo conversationId (nếu người dùng đã đăng nhập)
            if (currentUserId != null && house.IdUserNavigation != null)
            {
                ViewBag.ConversationId = ChatController.GenerateConversationId(
                    currentUserId.Value,
                    house.IdUserNavigation.IdUser
                );
            }
            var otherHouses = _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.IdUser == house.IdUser && h.IdHouse != id)
                .Select(h => new House
                {
                    IdHouse = h.IdHouse,
                    NameHouse = h.NameHouse,
                    HouseDetails =
                        h.HouseDetails.ToList() // Chuyển HashSet thành List
                    ,
                })
                .ToList();

            ViewBag.OtherHouses = otherHouses;

            return PartialView("HouseDetails", house);
        }
    }
}
