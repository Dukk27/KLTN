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
