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
    public class UserController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();





        public ActionResult Login()
        {
            return View();
        }
        /*****************************************************************************
         * 函数名：Login(t_User t_user, FormCollection form)
         * 功能：后台用户的登录
         * 作者：庞碧娟
         * 入参：t_User对象，窗体对象
         * 输出：登录到后台用户的首页，把用户登录名存放在Session["UserLoginName"]里面
         *       用户ID存在Session["UserID"]里面
         * 修改记录：2015-01-22
         * ***************************************************************************/
        [HttpPost]
        public ActionResult Login(t_User t_user, FormCollection form)
        {

            //此处进行用户名和密码的验证，用MD5加密
            string strPwd = MD5Help.GetMD5(t_user.password + MD5Help.GetPasswordSalt());
            //判断用户名和密码是否正确
            t_User temp = db.t_User.SingleOrDefault(t => t.loginName == t_user.loginName && t.password == strPwd);
            //如果查询的结果为空，说明不存在该用户。
            if (temp == null)
            {
                //不存在该用户名和密码，password为数据模型的属性
                ModelState.AddModelError("password", "提供的用户名或密码不正确。");
                return View(t_user);
            }
            //判断验证码是否正确，验证码存放在Session["Code"]里面，VerificationCode是数据模型的隐藏字段，不存放在数据库中。
            if (t_user.VerificationCode != Convert.ToString(Session["Code"]))
            {
                ModelState.AddModelError("VerificationCode", "输入的验证码为空不正确");
                return View(t_user);
            }
            //把后台用户登录名存放在 Session["UserLoginName"]
            t_User user = db.t_User.Single(t => t.loginName == t_user.loginName);
            Session["UserLoginName"] = t_user.loginName;
            Session["UserID"] = user.userID;
            
            //将用户的权限列表放在Session里面
            Session["FunctionNameList"] = CheckAuthorize.GetFunctionName(user.userID);
            //成功转到User下的userList页面
            return RedirectToAction("UserList", "User");
        }

        public ActionResult Create()
        {
            return View();
        }

        /*****************************************************************************
         * 函数名：ActionResult AddUser(t_User t_user, FormCollection form)
         * 功能：后台用户的添加
         * 作者：庞碧娟
         * 入参：t_User对象，窗体对象
         * 输出：可返回上一页，可返回用户管理，添加后台用户成功返回后台用户管理页面
         * 修改记录：2015-01-22
         * ***************************************************************************/

        [HttpPost]
        public ActionResult AddUser(t_User t_user, FormCollection form)
        {
            //获取View层submit控件的name，根据name相同，不同的value来判断是哪个submit控件，从而实现不同的功能。
            string strSubmit = form["submit"];
            #region 返回后台用户管理的代码
            if (strSubmit == "后台用户管理")
            {
                return RedirectToAction("UserList", "User");//转到的页面
            }
            #endregion

            #region 添加用户的代码
            if (strSubmit == "添加")
            {
                //获取控件的值
                string strPwdAgain = form["PwdAgain"];
                string strRemarks = form["remarks"];
                string strSexRadio = form["sexRadio"];
                string strRoleRadio = form["role"];

                //查询用户输入的Email，loginName和tel
                var chkEmail = db.t_User.Where(model => model.email == t_user.email).FirstOrDefault();
                var chkLoginName = db.t_User.Where(model => model.loginName == t_user.loginName).FirstOrDefault();
                var chkTel = db.t_User.Where(model => model.tel == t_user.tel).FirstOrDefault();



                #region 验证信息
                if (chkEmail != null)
                {
                    ModelState.AddModelError("email", "该邮箱已经存在！");

                }
                if (chkLoginName != null)
                {
                    ModelState.AddModelError("loginName", "该用户名已经存在！");
                }
                if (chkTel != null)
                {
                    ModelState.AddModelError("tel", "该手机号已经存在！");
                }
                if (strPwdAgain == null || strPwdAgain != t_user.password)
                {
                    ModelState.AddModelError("PwdAgain", "确认密码为空或两次输入的密码不一致！");
                }
                if (strRoleRadio == null)
                {
                    ModelState.AddModelError("roleID", "请选择角色！");
                }
                #endregion

                //所有得验证通过执行
                if (ModelState.IsValid)
                {
                    //1表示男性
                    if (strSexRadio == "1")
                    {
                        t_user.sex = 1;
                    }
                    else
                    {
                        t_user.sex = 0;
                    }
                    //2表示高级管理员
                    if (strRoleRadio == "2")
                    {
                        t_user.roleID = 2;
                    }
                    else
                    {
                        t_user.roleID = 3;
                    }
                    //t_Role role = db.t_Role.Single(r => r.roleName == strRoleRadio);
                    t_user.registerTime = DateTime.Now;
                    //t_user.roleID = role.roleID;
                    t_user.password = MD5Help.GetMD5(t_user.password + MD5Help.GetPasswordSalt());
                    t_user.remarks = strRemarks;
                    db.t_User.AddObject(t_user);
                    int iNum = db.SaveChanges();
                    try
                    {
                        return Content(String.Format("<script>alert('添加成功！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
                    }
                    catch (Exception ex)
                    {
                        return Content(String.Format("<script>alert('" + ex.Message + "');location.href='{0}'</script>", Url.Action("AddUser", "User")), "text/html");
                    }
                }

            }
            #endregion

            return View(t_user);
        }

        public ActionResult AddUser()
        {
             
            return View();
        }

        /*****************************************************************************
          * 函数名：ActionResult UserList(FormCollection form)
          * 功能：后台用户管理
          * 作者：庞碧娟
          * 入参：窗体对象form
          * 输出：显示所有用户的信息，还可根据用户姓名查询记录，可链接到添加用户界面，
          * 修改记录：2015-01-22
          * ***************************************************************************/
       
        public ActionResult UserList(FormCollection form)
        {
            //获取view视图name为selectUserName的值
            string strSelect = form["selectUserName"];
            string strSubmit = form["submit"];
            string strCheck=form["chkId"];
          
            //有选择的删除
            if (strSubmit == "删除")
            {
                //把所有选中的checkbox存放在temp里面，以逗号分隔开
                string[] temp = strCheck.Split(',');
                int iNum = 0;
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        //根据选中的id查找数据，在view里面，checkbox的value存放的是userID
                        long id = int.Parse(temp[i]);
                        var data=db.t_User.SingleOrDefault(d => d.userID ==id);
                        db.t_User.DeleteObject(data);//删除
                        iNum = db.SaveChanges();
                        iNum++;
                    }
                    if (iNum > 0) 
                    {
                        return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
                    }
                    return Content(String.Format("<script>alert('修改失败，稍后再操作！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
                }
                //当长度不大于0的时候，就提示选择数据。
                return Content(String.Format("<script>alert('请选择数据！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
            }

            //根据关键
            if (strSubmit == "查询"&&strSelect!="")
            {
                //查询符合条件的信息放在list里面
                List<Models.t_User> list = (from u in db.t_User where u.userName == strSelect select u).ToList();
                ViewBag.DataList = list;
                return View();
            }
            if (strSubmit == "添加后台用户")
            {
                return RedirectToAction("AddUser");
            }
            List<Models.t_User> list1 = (from u in db.t_User select u).ToList();
            ViewBag.DataList = list1;

            //将
            Dictionary<long, string> roleDict = new Dictionary<long,string>();
            foreach (t_User user in list1)
	        {
                t_Role role = db.t_Role.Single( r => r.roleID == user.roleID);
                roleDict[user.roleID]= role.roleName;
	        }
            ViewBag.RoleDict = roleDict;

            return View();
        }
       
        /*****************************************************************************
         * 函数名：ActionResult UserDetails(long id = 0)
         * 功能：后台用户详情信息
         * 作者：庞碧娟
         * 入参：用户id
         * 输出：显示用户的所有信息
         * 修改记录：2015-01-22
         * ***************************************************************************/

     
        public ActionResult UserDetails(long id = 0)
        {
            t_User t_user = db.t_User.Single(t => t.userID == id);
            t_Role t_role = db.t_Role.Single(r => r.roleID == t_user.roleID);
            ViewData["roleID"] = t_role.roleName;
            if (t_user.sex == 0)
            {
                ViewData["sex"] = "女";
            }
            else
            {
                ViewData["sex"] = "男";
            }
            if (t_user == null)
            {
                return HttpNotFound();
            }
            return View(t_user);
        }
        [HttpPost]
        public ActionResult UserDetails()
        {
            return RedirectToAction("AddUser", "User");
        }


        /*****************************************************************************
        * 函数名：ActionResult EditUser(long id = 0)
        * 功能：后台用户信息修改
        * 作者：庞碧娟
        * 入参：用户id
        * 输出：根据用户id显示出用户对应信息
        * 修改记录：2015-01-28
        * ***************************************************************************/
        public ActionResult EditUser(long id = 0)
        {
            t_User t_user = db.t_User.Single(t => t.userID == id);
            if (t_user == null)
            {
                return HttpNotFound();
            }
            return View(t_user);
        }

        /*****************************************************************************
       * 函数名：ActionResult EditUser(t_User t_user, FormCollection form, long id = 0)
       * 功能：后台用户信息修改
       * 作者：庞碧娟
       * 入参：视图的t_User对象，form窗体，从信息页面传过来的用户id
       * 输出：可回到后台管理，修改成功，返回到userList页面，修改不成功，停留在当页面
       * 修改记录：2015-01-28
       * ***************************************************************************/
        [HttpPost]
        public ActionResult EditUser(t_User t_user, FormCollection form, long id = 0)
        {
            t_User user = db.t_User.AsNoTracking().Single(t => t.userID == id);
            string strSubmit = form["submit"];
            string strSexRadio = form["sexRadio"];
            string strRoleRadio = form["role"];
            var chkLoginName = db.t_User.AsNoTracking().Where(model => model.loginName == t_user.loginName).FirstOrDefault();
            var chkEmail = db.t_User.AsNoTracking().Where(model => model.email == t_user.email).FirstOrDefault();
            var chkTel = db.t_User.AsNoTracking().Where(model => model.tel == t_user.tel).FirstOrDefault();
            if (strSubmit == "后台用户管理")
            {
                return RedirectToAction("UserList", "User");//转到的页面
            }
            if (strSubmit == "修改")
            {

                if (user.loginName != t_user.loginName && chkLoginName != null)
                {
                    ModelState.AddModelError("loginName", "该登录名已存在，请重新输入！");

                }
                if (user.email != t_user.email && chkEmail != null)
                {
                    ModelState.AddModelError("email", "该邮箱已经存在！");
                }
                if (user.tel != t_user.tel && chkTel != null)
                {
                    ModelState.AddModelError("tel", "该手机号码已经存在！");
                }

                if (strRoleRadio == null)
                {
                    ModelState.AddModelError("roleID", "请选择角色！");
                }
                if (!ModelState.IsValid)
                {
                    if (strSexRadio == "1")
                    {
                        t_user.sex = 1;
                    }
                    else
                    {
                        t_user.sex = 0;
                    }

                    if (strRoleRadio == "2")
                    {
                        t_Role superRole = db.t_Role.Single(r => r.roleName == "超级管理员");
                        t_user.roleID = superRole.roleID;
                    }
                    else
                    {
                        t_Role ordinaryRole = db.t_Role.Single(r => r.roleName == "普通管理员");
                        t_user.roleID = ordinaryRole.roleID;
                    }
                    //要添加时间，时间没有变，等于原来注册的时候，若没有该句，要在视图添加隐藏字段
                    t_user.registerTime = user.registerTime;
                    t_user.password = MD5Help.GetMD5(t_user.password + MD5Help.GetPasswordSalt());
                    db.t_User.Attach(t_user);
                    db.ObjectStateManager.ChangeObjectState(t_user, EntityState.Modified);
                    int iNum=db.SaveChanges();
                    if ( iNum> 0)
                    {
                        return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
                    }
                    else 
                    {
                        return Content(String.Format("<script>alert('修改失败！');location.href='{0}'</script>", Url.Action("EditUser", "User")), "text/html");
                    }
             
                }
            }

            return View(t_user);
        }

        public ActionResult ChangePwd(long id = 0)
        {
            t_User t_user = db.t_User.Single(t => t.userID == id);
            if (t_user == null)
            {
                return HttpNotFound();
            }
            return View(t_user);
        }
        /******************************************************************************
        * 函数名：ActionResult ChangePwd(FormCollection form, long id = 0)
        * 功能：后台用户密码修改
        * 作者：庞碧娟
        * 入参：form窗体，从信息页面传过来的用户id
        * 输出：可回到后台管理，修改成功，返回到userList页面，修改不成功，停留在当页面
        * 修改记录：2015-01-28
        * ***************************************************************************/
        [HttpPost]
        public ActionResult ChangePwd(FormCollection form, long id = 0)
        {
            string strSubmit = form["submit"];
            if (strSubmit == "后台用户管理")
            {
                return RedirectToAction("UserList", "User");//转到的页面
            }
            t_User user = db.t_User.AsNoTracking().Single(t => t.userID == id);
            string strPwd = form["changPwd"];
            string strPwdMD5 = MD5Help.GetMD5((form["changPwd"]) + MD5Help.GetPasswordSalt());
            string strPwdAgain = form["changPwdAgain"];
            if (strPwdMD5 == user.password)
            {
                ModelState.AddModelError("password", "新密码不能与旧密码相同！");
            }
            if (strPwd == "" || strPwdAgain == "")
            {
                ModelState.AddModelError("password", "修改密码与确定密码不能为空！");
            }
            if (strPwd != strPwdAgain)
            {
                ModelState.AddModelError("password", "输入的两次密码不相同！");
            }
            user.password = strPwdMD5;
            if (ModelState.IsValid)
            {
                db.t_User.Attach(user);
                db.ObjectStateManager.ChangeObjectState(user, EntityState.Modified);
                int iNum=db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("UserList", "User")), "text/html");
                }
                else
                {
                    return Content(String.Format("<script>alert('修改失败！');location.href='{0}'</script>", Url.Action("ChangePwd", "User")), "text/html");
                }
            }
            return View(user);
        }

        /******************************************************************************
         * 函数名：ActionResult DeleteUser(long id=0)
         * 功能：后台用户删除
         * 作者：庞碧娟
         * 入参：从页面传过来的用户id
         * 输出：删除成功，返回到userList页面，修改不成功，提示删除失败
         * 修改记录：2015-01-28
         * ***************************************************************************/
        public ActionResult DeleteUser(long id=0)
        {
            t_User t_user = db.t_User.Single(t => t.userID == id);
            db.t_User.DeleteObject(t_user);
            int iNum=db.SaveChanges();
            if (iNum > 0)
            {
                return Content("<script>alert('删除成功！');location.href='/User/userList' </script>");
            }
            else
            {
                return Content("<script>alert('删除失败，稍后再操作！')</script>");
            }
        }

      

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}