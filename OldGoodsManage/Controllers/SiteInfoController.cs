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
    public class SiteInfoController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();
 

        public ActionResult Index()
        {
            return View(db.t_SiteInfo.ToList());
        }

        
        public ActionResult Edit(long id = 0)
        {
            t_SiteInfo t_siteinfo = db.t_SiteInfo.Single(t => t.siteInfoID == id);
            if (t_siteinfo == null)
            {
                return HttpNotFound();
            }
            return View(t_siteinfo);
        }

        //
        // POST: /SiteInfo/Edit/5

        [HttpPost]
        public ActionResult Edit(t_SiteInfo t_siteinfo)
        {
            if (ModelState.IsValid)
            {
                db.t_SiteInfo.Attach(t_siteinfo);
                db.ObjectStateManager.ChangeObjectState(t_siteinfo, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(t_siteinfo);
        }
 
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}