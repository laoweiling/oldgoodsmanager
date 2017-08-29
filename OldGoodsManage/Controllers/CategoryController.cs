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
    public class CategoryController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        /*****************************************************************************
       * 函数名：readCategory
       * 功能：显示首页的商品类目信息
       * 作者：王俊伟
       * 入参：无
       * 输出：所有的商品类目
       * 修改记录：2015-01-28
       * ***************************************************************************/
        public ActionResult ReadCategory()
        {
            //从数据库中读取所有的一级目录
            var firTitle = from fir in db.t_Category
                               where fir.parentID == 0
                               select fir;
            ViewBag.firTitle = firTitle.ToList();

            //从数据库中读取所有的二级目录
            var secTitle = from sec in db.t_Category
                               where sec.parentID != 0
                               select sec;
            ViewBag.secTitle = secTitle.ToList();
            return View(db.t_Category.ToList());
        }

        /*****************************************************************************
       * 函数名：ShowFirTitle
       * 功能：用于后台显示商品一级类目
       * 作者：王俊伟
       * 入参：无
       * 输出：商品所有一级类目
       * 修改记录：2015-01-28
       * ***************************************************************************/
        public ActionResult ShowFirTitle()
        {
            //找出所有的一级目录
            var firTitle = from t in db.t_Category
                        where t.parentID == 0
                        select t;
            return View(firTitle);
        }

        /// <summary>
        /// 显示一级目录对应的所有的二级目录
        /// </summary>
        /// <param name="id">传入一级目录的CategoryID作为二级目录的parentID</param>
        /// <param name="name">传入一级目录的名称</param>
        /// <returns></returns>
        public ActionResult ShowSecTitle(long id = 0, string name = "")
        {
            Session["ParentID"] = id;       //Session的内容可供其它action和view使用
            Session["ParentName"] = name;
            
            //通过一级目录的CategoryID来查询对应所有的二级目录
            var findsecTitle = from t in db.t_Category
                               where t.parentID == id
                               select t;
            return View(findsecTitle);
        }

        /************************************************************************
        函数名称： CreateFirTitle
        函数作用：新建一级目录
        传入参数：一级目录的名称name
        返回参数：无
        修改记录： 作者          时间         原因 
        *          王俊伟        2015.1.28   create
       * **********************************************************************/
        public ActionResult CreateFirTitle(string name = "")
        {
            string javascript = "";                     //表示返回的脚本内容
            try
            {
                //查找数据库中是否已经存在有相同名称的一级目录，如果有则提示插入失败并返回当前页面
                 t_Category existCategory = db.t_Category.Single(t => (t.categoryName == name) && (t.parentID == 0));
                 return Content(String.Format("<script>alert('已存在该一级目录！插入失败！');location.href='{0}'</script>", Url.Action("ShowFirTitle","Category")));
                 
            }
            catch
            {
                //新建一个一级目录
                t_Category t_category = new t_Category();
                t_category.categoryName = name;
                t_category.parentID = 0;

                //将新建一级目录存入数据库中，并返回脚本内容
                if (ModelState.IsValid)
                {
                    db.t_Category.AddObject(t_category);
                    db.SaveChanges();

                    return Content(String.Format("<script>alert('新建成功！');location.href='{0}'</script>", Url.Action("ShowFirTitle", "Category")));
                }
            }
            return View("ShowFirTitle");

        }

        /// <summary>
        /// 新建二级目录
        /// </summary>
        /// <param name="id">二级目录对应的一级目录的CategoryID</param>
        /// <param name="name">传入新建的的二级目录的名称</param>
        /// <returns></returns>
        public ActionResult CreateSecTitle(long id = 0, string name = "")
        {
            string javascript = "";
            string parentName =  Convert.ToString(Session["ParentName"]);       //从ShowSecTitle传入的Session["ParentName"]是二级目录对应的一级目录的名称
            try
            {
                //查找是否存在一级目录下已有的二级目录，如果已存在则提示插入失败并返回当前页面
                t_Category existCategory = db.t_Category.Single(t => (t.categoryName == name) && (t.parentID == id));
                return Content(String.Format("<script> alert('已存在该二级目录！插入失败！'); location.href='{0}",Url.Action("ShowSecTitle","Category")) + id + "?name=" + parentName + "' </script>"); 
            }
            catch
            {
                //新建二级目录
                t_Category t_category = new t_Category();
                t_category.categoryName = name;
                t_category.parentID = id;
                
                //将新建的二级目录存入数据库中，并返回脚本内容
                if (ModelState.IsValid)
                {
                    db.t_Category.AddObject(t_category);
                    db.SaveChanges();
                    
                    return Content(String.Format("<script>alert('新建成功！'); window.location.href='{0}",Url.Action("ShowSecTitle","Category")) + Session["ParentID"] + "?name=" + Session["ParentName"] + "' </script>");
                }
            }
            return View("ShowFirTitle");
        }

        /// <summary>
        /// 编辑一级目录
        /// </summary>
        /// <param name="id">传入一级目录的CategoryID</param>
        /// <param name="name">传入一级目录的名称</param>
        /// <returns></returns>
        public ActionResult EditFirTitle(long id = 0, string name = "")
        {
            string javascript = "";
            try
            {
                //查找数据库中是否已经存在相同的一级目录，如果有则提示插入失败并返回当前页面
                t_Category existCategory = db.t_Category.Single(t => (t.categoryName == name));
                
                return Content(String.Format(String.Format("<script> alert('已存在该一级目录！编辑失败！'); location.href='{0}'"),Url.Action("ShowFirTitle","Category")+" </script>"));
            }
            catch
            {
                //查找要编辑的一级目录，并进入数据库中修改对应的记录
                t_Category t_category = db.t_Category.Single(t => t.categoryID == id);
                t_category.categoryName = name;
                if (ModelState.IsValid)
                {
                    db.ObjectStateManager.ChangeObjectState(t_category, EntityState.Modified);
                    db.SaveChanges();

                    return Content(String.Format("<script> alert('编辑成功！'); location.href='{0}' </script>", Url.Action("ShowFirTitle", "Category")));
                }
            }
            return View("ShowFirTitle");
        }

        /// <summary>
        /// 编辑二级目录
        /// </summary>
        /// <param name="id">传入所要的二级目录的CategoryID</param>
        /// <param name="name">传入二级目录的名称</param>
        /// <param name="parentID">传入二级目录对应的一级目录的CategoryID</param>
        /// <param name="parentName">传入二级目录对应的一级目录的名称</param>
        /// <returns></returns>
        public ActionResult EditSecTitle(long id = 0, string name = "", long parentID = 0, string parentName = "")
        {
            string javascript = "";
            try
            {
                //查找数据库中一级目录下是否已经存在相同的二级目录，如果有则提示插入失败并返回当前页面
                t_Category existCategory = db.t_Category.Single(t => (t.categoryName == name)&&(t.parentID == parentID));
                javascript = "<script> alert('已存在该二级目录！编辑失败！'); window.location.href='/Category/ShowSecTitle/" + parentID + "?name=" + parentName + "' </script>";
                return Content(javascript);
            }
            catch
            {
                //查找要编辑的二级目录，并进入数据库中修改对应的记录
                t_Category t_category = db.t_Category.Single(t => t.categoryID == id);
                t_category.categoryName = name;
                if (ModelState.IsValid)
                {
                    db.ObjectStateManager.ChangeObjectState(t_category, EntityState.Modified);
                    db.SaveChanges();
                    javascript = "<script> alert('编辑成功！'); window.location.href='/Category/ShowSecTitle/" + parentID + "?name=" + parentName + "' </script>";
                    return Content(javascript);
                }
            }
            return View("ShowFirTitle");
        }

        /// <summary>
        /// 删除一级目录及其对应的所有的二级目录
        /// </summary>
        /// <param name="id">传入一级目录对应的CategoryID</param>
        /// <returns></returns>
        [HttpGet, ActionName("DeleteFirTitle")]
        public ActionResult FirDeleteConfirmed(long id)
        {
            string javascript = "";
            try
            {
                //查找数据库中的一级目录并删除
                t_Category t_category = db.t_Category.Single(t => t.categoryID == id);
                db.t_Category.DeleteObject(t_category);
                db.SaveChanges();
            }
            catch
            {
                //若不存在该记录则返回当前页面
                return View("ShowFirTitle");
            }

            while (true)
            {               
                try
                {
                    //查找该一级目录对应的所有的二级目录，一条一条删除
                    t_Category t_categorySub = db.t_Category.First(t => t.parentID == id);
                    db.t_Category.DeleteObject(t_categorySub);
                    db.SaveChanges();
                }
                catch
                {
                    //当删除完所有二级目录时，则返回到当前页面

                    return Content(String.Format("<script> alert('删除成功！'); location.href='{0}'; </script>", Url.Action("ShowFirTitle", "Category")));
                }
            }
        }

        /// <summary>
        /// 删除二级目录
        /// </summary>
        /// <param name="id">传入二级目录对应的CategoryID</param>
        /// <returns></returns>
        [HttpGet, ActionName("DeleteSecTitle")]
        public ActionResult SecDeleteConfirmed(long id)
        {
            string javascript = "";
            //查询数据库中的二级目录的记录并删除
            t_Category t_category = db.t_Category.Single(t => t.categoryID == id);
            db.t_Category.DeleteObject(t_category);
            db.SaveChanges();
            javascript = "<script> alert('删除成功！'); window.location.href='/Category/ShowSecTitle/" + Session["ParentID"] + "?name=" + Session["ParentName"] + "' </script>";
            return Content(javascript); 
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}