function viewDetail(id, type) {
  const url = `/Home/Detail?id=${id}&type=${type}`;
  fetch(url, { method: "GET" })
    .then((response) => {
      if (response.ok) {
        window.location.href = url;
      } else {
        iziToast.error({
          title: "Lỗi!",
          message: "Không thể tải chi tiết. Vui lòng thử lại.",
          position: "topRight",
        });
      }
    })
    .catch(() => {
      iziToast.error({
        title: "Lỗi!",
        message: "Đã xảy ra lỗi khi xử lý yêu cầu.",
        position: "topRight",
      });
    });
}

function editItem(id) {
  const url = `/QuanLi/EditHouse?id=${id}`;
  fetch(url, { method: "GET" })
    .then((response) => {
      if (response.ok) {
        window.location.href = url;
      } else {
        iziToast.error({
          title: "Lỗi!",
          message: "Không thể tải trang chỉnh sửa. Vui lòng thử lại.",
          position: "topRight",
        });
      }
    })
    .catch(() => {
      iziToast.error({
        title: "Lỗi!",
        message: "Đã xảy ra lỗi khi xử lý yêu cầu.",
        position: "topRight",
      });
    });
}

async function deleteHouse(id) {
  iziToast.show({
    class: "iziToast-custom",
    title: "Xác nhận xóa",
    message: "Bạn có chắc chắn muốn xóa? Dữ liệu này sẽ không thể khôi phục!",
    position: "center",
    timeout: false,
    close: false,
    overlay: true,
    displayMode: "once",
    drag: false,
    icon: "fa fa-warning",
    buttons: [
      [
        '<button><i class="fa fa-check"></i> Xóa ngay</button>',
        async function (instance, toast) {
          instance.hide({ transitionOut: "fadeOut" }, toast, "button");

          try {
            const response = await fetch(`/QuanLi/DeleteHouse?id=${id}`, {
              method: "DELETE",
            });
            const result = await response.json();

            if (result.success) {
              iziToast.success({
                title: "Thành công!",
                message: result.message,
                position: "topRight",
                timeout: 1500,
              });
              setTimeout(() => location.reload(), 1200);
            } else {
              iziToast.error({
                title: "Lỗi!",
                message: result.message,
                position: "topRight",
              });
            }
          } catch (error) {
            iziToast.error({
              title: "Lỗi!",
              message: "Không thể xóa nhà trọ!",
              position: "topRight",
            });
          }
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
}

function showUnauthorizedMessage() {
  iziToast.warning({
    title: "Cảnh báo!",
    message: "Bạn không đủ thẩm quyền để truy cập chức năng này.",
    position: "topRight",
  });
}
function viewDetail(id, type) {
  const url = `/Home/Detail?id=${id}&type=${type}`;
  fetch(url, { method: "GET" })
    .then((response) => {
      if (response.ok) {
        window.location.href = url;
      } else {
        iziToast.error({
          title: "Lỗi!",
          message: "Không thể tải chi tiết. Vui lòng thử lại.",
          position: "topRight",
        });
      }
    })
    .catch(() => {
      iziToast.error({
        title: "Lỗi!",
        message: "Đã xảy ra lỗi khi xử lý yêu cầu.",
        position: "topRight",
      });
    });
}

function editItem(id) {
  const url = `/QuanLi/EditHouse?id=${id}`;
  fetch(url, { method: "GET" })
    .then((response) => {
      if (response.ok) {
        window.location.href = url;
      } else {
        iziToast.error({
          title: "Lỗi!",
          message: "Không thể tải trang chỉnh sửa. Vui lòng thử lại.",
          position: "topRight",
        });
      }
    })
    .catch(() => {
      iziToast.error({
        title: "Lỗi!",
        message: "Đã xảy ra lỗi khi xử lý yêu cầu.",
        position: "topRight",
      });
    });
}

async function deleteHouse(id) {
  iziToast.show({
    class: "iziToast-custom",
    title: "Xác nhận xóa",
    message: "Bạn có chắc chắn muốn xóa? Dữ liệu này sẽ không thể khôi phục!",
    position: "center",
    timeout: false,
    close: false,
    overlay: true,
    displayMode: "once",
    drag: false,
    icon: "fa fa-warning",
    buttons: [
      [
        '<button><i class="fa fa-check"></i> Xóa ngay</button>',
        async function (instance, toast) {
          instance.hide({ transitionOut: "fadeOut" }, toast, "button");

          try {
            const response = await fetch(`/QuanLi/DeleteHouse?id=${id}`, {
              method: "DELETE",
            });
            const result = await response.json();

            if (result.success) {
              iziToast.success({
                title: "Thành công!",
                message: result.message,
                position: "topRight",
                timeout: 1500,
              });
              setTimeout(() => location.reload(), 1200);
            } else {
              iziToast.error({
                title: "Lỗi!",
                message: result.message,
                position: "topRight",
              });
            }
          } catch (error) {
            iziToast.error({
              title: "Lỗi!",
              message: "Không thể xóa nhà trọ!",
              position: "topRight",
            });
          }
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
}

function showUnauthorizedMessage() {
  iziToast.warning({
    title: "Cảnh báo!",
    message: "Bạn không đủ thẩm quyền để truy cập chức năng này.",
    position: "topRight",
  });
}
document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".btn-hide").forEach((button) => {
    button.addEventListener("click", function () {
      const houseId = this.dataset.id;

      iziToast.show({
        class: "iziToast-custom",
        title: "Xác nhận",
        message:
          "Bạn có chắc muốn ẩn bài này? Bài đăng sẽ không hiển thị công khai!",
        position: "center",
        timeout: false,
        close: false,
        overlay: true,
        displayMode: "once",
        drag: false,
        icon: "fa fa-eye-slash",
        buttons: [
          [
            '<button><i class="fa fa-check"></i> Ẩn ngay</button>',
            function (instance, toast) {
              instance.hide({ transitionOut: "fadeOut" }, toast, "button");

              fetch("/QuanLi/HideHouse", {
                method: "POST",
                headers: {
                  "Content-Type": "application/x-www-form-urlencoded",
                },
                body: new URLSearchParams({ id: houseId }),
              })
                .then((response) => response.json())
                .then((data) => {
                  if (data.success) {
                    iziToast.success({
                      title: "Thành công!",
                      message: data.message,
                      position: "topRight",
                      timeout: 1000,
                    });
                    setTimeout(() => location.reload(), 1000);
                  } else {
                    iziToast.error({
                      title: "Lỗi!",
                      message: data.message,
                      position: "topRight",
                    });
                  }
                })
                .catch(() => {
                  iziToast.error({
                    title: "Lỗi!",
                    message: "Không thể thực hiện yêu cầu!",
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
  });

  document.querySelectorAll(".btn-show").forEach((button) => {
    button.addEventListener("click", function () {
      const houseId = this.dataset.id;

      iziToast.show({
        class: "iziToast-custom",
        title: "Xác nhận",
        message:
          "Bạn có chắc muốn hiện bài này? Bài đăng sẽ hiển thị công khai trở lại!",
        position: "center",
        timeout: false,
        close: false,
        overlay: true,
        displayMode: "once",
        drag: false,
        icon: "fa fa-eye",
        buttons: [
          [
            '<button><i class="fa fa-check"></i> Hiện ngay</button>',
            function (instance, toast) {
              instance.hide({ transitionOut: "fadeOut" }, toast, "button");

              fetch("/QuanLi/ShowHouse", {
                method: "POST",
                headers: {
                  "Content-Type": "application/x-www-form-urlencoded",
                },
                body: new URLSearchParams({ id: houseId }), // Gửi dưới dạng form-data
                })
                .then((response) => response.json())
                .then((data) => {
                  if (data.success) {
                    iziToast.success({
                      title: "Thành công!",
                      message: data.message,
                      position: "topRight",
                      timeout: 1000,
                    });
                    setTimeout(() => location.reload(), 1000);
                  } else {
                    iziToast.error({
                      title: "Lỗi!",
                      message: data.message,
                      position: "topRight",
                    });
                  }
                })
                .catch(() => {
                  iziToast.error({
                    title: "Lỗi!",
                    message: "Không thể thực hiện yêu cầu!",
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
  });
});

