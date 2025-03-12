using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.Services;
using KLTN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Create(HousePostViewModel model, IFormFile imageFile)
        {
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = await _context.Accounts.FindAsync(userId);

            if (!ModelState.IsValid)
            {
                model.Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList();
                model.SelectedAmenities ??= new List<int>();
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin các trường bắt buộc.");

                if (isAjaxRequest)
                {
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Any())
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return Json(new { success = false, errors });
                }

                return View(model);
            }

            ModelState.Remove("IdHouseNavigation");
            model.HouseDetail.TimePost = DateTime.Now;

            if (imageFile != null && imageFile.Length > 0)
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

                string uniqueFileName =
                    Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                model.HouseDetail.Image = Path.Combine("/img", "houses", uniqueFileName)
                    .Replace("\\", "/");
            }

            model.House.IdUser = userId;
            model.House.HouseTypeId = model.SelectedHouseType;

            // Xử lý trạng thái bài đăng dựa vào số lượt miễn phí còn lại
            if (user.FreePostsUsed < 3)
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
                                message = "Bạn đã sử dụng hết 3 bài đăng miễn phí. Vui lòng thanh toán để tiếp tục.",
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
        [Authorize]
        public async Task<IActionResult> CreatePartial()
        {
            var model = new HousePostViewModel
            {
                Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList(),
                HouseTypes = (await _houseTypeRepository.GetAllHouseTypes()).Select(
                    ht => new SelectListItem { Value = ht.IdHouseType.ToString(), Text = ht.Name }
                ),
            };

            return PartialView("_CreateHousePartial", model);
        }
    }
}
