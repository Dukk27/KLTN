using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.Repository.House;
using KLTN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KLTN.Controllers
{
    public class QuanLiController : Controller
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IHouseDetailRepository _houseDetailRepository;
        private readonly IHouseTypeRepository _houseTypeRepository;
        private readonly IAmenityRepository _amenityRepository;

        public QuanLiController(
            IHouseRepository houseRepository,
            IHouseDetailRepository houseDetailRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository
        )
        {
            _houseRepository = houseRepository;
            _houseDetailRepository = houseDetailRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
        }

        [Authorize]
        public async Task<IActionResult> ListHouseRoom()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                string userName = HttpContext.Session.GetString("UserName") ?? "";

                IEnumerable<House> houses;
                if (userName == "Admin")
                {
                    houses = await _houseRepository.GetAllHousesAsync();
                }
                else
                {
                    houses = await _houseRepository.GetHousesByUserId(userId);
                    houses = houses.Where(h =>
                        h.Status == HouseStatus.Approved
                        || h.Status == HouseStatus.Pending
                        || h.Status == HouseStatus.Active
                        || h.Status == HouseStatus.Hidden
                    );
                }
                var viewModel = new HomeViewModel
                {
                    Houses = houses,
                    IsChuTro = userName != "Admin",
                    IsAdmin = User.IsInRole("Admin"),
                };

                ViewBag.UserId = userId;
                // ViewBag.UserName = userName;
                ViewBag.UserName = User.Identity.Name;
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditHouse(int id)
        {
            var house = await _houseRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return NotFound();
            }

            var viewModel = new HousePostViewModel
            {
                House = house,
                HouseDetail = house.HouseDetails.FirstOrDefault(),
                SelectedAmenities = house.IdAmenities.Select(a => a.IdAmenity).ToList(),
                Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList(),
                SelectedHouseType = house.HouseTypeId,
                HouseTypes = (await _houseTypeRepository.GetAllHouseTypes())
                    .Select(ht => new SelectListItem
                    {
                        Value = ht.IdHouseType.ToString(),
                        Text = ht.Name,
                        Selected = ht.IdHouseType == house.HouseTypeId,
                    })
                    .ToList(),

                ContactName2 = house.HouseDetails.FirstOrDefault()?.ContactName2,
                ContactPhone2 = house.HouseDetails.FirstOrDefault()?.ContactPhone2,
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditHouse(
            HousePostViewModel viewModel,
            IFormFile? imageFile
        )
        {
            Console.WriteLine($"ContactName2 nhận từ View: {viewModel.ContactName2}");
            Console.WriteLine($"ContactPhone2 nhận từ View: {viewModel.ContactPhone2}");
            if (!ModelState.IsValid)
            {
                viewModel.Amenities = (await _amenityRepository.GetAllAmenitiesAsync()).ToList();
                return View(viewModel);
            }

            var existingHouse = await _houseRepository.GetHouseWithDetailsAsync(
                viewModel.House.IdHouse
            );
            if (existingHouse == null)
            {
                return NotFound();
            }

            var existingHouseDetail = existingHouse.HouseDetails.FirstOrDefault();
            if (existingHouseDetail == null)
            {
                return NotFound();
            }

            existingHouse.NameHouse = !string.IsNullOrEmpty(viewModel.House.NameHouse)
                ? viewModel.House.NameHouse
                : existingHouse.NameHouse;

            existingHouse.HouseTypeId = viewModel.SelectedHouseType;

            existingHouseDetail.Address = !string.IsNullOrEmpty(viewModel.HouseDetail.Address)
                ? viewModel.HouseDetail.Address
                : existingHouseDetail.Address;
            existingHouseDetail.ContactName2 = !string.IsNullOrEmpty(viewModel.ContactName2)
                ? viewModel.ContactName2
                : existingHouseDetail.ContactName2;

            existingHouseDetail.ContactPhone2 = !string.IsNullOrEmpty(viewModel.ContactPhone2)
                ? viewModel.ContactPhone2
                : existingHouseDetail.ContactPhone2;

            existingHouseDetail.Price =
                viewModel.HouseDetail.Price != 0
                    ? viewModel.HouseDetail.Price
                    : existingHouseDetail.Price;
            existingHouseDetail.DienTich =
                viewModel.HouseDetail.DienTich != 0
                    ? viewModel.HouseDetail.DienTich
                    : existingHouseDetail.DienTich;
            existingHouseDetail.TienDien =
                viewModel.HouseDetail.TienDien != ""
                    ? viewModel.HouseDetail.TienDien
                    : existingHouseDetail.TienDien;
            existingHouseDetail.TienNuoc =
                viewModel.HouseDetail.TienNuoc != ""
                    ? viewModel.HouseDetail.TienNuoc
                    : existingHouseDetail.TienNuoc;
            existingHouseDetail.TienDv =
                viewModel.HouseDetail.TienDv != ""
                    ? viewModel.HouseDetail.TienDv
                    : existingHouseDetail.TienDv;
            existingHouseDetail.Describe = !string.IsNullOrEmpty(viewModel.HouseDetail.Describe)
                ? viewModel.HouseDetail.Describe
                : existingHouseDetail.Describe;
            existingHouseDetail.Status = !string.IsNullOrEmpty(viewModel.HouseDetail.Status)
                ? viewModel.HouseDetail.Status
                : existingHouseDetail.Status;

            if (imageFile != null) 
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/img/houses/",
                    fileName
                );

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                existingHouseDetail.Image = "/img/houses/" + fileName;
            }
            var selectedAmenities = viewModel.SelectedAmenities ?? new List<int>();
            var currentAmenities = existingHouse.IdAmenities.Select(a => a.IdAmenity).ToList();

            foreach (var amenityId in selectedAmenities)
            {
                if (!currentAmenities.Contains(amenityId))
                {
                    var amenity = await _amenityRepository.GetAmenityByIdAsync(amenityId);
                    if (amenity != null)
                    {
                        existingHouse.IdAmenities.Add(amenity);
                    }
                }
            }

            foreach (var amenityId in currentAmenities)
            {
                if (!selectedAmenities.Contains(amenityId))
                {
                    var amenityToRemove = existingHouse.IdAmenities.FirstOrDefault(a =>
                        a.IdAmenity == amenityId
                    );
                    if (amenityToRemove != null)
                    {
                        existingHouse.IdAmenities.Remove(amenityToRemove);
                    }
                }
            }
            Console.WriteLine(
                $"ContactName2 trước: {existingHouseDetail.ContactName2}, sau: {viewModel.ContactName2}"
            );
            Console.WriteLine(
                $"ContactPhone2 trước: {existingHouseDetail.ContactPhone2}, sau: {viewModel.ContactPhone2}"
            );

            existingHouse.Status = HouseStatus.Pending; 
            await _houseRepository.UpdateAsync(existingHouse);
            await _houseDetailRepository.UpdateAsync(existingHouseDetail);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            return Json(new { success = true, message = "Cập nhật thành công, bài đăng sẽ sớm được Quản trị viên duyệt!" });
            //return Json(new { success = true, message = "Sửa nhà trọ thành công.", updatedHouse = existingHouse });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteHouse(int id)
        {
            try
            {
                var house = await _houseRepository.GetHouseWithDetailsAsync(id);
                if (house == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà trọ!" });
                }

                await _houseRepository.DeleteAsync(id);

                return Json(
                    new
                    {
                        success = true,
                        message = "Xóa nhà trọ và các liên kết liên quan thành công!",
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting house: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa nhà trọ!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> HideHouse(int id)
        {
            var house = await _houseRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            // Cập nhật trạng thái bài đăng thành "Ẩn"
            house.Status = HouseStatus.Hidden;
            await _houseRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã được ẩn thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> ShowHouse(int id)
        {
            var house = await _houseRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại!" });
            }

            // Đổi trạng thái thành Active (Hiện bài)
            house.Status = HouseStatus.Active;
            await _houseRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã được hiển thị lại!" });
        }
    }
}
