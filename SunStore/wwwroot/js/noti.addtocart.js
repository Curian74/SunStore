$(document).ready(function () {

    $(".atc").click(function () {

        $.ajax({
            url: "/Cart/AddToCart",
            data: {
                id: $(this).data("id"),
                quantity: $(".qq").val()
            },
            success: function (response) {

                var cq = 1;
                if (!response.exist) {
                    var temp = $(".cart_quantity").text();
                    if (temp == null || temp.length == 0 || temp == "0") {
                        temp = "0";
                    }
                    cq = cq + (isNaN(parseInt(temp)) ? 0 : parseInt(temp));
                    console.log(cq);
                    $(".cart_quantity").text(cq);
                    $("#c_quantity").css("display", "block");
                };

                const Toast = Swal.mixin({
                    toast: true,
                    position: "bottom-end",
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.onmouseenter = Swal.stopTimer;
                        toast.onmouseleave = Swal.resumeTimer;
                    }
                });
                Toast.fire({
                    icon: "success",
                    title: "Thêm vào giỏ hàng thành công"
                });

                // Swal.fire({
                //     position: "top-end",
                //     icon: "success",
                //     title: "Thêm vào giỏ hàng thành công",
                //     showConfirmButton: false,
                //     timer: 1500
                // });
            },
            error: function (xhr, status, error) {

                Swal.fire({
                    icon: "error",
                    title: "Ôi...",
                    text: "Đã có lỗi xảy ra!"
                });
            }
        });
    });
});
