function openChat(conversationId) {
    window.location.href = "/Chat/Chat?conversationId=" + conversationId;
}

function deleteConversation(event, conversationId) {
    event.stopPropagation(); // Ngăn không mở chat khi ấn nút xóa

    iziToast.question({
        class: 'iziToast-custom',
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        title: 'Xác nhận',
        message: 'Bạn có chắc chắn muốn xóa cuộc trò chuyện này không?',
        position: 'center',
        buttons: [
            ['<button><b>Đồng ý</b></button>', function (instance, toast) {
                fetch("/Chat/DeleteConversation", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-Requested-With": "XMLHttpRequest"
                    },
                    body: `conversationId=${encodeURIComponent(conversationId)}`
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            iziToast.success({
                                title: 'Thành công',
                                message: 'Hội thoại đã được xóa!',
                                position: 'topRight',
                                timeout: 1000
                            });

                            // Xóa hội thoại khỏi danh sách ngay lập tức
                            event.target.closest(".chat-item").remove();
                        } else {
                            iziToast.error({
                                title: 'Lỗi',
                                message: data.message,
                                position: 'topRight'
                            });
                        }
                    })
                    .catch(error => {
                        console.error("Lỗi:", error);
                        iziToast.error({
                            title: 'Lỗi',
                            message: 'Không thể kết nối với máy chủ!',
                            position: 'topRight'
                        });
                    });

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }, true],

            ['<button>Hủy</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }]
        ]
    });
} 