using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using DDX.OrderManagementSystem.App.Common.Utils;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using Newtonsoft.Json;
using System.Collections;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class PickingListController : BaseController
    {
        //
        // GET: /PickingList/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /PickingList/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /PickingList/Create

        public ActionResult CreateList()
        {
            return View();
        }

        //
        // POST: /PickingList/Create
        public ActionResult PrintList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /PickingList/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /PickingList/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /PickingList/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /PickingList/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                search = " where Status='已处理' and Enabled=1 and PickId=0 and IsOutOfStock=0 and IsAudit!=0 and IsError=0  and  " + Utilities.Resolve(search, true);

            }
            search = search.Replace("OrderProductType", "OrderProducts");
            IList<PickingListType> list2 = new List<PickingListType>();
            //base.NSession.CreateQuery("from OrderType " + str ).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<PickingListType>();
            PickingListType type = new PickingListType();
            //订单数
            object obj = base.NSession.CreateSQLQuery("select count(Id) from Orders " + search).UniqueResult();
            type.OrderCount = Convert.ToInt32(obj);
            //SKU品种数
            object obj2 = base.NSession.CreateSQLQuery("select count(distinct(sku)) from OrderProducts where OrderNo in (select OrderNo from Orders " + search + ")").UniqueResult();
            //货品总数
            object obj3 = base.NSession.CreateSQLQuery("select Sum(Qty) from OrderProducts where OrderNo in (select OrderNo from Orders " + search + ")").UniqueResult();
            type.SkuCategory = Convert.ToInt32(obj2);
            type.SKUcount = Convert.ToInt32(obj3);
            list2.Add(type);
            return base.Json(new { total = list2.Count, rows = list2 });
        }
        public JsonResult List2(int page, int rows, string sort, string order, string search)
        {

            if (!string.IsNullOrEmpty(search))
            {
                search = " where Status='已处理' and Enabled=1 and PickId=0 and IsOutOfStock=0 and IsAudit!=0 and IsError=0  and LogisticMode<> '' and " + Utilities.Resolve(search, true);

            }
            search = search.Replace("OrderProductType", "OrderProducts");
            IList<PickingLogicsType> list = new List<PickingLogicsType>();
            PickingLogicsType type = new PickingLogicsType();
            //订单数
            List<object[]> p = base.NSession.CreateSQLQuery("select count(Id) OrderCount2,LogisticMode from Orders " + search + " group by LogisticMode").List<object[]>().ToList();
            foreach (object[] o in p)
            {

                type.OrderCount2 = Convert.ToInt32(o[0]);
                type.LogisticsMode2 = o[1].ToString();
                list.Add(type);
                type = new PickingLogicsType();
            }
           
          
            return base.Json(new { total = list.Count, rows = list });
        }
        /// <summary>
        /// 物流列表
        /// </summary>
        /// <param name="fooList"></param>
        /// <returns></returns>
        public JsonResult ParentLogicsList()
        {
            IList<LogisticsType> objList = NSession.CreateQuery(" from  LogisticsType").List<LogisticsType>();
            IList<LogisticsModeType> objList1 = NSession.CreateQuery("from LogisticsModeType").List<LogisticsModeType>();
            SystemTree tree = new SystemTree { id = "0", text = "根目录" };
            List<LogisticsType> fristList = objList.ToList();
            List<SystemTree> trees = new List<SystemTree>();
            int count = objList.Count;
            foreach (LogisticsType obj in fristList)
            {
                List<LogisticsModeType> fooList = objList1.Where(p => p.ParentID == Convert.ToInt32(obj.Id)).ToList();
                List<SystemTree> tree2 = ConvertToTree(fooList);
                tree.children.Add(new SystemTree { id = obj.Id.ToString() + count, text = obj.LogisticsName, children = tree2 });
            }
            trees.Add(tree);
            return Json(trees);
        }
        public List<SystemTree> ConvertToTree(List<LogisticsModeType> fooList)
        {
            List<SystemTree> tree = new List<SystemTree>();
            foreach (LogisticsModeType item in fooList)
            {
                tree.Add(new SystemTree { id = item.Id.ToString(), text = item.LogisticsName });
            }
            return tree;

        }
        public JsonResult QListSearch()
        {
            IList<WarehouseType> objList = new List<WarehouseType>();
            objList.Insert(0, new WarehouseType() { WCode = "宁波仓库", WName = "宁波仓库" });
            objList.Insert(0, new WarehouseType() { WCode = "义乌仓库", WName = "义乌仓库" });
            return Json(objList);
        }
        public JsonResult QListSearchPrint()
        {
            IList<WarehouseType> objList = new List<WarehouseType>();
            objList.Insert(0, new WarehouseType() { WCode = "ALL", WName = "ALL" });
            objList.Insert(0, new WarehouseType() { WCode = "宁波仓库", WName = "宁波仓库" });
            objList.Insert(0, new WarehouseType() { WCode = "义乌仓库", WName = "义乌仓库" });
            return Json(objList);
        }
        /// <summary>
        /// 生成拣货单
        /// </summary>
        /// <returns></returns>
        public JsonResult CreatePickingList(string search, string warehouse, string pickingtype, string logisticmode)
        {

            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    List<int> pickidlist = new List<int>();
                    string strsearch = " where Status='已处理' and Enabled=1 and PickId=0 and IsOutOfStock=0 and IsAudit!=0 and IsError=0  and ";
                    string newsearch = "";
                    //如果拣货单类型没有选择，那么根据三种类型选择生成多张
                    if (pickingtype == "ALL")
                    {

                        List<string> data = new List<string>();
                        foreach (string str in Enum.GetNames(typeof(COnfigTempleCategory)))
                        {
                            data.Add(str);
                        }
                        for (int i = 0; i < data.Count; i++)
                        {
                            newsearch = search;
                            if (!string.IsNullOrEmpty(search))
                            {
                                newsearch += "OrderPdtType &" + data[i] + "^";
                                strsearch += Utilities.Resolve(newsearch, true);
                            }
                            strsearch = strsearch.Replace("OrderProductType", "OrderProducts");

                            //如果拣货单发货方式没有选择，那么根据多种发货方式生成多张
                            string[] strlogistic = logisticmode.Split(',');
                            if (strlogistic.Length > 0)
                            {
                                foreach (string logistic in strlogistic)
                                {
                                    //先查询是否有订单数
                                    string strlogicsearch = strsearch + " and [LogisticMode]='" + logistic + "'";
                                    //订单数
                                    object obj = base.NSession.CreateSQLQuery("select count(Id) from Orders " + strlogicsearch).UniqueResult();
                                    if (Convert.ToInt32(obj) > 0)//当前发货方式有订单数
                                    {
                                        PickingListType pick = new PickingListType();

                                        pick.PickingNo = Convert.ToInt32(Utilities.GetOrderNo(NSession));
                                        pick.CreateTime = DateTime.Now;
                                        pick.BeginWorkTime = DateTime.Now;
                                        pick.LogisticMode = logistic;
                                        pick.WareHouse = warehouse;
                                        pick.PickingType = data[i].ToString();
                                        pick.LogisticMode = logistic;
                                        pick.CreateBy = GetCurrentAccount().Realname;
                                        pick.State = "未打印";
                                        pick.OrderCount = Convert.ToInt32(obj);
                                        //SKU品种数
                                        object obj2 = base.NSession.CreateSQLQuery("select count(distinct(sku)) from OrderProducts where OrderNo in (select OrderNo from Orders " + strlogicsearch + ")  ").UniqueResult();
                                        //货品总数
                                        object obj3 = base.NSession.CreateSQLQuery("select sum(Qty) from OrderProducts where OrderNo in (select OrderNo from Orders " + strlogicsearch + ")  ").UniqueResult();
                                        pick.SkuCategory = Convert.ToInt32(obj2);
                                        pick.SKUcount = Convert.ToInt32(obj3);
                                        NSession.Save(pick);
                                        NSession.Flush();
                                        pickidlist.Add(pick.PickingNo);
                                        //更新订单表
                                        IList<OrderType> orderlist = NSession.CreateSQLQuery("select * from Orders" + strlogicsearch).AddEntity(typeof(OrderType)).List<OrderType>().ToList();
                                        foreach (OrderType order in orderlist)
                                        {
                                            order.PickID = pick.PickingNo;
                                            NSession.Update(order);
                                            NSession.Flush();
                                        }

                                    }
                                    strlogicsearch = strsearch;
                                }
                                strsearch = "  where Status='已处理' and Enabled=1 and PickId=0 and  IsOutOfStock=0 and IsAudit!=0    and IsError=0  and  ";


                            }
                        }


                    }
                    else
                    {
                        //如果拣货单发货方式没有选择，那么根据多种发货方式生成多张
                        string[] strlogistic = logisticmode.Split(',');
                        if (strlogistic.Length > 0)
                        {
                            foreach (string logistic in strlogistic)
                            {
                                //先查询是否有订单数
                                strsearch += Utilities.Resolve(search, true);
                                string strlogicsearch = strsearch + " and [LogisticMode]='" + logistic + "'";
                                strlogicsearch = strlogicsearch.Replace("OrderProductType", "OrderProducts");
                                //订单数
                                object obj = base.NSession.CreateSQLQuery("select count(Id) from Orders " + strlogicsearch).UniqueResult();
                                if (Convert.ToInt32(obj) > 0)//当前发货方式有订单数
                                {
                                    PickingListType pick = new PickingListType();

                                    pick.PickingNo = Convert.ToInt32(Utilities.GetOrderNo(NSession));
                                    pick.CreateTime = DateTime.Now;
                                    pick.BeginWorkTime = DateTime.Now;
                                    pick.LogisticMode = logistic;
                                    pick.WareHouse = warehouse;
                                    pick.PickingType = pickingtype;
                                    pick.CreateBy = GetCurrentAccount().Realname;
                                    pick.State = "未打印";
                                    pick.OrderCount = Convert.ToInt32(obj);
                                    //SKU品种数
                                    object obj2 = base.NSession.CreateSQLQuery("select count(distinct(sku)) from OrderProducts where OrderNo in (select OrderNo from Orders " + strlogicsearch + ")").UniqueResult();
                                    //货品总数
                                    object obj3 = base.NSession.CreateSQLQuery("select sum(Qty) from OrderProducts where OrderNo in (select OrderNo from Orders " + strlogicsearch + ") ").UniqueResult();
                                    pick.SkuCategory = Convert.ToInt32(obj2);
                                    pick.SKUcount = Convert.ToInt32(obj3);
                                    NSession.Save(pick);
                                    NSession.Flush();
                                    pickidlist.Add(pick.PickingNo);
                                    //更新订单表
                                    IList<OrderType> orderlist = NSession.CreateSQLQuery("select * from Orders" + strlogicsearch).AddEntity(typeof(OrderType)).List<OrderType>().ToList();
                                    foreach (OrderType order in orderlist)
                                    {
                                        order.PickID = pick.PickingNo;
                                        NSession.Update(order);
                                        NSession.Flush();
                                    }

                                }
                                strlogicsearch = "";
                                strsearch = " where Status='已处理' and Enabled=1 and PickId=0 and IsOutOfStock=0 and IsAudit!=0 and IsError=0  and ";

                            }



                        }
                    }

                    tx.Commit();
                    DataSet dataSet = new DataSet();
                    if (pickidlist.Count > 0)
                    {
                        string format = "select top 800 O.LogisticMode as '物流商',pick.PickingNo as '拣货单单号',pick.CreateBy as '建单人',pick.CreateTime as '建单时间',pick.OrderCount as '包裹数量',pick.SkuCategory as '货品种类数',pick.WareHouse as '仓库',pick.SKUcount as '货品总数', O.LogisticMode as '邮寄方式',OP.SKU as '物品SKU',OP.Qty as '物品Qty',OP.Standard  as '物品规格',P.ProductName as '物品中文标题','' as '打单人', (select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '货位',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',PickingType as '类型' from Orders O left join OrderProducts OP on O.Id=OP.OId left join Products P on OP.SKU=P.SKU inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join pickinglist pick on  O.PickId=pick.PickingNo where O.IsOutOfStock=0 and  O.IsError=0 and O.IsAudit!=0 and o.PickId in (" + listToString(pickidlist) + @")  order by PickId";
                        IDbCommand command = base.NSession.Connection.CreateCommand();
                        command.CommandText = format;
                        //command.Transaction = NSession.BeginTransaction();
                        SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);

                        adapter.Fill(dataSet);
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            row["打单人"] = GetCurrentAccount().Realname;
                        }

                        dataSet.Tables[0].DefaultView.Sort = " 拣货单单号 Asc";
                        string xml = dataSet.GetXml();
                        PrintDataType type = new PrintDataType
                        {
                            Content = xml,
                            CreateOn = DateTime.Now
                        };
                        base.NSession.Save(type);
                        base.NSession.Flush();
                        return base.Json(new { IsSuccess = true, Result = type.Id });
                    }
                    else
                    {

                        return base.Json(new { IsSuccess = true, Result = "生成失败" });
                    }




                }


                catch (Exception ex)
                {

                    tx.Rollback();
                    return base.Json(new { IsSuccess = true, Result = "生成失败" });

                }
            }
        }
        public static String listToString(IList list)
        {
            StringBuilder sb = new StringBuilder();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i < list.Count - 1)
                    {
                        sb.Append(list[i] + ",");
                    }
                    else
                    {
                        sb.Append(list[i]);
                    }
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 打印页面打印拣货单
        /// </summary>
        /// <param name="pickId"></param>
        /// <returns></returns>
        public ActionResult PrintPickList(string Id,string strtype)
        {
            string format = "select O.LogisticMode as '物流商',pick.PickingNo as '拣货单单号',pick.CreateBy as '建单人',pick.CreateTime as '建单时间',pick.OrderCount as '包裹数量',pick.SkuCategory as '货品种类数',pick.WareHouse as '仓库',pick.SKUcount as '货品总数', O.LogisticMode as '邮寄方式',OP.SKU as '物品SKU',sum(OP.Qty) as '物品Qty',OP.Standard  as '物品规格',P.ProductName as '物品中文标题','' as '打单人', (select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '货位',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',PickingType as '类型' from Orders O left join OrderProducts OP on O.Id=OP.OId left join Products P on OP.SKU=P.SKU inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join pickinglist pick on  O.PickId=pick.PickingNo where o.PickId in (" + Id + @") group by O.LogisticMode,PickingNo,pick.CreateBy,CreateTime,pick.OrderCount,
pick.SkuCategory,pick.WareHouse,pick.SKUcount,O.LogisticMode,OP.SKU,OP.Standard,P.ProductName,P.SKU,p.Location,P.Category,pick.PickingType
  Order by 货位";
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = format;
            //command.Transaction = NSession.BeginTransaction();
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);

            adapter.Fill(dataSet);
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                row["打单人"] = GetCurrentAccount().Realname;
            }

            dataSet.Tables[0].DefaultView.Sort = " 拣货单单号 Asc";
            string xml = dataSet.GetXml();
            PrintDataType type = new PrintDataType
            {
                Content = xml,
                CreateOn = DateTime.Now
            };
            base.NSession.Save(type);
            base.NSession.Flush();
            if (strtype == "print")
            {
                List<PickingListType> pick = NSession.CreateQuery(" from PickingListType where PickingNo='" + Id + "'").List<PickingListType>().ToList();
                if (pick.Count > 0)
                {
                    pick[0].State = PickingListStateEnum.等待包装.ToString();
                    base.NSession.Update(pick[0]);
                    base.NSession.Flush();
                }
                List<OrderType> orders = NSession.CreateQuery(" from OrderType where PickId='" + Id + "'").List<OrderType>().ToList();
                if (orders.Count > 0)
                {
                    foreach (OrderType order in orders)
                    {
                        order.IsPrint = 1;
                        NSession.Update(order);
                        NSession.Flush();
                    }
                }
            }

            return base.Json(new { IsSuccess = true, Result = type.Id });
        }
        public JsonResult QList(int page, int rows, string sort, string order, string search, string logistics)
        {
            if (!string.IsNullOrEmpty(search))
            {
                search = " where " + Utilities.Resolve(search, true);
                if (logistics != "" && logistics != null)
                {
                    search += " and P.LogisticMode in(select LogisticsName from LogisticsModeType where ParentID =(select Id from LogisticsType L where L.LogisticsName='" + logistics + "'))";
                }

            }
            else if (string.IsNullOrEmpty(search) && logistics != "" && logistics != null)
            {
                search = " where " + "P.LogisticMode in(select LogisticsName from LogisticsModeType where ParentID =(select Id from LogisticsType L where L.LogisticsName='" + logistics + "'))";
            }
            string orderby = "  order by CreateTime desc";
            IList<PickingListType> list =
            base.NSession.CreateQuery("from PickingListType  P" + search + orderby).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<PickingListType>();
            object count = NSession.CreateQuery("select count(Id) from PickingListType  P" + search).UniqueResult();

            return base.Json(new { total = count, rows = list });
        }
        public JsonResult DeletePrintList(int id)
        {
            try
            {
                PickingListType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
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
        public PickingListType GetById(int Id)
        {
            PickingListType obj = NSession.Get<PickingListType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }
        public ActionResult Show(int Id)
        {
            PickingListType p = NSession.Get<PickingListType>(Id);
            return View(p);
        }
   
        public JsonResult TagState(int Id)
        {
            PickingListType pick = GetById<PickingListType>(Convert.ToInt32(Id));
            pick.State = PickingListStateEnum.等待包装.ToString();
            base.NSession.Save(pick);
            base.NSession.Flush();
            return base.Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 标记面单已打印
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult TagPrint(string Id)
        {
            List<OrderPackageType> orderpack = NSession.CreateQuery(" from OrderPackageType where OrderNo in('" + Id.Replace(",", "','") + "')").List<OrderPackageType>().ToList();
            if (orderpack.Count > 0)
            {
                orderpack[0].IsPrint = 1;
                base.NSession.Save(orderpack[0]);
                base.NSession.Flush();
                return base.Json(new { IsSuccess = true });
            }
            return base.Json(new { IsSuccess = false });

        }
        /// <summary>
        /// 获取打印状态
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPickingState()
        {
            List<object> obj = new List<object>();
            foreach (string str in Enum.GetNames(typeof(PickingListStateEnum)))
            {
                obj.Add(new { id = str, text = str });
            }
            return base.Json(obj);
        }
        /// <summary>
        /// 问题包裹清单
        /// </summary>
        /// <param name="PickinglistNo"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult QuestionList(string Id, int page, int rows)
        {
            List<OrderType> list = NSession.CreateSQLQuery("select * from Orders where OrderNo not in (select OrderNo from OrderPackage where IsPrint>0) and PickId='" + Id + "' ").AddEntity(typeof(OrderType)).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<OrderType>().ToList();
            object count = NSession.CreateSQLQuery("select count(0) from Orders where OrderNo not in (select OrderNo from OrderPackage where IsPrint>0) and PickId='" + Id + "' ").UniqueResult();
            return base.Json(new { total = count, rows = list });
        }
        public ActionResult QuestionPackage(string id)
        {
            List<OrderType> list = NSession.CreateSQLQuery(" select * from Orders,PickingList where OrderNo not in (select OrderNo from OrderPackage where IsPrint>0) and [State]='已包装' and PickId='" + id + "' ").AddEntity(typeof(OrderType)).List<OrderType>().ToList();
            // byId.AddressInfo = base.NSession.Get<OrderAddressType>(byId.AddressId);
            if (list.Count > 0)
            {
               
                //base.ViewData["UserRole"] = GetCurrentAccount().RoleId;
                return base.View(list[0]);

            }
            base.ViewData["PickinglistNo"] = id;
            return base.View(); ;
        }
        /// <summary>
        /// 根据扫描的SKU信息获取订单信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult GetSKUByCode(string code, string PickingNo)
        {
            //首先判断该SKU是否属于当前拣货单而且不包含已打印的订单
            int i = Convert.ToInt32(NSession.CreateSQLQuery("select count(0) from OrderProducts where OId in (select id from orders where pickid='" + PickingNo + "') and sku=(select sku from SKUCode where Code=" + code + ") and OId not in (select id from orders where orderno in (select OrderNo from OrderPackage where IsPrint>0 and PickingNo='" + PickingNo + "'))").UniqueResult());
            if (i > 0)
            {

                IList<SKUCodeType> skulist =
                                NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", code).SetMaxResults(1).List<SKUCodeType>();
                if (skulist.Count > 0)
                {
                    SKUCodeType sku = skulist[0];
                    if (sku.IsOut != 0 && sku.IsScan == 0)
                    {
                        return Json(new { IsSuccess = false, Result = "商品已出库或已配货！系统无法操作！" });

                    }
                }
                //根据输入的商品条码去寻找属于当前拣货单对应的订单表的OrderNo
                List<OrderType> list =
                     NSession.CreateSQLQuery("select * from Orders where id in (select oid from OrderProducts where sku=(select sku from SKUCode where Code=:p)) and PickId=:q  and orderno not in (select OrderNo from OrderPackage where IsPrint>0 and PickingNo=:q)").AddEntity(typeof(OrderType)).SetString("p", code).SetString("q", PickingNo).List<OrderType>().ToList();
                if (list.Count > 0)
                {
                    OrderType type = list[0];
                    string str2;
                    if (type.Status != OrderStatusEnum.已处理.ToString() && type.Status != OrderStatusEnum.待包装.ToString())
                    {
                        return base.Json(new { IsSuccess = false, Result = " 系统无法进行包装扫描！ 当前状态为：" + type.Status + "" });


                    }
                    if (type.IsAudit == 0)
                    {
                        string str = "订单:" + type.OrderNo + ", 需要审核";
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    if ((type.IsError == 0) && string.IsNullOrEmpty(type.CutOffMemo))
                    {
                        int codeLength = -1;
                        IList<LogisticsType> list2 = base.NSession.CreateQuery("from LogisticsType where Id in(select ParentID from LogisticsModeType where LogisticsCode='" + type.LogisticMode + "')").List<LogisticsType>();
                        if (list2.Count > 0)
                        {
                            codeLength = list2[0].CodeLength;
                        }
                        type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();
                      
                        return base.Json(new { IsSuccess = true, Result = type, Code = codeLength });

                    }
                    else
                    {
                        str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                        return base.Json(new { IsSuccess = false, Result = str2 });
                    }

                }


                else
                {
                    return base.Json(new { IsSuccess = false, Result = "当前SKU找不到对应订单" });

                }





            }
            else
            {
                return base.Json(new { IsSuccess = false, Result = "当前SKU不属于当前拣货单或者已经完成拣货" });
            }

        }
      
        /// <summary>
        /// 拣货单扫描跟踪码发货扫描
        /// </summary>
        /// <param name="TrackCode"></param>
        /// <param name="Weight"></param>
        /// <param name="WarehouseId"></param>
        /// <param name="LogisticMode"></param>
        /// <returns></returns>
        public JsonResult Scan(string TrackCode, int Weight, int WarehouseId, string LogisticMode,string PickingListNo)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode ='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];

                if (type.Status == OrderStatusEnum.待发货.ToString() || type.Status==OrderStatusEnum.已处理.ToString())
                {
                    string ttt = type.TrackCode;
                    //type.TrackCode = t;
                    // 判断是否手动添加跟踪码,当手动添加跟踪码时跟踪码记录
                    if (type.TrackCode == "已用完")
                    {
                        type.TrackCode = TrackCode;
                    }
                    type.Weight = Weight;
                    if (LogisticMode != "")
                    {
                        type.LogisticMode = LogisticMode;
                    }

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.已发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    //将订单标记已打印
                    base.NSession.CreateQuery("update OrderPackageType set IsPrint=1 where OrderNo=:p and PickingNo=:q").SetString("p", type.OrderNo).SetString("q", PickingListNo).ExecuteUpdate();                   
                    // 订单商品出库
                    IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                    // 根据skucode获取商品所在仓库id【skucode无仓库记录信息无法获取】
                    foreach (OrderProductType type2 in list2)
                    {
                        Utilities.StockOut(WarehouseId, type2.SKU, type2.Qty, "扫描出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                    }
                    //更新商品重量
                    if (list2.Count == 1 && list2[0].Qty == 1)
                    {
                        List<ProductType> product =
                            NSession.CreateQuery("from ProductType where SKU='" + list2[0].SKU + "'").List
                                <ProductType>().ToList();
                        if (product.Count > 0)
                        {
                            bool iscon = false;
                            if (product[0].Weight != 0)
                            {
                                if (product[0].Weight <= 200)
                                {
                                    if ((product[0].Weight * 1.1) < Convert.ToDouble(Weight) ||
                                        (product[0].Weight * 0.9) > Convert.ToDouble(Weight))
                                        iscon = true;
                                }
                                else
                                {
                                    if ((product[0].Weight * 1.05) < Convert.ToDouble(Weight) ||
                                        (product[0].Weight * 0.95) > Convert.ToDouble(Weight))
                                        iscon = true;
                                }
                                if (iscon)
                                {
                                    product[0].Weight = Convert.ToInt32(Weight);
                                    NSession.Update(product[0]);
                                    NSession.Flush();
                                    LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + Weight, CurrentUser, NSession);

                                }
                            }

                        }
                    }
                    // 标记出库
                    base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + type.TrackCode + " 原单号:" + ttt, base.CurrentUser, base.NSession);

                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                    string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", LogisticMode, "  重量：", Weight });
                    try
                    {
                        // 测试期间暂不上传跟踪码
                        //new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad)) { IsBackground = true }.Start(type);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        // 计算订单财务数据
                        OrderHelper.ReckonFinance(type, base.NSession);
                    }
                    //计算已打印订单数量和拣货单包含的订单数量，如果数量一致就结束包装
                    int objPrintCount = Convert.ToInt32(NSession.CreateSQLQuery("select count(0) from OrderPackage where IsPrint=1 and      PickingNo=:p").SetString("p", PickingListNo).UniqueResult());
                    int objTotalCount = Convert.ToInt32(NSession.CreateSQLQuery("select count(0) from Orders where pickid=:p").SetString("p", PickingListNo).UniqueResult());
                    if (objTotalCount == objPrintCount)
                    {
                        base.NSession.CreateQuery("update PickingListType set State='" +PickingListStateEnum.已包装+ "' where PickingNo ='" +PickingListNo + "'").ExecuteUpdate();
                        return base.Json(new { IsSuccess = true, Result = "包装扫描结束" });
                    }
                    return base.Json(new { IsSuccess = true, Result = "订单：" + type.OrderNo + "发货扫描结束，等待扫描跟踪码...", OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 系统无法操作！当前状态为：" + type.Status + "！" });
            }
            return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });
        }
        /// <summary>
        /// SKU状态判断
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult GetSKUByCode3(string code)
        {
            IList<SKUCodeType> list =
                 NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", code).SetMaxResults(1).List<SKUCodeType>();
            if (list.Count > 0)
            {
                SKUCodeType sku = list[0];
                //if (sku.IsOut == 0 && sku.IsScan == 0)
                if (sku.IsOut != 0 && sku.IsScan == 0)
                {
                    
                    return Json(new { IsSuccess = false, Result = "商品已出库或已配货！系统无法操作！" });
                }
                else
                {
                    return Json(new { IsSuccess = true, Result = sku.SKU.Trim() });
                }
            }
            return Json(new { IsSuccess = false, Result = "没有找到该商品！" });

        }


        public JsonResult SetPrintData(string ids)
        {



            // 1、12小时内打印仓库入库人名 2、12小时外打印库位
            string format = "select [dbo].[F_GetSKU2](O.OrderNo) as '物品SKU2',[dbo].[F_GetProducts](O.OrderNo) as '产品名','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=40 then (case when Amount<=5 then Amount else 5 end) else 8 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo ";

            format = string.Format(format, ids.Replace(",", "','"));
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = format;
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            List<string> list2 = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                object obj = NSession.CreateSQLQuery(@"select top 1 AreaName from [LogisticsArea] where LId = (select top 1 ParentID from LogisticsMode where LogisticsCode='" + row["订单发货方式"] + "')  and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode  and CountryCode in (select ID from Country where ECountry=N'" + row["收件人国家"].ToString().Replace("'", "''") + "') )").UniqueResult();
                row["分区"] = obj;

                if (!list2.Contains(row["订单编号"].ToString()))
                {
                    LoggerUtil.GetOrderRecord(Convert.ToInt32(row["Id"]), row["订单编号"].ToString(), "订单打印", CurrentUser.Realname + "订单打印！", CurrentUser, NSession);
                    list2.Add(row["订单编号"].ToString());
                    base.NSession.CreateQuery("update OrderType set IsPrint=IsPrint+1 where  IsAudit=1 and  OrderNo IN('" + ids.Replace(",", "','") + "') ").ExecuteUpdate();
                }
                string country = dataSet.Tables[0].Rows[0]["收件人国家"].ToString();
                string postCode = dataSet.Tables[0].Rows[0]["收件人邮编"].ToString();
                HomeController home = new HomeController();
                string Fenjianma = home.GetFjm(country, postCode);
                row["分拣码"] = Fenjianma;
            }
            dataSet.Tables[0].DefaultView.Sort = " 订单编号 Asc";
            string xml = dataSet.GetXml();
            PrintDataType type = new PrintDataType
            {
                Content = xml,
                CreateOn = DateTime.Now
            };
            base.NSession.Save(type);
            base.NSession.Flush();
            object objtempid = NSession.CreateSQLQuery("select distinct p.id from  PrintTemplate p ,orders o,LogisticsMode l where p.Code=l.LogisticsCode and o.LogisticMode=l.LogisticsCode and o.OrderNo=:p").SetString("p", ids).UniqueResult();
            string tempid = string.Empty;
            if (objtempid != null)
            {
                tempid = objtempid.ToString();
            }
            return base.Json(new { IsSuccess = true, Result = type.Id, TmpID = tempid });

        }

    }
}
