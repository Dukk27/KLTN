goongjs.accessToken = "nFXjCGo9PppMz9YGX6S2fLdWnTwQKzCCmCimGy3G"; // MapTiles Key để hiển thị bản đồ
var map = new goongjs.Map({
  container: "map",
  style: "https://tiles.goong.io/assets/goong_map_web.json",
  center: [105.83991, 21.028], // Tọa độ mặc định (Hà Nội)
  zoom: 15,
});

function zoomIn() {
  var currentZoom = map.getZoom();
  if (currentZoom < 20) {
    map.zoomIn();
  }
}

function zoomOut() {
  var currentZoom = map.getZoom();
  if (currentZoom > 3) {
    map.zoomOut();
  }
}

// Hàm thay đổi kiểu bản đồ
function changeMapStyle(style) {
  let styleUrl = "";

  // Chọn URL của kiểu bản đồ tương ứng
  switch (style) {
    case "light":
      styleUrl = "https://tiles.goong.io/assets/goong_light_v2.json";
      break;
    case "dark":
      styleUrl = "https://tiles.goong.io/assets/goong_map_dark.json";
      break;
    case "navigation-day":
      styleUrl = "https://tiles.goong.io/assets/navigation_day.json";
      break;
    case "navigation-night":
      styleUrl = "https://tiles.goong.io/assets/navigation_night.json";
      break;
    case "default":
      styleUrl = "https://tiles.goong.io/assets/goong_map_web.json";
      break;
  }

  // Cập nhật kiểu bản đồ
  map.setStyle(styleUrl);
}

// Hàm hiển thị/ẩn menu khi người dùng nhấn nút
function toggleMapStyleMenu() {
  var menu = document.querySelector(".map-style-menu");
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

// Chức năng mở Google Maps để chỉ đường
function getDirections() {
  var address = document.getElementById("house-address").dataset.address;
  var decodedAddress = decodeHtmlEntities(address);

  if (!decodedAddress) {
    alert("Địa chỉ không hợp lệ.");
    return;
  }

  // Mở Google Maps với địa chỉ đã giải mã
  window.open(
    `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
      decodedAddress
    )}`,
    "_blank"
  );
}

// Giải mã HTML entities
function decodeHtmlEntities(input) {
  var doc = new DOMParser().parseFromString(input, "text/html");
  return doc.documentElement.textContent;
}

// Định vị vị trí người dùng
async function locateUser() {
  var address = document.getElementById("house-address").dataset.address;
  var decodedAddress = decodeHtmlEntities(address);

  if (!decodedAddress) {
    alert("Địa chỉ không hợp lệ.");
    return;
  }

  try {
    const response = await fetch(
      `https://rsapi.goong.io/Geocode?address=${encodeURIComponent(
        decodedAddress
      )}&api_key=RJ9d6oO2TLx8s4Q8riMgne1hI905XhC89HxJ7fIy`
    );
    const data = await response.json();

    if (data.results.length > 0) {
      const { geometry, formatted_address } = data.results[0];
      map.setCenter([geometry.location.lng, geometry.location.lat]);
      var marker = new goongjs.Marker()
        .setLngLat([geometry.location.lng, geometry.location.lat])
        .addTo(map)
        .setPopup(
          new goongjs.Popup().setHTML(
            `<strong>Địa chỉ:</strong> ${formatted_address}`
          )
        )
        .togglePopup();
    } else {
      alert("Không tìm thấy vị trí.");
    }
  } catch (error) {
    console.error("Lỗi khi gọi API Geocode:", error);
    alert("Không thể lấy vị trí.");
  }
}

// Hiện chi tiết ảnh
function openFullSizeImage(imgElement) {
  // Tạo div chứa ảnh full màn hình
  var fullScreenDiv = document.createElement("div");
  fullScreenDiv.classList.add("fullscreen-image");

  // Tạo ảnh lớn trong div
  var fullScreenImg = document.createElement("img");
  fullScreenImg.src = imgElement.src;

  // Khi click vào ảnh lớn thì đóng lại
  fullScreenDiv.onclick = function () {
    document.body.removeChild(fullScreenDiv);
  };

  // Thêm ảnh vào div
  fullScreenDiv.appendChild(fullScreenImg);
  document.body.appendChild(fullScreenDiv);
}

document.addEventListener("DOMContentLoaded", function () {
  let today = new Date().toISOString().split("T")[0];
  document.getElementById("appointmentDate").setAttribute("min", today);

  let isAuthenticated =
    document.getElementById("isAuthenticated").value === "true";
  let houseOwnerId = document.getElementById("houseOwnerId")?.value;
  let currentUserId = document.getElementById("currentUserId")?.value;

  document
    .getElementById("bookAppointmentBtn")
    .addEventListener("click", function () {
      if (!isAuthenticated) {
        iziToast.warning({
          title: "Cảnh báo",
          message: "Bạn cần đăng nhập để đặt lịch hẹn.",
          position: "topRight",
          timeout: 1000,
        });

        setTimeout(function () {
          window.location.href = "/Account/Login";
        }, 1000);

        return;
      }

      if (houseOwnerId === currentUserId) {
        iziToast.error({
          title: "Lỗi!",
          message: "Bạn không thể đặt lịch hẹn cho bài đăng của chính mình.",
          position: "topRight",
          timeout: 1500,
        });
        return;
      }

      document.getElementById("appointmentForm").style.display = "block";
      document.getElementById("appointmentOverlay").style.display = "block";
    });

  document.getElementById("closeForm").addEventListener("click", function () {
    document.getElementById("appointmentForm").style.display = "none";
    document.getElementById("appointmentOverlay").style.display = "none";
  });

  if (!document.getElementById("submitAppointment").dataset.listenerAdded) {
    document.getElementById("submitAppointment").dataset.listenerAdded = "true";

    document
      .getElementById("submitAppointment")
      .addEventListener("click", async function (event) {
        event.preventDefault();

        const date = document.getElementById("appointmentDate").value;
        const houseId = document.querySelector(".review-form").dataset.houseId;

        if (!date || date < today) {
          iziToast.error({
            title: "Lỗi!",
            message: "Vui lòng chọn ngày hợp lệ trước khi đặt lịch.",
            position: "topRight",
            timeout: 1500,
          });
          return;
        }

        const appointmentData = { HouseId: houseId, AppointmentDate: date };

        try {
          const response = await fetch("/Appointment/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(appointmentData),
          });

          const result = await response.json();

          if (result.success) {
            iziToast.success({
              title: "Thành công!",
              message: result.message,
              position: "topRight",
              timeout: 1500,
            });

            document.getElementById("appointmentForm").style.display = "none";
            document.getElementById("appointmentOverlay").style.display =
              "none";
          } else {
            iziToast.error({
              title: "Lỗi!",
              message: result.message || "Đã xảy ra lỗi khi đặt lịch.",
              position: "topRight",
              timeout: 1000,
            });
          }
        } catch (error) {
          iziToast.warning({
            title: "Cảnh báo",
            message: "Bạn cần đăng nhập để đặt lịch hẹn.",
            position: "topRight",
            timeout: 1000,
          });

          setTimeout(function () {
            window.location.href = "/Account/Login";
          }, 1000);
        }
      });
  }
});
