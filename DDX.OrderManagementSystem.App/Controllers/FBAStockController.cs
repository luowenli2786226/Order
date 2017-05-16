using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;

using NHibernate;

namespace DDX.OrderManagementSystem.Web.Controllers
{
    public class FBAStockController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(FBAStockType obj)
        {
            try
            {
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public FBAStockType GetById(int Id)
        {
            FBAStockType obj = NSession.Get<FBAStockType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            FBAStockType obj = GetById(id);
            return View(obj);
        }


        public JsonResult Synchronous(int Account)
        {
            NSession.CreateSQLQuery("delete from FBAStock where account='" + Get<AccountType>(Account).AccountName + "'").ExecuteUpdate();
            NSession.Flush();
            //OrderHelper.APIByAmazonStock(Get<AccountType>(78), NSession);
            //OrderHelper.APIByAmazonStock(Get<AccountType>(96), NSession);
            OrderHelper.APIByAmazonStock(Get<AccountType>(Account), NSession);
            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(FBAStockType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                FBAStockType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string where = "";
            string orderby = " order by Account asc ";
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
            IList<FBAStockType> objList = NSession.CreateQuery("from FBAStockType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<FBAStockType>();

            object count = NSession.CreateQuery("select count(Id) from FBAStockType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

    }
}

