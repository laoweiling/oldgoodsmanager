//------------------------------------------------------------------------------
// 
//    此代码是用于前后台数据校验的类，包含了Models的所有业务逻辑
//    作为OldGoods.Designer.cs的partial类，与自动生成的OldGoods.Designer.cs类
//    分离开来，避免了更新数据库时所带来的验证丢失。      
//    所有的业务逻辑的代码都应当写在这个类，包括信息格式定义、错误信息显示等等  
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web.Http;


namespace OldGoodsManage.Models
{
     
    [MetadataType(typeof(t_CustomerMetadata))]
    public partial class t_Customer : EntityObject
    {
        private class t_CustomerMetadata
        {
            [DisplayName("用户ID")]
            public global::System.Int64 customerID { get; set; }


            [DisplayName("用户姓名")]
            [Required(ErrorMessage = "请输入姓名")]
            public global::System.String customerName { get; set; }

            [DisplayName("用户类型")]
            [Required(ErrorMessage = "请输入用户类型")]
            public global::System.Int32 customerType { get; set; }

            [DisplayName("登录名")]
            [Required(ErrorMessage = "请输入登录名")]
            public global::System.String loginName { get; set; }

            [DisplayName("注册时间")]
            public global::System.DateTime registerTime { get; set; }

            [DisplayName("密码")]
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "请输入密码")]
            public global::System.String password { get; set; }

            [DisplayName("邮箱")]
            [Required(ErrorMessage = "请输入邮箱地址")]
            [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "请输入正确的Email！")]
            public global::System.String email { get; set; }

            [DisplayName("校区")]
            [Required(ErrorMessage = "请选择校区")]
            public global::System.Int64 schoolID { get; set; }

            [DisplayName("性别ID")]
            public Nullable<global::System.Int32> sex { get; set; }

            [DisplayName("手机号码")]
            [Required(ErrorMessage = "请输入手机号")]
            [RegularExpression(@"((\d{11})|^(\(\d{2,3}\))|(\d{3}\-))?1[3,8,5]{1}\d{9}$", ErrorMessage = "请输入正确的手机号码！")]
            public global::System.String tel { get; set; }


            [DisplayName("是否被锁定")]
            public global::System.Int32 locked { get; set; }

            [DisplayName("地址")]
            [Required(ErrorMessage = "请输入地址")]
            public global::System.String address { get; set; }

            [DisplayName("备注")]
            public global::System.String remarks { get; set; }

            [DisplayName("收藏ID")]
            public global::System.String collectionID { get; set; }


        }
        #region 新添加的验证码字段
        /// <summary>
        /// 验证码
        /// </summary>
        [Display(Name = "验证码")]
        public string VerificationCode { get; set; }
        #endregion

    }

    [MetadataType(typeof(t_FeedbackMetadata))]
    public partial class t_Feedback : EntityObject
    {
        private class t_FeedbackMetadata
        {
            [DisplayName("反馈ID")]
            public global::System.Int64 feedbackID { get; set; }

            [DisplayName("反馈内容")]
            [Required(ErrorMessage = "请输入反馈内容!")]
            public global::System.String feedback { get; set; }

            [DisplayName("联系方式")]
            public global::System.String contactWay { get; set; }

            [DisplayName("反馈时间")]
            public global::System.DateTime feedbackTime { get; set; }

            [DisplayName("是否已读")]
            public global::System.Int32 status { get; set; }



        }
    }

    [MetadataType(typeof(t_GoodsMeta))]
    public partial class t_Goods : EntityObject
    {

        private class t_GoodsMeta
        {
            [Required(ErrorMessage = "请输入商品名称")]
            [RegularExpression(@"^.{1,15}$", ErrorMessage = "不能多于15个字！")]
            public global::System.String goodsTitle { get; set; }

            [Required(ErrorMessage = "请输入商品详细信息")]
            [RegularExpression(@"^.{10,1000}$", ErrorMessage = "不能少于10个字！")]
            public global::System.String goodsInfo { get; set; }

            [Required(ErrorMessage = "请输入商品价格")]
            [RegularExpression(@"^[0-9]+(.[0-9]{2})?$", ErrorMessage = "商品价格必须为有效数字！")]
            public global::System.Decimal goodsPrice { get; set; }

            [DisplayName("商品数量：")]
            [Required(ErrorMessage = "请输入商品数量")]
            [RegularExpression(@"^\d+$", ErrorMessage = "商品数量必须为整数！")]
            [Range(0,10000)]
            public global::System.Int32 inventory { get; set; }

            [Required(ErrorMessage = "请输入商品交易地点")]
            public global::System.String tradeAddress { get; set; }

            [RegularExpression(@"^\+?[1-9][0-9]*$", ErrorMessage = "请输入正确的QQ号码！")]
            public global::System.String contactQQ { get; set; }

            [RegularExpression(@"((\d{11})|^(\(\d{2,3}\))|(\d{3}\-))?1[3,8,5]{1}\d{9}$", ErrorMessage = "请输入正确的手机号码！")]
            public global::System.String contactTel { get; set; }

            [DisplayName("提示信息:")]
            public string remarks { get; set; }

        }


        public static void ResizeAndSave(string savePath, string fileName, Stream imageBuffer, int maxSideSize, bool makeItSquare)
        {
            int newWidth;
            int newHeight;
            Image image = Image.FromStream(imageBuffer);
            int oldWidth = image.Width;
            int oldHeight = image.Height;
            Bitmap newImage;
            if (makeItSquare)
            {
                int smallerSide = oldWidth >= oldHeight ? oldHeight : oldWidth;
                double coeficient = maxSideSize / (double)smallerSide;
                newWidth = Convert.ToInt32(coeficient * oldWidth);
                newHeight = Convert.ToInt32(coeficient * oldHeight);
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                newImage = new Bitmap(maxSideSize, maxSideSize);
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
            }
            else
            {
                int maxSide = oldWidth >= oldHeight ? oldWidth : oldHeight;
                if (maxSide > maxSideSize)
                {
                    double coeficient = maxSideSize / (double)maxSide;
                    newWidth = Convert.ToInt32(coeficient * oldWidth);
                    newHeight = Convert.ToInt32(coeficient * oldHeight);
                }
                else
                {
                    newWidth = oldWidth;
                    newHeight = oldHeight;
                }
                newImage = new Bitmap(image, newWidth, newHeight);
            }
            newImage.Save(savePath + fileName + ".jpg", ImageFormat.Jpeg);
            image.Dispose();
            newImage.Dispose();
        }
    }

    [MetadataType(typeof(t_RecruitmentMeta))]
    public partial class t_Recruitment : EntityObject
    {
        private class t_RecruitmentMeta
        {
            [DisplayName("招聘信息ID")]
            public global::System.Int64 recruitmentID { get; set; }

            [DisplayName("部门")]
            [Required(ErrorMessage = "请输入部门")]
            public global::System.String department { get; set; }


            [DisplayName("要求")]
            [Required(ErrorMessage = "请输入招聘要求")]
            public global::System.String requirment { get; set; }

            [DisplayName("联系方式")]

            [Required(ErrorMessage = "请输入联系方式")]
            public global::System.String contactWay { get; set; }
        }
    }

    [MetadataType(typeof(t_RoleMeta))]
    public partial class t_Role : EntityObject
    {
        private class t_RoleMeta
        {
            [DisplayName("角色名")]
            public global::System.String roleName { get; set; }

            [DisplayName("备注")]
            public global::System.String remarks { get; set; }


        }
    }

    [MetadataType(typeof(t_SiteInfoMeta))]
    public partial class t_SiteInfo : EntityObject
    {
        private class t_SiteInfoMeta
        {
            [Required(ErrorMessage = "请输入公司简介")]
            public global::System.String companyInfo { get; set; }

            [Required(ErrorMessage = "请输入公司联系方式")]
            [RegularExpression(@"^((0\d{2,5}-)|\(0\d{2,5}\))?\d{7,8}(-\d{3,4})?$",
            ErrorMessage = "电话格式有误。可选]\n示例：023-12345678;(023)1234567-1234")]
            public global::System.String companyTel { get; set; }

            [Required(ErrorMessage = "请输入公司简Email")]
            [RegularExpression(@"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$",
            ErrorMessage = "请输入正确的Email格式\n示例：abc@123.com")]
            public global::System.String companyEmail { get; set; }


            [Required(ErrorMessage = "请输入公司地址")]
            [MaxLength(250, ErrorMessage = "Email 地址长度无法超过250个字符")]
            public global::System.String companyAddress { get; set; }


        }
    }

    [MetadataType(typeof(t_UserMeta))]
    public partial class t_User : EntityObject
    {
        private class t_UserMeta
        {
            [DisplayName("用户ID")]
            public global::System.Int64 userID { get; set; }

            [DisplayName("用户姓名")]
            [Required(ErrorMessage = "请输入用户姓名")]
            public global::System.String userName { get; set; }

            [DisplayName("登录名")]
            [Required(ErrorMessage = "请输入登录名")]
            public global::System.String loginName { get; set; }

            [DisplayName("注册时间")]
            public global::System.DateTime registerTime { get; set; }

            [DisplayName("密码")]
            [Required(ErrorMessage = "请输入密码")]
            [DataType(DataType.Password)]
            public global::System.String password { get; set; }

            [DisplayName("角色ID")]
            [Required(ErrorMessage = "请选择用户角色")]
            public global::System.Int64 roleID { get; set; }

            [DisplayName("电子邮箱")]
            [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "请输入正确的Email！")]
            public global::System.String email { get; set; }

            [DisplayName("性别ID")]
            public Nullable<global::System.Int32> sex { get; set; }

            [DisplayName("手机号码")]
            [Required(ErrorMessage = "请输入手机号码")]
            [RegularExpression(@"((\d{11})|^(\(\d{2,3}\))|(\d{3}\-))?1[3,8,5]{1}\d{9}$", ErrorMessage = "请输入正确的手机号码！")]
            public global::System.String tel { get; set; }

            [DisplayName("地址")]
            public global::System.String address { get; set; }

            [DisplayName("用户备注")]
            public global::System.String remarks { get; set; }
        }

        #region 新添加的验证码字段
        /// <summary>
        /// 验证码
        /// </summary>
        [Display(Name = "验证码")]
        public string VerificationCode { get; set; }
        #endregion
    } 
  
}