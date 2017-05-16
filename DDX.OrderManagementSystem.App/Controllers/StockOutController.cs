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
using System.Data.SqlClient;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class StockOutController : BaseController
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
        public JsonResult Create(StockOutType obj)
        {
            try
            {
                //if (obj.Qty <= 0)
                //{
                //    return Json(new { IsSuccess = false, ErrorMsg = "商品数量必须大于零!" });
                //}

                IList<StockOutType> StockOutList = NSession.CreateQuery("from StockOutType where Enabled=0 and OrderNo=:p1 and SKU=:p2").SetString("p1", obj.OrderNo).SetString("p2", obj.SKU).List<StockOutType>();

                WarehouseType warehouse = Get<WarehouseType>(obj.WId);
                obj.WName = warehouse.WName;
                obj.CreateBy = CurrentUser.Realname;
                obj.CreateOn = DateTime.Now;
                obj.IsAudit = 0;
                // 移库无订单号
                if (!string.IsNullOrEmpty(obj.OrderNo))
                {
                    obj.Price = StockOutList[0].Price;
                }
                //if (string.IsNullOrEmpty(obj.OrderNo))
                //{
                //    obj.OrderNo = DateTime.Now.Ticks.ToString();
                //}
                NSession.Save(obj);
                NSession.Flush();
                IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).SetString("p2", obj.SKU).List<WarehouseStockType>();
                if (list.Count > 0)
                {

                }
                // bool iscon = Utilities.StockOut(obj.WId, obj.SKU, obj.Qty, obj.OutType, CurrentUser.Realname, obj.Memo, obj.OrderNo, NSession);
                return Json(new { IsSuccess = true, ErrorMsg = "本地库存数量不足" });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult GetStockOutByCode(string o)
        {
            IList<StockOutType> list = NSession.CreateQuery("from StockOutType where OrderNo='" + o + "'").List<StockOutType>().ToList<StockOutType>();
            if (list.Count > 0)
            {
                string str2;
                StockOutType type = list[0];

                if (type.IsAudit != 1)
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 订单状态不对" });
                }

                str2 = "订单:" + type.OrderNo + ", 可以出库产品";
                str2 += "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='300px' >图片</th><td width='200px'>SKU*数量</td><td>规格</td><td>扫描次数</td>\r\n                                                        </tr>";
                string format = "<tr style='font-weight:bold; font-size:30px;' name='tr_{0}' code='{3}' qty='{1}' cqty='{4}'><td><img width=180px' src='{6}' /></td><td>{0}*{1}</td><td>{2}({5})</td><td><span><span id='r_{3}' style='color:red'>{4}</span>/<span style='color:green'>{1}</span></td></tr>";
                string str4 = "";

                IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                if (list2.Count > 0)
                {

                    str2 = str2 + string.Format(format, new object[] { type.SKU.Trim().ToUpper(), type.Qty, list2[0].Standard, type.Id, 0, list2[0].ProductAttribute, list2[0].PicUrl });


                }

                str2 = str2 + "</table>";

                return base.Json(new { IsSuccess = true, Result = str2 });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }


        [HttpPost]
        public JsonResult CreateByScan(string o, string w2, int w, string m, string t, int s)
        {
            try
            {
                IList<SKUCodeType> list =
                    NSession.CreateQuery("from SKUCodeType where Code=:p").SetInt32("p", s).List<SKUCodeType>();
                if (list.Count > 0)
                {
                    if (list[0].IsOut == 1 || list[0].IsSend == 1)
                    {
                        return Json(new { IsSuccess = false, Result = "该条码已经出库！" });
                    }

                    WarehouseType warehouseType = NSession.Get<WarehouseType>(w);
                    StockOutType stockOutType = new StockOutType();
                    stockOutType.WName = warehouseType.WName;
                    stockOutType.CreateBy = GetCurrentAccount().Realname;
                    stockOutType.CreateOn = DateTime.Now;
                    stockOutType.IsAudit = 1;
                    stockOutType.OrderNo = list[0].Code.ToString();
                    stockOutType.Qty = 1;
                    stockOutType.SKU = list[0].SKU;
                    stockOutType.OutType = t;

                    stockOutType.WId = warehouseType.Id;
                    stockOutType.Memo = m;
                    NSession.Save(stockOutType);
                    NSession.Flush();

                    Utilities.StockOut(w, list[0].SKU, 1, t, CurrentUser.Realname, m, o, NSession);
                    NSession.CreateQuery("update SKUCodeType set IsOut=1,IsSend=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='扫描出库' where Code=" + s).ExecuteUpdate();
                    return Json(new { IsSuccess = true, Result = "扫描完成！产品：" + list[0].SKU + "已经出库，出数量为1!!" });
                }
                return Json(new { IsSuccess = false, Result = "条码错误！无法找到这个产品" });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, Result = "出错了" });
            }

        }

        [HttpPost]
        public JsonResult DoAudit(int Id)
        {
            try
            {
                StockOutType obj = GetById(Id);
                obj.IsAudit = 1;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
                IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).SetString("p2", obj.SKU).List<WarehouseStockType>();
                if (list.Count > 0)
                {
                    Utilities.StockOut(obj, NSession);

                }

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }


        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public StockOutType GetById(int Id)
        {
            StockOutType obj = NSession.Get<StockOutType>(Id);
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
        public ActionResult Edit()
        {

            return View();
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(StockOutType obj)
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
                StockOutType obj = GetById(id);
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
            IList<StockOutType> objList = NSession.CreateQuery("from StockOutType" + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<StockOutType>();

            object count = NSession.CreateQuery("select count(Id) from StockOutType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        public JsonResult ToExcel(string search)
        {
            try
            {
                List<StockOutType> objList = NSession.CreateQuery("from StockOutType " + Utilities.SqlWhere(search))
                    .List<StockOutType>().ToList();
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

