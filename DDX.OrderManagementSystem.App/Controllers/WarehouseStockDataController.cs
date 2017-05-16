using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;

using NHibernate;
using DDX.OrderManagementSystem.App.Common.Statistics;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class WarehouseStockDataController : BaseController
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
        public JsonResult Create(WarehouseStockDataType obj)
        {
            try
            {
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { Msg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WarehouseStockDataType GetById(int Id)
        {
            WarehouseStockDataType obj = NSession.Get<WarehouseStockDataType>(Id);
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
            WarehouseStockDataType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseStockDataType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { Msg = "出错了" });
            }
            return Json(new { IsSuccess = true });

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                WarehouseStockDataType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { Msg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string where = "";
            string orderby = " order by Id desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);

            }
            IList<WarehouseStockDataType> objList = NSession.CreateQuery("from WarehouseStockDataType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<WarehouseStockDataType>();

            object count = NSession.CreateQuery("select count(Id) from WarehouseStockDataType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        public JsonResult GetStockData(int id, int wid)
        {
            string where = "";
            string orderby = " order by CreateOn desc ";
            //IList<WarehouseStockDataType> objList = NSession.CreateQuery("from WarehouseStockDataType  Where NowQty>0 and WId=" + wid + " and PId=" + id + orderby).List<WarehouseStockDataType>();
            IList<WarehouseStockDataType> objList = NSession.CreateQuery("from WarehouseStockDataType  Where NowQty>0 and WId=" + wid + " and PId=" + id + orderby).List<WarehouseStockDataType>();

            foreach (WarehouseStockDataType ws in objList)
            {

                ws.Total = Convert.ToDouble(ws.NowQty * ws.Amount);
                //计算单价背景色
                int i = 0;
                int[] kk2 = dateTimeDiff.toResult(ws.CreateOn.ToString(), DateTime.Now.ToShortDateString(), diffResultFormat.mm);
                i = kk2[0];
                string Background = string.Empty;
                List<GoodsDiscountRulesType> discountrules = NSession.CreateQuery(" from GoodsDiscountRulesType where DiscountCycleBegin<=" + i + " and DiscountCycleEnd>=" + i + "").List<GoodsDiscountRulesType>().ToList();
                if (discountrules.Count > 0)
                {
                    GoodsDiscountRulesType type = discountrules[0];
                    Background = type.WarningColor;
                }
                ws.Style = "color:blue;background-color:"+Background;
            }

            //List<WarehouseStockDataType> objList = NSession.CreateSQLQuery("select (NowQty*Amount) as TotalPrice,* from WarehouseStockData Where NowQty>0 and WId=" + wid + " and PId=" + id + orderby).AddEntity(typeof(WarehouseStockDataType))
            //   .List<WarehouseStockDataType>().ToList();

            return Json(new { total = objList.Count, rows = objList });
        }

    }
}

