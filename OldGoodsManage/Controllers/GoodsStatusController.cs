using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Filters;
using OldGoodsManage.Models;
using PagedList;


namespace OldGoodsManage.Controllers
{
    public enum GoodsStatus
    {
        AllGoods = -1,
        WaitToCheck=0,    //等待审核，0
        FailInCheck=1,    //审核未通过，1
        HaveReleased=2,   //已发布，2
        OffShelf=3,       //下架，3
        Frozen=4         //已冻结,4
    }


    public class GoodsStatusController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        /************************************************************************
        函数名称： Index
        函数作用：显示所有的商品审核状态
        传入参数： page:第几页,默认为第一页
        返回参数： 返回商品审核状态
        修改记录： 作者        时间        原因 
        *         王俊伟    2015.10.15     create
       * **********************************************************************/
        public ActionResult Index(int page = 1,int status = -1)
        {
            List<t_Goods> listGoods = null;
            //显示所有的商品
            if (status == -1)
            {
                listGoods = (from goods in db.t_Goods
                    orderby goods.releaseTime
                    select goods).ToList(); //每页10个商品
            }
            //显示待审核的商品
            else if (status == (int)GoodsStatus.WaitToCheck)
            {
                listGoods = (from goods in db.t_Goods
                    where goods.goodsStatus == (int) GoodsStatus.WaitToCheck
                    orderby goods.releaseTime
                    select goods).ToList();
            }
            //显示审核不通过的商品  
            else if (status == (int)GoodsStatus.FailInCheck)
            {
                listGoods = (from goods in db.t_Goods
                    where goods.goodsStatus == (int) GoodsStatus.FailInCheck
                    orderby goods.releaseTime
                    select goods).ToList();
            }
            //显示已发布的商品，可进行下架或冻结操作
            else if(status == (int)GoodsStatus.OffShelf || status == (int)GoodsStatus.Frozen)
            {
                listGoods = (from goods in db.t_Goods
                    where goods.goodsStatus == (int) GoodsStatus.HaveReleased
                    orderby goods.releaseTime
                    select goods).ToList();
            }
 
            //传递给页面以判断是哪种审核方式
            ViewData["temp"] = status;
            //对商品进行分页，每页10个商品
            var allGoods = listGoods.ToPagedList(page,10);
            return View(allGoods);
        }


        /************************************************************************
        函数名称： Index
        函数作用：对商品进行审核
        修改记录： 作者        时间        原因 
        *          王屏    2015.1.15      create
         *        王俊伟   2015.10.15     修改本方法的GET和POST分离开
       * **********************************************************************/
        [HttpPost]
        [UserFilter]
        public ActionResult Index(FormCollection form, int status = 0)
        {
            string strSubmit = form["submit"];
            string strCheck = form["chkId"];
            List<t_Goods> list = null;

            #region 删除商品

            if (strSubmit == "删除")
            {
                if (strCheck == null)
                {
                    return
                        Content(
                            String.Format("<script>alert('请选择数据！');location.href='{0}'</script>",
                                Url.Action("Index", "GoodsStatus")), "text/html");
                }
                string[] temp = strCheck.Split(',');

                int num = 0;
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        long id = int.Parse(temp[i]);
                        var data = db.t_Goods.SingleOrDefault(d => d.goodsID == id);
                        db.t_Goods.DeleteObject(data);
                        num = db.SaveChanges();
                        num++;
                    }

                    if (num > 0)
                    {
                        return
                            Content(
                                String.Format("<script>alert('删除成功！');location.href='{0}'</script>",
                                    Url.Action("Index", "GoodsStatus")), "text/html");
                    }
                    return
                        Content(
                            String.Format("<script>alert('失败，稍后再操作！');location.href='{0}'</script>",
                                Url.Action("Index", "GoodsStatus")), "text/html");
                }
            }

            #endregion

            #region 商品审核

            if (strSubmit == "审核通过" || strSubmit == "冻结商品" || strSubmit == "下架商品")
            {
                if (strCheck == null)
                {
                    return  Content(String.Format("<script>alert('请选择数据！');location.href='{0}'</script>",Url.Action("Index", "GoodsStatus")), "text/html");
                }
                string[] temp = strCheck.Split(',');

                int num = 0;
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        long id = int.Parse(temp[i]);
                        var data = db.t_Goods.SingleOrDefault(d => d.goodsID == id);
                        if (strSubmit == "审核通过")
                        {
                            data.goodsStatus = 2;
                        }
                        else if (strSubmit == "冻结商品")
                        {
                            data.goodsStatus = 4;
                        }
                        else if (strSubmit == "下架商品")
                        {
                            data.goodsStatus = 3;
                        }
                        num = db.SaveChanges();
                        num++;
                    }

                    if (num > 0)
                    {
                        return
                            Content(
                                String.Format("<script>alert('修改成功！');location.href='{0}'</script>",
                                    Url.Action("Index", "GoodsStatus")), "text/html");
                    }
                    return
                        Content(
                            String.Format("<script>alert('失败，稍后再操作！');location.href='{0}'</script>",
                                Url.Action("Index", "GoodsStatus")), "text/html");
                }
            }
            #endregion
            return View();
        }
    }
}
