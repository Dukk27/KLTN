using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using KLTN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                        message = "Cập nhật thông tin tài khoản thành công!",
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
                    Message = $"Bài đăng có tiêu đề: {house.NameHouse} đã bị xóa bởi Admin.",
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HideHouse(int id)
        {
            var house = await _housesRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            // Cập nhật trạng thái bài đăng thành "Ẩn"
            house.Status = HouseStatus.Hidden;
            await _housesRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã được ẩn thành công." });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ShowHouse(int id)
        {
            var house = await _housesRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại!" });
            }

            // Đổi trạng thái thành Active (Hiện bài)
            house.Status = HouseStatus.Active;
            await _housesRepository.UpdateAsync(house);

            return Json(new { success = true, message = "Bài đăng đã được hiển thị lại!" });
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
                                    $"Bình luận của bạn trên bài đăng có tiêu đề: {review.IdHouseNavigation?.NameHouse} đã bị xóa bởi Admin.",
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

            var houseDetail = house.HouseDetails.FirstOrDefault();
            if (houseDetail == null)
            {
                return Json(
                    new { success = false, message = "Thông tin chi tiết bài đăng không tồn tại." }
                );
            }

            var now = DateTime.Now;

            // Nếu bài được đăng lại và hạn quá 30 ngày thì cho timepost = thời gian duyệt
            if (house.Status == HouseStatus.Pending && houseDetail.TimePost.AddDays(30) < now)
            {
                houseDetail.TimePost = now;
            }

            house.Status = HouseStatus.Approved;
            houseDetail.TimeUpdate = DateTime.Now;
            house.IsAutoHidden = false; // Nếu trước đó bị ẩn tự động, reset lại
            house.ReportVersion += 1; // Reset lượt report bằng cách tăng version
            await _housesRepository.UpdateAsync(house);

            // Gửi thông báo cho chủ bài đăng
            var notification = new Notification
            {
                UserId = house.IdUser,
                Message = $"Bài đăng có tiêu đề: {house.NameHouse} đã được duyệt!",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Bài đăng đã được duyệt." });
        }

        [HttpPost]
        public async Task<IActionResult> RejectPost(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });
            }

            var house = await _housesRepository.GetHouseWithDetailsAsync(id);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            house.Status = HouseStatus.Rejected;
            await _housesRepository.UpdateAsync(house);

            // Gửi thông báo cho chủ bài đăng kèm lý do
            var notification = new Notification
            {
                UserId = house.IdUser,
                Message = $"Bài đăng có tiêu đề: {house.NameHouse} đã bị từ chối.\nLý do: {reason}",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(
                new { success = true, message = "Bài đăng đã bị từ chối và lý do đã được gửi." }
            );
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
                        TotalPosts = userPosts.Count(h => h.Status != HouseStatus.Unpaid),
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
            // Lấy dữ liệu từ database
            var houseDetails = _context
                .HouseDetails.Include(h => h.IdHouseNavigation)
                .Where(h =>
                    h.TimePost.Date == DateTime.Today
                    && h.IdHouseNavigation != null
                    && h.IdHouseNavigation.Status != HouseStatus.Unpaid
                )
                .OrderByDescending(h => h.TimePost)
                .ToList();

            // Sau khi có dữ liệu, xử lý hiện
            var postsByDay = houseDetails
                .Select(h => new PostByDayReportViewModel
                {
                    IdHouse = h.IdHouse,
                    Address = h.Address,
                    Price = h.Price,
                    Status = TranslateStatus(h.IdHouseNavigation!.Status),
                    TimePost = h.TimePost.Date,
                    UserName = _context
                        .Accounts.Where(a => a.IdUser == h.IdHouseNavigation!.IdUser)
                        .Select(a => a.UserName)
                        .FirstOrDefault(),
                })
                .ToList();

            return PartialView("_PostReportByDay", postsByDay);
        }

        // Lọc báo cáo theo ngày
        [HttpGet]
        public IActionResult PostReportByDayFilter(DateTime startDate, DateTime endDate)
        {
            // Lấy dữ liệu từ database
            var houseDetails = _context
                .HouseDetails.Include(h => h.IdHouseNavigation)
                .Where(h =>
                    h.TimePost.Date >= startDate.Date
                    && h.TimePost.Date <= endDate.Date
                    && h.IdHouseNavigation != null
                    && h.IdHouseNavigation.Status != HouseStatus.Unpaid
                )
                .OrderByDescending(h => h.TimePost)
                .ToList();

            // Sau khi có dữ liệu, chuyển thành ViewModel
            var report = houseDetails
                .Select(h => new
                {
                    timePost = h.TimePost,
                    idHouse = h.IdHouse,
                    address = h.Address,
                    price = h.Price,
                    status = TranslateStatus(h.IdHouseNavigation!.Status),
                    userName = _context
                        .Accounts.Where(a => a.IdUser == h.IdHouseNavigation!.IdUser)
                        .Select(a => a.UserName)
                        .FirstOrDefault(),
                })
                .ToList(); // Lấy dữ liệu đã xử lý

            return Json(report); // Trả về dữ liệu đã xử lý
        }

        private string TranslateStatus(HouseStatus status)
        {
            return status switch
            {
                HouseStatus.Pending => "Chờ duyệt",
                HouseStatus.Approved => "Đã duyệt",
                HouseStatus.Hidden => "Đã ẩn",
                HouseStatus.Active => "Đã duyệt",
                HouseStatus.Rejected => "Từ chối",
                _ => "Không xác định",
            };
        }

        // Hiển thị danh sách báo xấu
        [HttpGet]
        public async Task<IActionResult> ManageReports()
        {
            var reports = await _context
                .Reports.Include(r => r.House)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt) // Sắp xếp theo thời gian mới nhất
                .ToListAsync();

            return PartialView("_ManageReport", reports);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetBaoCao(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return Json(new { success = false, message = "Không tìm thấy báo cáo." });
            }

            // Đánh dấu báo cáo là đã duyệt
            report.IsApproved = true;

            var house = await _context.Houses.FindAsync(report.HouseId);
            if (house == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài đăng." });
            }

            // Tạo thông báo cho người báo cáo
            _context.Notifications.Add(
                new Notification
                {
                    UserId = report.UserId,
                    Message =
                        $"Báo cáo của bạn đối với bài đăng có tiêu đề: {house.NameHouse} đã được admin duyệt.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                }
            );

            // Đếm số lượng báo cáo đã duyệt của bài đăng này (kể cả cái hiện tại đã đánh dấu ở trên)
            var approvedCount = await _context
                .Reports.Where(r =>
                    r.HouseId == report.HouseId
                    && r.IsApproved
                    && r.ReportVersion == house.ReportVersion
                )
                .CountAsync();

            // thêm 1 lần true ở trên
            if (approvedCount >= 2 && house.Status != HouseStatus.Hidden)
            {
                house.Status = HouseStatus.Hidden;
                house.IsAutoHidden = true;

                // Thông báo cho chủ bài đăng
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = house.IdUser,
                        Message =
                            $"Bài đăng của bạn có tiêu đề: {house.NameHouse} đã bị ẩn do bị báo cáo quá nhiều lần.",
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                    }
                );

                // Thông báo bổ sung cho người báo cáo
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = report.UserId,
                        Message =
                            $"Báo cáo của bạn đối với bài đăng có tiêu đề: {house.NameHouse} đã được admin duyệt và bài đăng đã bị ẩn.",
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                    }
                );
            }
            else
            {
                // Nếu chưa đủ 3, vẫn gửi thông báo cho người đăng
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = house.IdUser,
                        Message =
                            $"Bài đăng của bạn có tiêu đề: {house.NameHouse} đã bị báo cáo và báo cáo đã được admin duyệt.",
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                    }
                );
            }

            // Đếm tổng số báo cáo đã duyệt cho tất cả bài đăng của người đăng
            var totalApprovedReportsForUser = await _context
                .Reports.Where(r =>
                    r.IsApproved
                    && _context.Houses.Any(h => h.IdHouse == r.HouseId && h.IdUser == house.IdUser)
                )
                .CountAsync();

            if (totalApprovedReportsForUser >= 10)
            {
                var account = await _context.Accounts.FindAsync(house.IdUser);
                if (account != null && !account.IsLocked)
                {
                    account.IsLocked = true;

                    _context.Notifications.Add(
                        new Notification
                        {
                            UserId = account.IdUser,
                            Message = "Tài khoản của bạn đã bị khóa do bị báo cáo quá nhiều lần.",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                        }
                    );
                }
            }

            // Chỉ lưu 1 lần duy nhất
            await _context.SaveChangesAsync();

            return Json(
                new
                {
                    success = true,
                    autoHidden = (approvedCount >= 3),
                    message = (approvedCount >= 3)
                        ? "Bài đăng đã bị ẩn do bị báo cáo nhiều lần."
                        : "Báo cáo đã được duyệt.",
                }
            );
        }

        [HttpPost]
        public async Task<IActionResult> TuChoiBaoCao(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return Json(new { success = false, message = "Không tìm thấy báo cáo." });
            }

            var house = await _context.Houses.FindAsync(report.HouseId);
            if (house == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại." });
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho người báo cáo
            var notification = new Notification
            {
                UserId = report.UserId, // Người báo cáo
                Message =
                    $"Báo cáo của bạn đối với bài đăng có tiêu đề: {house.NameHouse} đã bị từ chối bởi Admin.",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(
                new
                {
                    success = true,
                    message = "Đã từ chối báo cáo và gửi thông báo đến người báo cáo.",
                }
            );
        }
    }
}
