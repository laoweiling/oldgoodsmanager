using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Controllers;

namespace OldGoodsManage.Filters
{
    /************************************************************************
       类名称： UserFilterAttribute
       类作用： 用于执行Action前校验后q台用户是否登录
       修改记录： 作者        时间        原因 
       *         王俊伟      2015.10.15   create
      * **********************************************************************/
    public class UserFilterAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //在执行Action前执行此方法
            base.OnActionExecuting(filterContext);
            //通过Session判断后台用户是否已经登录
            if (HttpContext.Current.Session["UserID"] == null)
            {
                //未登录就先跳转到登录界面
                filterContext.Result = new RedirectResult("/User/Login");
            }
        }
    }
}