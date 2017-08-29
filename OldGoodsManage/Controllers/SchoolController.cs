using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Models;
using OldGoodsManage.Helper;

namespace OldGoodsManage.Controllers
{
    public class SchoolController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        /// <summary>
        /// 显示所有的学校记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowSchoolInfo()
        {
            return View(db.t_School.ToList());
        }

        /// <summary>
        /// 新建学校记录
        /// </summary>
        /// <param name="name">传入的学校名称</param>
        /// <returns></returns>
        public ActionResult Create(string name = "")
        {
            if (CheckAuthorize.IsChecked(CheckAuthorize.GetFunctionName(Convert.ToInt64(Session["UserID"])), FunctionName.SchoolManage) == false)
            {
                return Content("<script>alert('权限不足');history.back();</script>");
            }
            

            string javascript = "";          //表示返回的脚本内容
            try
            {
                //查找数据库中是否已经存在有相同名称的学校记录，如果有则提示插入失败并返回当前页面
                t_School ExistSchool = db.t_School.Single(t => t.schoolName == name);
                javascript = "<script>alert('已存在该学校记录！插入失败！');location.href='/School/ShowSchoolInfo'</script>";
                return Content(javascript);

            }
            catch
            {
                //新建一个学校记录
                t_School t_school = new t_School();
                t_school.schoolName = name;

                //将新建学校记录存入数据库中，并返回脚本内容
                if (ModelState.IsValid)
                {
                    db.t_School.AddObject(t_school);
                    db.SaveChanges();
                    javascript = "<script>alert('新建成功！');location.href='/School/ShowSchoolInfo'</script>";
                    return Content(javascript);
                }
            }
            return View("ShowSchoolInfo");

        }

        /************************************************************************
        函数名称： Edit
        函数作用：编辑学校名称
        传入参数：id：传入学校的SchoolID    name：传入修改的学校的名称
        返回参数：无
        修改记录： 作者          时间         原因 
        *          王俊伟        2015.1.28   create
       * **********************************************************************/
        public ActionResult Edit(long id = 0, string name = "")
        {
            string javascript = "";     //表示返回的脚本内容
            try
            {
                //查找数据库中是否已经存在相同的学校记录，如果有则提示插入失败并返回当前页面
                t_School ExistSchool = db.t_School.Single(t => (t.schoolName == name));
                javascript = "<script> alert('已存在该学校记录！编辑失败！'); window.location.href='/School/ShowSchoolInfo' </script>";
                return Content(javascript);
            }
            catch
            {
                //查找要编辑的学校记录，并进入数据库中修改对应的学校记录
                t_School t_school = db.t_School.Single(t => t.schoolID == id);
                t_school.schoolName = name;
                if (ModelState.IsValid)
                {
                    db.ObjectStateManager.ChangeObjectState(t_school, EntityState.Modified);
                    db.SaveChanges();
                    javascript = "<script> alert('编辑成功！'); window.location.href='/School/ShowSchoolInfo' </script>";
                    return Content(javascript);
                }
            }
            return View("ShowSchoolInfo");
        }

        /// <summary>
        /// 删除学校记录
        /// </summary>
        /// <param name="id">传入对应的学校记录的SchoolID</param>
        /// <returns></returns>
        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            string javascript = "";     //表示返回的脚本内容

            //查询数据库中的学校记录并删除
            t_School t_school = db.t_School.Single(t => t.schoolID == id);
            db.t_School.DeleteObject(t_school);
            db.SaveChanges();
            javascript = "<script> alert('删除成功！'); window.location.href='/School/ShowSchoolInfo/' </script>";
            return Content(javascript);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}