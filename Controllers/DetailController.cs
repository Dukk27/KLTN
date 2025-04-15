using System;
using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KLTN.Controllers
{
    public class DetailController : Controller
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly KLTNContext _context;

        public DetailController(
            IHouseRepository houseRepository,
            IReviewRepository reviewRepository,
            KLTNContext context
        )
        {
            _houseRepository = houseRepository;
            _reviewRepository = reviewRepository;
            _context = context;
        }

        // Hiển thị chi tiết nhà
        public async Task<IActionResult> Detail(int id)
        {
            var house = await _houseRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
                return NotFound("Không tìm thấy nhà trọ.");

            var reviews = await _reviewRepository.GetReviewsByHouseIdAsync(id);

            var viewModel = new HouseDetailViewModel { House = house, Reviews = reviews };

            return View(viewModel); // Trả về View với dữ liệu
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int id, [FromBody] Review review)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Hãy đăng nhập để đánh giá!" });
            }

            review.IdHouse = id;
            review.IdUser = userId.Value;
            review.ReviewDate = DateTime.Now;

            try
            {
                // Thêm đánh giá vào database
                await _reviewRepository.AddReviewAsync(review);

                // Lấy lại đánh giá vừa thêm với thông tin người dùng
                var newReview = await _reviewRepository.GetReviewByIdAsync(review.IdReview);
                if (newReview == null)
                {
                    return Json(
                        new { success = false, message = "Không tìm thấy đánh giá sau khi thêm!" }
                    );
                }

                // Kiểm tra xem có thông tin người dùng không
                string userName = newReview.IdUserNavigation?.UserName ?? "Người dùng ẩn danh";

                // Lấy thông tin nhà để xác định chủ bài đăng
                var house = await _houseRepository.GetHouseWithDetailsAsync(id);
                if (house == null || house.IdUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng!" });
                }

                // Tạo thông báo cho chủ bài đăng
                var notification = new Notification
                {
                    UserId = house.IdUser, // Chủ nhà trọ nhận thông báo
                    Message = $"Bài đăng '{house.NameHouse}' vừa nhận đánh giá từ {userName}.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(); // Lưu thông báo vào DB

                return Json(
                    new
                    {
                        success = true,
                        message = "Đánh giá thành công!",
                        review = new
                        {
                            IdReview = newReview.IdReview,
                            Content = newReview.Content,
                            Rating = newReview.Rating,
                            ReviewDate = newReview.ReviewDate?.ToString("dd/MM/yyyy"),
                            UserName = userName,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm đánh giá: " + ex.ToString()); // Log lỗi chi tiết
                return Json(
                    new
                    {
                        success = false,
                        message = "Lỗi khi thêm đánh giá!",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
