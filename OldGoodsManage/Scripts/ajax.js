function ajax(URL, type, data, onsuccess) {
    var xmlHttpRqt = window.XMLHttpRequest ? new XMLHttpRequest : new ActiveXObject("Microsoft.ajax");
    if (xmlHttpRqt) {
        xmlHttpRqt.open(type, URL + data, true);
        xmlHttpRqt.onreadystatechange = function () {
            if (xmlHttpRqt.readyState == 4) {
                if (xmlHttpRqt.state == 200) { 
                    onsuccess(xmlHttpRqt.responseText);
                }
            }
        };
    }
}