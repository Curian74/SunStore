//const { pop } = require("../vendor/fontawesome-free/js/v4-shims");

$(document).ready(function () {
    $(".canc").click(function () {
        var id = $(this).data("id");
        Swal.fire({
            title: "Bạn có chắc chắn muốn huỷ?",
            text: "Bạn sẽ không thể hoàn tác!",
            icon: "warning",
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            cancelButtonText: "Không huỷ",
            confirmButtonText: "Huỷ đơn hàng"
        }).then((result) => {
            if (result.isConfirmed) {

                $.ajax({
                    url: "/Bills/Cancel",
                    method: "post",
                    data: { id: id },
                    success: function () {
                        Swal.fire({
                            title: "Thành công!",
                            text: "Đơn hàng đã được huỷ.",
                            confirmButtonText: "Đồng ý"
                        });
                        var cancelSelector = "#stt_" + id;
                        var cancSelector = "#canc_" + id;
                        console.log(cancelSelector);
                        $(cancelSelector).text("Đã huỷ");
                        $(cancSelector).css("display", "none");
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

    $(".received").click(function () {
        var id = $(this).data("id");
        Swal.fire({
            title: "Xác nhận là đã nhận hàng?",
            text: "Bạn sẽ không thể hoàn tác!",
            icon: "info",
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            cancelButtonText: "Hủy",
            confirmButtonText: "Xác nhận"
        }).then((result) => {
            if (result.isConfirmed) {

                $.ajax({
                    url: `https://localhost:7270/api/Orders/received/${id}`,
                    method: "post",
                    success: function () {
                        Swal.fire({
                            title: "Thành công!",
                            text: "Xác nhận nhận hàng thành công.",
                            confirmButtonText: "Đồng ý"
                        });
                        var cancelSelector = "#stt_" + id;
                        var cancSelector = "#received_" + id;
                        $(cancelSelector).text("Đã nhận hàng");
                        $(cancSelector).css("display", "none");
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

    $(".reason").click(function () {
        var id = $(this).data("id");
        $.ajax({
            url: "/Bills/GetCancelReason",
            data: { id: id },
            success: function (data) {
                Swal.fire({
                    title: "Lí do từ chối đơn hàng",
                    text: data.reason,
                    confirmButtonText: "Đồng ý"
                });
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Ôi...",
                    text: "Đã có lỗi xảy ra!"
                });
            }
        })
    });
});