document.addEventListener('DOMContentLoaded', function () {
    let today = new Date().toISOString().split('T')[0];
    document.getElementById('appointmentDate').setAttribute('min', today);
});

// Khởi tạo bản đồ Leaflet
var map = L.map('map').setView([21.028511, 105.804817], 13); // Mặc định là Hà Nội
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);

var marker = L.marker([21.028511, 105.804817]).addTo(map);

// Lấy địa chỉ từ Model
var addressFromModel = document.getElementById('house-address').dataset.address;
var decodedAddress = new DOMParser().parseFromString(addressFromModel, "text/html").body.textContent;

async function setInitialAddress() {
    if (decodedAddress) {
        const response = await fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(decodedAddress)}&format=json`);
        const data = await response.json();

        if (data.length > 0) {
            const { lat, lon, display_name } = data[0];
            map.setView([lat, lon], 15);
            marker.setLatLng([lat, lon]).bindPopup(`<strong>Địa chỉ:</strong> ${display_name}`).openPopup();
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
    window.open(`https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(address)}`, '_blank');
}

// Hàm giải mã HTML entities
function decodeHtmlEntities(input) {
    var doc = new DOMParser().parseFromString(input, 'text/html');
    return doc.documentElement.textContent;
}

// Gửi đánh giá
async function submitReview() {
    const rating = document.querySelector('input[name="rating"]:checked');
    const content = document.getElementById('Content').value;
    const reviewMessage = document.getElementById('reviewMessage');

    if (!rating || !content) {
        reviewMessage.innerText = "Vui lòng nhập đầy đủ thông tin.";
        reviewMessage.style.display = "block";
        return;
    }

    const houseId = document.querySelector('.review-form').dataset.houseId;
    if (!houseId) {
        reviewMessage.innerText = "Không tìm thấy IdHouse. Vui lòng thử lại.";
        reviewMessage.style.display = "block";
        return;
    }

    const reviewData = {
        Rating: parseInt(rating.value),
        Content: content
    };

    try {
        const response = await fetch(`/api/Detail/house/detail/${houseId}/addreview`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(reviewData)
        });

        const result = await response.json();
        alert(result.message);
        if (response.ok) {
            location.reload();
        }
    } catch (error) {
        reviewMessage.innerText = "Đã xảy ra lỗi. Vui lòng thử lại.";
        reviewMessage.style.display = "block";
    }
}

// Định vị vị trí
async function locateUser() {
    if (!decodedAddress) {
        alert("Địa chỉ không hợp lệ.");
        return;
    }

    const response = await fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(decodedAddress)}&format=json`);
    const data = await response.json();

    if (data.length > 0) {
        const { lat, lon, display_name } = data[0];
        map.setView([lat, lon], 15);
        marker.setLatLng([lat, lon]).bindPopup(`<strong>Địa chỉ:</strong> ${display_name}`).openPopup();
    } else {
        alert("Không tìm thấy vị trí.");
    }
}

document.addEventListener('DOMContentLoaded', function () {
    let today = new Date().toISOString().split('T')[0];
    document.getElementById('appointmentDate').setAttribute('min', today);
});

document.addEventListener("DOMContentLoaded", function () {
    let isAuthenticated = document.getElementById("isAuthenticated").value === "true";
    let userRole = parseInt(document.getElementById("userRole").value);

    document.getElementById("bookAppointmentBtn").addEventListener("click", function () {
        if (!isAuthenticated) {
            Swal.fire({
                icon: "warning",
                title: "Yêu cầu đăng nhập",
                text: "Bạn cần đăng nhập để đặt lịch hẹn.",
                confirmButtonText: "Đăng nhập",
                showCancelButton: true,
                cancelButtonText: "Hủy"
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = "/Account/Login"; // Chuyển hướng đến trang login
                }
            });
            return;
        }

        if (userRole !== 2) {
            Swal.fire({
                icon: "error",
                title: "Không có quyền!",
                text: "Bạn không có quyền đặt lịch hẹn.",
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

    document.getElementById("submitAppointment")?.addEventListener("click", async function () {
        const date = document.getElementById("appointmentDate").value;
        const houseId = document.querySelector(".review-form").dataset.houseId;

        if (!date) {
            Swal.fire({
                icon: "warning",
                title: "Lỗi!",
                text: "Vui lòng chọn ngày đặt lịch hẹn.",
            });
            return;
        }

        const appointmentData = { HouseId: houseId, AppointmentDate: date };

        try {
            const response = await fetch("/Appointment/Create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(appointmentData)
            });

            const result = await response.json();

            if (result.success) {
                Swal.fire({
                    icon: "success",
                    title: "Thành công!",
                    text: result.message,
                }).then(() => {
                    document.getElementById("appointmentForm").style.display = "none";
                    document.getElementById("appointmentOverlay").style.display = "none";
                });
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi!",
                    text: result.message || "Đã xảy ra lỗi khi đặt lịch.",
                });
            }
        } catch (error) {
            Swal.fire({
                icon: "error",
                title: "Lỗi!",
                text: "Vui lòng đăng nhập để đặt lịch hẹn.",
                confirmButtonText: "Đăng nhập"
            }).then(() => {
                window.location.href = "/Account/Login";
            });
        }
    });
});
