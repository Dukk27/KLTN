document.addEventListener("DOMContentLoaded", function () {

    // Xác nhận lịch hẹn
    document.addEventListener("click", function (event) {
        const confirmBtn = event.target.closest(".confirm-btn");
        if (!confirmBtn) return;

        const id = confirmBtn.dataset.id;

        iziToast.question({
            class: "iziToast-custom",
            timeout: false,
            close: false,
            overlay: true,
            displayMode: "once",
            title: "Xác nhận lịch hẹn?",
            message: "Bạn có chắc chắn muốn xác nhận lịch hẹn này?",
            position: "center",
            buttons: [
                ["<button><b>Xác nhận</b></button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);

                    fetch("/Appointment/Confirm", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/x-www-form-urlencoded"
                        },
                        body: new URLSearchParams({ id })
                    })
                    .then(res => res.json())
                    .then(res => {
                        iziToast[res.success ? "success" : "error"]({
                            title: res.success,
                            message: res.message,
                            position: "topRight",
                            timeout: 1000
                        });

                        if (res.success) {
                            // Cập nhật trạng thái và badge
                            updateStatus(id, "Đã xác nhận", "badge bg-success");
                            disableActions(id);
                        }
                    });
                }, true],
                ["<button>Hủy</button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);
                }]
            ]
        });
    });

    // Hủy lịch hẹn
    document.addEventListener("click", function (event) {
        const cancelBtn = event.target.closest(".cancel-btn");
        if (!cancelBtn) return;

        const id = cancelBtn.dataset.id;

        iziToast.question({
            class: "iziToast-custom",
            timeout: false,
            close: false,
            overlay: true,
            displayMode: "once",
            title: "Hủy lịch hẹn?",
            message: "Bạn có chắc chắn muốn hủy lịch hẹn này?",
            position: "center",
            buttons: [
                ["<button><b>Hủy</b></button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);

                    fetch("/Appointment/Cancel", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/x-www-form-urlencoded"
                        },
                        body: new URLSearchParams({ id })
                    })
                    .then(res => res.json())
                    .then(res => {
                        iziToast[res.success ? "success" : "error"]({
                            title: res.success,
                            message: res.message,
                            position: "topRight",
                            timeout: 1000
                        });

                        if (res.success) {
                            // Cập nhật trạng thái và badge
                            updateStatus(id, "Đã hủy", "badge bg-danger");
                            disableActions(id);
                        }
                    });
                }, true],
                ["<button>Quay lại</button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);
                }]
            ]
        });
    });

    // Cập nhật trạng thái và badge
    function updateStatus(id, statusText, badgeClass) {
        const row = document.querySelector(`tr[data-id="${id}"]`);
        const badge = row?.querySelector(".badge");
        if (badge) {
            badge.className = badgeClass;
            badge.textContent = statusText;
        }
    }

    // Vô hiệu hóa nút sau khi xác nhận/hủy
    function disableActions(id) {
        const row = document.querySelector(`tr[data-id="${id}"]`);
        const actionsCell = row?.querySelector(".action-buttons");
        
        actionsCell?.querySelectorAll(".confirm-btn, .cancel-btn").forEach(btn => btn.remove());

        if (!actionsCell?.querySelector(".text-muted")) {
            const msg = document.createElement("span");
            msg.className = "text-muted";
            msg.textContent = "Không khả dụng";
            actionsCell.appendChild(msg);
        }
    }
});
