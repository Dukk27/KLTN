
goongjs.accessToken = "nFXjCGo9PppMz9YGX6S2fLdWnTwQKzCCmCimGy3G"; // MapTiles Key để hiển thị bản đồ
var map = new goongjs.Map({
  container: "map",
  style: "https://tiles.goong.io/assets/goong_map_web.json",
  center: [105.83991, 21.028], // Tọa độ mặc định (Hà Nội)
  zoom: 14,
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

// Sử dụng API key cho các chức năng geocode, autocomplete, v.v.
async function setInitialAddress() {
  var addressFromModel =
    document.getElementById("house-address").dataset.address;
  var decodedAddress = new DOMParser().parseFromString(
    addressFromModel,
    "text/html"
  ).body.textContent;
  console.log("Địa chỉ giải mã:", decodedAddress);

  if (decodedAddress) {
    try {
      // Gọi API Geocode với API key
      const response = await fetch(
        `https://rsapi.goong.io/Geocode?address=${encodeURIComponent(
          decodedAddress
        )}&api_key=RJ9d6oO2TLx8s4Q8riMgne1hI905XhC89HxJ7fIy`
      );
      const data = await response.json();
      console.log("Kết quả từ API Goong:", data);

      if (data.results.length > 0) {
        const { geometry, formatted_address } = data.results[0];
        map.setCenter([geometry.location.lng, geometry.location.lat]);
        var marker = new goongjs.Marker({ color: "red" })
          .setLngLat([geometry.location.lng, geometry.location.lat])
          .addTo(map)
          .setPopup(
            new goongjs.Popup().setHTML(
              `<strong>Địa chỉ:</strong> ${formatted_address}`
            )
          )
          .togglePopup();
      } else {
        alert("Không tìm thấy địa chỉ.");
      }
    } catch (error) {
      console.error("Lỗi khi gọi API Geocode:", error);
    }
  } else {
    console.warn("Địa chỉ không hợp lệ.");
  }
}

setInitialAddress();

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


// Khởi tạo bản đồ Leaflet
var map = L.map("map").setView([21.028511, 105.804817], 13); // Mặc định là Hà Nội
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
  maxZoom: 19,
  attribution:
    '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
}).addTo(map);

var marker = L.marker([21.028511, 105.804817]).addTo(map);

// Lấy địa chỉ từ Model
var addressFromModel = document.getElementById("house-address").dataset.address;
var decodedAddress = new DOMParser().parseFromString(
  addressFromModel,
  "text/html"
).body.textContent;

async function setInitialAddress() {
  if (decodedAddress) {
    const response = await fetch(
      `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(
        decodedAddress
      )}&format=json`
    );
    const data = await response.json();

    if (data.length > 0) {
      const { lat, lon, display_name } = data[0];
      map.setView([lat, lon], 15);
      marker
        .setLatLng([lat, lon])
        .bindPopup(`<strong>Địa chỉ:</strong> ${display_name}`)
        .openPopup();
    }
  }
}
setInitialAddress();

// Hàm mở Google Maps để chỉ đường
function getDirections() {
  var address = decodeHtmlEntities(addressFromModel);
  if (!address) {
    alert("Địa chỉ không hợp lệ.");
    return;
  }
  window.open(
    `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
      address
    )}`,
    "_blank"
  );
}

// Hàm giải mã HTML entities
function decodeHtmlEntities(input) {
  var doc = new DOMParser().parseFromString(input, "text/html");
  return doc.documentElement.textContent;
}

// Định vị vị trí
async function locateUser() {
  if (!decodedAddress) {
    alert("Địa chỉ không hợp lệ.");
    return;
  }

  const response = await fetch(
    `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(
      decodedAddress
    )}&format=json`
  );
  const data = await response.json();

  if (data.length > 0) {
    const { lat, lon, display_name } = data[0];
    map.setView([lat, lon], 15);
    marker
      .setLatLng([lat, lon])
      .bindPopup(`<strong>Địa chỉ:</strong> ${display_name}`)
      .openPopup();
  } else {
    alert("Không tìm thấy vị trí.");
  }
}