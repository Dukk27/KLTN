using System;
using System.Threading.Tasks;
using KLTN.Models;
using KLTN.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Route("api/[controller]")]
[ApiController]
public class DetailController : ControllerBase
{
    private readonly IHouseRepository _houseRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IHouseTypeRepository _houseTypeRepository;
    private readonly IAmenityRepository _amenityRepository;

    public DetailController(
        IHouseRepository houseRepository,
        IReviewRepository reviewRepository,
        IHouseTypeRepository houseTypeRepository,
        IAmenityRepository amenityRepository
    )
    {
        _houseRepository = houseRepository;
        _reviewRepository = reviewRepository;
        _houseTypeRepository = houseTypeRepository;
        _amenityRepository = amenityRepository;
    }

    [HttpGet("House/Detail/{id}")]
    public async Task<IActionResult> GetHouseDetail(int id)
    {
        var house = await _houseRepository.GetHouseWithDetailsAsync(id);
        //var houseType = _houseTypeRepository.GetHouseTypeName(house.IdHouseType);
        if (house == null)
            return NotFound(new { message = "Không tìm thấy nhà trọ." });

        var reviews = await _reviewRepository.GetReviewsByHouseIdAsync(id);
        return Ok(new { House = house, Reviews = reviews });
    }

    [HttpPost("house/detail/{id}/addreview")]
    public async Task<IActionResult> AddHouseReview(int id, [FromBody] Review review)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Hãy đăng nhập để được đánh giá." });
        

        ModelState.Remove("IdUserNavigation");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { message = "Dữ liệu đánh giá không hợp lệ.", errors = errors });
        }

        review.IdHouse = id;
        review.IdUser = userId.Value;
        review.ReviewDate = DateTime.Now;
        try
        {
            await _reviewRepository.AddReviewAsync(review);
            return Ok(new { message = "Đánh giá đã được thêm thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { message = "Lỗi máy chủ khi thêm đánh giá.", details = ex.Message }
            );
        }
    }
}
