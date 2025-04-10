function markAsRead(id) {
  fetch(`/Notification/MarkAsRead/${id}`, { method: "POST" })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        let listItem = document
          .querySelector(`button.mark-read[data-id='${id}']`)
          .closest("li");
        listItem.classList.remove("font-weight-bold");
        listItem.classList.add("text-muted");
        document.querySelector(`button.mark-read[data-id='${id}']`).remove();
      }
    });
}

function deleteNotification(id) {
  fetch(`/Notification/Delete/${id}`, { method: "POST" })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        document
          .querySelector(`button.delete[data-id='${id}']`)
          .closest("li")
          .remove();
      }
    });
}

function markAllAsRead() {
  fetch(`/Notification/MarkAllAsRead`, { method: "POST" })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        document.querySelectorAll(".notification-list li").forEach((item) => {
          item.classList.remove("font-weight-bold");
          item.classList.add("text-muted");
        });
        document
          .querySelectorAll(".mark-read")
          .forEach((button) => button.remove());
      }
    });
}

function deleteAllNotifications() {
  fetch(`/Notification/DeleteAll`, { method: "POST" })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        // Xóa danh sách thông báo
        document.querySelector(".notification-list").innerHTML = "<p class='text-center text-muted'>Không có thông báo nào.</p>";
                
        // Ẩn hai nút "Đánh dấu tất cả là đã đọc" và "Xóa tất cả"
        document.querySelector(".notification-actions").style.display = "none";
      }
    });
}
