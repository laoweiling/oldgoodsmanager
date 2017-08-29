function ShowDivHidden(senderID) {
    var div = document.getElementById("divHidden");
    var inputId = document.getElementById("messageId");
    inputId.value = senderID;
    div.style.display = "block";
}

function HiddenDiv() {
    var div = document.getElementById("divHidden");
    var text = document.getElementById("textReply");
    var inputId = document.getElementById("messageId");
    inputId.value = "";
    textReply.value = "";
    div.style.display = "none";
}
function CheckText() {
    var text = document.getElementById("textReply");
    if (text.value == null || text.value == "") {

        alert("请输入回复内容。");
        window.event.returnValue = false;
    }
}