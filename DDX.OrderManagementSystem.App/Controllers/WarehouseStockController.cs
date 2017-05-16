using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using Newtonsoft.Json;
using System.Collections;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class WarehouseStockController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ViewResult TodayIndex()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        public ViewResult MonthlyReport()
        {
            return View();
        }
        public ViewResult OutInDetails(String sku, String Warehouse)
        {
            ViewData["sku"] = sku;
            ViewData["Warehouse"] = Warehouse;
            return View();
        }
        public ViewResult ShowDiscountRecord(string PlanNo)
        {
            ViewData["PlanNo"] = PlanNo;
            return View();
        }
        public JsonResult ListDetails(String sku, String Warehouse, int page, int rows)
        {
            //对于包含union的sql语句进行分页会有问题,手动进行分页操作
            List<OutInDetailsType> objListsort = NSession.CreateSQLQuery("select * from(select (Id+CreateOn)as Id,CreateOn,''as OrderNo,InType as 'OutInType',WName,Qty,SourceQty,CreateBy,'入库'as Type from StockIn where SKU='" + sku + "' and WName='" + Warehouse + "' union select  (Id+CreateOn)as Id,CreateOn,OrderNo,OutType as 'OutInType',WName,Qty,SourceQty,CreateBy,'出库'as Type from StockOut where SKU='" + sku + "' and WName='" + Warehouse + "')as tb1 order by CreateOn").AddEntity(typeof(OutInDetailsType)).List<OutInDetailsType>().ToList();
            List<OutInDetailsType> objList = new List<OutInDetailsType>();
            for (int i = rows * (page - 1); i < rows * page && i < objListsort.Count; i++)
            {
                objList.Add(objListsort[i]);
            }
            return Json(new { total = objListsort.Count, rows = objList });
        }
        [HttpPost]
        public JsonResult Create(WarehouseStockType obj)
        {
            try
            {
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 订单列表内超时订单主管审批
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckPassCharge(string p)
        {
            IList<UserType> list = base.NSession.CreateQuery("from UserType where Realname in ('陈祖麒','邵纪银','雷刚')").List<UserType>();
            foreach (var user in list)
            {
                if (GetCurrentAccount().Realname == user.Realname && p == user.Password)
                {
                    return Json(new { IsSuccess = true, Msg = "成功！" });
                }
            }

            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }

        [HttpPost]
        public JsonResult CheckPass(string p)
        {
            //if (GetCurrentAccount().FromArea == "宁波")
            //{
            //    if (p == "us3616")
            //    {
            //        return Json(new { IsSuccess = true, Msg = "成功！" });
            //    }
            //}
            //else if (GetCurrentAccount().FromArea == "义乌")
            //{
            //    if (p == "463382")
            //    {
            //        return Json(new { IsSuccess = true, Msg = "成功！" });
            //    }
            //}
            //else
            //{
            //    if (p == GetCurrentAccount().Password)
            //    {
            //        return Json(new { IsSuccess = true, Msg = "成功！" });
            //    }
            //}
            // 控制重置操作只允许超级管理员和各组长
            /*   IList<UserType> list = base.NSession.CreateQuery(" from UserType where Username='admin'").List<UserType>();
               if (p == list[0].Password)
               {
                   return Json(new { IsSuccess = true, Msg = "成功！" });
               }*/
            //增加邵纪银重置包裹的权限
            IList<UserType> list = base.NSession.CreateQuery("from UserType where Realname in ('管理员','黄伟堒','李培斯','许东旭','陈祖麒','雷刚','张小雪','胡公博','王红燕','周晓伟','金晓霞','梅剑','邵纪银','吕晶晶')").List<UserType>();
            foreach (var user in list)
            {
                if (GetCurrentAccount().Realname == user.Realname && p == user.Password)
                {
                    return Json(new { IsSuccess = true, Msg = "成功！" });
                }
            }


            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }
        [HttpPost]
        public JsonResult CheckStockPass(string realname,string p)
        {
           IList<UserType> list = base.NSession.CreateQuery("from UserType where Realname in ("+realname+")").List<UserType>();
            foreach (var user in list)
            {
                if ( p == user.Password)
                {
                    return Json(new { IsSuccess = true, Msg = "成功！" });
                }
            }


            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }
        /// <summary>
        /// 移库
        /// </summary>
        /// <param name="FromWId">来自仓库</param>
        /// <param name="ToWId">移到仓库</param>
        /// <param name="SKU">商品</param>
        /// <param name="Qty">数量</param>
        /// <param name="TransferCharge">移库费用</param>
        /// <param name="TariffCharge">单位关税</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TransferWarehouse(int FromWId, int ToWId, string SKU, int Qty, double TransferCharge, double TariffCharge)
        {
            //using (ITransaction tx = base.NSession.BeginTransaction())
            //{
            try
            {
                // 获取仓库类型
                WarehouseType FromWarehouse = base.NSession.Get<WarehouseType>(FromWId);
                WarehouseType ToWarehouse = base.NSession.Get<WarehouseType>(ToWId);

                // 非指定方式终 海外仓-》小包仓 
                if (FromWarehouse.Type == "海外仓库" && ToWarehouse.Type == "小包仓库")
                {
                    return Json(new { IsSuccess = false, Msg = "禁止【海外仓-》小包仓】移库操作！" });
                }

                // 根据仓库类型选择计算方式
                double price = 0.00;

                int StockOutId = Utilities.StockOut(FromWId, SKU, Qty, "移库", CurrentUser.Realname,
                                   "移库到" + ToWarehouse.WName, "", NSession);

                // 出库
                StockOutType stockOutType = base.NSession.Get<StockOutType>(StockOutId);
                string Memo = string.Empty;

                // 1、小包仓-》海外仓：成本 =(原价*1.12)+单位运费（RMB）+单位关税（RMB）；
                if (FromWarehouse.Type == "小包仓库" && ToWarehouse.Type == "海外仓库")
                {
                    price = Convert.ToDouble((Convert.ToDouble(stockOutType.Price) * 1.12) + TransferCharge + TariffCharge);
                    Memo = " 成本price =(" + stockOutType.Price + " * 1.12)" + "+单位运费" + TransferCharge + "+单位关税" + TariffCharge;
                }
                // 2、海外仓-》海外仓：成本=原价+单位运费（RMB）+单位关税（RMB）
                if (FromWarehouse.Type == "海外仓库" && ToWarehouse.Type == "海外仓库")
                {
                    price = Convert.ToDouble(stockOutType.Price) + TransferCharge + TariffCharge;
                    Memo = " 成本price =" + stockOutType.Price + "+单位运费" + TransferCharge + "+单位关税" + TariffCharge;
                }

                // 判断当入库到“49 Waste-NB 海外废品(退件仓)”全额折损
                #region
                double BeforeUnitPrice = 0.00;// 记录全额折损单价
                if (ToWId == 49)
                {
                    // 修改批次单价
                    BeforeUnitPrice = price; // 记录全额折损单价
                    price = 0.00;
                }
                #endregion;

                int WarehouseStockDataId = 0; // 入库批次
                // 入库
                Utilities.StockIn(ToWId, SKU, Qty, price, "移库", base.CurrentUser.Realname, "移库来自" + FromWarehouse.WName + Memo, base.NSession, out WarehouseStockDataId, true, false);

                // 海外废品仓库损记录
                #region
                if (ToWId == 49)
                {
                    // 保存折损记录,记录单价
                    GoodsDiscountRecordType gd = new GoodsDiscountRecordType();
                    gd.DepositoryInId = WarehouseStockDataId; // 批次ID
                    gd.BeforeUnitPrice = decimal.Parse(BeforeUnitPrice.ToString());
                    gd.DiscountQty = Qty;
                    gd.DiscountCycle = 15;
                    gd.DiscountRate = 0;
                    gd.GmtCreate = DateTime.Now;
                    NSession.Save(gd);
                }
                #endregion;

                //Utilities.StockIn(ToWId, SKU, Qty, Convert.ToDouble(stockOutType.Price + Charge / Qty), "移库", base.CurrentUser.Realname, "移库来至" + FromWarehouse.WName, base.NSession, true, false);
                //tx.Commit(); // Utilities.StockIn，Utilities.StockOut内已存在事务，此处提示无法提交因此注释
                return Json(new { IsSuccess = true, Msg = "成功！" });
            }
            catch (HibernateException)
            {
                //tx.Rollback();
                return Json(new { IsSuccess = false, Msg = "失败！" });
            }
            //}
        }

        /// <summary>
        ///  重置库存
        /// </summary>
        /// <param name="w">仓库</param>
        /// <param name="o"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditReset(int w, string o)
        {
            try
            {
                WarehouseType warehouse = NSession.Get<WarehouseType>(w);
                o = o.Replace("\r", "").Replace("\t", " ").Replace("  ", " ");
                string[] strArray = o.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                foreach (string str2 in strArray)
                {
                    string[] strArray2 = str2.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray2.Length == 2)
                    {
                        string sku = strArray2[0];
                        int qty = Utilities.ToInt(strArray2[1]);
                        if (string.IsNullOrEmpty(sku)) continue;

                        try
                        {
                            object obj2 = base.NSession.CreateQuery("select count(Id) from WarehouseStockType where SKU='" + strArray2[0] + "' and WId=" + w).UniqueResult();
                            // 获取大于0的对应商品
                            List<WarehouseStockDataType> stockInDataTypes = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + w + " and SKU='" + sku + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();

                            IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + sku + "'").List<ProductType>();
                            base.NSession.Flush();
                            //添加sku到库存数据中
                            if (Utilities.ToInt(obj2) == 0)
                            {
                                if (list.Count > 0)
                                {
                                    Utilities.AddToWarehouse(list[0], CurrentUser.Realname, base.NSession, warehouse.Id, 0);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            // 当无批次时直接创新一个批次
                            if (stockInDataTypes.Count == 0)
                            {
                                //IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", obj.WId).SetString("p2", obj.SKU).List<WarehouseStockType>();
                                // 创建库存明细
                                WarehouseStockDataType stockData = new WarehouseStockDataType();
                                StockInType StockIn = new StockInType();
                                StockIn.IsAudit = 1;
                                StockIn.InType = "冲红操作";
                                StockIn.Memo = "重置库存自动调整";
                                StockIn.Price = Convert.ToDouble(list[0].Price);
                                StockIn.Qty = qty;
                                StockIn.SKU = list[0].SKU;
                                StockIn.WId = warehouse.Id;
                                StockIn.WName = warehouse.WName;
                                StockIn.CreateOn = DateTime.Now;
                                StockIn.CreateBy = GetCurrentAccount().Realname;
                                NSession.Save(StockIn);
                                NSession.Flush();

                                stockData.InId = StockIn.Id;
                                stockData.InNo = StockIn.Id.ToString();
                                stockData.WId = warehouse.Id;
                                stockData.Amount = Utilities.ToDecimal(list[0].Price);
                                stockData.WName = warehouse.WName;
                                stockData.SKU = list[0].SKU;
                                stockData.PId = list[0].Id;
                                stockData.PName = list[0].ProductName;
                                stockData.Qty = stockData.NowQty = qty;
                                stockData.CreateOn = DateTime.Now;
                                stockData.Id = 0;
                                NSession.Save(stockData);
                                NSession.Flush();
                            }

                            bool iscon = true;
                            int nowqty = qty; //重置数量
                            // 先进先出比对
                            foreach (var warehouseStockDataType in stockInDataTypes)
                            {
                                // 当重置数量为0时直接设置当前批次库存数量为0
                                if (nowqty == 0)
                                {
                                    warehouseStockDataType.NowQty = 0;
                                }
                                else
                                {
                                    // 当本批次到货数量大于重置数量,将当前数量直接设置为重置数量
                                    if (warehouseStockDataType.Qty > nowqty)
                                    {
                                        warehouseStockDataType.NowQty = nowqty;
                                        nowqty = 0;
                                    }
                                    else
                                    {
                                        // 判断如果没有下批次直接设置当前批量库存为设置剩余数量
                                        if (stockInDataTypes.Count == stockInDataTypes.IndexOf(warehouseStockDataType) + 1)
                                        {
                                            // 当只有一个批次时直接将该批次设置为重置数量
                                            warehouseStockDataType.NowQty = nowqty;
                                            warehouseStockDataType.Qty = nowqty;
                                            nowqty = 0;
                                        }
                                        else
                                        {
                                            warehouseStockDataType.NowQty = warehouseStockDataType.Qty;
                                            nowqty = nowqty - warehouseStockDataType.Qty;
                                        }
                                    }
                                }
                                NSession.Update(warehouseStockDataType);
                                NSession.Flush();
                            }

                            base.NSession.CreateSQLQuery(" update WarehouseStock set Qty=" + qty + " , UpdateOn='" + str + "'   where Warehouse='" + warehouse.WName + "' and  SKU='" + sku + "'").UniqueResult();
                            base.NSession.Flush();
                            LoggerUtil.GetProductRecord(list[0], "商品库存重置", "重置库存数量为：" + strArray2[1] + " 所在仓库：" + warehouse.WName, CurrentUser, NSession);
                            IList<OrderType> list2 = base.NSession.CreateQuery(" from OrderType where Id in(select OId from OrderProductType where SKU ='" + strArray2[0] + "' ) and  Status in ('已处理') Order By CreateOn Asc ").List<OrderType>();
                            base.NSession.CreateSQLQuery(" update OrderProducts set IsQue=0 where  SKU='" + sku + "'").UniqueResult();
                            foreach (OrderType type in list2)
                            {
                                OrderHelper.SetQueOrder(type, base.NSession);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, Msg = "出错了" });
            }
            return base.Json(new { IsSuccess = true, Msg = "成功！" });
        }


        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WarehouseStockType GetById(int Id)
        {
            WarehouseStockType obj = NSession.Get<WarehouseStockType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }

        [HttpPost]
        public ActionResult Export(string search)
        {
            StringBuilder sb = new StringBuilder();
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and Qty<>0 ";
            }
            else
            {
                where = " where Qty<>0 ";
            }
            //string sql = "select Warehouse,SKU,Qty,Title from WarehouseStock where Qty<>0";
            string sql = "select Warehouse,SKU,Title,Qty,TotalPrice,(select top 1 P.CreateBy from Products P  where P.SKU=WS.SKU) as '创建人'  from(select WS.Id,WS.WId,WS.PId,WS.Warehouse,WS.SKU,WS.Title,Area,(select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as Qty,(select sum(NowQty*Amount) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as TotalPrice  from WarehouseStock WS join Warehouse W on W.WName=WS.Warehouse ) WS" + where;
            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql;
            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);
            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            WarehouseStockType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseStockType obj)
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
                WarehouseStockType obj = GetById(id);
                if (GetCurrentAccount().Username == "hjj")//临时处理仓库库存用
                {
                    NSession.Delete(obj);
                    NSession.Flush();
                }
                else
                {
                    return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }


        public JsonResult TodayList(string sort, string order, string search)
        {
            if (sort == null)
                sort = "Qty";
            if (order == null)
                order = "Desc";
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);

            if (where.Length == 0)
                where = " where 1=1 ";
            where += " and IsAudit=1 and createOn>'" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "'";


            List<object[]> objList = NSession.CreateSQLQuery("select WId,WName as Warehouse,SKU,SUM(Qty) as 'Qty' from StockIn " + where + " group by WId,WName,SKU " + orderby)

               .List<object[]>().ToList();

            string ids = "";
            foreach (var objs in objList)
            {
                ids += objs[2] + "','";
            }
            if (ids.Length > 0)
            {
                //ids = ids.Trim(',');
            }
            List<ProductType> productTypes =
                NSession.CreateQuery(" from  ProductType where SKU in('" + ids + "')").List<ProductType>().ToList();
            List<WarehouseStockType> warehouseStockTypes = new List<WarehouseStockType>();
            int tt = 1;
            double totalPrice = 0;
            foreach (object[] objs in objList)
            {

                WarehouseStockType ws = new WarehouseStockType();
                ws.Id = tt;
                tt++;
                ws.SKU = objs[2].ToString();
                ws.Qty = Utilities.ToInt(objs[3].ToString());
                ws.WId = Utilities.ToInt(objs[0].ToString());
                ws.Warehouse = objs[1].ToString();
                ProductType p = productTypes.Find(x => x.SKU == ws.SKU);
                if (p != null)
                {
                    ws.Pic = p.PicUrl;
                    ws.Price = p.Price;
                    ws.TotalPrice = p.Price * ws.Qty;
                    totalPrice += ws.TotalPrice;
                }
                warehouseStockTypes.Add(ws);
            }




            List<WarehouseStockType> list3 = new List<WarehouseStockType> {
                new WarehouseStockType{ TotalPrice = totalPrice}
            };
            return Json(new { total = warehouseStockTypes.Count, rows = warehouseStockTypes, footer = list3 });
        }

        /// <summary>
        /// 获取月结报表
        /// </summary>
        /// <param name="DepositoryId"></param>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <param name="Sku"></param>
        /// <returns></returns>
        //public string GetMonthlyReport(int DepositoryId, int Year, int Month, string Sku)
        public string GetMonthlyReport(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            int DepositoryId = int.Parse(search.Split('&')[0]);
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string Sku = search.Split('&')[3];
            //int Year = 2016;
            //int Month = 12;
            //string Sku = "0";

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UP_MonthlyReport";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@DepositoryId";
            parameter1.Value = DepositoryId;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Year";
            parameter2.Value = Year;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@Month";
            parameter3.Value = Month;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@Sku";
            parameter4.Value = Sku;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }

        [HttpPost]
        public ActionResult ExportMonthlyReport(int DepositoryId, int Year, int Month, string Sku)
        {
            //if (search == null) { return ""; };

            //int DepositoryId = int.Parse(search.Split('&')[0]);
            //int Year = int.Parse(search.Split('&')[1]);
            //int Month = int.Parse(search.Split('&')[2]);
            //string Sku = search.Split('&')[3];
            ////int Year = 2016;
            ////int Month = 12;
            ////string Sku = "0";
            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UP_MonthlyReport";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@DepositoryId";
            parameter1.Value = DepositoryId;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Year";
            parameter2.Value = Year;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@Month";
            parameter3.Value = Month;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@Sku";
            parameter4.Value = Sku;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {

            //string orderby = Utilities.OrdeerBy(sort, order);
            string orderby = " order by " + "WS.Id desc";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
            }
            else
            {
                where = " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
            }

            //List<WarehouseStockType> objList = NSession.CreateSQLQuery("select *,(Qty*(select Price from Products where Products.SKU=WS.SKU)) as TotalPrice from WarehouseStock WS" + where + orderby).AddEntity(typeof(WarehouseStockType))
            //   .SetFirstResult(rows * (page - 1))
            //   .SetMaxResults(rows)
            //   .List<WarehouseStockType>().ToList();

            // TotalPrice作用，此处只做排序
            // string str="select *,(select sum(NowQty*Amount) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as TotalPrice from WarehouseStock WS left join Warehouse W on W.WName=WS.Warehouse" + where + orderby;


            List<DataDictionaryDetailType> list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day1'");
            string Day1 = list[0].DicValue;
            list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day2'");
            string Day2 = list[0].DicValue;
            list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day3'");
            string Day3 = list[0].DicValue;
            list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='FutureDays'");
            string FutureDays = list[0].DicValue;

            // List<WarehouseStockType> objList = NSession.CreateSQLQuery("select * from (select WS.Id,WS.WId,WS.Warehouse,WS.PId,WS.SKU,WS.Title,Area,(select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as Qty,WS.Pic,WS.UpdateOn,WS.FromArea,WS.UpdateBy,(select sum(NowQty*Amount) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as TotalPrice, Convert(float,[dbo].[F_GetParc1](" + Day1 + "," + Day2 + "," + Day3 + "," + FutureDays + ",SKU,WS.WId)) as Parc,[dbo].[F_GetParc](" + Day1 + "," + Day2 + "," + Day3 + "," + FutureDays + ",SKU,WS.WId)as Formula,[dbo].[F_GetAllQty](SKU)as AllQty from WarehouseStock WS join Warehouse W on W.WName=WS.Warehouse) WS" + where + orderby).AddEntity(typeof(WarehouseStockType))
            List<WarehouseStockType> objList = NSession.CreateSQLQuery("select * from (select WS.Id,WS.WId,WS.Warehouse,WS.PId,WS.SKU,WS.Title,W.Area,(select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as Qty,WS.Pic,WS.UpdateOn,WS.FromArea,WS.UpdateBy,(select sum(NowQty*Amount) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as TotalPrice, (select (select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) ) as Location, Convert(float,[dbo].[F_GetParc1](" + Day1 + "," + Day2 + "," + Day3 + "," + FutureDays + ",WS.SKU,WS.WId)) as Parc,[dbo].[F_GetParc](" + Day1 + "," + Day2 + "," + Day3 + "," + FutureDays + ",WS.SKU,WS.WId)as Formula,[dbo].[F_GetAllQty](WS.SKU)as AllQty from WarehouseStock WS join Warehouse W on W.WName=WS.Warehouse join Products P on WS.SKU=P.SKU) WS" + where + orderby).AddEntity(typeof(WarehouseStockType))
               .SetFirstResult(rows * (page - 1))
               .SetMaxResults(rows)
               .List<WarehouseStockType>().ToList();

            /*           List<WarehouseStockType> objList = NSession.CreateSQLQuery("select *,(select sum(NowQty*Amount) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID) as TotalPrice from WarehouseStock WS" + where + orderby).AddEntity(typeof(WarehouseStockType))
                          .SetFirstResult(rows * (page - 1))
                          .SetMaxResults(rows)
                          .List<WarehouseStockType>().ToList();
                       */
            string ids = "";
            foreach (var warehouseStockType in objList)
            {
                ids += warehouseStockType.SKU + "','";
            }
            if (ids.Length > 0)
            {
                // ids = ids.Trim(',');
            }
            List<ProductType> productTypes =
                NSession.CreateQuery(" from  ProductType where SKU in('" + ids + "')").List<ProductType>().ToList();

            foreach (WarehouseStockType ws in objList)
            {
                ProductType p = productTypes.Find(x => x.SKU == ws.SKU);
                if (p != null)
                {
                    ws.Pic = p.PicUrl;
                    ws.Price = p.Price;
                }
            }
            foreach (WarehouseStockType ws in objList)
            {
                ProductType p = productTypes.Find(x => x.SKU == ws.SKU);
                if (p != null)
                {
                    ws.Pic = p.PicUrl;
                    ws.Price = p.Price;
                }

                IList<WarehouseStockDataType> wsList = NSession.CreateQuery("from WarehouseStockDataType  Where NowQty>0 and WId=" + ws.WId + " and PId=" + ws.PId).List<WarehouseStockDataType>();

                foreach (WarehouseStockDataType obj in wsList)
                {
                    ws.TotalPrice = ws.TotalPrice + Convert.ToDouble(obj.NowQty * obj.Amount);
                    //ws.Qty = ws.Qty + obj.NowQty;
                    //totalPrice += ws.TotalPrice;
                }

                //IList<WarehouseStockDataType> wsList = NSession.CreateQuery("from WarehouseStockDataType  Where NowQty>0 and WId=" + ws.WId + " and PId=" + ws.PId + orderby).List<WarehouseStockDataType>();

                //foreach (WarehouseStockDataType obj in wsList)
                //{
                //    ws.TotalPrice = ws.TotalPrice + Convert.ToDouble(obj.NowQty * obj.Amount);
                //    //totalPrice += ws.TotalPrice;
                //}

            }
            //IList<object[]> objs =
            //    NSession.CreateQuery("select SKU,COUNT(Id) from SKUCodeType where SKU in('" + ids.Replace(",", "','") + "') and IsOut=0 group by SKU ").List<object[]>();
            //foreach (var objectse in objs)
            //{
            //    WarehouseStockType warehouse =
            //    objList.Find(x => x.SKU.Trim().ToUpper() == objectse[0].ToString().Trim().ToUpper());
            //    if (warehouse != null)
            //    {
            //        warehouse.UnPeiQty = Convert.ToInt32(objectse[1]);
            //        if (warehouse.UnPeiQty == 0)
            //        {
            //            warehouse.UnPeiQty = warehouse.Qty;
            //        }
            //    }
            //}

            //objList.Sort(Compare);
            //object count = NSession.CreateQuery("select count(Id) from WarehouseStockType " + where.Replace("WS.", "")).UniqueResult();
            //object count = NSession.CreateQuery("select count(WSS.Id) from WarehouseStockType WSS left join WarehouseType W on W.WName=WSS.Warehouse" + where.Replace("WS.", "")).UniqueResult();
            object count = null;
            object SUM = null;
            object SUM1 = null;

            //int count =null;
            if (where.Contains("Area"))
            {

                string a = "select count(0) as count from WarehouseStock WS  join Warehouse W on W.WName=WS.Warehouse ";
                string b = "select sum(NowQty*amount) as Totle from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse";
                string c = "select sum(NowQty) as Qty from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse ";
                string where1 = Utilities.SqlWhere(search);
                if (where1.Length > 0)
                {
                    a += where1.Replace("WS.", "").Replace("Area", "W.Area") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    b += where1.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId").Replace("Area", "W.Area") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    c += where1.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId").Replace("Area", "W.Area") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                }
                else
                {
                    a += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    b += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    c += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                }
                count = NSession.CreateSQLQuery(a).UniqueResult();
                SUM = NSession.CreateSQLQuery(b).UniqueResult();
                SUM1 = NSession.CreateSQLQuery(c).UniqueResult();

            }
            else
            {
                //string a = "select count(0) as count from WarehouseStock WS  join Warehouse W on W.WName=WS.Warehouse " + where.Replace("WS.", "");
                //string b = "select sum(NowQty*amount) as Totle from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse" + where.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId");
                //string c = "select sum(NowQty) as Qty from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse " + where.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId");
                string a = "select count(0) as count from WarehouseStock WS  join Warehouse W on W.WName=WS.Warehouse ";
                string b = "select sum(NowQty*amount) as Totle from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse";
                string c = "select sum(NowQty) as Qty from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId  join Warehouse W on W.WName=WS.Warehouse ";
                string where1 = Utilities.SqlWhere(search);
                if (where1.Length > 0)
                {
                    a += where1.Replace("WS.", "") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 "; ;
                    b += where1.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    c += where1.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId") + " and (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                }
                else
                {
                    a += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    b += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                    c += " where (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and WId=WS.WId and PId=WS.PID)>0 ";
                }
                count = NSession.CreateSQLQuery(a).UniqueResult();
                SUM = NSession.CreateSQLQuery(b).UniqueResult();
                SUM1 = NSession.CreateSQLQuery(c).UniqueResult();

                /*   count = NSession.CreateQuery("select count(Id) from WarehouseStockType " + where.Replace("WS.", "")).UniqueResult();
                   SUM = NSession.CreateSQLQuery("select sum(NowQty*amount) as Totle from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId " + where.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId")).UniqueResult();
                   SUM1 = NSession.CreateSQLQuery("select sum(NowQty) as Qty from WarehouseStockData left join WarehouseStock WS on WarehouseStockData.WId=WS.WId and WS.PId=WarehouseStockData.PId " + where.Replace("Qty", "WS.Qty").Replace("WId", "WS.WId")).UniqueResult();*/



            }
            // 排序库存总数量为0，但批次数量又存在时计算
            //object SUM = NSession.CreateSQLQuery("select SUM((case when Qty <0 then 0 else qty end)*Price) from (select Qty,(select isnull( Price,0) from Products where  Products.SKU=WarehouseStock.SKU) as Price from WarehouseStock " + where + ") as tbl ").UniqueResult();
            //object SUM = NSession.CreateSQLQuery("select sum(NowQty*amount) as Totle from WarehouseStockData " + where).UniqueResult();
            //object SUM1 = NSession.CreateSQLQuery("select sum(NowQty) as Qty from WarehouseStockData " + where).UniqueResult();




            if (SUM == null)
            {
                SUM = 0;
            }
            if (SUM1 == null)
            {
                SUM1 = 0;
            }
            List<WarehouseStockType> list3 = new List<WarehouseStockType> {

                new WarehouseStockType{TotalPrice =Utilities.ToDouble(SUM.ToString()),Qty=Convert.ToInt32( SUM1.ToString()) }

            };
            return Json(new { total = count, rows = objList, footer = list3 });
        }
        /// <summary>
        /// 获取折扣详细信息
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="Warehouse"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult DiscountRecord(string planNo, int page, int rows)
        {
            IList<GoodsDiscountRecordType> list = NSession.CreateQuery(" from GoodsDiscountRecordType where DepositoryInId='" + planNo + "'").SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows * page)
                .List<GoodsDiscountRecordType>();
            return base.Json(new { total = list.Count, rows = list });
        }


        // 记录集内排序
        //public int Compare(WarehouseStockType x, WarehouseStockType y)
        //{
        //    if (x.TotalPrice > y.TotalPrice)
        //    {
        //        return -1;
        //    }
        //    else
        //    {
        //        return 1;
        //    }
        //}
    }

}

