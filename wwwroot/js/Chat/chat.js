document.addEventListener("DOMContentLoaded", function () {
    var currentUserId = document.getElementById("currentUserId").value;
    var receiverId = document.getElementById("receiverId").value;

    console.log("[DEBUG] UserID:", currentUserId, "ReceiverID:", receiverId);

    // Kiểm tra nếu đã có kết nối SignalR thì không tạo mới
    if (window.chatConnection && window.chatConnection.state === "Connected") {
        console.log("[DEBUG] SignalR đã kết nối, không tạo lại.");
        return;
    }

    // Tạo kết nối SignalR nếu chưa có
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Lưu connection vào biến toàn cục để tránh tạo lại
    window.chatConnection = connection;

    // Xóa các sự kiện cũ trước khi đăng ký mới
    connection.off("ReceiveMessage");

    connection.on("ReceiveMessage", function (senderId, senderName, message, timestamp) {
        console.log(`[DEBUG] Nhận tin nhắn từ ${senderName} (${senderId}): ${message} lúc ${timestamp}`);
        if (senderId != currentUserId) {
            iziToast.info({
                title: `Tin nhắn từ ${senderName}`,
                message: message,
                position: 'topRight',
                timeout: 5000,
                transitionIn: 'fadeIn',
                transitionOut: 'fadeOut',
            });
        }
        displayMessage(senderId, senderName, message, timestamp);
    });

    function displayMessage(senderId, senderName, message, timestamp) {
        var messagesDiv = document.getElementById("messages");

        console.log("[DEBUG] Raw timestamp nhận được:", timestamp);

        // Chuyển timestamp thành định dạng chuẩn
        var time = new Date(timestamp);
        if (isNaN(time.getTime())) { // Kiểm tra timestamp có hợp lệ không
            console.error("[ERROR] Lỗi khi parse timestamp:", timestamp);
            time = new Date(); // Nếu lỗi, dùng thời gian hiện tại
        }

        var formattedTime = time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        // Tạo container tin nhắn
        var msgContainer = document.createElement("div");
        msgContainer.classList.add("message-container", senderId == currentUserId ? "sent" : "received");

        // Tên người gửi
        var senderSpan = document.createElement("span");
        senderSpan.classList.add("sender-name");
        senderSpan.innerText = senderId == currentUserId ? "Bạn" : senderName;

        // Nội dung tin nhắn
        var msgDiv = document.createElement("div");
        msgDiv.classList.add("message");
        console.log("[DEBUG] Nội dung tin nhắn hiển thị:", message);
        msgDiv.innerText = message; // Chỉ hiển thị nội dung tin nhắn

        // Thời gian gửi tin
        var timeSpan = document.createElement("span");
        timeSpan.classList.add("message-time");
        timeSpan.innerText = formattedTime;

        // Gán vào container (thứ tự: tên - tin nhắn - thời gian)
        msgContainer.appendChild(senderSpan);  // Thêm Tên trước
        msgContainer.appendChild(msgDiv);      // Thêm Nội dung
        msgContainer.appendChild(timeSpan);    // Thêm Thời gian sau

        // Thêm vào giao diện
        messagesDiv.appendChild(msgContainer);

        // Cuộn xuống cuối
        messagesDiv.scrollTop = messagesDiv.scrollHeight;

        console.log("[DEBUG] Tin nhắn hiển thị:", senderName, message, formattedTime);
    }

    // Kết nối SignalR
    connection.start().then(function () {
        console.log("[DEBUG] Kết nối SignalR thành công!");
    }).catch(function (err) {
        console.error("[ERROR] Không thể kết nối SignalR:", err.toString());
    });

    window.sendMessage = function () {
        var message = document.getElementById("messageInput").value.trim();
        if (!message) return;
        console.log("[DEBUG] Đang gửi tin nhắn:", message);

        connection.invoke("SendMessage", parseInt(currentUserId), parseInt(receiverId), message)
            .then(() => {
                console.log("[DEBUG] Tin nhắn gửi thành công!");
                document.getElementById("messageInput").value = ""; // Xóa nội dung sau khi gửi
            })
            .catch(err => console.error("[ERROR] Gửi tin nhắn thất bại:", err.toString()));
    };

    document.getElementById("messageInput").addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            if (!event.shiftKey) {  // Nếu chỉ nhấn Enter (không giữ Shift)
                event.preventDefault(); // Ngăn không cho Enter xuống dòng
                sendMessage(); // Gửi tin nhắn
            }
        }
    });
});