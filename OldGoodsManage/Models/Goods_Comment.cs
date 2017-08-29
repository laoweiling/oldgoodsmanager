using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OldGoodsManage.Models
{
    public  class Goods_Comment
    {
        public  t_Goods t_Goods { get; set; }
        public  List<t_Comment> t_CommentList{get;set;}
  
        public t_Comment t_Comment { get; set; }
        public List<t_Reply> t_replyList { get; set; }

    }
}