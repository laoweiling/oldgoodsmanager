  function Write() {
        var text = document.getElementById("SearchText").value;
        if (text == "请输入要搜索的内容") {
            document.getElementById("SearchText").value = "";
        }
    }
    function Content() {
        var text = document.getElementById("SearchText").value;
        if (text == null || text == "") {
            document.getElementById("SearchText").value = "请输入要搜索的内容";
        }
        else {
            document.getElementById("Search").href = "/Goods/readGoods?searchName=" + text;
        }
    }


    function mouseClick() {

        var ddListSchool = document.getElementById("ddListSchool");
        var selectIndex = ddListSchool.selectedIndex;

        var selectValue = ddListSchool.options[selectIndex].value;
        var selectText = ddListSchool.options[selectIndex].text;
        var url = "/Goods/readGoods/?id=1&schoolId=" + selectValue;

        document.getElementById("labSchool").innerHTML = selectText;
        location.href = url;
    }

    function getSearch() {
        var searchText = document.getElementById("searchTxt").value;

    }

    function clearText() {
        document.getElementById("searchTxt").value = "";
    }

    function custLogOut() {
        var sel = document.getElementById("logOut");
        var index = sel.selectedIndex;

        if (sel.options[index].text == "退出") {
            location.href = "/Customer/LogOut";
        }
    }

 