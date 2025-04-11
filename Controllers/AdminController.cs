using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.ViewModels;
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
        private readonly KLTNContext _context;

        public AdminController(
            IAccountRepository accountRepository,
            IHouseRepository housesRepository,
            IReviewRepository reviewRepository,
            IHouseTypeRepository houseTypeRepository,
            IAmenityRepository amenityRepository,
            KLTNContext context
        )
        {
            _accountRepository = accountRepository;
            _housesRepository = housesRepository;
            _reviewRepository = reviewRepository;
            _houseTypeRepository = houseTypeRepository;
            _amenityRepository = amenityRepository;
            _context = context;
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

        public async Task<IActionResult> Details(int id)
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
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
                return Json(new { success = false, message = "ID tài khoản không hợp lệ." });
            }

            try
            {
                await _accountRepository.UpdateAccountAsync(account);

                string previousPage =
                    TempData["PreviousPage"] as string
                    ?? Url.Action(nameof(ManageAccounts), "Admin");

                return Json(
                    new
                    {
                        success = true,
                        message = "Cập nhật tài khoản thành công!",
                        previousPage,
                    }
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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

                // Gửi thông báo cho chủ bài đăng
                var notification = new Notification
                {
                    UserId = house.IdUser,
                    Message = $"❌ Bài đăng '{house.NameHouse}' của bạn đã bị xóa.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa bài đăng thành công." });
            }
            catch
            {
                return Json(
                    new { success = false, message = "Đã xảy ra lỗi trong quá trình xóa bài đăng." }
                );
            }
        }

        // Quản lý bình luận
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            return PartialView("_ManageReviews", reviews); // Trả về PartialView
        }


        // Xóa bình luận
        [HttpPost]
        public async Task<IActionResult> DeleteSelectedReviews([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Không có bình luận nào được chọn." });
            }

            try
            {
                var reviews = await _reviewRepository.GetReviewsByIdsAsync(ids);

                if (reviews == null || !reviews.Any())
                {
                    return Json(
                        new { success = false, message = "Không tìm thấy bình luận nào để xóa." }
                    );
                }

                Console.WriteLine($"Tìm thấy {reviews.Count} bình luận để xóa.");

                await _reviewRepository.DeleteReviewsAsync(ids);

                // Kiểm tra nếu có bình luận hợp lệ để tạo thông báo
                List<Notification> notifications = new List<Notification>();

                foreach (var review in reviews)
                {
                    Console.WriteLine(
                        $"Xử lý bình luận ID {review.IdReview} của User {review.IdUser}"
                    );

                    if (review.IdUser != null) // Kiểm tra UserId hợp lệ
                    {
                        notifications.Add(
                            new Notification
                            {
                                UserId = review.IdUser,
                                Message =
                                    $"❌ Bình luận của bạn trên bài đăng '{review.IdHouseNavigation?.NameHouse}' đã bị xóa bởi Admin.",
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                            }
                        );
                    }
                }

                if (notifications.Any())
                {
                    _context.Notifications.AddRange(notifications);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Không có thông báo nào được thêm.");
                }

                return Json(
                    new
                    {
                        success = true,
                        message = "Đã xóa bình luận thành công và gửi thông báo!",
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return Json(
                    new { success = false, message = "Lỗi khi xóa bình luận: " + ex.Message }
                );
            }
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

            // Gửi thông báo cho chủ bài đăng
            var notification = new Notification
            {
                UserId = house.IdUser,
                Message = $"✅ Bài đăng '{house.NameHouse}' của bạn đã được duyệt!",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

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

            // Gửi thông báo cho chủ bài đăng
            var notification = new Notification
            {
                UserId = house.IdUser,
                Message = $"❌ Bài đăng '{house.NameHouse}' của bạn đã bị từ chối.",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Bài đăng đã bị từ chối." });
        }

        // Quản lý loại nhà
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


        // Quản lý tiện nghi
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


        // Quản lý nhà trọ
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
                    houses = await _housesRepository.GetAllHousesAsync();
                }
                else
                {
                    houses = await _housesRepository.GetHousesByUserId(userId);
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


        // Quản lý báo cáo bài đăng
        [HttpGet]
        public async Task<IActionResult> PostReport()
        {
            // Lấy toàn bộ danh sách tài khoản
            var accounts = await _accountRepository.GetAllAccountsAsync();

            // Lấy toàn bộ danh sách bài đăng
            var houses = await _housesRepository.GetAllHousesAsync();

            if (accounts == null || !accounts.Any())
            {
                return PartialView("_PostReport", new List<AccountReportViewModel>());
            }

            // Đảm bảo houses không bị trùng lặp
            var distinctHouses = houses.GroupBy(h => h.IdHouse).Select(g => g.First()).ToList();

            // Tạo danh sách báo cáo từ Account và House
            var accountReports = accounts
                .Select(account =>
                {
                    var userPosts = distinctHouses.Where(h => h.IdUser == account.IdUser).ToList();

                    return new AccountReportViewModel
                    {
                        IdUser = account.IdUser,
                        UserName = account.UserName,
                        PhoneNumber = account.PhoneNumber,
                        Email = account.Email,
                        TotalPosts = userPosts.Count,
                        ApprovedPosts = userPosts.Count(h =>
                            h.Status == HouseStatus.Approved || h.Status == HouseStatus.Active
                        ),
                        RejectedPosts = userPosts.Count(h => h.Status == HouseStatus.Rejected),
                        PendingPosts = userPosts.Count(h => h.Status == HouseStatus.Pending),
                        HiddenPosts = userPosts.Count(h => h.Status == HouseStatus.Hidden),
                    };
                })
                .ToList();

            return PartialView("_PostReport", accountReports);
        }


        // Quản lý chat
        public IActionResult ManageChat()
        {
            var messages = _context
                .Messages.OrderBy(m => m.Timestamp) // Sắp xếp theo thời gian
                .Select(m => new
                {
                    ConversationId = ChatController.GenerateConversationId(
                        m.SenderId,
                        m.ReceiverId
                    ),
                    SenderId = m.SenderId,
                    SenderName = _context
                        .Accounts.Where(a => a.IdUser == m.SenderId)
                        .Select(a => a.UserName)
                        .FirstOrDefault(),
                    ReceiverId = m.ReceiverId,
                    ReceiverName = _context
                        .Accounts.Where(a => a.IdUser == m.ReceiverId)
                        .Select(a => a.UserName)
                        .FirstOrDefault(),
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                })
                .ToList();

            // Nhóm tin nhắn theo cuộc hội thoại
            var groupedMessages = messages
                .GroupBy(m => new
                {
                    SortedId = m.SenderId < m.ReceiverId
                        ? $"{m.SenderId}-{m.ReceiverId}"
                        : $"{m.ReceiverId}-{m.SenderId}",
                    SortedName = string.Compare(m.SenderName, m.ReceiverName) < 0
                        ? $"{m.SenderName} - {m.ReceiverName}"
                        : $"{m.ReceiverName} - {m.SenderName}",
                })
                .Select(g => new ManageChatViewModel
                {
                    ConversationId = g.Key.SortedId,
                    SenderName = g.First().SenderName,
                    ReceiverName = g.First().ReceiverName,
                    Content = g.OrderBy(m => m.Timestamp)
                        .Select(m => new ChatMessageViewModel
                        {
                            Sender = m.SenderName,
                            Content = m.Content,
                            Timestamp = m.Timestamp,
                        })
                        .ToList(),
                })
                .ToList();

            return PartialView("_ManageChat", groupedMessages);
        }

        [HttpPost]
        public IActionResult DeleteChat(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId) || !conversationId.Contains("-"))
            {
                return Json(new { success = false, message = "ConversationId không hợp lệ!" });
            }

            var ids = conversationId.Split('-');
            if (
                ids.Length != 2
                || !int.TryParse(ids[0], out int id1)
                || !int.TryParse(ids[1], out int id2)
            )
            {
                return Json(new { success = false, message = "Lỗi khi phân tích ConversationId!" });
            }

            // Lấy tất cả tin nhắn của cuộc hội thoại này
            var messages = _context
                .Messages.Where(m =>
                    (m.SenderId == id1 && m.ReceiverId == id2)
                    || (m.SenderId == id2 && m.ReceiverId == id1)
                )
                .ToList();

            if (!messages.Any())
            {
                return Json(new { success = false, message = "Không có tin nhắn để xóa!" });
            }

            _context.Messages.RemoveRange(messages);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa hội thoại thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> LockAccount(int id)
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            account.IsLocked = true;
            await _accountRepository.UpdateAccountAsync(account);

            return Json(new { success = true, message = "Tài khoản đã bị khóa." });
        }

        [HttpPost]
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            account.IsLocked = false;
            await _accountRepository.UpdateAccountAsync(account);

            return Json(new { success = true, message = "Tài khoản đã được mở khóa." });
        }


        // Quản lý báo cáo bài đăng theo ngày
        [HttpGet]
        public IActionResult PostReportByDay()
        {
            var postsByDay = _context
                .HouseDetails.Where(h => h.TimePost.Date == DateTime.Today)
                .OrderByDescending(h => h.TimePost)
                .Select(h => new PostByDayReportViewModel
                {
                    IdHouse = h.IdHouse,
                    Address = h.Address,
                    Price = h.Price,
                    Status = h.Status.ToString(),
                    TimePost = h.TimePost.Date,
                })
                .ToList();

            return PartialView("_PostReportByDay", postsByDay);
        }


        // Quản lý báo cáo bài đăng theo khoảng thời gian
        [HttpGet]
        public IActionResult PostReportByDayFilter(DateTime startDate, DateTime endDate)
        {
            var report = _context
                .HouseDetails.Where(h =>
                    h.TimePost.Date >= startDate.Date && h.TimePost.Date <= endDate.Date
                )
                .Select(h => new
                {
                    timePost = h.TimePost,
                    idHouse = h.IdHouse,
                    address = h.Address,
                    price = h.Price,
                    status = h.Status,
                })
                .OrderByDescending(h => h.timePost)
                .ToList();

            return Json(report);
        }
    }
}
