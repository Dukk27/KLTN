using System.Text.Json;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.Services;
using KLTN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KLTN.Controllers
{
    public class HouseController : Controller
    {
        private readonly HouseService _houseService;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly KLTNContext _context;
        private readonly IHouseTypeRepository _houseTypeRepository;

        public HouseController(
            HouseService houseService,
            IAmenityRepository amenityRepository,
            IWebHostEnvironment webHostEnvironment,
            KLTNContext context,
            IHouseTypeRepository houseTypeRepository
        )
        {
            _houseService = houseService;
            _amenityRepository = amenityRepository;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _houseTypeRepository = houseTypeRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = new HousePostViewModel
            {
                Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList(),
                HouseTypes = (await _houseTypeRepository.GetAllHouseTypes()).Select(
                    ht => new SelectListItem { Value = ht.IdHouseType.ToString(), Text = ht.Name }
                ),
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateHousePartial", model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(
            HousePostViewModel model,
            List<IFormFile> imageFiles
        )
        {
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = await _context.Accounts.FindAsync(userId);

            if (!ModelState.IsValid)
            {
                model.Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList();
                model.SelectedAmenities ??= new List<int>();
                model.HouseTypes = (await _houseTypeRepository.GetAllHouseTypes()).Select(
                    ht => new SelectListItem { Value = ht.IdHouseType.ToString(), Text = ht.Name }
                );
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin các trường bắt buộc.");

                if (isAjaxRequest)
                {
                    // Trả về partial chứa form kèm lỗi, để client render lại trong modal
                    return PartialView("_CreateHousePartial", model);
                }

                // Trả về partial để giữ consistency, tránh full page reload
                return PartialView("_CreateHousePartial", model);
            }

            ModelState.Remove("IdHouseNavigation");
            model.HouseDetail.TimePost = DateTime.Now;

            if (imageFiles != null && imageFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "img",
                    "houses"
                );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                List<string> imagePaths = new List<string>();

                foreach (var file in imageFiles.Take(5)) // Giới hạn 5 ảnh
                {
                    string uniqueFileName =
                        Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    imagePaths.Add("/img/houses/" + uniqueFileName);
                }

                model.HouseDetail.Image = string.Join(",", imagePaths);
            }

            model.House.IdUser = userId;
            model.House.HouseTypeId = model.SelectedHouseType;

            // Xử lý trạng thái bài đăng dựa vào số lượt miễn phí còn lại
            if (user.FreePostsUsed < 5)
            {
                user.FreePostsUsed++;
                model.House.Status = HouseStatus.Pending; // Nếu còn miễn phí, duyệt bài ngay
            }
            else
            {
                model.House.Status = HouseStatus.Unpaid; // Nếu hết lượt miễn phí, chờ thanh toán
            }

            var result = await _houseService.CreateHouseAsync(model);

            if (result)
            {
                if (model.House.Status == HouseStatus.Pending)
                {
                    var userPost = new UserPost
                    {
                        UserId = userId,
                        HouseId = model.House.IdHouse,
                        IsFree = true,
                        PostDate = DateTime.Now,
                    };
                    _context.Add(userPost);
                    await _context.SaveChangesAsync();

                    // Tạo thông báo cho Admin khi có bài đăng mới
                    var adminAccounts = _context.Accounts.Where(u => u.Role == 0).ToList();
                    foreach (var admin in adminAccounts)
                    {
                        var notification = new Notification
                        {
                            UserId = admin.IdUser, // Gửi thông báo cho Admin
                            Message =
                                $"Bài đăng mới có tiêu đề: {model.House.NameHouse} cần được duyệt.",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                        };
                        _context.Notifications.Add(notification);
                    }

                    await _context.SaveChangesAsync();
                    if (isAjaxRequest)
                    {
                        return Json(
                            new
                            {
                                success = true,
                                message = "Thêm nhà trọ thành công.",
                                newHouse = model.House,
                                redirectUrl = Url.Action("Index", "Home"),
                            }
                        );
                    }
                    TempData["SuccessMessage"] = "Bài đăng của bạn đã được gửi đến quản trị viên!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (isAjaxRequest)
                    {
                        return Json(
                            new
                            {
                                success = true,
                                message = "Bạn đã sử dụng hết 5 bài đăng miễn phí. Vui lòng thanh toán để tiếp tục.",
                                redirectUrl = Url.Action(
                                    "Payment",
                                    "Payments",
                                    new { houseId = model.House.IdHouse }
                                ),
                            }
                        );
                    }

                    return RedirectToAction(
                        "Payment",
                        "Payments",
                        new { houseId = model.House.IdHouse }
                    );
                }
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài đăng.");
                model.Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList();

                if (isAjaxRequest)
                {
                    return Json(
                        new { success = false, message = "Có lỗi xảy ra khi tạo bài đăng." }
                    );
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFreePostsRemaining()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = await _context.Accounts.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            int freePostsRemaining = Math.Max(5 - user.FreePostsUsed, 0);

            return Json(new { success = true, freePostsRemaining });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreatePartial()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = await _context.Accounts.FindAsync(userId);
            int freePostsRemaining = Math.Max(5 - (user?.FreePostsUsed ?? 0), 0);

            var model = new HousePostViewModel
            {
                Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList(),
                HouseTypes = (await _houseTypeRepository.GetAllHouseTypes()).Select(
                    ht => new SelectListItem { Value = ht.IdHouseType.ToString(), Text = ht.Name }
                ),
                FreePostsRemaining = freePostsRemaining,
            };

            return PartialView("_CreateHousePartial", model);
        }

        [HttpPost]
        public IActionResult SaveLatLng([FromBody] LatLngDto dto)
        {
            var houseDetail = _context.HouseDetails.FirstOrDefault(h => h.IdHouse == dto.HouseId);
            if (houseDetail != null)
            {
                houseDetail.Latitude = dto.Lat;
                houseDetail.Longitude = dto.Lng;
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không tìm thấy bản ghi HouseDetail." });
        }

        public class LatLngDto
        {
            public int HouseId { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
