function ShowImg(src) {
    //当鼠标经过小图时显示大图
    var img = document.getElementById("BigImage");
    img.src = src;
}

function GetSrc() {
    //点击大图时在新的页面显示大图
    var img = document.getElementById("BigImage");
    var imgSrc = img.src;
    document.getElementById("getImgSrc").href = imgSrc;
}

function AnswerCustomer(text, id) {
    //将回复人的名字显示在回复编辑框里面
    var textcomment = document.getElementById("commentContent");
    var btnanswer = document.getElementById("btncomment");
    var inputID = document.getElementById("commentID");
    var commentator = document.getElementById("commentator");
    commentator.value = text;
    textcomment.value = "回复" + text + ":";
    btnanswer.value = "回复";
    inputID.value = id;
}

function OnFocus() { 
    //删除回复编辑框里面的初始内容
    var commentator = document.getElementById("commentator");
    var text = commentator.value;
    var textcomment = document.getElementById("commentContent");
    var newtext = "回复" + text + ":";
    if (textcomment.value == newtext) {
        textcomment.value = "";
    }
}

function ShowDivHidden() {
    //显示举报编辑框
    var div = document.getElementById("divHidden");
    div.style.display = "block";
}

function HiddenDiv() {
    //隐藏举报编辑框
    var div = document.getElementById("divHidden");
    div.style.display = "none";
    var text = document.getElementById("textComplain");
    text.value = "";
}

function CheckedIsNull() {
   //检查评论和回复是否为null
    var text = document.getElementById("commentContent");
    if (text.value==null||text.value=="") {
        alert("请填入评论内容。");
        window.event.returnValue = false;
    }
}

function CheckComplain() {
   //检查举报内容是否为null
    var text = document.getElementById("textComplain");
    if (text.value == null || text.value == "") {
        alert("请填入举报内容。");
        window.event.returnValue = false;
    }
}

function showDivFriends() {
    var div = document.getElementById("divFriends");
    div.style.display = "block";
}
function hideFriends() {
    var div = document.getElementById("divFriends");
    div.style.display = "none";
}
  