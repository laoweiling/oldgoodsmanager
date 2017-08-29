using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Filters;
using OldGoodsManage.Models;

namespace OldGoodsManage.Controllers
{
    public class MessageController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();


        //后台用户发送消息页面
        public ActionResult UserSendMsg()
        {
            List<string> usersName = (from t_user in db.t_User
                                      select t_user.userName).ToList();
            ViewBag.usersName = usersName; 
            return View();
        }

        //后台用户提交发送的信息
        [HttpPost]
        public ActionResult UserSendMsg(FormCollection form)
        {  
            t_Message t_message = new t_Message();
            //收信人名称
            string receiverName = form["receiverName"];
            //发送人名称
            t_message.senderID = Convert.ToInt64(Session["UserID"]);
            t_message.message = form["txtMessage"];
            t_message.messageTime = DateTime.Now;
            //消息类型，0代表后台用户之间发送信息
            t_message.type = 0;
            
            t_User user= db.t_User.Single(c => c.userName == receiverName);
            t_message.receriverID = user.userID;
  
            if (ModelState.IsValid)
            {
                db.t_Message.AddObject(t_message);
                db.SaveChanges();
                return Content(string.Format("<script>alert('消息发送成功!');location.href='{0}';</script>", Url.Action("UserSendMsg", "Message")), "text/html");
            }

            return Content(string.Format("<script>alert('消息发送失败!');location.href={0};</script>",Url.Action("UserSendMsg","Message")),"text/html");
        }

        //显示当前用户收到的信息
        public ActionResult UserReceiveMsg()
        {
            long receiverId = Convert.ToInt64(Session["UserID"]);
            //获取当前用户收到的所有消息
            List<t_Message> messageList = (from t_message in db.t_Message
                                           where t_message.receriverID == receiverId
                                           select t_message).ToList();

            //通过userId来获取List
            Dictionary<long, t_User> dicSenders = new Dictionary<long, t_User>();
            foreach (t_Message message in messageList)
            {
                t_User user = db.t_User.Single(u => u.userID == message.senderID);
                dicSenders[message.messageID] = user;
            }
            ViewBag.messageList = messageList;
            ViewBag.dicSenders = dicSenders;
            return View();
        }

        //显示当前用户已经发送的所有消息
        [UserFilter]
        public ActionResult MessageList()
        {
            //查询当前用户所有已发送的消息
            long senderID = Convert.ToInt64(Session["UserID"]);
            List<t_Message> listMessage = new List<t_Message>();
            listMessage = (from message in db.t_Message
                           where message.senderID == senderID
                           select message).ToList();

            //将发送人放入字典以方便页面显示
            Dictionary<long, string> dictSenderName = new Dictionary<long, string>();
            t_User user = null;
            foreach (t_Message message in listMessage)
            {
                user = db.t_User.Single(u => u.userID == message.senderID);
                dictSenderName[message.senderID] = user.userName;
            }

            ViewBag.listMessage = listMessage;
            ViewBag.dictSenderName = dictSenderName;

            return View();
        }

        /************************************************************************
       函数名称： DeleteMessage
       函数作用：删除消息
       传入参数：页面传入的表单form
       返回参数：无
       修改记录： 作者          时间         原因 
       *          王俊伟        2015.9.10   create
      * **********************************************************************/
        [HttpPost]
        public ActionResult DeleteMessage(FormCollection form)
        {
            //获取前台用户已打勾的消息
            string deleteMessage = form["ckbMessage"];
            string[] temp = deleteMessage.Split(',');
            long messageId;
            try
            {
                //批量删除页面打勾的消息
                foreach (string messageIdStr in temp)
                {
                    messageId = Convert.ToInt64(messageIdStr);
                    t_Message message = db.t_Message.Single(m => m.messageID == messageId);
                    db.DeleteObject(message);
                    db.SaveChanges();
                }
                return Content(string.Format("<script>alert('删除成功！');location.href='{0}';</script>", Url.Action("UserReceiveMsg", "Message")), "text/html");
            }
            catch (Exception)
            {
                return Content(string.Format("<script>alert('删除失败！');location.href='{0}';</script>", Url.Action("UserReceiveMsg", "Message")));
            }

        }




        ////用户之间发送消息(暂时没有这个功能)
        //public ActionResult CustmSendMsg()
        //{
        //    return View();
        //}

        ////前台用户之间发送消息(暂时没有这个功能)
        //[HttpPost]
        //public ActionResult CustmSendMsg(t_Message t_message,FormCollection form)
        //{
        //    if (Session["CustomerLoginName"] == null)
        //    {
        //        // return Content("<script>alert('亲，你还有没有登录，请先登录再执行操作。');location.href=''</script>", "text/html");
        //        return Content(String.Format("<script>alert('亲，你还有没有登录，请先登录再执行操作。');location.href='{0}'</script>", Url.Action("Login", "Customer")), "text/html");

        //    }

        //    string type = form["radioType"];
        //    string receiverName = form["txtReceiverName"];
        //    t_message.message = form["txtMessage"];
        //    t_message.senderID = Convert.ToInt64(Session["CustomerID"]);
        //    t_message.messageTime = DateTime.Now;
            
            
        //    if (type == "0")
        //    {
        //        t_message.type = 0;
        //        t_Customer customer = db.t_Customer.Single(c => c.customerName == receiverName);
        //        t_message.receriverID = customer.customerID;
        //    }
        //    else if (type == "1")
        //    {
        //        t_message.type = 1;
        //        t_User user = db.t_User.Single(u => u.userName == receiverName);
        //        t_message.receriverID = user.userID;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        db.t_Message.AddObject(t_message);
        //        db.SaveChanges();

        //    }

        //    return Content(String.Format("<script>alert('消息发送成功');location.href= '{0}';</script>",Url.Action("UserList","User")),"text/html");
        //}


 

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}