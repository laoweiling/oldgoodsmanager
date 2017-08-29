using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OldGoodsManage.Models;
using System.IO;
using PagedList;
using OldGoodsManage.Filters;

namespace OldGoodsManage.Controllers
{
    public class GoodsController : Controller
    {
        private OldGoodsManageEntities db = new OldGoodsManageEntities();

        public ActionResult EditGoods(long id = 0)
        {
            //根据商品ID获取商品的相关信息
            t_Goods t_goods = db.t_Goods.Single(t => t.goodsID == id);
            if (t_goods == null)
            {
                return HttpNotFound();
            }
            //设置商品类型,并设定应该选择的项目
            SetGoodsType(0, t_goods.categoryID);
            SetSchoolType(t_goods.schoolID);
            t_goods.goodsImage = t_goods.goodsImage + ",";  //此处是为了使editGoods中图片路径的分隔后数组不越界特意添加
            return View(t_goods);
        }

        [HttpPost]
        public ActionResult EditGoods(t_Goods t_goods)
        {
            if (ModelState.IsValid)
            {
                //QQ和Tel至少要填写一项
                if (null == t_goods.contactQQ && null == t_goods.contactTel)
                {
                    ModelState.AddModelError("contactQQ", "QQ和TEL，请务必填写一项！");
                    SetGoodsType(0, t_goods.categoryID);
                    SetSchoolType(t_goods.schoolID);
                    return View(t_goods);
                }

                //检测用户上传的图像是否合理
                List<HttpPostedFileBase> fileList = new List<HttpPostedFileBase>();
                //检测图片大小不能超过1M
                if (Request.Files.Count != 0)
                {
                    fileList.Add(Request.Files["image1"]);
                    fileList.Add(Request.Files["image2"]);
                    fileList.Add(Request.Files["image3"]);
                    if (!IsValidImage(fileList))
                    {
                        SetGoodsType(0, t_goods.categoryID);
                        SetSchoolType(t_goods.schoolID);
                        return View(t_goods);
                    }
                }
                //上传图片
                string strImagePath = UploadImage(fileList, t_goods.goodsID);  
                //重新获取图片路径
                strImagePath = "";
                string strServerPath = Request.PhysicalApplicationPath.ToString() + "Content\\Images\\" + t_goods.goodsID.ToString();
                string[] filenames = Directory.GetFiles(strServerPath);
                string strFile = "";
                foreach( string files in filenames){
                    strFile = files.Replace(strServerPath+"\\","");
                    strImagePath = strImagePath + "Content\\Images\\" + t_goods.goodsID.ToString() + "\\" + strFile + ",";
                }
                t_goods.goodsImage = strImagePath;
                t_goods.categoryID = long.Parse(Request.Form["GoodsSubType"]);
                t_goods.schoolID = long.Parse(Request.Form["SchoolType"]);
                //修改数据库
                db.t_Goods.Attach(t_goods);
                db.ObjectStateManager.ChangeObjectState(t_goods, EntityState.Modified);
                if (1 == db.SaveChanges())
                {
                    //提示添加成功
                    ModelState.AddModelError("remarks", "商品修改成功!");
                    return Content(String.Format("<script>alert('商品修改成功！');location.href='{0}'</script>", Url.Action("EditGoods", "Goods")), "text/html");
                   
                }
                else
                {
                    //提示添加失败，请用户联系管理员处理
                    ModelState.AddModelError("remarks", "商品修改失败，请联系管理员进行处理!");
                    return Content(String.Format("<script>alert('商品修改失败，请联系管理员进行处理！');location.href='{0}'</script>", Url.Action("EditGoods", "Goods")),"text/html");
                }

            }

            //根据小类ID号得到大类ID号
            SetGoodsType(0, t_goods.categoryID);
            SetSchoolType(t_goods.schoolID);
            return View(t_goods);
        }

        //
        // GET: /Goods/
        [CustomerFilter]
        public ActionResult AddGoods()
        {
            SetGoodsType();
            SetSchoolType();
            return View();
        }


        [HttpPost]
        public ActionResult AddGoods(t_Goods t_goods)
        {
            if (ModelState.IsValid)
            {
                //检测商品小类是否为空，如果为空提示需要联系管理员
                if (null == Request.Form["GoodsSubType"])
                {
                    SetGoodsType();
                    SetSchoolType();
                    return View(t_goods);
                }

                //QQ和Tel至少要填写一项
                if (null == t_goods.contactQQ && null == t_goods.contactTel)
                {
                    ModelState.AddModelError("contactQQ", "QQ和TEL，请务必填写一项！");
                    SetGoodsType();
                    SetSchoolType();
                    return View(t_goods);     
                }

                List<HttpPostedFileBase> fileList = new List<HttpPostedFileBase>();
                //检测图片大小不能超过1M，是否至少上传了一张图片
                if (Request.Files.Count != 0)
                {
                    fileList.Add(Request.Files["image1"]);
                    fileList.Add(Request.Files["image2"]);
                    fileList.Add(Request.Files["image3"]);
                    if (!IsValidImage(fileList) || !IsValidImageCount(fileList))
                    {
                        SetGoodsType();
                        SetSchoolType();
                        return View(t_goods);
                        
                    }
                }
               
                //将对应t_goods存放到数据库中
                //求出最大的goodsID并加1作为新记录的ID
                long iMaxID = (from d in db.t_Goods select d.goodsID).Max();
                iMaxID = iMaxID + 1;
                t_goods.goodsID = iMaxID;
                t_goods.goodsStatus = 0;  // 0:待审核，1:审核未通过，2:已发布，3:已下架，4:已锁定
                t_goods.categoryID = long.Parse(Request.Form["GoodsSubType"]);  //存放小类的categoryID
                t_goods.customerID = long.Parse(Session["CustomerID"].ToString());   //录入信息的用户ID
                t_goods.releaseTime = DateTime.Now;  //存放当前录入的时间
                t_goods.validTime = 30;  //有效期为30天
                t_goods.schoolID = long.Parse(Request.Form["SchoolType"]);       //此处存放当前学校的ID，代码待后续补充
                t_goods.goodsImage = UploadImage(fileList, iMaxID);  //图片存放路径
                db.t_Goods.AddObject(t_goods);
                if (1 == db.SaveChanges())
                {
                    //提示添加成功
                    return Content(String.Format("<script>alert('商品发布成功，等待管理员审核！');location.href='{0}'</script>", Url.Action("AddGoods", "Goods")), "text/html");
                }
                else
                {
                    //提示添加失败，请用户联系管理员处理
                    //ModelState.AddModelError("remarks", "商品发布失败，请联系管理员进行处理!");
                    return Content(String.Format("<script>alert('商品发布失败，请联系管理员进行处理！');location.href='{0}'</script>", Url.Action("AddGoods", "Goods")), "text/html");
                }
                
            }
            SetGoodsType();
            SetSchoolType();
            return View(t_goods);
        }




        /************************************************************************
        函数名称： ReadGoods
        函数作用：读取一页商品信息并显示，可以读取全部商品、读取某个类目下的商品、
         *        读取某个校区下个商品和搜索出来的商品。
        传入参数： id:              第几页         categoryID:要读取的商品类别 
                   schoolId:要读取的学校ID         searchName:搜索的商品名称
        返回参数： 返回读取的商品商品页面
        修改记录： 作者        时间        原因 
        *         王俊伟    2015.9.24     create
         *        王俊伟    2015.9.29     获取校区名称
       * **********************************************************************/
        public ActionResult ReadGoods(int id = 1, long categoryId = 0, long schoolId=0,string searchName="")
        {   
            //显示所有的商品
            List<t_Goods> goods = ReadAllGoods();
            //一页共有16个商品
            int pageSize = 16;

            //显示某个类目下的所有商品
            if (categoryId != 0)
            {
                goods = ReadCategoryGoods(categoryId);
            }
            //显示校区商品
            else if (schoolId != 0)
            {
                goods = ReadSchGoods(schoolId);
                //获取校区名称以便在页面显示
                Session["SchoolName"] = db.t_School.Single(s => s.schoolID == schoolId).schoolName;
            }
            //显示搜索得到的商品
            else if (!String.IsNullOrEmpty(searchName))
            {
                goods = ReadSearchGoods(searchName);
            }

            //对商品进行分页
            var pagedGoods = goods.ToPagedList(pageNumber: id, pageSize: pageSize);
            return View(pagedGoods);
        }


        /************************************************************************
        函数名称： ReadAllGoods
        函数作用：按时间顺序读取数据库中所有商品
        传入参数： 无
        返回参数： 读取的商品表
        修改记录： 作者        时间        原因 
        *         王俊伟     2015.6.5     create
         *        王俊伟     2015.10.14   修改成值从数据库中读取已审核通过的商品
       * **********************************************************************/
        public List<t_Goods> ReadAllGoods()
        {
            var allGoods = (from goods in db.t_Goods
                            orderby goods.releaseTime descending 
                            where goods.goodsStatus == 2//2代表商品审核已通过
                            select goods).ToList();
            //只读取第一张图片
            foreach (t_Goods goods in allGoods)
            {
                goods.goodsImage = goods.goodsImage.Split(',')[0];
            }
            return allGoods;
        }

        /************************************************************************
        函数名称： ReadCateGoods
        函数作用：按商品分类读取数据库中的商品
        传入参数： categoryID按照商品分类ID搜索 goods传入已有的商品表
        返回参数： 读取得到商品表
        修改记录： 作者        时间        原因 
        *         王俊伟    2015.6.5      create
       * **********************************************************************/
        public List<t_Goods> ReadCategoryGoods(long categoryID)
        {
            //读取categoryID对应的数据库记录
            t_Category category = db.t_Category.Single(a => a.categoryID == categoryID);
            List<t_Goods> categoryGoods = null;
            //若商品的类别为一级，则读取出其所有的二级分类下的商品
            if (category.parentID == 0)
            {
                categoryGoods = (from good in db.t_Goods
                                     from cate in db.t_Category
                                     where (good.categoryID == cate.categoryID) && (cate.parentID == category.categoryID) && (good.goodsStatus == 2)//2代表商品审核已通过
                                     orderby good.releaseTime
                                     select good).ToList();                
            }
            //若商品的类别为二级，则读取出其所有的商品
            else
            {
                categoryGoods = (from good in db.t_Goods
                                 where (good.categoryID == category.categoryID) && (good.goodsStatus == 2)//2代表商品审核已通过
                                 orderby good.releaseTime
                                 select good).ToList();
            }

            //只读取第一张图片
            foreach (t_Goods goods in categoryGoods)
            {
                goods.goodsImage = goods.goodsImage.Split(',')[0];
            }

            return categoryGoods;
        } 
   



        /************************************************************************
        函数名称： ReadSchGoods
        函数作用：按学校读取数据库中的商品
        传入参数： schoolName按照学校名称搜索 goods传入已有的商品表
        返回参数： 读取的商品表
        修改记录： 作者        时间        原因 
        *         王俊伟     2015.6.5       create
       * **********************************************************************/
        public List<t_Goods> ReadSchGoods(long schoolId)
        {
            //从数据库中读取的学校ID为schoolID的所有商品
            List<t_Goods> schoolGoods = (from goods in db.t_Goods
                                         where (goods.schoolID == schoolId) && (goods.goodsStatus == 2)//2代表商品审核已通过
                                         orderby goods.releaseTime
                                         select goods).ToList();

            //只读取第一张图片
            foreach (t_Goods goods in schoolGoods)
            {
                goods.goodsImage = goods.goodsImage.Split(',')[0];
            }

            return schoolGoods;
        }

        /************************************************************************
        函数名称： SearchGoods
        函数作用： 填充首页搜索栏及校区的数据
        传入参数： 无
        返回参数： 读取的商品表
        修改记录： 作者        时间        原因 
        *         王俊伟     2015.9.24     create
       * **********************************************************************/
        public ActionResult SearchGoods()
        {

            #region 从读取所有学校记录并填充学校下拉框
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (t_School item in db.t_School)
            {
                items.Add(new SelectListItem { Text = item.schoolName.ToString(), Value = item.schoolID.ToString() });
            }

            //将学校名称放入首页校区处,若不通过Session传值则学校名称会丢失
            ViewBag.School = Session["SchoolName"];
            ViewBag.DropDownList = items; 
            #endregion

            return View();
        }

        //获取要搜索得到的商品列表
        public List<t_Goods> ReadSearchGoods(string searchName)
        {
            List<t_Goods> goods = (from g in db.t_Goods
                                   where g.goodsTitle.Contains(searchName) && (g.goodsStatus ==2)
                                   select g).ToList();
            return goods;
        }

        /************************************************************************
        函数名称： UploadImage
        函数作用：上传图片，并返回图片路径（多个图片路径以,分隔）
        传入参数：所有file对象的list
        返回参数：无
        修改记录： 作者        时间        原因 
        *          lulu        2015.1.20   create
       * **********************************************************************/
        public string UploadImage(List<HttpPostedFileBase> fileList,long id)
        {
            //将图片上传到服务器Images文件夹下，先建立以goodsID命令的文件夹，然后存放图片的相对路径
            //图片默认放在Content/Images下面
          
            //建立以iMaxID为名称的文件夹,如果该文件夹已经存在，则要删除该文件夹的所有内容
            string strServerPath = Request.PhysicalApplicationPath.ToString() + "Content\\Images\\" + id.ToString();
            if (!Directory.Exists(strServerPath))
            {
                 //DirectoryInfo dImagePath = new DirectoryInfo(strServerPath);
                 //DeleteFile(dImagePath);
                 Directory.CreateDirectory(strServerPath);
            }
            //上传文件，图片以001 002 003命名
            string strExtension = "";
            string strFileName = "";
            string strImagePath = "";
            int i = 0;
            foreach (var file in fileList)
            {
                i++;  //表示当前是第几张图片，此处一定要这么用，兼容编辑模式下的图片上传（编辑模式下filename为空，说明该图片已经存在文件夹中了）
                if ("" == file.FileName)
                {
                    continue;
                }
                strExtension = Path.GetExtension(file.FileName);
                strFileName = string.Format("00{0}", i) + strExtension;
                strImagePath = strImagePath + "Content\\Images\\" + id.ToString() + "\\" + strFileName + ",";
                file.SaveAs(Path.Combine(strServerPath, strFileName));
            }
            return strImagePath;
        }

        /************************************************************************
        函数名称： IsValidImage
        函数作用：此处主要检测图片是否合法，包括至少包含一张图片，图片的大小不能超过1M
        传入参数：所有file对象的list
        返回参数：true or false
        修改记录： 作者        时间        原因 
        *          lulu        2015.1.20   create
       * **********************************************************************/
        public bool IsValidImage(List<HttpPostedFileBase> fileList)
        {
            int size = 0;
            int j = 0;  //表示当前一共上传了几张图片
            string strTemp;
            //先检测是否有超过1M的图片，如果有，提示无法上传
            foreach (var file in fileList)
            {
                if ("" == file.FileName)
                {
                    continue;
                }
                j++;
                size = Convert.ToInt32(file.ContentLength / (1024*1024));
                if (size >= 1)
                {
                    strTemp = "第{0}图片大小超过1M，请重新上传图片!";
                    strTemp = string.Format(strTemp, j);    
                    ModelState.AddModelError("remarks",strTemp);
                    return false;
                }
            }
          
            return true;
        }

        /************************************************************************
      函数名称： IsValidImageCount
      函数作用：此处主要检测图片个数是否至少上传一张
      传入参数：所有file对象的list
      返回参数：true or false
      修改记录： 作者        时间        原因 
      *          lulu        2015.1.20   create
     * **********************************************************************/
        public bool IsValidImageCount(List<HttpPostedFileBase> fileList)
        {
            int j = 0;
            foreach (var file in fileList)
            {
                if ("" == file.FileName)
                {
                    continue;
                }
                j++;    
            }
            //至少要上传一张图片
            if (0 == j)
            {
                ModelState.AddModelError("remarks", "请至少上传一张图片！");
                return false;
            }
            return true;
        }

        /************************************************************************
      函数名称： DeleteFile
      函数作用：删除某个文件夹下所有的文件及子目录下所有的文件
      传入参数：该文件夹的对象
      返回参数：无
      修改记录： 作者        时间        原因 
      *          lulu        2015.1.20   create
      * **********************************************************************/
        public void DeleteFile(DirectoryInfo path)
        {  
            foreach (System.IO.DirectoryInfo d in path.GetDirectories())  
            {
                DeleteFile(d);  
            }  
            foreach (System.IO.FileInfo f in path.GetFiles())  
            {  
                f.Delete();  
            }  
        }

        /************************************************************************
       函数名称： SetSchoolType()
       函数作用：填充选择学校的dropdownlist, 如果传入的参数不为零，则表示该值为当前选中的值
       传入参数：默认选择的大类商品ID和小类商品ID
       返回参数：无
       修改记录： 作者        时间        原因 
       *          lulu        2015.1.27   create
       * **********************************************************************/
        public void SetSchoolType(long schoolID = 0)
        {
            //从数据库中读出所有的数据
            Dictionary<long, string> dictSchool = null;
            dictSchool = db.t_School.ToDictionary(u => u.schoolID, u => u.schoolName);
            ViewBag.SchoolType = GetGoodsList(dictSchool, schoolID);
        }


        /************************************************************************
      函数名称： SetGoodsType()
      函数作用：将发布商品页面上商品类型的DropDownList进行填充
      传入参数：默认选择的大类商品ID和小类商品ID
      返回参数：无
      修改记录： 作者        时间        原因 
      *          lulu        2015.1.22   create
      * **********************************************************************/
        public void SetGoodsType(long GoodsTypeID = 0, long subGoodsTypeID = 0)
        {
            //先填充商品分类的dropdownlist,获取所有的以及目录并动态填写到Dropdownlist GoodsType中
            long iParentID = 0;
            Dictionary<long, string> ParentTypeDict = GetGoodsType(iParentID);
            if (0 == ParentTypeDict.Count)
            {
                ModelState.AddModelError("remarks", "请联系管理员添加商品分类信息，否则不能发布商品！");
                //此处提示请先添加商品分类
                ViewBag.GoodsType = GetGoodsList(ParentTypeDict, GoodsTypeID);
                ViewBag.GoodsSubType = GetGoodsList(ParentTypeDict, subGoodsTypeID);
                return;
            }
            if (0 != subGoodsTypeID)
            {
                var pID = from a in db.t_Category where a.categoryID == subGoodsTypeID select a.parentID;
                GoodsTypeID = pID.SingleOrDefault();
            }
            ViewBag.GoodsType = GetGoodsList(ParentTypeDict, GoodsTypeID);

            //子目录先默认是第一个大类对应的子目录
            iParentID = ParentTypeDict.Keys.First();  //获取第一个大类的ID
            ParentTypeDict = null;
            ParentTypeDict = GetGoodsType(iParentID); //获取该大类下的子类
            if (0 != ParentTypeDict.Count)
            {
                //此处添加商品二级目录信息
                ViewBag.GoodsSubType = GetGoodsList(ParentTypeDict, subGoodsTypeID);
            }
            return;
        }


        /************************************************************************
       函数名称： GetGoodsType
       函数作用：根据传入的父ID获取所有子类型，并以字典的方式返回
       传入参数：iParentID:父ID的值  如果入参为0表示找出所有的一级目录
       返回参数：字典：<商品类型ID，商品类型名称> 
       修改记录： 作者        时间        原因 
       *          lulu        2015.1.10   create
       * **********************************************************************/
        public Dictionary<long, string> GetGoodsType(long iParentID)
        {

            Dictionary<long, string> ParentTypeDict = null;
            //linmbda操作model
            ParentTypeDict = db.t_Category.Where(u => u.parentID == iParentID).ToDictionary(u => u.categoryID, u => u.categoryName);
            return ParentTypeDict;
        }

        /************************************************************************
        函数名称： GetGoodsList
        函数作用：将传入的字典的值写入到List中并返回
        传入参数：ParentTypeDict：商品类型ID，Name的字典
        返回参数：List：<Text = 商品类型名称, Value = 商品类型ID>,GoodsTypeID 默认选择的商品ID
        修改记录： 作者        时间        原因 
          *        lulu        2015.1.11   create
        * **********************************************************************/
        public List<SelectListItem> GetGoodsList(Dictionary<long, string> ParentTypeDict,long GoodsTypeID = 0)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach(var item in ParentTypeDict)
            {
                items.Add(new SelectListItem
                {
                    Text = item.Value.ToString(),
                    Value = item.Key.ToString(),
                    Selected = (item.Key.ToString() == GoodsTypeID.ToString()),       
                });
            } 
            return items;
        }


        /************************************************************************
       函数名称： GetCategory
       函数作用：根据传入的上一级栏目ID获取下一级的相关栏目信息, 用于dropdownlist的级联
       传入参数：int id
       返回参数： Json(list, JsonRequestBehavior.AllowGet) 供view中的javascript脚本使用
       修改记录： 作者        时间        原因 
         *        lulu        2015.1.11   create
       * **********************************************************************/
        public JsonResult GetCategory(int id)
        {
            var ccategory = from a in db.t_Category where a.parentID == id select a;
            return Json(ccategory.ToList(), JsonRequestBehavior.AllowGet);
        }

        /************************************************************************
    函数名称： DeleteGoods
    函数作用： 删除商品
    传入参数：商品的Id
    返回参数：无
    修改记录： 作者        时间        原因 
    *         李康志      2015.1.25    create
   * **********************************************************************/
        public ActionResult DeleteGoods(long id)
        {
            //根据id去删除原有的商品
            t_Goods t_Goods = db.t_Goods.Single(g => g.goodsID == id);
            db.t_Goods.DeleteObject(t_Goods);
            int num = db.SaveChanges();
            if (num >= 0)
            {
                return Content(String.Format("<script>alert('删除成功！');location.href='{0}'</script>", Url.Action("PersonalSell", "Customer")), "text/html");
            }
            else
            {
                return Content(String.Format("<script>alert('删除失败，请重试！');location.href='{0}'</script>", Url.Action("PersonalSell", "Customer")), "text/html");
            }
        }

        /************************************************************************
        函数名称： GoodsDetails
        函数作用： 显示商品详情
        传入参数：商品的Id
        返回参数：Goods_Comment对象
        修改记录： 作者        时间        原因 
        *         李康志      2015.1.25    create
         *        王俊伟      2015.9.13   增加好友分享列表          
       * **********************************************************************/
        public ActionResult GoodsDetails(long id)
        {
            //创建一个 Goods_Comment类型的对象，用来保存商品，评论，评论回复等数据
            Goods_Comment goods_Comment = new Goods_Comment();

            #region 商品详情
            //根据商品ID找出该商品
            goods_Comment.t_Goods = db.t_Goods.Where(g => g.goodsID == id).SingleOrDefault();
            //找出发布商品的人
            string SellName = db.t_Customer.Where(c => c.customerID == goods_Comment.t_Goods.customerID).Select(c => c.loginName).FirstOrDefault();
            ViewBag.SellName = SellName;
            //取出商品图片的路径，并且以逗号为界限将他们分隔成string类型的数组
            string imagePath = goods_Comment.t_Goods.goodsImage;
            string[] goodImage = imagePath.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            //传给view页面处理
            ViewBag.images = goodImage;
            if (goodImage.Count() >= 2)
            {
                ViewData["goodImage0"] = goodImage[0];
            }
            if (goodImage.Count() >= 3)
            {
                ViewData["goodImage1"] = goodImage[1];
            }
            if (goodImage.Count() >= 4)
            {
                ViewData["goodImage2"] = goodImage[2];
            }
            // 修改时间的格式，只显示**年**月**日
            // 发布商品的时间
            var releasetime = goods_Comment.t_Goods.releaseTime.Date.ToLongDateString().ToString();
            ViewBag.ReleaseTime = releasetime;
            #endregion

            #region 评论列表
            //根据商品的ID从评论表找出评论内容,放在评论列表里面
            goods_Comment.t_CommentList = db.t_Comment.Where(c => c.goodsID == goods_Comment.t_Goods.goodsID).OrderByDescending(c => c.commentID).ToList();

            //遍历评论列表，根据评论的的id在回复表里面找出所有的回复内容，放在回复列表里面
            goods_Comment.t_replyList = GetReplyList(goods_Comment.t_CommentList);
            //通过字典形式根据回复人的ID映射出回复人的登录名
            Dictionary<long, string> dictReplyer = new Dictionary<long, string>();
            foreach (var item in goods_Comment.t_replyList)
            {
                if (!dictReplyer.ContainsKey(item.replyerID))
                {
                    //如果该字典里面没有存放该item的键，则将键值对放到该字典里面
                    t_Customer commentator = db.t_Customer.Where(c => c.customerID == item.replyerID).SingleOrDefault();
                    dictReplyer.Add(item.replyerID, commentator.customerName);
                }

            }
            ViewBag.DictReplyer = dictReplyer;

            //根据评论表里面评论人的id从用户表里面找出评论人的名字
            Dictionary<long, string> dictCommentator = new Dictionary<long, string>();
            foreach (var item in goods_Comment.t_CommentList)
            {
                //判断评论列表是否已经有该ID的键，如果没有该键则将该键值对放到字典中
                if (!dictCommentator.ContainsKey(item.commentatorID))
                {
                    var commentator = db.t_Customer.Where(c => c.customerID == item.commentatorID).SingleOrDefault();
                    if (commentator != null)
                    {
                        dictCommentator.Add(item.commentatorID, commentator.customerName);
                    }
                }
            }

            ViewBag.DictCommentator = dictCommentator;
            #endregion

            //#region 好友列表
            ////如果当前用户已登录，则读取其好友列表
            //if (Session["CustomerID"] != null)
            //{
            //    long hostId = Int64.Parse(Session["CustomerID"].ToString());
            //    List<t_Friends> friendsList = (from friends in db.t_Friends
            //                                   where friends.hostId == hostId
            //                                   select friends).ToList();
            //    Dictionary<long, string> dicFriends = new Dictionary<long, string>();
            //    foreach (t_Friends t_friends in friendsList)
            //    {
            //        t_Customer friends = db.t_Customer.Single(f=>f.customerID == t_friends.friendsId);
            //        dicFriends[t_friends.friendsId] = friends.customerName;
            //    }
            //    ViewBag.friendsList = friendsList;
            //    ViewBag.dicFriends = dicFriends;
            //} 
            //#endregion


            return View(goods_Comment);
        }


        /************************************************************************
       函数名称： GetReplyList
       函数作用：得到回复列表
       传入参数：评论列表
       返回参数：回复列表
       修改记录： 作者        时间        原因 
       *         李康志     2015.1.30     封装
       修改记录： 作者        时间        原因 
       *         李康志     2015.1.31     解决了查找对同一条评论ID的回复时，如果有多条回复，只查找出第一条回复。
  * **********************************************************************/
        public List<t_Reply> GetReplyList(List<t_Comment> commentList)
        {
            List<t_Reply> t_replyList = new List<t_Reply>();
            for (int i = 0; i < commentList.Count; i++)
            {
                long commentID = commentList[i].commentID;
                //查找出所有对同意评论的回复的数量
                int num = db.t_Reply.Where(r => r.commentID == commentID).Count();
                //如果之后一条回复，则将该回复放在回复列表
                if (num==1)
                {
                    t_Reply t_Reply = db.t_Reply.Where(r => r.commentID == commentID).FirstOrDefault();
                    t_replyList.Add(t_Reply);
                }
                //如果回复有超过一条，则遍历找出来的所有数据并且加到回复列表里面
                else if (num > 1)
                {
                    List<t_Reply> newReplyList = db.t_Reply.Where(r => r.commentID == commentID).ToList();
                    t_replyList.AddRange(newReplyList);
                }
                //t_Reply t_Reply = db.t_Reply.Where(r => r.commentID == commentID).FirstOrDefault();
                //if (t_Reply != null)
                //{
                //    t_replyList.Add(t_Reply);

                //}
            }
            return t_replyList;
        }


        /************************************************************************
      函数名称： GoodsDetails
      函数作用：处理收藏商品，发表评论，回复评论
      传入参数：提交的类型 和  Goods_Comment的实体对象
      返回参数：无
      修改记录： 作者        时间        原因 
      *         李康志      2015.1.25    create
      修改记录： 作者        时间        原因 
      *         李康志      2015.2.1     新增举报商品功能    
      修改记录： 作者        时间        原因 
      *         李康志      2015.2.14    页面增加js的校验，评论内容和回复内容和举报内容都不能为空
      修改记录： 作者        时间        原因 
      *         王俊伟      2015.10.12   修改固定的回复人ID为当前用户ID
     * **********************************************************************/
        [HttpPost]
        public ActionResult GoodsDetails(string action, Goods_Comment model)
        {
            //判断用户是否已经登录,如果已经登录则允许发表评论和收藏
            if (Session["CustomerLoginName"] != null)
            {
                #region 发表评论
                //检测用户是否已经登录，提示用户登录之后就取到用户的ID，未完成
                if (action == "评论")
                {
                    if (String.IsNullOrEmpty(Request.Form["commentContent"].Trim()))
                    {
                        return Content("<script>alert('请输入评论内容。');location.href=''</script>", "text/html");
                    }
                    //创建一个用来保存评论的对象
                    t_Comment t_comment = new t_Comment();
                    t_comment.comment = Request.Form["commentContent"].Trim();
                    t_comment.goodsID = model.t_Goods.goodsID;
                    t_comment.commentatorID = Int64.Parse(Session["CustomerId"].ToString());
                    t_comment.commentTime = DateTime.Now;
                    db.t_Comment.AddObject(t_comment);
                    //保存修改 用num接收返回的值判断评论是否成功
                    int num = db.SaveChanges();
                    if (num > 0)
                    {
                        return Content(String.Format("<script>alert('评论成功');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID }), "text/html"));

                    }
                    else
                    {
                        return Content(String.Format("<script>alert('出了点问题，评论失败，请重试');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID }), "text/html"));
                    }
                }
                #endregion

                #region 回复评论
                if (action == "回复")
                {
                    //创建回复对象
                    t_Reply t_Reply = new t_Reply();
                    //给回复对象实体赋值
                    t_Reply.commentID = Convert.ToInt64(Request.Form["commentID"]);
                    t_Reply.replyerID = Convert.ToInt64(Session["CustomerID"].ToString());//此处应为已登录人的ID
                    t_Reply.reply = Request.Form["commentContent"].Trim();
                    if (String.IsNullOrEmpty(t_Reply.reply))
                    {
                        return Content("<script>alert('请输入回复内容。');location.href=''</script>", "text/html");
                    }
                    t_Reply.replyTime = DateTime.Now;
                    //保存到数据库
                    db.t_Reply.AddObject(t_Reply);
                    int num = db.SaveChanges();
                    if (num > 0)
                    {
                        return Content(String.Format("<script>alert('回复成功');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID }), "text/html"));

                    }
                    else
                    {
                        return Content(String.Format("<script>alert('出了点问题，回复失败，请重试');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID }), "text/html"));
                    }
                }
                #endregion

                #region 收藏商品
                if (action == "收藏")
                {
                    long customerId = (long)Session["CustomerID"];
                    t_Customer t_Customer = db.t_Customer.Where(c => c.customerID == customerId).FirstOrDefault();
                    //找出该用户已经收藏过的商品
                    string oldCollectionID = t_Customer.collectionID;
                    //如果收藏商品这一列为null，则代表将要收藏的商品的ID直接写进去
                    if (oldCollectionID == null)
                    {
                        t_Customer.collectionID = model.t_Goods.goodsID.ToString();
                    }
                    //如果用户已经收藏过其他商品，则在原来的基础上把现在的商品ID写进数据库
                    else
                    {
                        //将原来的收藏的id全部查找出来，判断要收藏的商品以前是否已经收藏过
                        string[] collectionID = oldCollectionID.Split(',');
                        foreach (var item in collectionID)
                        {
                            if (item == model.t_Goods.goodsID.ToString())
                            {
                                return Content(String.Format("<script>alert('您已经收藏过该商品,请到个人中心查看。');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID })), "text/html");
                            }
                        }
                        // 在原来的字符串后面追加要收藏的商品的ID，并以，分隔
                        t_Customer.collectionID = oldCollectionID + "," + model.t_Goods.goodsID;
                    }
                    int num = db.SaveChanges();
                    if (num >= 0)
                    {
                        return Content(String.Format("<script>alert('收藏成功！');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID })), "text/html");
                    }
                    else
                    {
                        return Content(String.Format("<script>alert('收藏失败，请重试！');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID })), "text/html");
                    }
                }
                #endregion

                #region 商品举报
                if (action=="举报")
                {
                    string reson = Request.Form["GoodsComplain"].Trim();
                    if (String.IsNullOrEmpty(reson))
                    {
                        return Content("<script>alert('请输入举报原因。');location.href=''</script>", "text/html");
                    }
                    else
                    {
                        t_GoodsComplain t_goods_complain = new t_GoodsComplain();
                        t_goods_complain.goodsID = model.t_Goods.goodsID;
                        t_goods_complain.reporterID = (long)Session["CustomerID"];
                        t_goods_complain.reason = reson;
                        t_goods_complain.complainTime = DateTime.Now;
                        db.t_GoodsComplain.AddObject(t_goods_complain);
                        int num = db.SaveChanges();
                        if (num >= 1)
                        {
                            return Content(String.Format("<script>alert('谢谢亲的举报，二货们会尽快处理。');location.href='{0}'</script>", Url.Action("GoodsDetails", "Goods", new { id = model.t_Goods.goodsID })), "text/html");
                        }
                        else
                        {
                            return Content("<script>alert('刚刚网络出了问题，举报不成功，请重试！');location.href='{0}'</script>","text/html");
                        }
                    }
                }
                #endregion
            }
            //如果未登录提示用户登录
            else
            {
                return Content("<script>alert('请登录后再执行操作.');location.href=''</script>", "text/html");
            }
            return View("GoodsDetails");

        }


 

    
       
    }
}