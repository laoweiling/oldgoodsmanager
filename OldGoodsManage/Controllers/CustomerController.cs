using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using OldGoodsManage.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using System.Web.Script.Serialization;
using OldGoodsManage.Filters;
using PagedList;

namespace OldGoodsManage.Controllers
{
    public class CustomerController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();
        public ActionResult Register()
        {
            GetAllSchool();
            return View();
        }
        /*****************************************************************************
       * 函数名：Register(t_Customer t_customer, FormCollection form)
       * 功能：前台用户的注册，通过邮箱发送验证码
       * 作者：庞碧娟
       * 入参：t_customer对象，窗体对象
       * 输出：注册成功转到登录页面，不成功停留在当前页面。
       * 修改记录：2015-01-28
       * ***************************************************************************/
        [HttpPost]
        public ActionResult Register(t_Customer t_customer, FormCollection form)
        {
            var chkEmail = db.t_Customer.Where(model => model.email == t_customer.email).FirstOrDefault();
            var chkLoginName = db.t_Customer.Where(model => model.loginName == t_customer.loginName).FirstOrDefault();
            var chktel = db.t_Customer.Where(model => model.tel == t_customer.tel).FirstOrDefault();
            string strPwd = form["password"];
            string strPwdAgain = form["passwordAgain"];
            string strSubmit = form["submit"];
            string strSchool = form["schools"];
            t_customer.schoolID = Convert.ToInt64(strSchool);
            if (chkEmail != null)
            {
                ModelState.AddModelError("email", "你输入的Email已经有人注册了！");
            }
            if (chkLoginName != null)
            {
                ModelState.AddModelError("loginName", "你输入的用户名已存在！");
            }
            //点击获取验证码按钮
            if (ModelState["email"].Errors.Count == 0 && ModelState["loginName"].Errors.Count == 0 && strSubmit == "点击获取验证码")
            {
                //获取所有的校区以填充页面
                GetAllSchool();
                if (SendEmail(t_customer.loginName, t_customer.email))
                {
                    ModelState.AddModelError("VerificationCode", "验证码发送成功！");
                    return View(t_customer);
                }
                ModelState.AddModelError("VerificationCode", "验证码发送失败，请重新点击发送！");
                return View(t_customer);
            }
            if (t_customer.password == null || strPwdAgain == "")
            {
                ModelState.AddModelError("password", "密码和确认密码不能为空！");
            }
            if (strPwd != strPwdAgain)
            {
                ModelState.AddModelError("password", "你两次输入的密码不相同！");
            }
            if (chktel != null)
            {
                ModelState.AddModelError("tel", "你输入的手机号已经存在！");
            }
            if (t_customer.VerificationCode != Convert.ToString(Session["Valid"]) || t_customer.VerificationCode == null)
            {
                ModelState.AddModelError("VerificationCode", "验证码为空或不正确！");
            }
            if (ModelState.IsValid)
            {

                t_customer.password = MD5Help.GetMD5(strPwd + MD5Help.GetPasswordSalt());
                t_customer.registerTime = DateTime.Now;
                t_customer.customerType = 2;//0为金牌，1为银牌，2为铜牌
                db.t_Customer.AddObject(t_customer);
                int iNum = db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('注册成功！');location.href='{0}'</script>", Url.Action("Login", "Customer")), "text/html");
                }
                else
                {

                    return Content(String.Format("<script>alert('注册失败！');location.href='{0}'</script>", Url.Action("Register", "Customer")), "text/html");
                }

            }
            return View();
        }

        //获取所有的校区
        public void GetAllSchool()
        {
            //获取所有的校区，以便前台用户进行注册
            List<t_School> listSchools = (from schools in db.t_School
                                          select schools).ToList();
            List<SelectListItem> selectSchools = listSchools.Select(listSchool => new SelectListItem() { Value = listSchool.schoolID.ToString(), Text = listSchool.schoolName }).ToList();

            ViewBag.selectSchools = selectSchools;
        }


        public ActionResult Login()
        {
            return View();
        }
        /*****************************************************************************
        * 函数名：Login(t_Customer t_customer, FormCollection form)
        * 功能：前台用户的登录
        * 作者：庞碧娟
        * 入参：t_customer对象，窗体对象
        * 输出：登录到前用户的个人中心页面，把用户登录名存放在Session["CustomerLoginName"]里面
        *       客户ID存在Session["CustomerID"]里面
        * 修改记录：2015-01-28
        * ***************************************************************************/
        [HttpPost]
        public ActionResult Login(t_Customer t_customer, FormCollection form)
        {

            //此处进行用户名和密码的验证
            string strPwd = MD5Help.GetMD5(t_customer.password + MD5Help.GetPasswordSalt());
            t_Customer temp = db.t_Customer.SingleOrDefault(t => t.loginName == t_customer.loginName && t.password == strPwd);
            if (temp == null)
            {
                //不存在该用户名和密码
                ModelState.AddModelError("password", "提供的用户名或密码不正确。");
                return View(t_customer);
            }
            if (t_customer.VerificationCode != Convert.ToString(Session["Code"]))
            {
                ModelState.AddModelError("VerificationCode", "您输入的验证码为空或不正确！");
                return View(t_customer);
            }
            if (temp.locked == 1)
            {
                ModelState.AddModelError("loginName", "该用户已被锁定，不能登录，请联系管理员！");
                return View(t_customer);
            }
            t_Customer customer = db.t_Customer.Single(t => t.loginName == t_customer.loginName);
            Session["CustomerLoginName"] = t_customer.loginName;
            Session["CustomerID"] = customer.customerID;
            return RedirectToAction("PersonalInfo", "Customer");//转到的页面
        }

        /*****************************************************************************
       * 函数名： bool SendEmail(string strLoginName, string strToEmail)
       * 功能：发送邮件
       * 作者：庞碧娟
       * 入参：用户名和接收邮箱地址
       * 输出：发送成功，返回true，失败，返回false
       * 修改记录：2015-01-28
       * ***************************************************************************/
        public bool SendEmail(string strLoginName, string strToEmail)
        {
            //收件人地址
            MailAddress to = new MailAddress(strToEmail);
            //发件人地址
            MailAddress from = new MailAddress("DWei50@126.com");
            //实例化对象
            MailMessage message = new MailMessage(from, to);
            //邮件标题
            message.Subject = "二手平台网站提示";
            //邮件的正文，GetVali()函数是获取验证码。
            message.Body = "您好！您正在使用二手平台网站，您的登录名为：'" + strLoginName + "'您的验证码为：'" + GetVali() + "'";
            //设置邮件正文的编码，获取当前代码页的编码
            message.BodyEncoding = System.Text.Encoding.Default;
            //创建一个SmtpClient类的新实例，并初始化它的SMTP事物的服务器，发送邮件使用SMTP
            SmtpClient client = new SmtpClient();
            //发件人邮箱地址的服务器，例如，163的邮箱是smtp.163.com；qq的邮箱是smtp.qq.com
            client.Host = "smtp.126.com";
            //发送的端口为25
            client.Port = 25;
            //获取发件人的邮箱和密码
            client.Credentials = new NetworkCredential("DWei50@126.com", "erhuowang");

            try
            {
                //发送邮箱
                client.Send(message);
                //return JavaScript("alert('验证码已经发到您的邮箱！')");
                return true;
               

            }
            catch
            {
                //return JavaScript("alert('" + ex.Message + "')");
                return false;
               
            }
        }

        /*****************************************************************************
         * 函数名：string GetVali()
         * 功能：验证码函数
         * 作者：庞碧娟
         * 入参：null
         * 输出：把验证码存放在Session["Valid"]，纯数字，为发送邮箱提供验证码
         * 修改记录：2015-01-28
         * ***************************************************************************/
        public string GetVali()
        {
            string strVali = "0,1,2,3,4,5,6,7,8,9";
            string[] VailArray = strVali.Split(',');
            string strReturnNum = "";
            int iNum = -1;
            Random vrand = new Random();
            for (int n = 1; n < 5; n++)
            {
                if (iNum != -1)
                {
                    vrand = new Random(n * iNum * unchecked((int)DateTime.Now.Ticks));
                }
                int iNum2 = vrand.Next(10);
                iNum = iNum2;
                strReturnNum += VailArray[iNum2];
            }
            Session["Valid"] = strReturnNum;
            return strReturnNum;
        }

        /*****************************************************************************
       * 函数名：personalInfo(FormCollection form)
       * 功能：前台客户的个人信息显示，用户没有登录会提示登录
       * 作者：庞碧娟
       * 入参：窗体对象
       * 输出：根据Session["CustomerLoginName"]显示对应用户的视图
       * 修改记录：2015-01-28
       * ***************************************************************************/
        [CustomerFilter]
        public ActionResult PersonalInfo(FormCollection form)
        {
 
            string strSubmit = form["submit"];
            string strLoginName = Convert.ToString(Session["CustomerLoginName"]);
            t_Customer t_customer = db.t_Customer.Single(t => t.loginName == strLoginName);
            if (strSubmit == "我要修改")
            {
                return RedirectToAction("EditCustomer", "Customer", new { id = t_customer.customerID });
            }
            List<Models.t_Customer> list = (from u in db.t_Customer where u.loginName == strLoginName select u).ToList();
            t_School t_School = db.t_School.Single(s => s.schoolID == t_customer.schoolID);
            ViewData["schoolID"] = t_School.schoolName;
            ViewBag.DataList = list;
            return View();
        }


        public ActionResult EditCustomer(long id = 0)
        {
            t_Customer t_customer = db.t_Customer.Single(t => t.customerID == id);
            t_School t_School = db.t_School.Single(s => s.schoolID == t_customer.schoolID);
            ViewData["schoolID"] = t_School.schoolName;
            if (t_customer == null)
            {
                return HttpNotFound();
            }
            return View(t_customer);
        }
        /*****************************************************************************
        * 函数名：editCustomer(t_Customer t_customer, FormCollection form, long id = 0)
        * 功能：前台客户的个人信息修改，点击可修改密码
        * 作者：庞碧娟
        * 入参：t_Customer对象，窗体对象，id
        * 输出：修改成功跳转到personalInfo个人中心页面，不成功提示错误，停留在当前页面
        * 修改记录：2015-01-28
        * ***************************************************************************/
        [HttpPost]
        public ActionResult EditCustomer(t_Customer t_customer, FormCollection form, long id = 0)
        {
            string strSubmit = form["submit"];
            string strSex = form["sex"];
            t_Customer customer = db.t_Customer.AsNoTracking().Single(t => t.customerID == id);
            var chkTel = db.t_Customer.AsNoTracking().Where(model => model.tel == t_customer.tel).FirstOrDefault();

            if (strSubmit == "我要修改密码")
            {
                return RedirectToAction("ChangePwd", "Customer", new { id = t_customer.customerID });
            }
            if (strSubmit == "保存")
            {
                if (strSex == "男")
                {
                    t_customer.sex = 1;
                }
                else
                {
                    t_customer.sex = 0;
                }
                if (customer.tel != t_customer.tel && chkTel != null)
                {
                    ModelState.AddModelError("tel", "该手机号码已经存在！");
                }
                //ModelState["tel"].Errors.Count==0只是判断tel字段，address字段和customerName字段，
                //其他不能修改的字段值在View里面赋值了，这里不用判断，所以不能用ModelState.IsValid
                if (ModelState["tel"].Errors.Count == 0 && ModelState["address"].Errors.Count == 0 && ModelState["customerName"].Errors.Count == 0)
                {
                    db.t_Customer.Attach(t_customer);
                    db.ObjectStateManager.ChangeObjectState(t_customer, EntityState.Modified);
                    int iNum = db.SaveChanges();
                    if (iNum > 0)
                    {
                        return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("PersonalInfo", "Customer")), "text/html");
                    }
                    else
                    {
                        return Content("<script>alert('修改失败，请重新修改！')</script>");
                    }

                }

            }

            return View(t_customer);
        }


        public ActionResult ChangePwd(long id = 0)
        {
            t_Customer t_customer = db.t_Customer.Single(t => t.customerID == id);
            return View(t_customer);
        }
        /*****************************************************************************
        * 函数名：ChangePwd(t_Customer t_customer, FormCollection form)
        * 功能：前台客户的个人密码修改
        * 作者：庞碧娟
        * 入参：t_Customer对象，窗体对象
        * 输出：修改成功跳转到personalInfo个人中心页面，不成功提示错误，停留在当前页面
        * 修改记录：2015-01-28
        * ***************************************************************************/
        [HttpPost]
        public ActionResult ChangePwd(t_Customer t_customer, FormCollection form)
        {
            int id = Convert.ToInt32(Session["CustomerID"]);
            t_Customer customer = db.t_Customer.Single(t => t.customerID == id);
            string strCode = form["code"];
            string strSubmit = form["submit"];
            string strPwd = form["newPwd"];
            string strPwdAgain = form["newPwdAgain"];
            string strPassword = MD5Help.GetMD5(strPwd + MD5Help.GetPasswordSalt());
            if (strSubmit == "点击获取")
            {
                if (SendEmail(customer.loginName, customer.email))
                {
                    ModelState.AddModelError("VerificationCode", "验证码发送成功！");
                    return View(customer);
                }
                ModelState.AddModelError("VerificationCode", "验证码发送失败，请重新点击发送！");
                return View(customer);
            }
            if (strCode != Convert.ToString(Session["Valid"]) || strCode == "")
            {
                ModelState.AddModelError("VerificationCode", "验证码不正确或为空！");
            }
            if (strPwd == "" || strPwdAgain == "")
            {
                ModelState.AddModelError("password", "密码和确认密码不能为空！");
            }
            if (customer.password == strPassword)
            {
                ModelState.AddModelError("password", "新密码不能与原密码相同！");
            }

            if (strPwd != strPwdAgain)
            {
                ModelState.AddModelError("password", "新密码与确认密码不相同！");
            }
            if (ModelState["password"].Errors.Count == 0)
            {
                customer.password = strPassword;
                int iNum = db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("PersonalInfo", "Customer")), "text/html");
                }
                else
                {
                    return Content("<script>alert('修改失败，请重新修改！')</script>");
                }
            }
            return View(customer);
        }



        /*****************************************************************************
         * 函数名：CustomerList(FormCollection form)
         * 功能：管理员显示客户在后台的信息
         * 作者：庞碧娟
         * 入参：窗体对象
         * 输出：视图
         * 修改记录：2015-01-28
         * ***************************************************************************/
        public ActionResult CustomerList(FormCollection form,List<t_Goods> listGoods)
        {
            //获取form里面的控件的值
            string strSelect = form["selectBox"];
            string strSelectCustomer = form["selectCustomer"];
            string strSubmit = form["submit"];
            string strCheck = form["chkId"];
            int num = 0;
            int isLocked = 0;
            List<Models.t_Customer> list = null;
            //多选删除
            #region 多选删除
            if (strSubmit == "删除")
            {
                //把所有选中的checkbox存放在temp里面，以逗号分隔开
                string[] temp = strCheck.Split(',');
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        //根据选中的id查找数据，在view里面，checkbox的value存放的是userID
                        long id = int.Parse(temp[i]);
                        var data = db.t_Customer.SingleOrDefault(d => d.customerID == id);
                        var chkCustomer = db.t_Goods.Where(model => model.customerID == id).FirstOrDefault();
                        if (chkCustomer != null)
                        {
                            return Content(String.Format("<script>alert('该用户还存在其他信息，不能删除！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")), "text/html");

                        }
                        db.t_Customer.DeleteObject(data);//删除
                        num = db.SaveChanges();
                        num++;
                    }
                    if (num > 0)
                    {
                        return Content(String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")), "text/html");
                    }
                    return Content(String.Format("<script>alert('修改失败，稍后再操作！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")), "text/html");
                }
                //当长度不大于0的时候，就提示选择数据。
                return Content(String.Format("<script>alert('请选择数据！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")), "text/html");
            } 
            #endregion

            //根据关键属性查询
            #region 根据关键属性查询 if (strSubmit == "查询")
            if (strSubmit == "查询" && strSelectCustomer != "全部")
            {
                if (strSelectCustomer == "用户类型")
                {
                    //根据不同的类型查询
                    switch (strSelect)
                    {
                        case "金牌":
                            num = 0; break;
                        case "银牌":
                            num = 1; break;
                        case "铜牌":
                            num = 2; break;
                    }
                    list = (from u in db.t_Customer where u.customerType == num select u).ToList();

                }
                if (strSelectCustomer == "用户姓名")
                {
                    list = (from u in db.t_Customer where u.customerName == strSelect select u).ToList();
                }
                if (strSelectCustomer == "是否被锁定")
                {
                    switch (strSelect)
                    {
                        case "是":
                            isLocked = 1; break;
                        case "否":
                            isLocked = 0; break;
                    }
                    list = (from u in db.t_Customer where u.locked == isLocked select u).ToList();
                }
                ViewBag.DataList = list;
                return View();
            }
            #endregion
            list = (from u in db.t_Customer select u).ToList();
            ViewBag.DataList = list;
            return View();
        }


        /*****************************************************************************
         * 函数名：customerDetails(long id = 0)
         * 功能：管理员查看客户的详情信息
         * 作者：庞碧娟
         * 入参：用户id
         * 输出：根据id显示对应视图
         * 修改记录：2015-01-28
         * ***************************************************************************/
        public ActionResult CustomerDetails(long id = 0)
        {
            t_Customer t_customer = db.t_Customer.Single(t => t.customerID == id);

            if (t_customer == null)
            {
                return HttpNotFound();
            }
            //根据schoolID查找出schoolName，存放在 ViewData["schoolID"]
            t_School t_School = db.t_School.Single(s => s.schoolID == t_customer.schoolID);
            ViewData["schoolID"] = t_School.schoolName;
            return View(t_customer);
        }

        /*****************************************************************************
       * 函数名：deleteCustomer(long id=0)
       * 功能：管理员删除前台客户信息
       * 作者：庞碧娟
       * 入参：id
       * 输出：删除成功提示成功，不成功提示错误
       * 修改记录：2015-01-28
       * ***************************************************************************/
        public ActionResult DeleteCustomer(long id=0)
        {
            var chkCustomer = db.t_Goods.Where(model => model.customerID == id).FirstOrDefault();
            if (chkCustomer != null)
            {
                return Content("<script>alert('不能删除该用户，请先删除该用户已发布的商品！')</script>");

            }
            t_Customer t_customer = db.t_Customer.Single(t => t.customerID == id);
            db.t_Customer.DeleteObject(t_customer);
            int iNum = db.SaveChanges();
            if (iNum > 0)
            {
                return Content(String.Format("<script>alert('删除成功！');location.href='{0}'</script>"), Url.Action("CustomerList", "Customer"));
            }
            else
            {
                return Content(String.Format("<script>alert('删除失败，请重新删除！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")));
            }
        }

        //锁定与解锁
        public ActionResult Locked(FormCollection form, long id = 0)
        {
            string strSubmit = form["submit"];
            t_Customer t_customer = db.t_Customer.Single(t => t.customerID == id);
            int iNum = 0;
            if (strSubmit == "锁定")
            {
                if (t_customer.locked == 1)
                {

                    return Content(String.Format("<script>alert('该用户已被锁定，不能再进行锁定，请先解锁！');location.href='{0}'</script>", Url.Action("Locked", "Customer")), "text/html");
                }
                t_customer.locked = 1;
                //db.t_Customer.Attach(t_customer);
                db.ObjectStateManager.ChangeObjectState(t_customer, EntityState.Modified);
                iNum = db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('解锁成功！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")));
                }
                else
                {
                    return Content(String.Format("<script>alert('解锁失败，请重新解锁！');location.href='{0}'</script>", Url.Action("CustomerList", "Customer")));
                }
            }
            if (strSubmit == "解锁")
            {
                if (t_customer.locked == 0)
                {
                    return Content(String.Format("<script>alert('该用户已解锁，不能再进行解锁，请先锁定！');location.href='{0}'</script>", Url.Action("Locked", "Customer")), "text/html");
                }
                t_customer.locked = 0;
                //db.t_Customer.Attach(t_customer);
                db.ObjectStateManager.ChangeObjectState(t_customer, EntityState.Modified);
                iNum = db.SaveChanges();
                if (iNum > 0)
                {
                    return Content(String.Format("<script>alert('解锁成功！');location.href='{0}'</script>",Url.Action("CustomerList","Customer")));
                }
                else
                {
                    return Content(String.Format("<script>alert('解锁失败，请重新解锁！');location.href='{0}'</script>",Url.Action("CustomerList","Customer")));
                }
            }

            if (t_customer == null)
            {
                return HttpNotFound();
            }
            return View(t_customer);
        }



        /************************************************************************
       函数名称： personalSell
       函数作用： 显示个人发布
       传入参数：无
       返回参数：发布列表
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.26    create
         *       王俊伟      2015.10.14   将个人发布的商品分页
      * **********************************************************************/
        [CustomerFilter]
        public ActionResult PersonalSell(int page=1)
        {  

            //根据登陆者的ID通过商品表查找出所有与该ID关联的商品，放在商品列表中
            long customerID = Convert.ToInt64(Session["CustomerID"]);
            List<t_Goods> listGoods = db.t_Goods.Where(g => g.customerID == customerID).ToList();

            //将个人发布的商品分页，每页pageSize个商品
            int pageSize = 4;
            var personalGoods = listGoods   .ToPagedList(pageNumber: page, pageSize: pageSize);
            return View(personalGoods);
            
        }

        /************************************************************************
     函数名称： personalCollect
     函数作用： 显示个人收藏
     传入参数：无
     返回参数：收藏列表
     修改记录： 作者        时间        原因 
     *         李康志      2015.1.26    create
         *     王俊伟      2015.10.14   将个人收藏的商品分页显示
    * **********************************************************************/
        [CustomerFilter]
        public ActionResult PersonalCollect(int page=1)
        {
            //根据登陆者的ID查找出收藏的商品ID
            long customerID = Convert.ToInt64(Session["CustomerID"]);
            t_Customer t_customer=db.t_Customer.Where(c=>c.customerID==customerID).FirstOrDefault();
            string collectionID = t_customer.collectionID;
            //定义一个列表接收收藏的商品实体
            List<t_Goods> goodsList = new List<t_Goods>();
            if (collectionID != null)
            {
                //如果收藏ID不为null则通过逗号分隔此字符串，并且放在字符串数组里面
                string[] collectionList = collectionID.Split(',');
               
                //倒序遍历出所有的商品id，并且根据id查找相对应的商品，放在商品列表中
                for (int i = collectionList.Length-1; i>=0; i--)
                {
                    long singleID = Convert.ToInt64(collectionList[i]);
                    t_Goods t_goods = db.t_Goods.Where(g => g.goodsID == singleID).FirstOrDefault();
                    if (t_goods != null)
                    {
                        goodsList.Add(t_goods);
                    }            
                }   
                
            }
            //将个人收藏的商品进行分页显示，每页4个商品
            int pageSize = 4;
            var goods = goodsList.ToPagedList(pageNumber:page,pageSize:pageSize);
            return View(goods);
        }

        /************************************************************************
           函数名称： DeleteCollection
           函数作用： 删除个人收藏
           传入参数：商品ID
           返回参数：无
           修改记录： 作者        时间        原因 
           *         李康志      2015.1.26    create
          * **********************************************************************/
        public ActionResult DeleteCollection(long id)
        {
            //根据登陆者的ID查找出收藏的商品ID
            long customerID = Convert.ToInt64(Session["CustomerID"]);
            t_Customer t_customer = db.t_Customer.Where(c => c.customerID == customerID).FirstOrDefault();
            //将收藏列表的ID字符穿分隔成字符创数组，每一个字符串都是一个商品Id
            string collectionID = t_customer.collectionID;
            string[] collectionList = collectionID.Split(',');
            string newcollectionID = null;
            //遍历收藏商品id数组
            foreach (var singleID in collectionList)
            {
                if (singleID != id.ToString())
                {
                    //如果收藏商品id不等于要删除的ID，则继续放在收藏列表里面
                    if (newcollectionID == null)
                    {
                        newcollectionID = singleID;
                    }
                    else
                    {
                        newcollectionID = newcollectionID + "," + singleID;
                    }
                }
            }
            t_customer.collectionID = newcollectionID;
            //保存更改
            int num = db.SaveChanges();
            if (num >= 0)
            {
                return Content(String.Format("<script>alert('删除收藏成功！');location.href='{0}'</script>", Url.Action("PersonalCollect", "Customer")), "text/html");
            }
            else
            {
                return Content(String.Format("<script>alert('删除失败，请重试操作。');location.href='{0}'</script>", Url.Action("PersonalCollect", "Customer")), "text/html");
            }
            

        }

        /************************************************************************
           函数名称： SystemMessage
           函数作用： 显示系统消息
           传入参数：无
           返回参数：消息列表 还有一个以后台用户ID为键，用户名为值的键值对类型的字典
           修改记录： 作者        时间        原因 
           *         李康志      2015.2.1    create
          * **********************************************************************/
        [CustomerFilter]
        public ActionResult SystemMessage()
        {
            long customerId = Convert.ToInt64(Session["CustomerID"]);
            //定义一个信息类型的列表，将所有该登录用户收到的信息都放在里面
            List<t_Message> messageList = new List<t_Message>();
            messageList = db.t_Message.Where(m => m.receriverID == customerId).OrderByDescending(m => m.messageID).ToList();
            if(messageList!=null)
            {
                //如果信息列表不为空则则遍历信息列表里面所有的发送者的id，并且根据id将发送者的用户名以键值对的形式存到字典里面
                Dictionary<long, string> dictIdName = new Dictionary<long, string>();
                foreach (var item in messageList)
                {
                    if (!dictIdName.ContainsKey(item.senderID))
                    {
                        string name = db.t_User.Where(u => u.userID == item.senderID).Select(u => u.loginName).FirstOrDefault();
                        dictIdName.Add(item.senderID, name);
                    }
               
                
                }
                ViewBag.messageList = messageList;
                ViewBag.dictIdName = dictIdName;
            }
           
            return View();
        }

        /************************************************************************
           函数名称： SystemMessage
           函数作用： 将用户回复的内容写进数据库
           传入参数：表单对象
           返回参数：无
           修改记录： 作者        时间        原因 
           *         李康志      2015.2.1    create
           修改记录： 作者        时间        原因 
           *         李康志      2015.2.14    页面增加js的校验，回复内容不能为空
          * **********************************************************************/
        [HttpPost]
        [CustomerFilter]
        public ActionResult SystemMessage(FormCollection frm)
        {
            //接受页面传送过来的信息的id
                long messageId = Convert.ToInt64(frm["messageId"]);
                string replyText = frm["messageReply"].Trim(); ;
                if (String.IsNullOrEmpty(replyText))
                {
                    return Content("<script>alert('请输入回复内容。');location.href=''</script>", "text/html");
                }

                else
                {
                    //首先根据信息id将原来原来的信息取出来，然后将用户回复的内容写进reply字段
                    t_Message model = db.t_Message.Where(m => m.messageID == messageId).First();
                    if (model != null)
                    {
                        model.reply = replyText;
                        db.ObjectStateManager.ChangeObjectState(model, EntityState.Modified);
                        int num = db.SaveChanges();
                        if (num == 1)
                        {
                            return Content(String.Format("<script> alert('亲的回复管理员已经收到，二货们会尽快处理。');location.href='{0}'</script>", Url.Action("SystemMessage", "Customer")), "text/html");
                        }
                        else
                        {
                            return Content("<script>alert('刚刚忘了出了点问题，请重试。');location.href=''</script>", "text/html");
                        }
                    }
                }
            return View();
        }

        /************************************************************************
           函数名称： CustomerMessage
           函数作用： 显示商品评论
           传入参数：无
           返回参数：无
           修改记录： 作者        时间        原因 
           *         李康志      2015.2.2    create
         *           王俊伟      2015.9.25   修改Bug，将db.t_Goods.Where改为db.t_Goods.Single
          * **********************************************************************/
        [CustomerFilter]
        public ActionResult CustomerMessage()
        {
 
            //查找出登录用于发布的所有商品添加到商品列表里面
            long customerId = Convert.ToInt64(Session["CustomerID"]);
            List<t_Goods> goodsList = db.t_Goods.Where(g => g.customerID == customerId).OrderByDescending(g => g.goodsID).ToList();
            if (goodsList != null)
            {
                //遍历商品列表，根据goodsid在评论表里面查找是否有相关的评论，如果没有，则将该商品从列表里面移除。
                for (int i = 0; i < goodsList.Count; i++)
                {   
                    long goodsId=goodsList[i].goodsID;
                    bool newComment = db.t_Comment.Where(c => c.goodsID == goodsId).Any();
                    if (newComment ==false)
                    {
                        t_Goods good = db.t_Goods.Single(g => g.goodsID == goodsId);
                        goodsList.Remove(good); 
                    }
                }
            
            }
            ViewBag.goodsList = goodsList;
            return View();
        }

        //返回json格式的前台用户
        public string GetCustomerByName(string loginName)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            t_Customer customer = null;
            string json = null;
            try
            {
                customer = db.t_Customer.Single(c => c.loginName == loginName);
                //序列化
                json = jss.Serialize(customer);
            }
            catch (Exception)
            {
                return null;
            }
            return json;
        }

        public ActionResult LogOut() 
        {
            //清除船体验证的cookies
            FormsAuthentication.SignOut();
            //清除所有的Session值
            Session.Clear();
            return RedirectToAction("ReadGoods", "Goods");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}