using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace OldGoodsManage.Filters
{
    /************************************************************************
       类名称： CustomerFilterAttribute
       类作用： 用于执行Action前校验前台用户是否登录
       修改记录： 作者        时间        原因 
       *         王俊伟      2015.10.15   create
      * **********************************************************************/
    public class CustomerFilterAttribute:ActionFilterAttribute
    {
        //在执行Action前执行此方法
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //通过Session判断前台用户是否已经登录
            if (HttpContext.Current.Session["CustomerID"] == null)
            {
                //未登录就先跳转到登录界面
                filterContext.Result = new RedirectResult("/Customer/Login");
            }
        }

    }
}