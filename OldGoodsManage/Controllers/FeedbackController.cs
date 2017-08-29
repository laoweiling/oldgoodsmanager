using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Models;

namespace OldGoodsManage.Controllers
{
    public class FeedbackController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

       
        public ActionResult FeedbackList(FormCollection form)
        {
            string strSelect = form["select"];
            string strSubmit = form["submit"];
            List<Models.t_Feedback> list = null;
            int iStatus = 0;
            if (strSubmit == "查询" && strSelect != "")
            {
                if (strSelect == "已读")
                {
                    iStatus = 1;
                }
                list = (from u in db.t_Feedback where u.status == iStatus select u).ToList();
                ViewBag.DataList = list;
                return View();
            }

            list = (from u in db.t_Feedback select u).ToList();
            ViewBag.DataList = list;
            return View();
        }



        public ActionResult FeedbackDetails(long id = 0)
        {
            t_Feedback t_feedback = db.t_Feedback.Single(t => t.feedbackID == id);
            //t_feedback.status = 1;//查看后，状态变为已读了
            if (t_feedback == null)
            {
                return HttpNotFound();
            }
            if (t_feedback.status == 0)
            {
                t_feedback.status = 1;
                db.SaveChanges();

            }
            return View(t_feedback);
        }
        [HttpPost]
        public ActionResult FeedbackDetails()
        {
            return RedirectToAction("FeedbackList", "Feedback");
        }
       
        public ActionResult AddFeedback()
        {
            return View();
        }
         [HttpPost]
        public ActionResult AddFeedback(t_Feedback t_feedback, FormCollection form)
        {
            string strTel = form["Tel"];
            string strQQ = form["QQ"];
            if (strTel == "" && strQQ == "")
            {
                ModelState.AddModelError("contactWay", "联系方式不能为空，至少要输入一种联系方式！");
            }
            if (ModelState.IsValid)
            {
                if (strTel != "")
                {
                    t_feedback.contactWay = strTel;
                }
                else
                {
                    t_feedback.contactWay = strQQ;
                }
                t_feedback.status = 0;//0是未读。1是已读
                t_feedback.feedbackTime = DateTime.Now;
                db.t_Feedback.AddObject(t_feedback);
                int iNum = db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('提交成功！')</script>"));
                }
                return Content(String.Format("<script>alert('提交不成功，请稍后再操作！')</script>"));
            }

            return View(t_feedback);
        }



        public ActionResult DeleteFeedback(long id = 0)
        {
            t_Feedback t_feedback = db.t_Feedback.Single(t => t.feedbackID == id);
            if (t_feedback.status == 0)
            {
                ViewData["status"] = "未读";
            }
            else
            {
                ViewData["status"] = "已读";
            }
            if (t_feedback == null)
            {
                return HttpNotFound();
            }
            return View(t_feedback);
        }

        [HttpPost, ActionName("deleteFeedback")]
        public ActionResult DeleteFeedbackConfirmed(long id)
        {
            t_Feedback t_feedback = db.t_Feedback.Single(t => t.feedbackID == id);
            db.t_Feedback.DeleteObject(t_feedback);
            int iNum=db.SaveChanges();
            if (iNum > 0)
            {
                return Content(String.Format("<script>alert('删除成功！');location.href='{0}'</script>", Url.Action("FeedbackList", "Feedback")), "text/html");
            }
            else 
            {
                return Content(String.Format("<script>alert('删除不成功，请稍后再操作！')</script>"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}