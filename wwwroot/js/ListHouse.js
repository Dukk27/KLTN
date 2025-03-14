$(document).ready(function () {
    // Lưu trữ các hàm giao tiếp giữa iframe và trang chính
    window.closeModalFromIframe = function () {
        $('#createHouseModal').modal('hide');
    };

    // Xử lý sự kiện khi modal đóng
    $('#createHouseModal').on('hidden.bs.modal', function () {
        $('#createHouseIframe').attr('src', '');
        $('#createHouseIframe').hide();
        $('#loadingSpinner').show();
    });

    $("#showCreateHouseForm").click(function () {
        const isAuthenticated = $("#userAuthenticated").val().toLowerCase() === "true";

        if (!isAuthenticated) {
            alert('Bạn chưa đăng nhập. Vui lòng đăng nhập để thực hiện chức năng này.');
            return;
        }

        $("#createHouseModal").modal('show');
        $('#loadingSpinner').show();
        $('#createHouseIframe').hide();

        var iframeSrc = '/House/CreatePartial';
        $('#createHouseIframe').attr('src', iframeSrc);

        $('#createHouseIframe').on('load', function () {
            $('#loadingSpinner').hide();
            $('#createHouseIframe').show();

            try {
                var iframeHeight = $('#createHouseIframe').contents().find('body').height();
                $('#createHouseIframe').height(iframeHeight + 50);
            } catch (e) {
                console.error("Không thể điều chỉnh chiều cao iframe:", e);
                $('#createHouseIframe').height(600);
            }

            try {
                $('#createHouseIframe')[0].contentWindow.closeModal = window.closeModalFromIframe;
            } catch (e) {
                console.error("Không thể truyền hàm closeModal vào iframe:", e);
            }
        });
    });

    $("#closeModalBtn, #closeModalFooterBtn").click(function () {
        $('#createHouseModal').modal('hide');
    });
});

function goToDetail(houseId) {
    const detailUrl = `/Home/Detail?id=${houseId}`;
    fetch(detailUrl, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (response.ok) {
                window.location.href = detailUrl;
            } else {
                alert('Không tìm thấy trang chi tiết cho nhà trọ này.');
            }
        })
        .catch(error => {
            console.error('Có lỗi xảy ra:', error);
            alert('Đã xảy ra lỗi khi chuyển đến trang chi tiết.');
        });
}

document.addEventListener('DOMContentLoaded', function () {
    const filterModal = document.getElementById('filterModal');
    if (filterModal) {
        filterModal.addEventListener('show.bs.modal', function () {
            const form = filterModal.querySelector('form');
            if (form) {
                form.reset();
            }
        });
    }
});
