// const goongApiKey = "RJ9d6oO2TLx8s4Q8riMgne1hI905XhC89HxJ7fIy";

// Xử lý xem trước hình ảnh
document.addEventListener("DOMContentLoaded", function () {
  const imageFileInput = document.getElementById("imageFile");
  const imageFilesInput = document.getElementById("imageFiles");
  const imagePreview = document.getElementById("imagePreview");
  const imageError = document.getElementById("imageError");
  const addressInput = document.getElementById("fullAddress");
  const addressSuggestions = document.getElementById("addressSuggestions");

  if (imageFileInput) {
    imageFileInput.addEventListener("change", function () {
      const file = this.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
          imagePreview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 200px;" />`;
        };
        reader.readAsDataURL(file);
      }
    });
  }

  if (imageFilesInput) {
    imageFilesInput.addEventListener("change", function () {
      const files = this.files;
      imagePreview.innerHTML = ""; // Xóa ảnh cũ

      if (files.length > 5) {
        imageError.textContent = "Chỉ được tải lên tối đa 5 ảnh.";
        this.value = ""; // Xóa file đã chọn
        return;
      } else {
        imageError.textContent = "";
      }

      Array.from(files).slice(0, 5).forEach((file) => {
        const reader = new FileReader();
        reader.onload = function (e) {
          const previewItem = document.createElement("div");
          previewItem.classList.add("preview-item");

          const img = document.createElement("img");
          img.src = e.target.result;
          img.alt = "Preview";

          const removeBtn = document.createElement("button");
          removeBtn.type = "button";
          removeBtn.classList.add("btn", "btn-danger", "btn-sm", "remove-image");
          removeBtn.innerHTML = '<i class="fa-solid fa-circle-xmark"></i>';
          removeBtn.addEventListener("click", function () {
            previewItem.remove();
            if (imagePreview.children.length === 0) {
              imageFilesInput.value = "";
            } 
          });

          previewItem.appendChild(img);
          previewItem.appendChild(removeBtn);
          imagePreview.appendChild(previewItem);
        };
        reader.readAsDataURL(file);
      });
    });
  }
});
