using DDX.OrderManagementSystem.App;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI;


namespace DDX.OrderManagementSystem.App.Controllers
{


    public class AliActivityRecordController : BaseController
    {
        public JsonResult ALLList()
        {
            //string str = " where Status='活动中' and ((BeginDate >='" + DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + 1).ToString("yyyy-MM-dd") + "' and BeginDate <'" + DateTime.Now.AddDays((double)(8 - DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "' and  EndDate <='" + DateTime.Now.AddDays((double)(8 - DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "') Or (BeginDate <'" + DateTime.Now.AddDays((double)(8 - DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "' and  EndDate >'" + DateTime.Now.AddDays((double)(8 - DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "') Or (BeginDate <'" + DateTime.Now.AddDays((-(int)DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "' and  EndDate >'" + DateTime.Now.AddDays((-(int)DateTime.Now.DayOfWeek)).ToString("yyyy-MM-dd") + "'))";
            string str = string.Format("where Status='活动中' and BeginDate<='{0}' and EndDate>='{0}' ", DateTime.Now.ToString("yyyy-MM-dd"));
            string str2 = " order by Account ";
            IList<AliActivityType> list = base.NSession.CreateQuery("from AliActivityType " + str + str2).List<AliActivityType>();
            object obj2 = base.NSession.CreateQuery("select count(Id) from AliActivityType " + str).UniqueResult();
            return base.Json(new { total = obj2, rows = list });
        }

        public ActionResult Create()
        {
            return base.View();
        }

        [HttpPost]
        public JsonResult Create(AliActivityRecordType obj)
        {
            try
            {
                obj.CreateBy = base.CurrentUser.Realname;
                obj.CreateOn = DateTime.Now;
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
                AliActivityRecordType byId = this.GetById(id);
                base.NSession.Delete(byId);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost, OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult DoAudit(int k, int a, string m)
        {
            try
            {
                AliActivityRecordType type;
                string errorMsg;
                if ((a == 1) || (a == 9))
                {
                    type = base.Get<AliActivityRecordType>(k);
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
                else if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                {
                    type = base.Get<AliActivityRecordType>(k);
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
        public ActionResult Edit(AliActivityRecordType obj)
        {
            try
            {
                base.NSession.Update(obj);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            AliActivityRecordType byId = this.GetById(id);
            return base.View(byId);
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult EditStatus(int id, string s)
        {
            try
            {
                AliActivityRecordType type = base.Get<AliActivityRecordType>(id);
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

        public AliActivityRecordType GetById(int Id)
        {
            AliActivityRecordType type = base.NSession.Get<AliActivityRecordType>(Id);
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
            string str2 = " order by Id desc ";
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
            IList<AliActivityRecordType> list = base.NSession.CreateQuery("from AliActivityRecordType " + str + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<AliActivityRecordType>();
            object obj2 = base.NSession.CreateQuery("select count(Id) from AliActivityRecordType " + str).UniqueResult();
            return base.Json(new { total = obj2, rows = list });
        }

        public ActionResult ReportIndex()
        {
            return base.View();

        }
    }
}

