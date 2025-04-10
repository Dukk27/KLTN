using System.Threading.Tasks;
using KLTN.Helpers;
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
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public HomeController(
            IHouseRepository houseRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository,
            IAccountRepository accountRepository,
            KLTNContext context,
            IConfiguration configuration,
            EmailService emailService
        )
        {
            _houseRepository = houseRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
            _accountRepository = accountRepository;
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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
                TotalPosts = totalHouses,
            };

            int? currentUserId = null;
            int unreadMessages = 0;

            if (User.Identity.IsAuthenticated)
            {
                currentUserId = await _accountRepository.GetUserIdByUsername(User.Identity.Name);
                if (currentUserId.HasValue)
                {
                    unreadMessages = await _context
                        .Messages.Where(m => m.ReceiverId == currentUserId.Value && !m.IsRead)
                        .CountAsync();
                }
            }
            ViewBag.CurrentUserId = currentUserId ?? 0;
            ViewBag.UnreadMessages = unreadMessages; // Truyền số tin nhắn chưa đọc vào ViewBag

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

            var otherHousesUser = _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.IdUser != house.IdUser && h.IdHouse != id)
                .Take(3) // Lấy 3 nhà trọ khác
                .Select(h => new House
                {
                    IdHouse = h.IdHouse,
                    NameHouse = h.NameHouse,
                    HouseDetails =
                        h.HouseDetails.ToList() // Chuyển HashSet thành List
                    ,
                })
                .ToList();

            ViewBag.OtherHousesUser = otherHousesUser;
            ViewBag.OtherHouses = otherHouses;

            return PartialView("HouseDetails", house);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadMessagesCount()
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { count = 0 });

            int? userId = await _accountRepository.GetUserIdByUsername(User.Identity.Name);
            if (userId == null)
                return Json(new { count = 0 });

            int unreadCount = await _context
                .Messages.Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();

            return Json(new { count = unreadCount });
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(string userName, string message)
        {
            string subject = $"Góp ý từ người dùng: {userName}";
            string body =
                $@"
                    <p><strong>Tên người gửi:</strong> {userName}</p>
                    <p><strong>Nội dung góp ý:</strong> {message}</p>
                ";
            try
            {
                await _emailService.SendEmailAsync(
                    emailType: "Appointment",
                    toEmail: "dn596209@gmail.com",
                    subject: subject,
                    body: body
                );

                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi góp ý!";
            }
            catch (Exception ex)
            {
                TempData["SuccessMessage"] = "Đã xảy ra lỗi khi gửi góp ý. Vui lòng thử lại sau.";
                Console.WriteLine($"Lỗi gửi góp ý: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetAllHousesForMap()
        {
            var allHouses = _context
                .Houses.Include(h => h.HouseDetails)
                .Where(h => h.Status == HouseStatus.Active || h.Status == HouseStatus.Approved && h.HouseDetails.Any(d => d.Status == "Chưa cho thuê")) 
                .Select(h => new
                {
                    Id = h.IdHouse,
                    Name = h.NameHouse,
                    Lat = h.HouseDetails.FirstOrDefault().Latitude,
                    Lng = h.HouseDetails.FirstOrDefault().Longitude,
                    Address = h.HouseDetails.FirstOrDefault().Address,
                    Price = h
                        .HouseDetails.FirstOrDefault()
                        .Price.ToString("#,0", new System.Globalization.CultureInfo("vi-VN")),
                })
                .ToList();

            return Json(allHouses);
        }
    }
}
