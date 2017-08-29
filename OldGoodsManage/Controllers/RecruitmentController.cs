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
    public class RecruitmentController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        
        //get
        /************************************************************************
        函数名称： ShowRecruitment
        函数作用： 用于前台页面展示招聘信息
        传入参数：无
        返回参数：招聘实体的对象
        修改记录： 作者        时间        原因 
        *         李康志      2015.3.25    create
       * **********************************************************************/
        public ActionResult ShowRecruitment()
        {
            t_Recruitment recruitment = db.t_Recruitment.OrderByDescending(r => r.recruitmentID).FirstOrDefault();
            return View(recruitment);
        }

        //get
        /************************************************************************
        函数名称： addRecruitment
        函数作用： 后台管理员增加招聘信息           
        传入参数：无
        返回参数：无
        修改记录： 作者        时间        原因 
        *         李康志      2015.3.25    create
       * **********************************************************************/
        public ActionResult AddRecruitment()
        {
            return View();
        }

        /************************************************************************
       函数名称： addRecruitment
       函数作用： 将招聘信息存进数据库                
       传入参数：无
       返回参数：无
       修改记录： 作者        时间        原因 
       *         李康志      2015.3.25    create
      * **********************************************************************/
        [HttpPost]
        public ActionResult AddRecruitment(t_Recruitment model)
        {
            if (ModelState.IsValid)
            {
                db.t_Recruitment.AddObject(model);
                int num = db.SaveChanges();
                if (num > 0)
                {
                    return Content(String.Format("<script>alert('添加招聘信息成功');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
                }
            }
            return View();
        }
        /************************************************************************
        函数名称： RecruitmentList
        函数作用： 后台展示招聘信息                  
        传入参数：无
        返回参数：无
        修改记录： 作者        时间        原因 
        *         李康志      2015.3.25    create
       * **********************************************************************/
        //get
        public ActionResult RecruitmentList()
        {
            List<t_Recruitment> recruitmentList = new List<t_Recruitment>();
            recruitmentList = db.t_Recruitment.ToList();
            //如果招聘信息列表为null则提示系统没有任何招聘
            if (recruitmentList == null)
            {
                return Content(String.Format("<script>alert('目前系统没有任何招聘信息');location.href='{0}'</script>", Url.Action("AddRecruitment", "Recruitment")), "text/html");
            }
            //通过ViewBag动态类型对象传给view
            ViewBag.recruitmentList = recruitmentList;
            return View();
        }

        /************************************************************************
       函数名称： RecruitmentDetails
       函数作用： 显示招聘信息详情                    
       传入参数：招聘信息ID  
       返回参数：无
       修改记录： 作者        时间        原因 
       *         李康志      2015.3.25    create
      * **********************************************************************/
        public ActionResult RecruitmentDetails(long id)
        {
            t_Recruitment recruitment = db.t_Recruitment.Where(r => r.recruitmentID == id).FirstOrDefault();
            if (recruitment==null)
            {
                return Content(String.Format("<script>alert('没找到该招聘信息，请重试');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
            }
            return View(recruitment);
        }

        /************************************************************************
       函数名称： deleteRecruitment
       函数作用： 删除特定的招聘信息                    
       传入参数：招聘信息ID  
       返回参数：无
       修改记录： 作者        时间        原因 
       *         李康志      2015.3.25    create
      * **********************************************************************/
        public ActionResult DeleteRecruitment(long id)
        {
            t_Recruitment rectruitment = new t_Recruitment();
            rectruitment = db.t_Recruitment.Where(r => r.recruitmentID == id).FirstOrDefault();
            db.t_Recruitment.DeleteObject(rectruitment);
            int num = db.SaveChanges();
            if (num > 0)
            {
                return Content(String.Format("<script>alert('删除招聘信息成功！');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
            }
            else
            {
                return Content(String.Format("<script>alert('删除招聘信息失败，请重试！');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
            }
        }

        //get
        /************************************************************************
      函数名称： EditRecruitment
      函数作用： 编辑特定的招聘信息                      
      传入参数：招聘信息ID  
      返回参数：无
      修改记录： 作者        时间        原因 
      *         李康志      2015.3.25    create
     * **********************************************************************/
        public ActionResult EditRecruitment(long id)
        {
            t_Recruitment recruitment = db.t_Recruitment.Where(r => r.recruitmentID ==id).FirstOrDefault();
            if (recruitment!=null)
            {
                return View(recruitment);
            }
            return Content(String.Format("<script>alert('没有找到该招聘信息，请重试！');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
        }

        /************************************************************************
     函数名称： editRecruitment
     函数作用： 将更新后的招聘信息存进数据库                      
     传入参数：招聘信息ID  
     返回参数：无
     修改记录： 作者        时间        原因 
     *         李康志      2015.3.25    create
    * **********************************************************************/
        [HttpPost]
        public ActionResult EditRecruitment(t_Recruitment model)
        {
            if (ModelState.IsValid)
            {
                db.t_Recruitment.Attach(model);
                db.ObjectStateManager.ChangeObjectState(model,EntityState.Modified);
                int num=db.SaveChanges();
                if (num>=0)
                {
                    return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
                }
                return Content(String.Format("<script>alert('修改失败，请稍后再试！');location.href='{0}'</sctipt>", Url.Action("RecruitmentList", "Recruitment")), "text/html");
            }

            return View();
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}