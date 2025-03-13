using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ admin được phép truy cập
    public class AdminController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IHouseRepository _housesRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IHouseTypeRepository _houseTypeRepository;
        private readonly IAmenityRepository _amenityRepository;

        public AdminController(
            IAccountRepository accountRepository,
            IHouseRepository housesRepository,
            IReviewRepository reviewRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository
        )
        {
            _accountRepository = accountRepository;
            _housesRepository = housesRepository;
            _reviewRepository = reviewRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
        }

        // Trang Dashboard
        public IActionResult Index()
        {
            return View();
        }

        // Quản lý tài khoản
        public async Task<IActionResult> ManageAccounts()
        {
            TempData["PreviousPage"] = Request.Headers["Referer"].ToString();
            var accounts = await _accountRepository.GetAllAccountsAsync();
            return PartialView("_ManageAccounts", accounts); // Trả về PartialView
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _accountRepository.GetAccountByIdAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdUser,UserName,Password,Role,PhoneNumber,Email,FreePostsUsed")] Account account
        )
        {
            if (id != account.IdUser)
            {
                return NotFound();
            }

            // if (ModelState.IsValid) // Kiểm tra tính hợp lệ của dữ liệu
            // {
            await _accountRepository.UpdateAccountAsync(account);

            // Chuyển hướng về trang trước khi gọi action Edit
            var previousPage = TempData["PreviousPage"] as string;
            if (string.IsNullOrEmpty(previousPage))
            {
                return RedirectToAction(nameof(ManageAccounts)); // Nếu không có URL trước đó thì quay lại trang quản lý tài khoản
            }
            return Redirect(previousPage); // Quay lại trang trước đó

            return View(account); // Trả về View nếu có lỗi
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            await _accountRepository.DeleteAccountAsync(id);
            return Json(new { success = true, message = "Tài khoản đã được xóa thành công." });
        }

        // Quản lý bài đăng
        public async Task<IActionResult> ManagePosts()
        {
            var posts = await _housesRepository.GetAllHousesAsync();
            posts = posts.Where(h => h.Status != HouseStatus.Unpaid).ToList();
            return PartialView("_ManagePosts", posts);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var house = await _housesRepository.GetHouseWithDetailsAsync(id);
                if (house == null)
                {
                    return Json(new { success = false, message = "Bài đăng không tồn tại." });
                }

                await _housesRepository.DeleteAsync(id);

                return Json(new { success = true, message = "Xóa bài đăng thành công." });
            }
            catch
            {
                return Json(
                    new { success = false, message = "Đã xảy ra lỗi trong quá trình xóa bài đăng." }
                );
            }
        }

        // // Quản lý bình luận
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            return PartialView("_ManageReviews", reviews); // Trả về PartialView
        }

        // [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại." });
            }

            await _reviewRepository.DeleteReviewAsync(id);
            return Json(new { success = true, message = "Bình luận đã được xóa thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var house = await _housesRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            house.Status = HouseStatus.Approved;
            await _housesRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã được duyệt." });
        }

        [HttpPost]
        public async Task<IActionResult> RejectPost(int id)
        {
            var house = await _housesRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            house.Status = HouseStatus.Rejected;
            await _housesRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã bị từ chối." });
        }

        public async Task<IActionResult> ManageHouseTypes()
        {
            var houseTypes = await _houseTypeRepository.GetAllHouseTypes();
            return PartialView("_ManageHouseTypes", houseTypes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHouseType([FromBody] HouseType houseType)
        {
            if (houseType == null || string.IsNullOrWhiteSpace(houseType.Name))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                await _houseTypeRepository.CreateHouseTypeAsync(houseType);
                return Json(new { success = true, message = "Thêm loại nhà thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditHouseType([FromBody] HouseType houseType)
        {
            if (houseType == null || string.IsNullOrWhiteSpace(houseType.Name))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                await _houseTypeRepository.UpdateHouseTypeAsync(houseType);
                return Json(new { success = true, message = "Cập nhật loại nhà thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHouseType(int id)
        {
            try
            {
                await _houseTypeRepository.DeleteHouseTypeAsync(id);
                return Json(new { success = true, message = "Xóa loại nhà thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public async Task<IActionResult> ManageAmenities()
        {
            var amenities = await _amenityRepository.GetAllAmenitiesAsync();
            return PartialView("_ManageAmenities", amenities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAmenity([FromBody] Amenity amenity)
        {
            if (amenity == null || string.IsNullOrWhiteSpace(amenity.Name))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                await _amenityRepository.AddAsync(amenity);
                return Json(new { success = true, message = "Thêm tiện nghi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAmenity([FromBody] Amenity amenity)
        {
            if (amenity == null || string.IsNullOrWhiteSpace(amenity.Name))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                await _amenityRepository.UpdateAsync(amenity);
                return Json(new { success = true, message = "Cập nhật tiện nghi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            try
            {
                await _amenityRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xóa tiện nghi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}
