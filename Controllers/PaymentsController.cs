using System;
using System.Linq;
using System.Threading.Tasks;
using KLTN.Helpers;
using KLTN.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KLTN.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly KLTNContext _context;
        private readonly ILogger<PaymentsController> _logger;
        private const string VNP_TMNCODE = "NLN17MWG";
        private const string VNP_HASHSECRET = "E8NF6Z7ZKFJPNWP0OZKZ3PHIWM0X8LOJ";
        private const string VNP_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public PaymentsController(KLTNContext context, ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Payment(int houseId)
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lưu thông tin thanh toán vào session
                HttpContext.Session.SetInt32("PaymentHouseId", houseId);
                HttpContext.Session.SetInt32("PaymentUserId", userId);

                string orderInfo = $"Thanh toán cho bài đăng {houseId}";
                string orderType = "billpayment";
                decimal amount = 10000; // Số tiền thanh toán

                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", VNP_TMNCODE);
                vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", vnpay.GetIpAddress(HttpContext));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
                vnpay.AddRequestData("vnp_OrderType", orderType);
                vnpay.AddRequestData(
                    "vnp_ReturnUrl",
                    Url.Action("PaymentCallback", "Payments", null, Request.Scheme)
                );
                vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(VNP_URL, VNP_HASHSECRET);
                _logger.LogInformation($"Payment URL: {paymentUrl}");

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Payment: {ex.Message}");
                return View("Failure");
            }
        }

        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                _logger.LogInformation("PaymentCallback called");

                // Lấy thông tin từ query string
                var vnpayData = Request.Query;
                foreach (var item in vnpayData)
                {
                    _logger.LogInformation($"{item.Key}: {item.Value}");
                }

                string vnp_ResponseCode = vnpayData["vnp_ResponseCode"].ToString();
                string vnp_TransactionNo = vnpayData["vnp_TransactionNo"].ToString();
                string vnp_TxnRef = vnpayData["vnp_TxnRef"].ToString();
                string vnp_SecureHash = vnpayData["vnp_SecureHash"].ToString();

                // Lấy thông tin từ session
                int houseId = HttpContext.Session.GetInt32("PaymentHouseId") ?? 0;
                int userId = HttpContext.Session.GetInt32("PaymentUserId") ?? 0;

                _logger.LogInformation($"HouseId: {houseId}, UserId: {userId}");

                if (vnp_ResponseCode == "00") // Thanh toán thành công
                {
                    var house = await _context.Houses.FindAsync(houseId);
                    if (house != null && house.Status == HouseStatus.Unpaid)
                    {
                        house.Status = HouseStatus.Pending;
                        await _context.SaveChangesAsync();
                    }

                    var payment = new Payment
                    {
                        UserId = userId,
                        HouseId = houseId,
                        Amount = 10000,
                        PaymentStatus = "Success",
                        TransactionId = vnp_TransactionNo,
                        PaymentDate = DateTime.Now,
                    };

                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    var userPost = new UserPost
                    {
                        UserId = userId,
                        HouseId = houseId,
                        IsFree = false,
                        PostDate = DateTime.Now,
                    };

                    // Thêm thông báo cho admin khi bài đăng được duyệt sau thanh toán
                    var adminAccounts = _context.Accounts.Where(u => u.Role == 0).ToList();
                    foreach (var admin in adminAccounts)
                    {
                        var notification = new Notification
                        {
                            UserId = admin.IdUser, // Gửi thông báo cho Admin
                            Message =
                                $"💰 Bài đăng có tiêu đề: {house.NameHouse} đã được thanh toán và chờ duyệt.",
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                        };
                        _context.Notifications.Add(notification);
                    }

                    _context.UserPosts.Add(userPost);
                    await _context.SaveChangesAsync();

                    return View("Success");
                }
                else
                {
                    return RedirectToAction("CancelPayment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PaymentCallback: {ex.Message}");
                return View("Failure");
            }
        }

        public async Task<IActionResult> CancelPayment()
        {
            int houseId = HttpContext.Session.GetInt32("PaymentHouseId") ?? 0;
            if (houseId > 0)
            {
                var house = await _context.Houses.FindAsync(houseId);
                if (house != null && house.Status == HouseStatus.Unpaid)
                {
                    _context.Houses.Remove(house);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
