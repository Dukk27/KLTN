// Gửi đánh giá
async function submitReview() {
    const rating = document.querySelector('input[name="rating"]:checked');
    const content = document.getElementById("Content").value;

    if (!rating || !content) {
        iziToast.warning({
            title: "Cảnh báo",
            message: "Vui lòng nhập đầy đủ thông tin.",
            position: "topRight",
            timeout: 1500,
        });
        return;
    }

    const houseId = document.querySelector(".review-form").dataset.houseId;
    if (!houseId) {
        iziToast.error({
            title: "Lỗi",
            message: "Không tìm thấy IdHouse. Vui lòng thử lại.",
            position: "topRight",
        });
        return;
    }

    const reviewData = {
        Rating: parseInt(rating.value),
        Content: content,
    };

    try {
        const response = await fetch(
            `/api/Detail/house/detail/${houseId}/addreview`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(reviewData),
            }
        );

        const result = await response.json();

        if (response.ok) {
            iziToast.success({
                title: "Thành công",
                message: result.message,
                position: "topRight",
                timeout: 1500,
            });

            setTimeout(() => location.reload(), 1500);
        } else {
            iziToast.error({
                title: "Lỗi",
                message: result.message || "Đã xảy ra lỗi khi gửi đánh giá.",
                position: "topRight",
                timeout: 1500,
            });
        }
    } catch (error) {
        iziToast.error({
            title: "Lỗi",
            message: "Đã xảy ra lỗi. Vui lòng thử lại.",
            position: "topRight",
            timeout: 1500,
        });
    }
}

document.addEventListener("DOMContentLoaded", function () {
    let today = new Date().toISOString().split("T")[0];
    document.getElementById("appointmentDate").setAttribute("min", today);

    let isAuthenticated =
        document.getElementById("isAuthenticated").value === "true";
    let houseOwnerId = document.getElementById("houseOwnerId")?.value;
    let currentUserId = document.getElementById("currentUserId")?.value;

    document.getElementById("bookAppointmentBtn").addEventListener("click", function () {
        if (!isAuthenticated) {
            window.location.href = "/Account/Login";
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

        document.getElementById("submitAppointment").addEventListener("click", async function (event) {
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
                    document.getElementById("appointmentOverlay").style.display = "none";
                } else {
                    iziToast.error({
                        title: "Lỗi!",
                        message: result.message || "Đã xảy ra lỗi khi đặt lịch.",
                        position: "topRight",
                        timeout: 1000,
                    });
                }
            } catch (error) {
                window.location.href = "/Account/Login";
            }
        });
    }
});