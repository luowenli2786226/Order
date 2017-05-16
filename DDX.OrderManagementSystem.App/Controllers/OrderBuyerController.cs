using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class OrderBuyerController : BaseController
    {
        public ViewResult Index()
        {
            GetPermission();
            ViewData["toolbarButtons"] = BuildToolBarButtons();
            return View();
        }
        public ViewResult BIndex()
        {
            GetPermission();
            ViewData["toolbarButtons"] = BuildToolBarButtons();
            return View();
        }

        public override void GetPermission()
        {
            this.permissionAdd = this.IsAuthorized("OrderBuyer.Add");
            this.permissionDelete = this.IsAuthorized("OrderBuyer.Delete");
            this.permissionEdit = this.IsAuthorized("OrderBuyer.Edit");
            this.permissionExport = this.IsAuthorized("OrderBuyer.Export");
        }

        /// <summary>  
        /// 加载工具栏  
        /// </summary>  
        /// <returns>工具栏HTML</returns>  
        public override string BuildToolBarButtons(int t = 0)
        {
            StringBuilder sb = new StringBuilder();
            string linkbtn_template = "<a id=\"a_{0}\" class=\"easyui-linkbutton\" style=\"float:left\"  plain=\"true\" href=\"javascript:;\" icon=\"{1}\"  {2} title=\"{3}\" onclick='{5}'>{4}</a>";
            sb.Append("<a id=\"a_refresh\" class=\"easyui-linkbutton\" style=\"float:left\"  plain=\"true\" href=\"javascript:;\" icon=\"icon-reload\"  title=\"重新加载\"  onclick='refreshClick()'>刷新</a> ");
            sb.Append("<div class='datagrid-btn-separator'></div> ");
            sb.Append(string.Format(linkbtn_template, "add", "icon-add", permissionAdd ? "" : "disabled=\"True\"", "添加用户", "添加", "addClick()"));
            sb.Append(string.Format(linkbtn_template, "edit", "icon-edit", permissionEdit ? "" : "disabled=\"True\"", "修改用户", "修改", "editClick()"));
            sb.Append(string.Format(linkbtn_template, "delete", "icon-remove", permissionDelete ? "" : "disabled=\"True\"", "删除用户", "删除", "delClick()"));
            sb.Append("<div class='datagrid-btn-separator'></div> ");
            sb.Append("<a href=\"#\" class='easyui-menubutton' " + (permissionExport ? "" : "disabled='True'") + " data-options=\"menu:'#dropdown',iconCls:'icon-undo'\">导出</a>");

            return sb.ToString();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(OrderBuyerType obj)
        {
            bool isOk = Save(obj);
            return Json(new { IsSuccess = isOk });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OrderBuyerType GetById(int Id)
        {
            OrderBuyerType obj = Get<OrderBuyerType>(Id);
            return obj;
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            OrderBuyerType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(OrderBuyerType obj)
        {
            bool isOk = Update<OrderBuyerType>(obj);
            return Json(new { IsSuccess = isOk });
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {
            bool isOk = DeleteObj<OrderBuyerType>(id);
            return Json(new { IsSuccess = isOk });
        }

        public JsonResult ExportData(string search)
        {
            StringBuilder builder = new StringBuilder();
            string where = "";
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            string sql = @"select * from (
select BuyerEmail,Account from Orders 
where Status not in('作废订单','待处理')   group by Account,BuyerEmail 
)as tbl ";
            IList<object[]> objList = NSession.CreateSQLQuery(sql + where)

                .List<object[]>();
            foreach (object[] objectse in objList)
            {
                if (objectse[0] != null && !string.IsNullOrEmpty(objectse[0].ToString()))
                    builder.AppendLine(objectse[0].ToString());
            }

            base.Session["ExportDown"] = builder.ToString();
            return base.Json(new { IsSuccess = true });
        }


        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string where = "";
            string orderby = " order by FristBuyOn desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            string sql = @"select * from (
select BuyerName,BuyerEmail,Count(BuyerName) as BuyCount,SUM(Amount) as 'BuyAmount',MIN(GenerateOn) as 'FristBuyOn',Max(GenerateOn) as 'ListBuyOn' ,Platform,Account from Orders 
where Status not in('作废订单','待处理')   group by BuyerName,Platform,Account,BuyerEmail 
)as tbl ";
            IList<object[]> objList = NSession.CreateSQLQuery(sql + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<object[]>();
            List<OrderBuyerCount> list = new List<OrderBuyerCount>();
            foreach (object[] objectse in objList)
            {
                OrderBuyerCount orderBuyerCount = new OrderBuyerCount();
                orderBuyerCount.BuyerName = Utilities.ToStr(objectse[0]);
                orderBuyerCount.BuyerEmail = Utilities.ToStr(objectse[1]);
                orderBuyerCount.BuyCount = Convert.ToInt32(objectse[2]);
                orderBuyerCount.BuyAmount = Math.Round(Convert.ToDouble(objectse[3]), 2);
                orderBuyerCount.FristBuyOn = Convert.ToDateTime(objectse[4]);
                orderBuyerCount.ListBuyOn = Convert.ToDateTime(objectse[5]);
                orderBuyerCount.Platform = objectse[6].ToString();
                orderBuyerCount.Account = Utilities.ToStr(objectse[7]);
                list.Add(orderBuyerCount);
            }
            object count = NSession.CreateSQLQuery(" select count(1) from (select BuyerName,BuyerEmail,Count(BuyerName) as BuyCount,SUM(Amount) as 'BuyAmount',MIN(GenerateOn) as 'FristBuyOn',Max(GenerateOn) as 'ListBuyOn' ,Platform,Account from Orders where Status not in('作废订单','待处理')   group by BuyerName,Platform,BuyerEmail,Account )as tbl " + where).UniqueResult();
            return Json(new { total = count, rows = list });
        }
        public JsonResult BList(int page, int rows, string sort, string order, string search)
        {
            string where = "";
            string orderby = " order by FristBuyOn desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            string sql = @"select * from (
  select O.BuyerName,( select Count(OO.OrderNo)   from Orders OO where O.BuyerName=OO.BuyerName) as BuyCount,Count(distinct D.OrderNo) as BuyUnreason,MIN(GenerateOn) as 'FristBuyOn',Max(GenerateOn) as 'ListBuyOn',O.Platform,O.Account  from Orders O  left join DisputeRecordType D on O.OrderExNo =D.OrderNo where D.ExamineClass='客户无理取闹' group by O.BuyerName,O.Platform,O.Account
)as tbl ";
            IList<object[]> objList = NSession.CreateSQLQuery(sql + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<object[]>();
            List<OrderBuyerCount> list = new List<OrderBuyerCount>();
            foreach (object[] objectse in objList)
            {
                OrderBuyerCount orderBuyerCount = new OrderBuyerCount();
                orderBuyerCount.BuyerName = Utilities.ToStr(objectse[0]);
                orderBuyerCount.BuyCount = Convert.ToInt32(objectse[1]);
                orderBuyerCount.BuyUnreason = Convert.ToInt32(objectse[2]);
                orderBuyerCount.FristBuyOn = Convert.ToDateTime(objectse[3]);
                orderBuyerCount.ListBuyOn = Convert.ToDateTime(objectse[4]);
                orderBuyerCount.Platform = objectse[5].ToString();
                orderBuyerCount.Account = Utilities.ToStr(objectse[6]);
                list.Add(orderBuyerCount);
            }
            object count = NSession.CreateSQLQuery(" select count(1) from (  select O.BuyerName,( select Count(OO.OrderNo)   from Orders OO where O.BuyerName=OO.BuyerName) as BuyCount,Count(distinct D.OrderNo) as BuyUnreason,MIN(GenerateOn) as 'FristBuyOn',Max(GenerateOn) as 'ListBuyOn',O.Platform,O.Account  from Orders O  left join DisputeRecordType D on O.OrderExNo =D.OrderNo where D.ExamineClass='客户无理取闹' group by O.BuyerName,O.Platform,O.Account )as tbl " + where).UniqueResult();
            return Json(new { total = count, rows = list });
        }

    }
}

