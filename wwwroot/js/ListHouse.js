document.addEventListener("DOMContentLoaded", function () {
  // Lưu trữ các hàm giao tiếp giữa iframe và trang chính
  window.closeModalFromIframe = function () {
    const modal = document.getElementById("createHouseModal");
    if (modal) {
      let modalInstance = bootstrap.Modal.getInstance(modal);
      if (modalInstance) modalInstance.hide();
    }
  };

  // Xử lý sự kiện khi modal đóng
  const createHouseModal = document.getElementById("createHouseModal");
  if (createHouseModal) {
    createHouseModal.addEventListener("hidden.bs.modal", function () {
      const iframe = document.getElementById("createHouseIframe");
      const loadingSpinner = document.getElementById("loadingSpinner");

      if (iframe) {
        iframe.src = "";
        iframe.style.display = "none";
      }
      if (loadingSpinner) loadingSpinner.style.display = "block";
    });
  }

  const showCreateHouseForm = document.getElementById("showCreateHouseForm");
  if (showCreateHouseForm) {
    showCreateHouseForm.addEventListener("click", function () {
      const userAuth = document.getElementById("userAuthenticated");
      const isAuthenticated = userAuth && userAuth.value.toLowerCase() === "true";

      // if (!isAuthenticated) {
      //   alert("Bạn chưa đăng nhập. Vui lòng đăng nhập để thực hiện chức năng này.");
      //   return;
      // }

      const modal = new bootstrap.Modal(document.getElementById("createHouseModal"));
      modal.show();

      const loadingSpinner = document.getElementById("loadingSpinner");
      const iframe = document.getElementById("createHouseIframe");

      if (loadingSpinner) loadingSpinner.style.display = "block";
      if (iframe) {
        iframe.style.display = "none";
        iframe.src = "/House/CreatePartial";

        iframe.onload = function () {
          if (loadingSpinner) loadingSpinner.style.display = "none";
          iframe.style.display = "block";

          try {
            let iframeHeight = iframe.contentWindow.document.body.scrollHeight;
            iframe.style.height = iframeHeight + 50 + "px";
          } catch (e) {
            console.error("Không thể điều chỉnh chiều cao iframe:", e);
            iframe.style.height = "600px";
          }

          try {
            iframe.contentWindow.closeModal = window.closeModalFromIframe;
          } catch (e) {
            console.error("Không thể truyền hàm closeModal vào iframe:", e);
          }
        };
      }
    });
  }

  // Đóng modal khi nhấn nút
  document.querySelectorAll("#closeModalBtn, #closeModalFooterBtn").forEach((btn) => {
    btn.addEventListener("click", function () {
      const modal = bootstrap.Modal.getInstance(document.getElementById("createHouseModal"));
      if (modal) modal.hide();
    });
  });

  // Đóng modal khi ấn bên ngoài
  if (createHouseModal) {
    createHouseModal.addEventListener("click", function (event) {
      if (event.target === createHouseModal) {
        const modalInstance = bootstrap.Modal.getInstance(createHouseModal);
        if (modalInstance) modalInstance.hide();
      }
    });
  }
});

// Điều hướng đến trang chi tiết
function goToDetail(houseId) {
  const detailUrl = `/Home/Detail?id=${houseId}`;
  fetch(detailUrl, {
    method: "GET",
    headers: { "X-Requested-With": "XMLHttpRequest" },
  })
    .then((response) => {
      if (response.ok) {
        window.location.href = detailUrl;
      } else {
        alert("Không tìm thấy trang chi tiết cho nhà trọ này.");
      }
    })
    .catch((error) => {
      console.error("Có lỗi xảy ra:", error);
      alert("Đã xảy ra lỗi khi chuyển đến trang chi tiết.");
    });
}

// Reset form khi mở modal filter
document.addEventListener("DOMContentLoaded", function () {
  const filterModal = document.getElementById("filterModal");
  if (filterModal) {
    filterModal.addEventListener("show.bs.modal", function () {
      const form = filterModal.querySelector("form");
      if (form) form.reset();
    });
  }
});

function toggleChatList() {
  let chatBox = $("#chatBoxContainer");
  if (chatBox.is(":visible")) {
    chatBox.fadeOut();
  } else {
    $.get("/Chat/ChatList", function (data) {
      $("#chatBoxContent").html(data);
      chatBox.fadeIn();
    });
  }
}

$(document).ready(function () {
  $('.slider-container').on('init', function (event, slick) {
      $(this).css('visibility', 'visible');
  });

  $('.slider-container').slick({
      infinite: true,
      slidesToShow: 1,
      slidesToScroll: 1,
      autoplay: true,
      autoplaySpeed: 2000,
      dots: true,
      fade: true
  });

  AOS.init({
      duration: 1000,
      once: true
  });
});

$(document).ready(function () {
  // Khởi tạo Select2 cho các trường
  $('#province, #district, #ward').select2({
      width: '100%',
      theme: 'classic',
      placeholder: 'Chọn một tùy chọn',
      allowClear: true
  });

  // Load tỉnh từ API và giữ lại giá trị đã chọn
  async function loadProvinces() {
      const res = await fetch("https://provinces.open-api.vn/api/p/");
      const data = await res.json();
      const provinceSelect = $('#province');
      const selectedProvince = provinceSelect.data('selected');

      provinceSelect.empty().append('<option value="">Tất cả</option>');
      data.forEach(p => {
          provinceSelect.append(`<option value="${p.name}" data-code="${p.code}" ${p.name === selectedProvince ? 'selected' : ''}>${p.name}</option>`);
      });

      // Nếu có tỉnh đã chọn thì load tiếp quận/huyện
      if (selectedProvince) {
          const selected = data.find(p => p.name === selectedProvince);
          if (selected) await loadDistricts(selected.code);
      }
  }

  // Load quận/huyện theo tỉnh và giữ lại giá trị đã chọn
  async function loadDistricts(code) {
      const res = await fetch(`https://provinces.open-api.vn/api/p/${code}?depth=2`);
      const data = await res.json();
      const districtSelect = $('#district');
      const selectedDistrict = districtSelect.data('selected');

      districtSelect.empty().append('<option value="">Tất cả</option>');
      data.districts.forEach(d => {
          districtSelect.append(`<option value="${d.name}" data-code="${d.code}" ${d.name === selectedDistrict ? 'selected' : ''}>${d.name}</option>`);
      });

      districtSelect.prop('disabled', false);

      // Nếu có quận/huyện đã chọn thì load tiếp phường/xã
      if (selectedDistrict) {
          const selected = data.districts.find(d => d.name === selectedDistrict);
          if (selected) await loadWards(selected.code);
      }
  }

  // Load phường/xã theo quận/huyện và giữ lại giá trị đã chọn
  async function loadWards(code) {
      const res = await fetch(`https://provinces.open-api.vn/api/d/${code}?depth=2`);
      const data = await res.json();
      const wardSelect = $('#ward');
      const selectedWard = wardSelect.data('selected');

      wardSelect.empty().append('<option value="">Tất cả</option>');
      data.wards.forEach(w => {
          wardSelect.append(`<option value="${w.name}" ${w.name === selectedWard ? 'selected' : ''}>${w.name}</option>`);
      });

      wardSelect.prop('disabled', false);
  }

  // Khi chọn tỉnh
  $('#province').on('change', async function () {
      const provinceCode = $(this).find(':selected').data('code');
      
      // Reset quận/huyện và phường/xã khi thay đổi tỉnh
      $('#district').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      $('#ward').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      
      if (provinceCode) {
          await loadDistricts(provinceCode);  // Load quận/huyện sau khi chọn tỉnh
      }
  });

  // Khi chọn quận/huyện
  $('#district').on('change', async function () {
      const districtCode = $(this).find(':selected').data('code');
      if (districtCode) {
          await loadWards(districtCode);  // Load phường/xã sau khi chọn quận/huyện
      } else {
          $('#ward').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      }
  });

  // Khởi tạo các tỉnh lúc load trang
  loadProvinces();
});

// Lọc tỉnh, huyện, xã cho phần bên phải dùng select2
$(document).ready(function () {
  // Khởi tạo Select2 cho các trường
  $('#provinceFilter, #districtFilter, #wardFilter').select2({
      width: '100%',
      theme: 'classic',
      placeholder: 'Chọn một tùy chọn',
      allowClear: true
  });

  // Load tỉnh từ API và giữ lại giá trị đã chọn
  async function loadProvincesFilter() {
      const res = await fetch("https://provinces.open-api.vn/api/p/");
      const data = await res.json();
      const provinceSelect = $('#provinceFilter');
      const selectedProvince = provinceSelect.data('selected');

      provinceSelect.empty().append('<option value="">Tất cả</option>');
      data.forEach(p => {
          provinceSelect.append(`<option value="${p.name}" data-code="${p.code}" ${p.name === selectedProvince ? 'selected' : ''}>${p.name}</option>`);
      });

      // Nếu có tỉnh đã chọn thì load tiếp quận/huyện
      if (selectedProvince) {
          const selected = data.find(p => p.name === selectedProvince);
          if (selected) await loadDistrictsFilter(selected.code);
      }
  }

  // Load quận/huyện theo tỉnh và giữ lại giá trị đã chọn
  async function loadDistrictsFilter(code) {
      const res = await fetch(`https://provinces.open-api.vn/api/p/${code}?depth=2`);
      const data = await res.json();
      const districtSelect = $('#districtFilter');
      const selectedDistrict = districtSelect.data('selected');

      districtSelect.empty().append('<option value="">Tất cả</option>');
      data.districts.forEach(d => {
          districtSelect.append(`<option value="${d.name}" data-code="${d.code}" ${d.name === selectedDistrict ? 'selected' : ''}>${d.name}</option>`);
      });

      districtSelect.prop('disabled', false);

      // Nếu có quận/huyện đã chọn thì load tiếp phường/xã
      if (selectedDistrict) {
          const selected = data.districts.find(d => d.name === selectedDistrict);
          if (selected) await loadWardsFilter(selected.code);
      }
  }

  // Load phường/xã theo quận/huyện và giữ lại giá trị đã chọn
  async function loadWardsFilter(code) {
      const res = await fetch(`https://provinces.open-api.vn/api/d/${code}?depth=2`);
      const data = await res.json();
      const wardSelect = $('#wardFilter');
      const selectedWard = wardSelect.data('selected');

      wardSelect.empty().append('<option value="">Tất cả</option>');
      data.wards.forEach(w => {
          wardSelect.append(`<option value="${w.name}" ${w.name === selectedWard ? 'selected' : ''}>${w.name}</option>`);
      });

      wardSelect.prop('disabled', false);
  }

  // Khi chọn tỉnh
  $('#provinceFilter').on('change', async function () {
      const provinceCode = $(this).find(':selected').data('code');
      
      // Reset quận/huyện và phường/xã khi thay đổi tỉnh
      $('#districtFilter').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      $('#wardFilter').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      
      if (provinceCode) {
          await loadDistrictsFilter(provinceCode);  // Load quận/huyện sau khi chọn tỉnh
      }
  });

  // Khi chọn quận/huyện
  $('#districtFilter').on('change', async function () {
      const districtCode = $(this).find(':selected').data('code');
      if (districtCode) {
          await loadWardsFilter(districtCode);  // Load phường/xã sau khi chọn quận/huyện
      } else {
          $('#wardFilter').prop('disabled', true).empty().append('<option value="">Tất cả</option>');
      }
  });

  // Gọi hàm khởi tạo
  loadProvincesFilter();
});
