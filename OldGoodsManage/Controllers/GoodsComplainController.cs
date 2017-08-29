using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Models;

namespace OldGoodsManage.Controllers
{
    public class GoodsComplainController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        //
        // GET: /GoodsComplain/

        public ActionResult  Index(FormCollection form)
        {
            string strSubmit = form["submit"];
            string strCheck = form["chkId"];
            List<Models.t_GoodsComplain> list = null;
            #region 删除商品
            if (strSubmit == "删除")
            {
                if (strCheck == null)
                {
                    return Content(String.Format("<script>alert('请选择数据！');location.href='{0}'</script>", Url.Action("Index", "GoodsComplain")), "text/html");
                }
                string[] temp = strCheck.Split(',');

                int num = 0;
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        long id = int.Parse(temp[i]);
                        var data = db.t_GoodsComplain.SingleOrDefault(d => d.goodsID == id);
                        db.t_GoodsComplain.DeleteObject(data);
                        num = db.SaveChanges();
                        num++;
                    }

                    if (num > 0)
                    {
                        return Content(String.Format("<script>alert('删除成功！');location.href='{0}'</script>", Url.Action("Index", "GoodsComplain")), "text/html");
                    }
                    return Content(String.Format("<script>alert('失败，稍后再操作！');location.href='{0}'</script>", Url.Action("Index", "GoodsComplain")), "text/html");
                }


            }
          
         list =(from u in db.t_GoodsComplain select u).ToList();
         ViewBag.DataList =  list;
         return View();
        }
            #endregion
        //
        // GET: /GoodsComplain/Details/5


        public ActionResult Details(long id = 0)
        {
            t_GoodsComplain t_GoodsComplain = db.t_GoodsComplain.Single(t => t.goodsComplainID == id);
            if (t_GoodsComplain == null)
            {
                return HttpNotFound();
            }
            return View(t_GoodsComplain);
        }

        //
        // GET: /GoodsComplain/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /GoodsComplain/Create

        [HttpPost]
        public ActionResult Create(t_GoodsComplain t_GoodsComplain)
        {
            if (ModelState.IsValid)
            {
                db.t_GoodsComplain.AddObject(t_GoodsComplain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(t_GoodsComplain);
        }

        //
        // GET: /GoodsComplain/Edit/5

        public ActionResult Edit(long id = 0)
        {
            t_GoodsComplain t_GoodsComplain = db.t_GoodsComplain.Single(t => t.goodsComplainID == id);
            if (t_GoodsComplain == null)
            {
                return HttpNotFound();
            }
            return View(t_GoodsComplain);
        }

        //
        // POST: /GoodsComplain/Edit/5

        [HttpPost]
        public ActionResult Edit(t_GoodsComplain t_GoodsComplain)
        {
            if (ModelState.IsValid)
            {
                db.t_GoodsComplain.Attach(t_GoodsComplain);
                db.ObjectStateManager.ChangeObjectState(t_GoodsComplain, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(t_GoodsComplain);
        }

        //
        // GET: /GoodsComplain/Delete/5

        public ActionResult Delete(long id = 0)
        {
            t_GoodsComplain t_GoodsComplain = db.t_GoodsComplain.Single(t => t.goodsComplainID == id);
            if (t_GoodsComplain == null)
            {
                return HttpNotFound();
            }
            return View(t_GoodsComplain);
        }

        //
        // POST: /GoodsComplain/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            t_GoodsComplain t_GoodsComplain = db.t_GoodsComplain.Single(t => t.goodsComplainID == id);
            db.t_GoodsComplain.DeleteObject(t_GoodsComplain);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}