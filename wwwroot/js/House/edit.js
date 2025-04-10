let deletedImagesArray = []; 
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".remove-image").forEach(button => {
        button.addEventListener("click", function () {
            let imgUrl = this.getAttribute("data-img");
            let deletedImagesInput = document.getElementById("deletedImages");

            // Nếu ảnh chưa được thêm vào danh sách xóa, thì thêm vào
            if (!deletedImagesInput.value.includes(imgUrl)) {
                deletedImagesInput.value += deletedImagesInput.value ? `,${imgUrl}` : imgUrl;
                deletedImagesArray.push(imgUrl);
                deletedImagesInput.value = deletedImagesArray.join(","); // Cập nhật danh sách xóa
            }

            this.parentElement.remove(); // Xóa ảnh khỏi giao diện
        });
    });

    // Xử lý xem trước ảnh mới khi chọn file
    document.getElementById("imageFiles").addEventListener("change", function (event) {
        let previewContainer = document.getElementById("imagePreview");
        if (!previewContainer) {
            previewContainer = document.createElement("div");
            previewContainer.id = "imagePreview";
            previewContainer.classList.add("mt-2", "d-flex", "flex-wrap");
            this.parentElement.appendChild(previewContainer);
        }
        previewContainer.innerHTML = ""; // Xóa ảnh xem trước cũ

        Array.from(event.target.files).forEach(file => {
            let reader = new FileReader();
            reader.onload = function (e) {
                let imgElement = document.createElement("img");
                imgElement.src = e.target.result;
                imgElement.style = "max-width: 100px; height: 100px; object-fit: cover; margin: 5px;";
                previewContainer.appendChild(imgElement);
            };
            reader.readAsDataURL(file);
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");

    form.addEventListener("submit", function (event) {
        event.preventDefault(); // Ngăn chặn submit mặc định

        iziToast.show({
            class: "iziToast-custom",
            title: "🔔 Xác nhận chỉnh sửa",
            message: "Bạn có chắc chắn muốn chỉnh sửa bài đăng này?",
            position: "center",
            timeout: false,
            close: false,
            overlay: true,
            displayMode: "once",
            drag: false,
            icon: "fa fa-question-circle",
            buttons: [
                [
                    '<button><i class="fa fa-check"></i> Lưu</button>',
                    function (instance, toast) {
                        instance.hide({ transitionOut: "fadeOut" }, toast, "button");

                        fetch(form.action, {
                            method: "POST",
                            body: new FormData(form)
                        })
                            .then(response => response.json())
                            .then(data => {
                                if (data.success) {
                                    iziToast.success({
                                        title: "Thành công!",
                                        message: data.message,
                                        position: "topRight",
                                        timeout: 1000,
                                        onClosing: function () {
                                            window.location.href = "/QuanLi/ListHouseRoom";
                                        }
                                    });
                                } else {
                                    iziToast.error({
                                        title: "Lỗi!",
                                        message: data.message || "Có lỗi xảy ra!",
                                        position: "topRight"
                                    });
                                }
                            })
                            .catch(() => {
                                iziToast.error({
                                    title: "Lỗi!",
                                    message: "Không thể lưu thay đổi!",
                                    position: "topRight"
                                });
                            });
                    },
                    true
                ],
                [
                    '<button><i class="fa fa-times"></i> Hủy</button>',
                    function (instance, toast) {
                        instance.hide({ transitionOut: "fadeOut" }, toast, "button");
                    }
                ]
            ]
        });
    });

    function updateElectricityPrice() {
        let method = document.getElementById("electricityBillingMethod")?.value;
        let input = document.getElementById("tienDien");

        if (method === "bao-gom") {
            input.value = "Bao gồm trong tiền trọ";
            input.readOnly = true;
        } else {
            if (input.readOnly) {
                input.value = "";
            }
            input.readOnly = false;

            let value = input.value.replace(/\D/g, "");
            if (value) {
                input.value = method === "theo-so" ? value + " VND/kWh" : value + " VND/Tháng";
            }
        }
    }

    function updateWaterPrice() {
        let method = document.getElementById("waterBillingMethod")?.value;
        let input = document.getElementById("tienNuoc");

        if (method === "bao-gom") {
            input.value = "Bao gồm trong tiền trọ";
            input.readOnly = true;
        } else {
            if (input.readOnly) {
                input.value = "";
            }
            input.readOnly = false;

            let value = input.value.replace(/\D/g, "");
            if (value) {
                input.value = method === "theo-so" ? value + " VND/m³" : value + " VND/Tháng";
            }
        }
    }

    document.getElementById("electricityBillingMethod")?.addEventListener("change", updateElectricityPrice);
    document.getElementById("tienDien")?.addEventListener("input", updateElectricityPrice);

    document.getElementById("waterBillingMethod")?.addEventListener("change", updateWaterPrice);
    document.getElementById("tienNuoc")?.addEventListener("input", updateWaterPrice);

    const goongApiKey = "RJ9d6oO2TLx8s4Q8riMgne1hI905XhC89HxJ7fIy";

    document.getElementById("houseAddress")?.addEventListener("input", function () {
        let inputText = this.value.trim();
        let addressSuggestions = document.getElementById("addressSuggestions");

        if (inputText.length < 3) {
            addressSuggestions.innerHTML = "";
            return;
        }

        let apiUrl = `https://rsapi.goong.io/Place/AutoComplete?api_key=${goongApiKey}&input=${encodeURIComponent(inputText)}`;

        fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            addressSuggestions.innerHTML = "";
            if (data.predictions) {
                data.predictions.forEach(item => {
                    let listItem = document.createElement("li");
                    listItem.classList.add("list-group-item", "address-item");
                    listItem.textContent = item.description;
                    listItem.dataset.address = item.description;
                    addressSuggestions.appendChild(listItem);
                });
                addressSuggestions.style.display = "block";
            }
        });
    });

    document.getElementById("addressSuggestions")?.addEventListener("click", function (event) {
        if (event.target.classList.contains("address-item")) {
            document.getElementById("houseAddress").value = event.target.dataset.address;
            this.innerHTML = "";
            this.style.display = "none";
        }
    });

    document.addEventListener("click", function (event) {
        let addressSuggestions = document.getElementById("addressSuggestions");
        if (!event.target.closest("#houseAddress, #addressSuggestions")) {
            addressSuggestions.innerHTML = "";
            addressSuggestions.style.display = "none";
        }
    });
});

