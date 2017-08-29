using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OldGoodsManage.Controllers
{
    public class BaseController : Controller
   {
       /************************************************************************
       函数名称： OnActionExecuting
       函数作用：在要执行的Action之前执行该方法，如果后台用户没有登录则跳到登录页面
       传入参数：filterContext
       返回参数：无
       修改记录： 作者        时间        原因 
       *          李康志     2015.3.26   create
      * **********************************************************************/

       protected override void OnActionExecuting(ActionExecutingContext filterContext)
 {

   //首先检验一下Session里面是否已经有用户登录
     if (Session["UserLoginName"] == null)
      {
        //如果session里面没有值，则代表没有用户登录
        //跳转到登陆界面
        filterContext.HttpContext.Response.Redirect("/User/Login"); 
       }    

  base.OnActionExecuting(filterContext);  
  }

    }
}
