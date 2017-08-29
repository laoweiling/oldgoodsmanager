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
    public class RoleController : BaseController
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();


        /************************************************************************
       函数名称： RoleList
       函数作用： 显示全部角色
       传入参数：无
       返回参数：角色列表
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.29    create
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.31    解决了用户输入的角色名前后有空格的bug.
      * **********************************************************************/
        public ActionResult RoleList()
        {
           

            //创建角色列表，找出所有的角色
            List<t_Role> rolesList = new List<t_Role>();
            rolesList = db.t_Role.ToList();
            //如果角色列表为null则提示系统没有任何角色
            if (rolesList == null)
            {
                return Content(String.Format("<script>alert('目前系统没有任何角色');location.href='{0}'</script>", Url.Action("Index", "User")), "text/html");
            }
            //通过ViewBag动态类型对象传给view
            ViewBag.rolesList = rolesList;
            return View();
        }


        /************************************************************************
      函数名称： AddRole
      函数作用： 显示增加角色页面
      传入参数：无
      返回参数：角色列表
      修改记录： 作者        时间        原因 
      *         李康志      2015.1.29    create
     * **********************************************************************/
        [HttpGet]
        public ActionResult AddRole()
        {
            //将数据库里面所有的功能找出来保存再列表里面传给view层
            List<t_Function> functionList = new List<t_Function>();
            functionList = db.t_Function.ToList();
            ViewBag.functionList = functionList;
            return View();
        }



        /************************************************************************
     函数名称： AddRole
     函数作用： 增加角色
     传入参数：表单对象  角色实体
     返回参数：角色列表
     修改记录： 作者        时间        原因 
     *         李康志      2015.1.29    create
    * **********************************************************************/
        [HttpPost]
        public ActionResult AddRole(FormCollection frm,t_Role model)
        {
            //判断角色名是否为空
            if(String.IsNullOrEmpty(model.roleName.Trim()))
            {
                return Content(String.Format("<script>alert('角色名不能为空！');location.href='{0}'</script>", Url.Action("AddRole", "Role")), "text/html");
            }

            //获取用户选中的功能ID的字符串
            string functionStr = frm["checkboxFunction"];
            //判断用户是否选择了功能
            if(functionStr==null)
            {
                return Content("<script>alert('您尚未为角色分配权限，请重试。');location.href=''</script>", "text/html");
            }

            else
            {
                model.roleName = model.roleName.Trim();
                //根据角色名查找数据库是否有该用户
                bool checkroleName = db.t_Role.Where(u => u.roleName ==model.roleName).Any();
                //如果返回true则角色表已存在同名角色
                if (checkroleName==true)
                {                    //ModelState.AddModelError("roleName","该角色名已经存在");

                    return Content("<script>alert('系统已存在同名角色，请换一个角色名重试。');location.href=''</script>", "text/html");
                }

                else if (ModelState.IsValid)
                {
                    //通过所有的验证之后开始将该角色存进角色表
                    db.t_Role.AddObject(model);
                    int addRoleNum = db.SaveChanges();
                    //成功存进角色表后将功能和角色存进角色功能关联表
                    if (addRoleNum > 0)
                    {
                        //获取已存进数据库的角色Id
                        long roleId = db.t_Role.Where(r => r.roleName == model.roleName).Select(r => r.roleID).FirstOrDefault();

                        string[] functionIDs = functionStr.Split(',');
                        //遍历用户给角色分配的功能，写进数据库
                        foreach (var singleID in functionIDs)
                        {
                            t_Role_Function t_role_function = new t_Role_Function();
                            t_role_function.roleID = roleId;
                            t_role_function.functionID = Convert.ToInt64(singleID);
                            db.t_Role_Function.AddObject(t_role_function);
                                                     
                        }
                        int num = db.SaveChanges();
                        if (num > 0)
                        {
                            return Content(String.Format("<script>alert('添加角色成功');location.href='{0}'</script>", Url.Action("RoleList", "Role")), "text/html");
                        }
                        else
                        {
                             return Content("<script>alert('添加角色失败，请重试。');location.href=''</script>", "text/html");
                        }
                    }
                    else
                    {
                        return Content("<script>alert('添加角色失败，请重试。');location.href=''</script>", "text/html");
                    }
                }
            }
            return View();
        }


            /************************************************************************
       函数名称： EditRole
       函数作用： 编辑角色
       传入参数：角色Id
       返回参数：功能列表  角色实体
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.29    create
      * **********************************************************************/
        public ActionResult EditRole(long id)
        {
            //根据角色ID找出角色实体对象
            t_Role t_role = db.t_Role.Where(r => r.roleID == id).FirstOrDefault();
            if (t_role == null)
            {
                return HttpNotFound();
            }
            //找出功能列表
            List<t_Function> functionList = new List<t_Function>();
            functionList = db.t_Function.ToList();
            ViewBag.functionList = functionList;
            return View(t_role);
        }


            /************************************************************************
       函数名称： EditRole
       函数作用： 角色
       传入参数：角色实体
       返回参数：无
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.29    create
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.31    解决了用户输入的角色名前后有空格的bug.
      * **********************************************************************/
        [HttpPost]
        public ActionResult EditRole(t_Role model)
        {         
            //接收用户选择的功能的ID的 字符串
            string functionStr = Request.Form["checkboxFunction"];
            //判断用户角色名是否为空
            model.roleName = model.roleName.Trim();
            if (String.IsNullOrEmpty(model.roleName))
            {
                return Content(String.Format("<script>alert('角色名不能为空！');location.href='{0}'</script>", Url.Action("EditRole", "Role",new {id=model.roleID})), "text/html");
            }
           //判断用户是否已经为角色分配了功能
            if(functionStr==null)
            {
                return Content(String.Format("<script>alert('您尚未为角色分配权限，请重试');location.href='{0}'</script>", Url.Action("EditRole", "Role", new { id = model.roleID })), "text/html");
            } 
            ///根据角色Id找出原来的角色对象
             t_Role t_role = db.t_Role.AsNoTracking().Single(r => r.roleID == model.roleID);
            //根据角色名查找数据库是否有同名角色
             bool checkRole = db.t_Role.Where(r => r.roleName == model.roleName).Any();

             if (model.roleName!= t_role.roleName && checkRole == true)
            { 
                //如果角色名不等于原来的角色名并且数据库存在同名角色名则角色名重复
                return Content(String.Format("<script>alert('系统已存在同名角色，请换一个角色名重试。');location.href='{0}'</script>", Url.Action("EditRole", "Role", new { id = model.roleID })), "text/html");
               }

           if (ModelState.IsValid)
                {
                    //当通过所有的验证之后，更新该角色表里面的角色
                    db.t_Role.Attach(model);
                    db.ObjectStateManager.ChangeObjectState(model, EntityState.Modified);
                     db.SaveChanges();
                     
                       //删除角色功能表里面的所有角色ID为同一个更新的角色的数据
                       List<t_Role_Function> oldFunctionIDList = new List<t_Role_Function>();
                       oldFunctionIDList = db.t_Role_Function.Where(r => r.roleID == model.roleID).ToList();
                       foreach (var item in oldFunctionIDList)
                       {
                           db.t_Role_Function.DeleteObject(item);

                       }        
                       //将新的角色对应的功能写进数据库
                       string[] newNunctionIdList = functionStr.Split(',');
                       foreach (var item in newNunctionIdList)
	                {
                        t_Role_Function roleFunction = new t_Role_Function();
                        roleFunction.roleID = model.roleID;
                        roleFunction.functionID = Convert.ToInt64(item);
                        db.t_Role_Function.AddObject(roleFunction);
	                }
                    int num = db.SaveChanges();
                    if (num > 0)
                    {
                        return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("RoleList", "Role", new { id = model.roleID })), "text/html");
                    }
                    else
                    {
                        return Content(String.Format("<script>alert('修改失败，请重试。');location.href='{0}'</script>", Url.Action("EditRole", "Role", new { id = model.roleID })), "text/html");
                    }
                }
           return View();
        }

       /************************************************************************
       函数名称： RoleDetails
       函数作用： 查看角色详情
       传入参数：角色ID
       返回参数：角色实体
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.29    create
      * **********************************************************************/
        public ActionResult RoleDetails(long id)
        {
            //根据角色id找出该角色对象
            t_Role t_role = db.t_Role.Where(r => r.roleID == id).FirstOrDefault();
            if (t_role == null)
            {
                return HttpNotFound();
            }
            //创建功能Id列表
            List<long?> functionIDLidt = new List<long?>();
            //根据角色ID从角色功能对应表中查找出所有功能ID
            functionIDLidt = db.t_Role_Function.Where(rf => rf.roleID == id).Select(rf => rf.functionID).ToList();
            //创建功能列表，通过遍历功能Id列表查找出所有的功能实体
            List<t_Function> functionList = new List<t_Function>();
            foreach (var item in functionIDLidt)
            {
                t_Function t_function = db.t_Function.Where(f => f.functionID == item).FirstOrDefault();
                if (t_function != null)
                {
                    functionList.Add(t_function);
                }
            }

            ViewBag.functionList = functionList;
            return View(t_role);
        }


        /************************************************************************
       函数名称： DeleteRole
       函数作用： 删除角色
       传入参数：角色ID
       返回参数：无
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.29    create
      * **********************************************************************/
        public ActionResult DeleteRole(long id)
        {
            //首先查找该角色是否已经被用户所使用
            bool isUsed=db.t_User.Where(u => u.roleID == id).Any();
            if (isUsed == true)
            {
                return Content(String.Format("<script>alert('该角色正被使用，不能删除！');location.href='{0}'</script>", Url.Action("RoleList","Role")), "text/html");
            }
            else
            {
                //删除角色功能对应表里面的所有角色ID等于要删除的角色ID的数据
               List<t_Role_Function> roleFunctionList=db.t_Role_Function.Where(rf=>rf.roleID==id).ToList();
               foreach (var item in roleFunctionList)
               {
                   db.t_Role_Function.DeleteObject(item);
               }
               
                //删除角色表里面对应的角色
               t_Role t_role=new t_Role();
               t_role=db.t_Role.Where(r=>r.roleID==id).FirstOrDefault();
               db.t_Role.DeleteObject(t_role);
               int num=db.SaveChanges();
               if (num > 0)
               {
                   return Content(String.Format("<script>alert('删除角色成功！');location.href='{0}'</script>", Url.Action("RoleList", "Role")), "text/html");
               }
               else
               {
                   return Content(String.Format("<script>alert('删除角色失败，请重试！');location.href='{0}'</script>", Url.Action("RoleList", "Role")), "text/html");
               }
            }
        }
    }
}
