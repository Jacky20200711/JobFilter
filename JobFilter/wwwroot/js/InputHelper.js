function AddGmail() {
    var InputOfEmail = document.getElementById("InputOfEmail");
    // 若 Input 沒有 gmail 則幫忙補上
    if (InputOfEmail.value.indexOf("@@gmail") < 0) {
        InputOfEmail.value = InputOfEmail.value + "@@gmail.com";
    }
}
function AddYahoo() {
    var InputOfEmail = document.getElementById("InputOfEmail");
    // 若 Input 沒有 yahoo 則幫忙補上
    if (InputOfEmail.value.indexOf("@@yahoo") < 0) {
        InputOfEmail.value = InputOfEmail.value + "@@yahoo.com.tw";
    }
}