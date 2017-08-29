using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OldGoodsManage.Models;

namespace OldGoodsManage.Repositories
{
    public class UserRepository
    {
        //创建一个操作数据的上下文对象
        static OldGoodsManageEntities db = new OldGoodsManageEntities();
        //找出所有的用户和角色名字
        private static List<t_User> listUsers = db.t_User.ToList();
        private static List<t_Role> listRoles = db.t_Role.ToList();
       
        /// <summary>
        /// 验证数据库是否有该用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName,string password)
        {
            return listUsers.Any(u => u.loginName == userName && u.password == password);
        }

        /// <summary>
        /// 根据用户名找出该用户的角色
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string[] GetRoles(string userName)
        {
            string[] roles=new string [1] ;
            //根据用户名找出用户的角色ID
            long roleId = listUsers.Where(u => u.loginName == userName)
                .Select(u => u.roleID).FirstOrDefault();
            //根据角色的Id找出角色名
            string role = listRoles.Where(r =>r.roleID== roleId)
                .Select(r => r.roleName)
                .FirstOrDefault();
            //将该角色名放到一个数组里面并且返回该角色的数组
            roles[0] = role;
            return roles;
        }
    }
}