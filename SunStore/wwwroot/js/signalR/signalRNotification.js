document.addEventListener("DOMContentLoaded", function () {
    const badge = document.querySelector('.notification-badge');
    const savedCount = parseInt(localStorage.getItem("notificationCount")) || 0;

    if (badge) {
        if (savedCount > 0) {
            badge.textContent = savedCount;
            badge.style.display = 'inline';
        }
        else {
            badge.style.display = 'none';
        }
    }

    // Remove badge when clicks on the bell.
    document.getElementById('notificationDropdown').addEventListener('click', function () {
        localStorage.setItem("notificationCount", 0);
        if (badge) {
            badge.textContent = "0";
            badge.style.display = 'none';
        }
    });
});

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7270/notificationHub")
    .build();

connection.on("ReceiveNotification", function (message) {
    const badge = document.querySelector('.notification-badge');
    let count = parseInt(localStorage.getItem("notificationCount")) || 0;
    count++;
    localStorage.setItem("notificationCount", count);

    if (badge) {
        badge.textContent = count;
        badge.style.display = 'inline';
    }

    const Toast = Swal.mixin({
        toast: true,
        position: "bottom-start",
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    });
    Toast.fire({
        icon: "info",
        title: "Có đơn hàng mới"
    });

});

connection.start().catch(function (err) {
    return console.error("SignalR error: ", err.toString());
});