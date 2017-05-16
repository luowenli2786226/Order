using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using System.Data.SqlClient;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class StockInController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        public ActionResult AutoInOut()
        {
            StockInType inType = new StockInType();
            inType.Qty = 1;
            inType.Price = 0;
            return View(inType);
        }

        [HttpPost]
        public JsonResult Create(StockInType obj)
        {
            try
            {
                if (obj.Price <= 0)
                {
                    return Json(new { IsSuccess = false, ErrorMsg = "商品单价必须大于零!" });
                }
                //if (obj.Qty <= 0)
                //{
                //    return Json(new { IsSuccess = false, ErrorMsg = "商品数量必须大于零!" });
                //}
                ///当入库的仓库为宁波仓库时，只允许戚波，陈尔，吕晶晶有权限入库
                if (obj.WId == 1)
                {
                    bool CanStockIn = false;
                    IList<UserType> userlist = base.NSession.CreateQuery("from UserType where Realname in ('吕晶晶','陈尔','戚波')").List<UserType>();
                    foreach (var user in userlist)
                    {
                        if (GetCurrentAccount().Realname == user.Realname)
                        {
                            CanStockIn = true;
                        }
                    }
                    if (!CanStockIn)
                    {
                        return Json(new { IsSuccess = false, ErrorMsg = "当前登录人员没有权限，入库权限人员为戚波，陈尔，吕晶晶" });
                    }
                }
                IList<WarehouseStockType> list =
                    NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).
                        SetString("p2", obj.SKU).List<WarehouseStockType>();
                if (list.Count > 0)
                {
                    WarehouseType w = Get<WarehouseType>(obj.WId);
                    obj.CreateOn = DateTime.Now;
                    obj.WName = w.WName;

                    obj.IsAudit = 0;
                    obj.CreateBy = GetCurrentAccount().Realname;
                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                }
                else
                {
                    IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + obj.SKU + "'").List<ProductType>();
                    if (productTypes.Count > 0)
                    {
                        WarehouseType w = Get<WarehouseType>(obj.WId);
                        Utilities.AddToWarehouse(productTypes[0], GetCurrentAccount().Realname, NSession, obj.WId, 0);
                        obj.Price = productTypes[0].Price;
                        obj.CreateOn = DateTime.Now;
                        obj.WName = w.WName;
                        obj.IsAudit = 0;
                        obj.CreateBy = GetCurrentAccount().Realname;
                        NSession.SaveOrUpdate(obj);
                        NSession.Flush();
                    }
                    else
                    {
                        return Json(new { IsSuccess = false, ErrorMsg = "系统中没有这个产品" });
                    }
                }

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }


        [HttpPost]
        public JsonResult CreateAutoInOut(StockInType obj)
        {
            try
            {
                WarehouseType warehouseType = NSession.Get<WarehouseType>(obj.WId);
                IList<WarehouseStockType> list =
                    NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).
                        SetString("p2", obj.SKU).List<WarehouseStockType>();
                if (list.Count > 0)
                {
                    IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + obj.SKU + "'").List<ProductType>();
                    if (productTypes.Count > 0)
                    {
                        obj.Price = productTypes[0].Price;
                    }
                    WarehouseType w = Get<WarehouseType>(obj.WId);
                    obj.CreateOn = DateTime.Now;
                    obj.WName = w.WName;
                    obj.IsAudit = 1;
                    obj.CreateBy = GetCurrentAccount().Realname;
                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                }
                else
                {
                    IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + obj.SKU + "'").List<ProductType>();
                    if (productTypes.Count > 0)
                    {
                        Utilities.AddToWarehouse(productTypes[0], GetCurrentAccount().Realname, NSession, warehouseType.Id, 0);

                        obj.CreateOn = DateTime.Now;
                        obj.WName = warehouseType.WName;
                        obj.IsAudit = 1;
                        //obj.Price = productTypes[0].Price;
                        obj.CreateBy = GetCurrentAccount().Realname;
                        NSession.SaveOrUpdate(obj);
                        NSession.Flush();
                    }
                }

                Utilities.CreateSKUCode(obj.SKU, obj.Qty, "", obj.Id.ToString(), NSession);
                // DoAudit(obj.Id);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        public JsonResult DoAudit(int Id)
        {
            try
            {
                StockInType obj = GetById(Id);
                obj.IsAudit = 1;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
                IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).SetString("p2", obj.SKU).List<WarehouseStockType>();
                if (list.Count > 0)
                {
                    WarehouseStockType ws = list[0];
                    ws.Qty = ws.Qty + obj.Qty;
                    NSession.SaveOrUpdate(ws);
                    NSession.Flush();
                    Utilities.SetComposeStock(obj.SKU, obj.WId, GetCurrentAccount().Realname, NSession);
                    Utilities.CreateSKUCode(obj.SKU, obj.Qty, "", obj.Id.ToString(), NSession);

                    IList<OrderType> list2 = base.NSession.CreateQuery(" from OrderType where Id in(select OId from OrderProductType where SKU ='" + obj.SKU + "' ) and  Status in ('已处理') Order By CreateOn Asc ").List<OrderType>();
                    base.NSession.CreateSQLQuery(" update OrderProducts set IsQue=0 where  SKU='" + obj.SKU + "'").UniqueResult();
                    foreach (OrderType type in list2)
                    {
                        OrderHelper.SetQueOrder(type, base.NSession);
                    }
                    // 当库存数量不为0时，对库存批次进行操作
                    if (ws.Qty != 0)
                    {
                        // 查获库存明细是存在，存在减库存，相反创建库存明细
                        if (obj.Qty < 0)
                        {
                            // 负数减库存明细
                            List<WarehouseStockDataType> WarehouseStockDataList = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + obj.WId + " and SKU='" + obj.SKU + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();
                            if (WarehouseStockDataList.Count > 0)
                            {
                                int fooQty = obj.Qty;
                                // 减库存明细
                                foreach (var stockInDataType in WarehouseStockDataList)
                                {
                                    if (stockInDataType.NowQty > fooQty)
                                    {
                                        // 批次匹配
                                        fooQty = (fooQty) + stockInDataType.NowQty;
                                        stockInDataType.NowQty = fooQty < 0 ? 0 : fooQty;
                                        NSession.Clear();
                                        NSession.Update(stockInDataType);
                                        NSession.Flush();
                                    }
                                    if (fooQty >= 0) break;
                                }
                            }
                        }
                        else
                        {
                            // 创建库存明细
                            WarehouseStockDataType stockData = new WarehouseStockDataType();

                            stockData.InId = obj.Id;
                            stockData.InNo = obj.Id.ToString();
                            stockData.WId = ws.WId;
                            stockData.Amount = Utilities.ToDecimal(obj.Price);
                            stockData.WName = ws.Warehouse;
                            stockData.SKU = ws.SKU;
                            stockData.PId = ws.PId;
                            stockData.PName = ws.Title;
                            stockData.Qty = stockData.NowQty = obj.Qty;
                            stockData.CreateOn = DateTime.Now;
                            stockData.Id = 0;
                            NSession.Save(stockData);
                            NSession.Flush();
                        }
                    }
                }

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public ActionResult PrintSKU(string Id)
        {
            List<StockInType> objs = NSession.CreateQuery("from StockInType where Id in(" + Id + ")").List<StockInType>().ToList();
            if (objs != null)
            {
                NSession.Flush();
                DataTable dt = new DataTable();
                dt.Columns.Add("sku");
                dt.Columns.Add("name");
                dt.Columns.Add("num");
                dt.Columns.Add("date");
                dt.Columns.Add("people");
                dt.Columns.Add("desc");
                dt.Columns.Add("code");
                dt.Columns.Add("库位");
                foreach (StockInType obj in objs)
                {
                    IList<SKUCodeType> list =
                                       NSession.CreateQuery("from SKUCodeType where SId='" + obj.Id + "'").
                                         List<SKUCodeType>();
                    IList<ProductType> list2 = NSession.CreateQuery("from ProductType where SKU='" + obj.SKU + "'").
                           List<ProductType>(); ;
                    int i = 1;
                    foreach (SKUCodeType skuCodeType in list)
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = skuCodeType.SKU;
                        dr[1] = list2[0].ProductName;
                        dr[2] = i + "/" + obj.Qty;
                        dr[3] = obj.CreateOn;
                        dr[4] = obj.CreateBy;
                        dr[5] = "手动入库";
                        dr[6] = skuCodeType.Code;
                        dr[7] = list2[0].Location;
                        dt.Rows.Add(dr);
                        i++;
                    }
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
            return Json(new { IsSuccess = false });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public StockInType GetById(int Id)
        {
            StockInType obj = NSession.Get<StockInType>(Id);
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
            StockInType obj = GetById(id);
            ViewData["sku"] = obj.SKU;
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(StockInType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                StockInType obj = GetById(id);
                NSession.Delete("from SKUCodeType where SId='" + obj.Id + "'");
                NSession.Flush();
                if (obj.IsAudit == 1)
                {
                    Utilities.StockOut(obj.WId, obj.SKU, obj.Qty, "入库删除", CurrentUser.Realname, "", "", NSession);
                }

                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            IList<StockInType> objList = NSession.CreateQuery("from StockInType" + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<StockInType>();

            object count = NSession.CreateQuery("select count(Id) from StockInType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        public JsonResult ToExcel(string search)
        {
            try
            {
                List<StockInType> objList = NSession.CreateQuery("from StockInType " + Utilities.SqlWhere(search))
                    .List<StockInType>().ToList();
                if (objList.Count == 0)
                {
                    Session["ExportDown"] = "";
                    return Json(new { IsSuccess = false, ErrorMsg = "条件记录为空" });
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((objList)));

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

    }
}

