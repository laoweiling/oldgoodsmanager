//add by lulu  主要是将图片显示等相关javascript的代码集中起来，涉及到的页面有addGoods, editGoods

    window.imagecount = 0;
    //添加图片的响应函数
    function addfile() {
        if (3 == window.imagecount) {
            alert("已经添加了3张图片!");
            return;
        }
        var table = $$("idPicList");
        for (i = 0; i < 3; i++) {
            tr = table.getElementsByTagName("tr")[i];
            if ("none" == tr.style.display) {
                tr.style.display = "inline";
                break;
            }
        }    
    }

    function isIe() {
        return ("ActiveXObject" in window);
    }

    //显示用户输入路径的图片
    function viewimage(imageId, picimageId) {
        var imagepath;
        var obj = document.getElementById(imageId);
        if (isIe()) {
            if (document.selection) {     //非IE11的版本
                obj.select();
                obj.blur();   //没放在框架内使用
                //getObj('idPicList').focus();   //放在框架内使用
                imagepath = document.selection.createRange().text;
            }
            else {
                obj.select();
                imagepath = obj.value;    //IE11的版本  
            }
        }
        else {
            imagepath = obj.value.substring(obj.selectionStart, obj.selectionEnd);
        }
       
        if (false == CheckPreview(imageId)) {
            return;
        }
        paths += imagepath + "|";
        //设置预览图片的路径，高度和宽度
        var objPic = this.document.getElementById(picimageId);
       // alert(imagepath);
        objPic.src = imagepath;
        //objPic.setAttribute("src", imagepath);
       // $(picimageId).attr("src", "C:\Users\Administrator\Desktop\图片\201303060907189518.jpg");  
       // alert(objPic.src);
        objPic.height = 100;
        objPic.width = 100;
        window.imagecount = window.imagecount + 1;
        
    }

    //检测输入图片的合法性
    var exts = "jpg|gif|bmp", paths = "|";
    function CheckPreview(imageId) {
        var file = document.getElementById(imageId);
        var value = file.value, check = true;
        if (!value) {
            check = false; alert("请先选择文件！");
        } else if (!RegExp("\.(?:" + exts + ")$$", "i").test(value)) {
            check = false; alert("只能上传以下类型：" + exts);
        } else if (paths.indexOf("|" + value + "|") >= 0) {
            check = false; alert("已经有相同文件！");
        }
        check || ResetFile(file);
        return check;
    }

    //移除按钮对应的响应函数，将图片设置为空，并把该行隐藏
    function RemoveImage(imageId,picimageId, rowCount) {
        var table = $$("idPicList");
        tr = table.getElementsByTagName("tr")[rowCount - 1];
        tr.style.display = "none";
        var picImage = this.document.getElementById(picimageId);
        picImage.src = "";
        picImage.height = 0;
        picImage.width = 0;
        window.imagecount = window.imagecount - 1;
        var file = this.document.getElementById(imageId);
       
        //从path中去除图片路径
        var imagePath = file.value + "|";
        paths = paths.replace(imagePath, "");

        //清空file控件
        ResetFile(file);
    }
    function ResetFile(file) {
        file.value = ""; //ff chrome safari
        if (file.value) {
            if ($$B.ie) {//ie
                with (file.parentNode.insertBefore(document.createElement('form'), file)) {
                    appendChild(file); reset(); removeNode(false);
                }
            } else {//opera
                file.type = "text"; file.type = "file";
            }
        }
    }

