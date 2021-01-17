function IsValidString(value) {
    for (var i = 0; i < value.length; i++) {
        var CharCode = value[i].charCodeAt();
        if (!(value[i] == ',' || value[i] == '_' || value[i] == '+' ||
            CharCode > 47 && CharCode < 58 ||
            CharCode > 64 && CharCode < 91 ||
            CharCode > 96 && CharCode < 123 ||
            CharCode > 0x4E00 && CharCode < 0x9FA5)) {
            alert("排除內容和備註只能輸入中英數、逗點、底線、加號");
            return false;
        }
    }
    return true;
}
function CheckValue() {
    // 檢查目標網址
    var CrawlUrl = document.getElementById("CrawlUrl");
    var TargetUrlHead = "https://www.104.com.tw/jobs/search/";
    if (CrawlUrl.value.length < 1 || !CrawlUrl.value.startsWith(TargetUrlHead)) {
        alert("網址開頭必須是" + TargetUrlHead);
        return false;
    }
    // 檢查最高月薪是否大於等於最低月薪
    var MaxWage = document.getElementById("MaximumWage").value;
    var MinWage = document.getElementById("MinimumWage").value;
    if (parseInt(MaxWage) < parseInt(MinWage)) {
        alert("最高月薪必須大於或等於最低月薪");
        return false;
    }
    // 檢查想排除的關鍵字
    if (!IsValidString(document.getElementById("ExcludeWord").value)) {
        return false;
    }
    // 檢查想排除的公司
    if (!IsValidString(document.getElementById("IgnoreCompany").value)) {
        return false;
    }
    // 檢查備註
    if (!IsValidString(document.getElementById("Remarks").value)) {
        return false;
    }
    return true;
}
// 攔截最低月薪的輸入
$('#MinimumWage').on("input", function (e) {
    var value = document.getElementById("MinimumWage").value;
    // 將輸入修正成只有數字
    var InputModify = "";
    for (var i = 0; i < value.length; i++) {
        if (value[i].charCodeAt() > 47 && value[i].charCodeAt() < 58) {
            InputModify += value[i];
        }
    }
    document.getElementById("MinimumWage").value = InputModify;
    // 限制輸入長度
    if (value.length > 6) {
        document.getElementById("MinimumWage").value = value.substr(0, 6);
    }
});
// 攔截最高月薪的輸入
$('#MaximumWage').on("input", function (e) {
    var value = document.getElementById("MaximumWage").value;
    // 將輸入修正成只有數字
    var InputModify = "";
    for (var i = 0; i < value.length; i++) {
        if (value[i].charCodeAt() > 47 && value[i].charCodeAt() < 58) {
            InputModify += value[i];
        }
    }
    document.getElementById("MaximumWage").value = InputModify;
    // 限制輸入長度
    if (value.length > 6) {
        document.getElementById("MaximumWage").value = value.substr(0, 6);
    }
});