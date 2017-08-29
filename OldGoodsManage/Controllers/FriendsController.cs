using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Filters;
using OldGoodsManage.Models;

namespace OldGoodsManage.Controllers
{
    public class FriendsController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

      //  /************************************************************************
      // 函数名称： MyFriends
      // 函数作用：找出当前用户的所有好友
      // 传入参数：无
      // 返回参数：无
      // 修改记录： 作者          时间         原因 
      // *          王俊伟        2015.9.10   create
      //* **********************************************************************/
      //  [CustomerFilter]
      //  public ActionResult MyFriends()
      //  {
      //      long customerId = Int64.Parse(Session["CustomerId"].ToString());
      //      找出当前登录账号的所有好友Id
      //      List<t_Friends> friendsList = (from friends in db.t_Friends
      //                                      where friends.hostId == customerId
      //                                      select friends).ToList();

      //      将所有的好友存放在List中
      //      List<t_Customer> listFriends = new List<t_Customer>();
      //      foreach (var friends in friendsList)
      //      {
      //          t_Customer t_customer = db.t_Customer.Single(t => t.customerID == friends.friendsId);
      //          listFriends.Add(t_customer);
      //      }
      //      ViewBag.listFriends = listFriends;

      //      return View();
      //  }

      //  /************************************************************************
      // 函数名称： MyFriends
      // 函数作用：添加好友(双向添加好友)
      // 传入参数：通过登录名loginName来双向添加好友
      // 返回参数：true则添加好友成功，false则添加好友失败
      // 修改记录： 作者          时间         原因 
      // *          王俊伟        2015.9.10   create
      //* **********************************************************************/
      //  public bool AddFriends(string loginName)
      //  {
      //      当前用户的数据库记录
      //      t_Friends host = new t_Friends();

      //      我的好友的数据库记录
      //      t_Friends friends = new t_Friends();

      //      host.hostId = Int64.Parse(Session["CustomerId"].ToString());
      //      try
      //      {
      //          long friendsId = db.t_Customer.Single(f => f.loginName == loginName).customerID;
      //          不能添加自己为好友
      //          if (host.hostId == friendsId)
      //          {
      //              return false;
      //          }
                 
      //          host.friendsId = friendsId;

      //          数据库记录恰好相反
      //          friends.hostId = host.friendsId;
      //          friends.friendsId = host.hostId;

      //          if (ModelState.IsValid)
      //          {
      //              List<t_Friends> friendsList = (from t_friends in db.t_Friends
      //                                             where (t_friends.friendsId == host.friendsId && t_friends.hostId == host.hostId)
      //                                             select t_friends).ToList();
      //              if (friendsList.Count!=0)
      //              {
      //                  return false;
      //              }

      //              双向添加好友
      //              db.t_Friends.AddObject(host);
      //              db.t_Friends.AddObject(friends);
      //              db.SaveChanges();
      //          }
      //      }
      //      catch (Exception)
      //      {
      //          return false;
      //      }
      //      return true;
      //  }
          
      //  删除好友
      //  public ActionResult DeleteFriends(long friendsId)
      //  {
      //      long hostId = Int64.Parse(Session["CustomerID"].ToString());
      //      双向的删除好友
      //      t_Friends host = db.t_Friends.Single(t => (t.hostId == hostId && t.friendsId == friendsId));
      //      t_Friends friends = db.t_Friends.Single(t => (t.hostId == friendsId && t.friendsId == hostId));
      //      保存到数据库
      //      db.t_Friends.DeleteObject(host);
      //      db.t_Friends.DeleteObject(friends);
      //      db.SaveChanges();
      //      return RedirectToAction("Myfriends");
      //  }



      //  分享商品给好友
      //  [HttpPost]
      //  public bool ShareGoodsToFrds(string goodsId,long friendsId)
      //  { 
      //      long hostId = Int64.Parse(Session["CustomerId"].ToString());
      //      当前用户的数据库记录
      //      t_Friends host = db.t_Friends.Single(f => (f.hostId == hostId && f.friendsId == friendsId));

      //      当前用户的好友的数据库记录(正好相反)
      //      t_Friends friends = db.t_Friends.Single(f => (f.hostId == friendsId && f.friendsId == hostId));

      //      将当前用户分享给好友的商品以 '，'分隔开来,放在当前用户的数据库记录中
      //      if (!string.IsNullOrEmpty(host.goodsToFriends))
      //      {
      //          host.goodsToFriends = host.goodsToFriends + "," + goodsId;
      //      }
      //      else
      //      {
      //          host.goodsToFriends = goodsId;
      //      }     
            
      //      将当前用户分享给好友的商品以 '，'分隔开来，放在好友的数据库记录中
      //      if (!string.IsNullOrEmpty(friends.goodsFromFriends))
      //      {
      //          friends.goodsFromFriends = friends.goodsFromFriends + "," + goodsId;
      //      }
      //      else
      //      {
      //          friends.goodsFromFriends = goodsId;
      //      }

      //      将修改的值保存到数据库中
      //      if (ModelState.IsValid)
      //      {
      //          db.ObjectStateManager.ChangeObjectState(host, EntityState.Modified);
      //          db.ObjectStateManager.ChangeObjectState(friends, EntityState.Modified);
      //          db.SaveChanges();
      //          return true;
      //      }
      //      return false;
      //  }

      //  当前用户分享给好友的商品页面
      //  public ActionResult ShareGoodsToFrds()
      //  {
      //      long hostId = Int64.Parse(Session["CustomerId"].ToString());
      //      获取所有的好友列表
      //      List<t_Friends> friendsList = GetFriendsList(hostId);

      //      将来自好友分享的商品放在dictionary里
      //      Dictionary<long, t_Goods> dicGoods = new Dictionary<long, t_Goods>();
      //      将我的好友的信息放入字典中
      //      Dictionary<long, t_Customer> dicCustomer = new Dictionary<long, t_Customer>();


      //      foreach (t_Friends t_friends in friendsList)
      //      {
      //          将我的好友的信息放入字典中
      //          dicCustomer[t_friends.friendsId] = db.t_Customer.Single(c => c.customerID == t_friends.friendsId);

      //          若没有好友分享给我商品则不需要放入dictionary
      //          if (string.IsNullOrEmpty(t_friends.goodsToFriends))
      //          {
      //              continue;
      //          }
      //          以','分隔将商品id取出来
      //          string[] goods = t_friends.goodsToFriends.Split(',');
      //          foreach (string goodsId in goods)
      //          {
      //              long eachGoodsId = Int64.Parse(goodsId);
      //              dicGoods[eachGoodsId] = db.t_Goods.Single(g => g.goodsID == eachGoodsId);
      //          }
      //      }
      //      ViewBag.friendsList = friendsList;
      //      ViewBag.dicGoods = dicGoods;
      //      ViewBag.dicCustomer = dicCustomer;

      //      return View();
      //  }

      //  来自好友分享的商品
      //  public ActionResult ShareGoodsFromFrds()
      //  {
      //      long hostId = Int64.Parse(Session["CustomerId"].ToString());
      //      获取所有的好友列表
      //      List<t_Friends> friendsList = GetFriendsList(hostId);
      //      将来自好友分享的商品放在dictionary里
      //      Dictionary<long, t_Goods> dicGoods = new Dictionary<long, t_Goods>();
      //      将我的好友的信息放入字典中
      //      Dictionary<long, t_Customer> dicCustomer = new Dictionary<long, t_Customer>();


      //      foreach (t_Friends t_friends in friendsList)
      //      {
      //          将我的好友的信息放入字典中
      //          dicCustomer[t_friends.friendsId] = db.t_Customer.Single(c => c.customerID == t_friends.friendsId);

      //          若没有好友分享给我商品则不需要放入dictionary
      //          if (string.IsNullOrEmpty(t_friends.goodsFromFriends))
      //          {
      //              continue;
      //          }
                
      //          以','分隔将商品id取出来
      //          string[] goods = t_friends.goodsFromFriends.Split(',');
      //          foreach (string goodsId in goods)
      //          {
      //              long eachGoodsId = Int64.Parse(goodsId);
      //              dicGoods[eachGoodsId] = db.t_Goods.Single(g => g.goodsID == eachGoodsId);
      //          }
      //      }
      //      ViewBag.friendsList = friendsList;
      //      ViewBag.dicGoods = dicGoods;
      //      ViewBag.dicCustomer = dicCustomer;

      //      return View();
      //  }

      //  通过当前用户的Id获取好友列表
      //  public List<t_Friends> GetFriendsList(long hostId) 
      //  {
      //      return (from t_friends in db.t_Friends
      //              where t_friends.hostId == hostId
      //              select t_friends).ToList();
      //  }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}