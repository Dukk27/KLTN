let deletedImagesArray = [];
document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".remove-image").forEach((button) => {
    button.addEventListener("click", function () {
      let imgUrl = this.getAttribute("data-img");
      let deletedImagesInput = document.getElementById("deletedImages");

      // Nếu ảnh chưa được thêm vào danh sách xóa, thì thêm vào
      if (!deletedImagesInput.value.includes(imgUrl)) {
        deletedImagesInput.value += deletedImagesInput.value
          ? `,${imgUrl}`
          : imgUrl;
        deletedImagesArray.push(imgUrl);
        deletedImagesInput.value = deletedImagesArray.join(","); // Cập nhật danh sách xóa
      }

      this.parentElement.remove(); // Xóa ảnh khỏi giao diện
    });
  });

  // Xử lý xem trước ảnh mới khi chọn file
  document
    .getElementById("imageFiles")
    .addEventListener("change", function (event) {
      let previewContainer = document.getElementById("imagePreview");
      if (!previewContainer) {
        previewContainer = document.createElement("div");
        previewContainer.id = "imagePreview";
        previewContainer.classList.add("mt-2", "d-flex", "flex-wrap");
        this.parentElement.appendChild(previewContainer);
      }
      previewContainer.innerHTML = ""; // Xóa ảnh xem trước cũ

      Array.from(event.target.files).forEach((file) => {
        let reader = new FileReader();
        reader.onload = function (e) {
          let imgElement = document.createElement("img");
          imgElement.src = e.target.result;
          imgElement.style =
            "max-width: 100px; height: 100px; object-fit: cover; margin: 5px;";
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
    
    let isValid = true;
    $(".text-danger").text(""); // Xóa thông báo lỗi cũ

    if (!$("#fullAddress").val().trim()) {
      $("#fullAddressError").text("Vui lòng nhập địa chỉ cụ thể");
      isValid = false;
    }

    // Kiểm tra nếu địa chỉ không được chọn từ gợi ý
    if (!$('#address').val().trim()) {
        $('#fullAddressError').text("Vui lòng chọn địa chỉ từ danh sách gợi ý");
        isValid = false;
    }

    if (!$("#House_NameHouse").val().trim()) {
      $("#House_NameHouse").next(".text-danger").text("Vui lòng nhập tiêu đề!");
      isValid = false;
    }

    let dienTich = parseFloat($("#HouseDetail_DienTich").val());
    if (isNaN(dienTich) || dienTich <= 0) {
      $("#HouseDetail_DienTich")
        .next(".text-danger")
        .text("Diện tích phải là số dương!");
      isValid = false;
    }

    let price = parseFloat($("#HouseDetail_Price").val());
    if (isNaN(price) || price < 0) {
      $("#HouseDetail_Price")
        .next(".text-danger")
        .text("Giá thuê phải là số dương!");
      isValid = false;
    }

    let tienDienVal = $("#tienDien").val().replace(/\D/g, "");
    if ($("#electricityBillingMethod").val() !== "bao-gom") {
      if (!tienDienVal || isNaN(tienDienVal) || parseFloat(tienDienVal) <= 0) {
        $("span[data-valmsg-for='HouseDetail.TienDien']").text(
          "Vui lòng nhập giá điện hợp lệ!"
        );
        isValid = false;
      }
    }

    let tienNuocVal = $("#tienNuoc").val().replace(/\D/g, "");
    if ($("#waterBillingMethod").val() !== "bao-gom") {
      if (!tienNuocVal || isNaN(tienNuocVal) || parseFloat(tienNuocVal) <= 0) {
        $("span[data-valmsg-for='HouseDetail.TienNuoc']").text(
          "Vui lòng nhập giá nước hợp lệ!"
        );
        isValid = false;
      }
    }

    // Kiểm tra giá dịch vụ (phải là số dương)
    let tienDv = parseFloat($("#HouseDetail_TienDv").val());

    if (isNaN(tienDv) || tienDv < 0) {
      $("#HouseDetail_TienDv")
        .next(".text-danger")
        .text("Giá dịch vụ phải là số dương!");
      isValid = false;
    }

    // Kiểm tra loại nhà trọ (bắt buộc chọn)
    if (!$("#SelectedHouseType").val()) {
      $("#SelectedHouseType")
        .next(".text-danger")
        .text("Vui lòng chọn loại nhà trọ!");
      isValid = false;
    }

    // Kiểm tra tiện ích (ít nhất 1 tiện ích được chọn)
    if ($("input[name='SelectedAmenities']:checked").length === 0) {
      isValid = false;
      $(".amenities-container").append(
        '<span id="amenitiesError" class="text-danger d-block mt-2">Vui lòng chọn ít nhất một tiện ích!</span>'
      );
    }

    if (!isValid) {
      const firstError = document.querySelector(".text-danger:not(:empty)");
      if (firstError) {
        window.scrollTo({
          top: firstError.getBoundingClientRect().top + window.scrollY - 200,
          behavior: "smooth",
        });
      }
      return; // Ngăn iziToast nếu lỗi
    }

    iziToast.show({
      class: "iziToast-custom",
      title: "Xác nhận chỉnh sửa",
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
              body: new FormData(form),
            })
              .then((response) => response.json())
              .then((data) => {
                if (data.success) {
                  iziToast.success({
                    title: "Thành công!",
                    message: data.message,
                    position: "topRight",
                    timeout: 1000,
                    onClosing: function () {
                      window.location.href = "/QuanLi/ListHouseRoom";
                    },
                  });
                } else {
                  iziToast.error({
                    title: "Lỗi!",
                    message: data.message || "Có lỗi xảy ra!",
                    position: "topRight",
                  });
                }
              })
              .catch(() => {
                iziToast.error({
                  title: "Lỗi!",
                  message: "Không thể lưu thay đổi!",
                  position: "topRight",
                });
              });
          },
          true,
        ],
        [
          '<button><i class="fa fa-times"></i> Hủy</button>',
          function (instance, toast) {
            instance.hide({ transitionOut: "fadeOut" }, toast, "button");
          },
        ],
      ],
    });
  });

  function updateElectricityPrice() {
    let method = document.getElementById("electricityBillingMethod")?.value;
    let input = document.getElementById("tienDien");

    if (method === "bao-gom") {
        // Nếu phương thức là "Bao gồm trong tiền trọ"
        if (input.value !== "Bao gồm trong tiền trọ") {
            input.value = "Bao gồm trong tiền trọ"; // Gán lại giá trị
        }
        input.readOnly = true; // Làm cho ô nhập liệu không thể chỉnh sửa
    } else {
        input.readOnly = false; // Làm cho ô nhập liệu có thể chỉnh sửa lại
        let value = input.value.replace(/\D/g, ""); // Loại bỏ các ký tự không phải số
        if (value) {
            input.value = method === "theo-so" ? value + " VND/kWh" : value + " VND/Tháng";
        } else {
            input.value = ""; // Nếu không có giá trị thì gán rỗng
        }
    }
  }

  function updateWaterPrice() {
      let method = document.getElementById("waterBillingMethod")?.value;
      let input = document.getElementById("tienNuoc");

      if (method === "bao-gom") {
          // Nếu phương thức là "Bao gồm trong tiền trọ"
          if (input.value !== "Bao gồm trong tiền trọ") {
              input.value = "Bao gồm trong tiền trọ"; // Gán lại giá trị
          }
          input.readOnly = true; // Làm cho ô nhập liệu không thể chỉnh sửa
      } else {
          input.readOnly = false; // Làm cho ô nhập liệu có thể chỉnh sửa lại
          let value = input.value.replace(/\D/g, ""); // Loại bỏ các ký tự không phải số
          if (value) {
              input.value = method === "theo-so" ? value + " VND/m³" : value + " VND/Tháng";
          } else {
              input.value = ""; // Nếu không có giá trị thì gán rỗng
          }
      }
  }

  document
      .getElementById("electricityBillingMethod")
      ?.addEventListener("change", updateElectricityPrice);

  document
      .getElementById("waterBillingMethod")
      ?.addEventListener("change", updateWaterPrice);

  // Hàm khởi tạo trang, kiểm tra trạng thái ban đầu của các phương thức
  $(document).ready(function() {
      updateElectricityPrice(); // Đảm bảo giá trị được khởi tạo đúng khi trang load
      updateWaterPrice(); // Đảm bảo giá trị được khởi tạo đúng khi trang load
  });
});
