using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private const int MaxLoginAtt = 3; // Số lần thử sai tối đa
        private const int Lockout = 5; // Thời gian khóa tài khoản (phút)

        public AccountController(
            IAccountRepository accountRepository,
            IMemoryCache memoryCache,
            IConfiguration configuration
        )
        {
            _accountRepository = accountRepository;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
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

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            // Kiểm tra tên đăng nhập
            if (await _accountRepository.IsUserNameExistAsync(model.UserName))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View(model);
            }
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
                HttpContext.Session.SetString("UserName", model.UserName);

                string role;
                switch (model.Role)
                {
                    case 1:
                        role = "ChuTro";
                        break;
                    case 2:
                        role = "NguoiTimPhong";
                        break;
                    default:
                        throw new InvalidOperationException("Role không hợp lệ");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
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

                return RedirectToAction("Login", "Account");
            }

            ViewBag.Error = "Đăng ký thất bại.";
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

        // Xử lý việc cập nhật tài khoản
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

            // if (ModelState.IsValid)
            // {
            await _accountRepository.UpdateAccountAsync(account);
            return RedirectToAction(nameof(Index), "Home");
            //}
            return View(account);
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

        // Xử lý gửi OTP qua email
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _accountRepository.GetAccountByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Tạo mã OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào MemoryCache (hết hạn sau 5 phút)
            _memoryCache.Set($"OTP_{email}", otp, TimeSpan.FromMinutes(5));

            // Gửi OTP qua email
            var subject = "Mã OTP đặt lại mật khẩu";
            var body = $"Mã OTP của bạn là: <b>{otp}</b>. Mã này sẽ hết hạn sau 5 phút.";
            await SendEmailAsync(email, subject, body);

            return RedirectToAction("VerifyOtp", new { email });
        }

        // Giao diện nhập OTP
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // Xác nhận OTP
        [HttpPost]
        public IActionResult VerifyOtp(string email, int otp)
        {
            // Kiểm tra OTP từ MemoryCache
            if (_memoryCache.TryGetValue($"OTP_{email}", out string savedOtp))
            {
                if (savedOtp == otp.ToString())
                {
                    // OTP hợp lệ, chuyển đến đặt lại mật khẩu
                    return RedirectToAction("ResetPassword", new { email });
                }
                else
                {
                    ViewBag.Message = "Mã OTP không chính xác!";
                }
            }
            else
            {
                ViewBag.Message = "Mã OTP đã hết hạn hoặc không tồn tại!";
            }

            ViewBag.Email = email;
            return View();
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
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Mật khẩu xác nhận không khớp!";
                ViewBag.Email = email;
                return View();
            }

            // Lấy user theo email
            var user = await _accountRepository.GetAccountByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Message = "Tài khoản không tồn tại!";
                return View();
            }

            // Cập nhật mật khẩu mới
            user.Password = newPassword;
            await _accountRepository.UpdateAccountAsync(user);

            ViewBag.Message = "Mật khẩu đã được cập nhật thành công!";
            return RedirectToAction("Login");
        }

        // Hàm gửi email
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            var client = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
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
    }
}
