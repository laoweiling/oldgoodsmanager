using System;
using System.Collections.Generic;
using System.Linq;
using OldGoodsManage.Models;

namespace OldGoodsManage.Helper
{
    /// <summary>
    /// 枚举，表示系统的功能
    /// </summary>
    public enum FunctionName
    {
        UserManage,        //后台用户管理
        CustomerManage,    //前台用户管理
        SchoolManage,      //校区管理
        MessageManage,     //消息管理
        AuthorizeManage	,  //权限管理
        RoleManage	,      //角色管理
        FunctionManage,    //功能管理
        CompanyInfoManage, //公司信息管理
        GoodManage	,      //商品管理
        RecruitmentManage, //反馈信息管理

    };
    /// <summary>
    /// 权限类
    /// </summary>
    public class CheckAuthorize
    {
        private static OldGoodsManageEntities db = new OldGoodsManageEntities();

        /************************************************************************
       函数名称： GetFunction
       函数作用： 找出当前用户所拥有      
       传入参数：当前用户的ID
       返回参数：权限列表
       修改记录： 作者        时间        原因 
       *         李康志      2015.1.30    create
       *         李康志      2015.3.24    修改函数功能
      * **********************************************************************/
        public static List<string>  GetFunctionName(long userID)
        {
            //根据登录者的ID找出角色ID
            long roleID = db.t_User.Where(u => u.userID == userID).Select(u => u.roleID).FirstOrDefault();
            //根据角色ID查找出功能ID
            List<long?> functionIdList =db.t_Role_Function.Where(rf => rf.roleID == roleID).Select(rf => rf.functionID).ToList();
            List<String> functionNameList = new List<string>();
            //遍历列表，根据id找出功能名,如果功能名不为空并且等于传进来的功能名，则立刻返回true
            for (int i = 0; i < functionIdList.Count; i++)
            {
                long Id = Convert.ToInt64(functionIdList[i]);
                string funcName = db.t_Function.Where(f => f.functionID == Id).Select(f => f.functionName).FirstOrDefault();

                functionNameList.Add(funcName);                        
            }
            return functionNameList;
        }


        /************************************************************************
      函数名称： IsChecked
      函数作用： 判断当前用户是否拥有当前操作的权限
      传入参数：当前用户的ID
      返回参数：true 或者false
      修改记录： 作者        时间        原因 
      *         李康志      2015.3.24    create
     * **********************************************************************/
        public static bool IsChecked(List<string> functionNameList,FunctionName functionName)
        {
          //  List<string> functionNameList=Session
            foreach (var funcName in functionNameList)
            {
                if (funcName!=null && funcName==functionName.ToString())
                {
                    //通过权限
                    return true;
                }
            }
            //没有该权限。
            return false;
        }
    }
}