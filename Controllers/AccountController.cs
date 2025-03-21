using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KLTN.Helpers;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace KLTN.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private const int MaxLoginAtt = 3; // Số lần thử sai tối đa
        private const int Lockout = 5; // Thời gian khóa tài khoản (phút)

        public AccountController(
            IAccountRepository accountRepository,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            EmailService emailService
        )
        {
            _accountRepository = accountRepository;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var userLock = await _accountRepository.GetUserIdByUsername(userName);
            if (userLock != null)
            {
                var account = await _accountRepository.GetAccountByIdAsync(userLock.Value);
                if (account.IsLocked)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
                    return View();
                }
            }

            if (_memoryCache.TryGetValue($"Lockout_{userName}", out DateTime lockoutEndTime))
            {
                if (lockoutEndTime > DateTime.Now)
                {
                    ViewBag.Error =
                        $"Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau {lockoutEndTime - DateTime.Now}.";
                    return View();
                }
                else
                {
                    _memoryCache.Remove($"Lockout_{userName}");
                }
            }

            var user = await _accountRepository.LoginAsync(userName, password);

            if (user != null)
            {
                _memoryCache.Remove($"FailedAttempts_{userName}");

                HttpContext.Session.SetInt32("UserId", user.IdUser);
                HttpContext.Session.SetString("UserName", user.UserName);
                string role;
                switch (user.Role)
                {
                    case 0:
                        role = "Admin";
                        break;
                    case 1:
                        role = "ChuTro";
                        break;
                    default:
                        role = "NguoiTimPhong";
                        break;
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, role),
                };
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
                if (role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            int failedAttempts = _memoryCache.Get<int>($"FailedAttempts_{userName}") + 1;
            _memoryCache.Set($"FailedAttempts_{userName}", failedAttempts);

            if (failedAttempts >= MaxLoginAtt)
            {
                _memoryCache.Set($"Lockout_{userName}", DateTime.Now.AddMinutes(Lockout));

                ViewBag.Error =
                    "Tài khoản của bạn đã bị khóa sau "
                    + MaxLoginAtt
                    + " lần thử sai. Vui lòng thử lại sau 5 phút.";
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("UserName");

            var userName = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(userName))
            {
                _memoryCache.Remove($"FailedAttempts_{userName}");
                _memoryCache.Remove($"Lockout_{userName}");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<string> errors = new List<string>();

            // Kiểm tra số điện thoại hợp lệ
            if (!Regex.IsMatch(model.PhoneNumber, @"^0\d{9}$"))
            {
                errors.Add(
                    "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số và bắt đầu bằng số 0."
                );
            }

            // Kiểm tra mật khẩu xác nhận
            if (model.Password != model.ConfirmPassword)
            {
                errors.Add("Mật khẩu xác nhận không khớp.");
            }

            if (model.ConfirmPassword == model.UserName)
            {
                errors.Add("Mật khẩu không được trùng với tên đăng nhập.");
            }

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (await _accountRepository.IsUserNameExistAsync(model.UserName))
            {
                errors.Add("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
            }

            // Nếu có bất kỳ lỗi nào, lưu vào ViewBag và quay lại View
            if (errors.Any())
            {
                ViewBag.Errors = errors; // Truyền danh sách lỗi
                return View(model);
            }

            // Nếu không có lỗi, tiếp tục đăng ký
            var account = new Account
            {
                UserName = model.UserName,
                Password = model.Password,
                Role = model.Role,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
            };

            bool isRegistered = await _accountRepository.RegisterAsync(account);

            if (isRegistered)
            {
                ViewBag.Success = "Đăng ký thành công! Hãy đăng nhập để tiếp tục.";
                return View(model);
            }

            ViewBag.Errors = new List<string> { "Đăng ký thất bại. Vui lòng thử lại." };
            return View(model);
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Manage()
        {
            var accounts = await _accountRepository.GetAllAccountsAsync();
            return View(accounts);
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
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            var existingUser = await _accountRepository.GetAccountByIdAsync(id);
            if (
                existingUser.UserName != account.UserName
                && await _accountRepository.IsUserNameExistAsync(account.UserName)
            )
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác!",
                    }
                );
            }

            var phonePattern = @"^0\d{9}$";
            if (!Regex.IsMatch(account.PhoneNumber, phonePattern))
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số và bắt đầu bằng số 0!",
                    }
                );
            }

            // Kiểm tra email hợp lệ
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(account.Email, emailPattern))
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Email không hợp lệ. Vui lòng nhập đúng định dạng!",
                    }
                );
            }

            await _accountRepository.UpdateAccountAsync(account);
            HttpContext.Session.SetString("UserName", account.UserName);

            return Json(
                new
                {
                    success = true,
                    message = "Cập nhật thông tin thành công!",
                    updatedData = new
                    {
                        userName = account.UserName,
                        phoneNumber = account.PhoneNumber,
                        email = account.Email,
                    },
                }
            );
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword
        )
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Bạn cần đăng nhập để thực hiện chức năng này!",
                    }
                );
            }

            var user = await _accountRepository.GetAccountByIdAsync(userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại!" });
            }

            if (user.Password != currentPassword)
            {
                return Json(new { success = false, message = "Mật khẩu cũ không đúng!" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Mật khẩu mới và xác nhận mật khẩu không khớp!",
                    }
                );
            }

            // Kiểm tra mật khẩu mới theo chuẩn Regex: ít nhất 8 ký tự, có số và chữ
            var passwordPattern = @"^(?=.*\d)(?=.*[a-zA-Z]).{8,}$";
            if (!Regex.IsMatch(newPassword, passwordPattern))
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ và số!",
                    }
                );
            }

            // Cập nhật mật khẩu mới
            user.Password = newPassword;
            await _accountRepository.UpdateAccountAsync(user);

            return Json(new { success = true, message = "Mật khẩu đã được thay đổi thành công!" });
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

        // Giao diện quên mật khẩu
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _accountRepository.GetAccountByEmailAsync(email);
            if (user == null)
            {
                return Json(
                    new { success = false, message = "Email không tồn tại trong hệ thống." }
                );
            }

            // Tạo mã OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào MemoryCache (hết hạn sau 5 phút)
            _memoryCache.Set($"OTP_{email}", otp, TimeSpan.FromMinutes(5));

            // Gửi OTP qua email bằng EmailService
            var subject = "Mã OTP đặt lại mật khẩu";
            var body = $"Mã OTP của bạn là: <b>{otp}</b>. Mã này sẽ hết hạn sau 5 phút.";

            await _emailService.SendEmailAsync("OTP", email, subject, body);

            return Json(
                new
                {
                    success = true,
                    message = "Mã OTP đã được gửi đến email của bạn.",
                    email,
                }
            );
        }

        // Giao diện nhập OTP
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string email, int otp)
        {
            if (_memoryCache.TryGetValue($"OTP_{email}", out string savedOtp))
            {
                if (savedOtp == otp.ToString())
                {
                    // Xóa OTP khỏi cache sau khi xác minh thành công
                    _memoryCache.Remove($"OTP_{email}");

                    return Json(
                        new
                        {
                            success = true,
                            message = "Xác minh thành công! Đang chuyển hướng...",
                            redirectUrl = Url.Action("ResetPassword", "Account", new { email }),
                        }
                    );
                }
                else
                {
                    return Json(new { success = false, message = "Mã OTP không chính xác!" });
                }
            }

            return Json(new { success = false, message = "Mã OTP đã hết hạn hoặc không tồn tại!" });
        }

        // Giao diện đặt lại mật khẩu
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // Xử lý đặt lại mật khẩu
        [HttpPost]
        public async Task<IActionResult> ResetPassword(
            string email,
            string newPassword,
            string confirmPassword
        )
        {
            var passwordPattern = @"^(?=.*\d)(?=.*[a-zA-Z]).{8,}$";
            if (!Regex.IsMatch(newPassword, passwordPattern))
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ và số!",
                    }
                );
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp!" });
            }

            // Lấy user theo email
            var user = await _accountRepository.GetAccountByEmailAsync(email);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại!" });
            }

            // Cập nhật mật khẩu mới
            user.Password = newPassword;
            await _accountRepository.UpdateAccountAsync(user);

            return Json(
                new
                {
                    success = true,
                    message = "Mật khẩu đã được cập nhật thành công!",
                    redirectUrl = Url.Action("Login", "Account"),
                }
            );
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var account = await _accountRepository.GetAccountByIdAsync(userId.Value);
            if (account == null)
            {
                return NotFound();
            }

            return PartialView("_Profile", account);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserName()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false });

            var user = await _accountRepository.GetAccountByIdAsync(userId.Value);
            return Json(new { success = true, userName = user?.UserName });
        }
    }
}
