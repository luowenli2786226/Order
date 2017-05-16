using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using System.Data;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class OrderReturnRecordController : BaseController
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
        public JsonResult GetOrderByReturn(string o)
        {
            List<OrderType> orders =
                NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode='" + o + "'").List<OrderType>().ToList();
            if (orders.Count > 0)
            {
                OrderType order = orders[0];
                if (order.Status != OrderStatusEnum.退货订单.ToString())
                {
                    string html = "<b>订单:{0}  <br/>平台:{5} <br/>账户:{6} <br/>下单时间:{1} <br/>发货时间:{2} <br/>发货渠道:{3} <br/>发货条码:{4}</b>";
                    return Json(new { IsSuccess = true, Result = string.Format(html, order.OrderNo, order.CreateOn, order.ScanningOn, order.LogisticMode, order.TrackCode, order.Platform, order.Account) });
                }
                return Json(new { IsSuccess = false, Result = "该订单已经退货！请不要重复扫描！" });
            }
            return Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        [HttpPost]
        public JsonResult ReturnOrder(string o, string p)
        {
            List<OrderType> orders =
                   NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode='" + o + "'").List<OrderType>().ToList();
            if (orders.Count > 0)
            {
                OrderType order = orders[0];
                order.Status = OrderStatusEnum.退货订单.ToString();
                order.BuyerMemo = p + "  " + order.BuyerMemo;
                NSession.Update(order);
                NSession.Flush();
                OrderReturnRecordType record = new OrderReturnRecordType();
                record.Account = order.Account;
                record.Platform = order.Platform;
                record.ReturnLogisticsMode = order.LogisticMode;
                record.OrderExNO = order.OrderExNo;
                record.OrderNo = order.OrderNo;
                record.OrderSendOn = order.ScanningOn;
                record.ReturnType = p;
                record.OldTrackCode = order.TrackCode;
                record.CreateOn = DateTime.Now;
                record.CreateBy = GetCurrentAccount().Realname;
                record.Country = order.Country;
                record.CurrencyCode = order.CurrencyCode;
                record.Amount = order.Amount;
                record.BuyerName = order.BuyerName;
                record.OrderCreateOn = order.CreateOn;
                record.OId = order.Id;


                NSession.Save(record);
                NSession.Flush();

                //1、物品状态初始化为配货前状态 ；
                using (ITransaction tx = NSession.BeginTransaction())
                {
                    try
                    {
                        List<SKUCodeType> SKUCodeList =
                               NSession.CreateQuery("from SKUCodeType where OrderNo='" + order.OrderNo + "'").List<SKUCodeType>().ToList();
                        if (SKUCodeList.Count > 0)
                        {
                            foreach (SKUCodeType obj in SKUCodeList)
                            {
                                obj.IsOut = 0;
                                obj.IsSend = 0;
                                obj.SendOn = "";
                                obj.OrderNo = "";
                                obj.PeiOn = "";
                                NSession.Update(obj);
                            }
                            //SKUCodeList[0].IsOut = 0;
                            //SKUCodeList[0].IsSend = 0;
                            //SKUCodeList[0].SendOn = "";
                            //SKUCodeList[0].OrderNo = "";
                            //SKUCodeList[0].PeiOn = "";
                            //NSession.Update(SKUCodeList[0]);
                        }
                        tx.Commit();
                    }
                    catch (HibernateException)
                    {
                        tx.Rollback();
                    }
                }

                //2、增加对应出库负数记录做冲红； 
                //using (ITransaction tx = NSession.BeginTransaction())
                //{
                //    try
                //    {
                List<StockOutType> StockOutList =
                       NSession.CreateQuery("from StockOutType where OrderNo='" + order.OrderNo + "'").List<StockOutType>().ToList();
                if (StockOutList.Count > 0)
                {
                    Utilities.StockOut(StockOutList[0].WId, StockOutList[0].SKU, StockOutList[0].Qty * -1, "退件入库冲红", CurrentUser.Realname, StockOutList[0].Memo, StockOutList[0].OrderNo, NSession);
                }
                //tx.Commit();
                //}
                //catch (HibernateException)
                //{
                //    tx.Rollback();
                //}
                //}

                LoggerUtil.GetOrderRecord(order, "订单退货扫描！", "订单设置为退货", CurrentUser, NSession);

                return Json(new { IsSuccess = true, Result = "订单：" + order.OrderNo + "  已经添加到退货~！" });
            }
            return Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        // 重新打印老SKU Code
        public JsonResult PrintSKU(string OrderNo)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("sku");
            dt.Columns.Add("name");
            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("people");
            dt.Columns.Add("desc");
            dt.Columns.Add("code");
            int i = 1;
            IList<SKUCodeType> list = NSession.CreateQuery("from SKUCodeType where OrderNo=:p").SetString("p", OrderNo).List<SKUCodeType>();
            if (list.Count == 0)
            {
                IList<OrderType> order = NSession.CreateQuery("from OrderType where TrackCode=:p1").SetString("p1", OrderNo).List<OrderType>();
                // 无法通过订单号查询时，通过跟踪码查询
                if (order.Count > 0)
                {
                    list = NSession.CreateQuery("from SKUCodeType where OrderNo=:p").SetString("p", order[0].OrderNo).List<SKUCodeType>();
                }
                else
                {
                    return Json(new { IsSuccess = false, Result = "错误!无法查找到对应订单信息!" });
                }
            }
            foreach (SKUCodeType skuCodeType in list)
            {
                IList<ProductType> list2 = NSession.CreateQuery("from ProductType where SKU='" + skuCodeType.SKU + "'").
                       List<ProductType>();

                DataRow dr = dt.NewRow();
                dr[0] = skuCodeType.SKU;
                dr[1] = list2[0].ProductName;
                dr[2] = i;
                dr[3] = DateTime.Now;
                dr[4] = GetCurrentAccount().Realname;
                dr[5] = "退件入库";
                dr[6] = skuCodeType.Code;
                dt.Rows.Add(dr);
                i++;
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            PrintDataType data = new PrintDataType();
            data.Content = ds.GetXml();
            data.CreateOn = DateTime.Now;
            NSession.Save(data);
            NSession.Flush();
            return Json(new { IsSuccess = true, Result = data.Id });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OrderReturnRecordType GetById(int Id)
        {
            OrderReturnRecordType obj = NSession.Get<OrderReturnRecordType>(Id);
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
            OrderReturnRecordType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(OrderReturnRecordType obj)
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
                OrderReturnRecordType obj = GetById(id);
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
            string orderby = " order by Id desc ";
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
            IList<OrderReturnRecordType> objList = NSession.CreateQuery("from OrderReturnRecordType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<OrderReturnRecordType>();

            object count = NSession.CreateQuery("select count(Id) from OrderReturnRecordType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

    }
}

