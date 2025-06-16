function increaseQuantity() {
    var quantityInput = document.getElementById("quantity");
    var currentValue = parseInt(quantityInput.value);
    if (currentValue < maxQuantity) {
        quantityInput.value = currentValue + 1;
    }
}

function decreaseQuantity() {
    var quantityInput = document.getElementById("quantity");
    var currentValue = parseInt(quantityInput.value);
    if (currentValue > 1) {
        quantityInput.value = currentValue - 1;
    }
}

let quantityInput = document.getElementById("quantity");
quantityInput.addEventListener('input', () => {
    quantity = quantityInput.value;
    quan = parseInt(quantity);
    quan = isNaN(quan) ? 1 : quan;
    quan = (quan > maxQuantity) ? maxQuantity : quan;
    quantityInput.value = quan;
})