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

    // Chọn tất cả checkbox
    const selectAll = document.getElementById("select-all");
    selectAll?.addEventListener("change", function () {
        const checkboxes = document.querySelectorAll(".appointment-checkbox");
        checkboxes.forEach(cb => cb.checked = this.checked);
        toggleDeleteSelectedButton();
    });

    // Toggle khi chọn từng checkbox
    document.addEventListener("change", function (event) {
        if (event.target.classList.contains("appointment-checkbox")) {
            const all = document.querySelectorAll(".appointment-checkbox");
            const checked = document.querySelectorAll(".appointment-checkbox:checked");
            if (selectAll) selectAll.checked = all.length === checked.length;
            toggleDeleteSelectedButton();
        }
    });

    // Xóa nhiều lịch hẹn
    document.querySelector(".delete-selected-btn")?.addEventListener("click", function () {
        const selectedIds = Array.from(document.querySelectorAll(".appointment-checkbox:checked")).map(cb => cb.value);
        if (!selectedIds.length) return;

        iziToast.question({
            class: "iziToast-custom",
            timeout: false,
            close: false,
            overlay: true,
            displayMode: "once",
            title: "Xóa lịch hẹn đã chọn?",
            message: `Bạn chắc chắn muốn xóa ${selectedIds.length} lịch hẹn?`,
            position: "center",
            buttons: [
                ["<button><b>Xóa</b></button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);

                    fetch("/Appointment/DeleteSelected", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify(selectedIds)
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
                                selectedIds.forEach(id => {
                                    document.querySelector(`.accordion-item[data-id="${id}"]`)?.remove();
                                });
                                toggleDeleteSelectedButton();
                                checkAndShowNoAppointments();
                            }
                        });
                }, true],
                ["<button>Hủy</button>", function (instance, toast) {
                    instance.hide({ transitionOut: "fadeOut" }, toast);
                }]
            ]
        });
    });

    // Toggle nút xóa nhiều
    function toggleDeleteSelectedButton() {
        const count = document.querySelectorAll(".appointment-checkbox:checked").length;
        document.querySelector(".delete-selected-btn").disabled = count === 0;
    }

    // Khi không còn lịch hẹn nào
    function checkAndShowNoAppointments() {
        if (document.querySelectorAll(".accordion-item").length === 0) {
            document.querySelector(".table-container")?.remove();
            document.querySelector(".delete-selected-btn")?.remove();
            if (!document.querySelector(".no-appointments")) {
                const h2 = document.querySelector("h2");
                h2?.insertAdjacentHTML("afterend", '<div class="no-appointments">Chưa có lịch hẹn nào.</div>');
            }
        }
    }

    // Đổi badge trạng thái
    function updateStatus(id, statusText, badgeClass) {
        const item = document.querySelector(`.accordion-item[data-id="${id}"]`);
        const badge = item?.querySelector(".accordion-body .badge");
        if (badge) {
            badge.className = badgeClass;
            badge.textContent = statusText;
        }
    }

    // Vô hiệu hóa nút sau khi xác nhận/hủy
    function disableActions(id) {
        const item = document.querySelector(`.accordion-item[data-id="${id}"]`);
        const body = item?.querySelector(".accordion-body");
        body?.querySelectorAll(".confirm-btn, .cancel-btn").forEach(btn => btn.remove());

        if (!body?.querySelector(".text-muted")) {
            const msg = document.createElement("span");
            msg.className = "text-muted";
            msg.textContent = "Không khả dụng";
            body.appendChild(msg);
        }
    }
});
