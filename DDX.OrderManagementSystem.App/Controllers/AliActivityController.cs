using DDX.OrderManagementSystem.App;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI;

namespace DDX.OrderManagementSystem.App.Controllers
{


    public class AliActivityController : BaseController
    {
        public ActionResult Create()
        {
            return base.View();
        }

        [HttpPost]
        public JsonResult Create(AliActivityType obj)
        {
            try
            {
                if (Convert.ToDouble(obj.ProfitAndLoss) * obj.ExpectedSales < -2000)
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "总盈亏情况大于2000" });
                }
                if (Convert.ToDouble(obj.ProfitAndLoss) < -3)
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "单个盈亏情况大于3" });
                }
                obj.IsAudit = 2;
                obj.CreateBy = base.CurrentUser.Realname;
                obj.CreateOn = DateTime.Now;
                obj.SortCode = 0;
                base.NSession.SaveOrUpdate(obj);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                AliActivityType byId = this.GetById(id);
                base.NSession.Delete(byId);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoAudit(int k, int a, string m)
        {
            try
            {
                AliActivityType type;
                string errorMsg;
                if ((a == 1) || (a == 9))
                {
                    type = base.Get<AliActivityType>(k);
                    if (type.Account.IndexOf("yw") != -1)
                    {
                        if (base.CurrentUser.Realname == "雷刚")
                        {
                            if (Utilities.ToDouble(type.ProfitAndLoss) >= 0.0)
                            {
                                type.IsAudit = 2;
                            }
                            else
                            {
                                type.IsAudit = 1;
                            }
                            errorMsg = type.ErrorMsg;
                            type.ErrorMsg = errorMsg + base.CurrentUser.Realname + " " + m + "\n";
                            base.NSession.Update(type);
                            base.NSession.Flush();
                        }
                    }
                    else if (base.CurrentUser.Realname == "邵纪银")
                    {
                        type.IsAudit = a;
                        errorMsg = type.ErrorMsg;
                        type.ErrorMsg = errorMsg + base.CurrentUser.Realname + " " + m + "\n";
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }
                }

                if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                {
                    type = base.Get<AliActivityType>(k);
                    type.IsAudit = a;
                    errorMsg = type.ErrorMsg;
                    type.ErrorMsg = errorMsg + base.CurrentUser.Realname + " " + m + "\n";
                    base.NSession.Update(type);
                    base.NSession.Flush();
                }
                else
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足！" });
                }
            }
            catch (Exception)
            {
                return base.Json(new { ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost, OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(AliActivityType obj)
        {
            try
            {
                if (obj.Status == "申报中")
                {
                    obj.SortCode = 0;
                }
                if (obj.Status == "活动中")
                {
                    obj.SortCode = 1;
                }
                if (obj.Status == "已结束")
                {
                    obj.SortCode = 2;
                    if (obj.ActualSales == 0)
                    {
                        obj.SortCode = 3;
                    }
                }
                if (obj.ActualSales > obj.ExpectedSales)
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "实际销量不能大于预计销量" });
                }
                if (obj.ActualSales > 0)
                {
                    obj.TotalSale = Math.Round(Utilities.ToDecimal(obj.ActualSales * obj.ActivityPrice), 2);
                    obj.Rate = Math.Round(Utilities.ToDecimal(obj.ProfitAndLoss) * Utilities.ToDecimal(obj.ActualSales) / obj.TotalSale / 6.1m, 3) * 100;
                }
                else
                {

                    obj.TotalSale = 0;
                    obj.Rate = 0;
                }
                base.NSession.Update(obj);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            AliActivityType byId = this.GetById(id);
            return base.View(byId);
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult EditStatus(int id, string s)
        {
            try
            {
                AliActivityType type = base.Get<AliActivityType>(id);
                type.Status = s;
                base.NSession.Update(type);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        public AliActivityType GetById(int Id)
        {
            AliActivityType type = base.NSession.Get<AliActivityType>(Id);
            if (type == null)
            {
                throw new Exception("返回实体为空");
            }
            return type;
        }

        public ViewResult Index()
        {
            return base.View();
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string str = "";
            string str2 = " order by IsAudit asc,SortCode Desc,EndDate desc ";
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where " + str;
                }
            }
            IList<AliActivityType> list = base.NSession.CreateQuery("from AliActivityType " + str + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<AliActivityType>();
            object obj2 = base.NSession.CreateQuery("select count(Id) from AliActivityType " + str).UniqueResult();
            // object obj3 = base.NSession.CreateSQLQuery("select SUM(cast(ProfitAndLoss as float)*ExpectedSales) from AliActivity " + str).UniqueResult();
            object obj4 = base.NSession.CreateSQLQuery("select SUM(cast(ProfitAndLoss as float)*ActualSales) from AliActivity " + str).UniqueResult();
            object obj5 = base.NSession.CreateSQLQuery("select SUM(TotalSale) from AliActivity " + str).UniqueResult();
            object obj6 = base.NSession.CreateSQLQuery("select SUM(ActualSales) from AliActivity " + str).UniqueResult();
            List<AliActivityType> footerlist = new List<AliActivityType>();
            //footerlist.Add(new AliActivityType() {  ProfitAndLoss = obj3.ToString() });
            if (Utilities.ToDecimal(obj5) == 0)
            {
                footerlist.Add(new AliActivityType() { Id = 0, ActualSales = Utilities.ToInt(obj6), Reason = Math.Round(Utilities.ToDecimal(obj4), 2).ToString(), TotalSale = Math.Round(Utilities.ToDecimal(obj5), 2), Rate = 0 });
            }
            else
            {
                footerlist.Add(new AliActivityType() { Id = 0, ActualSales = Utilities.ToInt(obj6), Reason = Math.Round(Utilities.ToDecimal(obj4), 2).ToString(), TotalSale = Math.Round(Utilities.ToDecimal(obj5), 2), Rate = Math.Round(Utilities.ToDecimal(obj4) / Utilities.ToDecimal(obj5) / 6.1m, 3) * 100 });
            }

            return base.Json(new { total = obj2, rows = list, footer = footerlist });
        }
    }
}

