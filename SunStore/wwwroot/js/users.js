// Xử lý nút "canc" để chặn người dùng
$(document).on("click", ".ban", function () {
    console.log("ban");
    var id = $(this).data("id");

    Swal.fire({
        title: "Xác nhận chặn người dùng?",
        showCancelButton: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: "Users/BanUser",
                data: {
                    id: id
                },
                success: function () {
                    var cancelSelector = "#stt_" + id;
                    var banSelector = "#unban_" + id;
                    console.log(cancelSelector);

                    $(banSelector).text("Chặn người dùng").removeClass("ban").addClass("unban");

                    Swal.fire({
                        title: "Thành công!",
                        text: "Người dùng đã bị chặn.",
                        confirmButtonText: "Đồng ý"
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.reload();
                        }
                    });
                },
                error: function () {
                    Swal.fire({
                        icon: "error",
                        title: "Ôi...",
                        text: "Đã có lỗi xảy ra!"
                    });
                }
            });
        }
    });
});

// Xử lý nút "Bị chặn" để kích hoạt lại người dùng
$(document).on("click", ".unban", function () {
    console.log("unban");
    var id = $(this).data("id");

    Swal.fire({
        title: "Xác nhận kích hoạt lại người dùng?",
        showCancelButton: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: "Users/UnBanUser",
                data: {
                    id: id
                },
                success: function () {
                    var cancelSelector = "#stt_" + id;
                    var unbanSelector = "#unban_" + id;
                    console.log(cancelSelector);

                    $(unbanSelector).text("Kích hoạt người dùng").removeClass("unban").addClass("ban");

                    Swal.fire({
                        title: "Thành công!",
                        text: "Người dùng đã được kích hoạt lại.",
                        confirmButtonText: "Đồng ý"
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.reload();
                        }
                    });
                },
                error: function () {
                    Swal.fire({
                        icon: "error",
                        title: "Ôi...",
                        text: "Đã có lỗi xảy ra!"
                    });
                }
            });
        }
    });
});
