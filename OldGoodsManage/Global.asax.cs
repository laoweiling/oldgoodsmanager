using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Security.Principal;
using OldGoodsManage.Repositories;

namespace OldGoodsManage
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public MvcApplication()
        {
            //将自定义的安全验证方法安全模块中，每次跳转都会触发该次验证
            AuthorizeRequest += new EventHandler(MvcApplication_Authorize);
        }
        /// <summary>
        /// 定义一个验证用户权限的方法
        /// 检测用户是否已经登录，如果已经登录则检测用户的角色
        /// 如果用户为登录则进入到用户登录的页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MvcApplication_Authorize(object sender, EventArgs e)
        {
            IIdentity id = Context.User.Identity;
            if (id.IsAuthenticated)
            {
                var roles = new UserRepository().GetRoles(id.Name);
                Context.User = new GenericPrincipal(id, roles);
            }
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }
    }
}