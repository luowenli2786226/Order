using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Web;
using DDX.OrderManagementSystem.App.Common;
using DDX.OrderManagementSystem.App.ExLogisticMode;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using FBAInventoryServiceMWS;
using FBAInventoryServiceMWS.Model;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.Model;
using NHibernate;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using OrderType = DDX.OrderManagementSystem.Domain.OrderType;
using ProductType = DDX.OrderManagementSystem.Domain.ProductType;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
using System.Reflection;
using DDX.OrderManagementSystem.App.Controllers;
using MWSClientCsRuntime;
using FBAInboundServiceMWS;
using FBAInboundServiceMWS.Model;
using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;

namespace DDX.OrderManagementSystem.App
{
    public class OrderHelper
    {
        public static DataTable GetDataTable(string fileName)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";" + "Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
            DataSet ds = new DataSet();
            OleDbDataAdapter oada = new OleDbDataAdapter("select * from [Sheet1$]", strConn);
            try
            {
                oada.Fill(ds);
            }
            catch
            {
                // 报错外部表不是预期的格式错误
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";" + "Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
                oada = new OleDbDataAdapter("select * from [Sheet1$]", strConn);
                oada.Fill(ds);
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// 用于获得token（统一获取类）
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string GetAliToken(AccountType account, ISession NSession)
        {
            account.ApiTokenInfo = AliUtil.RefreshToken(account);

            NSession.Update(account);
            NSession.Flush();
            return account.ApiTokenInfo;
        }

        /// <summary>
        /// 用于获得token（统一获取类）
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>

        #region 订单数据导入


        public static List<ResultInfo> ImportAmazonFBAFees(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                if (dr.Table.Columns.Count == 2)
                {
                    if (!dic.ContainsKey(Utilities.ToString(dr[0])))
                    {
                        // dic.Add();
                    }
                }

            }
            return results;
        }

        public static List<ResultInfo> ImportByAmount(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                if (dr.Table.Columns.Count == 2)
                    NSession.CreateQuery("update OrderType set Amount=:p1 where OrderExNo=:p2 and Enabled=1 and Amount<>0 ").SetDouble("p1", Utilities.ToDouble(dr[1].ToString())).SetDouble("p3", Utilities.ToDouble(dr[2].ToString())).SetString("p2", dr[0].ToString()).ExecuteUpdate();
            }
            return results;

        }

        public static List<ResultInfo> ImportBySMT(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                string OrderExNo = dr["订单号"].ToString().Trim();
                string o = dr["订单状态"].ToString();
                if (o != "等待您发货")
                {
                    results.Add(GetResult(OrderExNo, "订单已经发货", "导入失败"));
                    continue;
                }

                bool isExist = IsExist(OrderExNo, NSession);
                if (!isExist)
                {
                    OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now };
                    try
                    {
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = "USD";
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(dr["订单金额"].ToString());
                        order.BuyerMemo = dr["订单备注"].ToString();
                        order.Country = dr["收货国家"].ToString();
                        order.BuyerName = dr["买家名称"].ToString();
                        order.BuyerEmail = dr["买家邮箱"].ToString();
                        order.TId = "";
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(dr["付款时间"]);
                        order.Platform = PlatformEnum.Aliexpress.ToString();
                        //舍弃原来的客户表
                        //下面地址
                        order.AddressId = CreateAddress(dr["收货人名称"].ToString(), dr["地址"].ToString(), dr["城市"].ToString(), dr["州/省"].ToString(), dr["收货国家"].ToString(), dr["收货国家"].ToString(), dr["联系电话"].ToString(), dr["手机"].ToString(), dr["买家邮箱"].ToString(), dr["邮编"].ToString(), 0, NSession);


                        NSession.Save(order);
                        NSession.Flush();

                        //CreateOrderPruduct(dr["平台SKU"].ToString(), dr["SKU"].ToString(), Utilities.ToInt(dr["数量"].ToString()), dr["产品品名"].ToString(), dr["备注"].ToString(), Convert.ToDouble(dr["价格"].ToString()), dr["产品链接"].ToString(), order.Id, order.OrderNo, NSession);
                        //
                        //添加产品
                        //
                        string info = dr["产品信息_（双击单元格展开所有产品信息！）"].ToString();
                        string[] cels = info.Split(new char[] { '【' }, StringSplitOptions.RemoveEmptyEntries);
                        if (cels.Length == 0)
                        {
                            results.Add(GetResult(OrderExNo, "没有产品信息", "导入失败"));
                            continue;//物品信息出错
                        }
                        for (int i = 0; i < cels.Length; i++)
                        {
                            string Str = cels[i];
                            System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(@"】(?<title>.*)\n", System.Text.RegularExpressions.RegexOptions.None);
                            System.Text.RegularExpressions.Regex r4 = new System.Text.RegularExpressions.Regex(@"\(产品属性:(?<ppp>.*)\n", System.Text.RegularExpressions.RegexOptions.None);
                            System.Text.RegularExpressions.Regex r5 = new System.Text.RegularExpressions.Regex(@"\(产品数量:(?<quantity>\d+)", System.Text.RegularExpressions.RegexOptions.None);
                            System.Text.RegularExpressions.Regex r3 = new System.Text.RegularExpressions.Regex(@"\(商家编码:(?<sku>.*)\)", System.Text.RegularExpressions.RegexOptions.None);
                            //System.Text.RegularExpressions.Regex r6 = new System.Text.RegularExpressions.Regex(@"\(物流等级&买家选择物流:(?<wuliu>.+)\)", System.Text.RegularExpressions.RegexOptions.None);
                            System.Text.RegularExpressions.Match mc2 = r2.Match(Str);
                            System.Text.RegularExpressions.Match mc3 = r3.Match(Str);
                            System.Text.RegularExpressions.Match mc4 = r4.Match(Str);
                            System.Text.RegularExpressions.Match mc5 = r5.Match(Str);

                            order.LogisticMode = dr["买家选择物流"].ToString();
                            if (order.LogisticMode.IndexOf("\n") != -1)
                            {
                                order.LogisticMode = order.LogisticMode.Substring(0, order.LogisticMode.IndexOf("\n"));
                            }
                            NSession.Update(order);
                            NSession.Flush();
                            CreateOrderPruduct(mc3.Groups["sku"].Value, Utilities.ToInt(mc5.Groups["quantity"].Value.Trim(')').Trim()), mc2.Groups["title"].Value, mc4.Groups["ppp"].Value.Replace("(产品属性: ", "").Replace(")", ""), 0, "", order.Id, order.OrderNo, NSession);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    results.Add(GetResult(OrderExNo, "", "导入成功"));
                    LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                }
                else
                {
                    results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                }
            }
            return results;

        }
        public static List<ResultInfo> ImportByLazada(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                string OrderExNo = dr["订单号"].ToString().Trim();
                bool isExist = IsExist(OrderExNo, NSession);
                if (!isExist)
                {
                    OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now, Freight = 0.01, IsFreight = 1 };
                    try
                    {
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = dr["货币类型"].ToString();
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(dr["订单金额"].ToString());
                        order.BuyerMemo = dr["订单备注"].ToString();
                        order.Country = dr["收货国家"].ToString();
                        order.BuyerName = dr["买家名称"].ToString();
                        order.BuyerEmail = dr["买家邮箱"].ToString();
                        order.TId = dr["流水号"].ToString();
                        order.LogisticMode = dr["发货方式"].ToString();
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(dr["付款时间"]);
                        order.Platform = PlatformEnum.Lazada.ToString();
                        //舍弃原来的客户表
                        //下面地址
                        order.AddressId = CreateAddress(dr["收货人名称"].ToString(), dr["地址"].ToString(), dr["城市"].ToString(), dr["州/省"].ToString(), dr["收货国家"].ToString(), dr["收货国家"].ToString(), dr["联系电话"].ToString(), dr["手机"].ToString(), dr["买家邮箱"].ToString(), dr["邮编"].ToString(), 0, NSession);


                        NSession.Save(order);
                        NSession.Flush();
                        CreateOrderPruduct(dr["平台SKU"].ToString(), dr["SKU"].ToString(), Utilities.ToInt(dr["数量"].ToString()), dr["产品品名"].ToString(), dr["备注"].ToString(), Convert.ToDouble(dr["价格"].ToString()), dr["产品链接"].ToString(), order.Id, order.OrderNo, NSession);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    results.Add(GetResult(OrderExNo, "", "导入成功"));
                    LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                }
                else
                {
                    results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                }
            }
            return results;

        }

        public static List<ResultInfo> ImportByAmazon(AccountType account, string fileName, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            CsvReader csv = new CsvReader(fileName, Encoding.Default);

            List<Dictionary<string, string>> lsitss = csv.ReadAllData();
            Dictionary<string, int> listOrder = new Dictionary<string, int>();
            foreach (Dictionary<string, string> item in lsitss)
            {
                if (item.Count < 10)//判断列数
                    continue;
                string OrderExNo = item["order-id"];

                if (listOrder.ContainsKey(OrderExNo))
                {
                    CreateOrderPruduct(item["sku"], Utilities.ToInt(item["quantity-purchased"]), item["sku"], "", 0, "", listOrder[OrderExNo], OrderExNo, NSession);
                    continue;
                }
                bool isExist = IsExist(OrderExNo, NSession);
                if (!isExist)
                {
                    OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now };
                    order.OrderNo = Utilities.GetOrderNo(NSession);
                    try
                    {
                        order.CurrencyCode = item["currency"].ToUpper().Trim();
                    }
                    catch (Exception)
                    {
                        order.CurrencyCode = "USD";
                    }
                    order.OrderExNo = account.AccountName + "_" + item["order-id"];
                    //order.Amount =Utilities.ToDouble(dr["订单金额"]);
                    //order.BuyerMemo = dr["订单备注"].ToString();
                    order.Country = item["ship-country"];
                    order.BuyerName = item["buyer-name"];
                    order.BuyerEmail = item["buyer-email"];
                    order.TId = "";
                    order.Account = account.AccountName;
                    order.GenerateOn = Convert.ToDateTime(item["payments-date"]);
                    order.Platform = PlatformEnum.Amazon.ToString();
                    order.AddressId = CreateAddress(item["recipient-name"], item["ship-address-1"] + item["ship-address-2"] + item["ship-address-3"], item["ship-city"], item["ship-state"], item["ship-country"], item["ship-country"], item["buyer-phone-number"], item["buyer-phone-number"], item["buyer-email"], item["ship-postal-code"], 0, NSession);
                    NSession.Save(order);
                    NSession.Flush();
                    CreateOrderPruduct(item["sku"], Utilities.ToInt(item["quantity-purchased"]), item["sku"], "", 0, "", order.Id, order.OrderNo, NSession);
                    results.Add(GetResult(OrderExNo, "", "导入成功"));
                    listOrder.Add(OrderExNo, order.Id);
                    LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                }
                else
                {
                    results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                }
            }
            return results;
        }

        public static List<ResultInfo> ImportByGmarket(AccountType account, string fileName, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            CsvReader csv = new CsvReader(fileName, Encoding.Default);

            List<Dictionary<string, string>> lsitss = csv.ReadAllData();
            Dictionary<string, int> listOrder = new Dictionary<string, int>();
            foreach (Dictionary<string, string> item in lsitss)
            {
                try
                {
                    if (item.Count < 10)//判断列数
                        continue;
                    string OrderExNo = item["Cart no."];
                    double price = Convert.ToDouble(item["Settle Price"].Replace(",", ""));
                    if (listOrder.ContainsKey(OrderExNo))
                    {
                        CreateOrderPruduct(item["Item code"], item["Option Code"], Utilities.ToInt(item["Qty."]), item["Item"], item["Options"], 0, "", listOrder[OrderExNo], OrderExNo, NSession);
                        NSession.CreateSQLQuery("update orders set Amount=Amount+" + price + " where Id=" +
                                                listOrder[OrderExNo]).UniqueResult();
                        continue;
                    }
                    bool isExist = IsExist(OrderExNo, NSession, account.AccountName);
                    if (!isExist)
                    {
                        OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now };
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = item["Currency"];
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(item["Settle Price"]);
                        order.BuyerMemo = item["Memo to Seller"];
                        order.Country = item["Nation"];
                        order.BuyerName = item["Customer"];
                        order.BuyerEmail = "";
                        order.TId = item["Order no."];
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(item["Payment Complete"]);
                        order.Platform = PlatformEnum.Gmarket.ToString();
                        order.BuyerMemo = item["Memo to Seller"] + item["Options"];
                        order.AddressId = CreateAddress(item["Recipient"], item["Address"], "", "", item["Nation"], item["Nation"], item["Recipient Phone number"], item["Recipient mobile Phone number"], "", item["Postal code"], 0, NSession);
                        NSession.Save(order);
                        NSession.Flush();
                        CreateOrderPruduct(item["Item code"], item["Option Code"], Utilities.ToInt(item["Qty."]), item["Item"], item["Options"], 0, "", order.Id, order.OrderNo, NSession);
                        results.Add(GetResult(OrderExNo, "", "导入成功"));
                        listOrder.Add(OrderExNo, order.Id);
                        LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                    }
                    else
                    {
                        results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
            return results;
        }


        public static List<ResultInfo> ImportByB2C(AccountType account, string fileName, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow item in OrderHelper.GetDataTable(fileName).Rows)
            {
                try
                {
                    string OrderExNo = item["订单编号"].ToString();
                    bool isExist = IsExist(OrderExNo, NSession);
                    if (!isExist)
                    {
                        OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now };
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(item["金额"].ToString());
                        order.Country = item["国家"].ToString();
                        order.BuyerName = item["用户名"].ToString();
                        order.CurrencyCode = "USD";
                        order.Account = account.AccountName;
                        order.GenerateOn = DateTime.Now;
                        order.Platform = PlatformEnum.WebSite.ToString();

                        order.AddressId = CreateAddress(item["收件人"].ToString(), item["街道"].ToString(), item["城市"].ToString(), item["省"].ToString(), item["国家"].ToString(), item["国家"].ToString(), item["电话"].ToString(), item["电话"].ToString(), item["邮箱"].ToString(), item["邮编"].ToString(), 0, NSession);

                        NSession.Save(order);
                        NSession.Flush();


                        CreateOrderPruduct(item["商品"].ToString(), item["商品"].ToString(), Utilities.ToInt(item["数量"].ToString()), "", "", 0, item["属性"].ToString(), order.Id, order.OrderNo, NSession);
                        results.Add(GetResult(OrderExNo, "", "导入成功"));
                        LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
            return results;
        }

        public static List<ResultInfo> ImportByWish(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                string OrderExNo = dr["订单号"].ToString().Trim();
                bool isExist = IsExist(OrderExNo, NSession);
                if (!isExist)
                {
                    OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now, Freight = 0.01, IsFreight = 1 };
                    try
                    {
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = dr["货币类型"].ToString();
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(dr["订单金额"].ToString());
                        order.BuyerMemo = dr["订单备注"].ToString();
                        order.Country = dr["收货国家"].ToString();
                        order.BuyerName = dr["买家名称"].ToString();
                        order.BuyerEmail = dr["买家邮箱"].ToString();
                        order.TId = dr["流水号"].ToString();
                        order.LogisticMode = dr["发货方式"].ToString();
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(dr["付款时间"]);
                        order.Platform = PlatformEnum.Wish.ToString();
                        //舍弃原来的客户表
                        //下面地址
                        order.AddressId = CreateAddress(dr["收货人名称"].ToString(), dr["地址"].ToString(), dr["城市"].ToString(), dr["州/省"].ToString(), dr["收货国家"].ToString(), dr["收货国家"].ToString(), dr["联系电话"].ToString(), dr["手机"].ToString(), dr["买家邮箱"].ToString(), dr["邮编"].ToString(), 0, NSession);


                        NSession.Save(order);
                        NSession.Flush();
                        CreateOrderPruduct(dr["商品"].ToString(), dr["商品"].ToString(), Utilities.ToInt(dr["数量"].ToString()), "", "", 0, dr["属性"].ToString(), order.Id, order.OrderNo, NSession);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    results.Add(GetResult(OrderExNo, "", "导入成功"));
                    LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                }
                else
                {
                    results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                }
            }
            return results;

        }
        public static List<ResultInfo> ImportByCdiscount(AccountType account, string fileName, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            foreach (DataRow dr in GetDataTable(fileName).Rows)
            {
                string OrderExNo = dr["订单号"].ToString().Trim();
                bool isExist = IsExist(OrderExNo, NSession);
                if (!isExist)
                {
                    OrderType order = new OrderType { IsMerger = 0, Enabled = 1, IsOutOfStock = 0, IsRepeat = 0, IsSplit = 0, Status = OrderStatusEnum.待处理.ToString(), IsPrint = 0, CreateOn = DateTime.Now, ScanningOn = DateTime.Now, Freight = 0.01, IsFreight = 1 };
                    try
                    {
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = dr["货币类型"].ToString();
                        order.OrderExNo = OrderExNo;
                        order.Amount = Utilities.ToDouble(dr["订单金额"].ToString());
                        order.BuyerMemo = dr["订单备注"].ToString();
                        order.Country = dr["收货国家"].ToString();
                        order.BuyerName = dr["买家名称"].ToString();
                        order.BuyerEmail = dr["买家邮箱"].ToString();
                        order.TId = dr["流水号"].ToString();
                        order.LogisticMode = dr["发货方式"].ToString();
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(dr["付款时间"]);
                        order.Platform = PlatformEnum.Cdiscount.ToString();
                        //舍弃原来的客户表
                        //下面地址
                        order.AddressId = CreateAddress(dr["收货人名称"].ToString(), dr["地址"].ToString(), dr["城市"].ToString(), dr["州/省"].ToString(), dr["收货国家"].ToString(), dr["收货国家"].ToString(), dr["联系电话"].ToString(), dr["手机"].ToString(), dr["买家邮箱"].ToString(), dr["邮编"].ToString(), 0, NSession);


                        NSession.Save(order);
                        NSession.Flush();
                        CreateOrderPruduct(dr["平台SKU"].ToString(), dr["SKU"].ToString(), Utilities.ToInt(dr["数量"].ToString()), dr["产品品名"].ToString(), dr["备注"].ToString(), Convert.ToDouble(dr["价格"].ToString()), dr["产品链接"].ToString(), order.Id, order.OrderNo, NSession);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    results.Add(GetResult(OrderExNo, "", "导入成功"));
                    LoggerUtil.GetOrderRecord(order, "订单导入", "导入成功", NSession);
                }
                else
                {
                    results.Add(GetResult(OrderExNo, "订单已存在", "导入失败"));
                }
            }
            return results;

        }

        #endregion

        #region 订单数据ＡＰＩ同步
        public static List<ResultInfo> APIByB2C(AccountType account, DateTime st, DateTime et, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();

            string s = DownHtml("http://www.gamesalor.com/GetOrdersHandler.ashx?startTime=" + st.ToShortDateString() + "&endTime=" + et.ToShortDateString() + "", System.Text.Encoding.UTF8);
            System.Collections.Generic.List<Order> orders = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<Order>>(s);
            foreach (Order foo in orders)
            {
                try
                {
                    bool isExist = IsExist(foo.GoodsDataWare.ItemNumber, NSession);
                    if (!isExist)
                    {
                        OrderType order = new OrderType
                        {
                            Enabled = 1,
                            IsMerger = 0,
                            IsOutOfStock = 0,
                            IsRepeat = 0,
                            IsSplit = 0,
                            Status = OrderStatusEnum.待处理.ToString(),
                            IsPrint = 0,
                            CreateOn = DateTime.Now,
                            ScanningOn = DateTime.Now
                        };
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = foo.GoodsDataWare.McCurrency;
                        order.OrderExNo = foo.GoodsDataWare.ItemNumber;
                        order.Amount = Utilities.ToDouble(foo.GoodsDataWare.McGross.ToString());
                        order.BuyerMemo = "客户付款账户" + foo.GoodsDataWare.Business + "  " + foo.GoodsDataWare.Memo;
                        order.Country = foo.GoodsDataWare.AddressCountry;
                        order.BuyerName = foo.GoodsDataWare.FirstName + " " + foo.GoodsDataWare.LastName;
                        order.BuyerEmail = foo.GoodsDataWare.PayerEmail;
                        order.TId = foo.GoodsDataWare.TxnId;
                        order.Account = account.AccountName;
                        order.GenerateOn = foo.GoodsDataWare.PaymentDate;
                        order.Platform = PlatformEnum.DH.ToString();
                        order.LogisticMode = foo.GoodsDataWare.EMS;
                        order.AddressId = CreateAddress(foo.GoodsDataWare.AddressName, foo.GoodsDataWare.AddressStreet,
                                                        foo.GoodsDataWare.AddressCity, foo.GoodsDataWare.AddressState,
                                                        foo.GoodsDataWare.AddressCountry,
                                                        foo.GoodsDataWare.AddressCountryCode, foo.GoodsDataWare.ContactPhone,
                                                        foo.GoodsDataWare.ContactPhone, foo.GoodsDataWare.PayerEmail,
                                                        foo.GoodsDataWare.AddressZip, 0, NSession);
                        NSession.Save(order);
                        NSession.Flush();
                        foreach (GoodsDataOrder item in foo.GoodsDataOrderList)
                        {
                            CreateOrderPruduct(item.ItemID, item.Quantity, item.ItemID, "", 0, item.Url, order.Id,
                                               order.OrderNo, NSession);
                        }
                        results.Add(GetResult(order.OrderExNo, "", "导入成功"));
                    }
                    else
                    {

                        results.Add(GetResult(foo.GoodsDataWare.ItemNumber, "该订单已存在", "导入失败"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
            return results;
        }

        public static List<ResultInfo> APIByAmazon(AccountType account, DateTime st, DateTime et, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            if (et > DateTime.Now)
            {
                et = DateTime.Now.AddMinutes(-DateTime.Now.Minute);
            }
            // Developer AWS access key
            string accessKey = account.ApiKey;//"AKIAIFEICDUPSGL36SWA";

            // Developer AWS secret key
            string secretKey = account.ApiSecret;//"jRMN9OpFS5vFAETyTPzvidxwRd32+SemZWM5NAgX";

            // The client application name
            string appName = "bestore";

            // The client application version
            string appVersion = "1.0";

            // The endpoint for region service and version (see developer guide)
            // ex: https://mws.amazonservices.com
            string serviceURL = account.Email;


            MarketplaceWebServiceOrdersConfig config = new MarketplaceWebServiceOrdersConfig();
            config.ServiceURL = serviceURL;
            // Set other client connection configurations here if needed
            // Create the client itself
            MarketplaceWebServiceOrdersClient client = new MarketplaceWebServiceOrdersClient(accessKey, secretKey, appName, appVersion, config);
            string NextToken = "";

            try
            {

                ListOrdersRequest request = new ListOrdersRequest();
                ListOrdersByNextTokenRequest listOrdersByNextTokenRequest = new ListOrdersByNextTokenRequest();
                ListOrdersByNextTokenResponse listOrdersByNextTokenResponse = new ListOrdersByNextTokenResponse();
                request.SellerId = account.ApiToken;//"A2EPQPAS638GSV";
                request.WithCreatedAfter(st);
                request.WithCreatedBefore(et);
                request.MarketplaceId = new List<string>();
                //"ATVPDKIKX0DER" 
                request.WithMarketplaceId(new string[] { account.Phone });
                ListOrdersResponse response = client.ListOrders(request);
                List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                NextToken = response.ListOrdersResult.NextToken;
                List<MarketplaceWebServiceOrders.Model.Order> orders = new List<MarketplaceWebServiceOrders.Model.Order>();
                orders = response.ListOrdersResult.Orders;
                do
                {
                    foreach (MarketplaceWebServiceOrders.Model.Order order in orders)
                    {
                        CreateAmazonOrder(account, NSession, order, client, countryTypes, results, currencies);
                    }
                    if (!string.IsNullOrEmpty(NextToken))
                    {
                        listOrdersByNextTokenRequest.WithNextToken(NextToken);
                        listOrdersByNextTokenRequest.SellerId = account.ApiToken;
                        listOrdersByNextTokenResponse = client.ListOrdersByNextToken(listOrdersByNextTokenRequest);
                        NextToken = listOrdersByNextTokenResponse.ListOrdersByNextTokenResult.NextToken;
                        orders = listOrdersByNextTokenResponse.ListOrdersByNextTokenResult.Orders;
                    }
                    else
                    {
                        break;

                    }


                } while (1 == 1);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return results;
        }

        public static void CreateAmazonOrder(AccountType account, ISession NSession, MarketplaceWebServiceOrders.Model.Order order,
                                             MarketplaceWebServiceOrdersClient client, List<CountryType> countryTypes, List<ResultInfo> results,
                                             List<CurrencyType> currencies)
        {

            //if (order.AmazonOrderId == "113-1745119-0554600")
            //{
            //    int nI = 0;
            //}
            //else
            //{
            //    return;
            //}
            if (order.OrderStatus != "Unshipped" && order.OrderStatus != "Shipped")
            {
                return;
            }
            bool isExist = AmazonIsExist(order.AmazonOrderId.ToString(), NSession, account.AccountName);
            if (!isExist)
            {
                OrderType orderType = new OrderType
                {
                    OrderNo = Utilities.GetOrderNo(NSession),
                    IsMerger = 0,
                    Enabled = 1,
                    IsOutOfStock = 0,
                    IsRepeat = 0,
                    IsSplit = 0,
                    IsFBA = 0,
                    Status = OrderStatusEnum.待处理.ToString(),
                    IsPrint = 0,
                    CreateOn = DateTime.Now,
                    ScanningOn = DateTime.Now,
                    Platform = PlatformEnum.Amazon.ToStr(),
                    Account = account.AccountName
                };
                if (order.FulfillmentChannel == "AFN")
                {
                    orderType.IsFBA = 1;
                    orderType.FBABy = "FBA";
                    orderType.Freight = 0.01;
                    orderType.IsFreight = 1;
                }

                //orderType.OrderExNo = order.AmazonOrderId;
                orderType.OrderExNo = account.AccountName + "_" + order.AmazonOrderId;
                orderType.CurrencyCode = order.OrderTotal.CurrencyCode;
                orderType.Amount = orderType.AmountOld = Utilities.ToDouble(order.OrderTotal.Amount);
                if (orderType.CurrencyCode != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == orderType.CurrencyCode);
                    CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
                    if (currencyType != null || currencyType2 != null)
                    {
                        orderType.AmountOld =
                            Math.Round(
                                orderType.Amount * Convert.ToDouble(currencyType.CurrencyValue) /
                                Convert.ToDouble(currencyType2.CurrencyValue),
                                2);
                    }
                }

                orderType.BuyerName = order.BuyerName;
                orderType.BuyerEmail = order.BuyerEmail;
                orderType.GenerateOn = order.PurchaseDate;

                if (order.ShippingAddress != null)
                {
                    CountryType country =
                        countryTypes.Find(
                            p => p.CountryCode.ToUpper() == order.ShippingAddress.CountryCode);
                    if (country != null)
                    {
                        orderType.Country = country.ECountry;
                    }
                    else
                    {
                        orderType.Country = order.ShippingAddress.CountryCode;
                    }
                    bool flag = false;
                    //将国家由Great Britain改为United Kingdom
                    if (orderType.Country == "Great Britain")
                    {
                        orderType.Country = "United Kingdom";
                        flag = true;
                    }
                    //亚马逊订单德语Deutschland同步自动转换为英文的德国
                    if (orderType.Country == "Deutschland")
                    {
                        orderType.Country = "Germany";
                        flag = true;
                    }
                    orderType.AddressId = CreateAddress(order.ShippingAddress.Name,
                                                        order.ShippingAddress.AddressLine1 + " " +
                                                        order.ShippingAddress.AddressLine2 + " " +
                                                        order.ShippingAddress.AddressLine3 + " " +
                                                        order.ShippingAddress.County + " " +
                                                        order.ShippingAddress.District,
                                                        order.ShippingAddress.City,
                                                        order.ShippingAddress.StateOrRegion, orderType.Country,
                                                        order.ShippingAddress.CountryCode,
                                                        order.ShippingAddress.Phone, order.ShippingAddress.Phone,
                                                        order.BuyerEmail, order.ShippingAddress.PostalCode, 0, flag,
                                                        NSession);
                }
                else
                {
                    orderType.Country = "地址保密";
                    orderType.AddressId = CreateAddress("地址保密",
                                                        "地址保密",
                                                        "地址保密",
                                                        "地址保密", "地址保密",
                                                        "地址保密",
                                                        "地址保密", "地址保密",
                                                        "地址保密", "地址保密", 0,
                                                        NSession);
                }
                if (!string.IsNullOrEmpty(order.SellerOrderId))
                {
                    orderType.IsFBA = 1;
                }

                NSession.Save(orderType);
                NSession.Flush();

                ListOrderItemsResponse itemsResponse =
                    client.ListOrderItems(
                        new ListOrderItemsRequest().WithAmazonOrderId(order.AmazonOrderId).WithSellerId(account.ApiToken));
                foreach (OrderItem orderItem in itemsResponse.ListOrderItemsResult.OrderItems)
                {
                    if (orderItem.SellerSKU != null)
                        orderItem.SellerSKU = orderItem.SellerSKU.Replace("-FR", "");
                    CreateOrderPruduct(orderItem.OrderItemId, GetRealSKU(orderItem.SellerSKU, account.AccountName, NSession),
                                       Utilities.ToInt(orderItem.QuantityOrdered),
                                       orderItem.Title,
                                       orderItem.ASIN, orderItem.ItemPrice == null ? 0 : Utilities.ToDouble(orderItem.ItemPrice.Amount),
                                       "",
                                       orderType.Id,
                                       orderType.OrderNo, NSession);
                }
                //订单同步同时设定预定重量
                orderType.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(orderType.OrderNo, NSession));
                GetOrderChoose(orderType, NSession);
                NSession.Clear();
                NSession.Update(orderType);
                NSession.Flush();

                LoggerUtil.GetOrderRecord(orderType, "同步订单", "同步订单", NSession);
                ///////////////////////////////////////////
                ////FBA订单直接扣除库存
                //if (orderType.FBABy == "FBA")
                //{
                //    //orderType.IsFBA = 1;
                //    //orderType.FBABy = f;
                //    orderType.Status = "已发货";
                //    orderType.Enabled = 1;
                //    orderType.IsAudit = 1;
                //    NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + orderType.Id).ExecuteUpdate();
                //    //LoggerUtil.GetOrderRecord(orderType, "设置海外仓订单", "设置为海外仓订单", curruser, NSession);

                //    List<WarehouseType> warehouseTypes = NSession.CreateQuery("from WarehouseType where WCode='" + orderType.Account + "'").List<WarehouseType>().ToList();
                //    if (warehouseTypes.Count > 0)
                //    {
                //        foreach (OrderItem orderItem in itemsResponse.ListOrderItemsResult.OrderItems)
                //        {
                //            Utilities.StockOut(warehouseTypes[0].Id, GetRealSKU(orderItem.SellerSKU, account.AccountName, NSession), Utilities.ToInt(orderItem.QuantityOrdered), "海外仓发货", "系统",
                //                               "", orderType.OrderNo, NSession);
                //        }
                //    }

                //    NSession.SaveOrUpdate(orderType);
                //    NSession.Flush();
                //}
                ///////////////////////////////////////////

                results.Add(GetResult(orderType.OrderExNo, "", "导入成功"));
            }
            else
            {
                results.Add(GetResult(order.AmazonOrderId.ToString(), "该订单已存在", "导入失败"));
            }
        }

        #region 亚马逊是否存在订单 public static bool IsExist(string OrderExNo)
        public static bool AmazonIsExist(string OrderExNo, ISession NSession, string Account)
        {
            object obj = 0;

            obj = NSession.CreateQuery("select count(Id) from OrderType where OrderExNo=:p").SetString("p", Account + "_" + OrderExNo).UniqueResult();
            //obj = NSession.CreateQuery("select count(Id) from OrderType where OrderExNo=:p or OrderExNo=:q").SetString("p", Account + "_" + OrderExNo).SetString("q", OrderExNo).UniqueResult();


            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        public static List<ResultInfo> APIByAmazonStock(AccountType account, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();

            // Developer AWS access key
            string accessKey = account.ApiKey;//"AKIAIFEICDUPSGL36SWA";

            // Developer AWS secret key
            string secretKey = account.ApiSecret;//"jRMN9OpFS5vFAETyTPzvidxwRd32+SemZWM5NAgX";

            // The client application name
            string appName = "bestore";

            // The client application version
            string appVersion = "1.0";

            // The endpoint for region service and version (see developer guide)
            // ex: https://mws.amazonservices.com
            string serviceURL = account.Email;


            FBAInventoryServiceMWSConfig config = new FBAInventoryServiceMWSConfig();
            config.ServiceURL = serviceURL;
            // Set other client connection configurations here if needed
            // Create the client itself
            FBAInventoryServiceMWSClient client = new FBAInventoryServiceMWSClient(accessKey, secretKey, appName, appVersion, config);


            try
            {
                ListInventorySupplyRequest request = new ListInventorySupplyRequest();
                //ListOrdersRequest request = new ListOrdersRequest();
                request.SellerId = account.ApiToken;//"A2EPQPAS638GSV";
                request.QueryStartDateTime = DateTime.Now.AddYears(-1);
                request.ResponseGroup = "Detailed";
                ListInventorySupplyResponse response = client.ListInventorySupply(request);
                OrderHelper helper = new OrderHelper();
                foreach (InventorySupply supply in response.ListInventorySupplyResult.InventorySupplyList.member)
                {
                    helper.UpdateStockType(account, NSession, supply);


                }
                //获取下一页的数据
                if (!string.IsNullOrEmpty(response.ListInventorySupplyResult.NextToken))
                {
                    ListInventorySupplyByNextTokenRequest requestnexttoken = new ListInventorySupplyByNextTokenRequest();
                    requestnexttoken.SellerId = account.ApiToken;//"A2EPQPAS638GSV";
                    requestnexttoken.NextToken = response.ListInventorySupplyResult.NextToken;
                    ListInventorySupplyByNextTokenResponse responseNext = client.ListInventorySupplyByNextToken(requestnexttoken);
                    foreach (InventorySupply supply in responseNext.ListInventorySupplyByNextTokenResult.InventorySupplyList.member)
                    {
                        helper.UpdateStockType(account, NSession, supply);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return results;
        }

        public void UpdateStockType(AccountType account, ISession NSession, InventorySupply supply)
        {
            string accessKey = account.ApiKey;//"AKIAIFEICDUPSGL36SWA";

            // Developer AWS secret key
            string secretKey = account.ApiSecret;//"jRMN9OpFS5vFAETyTPzvidxwRd32+SemZWM5NAgX";

            // The client application name
            string appName = "bestore";

            // The client application version
            string appVersion = "1.0";

            // The endpoint for region service and version (see developer guide)
            // ex: https://mws.amazonservices.com
            string serviceURL = account.Email;

            FBAStockType fbaStockType = new FBAStockType();
            fbaStockType.Account = account.AccountName;
            fbaStockType.Qty = Convert.ToInt32(supply.InStockSupplyQuantity);
            //商品总数
            fbaStockType.TotalQty = Convert.ToInt32(supply.TotalSupplyQuantity);
            //SKU
            fbaStockType.SKU = supply.SellerSKU;
            fbaStockType.CreateOn = fbaStockType.UpdateOn = DateTime.Now;
            fbaStockType.Condition = supply.Condition;
            fbaStockType.FNSKU = supply.FNSKU;
            fbaStockType.ASIN = supply.ASIN;
            InventorySupplyDetailList detaillist = supply.SupplyDetail;
            if (detaillist != null && detaillist.member.Count > 0)
            {
                foreach (InventorySupplyDetail detail in detaillist.member)
                {
                    if (detail.SupplyType == "Transfer")
                    {
                        fbaStockType.TransferQty += detail.Quantity;
                    }
                }
            }

            //增加商品信息
            GetMyPriceForASINRequest productrequest = new GetMyPriceForASINRequest();
            MarketplaceWebServiceProductsConfig productconfig = new MarketplaceWebServiceProductsConfig();
            productconfig.ServiceURL = serviceURL;

            //返回您自己的商品的价格信息
            MarketplaceWebServiceProductsClient productclient = new MarketplaceWebServiceProductsClient(appName, appVersion, accessKey, secretKey, productconfig);
            ASINListType asinlist = new ASINListType();
            asinlist.ASIN.Add(supply.ASIN);
            productrequest.ASINList = asinlist;
            productrequest.SellerId = account.ApiToken;
            productrequest.MarketplaceId = "";
            productrequest.WithMarketplaceId(account.Phone);
            GetMyPriceForASINResponse productresponse = productclient.GetMyPriceForASIN(productrequest);
            List<GetMyPriceForASINResult> r = productresponse.GetMyPriceForASINResult;
            if (r != null && r.Count > 0 && r[0].Product.Offers.Offer != null && r[0].Product.Offers.Offer.Count > 0)
            {
                //商品的配送渠道
                fbaStockType.FulfillmentChannel = r[0].Product.Offers.Offer[0].FulfillmentChannel;

            }
            NSession.SaveOrUpdate(fbaStockType);
            NSession.Flush();
        }
        //public static List<ResultInfo> GetMyPriceForASIN(FBAStockType f)
        //{

        //}
        public static DateTime GetAliDate(string DateStr)
        {
            return new DateTime(Convert.ToInt32(DateStr.Substring(0, 4)), Convert.ToInt32(DateStr.Substring(4, 2)), Convert.ToInt32(DateStr.Substring(6, 2)), Convert.ToInt32(DateStr.Substring(8, 2)), Convert.ToInt32(DateStr.Substring(10, 2)), Convert.ToInt32(DateStr.Substring(12, 2))).AddHours(16);
        }

        public static List<ResultInfo> APIBySMT(AccountType account, DateTime st, DateTime et, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            string token = AliUtil.RefreshToken(account);
            List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();
            AliOrderListType aliOrderList = null;
            int page = 1;
            do
            {
                try
                {
                    aliOrderList = AliUtil.findOrderListQuery(account.ApiKey, account.ApiSecret, token, page, null, null);

                    if (aliOrderList.totalItem != 0)
                    {
                        foreach (var o in aliOrderList.orderList)
                        {
                            //if (o.orderId.ToString() == "78817852985973")
                            //{
                            //    int nI = 0;
                            //}

                            bool isExist = IsExist(o.orderId.ToString(), NSession);
                            if (!isExist)
                            {
                                AliOrderType ot = AliUtil.findOrderById(account.ApiKey, account.ApiSecret, token, o.orderId.ToString());
                                if (ot.id == null) continue;

                                OrderType order = new OrderType
                                {
                                    IsMerger = 0,
                                    Enabled = 1,
                                    IsOutOfStock = 0,
                                    IsRepeat = 0,
                                    IsSplit = 0,
                                    Status = OrderStatusEnum.待处理.ToString(),
                                    IsPrint = 0,
                                    CreateOn = DateTime.Now,
                                    ScanningOn = DateTime.Now
                                };
                                order.OrderNo = Utilities.GetOrderNo(NSession);
                                order.CurrencyCode = ot.orderAmount.currencyCode;
                                order.OrderExNo = ot.id.ToString();
                                order.Amount = order.AmountOld = ot.orderAmount.amount;
                                order.LogisticMode = o.productList[0].logisticsServiceName;
                                CountryType country =
                                    countryTypes.Find(
                                        p => p.CountryCode.ToUpper() == ot.receiptAddress.country.ToUpper());
                                if (country != null)
                                {
                                    order.Country = country.ECountry;
                                }
                                else
                                {
                                    order.Country = ot.receiptAddress.country;
                                }
                                bool flag = false;
                                //将国家由Great Britain改为United Kingdom
                                if (order.Country == "Great Britain")
                                {
                                    order.Country = "United Kingdom";
                                    flag = true;
                                }
                                order.BuyerName = ot.buyerInfo.firstName + " " + ot.buyerInfo.lastName;
                                order.BuyerEmail = ot.buyerInfo.email;
                                foreach (ProductList p in o.productList)
                                {
                                    order.BuyerMemo += p.memo;
                                }
                                // OrderMsgTypeList msgTypes = AliUtil.findOrderMsgByOrderId(account.ApiKey, account.ApiSecret, token, order.OrderExNo);

                                //foreach (OrderMsgType orderMsgType in msgTypes.result)
                                //{
                                //    order.BuyerMemo += "<br/>" + orderMsgType.senderName + "  " +
                                //                       GetAliDate(orderMsgType.gmtCreate).ToString("yyyy-MM-dd HH:mm:ss") +
                                //                       ":" + orderMsgType.content + "";
                                //}
                                if (!string.IsNullOrEmpty(order.BuyerMemo))
                                    order.IsLiu = 1;

                                order.TId = "";
                                order.OrderFees = 0;
                                order.OrderCurrencyCode = "";
                                order.Account = account.AccountName;
                                order.GenerateOn = GetAliDate(ot.gmtPaySuccess);
                                order.Platform = PlatformEnum.Aliexpress.ToString();
                                order.AddressId = CreateAddress(ot.receiptAddress.contactPerson,
                                                                ot.receiptAddress.detailAddress + "  " + ot.receiptAddress.address2,
                                                                ot.receiptAddress.city, ot.receiptAddress.province,
                                                                country == null
                                                                    ? ot.receiptAddress.country
                                                                    : country.ECountry,
                                                                country == null
                                                                    ? ot.receiptAddress.country
                                                                    : country.CountryCode, ot.receiptAddress.phoneCountry + " " + ot.receiptAddress.phoneArea + " " + ot.receiptAddress.phoneNumber,
                                                                ot.receiptAddress.mobileNo, ot.buyerInfo.email,
                                                                ot.receiptAddress.zip, 0, flag, NSession);
                                NSession.Save(order);
                                NSession.Flush();
                                foreach (ChildOrderList item in ot.childOrderList)
                                {
                                    string remark = "";
                                    if (item.productAttributes.Length > 0)
                                    {
                                        SkuListType skuList =
                                            JsonConvert.DeserializeObject<SkuListType>(
                                                item.productAttributes);
                                        foreach (SkuType skuType in skuList.sku)
                                        {
                                            remark += skuType.pName + ":" + skuType.pValue;
                                        }
                                    }
                                    string pic = "";

                                    CreateOrderPruduct(item.productId.ToString(), GetRealSKU(item.skuCode, account.AccountName, NSession), item.productCount,
                                                       item.productName,
                                                       remark, item.initOrderAmt.amount, pic
                                                       ,
                                                       order.Id,
                                                       order.OrderNo, NSession);
                                }
                                //订单同步同时设定预定重量
                                order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                                GetOrderChoose(order, NSession);
                                NSession.Clear();
                                NSession.Update(order);
                                NSession.Flush();
                                LoggerUtil.GetOrderRecord(order, "同步订单", "同步订单", NSession);
                                results.Add(GetResult(order.OrderExNo, "", "导入成功"));

                            }
                            else
                            {
                                //NSession.CreateSQLQuery("update Orders set Status='已发货' where OrderExNo='" + o.orderId.ToString() + "'").UniqueResult();
                                results.Add(GetResult(o.orderId.ToString(), "该订单已存在", "导入失败"));
                            }
                        }
                        page++;
                    }
                }
                catch (Exception)
                {
                    token = AliUtil.RefreshToken(account);
                    continue;
                }

            } while (aliOrderList.totalItem > (page - 1) * 50);
            return results;
        }

        public static List<ResultInfo> APIByWish(AccountType account, DateTime st, DateTime et, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            // string token = AliUtil.RefreshToken(account);
            List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();
            //AliOrderListType aliOrderList = null;
            WishOrderList wishOrderList = null;
            int page = 1;
            // List<string> list = new List<string>();
            account.ApiTokenInfo = WishUtil.RefreshToken(account);
            wishOrderList = WishUtil.GetOrderList(account.ApiTokenInfo, st.ToString("yyyy-MM-dd"));

            do
            {

                foreach (WishOrder wishOrder in wishOrderList.data)
                {

                    // list.Add(wishOrder.Order.order_id); continue;
                    if (wishOrder.Order.state != "APPROVED")
                        continue;
                    bool isExist = IsExist(wishOrder.Order.order_id.ToString(), NSession);

                    if (!isExist)
                    {
                        OrderType order = new OrderType
                        {
                            IsMerger = 0,
                            Enabled = 1,
                            IsOutOfStock = 0,
                            IsRepeat = 0,
                            IsSplit = 0,
                            Status = OrderStatusEnum.待处理.ToString(),
                            IsPrint = 0,
                            CreateOn = DateTime.Now,
                            ScanningOn = DateTime.Now
                        };
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = "USD";
                        order.OrderExNo = wishOrder.Order.order_id.ToString();
                        order.Amount = order.AmountOld = (Utilities.ToDouble(wishOrder.Order.price) +
                                     Utilities.ToDouble(wishOrder.Order.shipping)) *
                                    Utilities.ToDouble(wishOrder.Order.quantity);
                        order.LogisticMode = "";
                        order.BuyerName = wishOrder.Order.buyer_id;
                        order.BuyerEmail = "";
                        CountryType country =
                            countryTypes.Find(
                                p => p.CountryCode.ToUpper() == wishOrder.Order.ShippingDetail.country.ToUpper());
                        if (country != null)
                        {
                            order.Country = country.ECountry;
                        }
                        else
                        {
                            order.Country = wishOrder.Order.ShippingDetail.country;
                        }
                        order.OrderFees = 0;
                        order.OrderCurrencyCode = "";
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(wishOrder.Order.order_time);
                        order.Platform = PlatformEnum.Wish.ToString();
                        bool flag = false;
                        //将义乌wish国家由Great Britain改为United Kingdom
                        if (order.Account.Contains("yw"))
                        {
                            if (order.Country == "Great Britain")
                            {
                                order.Country = "United Kingdom";
                                flag = true;
                            }
                        }
                        order.AddressId = CreateAddress(wishOrder.Order.ShippingDetail.name,
                                                        wishOrder.Order.ShippingDetail.street_address1 + "  " +
                                                        wishOrder.Order.ShippingDetail.street_address2,
                                                        wishOrder.Order.ShippingDetail.city,
                                                        wishOrder.Order.ShippingDetail.state,
                                                        country == null
                                                            ? wishOrder.Order.ShippingDetail.country
                                                            : country.ECountry,
                                                        country == null
                                                            ? wishOrder.Order.ShippingDetail.country
                                                            : country.CountryCode,
                                                        wishOrder.Order.ShippingDetail.phone_number,
                                                        wishOrder.Order.ShippingDetail.phone_number, "",
                                                        wishOrder.Order.ShippingDetail.zipcode, 0, flag, NSession);
                        NSession.Save(order);
                        NSession.Flush();
                        CreateOrderPruduct(wishOrder.Order.variant_id, GetRealSKU(wishOrder.Order.sku, account.AccountName, NSession),
                                           Utilities.ToInt(wishOrder.Order.quantity),
                                           wishOrder.Order.product_name,
                                           wishOrder.Order.color + " " + wishOrder.Order.size,
                                           Utilities.ToDouble(wishOrder.Order.cost),
                                           wishOrder.Order.product_image_url, order.Id, order.OrderNo, NSession);
                        results.Add(GetResult(order.OrderExNo, "", "导入成功"));
                        //订单同步同时设定预定重量
                        order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                        GetOrderChoose(order, NSession);
                        NSession.Clear();
                        NSession.Update(order);
                        NSession.Flush();


                    }
                    else
                    {
                        //NSession.CreateSQLQuery("update Orders set Status='已发货' where OrderExNo='" + o.orderId.ToString() + "'").UniqueResult();
                        results.Add(GetResult(wishOrder.Order.order_id.ToString(), "该订单已存在", "导入失败"));
                    }
                }
                if (!string.IsNullOrEmpty(wishOrderList.paging.next))
                {
                    wishOrderList = WishUtil.GetOrderListByUrl(wishOrderList.paging.next);
                }
                else
                {
                    break;

                }
            } while (1 == 1);
            //StringBuilder sb=new StringBuilder();
            //foreach (string s in list)
            //{
            //    sb.AppendLine(s);
            //}
            return results;
        }

        /// <summary>
        /// Cdisount获取订单
        /// </summary>
        /// <param name="account"></param>
        /// <param name="st"></param>
        /// <param name="et"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static List<ResultInfo> APIByCdisount(AccountType account, DateTime st, DateTime et, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();

            // 货币集合
            List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();

            // 获取Token
            string token = CdiscountUtil.SecuredService.GetTokenInteg(account.Phone, account.Email);

            // 通过API获取订单
            DDX.OrderManagementSystem.App.com.cdiscount.wsvc.OrderListMessage OrderList = CdiscountUtil.SecuredService.GetOrderList(token, st, et);

            foreach (DDX.OrderManagementSystem.App.com.cdiscount.wsvc.Order Order in OrderList.OrderList)
            {
                if (Order.OrderState != com.cdiscount.wsvc.OrderStateEnum.WaitingForShipmentAcceptation)
                    continue;
                bool isExist = IsExist(Order.OrderNumber, NSession);

                if (!isExist)
                {
                    OrderType order = new OrderType
                    {
                        IsMerger = 0,
                        Enabled = 1,
                        IsOutOfStock = 0,
                        IsRepeat = 0,
                        IsSplit = 0,
                        Status = OrderStatusEnum.待处理.ToString(),
                        IsPrint = 0,
                        CreateOn = DateTime.Now,
                        ScanningOn = DateTime.Now
                    };
                    order.OrderNo = Utilities.GetOrderNo(NSession);
                    order.CurrencyCode = "EUR"; // 欧元
                    order.OrderExNo = Order.OrderNumber;
                    order.Amount = order.AmountOld = (Utilities.ToDouble(Order.InitialTotalAmount));
                    //order.Amount = order.AmountOld = (Utilities.ToDouble(wishOrder.Order.price) +
                    //             Utilities.ToDouble(wishOrder.Order.shipping)) *
                    //            Utilities.ToDouble(wishOrder.Order.quantity);
                    order.LogisticMode = "";
                    order.BuyerName = Order.Customer.FirstName + " " + Order.Customer.LastName;
                    order.BuyerEmail = "";
                    CountryType country =
                        countryTypes.Find(
                            p => p.CountryCode.ToUpper() == Order.BillingAddress.Country);
                    if (country != null)
                    {
                        order.Country = country.ECountry;
                    }
                    else
                    {
                        order.Country = Order.BillingAddress.Country;
                    }
                    bool flag = false;
                    //将国家由Great Britain改为United Kingdom
                    if (order.Country == "Great Britain")
                    {
                        order.Country = "United Kingdom";
                        flag = true;
                    }
                    order.OrderFees = 0;
                    order.OrderCurrencyCode = "";
                    order.Account = account.AccountName;
                    order.GenerateOn = Convert.ToDateTime(Order.CreationDate);
                    order.Platform = PlatformEnum.Cdiscount.ToString();
                    order.AddressId = CreateAddress(Order.ShippingAddress.FirstName + " " + Order.ShippingAddress.LastName,
                                                    String.Format("Apartment {0} {1} Building {2} {3} {4} {5}",
                                                          Order.ShippingAddress.ApartmentNumber,
                                                          Order.ShippingAddress.Street,
                                                          Order.ShippingAddress.Building,
                                                          Order.ShippingAddress.ZipCode,
                                                          Order.ShippingAddress.City,
                                                          Order.ShippingAddress.Country),
                                                    Order.ShippingAddress.City,
                                                    Order.ShippingAddress.County,
                                                    country == null
                                                        ? Order.ShippingAddress.Country
                                                        : country.ECountry,
                                                    country == null
                                                        ? Order.ShippingAddress.Country
                                                        : country.CountryCode,
                                                    Order.Customer.Phone,
                                                    Order.Customer.MobilePhone, "",
                                                    Order.ShippingAddress.ZipCode, 0, flag, NSession);
                    NSession.Save(order);
                    NSession.Flush();
                    // 创建订单商品
                    foreach (DDX.OrderManagementSystem.App.com.cdiscount.wsvc.OrderLine product in Order.OrderLineList)
                    {
                        CreateOrderPruduct(product.Sku, GetRealSKU(product.Sku, account.AccountName, NSession),
                                           Utilities.ToInt(product.Quantity),
                                           product.Name,
                                           "",  // 规格
                                           Utilities.ToDouble(product.PurchasePrice),
                                           "",  // image_url
                                           order.Id,
                                           order.OrderNo, NSession);
                    }
                    results.Add(GetResult(order.OrderExNo, "", "导入成功"));
                    //订单同步同时设定预定重量
                    order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                    GetOrderChoose(order, NSession);
                    NSession.Clear();
                    NSession.Update(order);
                    NSession.Flush();

                }
                else
                {
                    //NSession.CreateSQLQuery("update Orders set Status='已发货' where OrderExNo='" + o.orderId.ToString() + "'").UniqueResult();
                    results.Add(GetResult(Order.OrderNumber, "该订单已存在", "导入失败"));
                }
            }
            //StringBuilder sb=new StringBuilder();
            //foreach (string s in list)
            //{
            //    sb.AppendLine(s);
            //}
            return results;
        }

        public static List<ResultInfo> APIByLazada(AccountType account, DateTime st, DateTime et, ISession NSession)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Lazada_Request.Request.GetOrders(account.UserName, account.ApiSecret, st, et, account.Phone));
            request.Method = "POST";
            request.ContentLength = 0;//远程服务器返回错误: (411) 所需的长度 报错解决
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            string ret = sr.ReadToEnd();
            sr.Close();
            response.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ret);
            //string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
            ////JToken orderlist = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            //JObject orderlist = JsonConvert.DeserializeObject<JObject>(json);
            //JArray orders = (JArray)orderlist["SuccessResponse"]["Body"]["Orders"]["Order"];

            /////////////////////////////////////////
            XmlNodeList nodeList = doc.SelectSingleNode("SuccessResponse/Body/Orders").ChildNodes;

            //遍历所有子节点
            //foreach (XmlNode xn in nodeList)
            //{
            //    xn["OrderId"].InnerText;
            //}
            List<ResultInfo> results = new List<ResultInfo>();
            List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();

            //foreach (JObject obj in orders)
            foreach (XmlNode obj in nodeList)
            {
                if (obj["Statuses"]["Status"].InnerText != "ready_to_ship" && obj["Statuses"]["Status"].InnerText != "pending")
                    continue;
                bool isExist = IsExist(obj["OrderNumber"].InnerText, NSession);

                if (!isExist)
                {
                    OrderType order = new OrderType
                    {
                        IsMerger = 0,
                        Enabled = 1,
                        IsOutOfStock = 0,
                        IsRepeat = 0,
                        IsSplit = 0,
                        Status = OrderStatusEnum.待处理.ToString(),
                        IsPrint = 0,
                        CreateOn = DateTime.Now,
                        ScanningOn = DateTime.Now,
                        Freight = 0.01,
                        IsFreight = 1
                    };
                    order.OrderNo = Utilities.GetOrderNo(NSession);
                    // 判断货币类型
                    switch (obj["AddressBilling"]["Country"].InnerText)
                    {
                        case "Malaysia":
                            // 马来
                            order.CurrencyCode = "MYR";
                            break;
                        case "Indonesia":
                            // 印尼
                            order.CurrencyCode = "IDR";
                            break;
                        case "Philippines":
                            // 菲律宾
                            order.CurrencyCode = "PHP";
                            break;
                        case "Thailand":
                            // 泰国
                            order.CurrencyCode = "THB";
                            break;
                        case "Singapore":
                            // 泰国
                            order.CurrencyCode = "SGD";
                            break;
                    }
                    //order.CurrencyCode = "USD";
                    order.OrderExNo = obj["OrderNumber"].InnerText;
                    order.TId = obj["OrderId"].InnerText;
                    order.Amount = order.AmountOld = Utilities.ToDouble(obj["Price"].InnerText);
                    //order.Amount = order.AmountOld = (Utilities.ToDouble(wishOrder.Order.price) +
                    //             Utilities.ToDouble(wishOrder.Order.shipping)) *
                    //            Utilities.ToDouble(wishOrder.Order.quantity);
                    order.LogisticMode = "";
                    order.BuyerName = obj["CustomerFirstName"].InnerText + " " + obj["CustomerLastName"].InnerText;
                    order.BuyerEmail = "";
                    CountryType country =
                        countryTypes.Find(
                            p => p.CountryCode.ToUpper() == order.CurrencyCode.ToUpper());
                    if (country != null)
                    {
                        order.Country = country.ECountry;
                    }
                    else
                    {
                        order.Country = obj["AddressBilling"]["Country"].InnerText;
                    }
                    bool flag = false;
                    //将国家由Great Britain改为United Kingdom
                    if (order.Country == "Great Britain")
                    {
                        order.Country = "United Kingdom";
                        flag = true;
                    }
                    order.OrderFees = 0;
                    order.OrderCurrencyCode = "";
                    order.Account = account.AccountName;
                    order.GenerateOn = Convert.ToDateTime(obj["CreatedAt"].InnerText);
                    order.Platform = PlatformEnum.Lazada.ToString();
                    order.AddressId = CreateAddress(obj["AddressShipping"]["FirstName"].InnerText + " " + obj["AddressShipping"]["LastName"].InnerText,
                                                    obj["AddressShipping"]["Address1"].InnerText,
                                                    obj["AddressShipping"]["City"].InnerText.Split('-')[0].ToString(),
                                                    obj["AddressShipping"]["City"].InnerText.Split('-')[1].ToString(),
                                                    country == null
                                                        ? obj["AddressBilling"]["Country"].InnerText
                                                        : country.ECountry,
                                                    country == null
                                                        ? obj["AddressBilling"]["Country"].InnerText
                                                        : country.CountryCode,
                                                    obj["AddressBilling"]["Phone"].InnerText,
                                                    obj["AddressBilling"]["Phone"].InnerText, "",
                                                    obj["AddressBilling"]["PostCode"].InnerText, 0, flag, NSession);


                    NSession.Save(order);
                    NSession.Flush();
                    ////////////////////////////////////////////////////////////
                    // 获取订单商品项目
                    HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(Lazada_Request.Request.GetMultipleOrderItems(account.UserName, account.ApiSecret, "[" + obj["OrderId"].InnerText + "]", account.Phone));
                    //request1.Method = "POST";
                    request1.ContentLength = 0;//远程服务器返回错误: (411) 所需的长度 报错解决
                    HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();

                    System.IO.StreamReader sr1 = new System.IO.StreamReader(response1.GetResponseStream(), System.Text.Encoding.UTF8);
                    string ret1 = sr1.ReadToEnd();
                    sr1.Close();
                    response1.Close();

                    XmlDocument doc1 = new XmlDocument();
                    doc1.LoadXml(ret1);

                    XmlNodeList nodeList1 = doc1.SelectSingleNode("SuccessResponse/Body/Orders/Order/OrderItems").ChildNodes;
                    // 创建订单商品
                    foreach (XmlNode OrderItem in nodeList1)
                    {
                        //OrderItem["Sku"].ToString();
                        //OrderItem["ItemPrice"].ToString();
                        string remark = "";
                        //if (OrderItem["ExtraAttributes"].InnerText.Length > 0)
                        //{
                        //    SkuListType skuList =
                        //        JsonConvert.DeserializeObject<SkuListType>(
                        //            OrderItem["ExtraAttributes"].InnerText);
                        //    foreach (SkuType skuType in skuList.sku)
                        //    {
                        //        remark += skuType.pName + ":" + skuType.pValue;
                        //    }
                        //}
                        string pic = "";
                        CreateOrderPruduct(OrderItem["ShopSku"].InnerText, GetRealSKU(OrderItem["Sku"].InnerText, account.AccountName, NSession), 1,
                                           OrderItem["Name"].InnerText,
                                           remark, Convert.ToDouble(OrderItem["ItemPrice"].InnerText), pic
                                           ,
                                           order.Id,
                                           order.OrderNo, OrderItem["OrderItemId"].InnerText, NSession);
                    }
                    //订单同步同时设定预定重量
                    order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                    GetOrderChoose(order, NSession);
                    NSession.Clear();
                    NSession.Update(order);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(order, "同步订单", "同步订单", NSession);
                    results.Add(GetResult(order.OrderExNo, "", "导入成功"));

                    //CreateOrderPruduct(wishOrder.Order.variant_id, GetRealSKU(wishOrder.Order.sku, account.AccountName, NSession),
                    //                   Utilities.ToInt(wishOrder.Order.quantity),
                    //                   wishOrder.Order.product_name,
                    //                   wishOrder.Order.color + " " + wishOrder.Order.size,
                    //                   Utilities.ToDouble(wishOrder.Order.cost),
                    //                   wishOrder.Order.product_image_url, order.Id, order.OrderNo, NSession);
                    //results.Add(GetResult(order.OrderExNo, "", "导入成功"));

                }
                else
                {
                    //NSession.CreateSQLQuery("update Orders set Status='已发货' where OrderExNo='" + o.orderId.ToString() + "'").UniqueResult();
                    results.Add(GetResult(obj["OrderNumber"].InnerText, "该订单已存在", "导入失败"));
                }
            }
            //if (!string.IsNullOrEmpty(wishOrderList.paging.next))
            //{
            //    wishOrderList = WishUtil.GetOrderListByUrl(wishOrderList.paging.next);
            //}
            //else
            //{
            //    break;

            //}
            //} while (1 == 1);
            //StringBuilder sb=new StringBuilder();
            //foreach (string s in list)
            //{
            //    sb.AppendLine(s);
            //}
            return results;
        }

        public static List<ResultInfo> APIByBellaBuy(AccountType account, DateTime st, DateTime et, ISession NSession)
        {

            List<ResultInfo> results = new List<ResultInfo>();
            List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();

            int page = 1;
            BellaBuyRootObject rootObject = new BellaBuyRootObject();



            do
            {
                rootObject = BellaBuyUtil.GetOrderList(account.ApiToken, page, st, et);
                foreach (BellaBuyData bellaBuyData in rootObject.data)
                {
                    if (bellaBuyData.status != "2")
                        continue;
                    bool isExist = IsExist(bellaBuyData.order_id, NSession);
                    if (!isExist)
                    {
                        OrderType order = new OrderType
                        {
                            IsMerger = 0,
                            Enabled = 1,
                            IsOutOfStock = 0,
                            IsRepeat = 0,
                            IsSplit = 0,
                            Status = OrderStatusEnum.待处理.ToString(),
                            IsPrint = 0,
                            CreateOn = DateTime.Now,
                            ScanningOn = DateTime.Now
                        };
                        order.OrderNo = Utilities.GetOrderNo(NSession);
                        order.CurrencyCode = "USD";
                        order.OrderExNo = bellaBuyData.order_id.ToString();
                        order.Amount = order.AmountOld = Utilities.ToDouble(bellaBuyData.order_total);
                        order.LogisticMode = "";
                        order.BuyerName = bellaBuyData.name;
                        order.BuyerEmail = "";

                        CountryType country =
                            countryTypes.Find(
                                p => p.CountryCode.ToUpper() == bellaBuyData.country.ToUpper());
                        if (country != null)
                        {
                            order.Country = country.ECountry;
                        }
                        else
                        {
                            order.Country = bellaBuyData.country.ToUpper();
                        }
                        order.OrderFees = 0;
                        order.OrderCurrencyCode = "";
                        order.Account = account.AccountName;
                        order.GenerateOn = Convert.ToDateTime(bellaBuyData.order_time);
                        order.Platform = PlatformEnum.Bellabuy.ToString();
                        bool flag = false;
                        //将国家由Great Britain改为United Kingdom
                        if (order.Country == "Great Britain")
                        {
                            order.Country = "United Kingdom";
                            flag = true;
                        }
                        order.AddressId = CreateAddress(bellaBuyData.name,
                                                       bellaBuyData.address,
                                                        bellaBuyData.city,
                                                        bellaBuyData.state,
                                                        country == null
                                                            ? order.Country
                                                            : country.ECountry,
                                                        country == null
                                                            ? order.Country
                                                            : country.CountryCode,
                                                        bellaBuyData.telphone,
                                                        bellaBuyData.mobile, "",
                                                        bellaBuyData.zipcode, 0, flag, NSession);

                        NSession.Save(order);
                        NSession.Flush();

                        foreach (var bellaBuyProductse in bellaBuyData.products)
                        {
                            CreateOrderPruduct(bellaBuyProductse.sku, bellaBuyProductse.sku,
                                          Utilities.ToInt(bellaBuyProductse.product_quantity),
                                          bellaBuyProductse.product_name,
                                          bellaBuyProductse.color + " " + bellaBuyProductse.size,
                                          Utilities.ToDouble(bellaBuyProductse.product_price),
                                          bellaBuyProductse.product_img, order.Id, order.OrderNo, NSession);
                        }

                        results.Add(GetResult(order.OrderExNo, "", "导入成功"));
                        //订单同步同时设定预定重量
                        order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                        GetOrderChoose(order, NSession);
                        NSession.Update(order);
                        NSession.Flush();

                    }
                    else
                    {
                        results.Add(GetResult(bellaBuyData.order_id.ToString(), "该订单已存在", "导入失败"));
                    }
                }

                page++;
            } while (page <= rootObject.totalpage);

            return results;
        }

        public static void APIByEbayFee(AccountType account, DateTime st, DateTime et, ISession NSession)
        {
            List<ResultInfo> results = new List<ResultInfo>();
            ApiContext context = AppSettingHelper.GetGenericApiContext("US");
            context.ApiCredential.eBayToken = account.ApiToken;
            eBay.Service.Call.GetOrdersCall apicall = new eBay.Service.Call.GetOrdersCall(context);
            apicall.IncludeFinalValueFee = true;
            apicall.DetailLevelList.Add(eBay.Service.Core.Soap.DetailLevelCodeType.ReturnAll);
            eBay.Service.Core.Soap.OrderTypeCollection m = null;
            int i = 1;
            do
            {
                apicall.Pagination = new eBay.Service.Core.Soap.PaginationType();
                apicall.Pagination.PageNumber = i;
                apicall.Pagination.EntriesPerPage = 50;
                apicall.OrderRole = eBay.Service.Core.Soap.TradingRoleCodeType.Seller;
                apicall.OrderStatus = eBay.Service.Core.Soap.OrderStatusCodeType.Completed;
                apicall.CreateTimeFrom = st;
                apicall.CreateTimeTo = et;
                apicall.Execute();
                m = apicall.OrderList;
                for (int k = 0; k < m.Count; k++)
                {
                    eBay.Service.Core.Soap.OrderType ot = m[k];

                    IList<OrderType> orders = NSession.CreateQuery("from OrderType where OrderExNo='" + m[k].OrderID + "'").List<OrderType>();
                    if (orders.Count > 0)
                    {
                        OrderType order = orders[0];
                        order.OrderFees = ot.ExternalTransaction[0].FeeOrCreditAmount.Value;
                        order.OrderCurrencyCode = ot.ExternalTransaction[0].FeeOrCreditAmount.currencyID.ToString();
                        foreach (TransactionType item in ot.TransactionArray)
                        {
                            order.OrderCurrencyCode2 = item.FinalValueFee.currencyID.ToString();
                            order.OrderFees2 += item.FinalValueFee.Value;
                        }
                        NSession.Update(order);
                        NSession.Flush();
                    }
                }
                i++;
            } while (apicall.HasMoreOrders);
        }

        public static List<ResultInfo> APIByEbay(AccountType account, DateTime st, DateTime et, ISession NSession)
        {
            try
            {
                List<ResultInfo> results = new List<ResultInfo>();
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                ApiContext context = AppSettingHelper.GetGenericApiContext("US");
                context.ApiCredential.eBayToken = account.ApiToken;
                eBay.Service.Call.GetOrdersCall apicall = new eBay.Service.Call.GetOrdersCall(context);
                apicall.IncludeFinalValueFee = true;
                apicall.DetailLevelList.Add(eBay.Service.Core.Soap.DetailLevelCodeType.ReturnAll);
                eBay.Service.Core.Soap.OrderTypeCollection m = null;
                List<string> list = new List<string>();
                int i = 1;
                do
                {
                    apicall.Pagination = new eBay.Service.Core.Soap.PaginationType();
                    apicall.Pagination.PageNumber = i;
                    apicall.Pagination.EntriesPerPage = 50;
                    apicall.OrderRole = eBay.Service.Core.Soap.TradingRoleCodeType.Seller;
                    apicall.OrderStatus = eBay.Service.Core.Soap.OrderStatusCodeType.Completed;
                    apicall.ModTimeFrom = st;
                    apicall.ModTimeTo = et;
                    apicall.Execute();

                    m = apicall.OrderList;
                    for (int k = 0; k < m.Count; k++)
                    {


                        eBay.Service.Core.Soap.OrderType ot = m[k];
                        if (ot.OrderStatus == eBay.Service.Core.Soap.OrderStatusCodeType.Authenticated ||
                            ot.OrderStatus == eBay.Service.Core.Soap.OrderStatusCodeType.CustomCode ||
                            ot.OrderStatus == eBay.Service.Core.Soap.OrderStatusCodeType.Default ||
                            ot.OrderStatus == eBay.Service.Core.Soap.OrderStatusCodeType.Inactive ||
                            ot.OrderStatus == eBay.Service.Core.Soap.OrderStatusCodeType.InProcess)
                        {
                            //去除别的订单状态
                            continue;
                        }
                        if (ot.PaidTime == DateTime.MinValue)
                        {
                            continue;
                        }
                        //if (ot.OrderID == "185714381010")
                        //{

                        //}
                        //查看是不是在订单系统里面存在
                        bool isExist = IsExist(ot.OrderID, NSession);

                        //if (ot.OrderID == "331870052375-1323202296014")
                        //{
                        //    int nI = 0;
                        //}

                        if (!isExist)
                        {
                            OrderType order = new OrderType
                            {
                                IsMerger = 0,
                                Enabled = 1,
                                IsOutOfStock = 0,
                                IsRepeat = 0,
                                IsSplit = 0,
                                Status = OrderStatusEnum.待处理.ToString(),
                                IsPrint = 0,
                                CreateOn = DateTime.Now,
                                ScanningOn = DateTime.Now
                            };
                            order.OrderNo = Utilities.GetOrderNo(NSession);
                            order.CurrencyCode = ot.AmountPaid.currencyID.ToString();
                            order.OrderExNo = ot.OrderID;
                            order.Amount = order.AmountOld = ot.AmountPaid.Value;
                            if (order.CurrencyCode != "USD")
                            {
                                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order.CurrencyCode);
                                CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
                                if (currencyType != null || currencyType2 != null)
                                {
                                    order.AmountOld =
                                        Math.Round(order.Amount * Convert.ToDouble(currencyType.CurrencyValue) / Convert.ToDouble(currencyType2.CurrencyValue),
                                                   2);
                                }
                            }
                            order.Country = ot.ShippingAddress.CountryName;
                            bool flag = false;
                            //将国家由Great Britain改为United Kingdom
                            if (order.Country == "Great Britain")
                            {
                                order.Country = "United Kingdom";
                                flag = true;
                            }
                            order.BuyerName = ot.BuyerUserID;
                            order.BuyerEmail = ot.TransactionArray[0].Buyer.Email;
                            order.BuyerMemo = ot.BuyerCheckoutMessage;

                            order.TId = ot.ExternalTransaction[0].ExternalTransactionID;
                            order.OrderFees = ot.ExternalTransaction[0].FeeOrCreditAmount.Value;
                            order.OrderCurrencyCode = ot.ExternalTransaction[0].FeeOrCreditAmount.currencyID.ToString();
                            order.Account = account.AccountName;
                            order.GenerateOn = ot.PaidTime;
                            order.Platform = PlatformEnum.Ebay.ToString();

                            order.AddressId = CreateAddress(ot.ShippingAddress.Name,
                                                            (string.IsNullOrEmpty(ot.ShippingAddress.Street)
                                                                 ? ""
                                                                 : ot.ShippingAddress.Street) +
                                                            (string.IsNullOrEmpty(ot.ShippingAddress.Street1)
                                                                 ? ""
                                                                 : ot.ShippingAddress.Street1) +
                                                            (string.IsNullOrEmpty(ot.ShippingAddress.Street2)
                                                                 ? ""
                                                                 : ot.ShippingAddress.Street2),
                                                            ot.ShippingAddress.CityName, ot.ShippingAddress.StateOrProvince,
                                                            ot.ShippingAddress.CountryName,
                                                            ot.ShippingAddress.Country.ToString(), ot.ShippingAddress.Phone,
                                                            ot.ShippingAddress.Phone, ot.TransactionArray[0].Buyer.Email,
                                                            ot.ShippingAddress.PostalCode, 0, flag, NSession);
                            NSession.Save(order);
                            NSession.Flush();
                            foreach (TransactionType item in ot.TransactionArray)
                            {
                                string sku = "";
                                if (item.Variation != null)
                                {
                                    sku = item.Variation.SKU;
                                }
                                else
                                {
                                    sku = item.Item.SKU;
                                }
                                if (item.FinalValueFee != null)
                                {
                                    order.OrderCurrencyCode2 = item.FinalValueFee.currencyID.ToString();
                                    order.OrderFees2 += item.FinalValueFee.Value;
                                }


                                CreateOrderPruduct(item.Item.ItemID, GetRealSKU(sku, account.AccountName, NSession), item.QuantityPurchased, item.Item.Title,
                                                   item.TransactionID, 0,
                                                   "http://thumbs.ebaystatic.com/pict/" + item.Item.ItemID + "6464_1.jpg",
                                                   order.Id,
                                                   order.OrderNo, NSession);
                                //CreateOrderPruduct(item.Item.ItemID, sku, item.QuantityPurchased, item.Item.Title,
                                //                   item.TransactionID, 0,
                                //                   "http://thumbs.ebaystatic.com/pict/" + item.Item.ItemID + "6464_1.jpg",
                                //                   order.Id,
                                //                   order.OrderNo, NSession);
                            }
                            //订单同步同时设定预定重量
                            order.Weight = Convert.ToInt32(OrderHelper.GerProductWeightExpect(order.OrderNo, NSession));
                            GetOrderChoose(order, NSession);
                            NSession.Clear();
                            NSession.Update(order);
                            NSession.Flush();
                            results.Add(GetResult(order.OrderExNo, "", "导入成功"));
                        }
                        else
                        {
                            results.Add(GetResult(ot.OrderID, "该订单已存在", "导入失败"));
                        }
                    }
                    i++;
                } while (apicall.HasMoreOrders);

                return results;
            }
            catch (Exception ex)
            {

                return null;
            }

        }
        #endregion
        #region 订单自动规则
        // 获取商品状态
        public static bool GerProductStatus(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();

                if (result.Contains(product[0].Status))
                {
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 是否包含货品
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="result"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductSKU(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();

                if (result.Contains(product[0].SKU))
                {
                    return true;
                }

            }
            return false;
        }
        public static bool GerProductSkuNum(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            int count = 0;
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();

                if (result.Contains(product[0].SKU))
                {
                    count += product.Count;
                }

            }
            return false;
        }
        /// <summary>
        /// 货品总数量
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="result"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductNum(string OrderNo, string result, ISession NSession)
        {
            bool flag;
            decimal count = 0;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                count += product.Count;

            }
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(count + resultlist[0]) && MyEvaluator.EvaluateToBool(count + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(count + result);
                return flag;
            }
            //decimal count = List.Sum(x => x.Price);
        }
        /// <summary>
        /// 是否包含订单标签
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="result"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductTag(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();

                if (result.Contains(product[0].ProductAttribute))
                {
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 是否属于该类别
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="result"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductCategory(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();

                if (result.Contains(product[0].Category))
                {
                    return true;
                }

            }
            return false;
        }
        //重量是否符合范围
        public static bool GerProductWeightTotal(string OrderNo, string result, ISession NSession)
        {
            decimal orderweight = GerProductAmountExpect(OrderNo, NSession);
            bool flag;
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(orderweight + resultlist[0]) && MyEvaluator.EvaluateToBool(orderweight + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(orderweight + result);
                return flag;
            }

        }
        /// <summary>
        /// 长度是否符合范围
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductLenghtTotal(string OrderNo, string result, ISession NSession)
        {
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            bool flag = false;
            double orderlength = 0;
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    orderlength += product[0].Long;
                }

            }
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(orderlength + resultlist[0]) && MyEvaluator.EvaluateToBool(orderlength + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(orderlength + result);
                return flag;
            }

        }
        /// <summary>
        /// 宽度是否符合范围
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductWideTotal(string OrderNo, string result, ISession NSession)
        {
            decimal OrderWide = 0;
            bool flag;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    OrderWide += Convert.ToDecimal(product[0].Wide);
                }
            }
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(OrderWide + resultlist[0]) && MyEvaluator.EvaluateToBool(OrderWide + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(OrderWide + result);
                return flag;
            }
        }
        /// <summary>
        /// 高度是否符合范围
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductHighTotal(string OrderNo, string result, ISession NSession)
        {
            decimal producthigh = 0;
            bool flag;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    producthigh += Convert.ToDecimal(product[0].High);
                }
            }
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(producthigh + resultlist[0]) && MyEvaluator.EvaluateToBool(producthigh + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(producthigh + result);
                return flag;
            }

        }
        /// <summary>
        /// 体积是否符合范围
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static bool GerProductVolumTotal(string OrderNo, string result, ISession NSession)
        {
            decimal productvolum = 0;
            bool flag;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    productvolum += Convert.ToDecimal(product[0].Long) * Convert.ToDecimal(product[0].High) * Convert.ToDecimal(product[0].Wide);
                }
            }
            if (result.Contains("且"))
            {
                string[] resultlist = result.Split('且');
                flag = MyEvaluator.EvaluateToBool(productvolum + resultlist[0]) && MyEvaluator.EvaluateToBool(productvolum + resultlist[1]);
                return flag;

            }
            else
            {
                flag = MyEvaluator.EvaluateToBool(productvolum + result);
                return flag;
            }

        }
        /// <summary>
        /// 获取可选条件
        /// </summary>
        /// <returns></returns>
        public static void GetOrderChoose(OrderType order, ISession NSession)
        {
            string logicmode = order.LogisticMode;
            string fbaby = order.FBABy;
            List<OrdersRulersType> orderrulerslist = NSession.CreateQuery(" from OrdersRulersType where OrderAction='需人工审核订单' and IsUse=1 order by Priority asc").List<OrdersRulersType>().ToList();
            //判断是否该订单满足规则
            bool flag = false;
            //先判断需要人工审核的订单规则，如果满足则改订单不需要做任何处理
            if (orderrulerslist != null && orderrulerslist.Count > 0)
            {
                flag = IsMatchRulers(order, orderrulerslist, 1, NSession);
            }
            //如果有留言的订单则需要人工审核
            if (!string.IsNullOrEmpty(order.BuyerMemo) && order.Account == "ywsmt02")
            {
                flag = true;
            }
            //如果该订单不满足人工审核的规则，则判断发货仓库和邮寄方式两种规则，必须都满足才可以变成已处理订单
            //2017-03-01调整  订单满足条件变为待处理，客服再次人工审核
            if (!flag)
            {
                //判断发货仓库
                List<OrdersRulersType> orderrulerslist2 = NSession.CreateQuery(" from OrdersRulersType where OrderAction='分配发货仓库'     and IsUse=1 order by Priority asc").List<OrdersRulersType>().ToList();
                bool WareHouse = false;
                if (orderrulerslist2 != null && orderrulerslist2.Count > 0)
                {
                    WareHouse = IsMatchRulers(order, orderrulerslist2, 2, NSession);
                }
                if (WareHouse)
                {
                    //如果仓库匹配成功
                    order.LogisticMode = logicmode;

                    //如果需要扣库存
                    if (order.IsminusStock == 1)
                    {
                        int i = EditOrderByFBA(order, order.FBABy, NSession);
                        if (i != 1)
                        {
                            order.ErrorInfo = "扣库存失败，请检查原因";
                        }
                    }
                    else
                    {

                        // order.Status = OrderStatusEnum.已处理.ToString();
                        // order.IsAudit = 1;
                        order.Status = OrderStatusEnum.待处理.ToString();

                    }
                }
                else
                {


                    //判断邮寄方式
                    List<OrdersRulersType> orderrulerslist3 = NSession.CreateQuery(" from OrdersRulersType where OrderAction='匹配邮寄方式' and IsUse=1 order by Priority asc").List<OrdersRulersType>().ToList();
                    if (orderrulerslist3 != null && orderrulerslist3.Count > 0)
                    {
                        bool LogicMode = IsMatchRulers(order, orderrulerslist3, 3, NSession);
                        if (LogicMode)
                        {
                            if (order.Account == "ywsmt02" && order.LogisticMode == "俄罗斯香港小包(联)")
                            {
                                OrderHelper.SetQueOrder(order, NSession);
                                if (order.IsOutOfStock == 1)
                                { }
                                else
                                {
                                    order.LogisticMode = "CDEK";
                                }
                                order.Status = OrderStatusEnum.已处理.ToString();
                                order.IsAudit = 1;
                            }
                            //如果需要扣库存
                            else if (order.IsminusStock == 1)
                            {
                                //order.Status = OrderStatusEnum.已发货.ToString();
                                //order.Enabled = 1;
                                //order.IsAudit = 1;
                                int i = EditOrderByFBA(order, order.FBABy, NSession);
                                if (i != 1)
                                {
                                    order.ErrorInfo = "扣库存失败，请检查原因";
                                }
                                //调用海外仓扣库存的方法
                                // order.FBABy=
                            }
                            else
                            {
                                // order.Status = OrderStatusEnum.已处理.ToString();
                                // order.IsAudit = 1;
                                order.Status = OrderStatusEnum.待处理.ToString();
                            }
                            order.TrackCode = Utilities.GetTrackCode(NSession, order.LogisticMode);
                        }


                        else
                        {
                            order.FBABy = fbaby;
                            order.LogisticMode = logicmode;
                        }
                    }
                }
            }

            else
            {
                order.ErrorInfo = "需人工审核订单";
            }




        }
        /// <summary>
        /// 该订单是否满足规则
        /// </summary>
        /// <returns></returns>
        public static bool IsMatchRulers(OrderType order, List<OrdersRulersType> orderrulerslist, int type, ISession NSession)
        {
            try
            {
                bool flag = false;
                string strError = "";
                if (order.AddressId != 0)
                {
                    order.AddressInfo = NSession.Get<OrderAddressType>(order.AddressId);
                }
                else
                {
                    order.AddressInfo = null;
                }
                foreach (OrdersRulersType ordersrulers in orderrulerslist)
                {
                    string orderchoose = ordersrulers.OrderChoose;
                    string[] divlist = orderchoose.Split(new string[] { "</a></div>" }, StringSplitOptions.RemoveEmptyEntries);
                    if (divlist.Length > 0)
                    {
                        //遍历规则中每一条子规则,如果其中一条子规则不满足则跳出循环
                        foreach (string div in divlist)
                        {
                            string[] result = div.Split(new string[] { ">", "</a>" }, StringSplitOptions.RemoveEmptyEntries);
                            string result2 = "";
                            if (result.Length >= 3)
                            {
                                result2 = result[2];
                            }

                            if (div.Contains("指定范围"))
                            {
                                result2 = result2.Replace("≥", ">=");
                                result2 = result2.Replace("&lt;", "<");
                                result2 = result2.Replace("&gt;", ">");
                                result2 = result2.Replace("≤", "<=");

                            }
                            if (div.Contains("订单来源为指定平台/站点/账号"))//必须包含账号,且账号满足了才显示不匹配原因
                            {
                                //遍历result2的账号，再一一进行比较
                                string[] accountresult = result2.Split(',');

                                for (int i = 0; i < accountresult.Length; i++)
                                {
                                    if (order.Account == accountresult[i])
                                    {
                                        flag = true;
                                        break;
                                    }
                                    else
                                    {
                                        flag = false;

                                    }
                                }

                                if (!flag)
                                {
                                    break;
                                }
                            }
                            else if (div.Contains("发货仓库为指定仓库"))
                            {
                                if (result2.Contains(order.FBABy))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    strError = "发货仓库不匹配";
                                    flag = false;
                                    break;

                                }
                            }
                            else if (div.Contains("买家选择的运输类型为指定运输类型"))
                            {
                                if (result2.Contains(order.LogisticMode == null ? "" : order.LogisticMode))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    strError = "运输类型不匹配";
                                    flag = false;
                                    break;
                                }
                            }

                            else if (div.Contains("订单目的地为指定国家"))
                            {

                                if (order.AddressInfo != null)
                                {
                                    List<CountryType> country = NSession.CreateQuery(" from CountryType where ECountry='" + order.AddressInfo.Country + "'").List<CountryType>().ToList();
                                    if (result2.Contains(country[0].CCountry))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单目的地不匹配";
                                        flag = false;
                                        break;

                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;

                                }
                            }
                            else if (div.Contains("订单收货地址省/州包含指定字符串"))
                            {
                                if (order.AddressInfo != null)
                                {
                                    if (result2.Contains(order.AddressInfo.Province))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单收货地址省/州包含字符串不匹配";
                                        flag = false;
                                        break;

                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;

                                }
                            }
                            else if (div.Contains("订单收货地址城市包含指定字符串"))
                            {
                                if (order.AddressInfo != null)
                                {
                                    if (result2.Contains(order.AddressInfo.City))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单收货地址城市包含字符串不匹配";
                                        flag = false;
                                        break;

                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;

                                }
                            }
                            else if (div.Contains("订单收货地址街道包含指定字符串"))
                            {
                                if (order.AddressInfo != null)
                                {
                                    if (result2.Contains(order.AddressInfo.Street))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单收货地址街道不匹配";
                                        flag = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            else if (div.Contains("订单收货地址街道信息字符长度小于指定长度"))
                            {
                                if (order.AddressInfo != null)
                                {
                                    if (order.AddressInfo.Province.Length < Convert.ToInt32(result2))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单收货地址街道信息字符长度不匹配";
                                        flag = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            else if (div.Contains("订单收货邮编字符小于指定长度"))
                            {
                                if (order.AddressInfo != null)
                                {
                                    if (order.AddressInfo.PostCode.Length < Convert.ToInt32(result2))
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        strError = "订单收货邮编字符不匹配";
                                        flag = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    flag = false;
                                    break;
                                }
                            }

                            else if (div.Contains("订单收件人电话要求指定条件"))
                            {
                                if (order.AddressInfo != null && order.AddressInfo.Tel != null && order.AddressInfo.Phone != null)
                                {
                                    int tellong = 0;
                                    if (result2.Contains("并且"))
                                    {
                                        string[] fanweilist = result2.Split(new string[] { "并且" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (fanweilist.Length > 0)
                                        {
                                            string[] tellength = fanweilist[0].Split('≤');
                                            if (tellength.Length >= 2)
                                            {
                                                tellong = Convert.ToInt32(tellength[1]);
                                            }
                                        }
                                        if (order.AddressInfo != null && order.AddressInfo.Tel != null)
                                        {
                                            if (order.AddressInfo.Tel.Length <= tellong)
                                            {
                                                flag = true;
                                            }
                                        }
                                        else
                                        {
                                            strError = "订单收件人电话不匹配";
                                            break;
                                        }

                                    }
                                    else if (result2.Contains("或者"))
                                    {
                                        string[] fanweilist = result2.Split(new string[] { "或者" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (fanweilist.Length > 0)
                                        {
                                            string[] tellength = fanweilist[0].Split('≤');
                                            if (tellength.Length >= 2)
                                            {
                                                tellong = Convert.ToInt32(tellength[1]);
                                            }
                                        }
                                        if (order.AddressInfo != null && order.AddressInfo.Tel != null)
                                        {
                                            if (order.AddressInfo.Phone.Length <= tellong)
                                            {
                                                flag = true;
                                            }
                                        }
                                        else
                                        {
                                            strError = "订单收件人电话不匹配";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (result2.Contains("移动电话"))
                                        {
                                            string[] tellength = result2.Split('≤');
                                            if (tellength.Length >= 2)
                                            {
                                                tellong = Convert.ToInt32(tellength[1]);
                                            }
                                            if (order.AddressInfo.Tel.Length <= tellong)
                                            {
                                                flag = true;
                                            }
                                            else
                                            {
                                                strError = "移动电话不匹配";
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            string[] tellength = result2.Split('≤');
                                            if (tellength.Length >= 2)
                                            {
                                                tellong = Convert.ToInt32(tellength[1]);
                                            }
                                            if (order.AddressInfo.Phone.Length <= tellong)
                                            {
                                                flag = true;
                                            }
                                            else
                                            {
                                                strError = "移动电话不匹配";
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else if (div.Contains("订单收件人姓名/地址存在指定异常"))
                            {
                                string[] yichanglist = result2.Split(',');
                                if (yichanglist.Length > 0)
                                {
                                    foreach (string yichang in yichanglist)
                                    {
                                        int num = Convert.ToInt32(yichang.Substring(yichang.Length - 1, 1));
                                        if (yichang.Contains("姓名字符中空格数"))
                                        {
                                            string Addressee = order.AddressInfo.Addressee;
                                            string[] Addresseelist = Addressee.Split(' ');
                                            if (Addresseelist.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "姓名字符中空格数不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (yichang.Contains("姓名字符数"))
                                        {
                                            if (order.AddressInfo.Addressee.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "姓名字符数不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (yichang.Contains("地址字符数"))
                                        {
                                            if (order.AddressInfo.Street.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "地址字符数不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (yichang.Contains("城市名字字符数"))
                                        {
                                            if (order.AddressInfo.City.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "城市名字字符数不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (yichang.Contains("省/州名字字符数"))
                                        {
                                            if (order.AddressInfo.Province.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "省/州名字字符数不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (yichang.Contains("电话号码数字字符"))
                                        {
                                            if (order.AddressInfo.Tel.Length < num && order.AddressInfo.Phone.Length < num)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                strError = "电话号码数字字符不匹配";
                                                flag = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }


                            else if (div.Contains("订单收货邮编指定格式"))
                            {
                                if (result2.Contains("并且"))
                                {
                                    string[] postcoderesult = result2.Split(new string[] { "并且" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (postcoderesult.Length >= 2)
                                    {

                                    }
                                    else
                                    {
                                        strError = "订单收货邮编不匹配";
                                        flag = false;
                                        break;
                                    }
                                }
                                else if (result2.Contains("或者"))
                                {
                                    string[] postcoderesult = result2.Split(new string[] { "或者" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (postcoderesult.Length >= 2)
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }

                            else if (div.Contains("订单总金额指定范围"))
                            {
                                //如果订单总金额的币种和输入选中的币种一直则不需要转换，否则需要转换比较
                                string inputcurrencycode = result2.Split(',')[0];
                                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                                CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == inputcurrencycode);
                                decimal orderamount = Convert.ToDecimal(order.Amount);
                                if (order.CurrencyCode != inputcurrencycode)
                                {

                                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order.CurrencyCode);

                                    orderamount = Convert.ToDecimal(order.Amount) * currencyType.CurrencyValue;
                                }
                                if (result2.Contains("且"))
                                {

                                    string[] fanweilist = result2.Split(',')[1].Split('且');
                                    decimal inputvalue1 = Convert.ToDecimal(fanweilist[0].Split(' ')[1]);
                                    decimal inputvalue2 = Convert.ToDecimal(fanweilist[1].Split(' ')[1]);
                                    //flag = MyEvaluator.EvaluateToBool(orderamount + Convert.ToDecimal(fanweilist[0]) * Convert.ToDecimal(currencyType2.CurrencyValue) &&  MyEvaluator.EvaluateToBool(orderamount + Convert.ToDecimal(fanweilist[1]) * Convert.ToDecimal(currencyType2.CurrencyValue));  
                                    if (order.CurrencyCode != inputcurrencycode)
                                    {

                                        CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order.CurrencyCode);

                                        inputvalue1 = inputvalue1 * Convert.ToDecimal(currencyType2.CurrencyValue);
                                        inputvalue2 = inputvalue2 * Convert.ToDecimal(currencyType2.CurrencyValue);
                                    }
                                    bool a = MyEvaluator.EvaluateToBool(orderamount + fanweilist[0].Split(' ')[0] + inputvalue1);
                                    bool b = MyEvaluator.EvaluateToBool(orderamount + fanweilist[1].Split(' ')[0] + inputvalue2);
                                    flag = a && b;
                                    if (!flag)
                                    {
                                        strError = "订单总金额不匹配";
                                        break;
                                    }


                                }
                                else
                                {
                                    decimal inputvalue = Convert.ToDecimal(result2.Split(' ')[1]);
                                    if (order.CurrencyCode != inputcurrencycode)
                                    {

                                        CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order.CurrencyCode);

                                        inputvalue = inputvalue * Convert.ToDecimal(currencyType2.CurrencyValue);

                                    }
                                    flag = MyEvaluator.EvaluateToBool(orderamount + result2.Split(',')[1].Split(' ')[0] + inputvalue);
                                    if (!flag)
                                    {
                                        strError = "订单总金额不匹配";
                                        break;
                                    }
                                }
                            }
                            else if (div.Contains("订单利润指定范围"))
                            {
                                if (result2.Contains("且"))
                                {
                                    string[] fanweilist = result2.Split('且');
                                    flag = MyEvaluator.EvaluateToBool(order.Profit + fanweilist[0]) && MyEvaluator.EvaluateToBool(order.Profit + fanweilist[1]); if (!flag)
                                    {
                                        strError = "订单利润不匹配";
                                        break;
                                    }


                                }
                                else
                                {
                                    flag = MyEvaluator.EvaluateToBool(order.Profit + result2);
                                    if (!flag)
                                    {
                                        strError = "订单利润不匹配";
                                        break;
                                    }
                                }
                            }
                            else if (div.Contains("订单单笔交易额指定范围"))
                            {
                                if (result2.Contains("且"))
                                {
                                    string[] fanweilist = result2.Split('且');
                                    flag = MyEvaluator.EvaluateToBool(order.Amount + fanweilist[0]) && MyEvaluator.EvaluateToBool(order.Amount + fanweilist[1]); if (!flag)
                                    {
                                        strError = "订单单笔交易额不匹配";
                                        break;
                                    }


                                }
                                else
                                {
                                    flag = MyEvaluator.EvaluateToBool(order.Amount + result2); if (!flag)
                                    {
                                        strError = "订单单笔交易额不匹配";
                                        break;
                                    }

                                }
                            }
                            else if (div.Contains("买家支付的运费指定范围"))
                            {
                                if (result2.Contains("且"))
                                {
                                    string[] fanweilist = result2.Split('且');
                                    flag = MyEvaluator.EvaluateToBool(order.Freight + fanweilist[0]) && MyEvaluator.EvaluateToBool(order.Freight + fanweilist[1]); if (!flag)
                                    {
                                        strError = "买家支付的运费不匹配";
                                        break;
                                    }


                                }
                                else
                                {
                                    flag = MyEvaluator.EvaluateToBool(order.Freight + result2); if (!flag)
                                    {
                                        strError = "买家支付的运费不匹配";
                                        break;
                                    }

                                }
                            }
                            else if (div.Contains("订单货品包含指定货品"))
                            {
                                flag = GerProductSKU(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单货品不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品总数量指定范围"))
                            {
                                flag = GerProductNum(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单货品总数量不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品包含指定货品且数量指定范围"))
                            {
                                flag = GerProductSkuNum(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单货品包含指定货品且数量不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品属于指定分类"))
                            {
                                flag = GerProductCategory(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "分类不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品标签包含指定的商品标签"))
                            {
                                flag = GerProductTag(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "商品标签不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单重量指定范围 (g)"))
                            {
                                flag = GerProductWeightTotal(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单重量不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品长度指定范围(cm)"))
                            {
                                flag = GerProductLenghtTotal(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单货品长度不匹配";
                                    break;
                                }

                            }
                            else if (div.Contains("订单货品宽度指定范围 (cm)"))
                            {
                                flag = GerProductWideTotal(order.OrderNo, result2, NSession); if (!flag)
                                {
                                    strError = "订单货品宽度不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品高度指定范围 (cm)"))
                            {
                                flag = GerProductHighTotal(order.OrderNo, result2, NSession);
                                if (!flag)
                                {
                                    strError = "订单货品高度不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品体积指定范围 (cm³)"))
                            {
                                flag = GerProductVolumTotal(order.OrderNo, result2, NSession);
                                if (!flag)
                                {
                                    strError = "订单货品体积不匹配";
                                    break;
                                }
                            }
                            else if (div.Contains("订单货品状态包含指定的商品状态"))
                            {
                                flag = GerProductStatus(order.OrderNo, result2, NSession);
                                if (!flag)
                                {
                                    strError = "商品状态不匹配";
                                    break;
                                }
                            }

                        }
                        //满足单条规则所有条件的情况下，处理该订单
                        if (flag)
                        {
                            //需要人工审批的订单不处理
                            if (type == 1)
                            {

                                break;
                            }
                            else if (type == 2) //判断发货仓库
                            {
                                order.IsminusStock = ordersrulers.IsMinusStock;
                                order.FBABy = ordersrulers.WareHouse;
                                break;

                            }
                            else //判断邮寄方式
                            {
                                order.LogisticMode = ordersrulers.Logictis;

                                break;
                            }
                        }
                    }

                }
                string strtype = "";
                if (type == 2)
                {
                    strtype = "匹配仓库";
                }
                else if (type == 3)
                {
                    strtype = "匹配邮寄方式";
                }
                if (flag && type != 1)
                {
                    //LoggerUtil.GetOrderRecord(order, "订单匹配", "匹配成功", NSession);

                    LoggerUtil.GetOrderRecord(order, "订单匹配", "匹配成功", NSession);
                }
                else if (type == 1)
                { }
                else
                {
                    if (!string.IsNullOrEmpty(strError))
                    {
                        LoggerUtil.GetOrderRecord(order, "订单匹配", strError, NSession);
                        order.ErrorInfo = strError;
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {

                LoggerUtil.GetOrderRecord(order, "订单匹配报错", ex.Message, NSession);
                return false;
            }

        }
        /// <summary>
        /// 扣除库存
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int EditOrderByFBA(OrderType orderType, string f, ISession NSession)
        {

            // 判断是否缺货订单并禁止发货

            OrderHelper.SetQueOrder(f, orderType, NSession);
            if (orderType.IsOutOfStock == 1)
            {
                //if (type.ErrorInfo == null)
                //{
                //    type.ErrorInfo = "";
                //}
                //type.ErrorInfo = type.ErrorInfo.Replace(" 无法出库缺货订单", "");
                //type.ErrorInfo += " 无法出库缺货订单";
                //NSession.Update(type);
                //NSession.Flush();
                //return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前订单" + type.OrderNo + "为缺货状态!" });
                //return Json(new { IsSuccess = false, ErrorMsg = "无法出库!当前订单为缺货状态" });
                //return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单为缺货状态！" });
                return -1;
            }



            if (orderType.IsFBA == 0 || orderType.Status == "待处理")
            {
                bool iscon = true;
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId='" + orderType.Id + "'").List<OrderProductType>().ToList();

                foreach (var item in orderType.Products)
                {
                    if (item.SKU == null)
                    {

                        orderType.ErrorInfo += "SKU不符";
                        break;
                    }
                    List<ProductType> products = NSession.CreateQuery("from ProductType where SKU='" + item.SKU + "'").List<ProductType>().ToList();
                    if (products.Count == 0)
                    {
                        iscon = false;
                        orderType.ErrorInfo += "SKU不符";
                        break;

                    }
                    else
                    {

                    }
                }
                if (iscon)
                {
                    orderType.IsFBA = 1;
                    orderType.FBABy = f;
                    //验俄罗斯海外仓订单显示已处理，跟单导入单号后再显示已发货
                    //if (f == "YWRU-AEA")
                    //{
                    //    orderType.Status = "已处理";
                    //}
                    //else
                    //{
                    orderType.Status = "已发货";
                    //}
                    orderType.Enabled = 1;
                    orderType.IsAudit = 1;
                    NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + orderType.Id).ExecuteUpdate();
                    //LoggerUtil.GetOrderRecord(orderType, "设置海外仓订单", "设置为海外仓订单", NSession);

                    ////FBA订单直接扣除库存
                    //List<WarehouseType> warehouseTypes = NSession.CreateQuery("from WarehouseType where WCode='" + orderType.Account + "'").List<WarehouseType>().ToList();
                    //if (warehouseTypes.Count > 0)
                    //{
                    //    if (f == "FBA")
                    //    {
                    //        foreach (var item in orderType.Products)
                    //        {
                    //            Utilities.StockOut(warehouseTypes[0].Id, item.SKU, item.Qty, "FBA出库", "admin",
                    //                               "", orderType.OrderNo, NSession);
                    //        }
                    //    }
                    //}

                    switch (f)
                    {
                        case "KS":
                            //KS订单直接扣除库存 KS仓库(美西-宁波)
                            List<WarehouseType> warehouseTypesKS = NSession.CreateQuery("from WarehouseType where WCode='KS'").List<WarehouseType>().ToList();
                            if (warehouseTypesKS.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesKS[0].Id, item.SKU, item.Qty, "KS仓库(美西-宁波)出库", "admin", "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "KS-YW":
                            //KS-YW订单直接扣除库存 KS仓库(义乌)
                            List<WarehouseType> warehouseTypesKSYW = NSession.CreateQuery("from WarehouseType where WCode='KS-YW'").List<WarehouseType>().ToList();
                            if (warehouseTypesKSYW.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesKSYW[0].Id, item.SKU, item.Qty, "KS仓库(义乌)出库", "admin", "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "US-East":
                            //US-East订单直接扣除库存 美东(宁波)
                            List<WarehouseType> warehouseTypesUSEast = NSession.CreateQuery("from WarehouseType where WCode='US-East'").List<WarehouseType>().ToList();
                            if (warehouseTypesUSEast.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesUSEast[0].Id, item.SKU, item.Qty, "美东(宁波)出库", "admin", "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "CA":
                            //CA订单直接扣除库存 美东(义乌)
                            List<WarehouseType> warehouseTypesCA = NSession.CreateQuery("from WarehouseType where WCode='CA'").List<WarehouseType>().ToList();
                            if (warehouseTypesCA.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesCA[0].Id, item.SKU, item.Qty, "美东(义乌)出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWRU-AEA":
                            //YWRU-AEA订单直接扣除库存 YWRU-AEA仓库
                            List<WarehouseType> warehouseTypesYWRUAEA = NSession.CreateQuery("from WarehouseType where WCode='YWAEA'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWRUAEA.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWRUAEA[0].Id, item.SKU, item.Qty, "YWRU-AEA出库", "admin", "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWRU-AEB":
                            //YWRU-AEB订单直接扣除库存 YWRU-AEB仓库
                            List<WarehouseType> warehouseTypesYWRUAEB = NSession.CreateQuery("from WarehouseType where WCode='YWAEB'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWRUAEB.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWRUAEB[0].Id, item.SKU, item.Qty, "YWRU-AEB出库", "admin", "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "LAI":
                            //LAI替换CA,ebay和ywaz us和us01都会用
                            List<WarehouseType> warehouseTypesLAI = NSession.CreateQuery("from WarehouseType where WCode='LAI'").List<WarehouseType>().ToList();
                            if (warehouseTypesLAI.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesLAI[0].Id, item.SKU, item.Qty, "LAI出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "UKMAN":
                            //UKMAN（英国海外仓）
                            List<WarehouseType> warehouseTypesUKMAN = NSession.CreateQuery("from WarehouseType where WCode='UKMAN'").List<WarehouseType>().ToList();
                            if (warehouseTypesUKMAN.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesUKMAN[0].Id, item.SKU, item.Qty, "UKMAN出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWAZDE":
                            //YWAZDE（德国三方海外仓）
                            List<WarehouseType> warehouseTypesYWAZDE = NSession.CreateQuery("from WarehouseType where WCode='YWAZDE'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWAZDE.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWAZDE[0].Id, item.SKU, item.Qty, "YWAZDE出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;

                        case "FBA":
                            //FBA订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBA = NSession.CreateQuery("from WarehouseType where WCode='" + orderType.Account + "'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBA.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBA[0].Id, item.SKU, item.Qty, "FBA出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB01":
                            //FBA-NB01订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB01 = NSession.CreateQuery("from WarehouseType where WCode='nbaz01'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB01.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB01[0].Id, item.SKU, item.Qty, "FBA-NB01出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB02":
                            //FBA-NB02订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB02 = NSession.CreateQuery("from WarehouseType where WCode='nbaz02'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB02.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB02[0].Id, item.SKU, item.Qty, "FBA-NB02出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB03":
                            //FBA-NB03订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB03 = NSession.CreateQuery("from WarehouseType where WCode='nbaz03'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB03.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB03[0].Id, item.SKU, item.Qty, "FBA-NB03出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB04":
                            //FBA-NB04订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB04 = NSession.CreateQuery("from WarehouseType where WCode='nbaz03'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB04.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB04[0].Id, item.SKU, item.Qty, "FBA-NB04出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB05":
                            //FBA-NB05订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB05 = NSession.CreateQuery("from WarehouseType where WCode='nbaz05'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB05.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB05[0].Id, item.SKU, item.Qty, "FBA-NB05出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB06":
                            //FBA-NB06订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB06 = NSession.CreateQuery("from WarehouseType where WCode='nbaz06'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB06.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB06[0].Id, item.SKU, item.Qty, "FBA-NB06出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB07":
                            //FBA-NB07订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB07 = NSession.CreateQuery("from WarehouseType where WCode='nbaz07'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB07.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB07[0].Id, item.SKU, item.Qty, "FBA-NB07出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB08":
                            //FBA-NB08订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB08 = NSession.CreateQuery("from WarehouseType where WCode='nbaz08'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB08.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB08[0].Id, item.SKU, item.Qty, "FBA-NB08出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-NB09":
                            //FBA-NB09订单直接扣除库存
                            List<WarehouseType> warehouseTypesFBANB09 = NSession.CreateQuery("from WarehouseType where WCode='nbaz09'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBANB09.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBANB09[0].Id, item.SKU, item.Qty, "FBA-NB09出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "GCUS-East":
                            //谷仓美东（宁波）订单直接扣除库存
                            List<WarehouseType> warehouseTypesGCUSEast = NSession.CreateQuery("from WarehouseType where WCode='GCUS-East'").List<WarehouseType>().ToList();
                            if (warehouseTypesGCUSEast.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesGCUSEast[0].Id, item.SKU, item.Qty, "谷仓美东（宁波）出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "NBRU-AEA":
                            //NBRU-AEA订单直接扣除库存 NBRU-AEA仓库
                            List<WarehouseType> warehouseTypesNBRUAEA = NSession.CreateQuery("from WarehouseType where WCode='NBRU-AEA'").List<WarehouseType>().ToList();
                            if (warehouseTypesNBRUAEA.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesNBRUAEA[0].Id, item.SKU, item.Qty, "NBRU-AEA出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZUS":
                            //FBA-YWAZUS订单直接扣除库存FBA-YWAZUS仓库
                            List<WarehouseType> warehouseTypesFBAYWAZUS = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWAZUS.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWAZUS[0].Id, item.SKU, item.Qty, "FBA-YWAZUS出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZUS01":
                            //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                            List<WarehouseType> warehouseTypesFBAYWAZUS01 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us01'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWAZUS01.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWAZUS01[0].Id, item.SKU, item.Qty, "FBA-YWAZUS01出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZUS02":
                            //FBA-YWAZUS02订单直接扣除库存FBA-YWAZUS02仓库
                            List<WarehouseType> warehouseTypesFBAYWAZUS02 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us02'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWAZUS02.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWAZUS02[0].Id, item.SKU, item.Qty, "FBA-YWAZUS02出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZUK":
                            //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                            List<WarehouseType> warehouseTypesFBAYWUK01 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-uk'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWUK01.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWUK01[0].Id, item.SKU, item.Qty, "FBA-YWAZUK出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZUK02":
                            //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                            List<WarehouseType> warehouseTypesFBAYWUK02 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-uk02'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWUK02.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWUK02[0].Id, item.SKU, item.Qty, "FBA-YWAZUK02出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZDE":
                            //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                            List<WarehouseType> warehouseTypesFBAYWAZDE = NSession.CreateQuery("from WarehouseType where WCode='ywaz-de'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWAZDE.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWAZDE[0].Id, item.SKU, item.Qty, "FBA-YWAZDE出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "FBA-YWAZFR":
                            //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                            List<WarehouseType> warehouseTypesFBAYWAZFR = NSession.CreateQuery("from WarehouseType where WCode='ywaz-fr'").List<WarehouseType>().ToList();
                            if (warehouseTypesFBAYWAZFR.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesFBAYWAZFR[0].Id, item.SKU, item.Qty, "FBAYWAZFR出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWGCUS-West":
                            //谷仓美西（义乌）订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWGCUSWest = NSession.CreateQuery("from WarehouseType where WCode='YWGCUS-West'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWGCUSWest.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWGCUSWest[0].Id, item.SKU, item.Qty, "谷仓美西（义乌）出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWGCUS-East":
                            //谷仓美东（义乌）订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWGCUSEast = NSession.CreateQuery("from WarehouseType where WCode='YWGCUS-East'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWGCUSEast.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWGCUSEast[0].Id, item.SKU, item.Qty, "谷仓美东（义乌）出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWCA-WEST(DONG)":
                            //美西(董)（义乌）订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWCAWESTDONG = NSession.CreateQuery("from WarehouseType where WCode='YWCA-WEST(DONG)'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWCAWESTDONG.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWCAWESTDONG[0].Id, item.SKU, item.Qty, "美西(董)（义乌）出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWNJ-EAST(LEO)":
                            //美东(LEO)（义乌）订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWNJEASTLEO = NSession.CreateQuery("from WarehouseType where WCode='YWNJ-EAST(LEO)'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWNJEASTLEO.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWNJEASTLEO[0].Id, item.SKU, item.Qty, "美东(LEO)（义乌）出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWMRU-AEA":
                            //YWMRU-AEA订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWMRUAEA = NSession.CreateQuery("from WarehouseType where WCode='YWMRU-AEA'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWMRUAEA.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWMRUAEA[0].Id, item.SKU, item.Qty, "YWMRU-AEA出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        case "YWMRU-AEB":
                            //YWMRU-AEB订单直接扣除库存
                            List<WarehouseType> warehouseTypesYWMRUAEB = NSession.CreateQuery("from WarehouseType where WCode='YWMRU-AEB'").List<WarehouseType>().ToList();
                            if (warehouseTypesYWMRUAEB.Count > 0)
                            {
                                foreach (var item in orderType.Products)
                                {
                                    Utilities.StockOut(warehouseTypesYWMRUAEB[0].Id, item.SKU, item.Qty, "YWMRU-AEB出库", "admin",
                                                       "", orderType.OrderNo, NSession);
                                }
                            }
                            break;
                        default:
                            break;
                    }


                    // 计算订单财务信息
                    OrderHelper.ReckonFinance(orderType, NSession);
                    NSession.Update(orderType);
                    NSession.Flush();
                }
                else
                {
                    //return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单已设为海外仓！" });
                    return -1;
                }
            }
            //return base.Json(new { IsSuccess = true });
            return 1;
        }

        #endregion
        #region 创建客户数据
        /// <summary>
        /// 创建客户数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="amount"></param>
        /// <param name="buyOn"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static int CreateBuyer(string name, string email, double amount, DateTime buyOn, PlatformEnum platform, ISession NSession)
        {

            IList<OrderBuyerType> list = NSession.CreateQuery(" from OrderBuyerType where BuyerName=:p and Platform=:p2").SetString("p", name).SetString("p2", platform.ToString()).List<OrderBuyerType>();
            OrderBuyerType buyer = new OrderBuyerType();
            if (list.Count > 0)
            {
                buyer = list[0];
                buyer.BuyCount += 1;
                buyer.BuyAmount += amount;
                buyer.ListBuyOn = buyOn;
            }
            else
            {
                buyer = new OrderBuyerType();
                buyer.BuyerName = name;
                buyer.BuyerEmail = email;
                buyer.FristBuyOn = buyOn;
                buyer.BuyCount = 1;
                buyer.BuyAmount = amount;
                buyer.ListBuyOn = buyOn;
                buyer.Platform = platform.ToString();
            }
            NSession.SaveOrUpdate(buyer);
            NSession.Flush();
            return buyer.Id;
        }
        #endregion

        #region 创建订单发货地址

        /// <summary>
        /// 创建订单发货地址 
        /// </summary>
        /// <param name="addressee"></param>
        /// <param name="street"></param>
        /// <param name="city"></param>
        /// <param name="province"></param>
        /// <param name="country"></param>
        /// <param name="countryCode"></param>
        /// <param name="tel"></param>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <param name="postcode"></param>
        /// <param name="buyerID"></param>
        /// <returns></returns>
        public static int CreateAddress(string addressee, string street, string city, string province, string country, string countryCode, string tel, string phone, string email, string postcode, int buyerID, ISession NSession)
        {
            try
            {

                OrderAddressType address = new OrderAddressType();
                address.Street = street;
                address.Tel = tel;
                address.City = city;
                address.Province = province;
                address.PostCode = postcode;
                address.Email = email;
                address.Country = country;
                string CC = Convert.ToString(NSession.CreateSQLQuery(string.Format("select CountryCode from Country where ECountry ='" + country + "'")).UniqueResult());
                if (CC.ToUpper() != countryCode.ToUpper())
                {
                    address.CountryCode = CC;
                }
                else
                {
                    address.CountryCode = countryCode;
                }
                address.Phone = phone;
                address.Addressee = addressee;
                address.BId = buyerID;
                NSession.Save(address);
                NSession.Flush();
                return address.Id;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public static int CreateAddress(string addressee, string street, string city, string province, string country, string countryCode, string tel, string phone, string email, string postcode, int buyerID, bool flag, ISession NSession)
        {
            try
            {

                OrderAddressType address = new OrderAddressType();
                address.Street = street;
                address.Tel = tel;
                address.City = city;
                address.Province = province;
                address.PostCode = postcode;
                address.Email = email;
                address.Country = country;
                address.CountryCode = countryCode;
                address.Phone = phone;
                address.Addressee = addressee;
                address.BId = buyerID;
                //将国家由Great Britain改为United Kingdom
                if (address.Country == "Great Britain" && flag)
                {
                    address.Country = "United Kingdom";
                    address.CountryCode = "UK";
                }
                //亚马逊订单德语Deutschland同步自动转换为英文的德国
                if (address.Country == "Deutschland" && flag)
                {
                    address.Country = "Germany";
                    address.CountryCode = "DE";
                }
                NSession.Save(address);
                NSession.Flush();
                return address.Id;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        #region 创建订单产品
        /// <summary>
        /// 创建订单产品
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="qty"></param>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="price"></param>
        /// <param name="url"></param>
        /// <param name="oid"></param>
        /// <param name="orderNo"></param>
        public static void CreateOrderPruduct(string sku, int qty, string name, string remark, double price, string url, int oid, string orderNo, ISession NSession)
        {
            CreateOrderPruduct(sku, sku, qty, name, remark, price, url, oid, orderNo, NSession);
        }

        /// <summary>
        /// 创建订单产品
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="qty"></param>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="price"></param>
        /// <param name="url"></param>
        /// <param name="oid"></param>
        /// <param name="orderNo"></param>
        public static void CreateOrderPruduct(string exSKU, string sku, int qty, string name, string remark, double price, string url, int oid, string orderNo, ISession NSession)
        {
            OrderProductType product = new OrderProductType();
            product.ExSKU = exSKU;
            if (sku != null)
                product.SKU = sku.Trim();
            product.Qty = qty;
            product.Price = price;
            product.Title = name;
            product.Url = url;
            product.OId = oid;
            product.OrderNo = orderNo;
            product.Remark = remark;
            CreateOrderPruduct(product, NSession);
        }

        /// <summary>
        /// 创建订单产品 for lazada
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="qty"></param>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="price"></param>
        /// <param name="url"></param>
        /// <param name="oid"></param>
        /// <param name="orderNo"></param>
        public static void CreateOrderPruduct(string exSKU, string sku, int qty, string name, string remark, double price, string url, int oid, string orderNo, string OrderItemId, ISession NSession)
        {
            OrderProductType product = new OrderProductType();
            product.ExSKU = exSKU;
            if (sku != null)
                product.SKU = sku.Trim();
            product.Qty = qty;
            product.Price = price;
            product.Title = name;
            product.Url = url;
            product.OId = oid;
            product.OrderNo = orderNo;
            product.Remark = remark;
            product.ImgUrl = OrderItemId;
            CreateOrderPruduct(product, NSession);
        }

        // 创建订单商品
        public static void CreateOrderPruduct(OrderProductType product, ISession NSession)
        {
            List<ProductComposeType> products = NSession.CreateQuery("from ProductComposeType where SKU='" + product.SKU + "'").List<ProductComposeType>().ToList();
            if (product.SKU == null)
                product.SKU = "";
            if (product.SKU.IndexOf("-") != -1)
            {
                product.SKU = product.SKU.Substring(0, product.SKU.IndexOf("-"));
            }
            GetItem(product, NSession);
            NSession.Save(product);
            NSession.Flush();
            // 当为组合商品时自动拆分SKU
            if (products.Count > 0)
                SplitProduct(product, products, NSession);

        }


        public static void GetItem(OrderProductType item, ISession NSession)
        {

            if (item.SKU.IndexOf(":") != -1)
            {
                System.Text.RegularExpressions.Regex r1 = new System.Text.RegularExpressions.Regex(@":D(?<num>\d+)", System.Text.RegularExpressions.RegexOptions.None);
                System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(@":A(?<desc>.+)", System.Text.RegularExpressions.RegexOptions.None);
                System.Text.RegularExpressions.Match mc1 = r1.Match(item.SKU);
                System.Text.RegularExpressions.Match mc2 = r2.Match(item.SKU);
                string[] cels = item.SKU.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (cels[0] != null)
                    item.SKU = cels[0].Trim();
                int fo = Utilities.ToInt(mc1.Groups["num"].ToString());
                if (fo != 0)
                    item.Qty = fo * item.Qty;
                try
                {
                    item.Remark += mc2.Groups["desc"].Value;
                }
                catch (Exception)
                {
                    item.Remark = "";
                }
            }

            IList<ProductType> ps = NSession.CreateQuery("from ProductType where sku='" + item.SKU + "'").List<ProductType>();
            if (ps.Count > 0)
            {
                item.Standard = ps[0].Standard;
            }
        }

        #endregion


        #region 获得真实的sku  public static string GetRealSKU(string SKUEx, string Account, ISession NSession)
        public static string GetRealSKU(string SKUEx, string Account, ISession NSession)
        {
            if (SKUEx.Contains("&amp;"))//平台带&SKU转换解决(SKU上传wish平台后平台sku无法修改)2016-9-20
            {
                SKUEx = SKUEx.Replace("&amp;", "&");
            }
            IList<SKUItemType> list =
                NSession.CreateQuery("from SKUItemType where SKUEx=:p and Account=:p2").SetString("p", SKUEx).SetString(
                    "p2", Account).SetMaxResults(1).List<SKUItemType>();
            if (list.Count > 0)
            {
                return list[0].SKU;
            }
            return SKUEx;


        }
        #endregion

        #region 是否存在订单 public static bool IsExist(string OrderExNo)
        public static bool IsExist(string OrderExNo, ISession NSession, string Account = null)
        {
            object obj = 0;
            if (string.IsNullOrEmpty(Account))
            {
                obj =
                   NSession.CreateQuery("select count(Id) from OrderType where OrderExNo=:p").SetString("p", OrderExNo)
                       .UniqueResult();
            }
            else
            {
                obj =
                     NSession.CreateQuery("select count(Id) from OrderType where OrderExNo=:p and Account=:p2").SetString("p", OrderExNo).SetString("p2", Account)
                         .UniqueResult();
            }
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 获得返回的数据
        public static ResultInfo GetResult(string key, string info, string result, string field1, string field2, string field3, string field4)
        {
            ResultInfo r = new ResultInfo();
            r.Field1 = field1;
            r.Field2 = field2;
            r.Field3 = field3;
            r.Field4 = field4;
            r.Key = key;
            r.Info = info;
            r.Result = result;
            r.CreateOn = DateTime.Now;
            return r;
        }

        public static ResultInfo GetResult(string key, string info, string result)
        {
            return GetResult(key, info, result, "", "", "", "");
        }
        #endregion


        #region 订单验证
        public static bool ValiOrder(OrderType order, List<CountryType> countrys, List<ProductType> products, List<CurrencyType> currencys, List<LogisticsModeType> logistics, ISession NSession)
        {
            bool resultValue = true;
            order.ErrorInfo = "";
            if (order.Account != "su-smt")
            {
                order.Products = NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>().ToList();

                if (order.Country != null)
                {
                    if (
                        countrys.FindIndex(
                            p => p.ECountry == order.Country || p.CountryCode.ToUpper() == order.Country.ToUpper()) == -1)
                    {
                        resultValue = false;
                        order.ErrorInfo += "国家不符 ";
                    }
                }
                else
                {
                    resultValue = false;
                    order.ErrorInfo += "国家不符 ";
                }
                if (order.CurrencyCode != null)
                {
                    CurrencyType currency = currencys.Find(p => p.CurrencyCode.ToUpper() == order.CurrencyCode.ToUpper());
                    if (currency == null)
                    {
                        resultValue = false;
                        order.ErrorInfo += "货币不符 ";
                    }
                }
                else
                {
                    resultValue = false;
                    order.ErrorInfo += "货币不符 ";
                }
                if (logistics.FindIndex(p => p.LogisticsCode == order.LogisticMode) == -1)
                {
                    resultValue = false;
                    order.ErrorInfo += "货运不符 ";
                }
                if (order.Amount == 0 && order.Platform != PlatformEnum.Amazon.ToString())
                {
                    resultValue = false;
                    order.ErrorInfo += "金额不能为0 ";
                }
                //smt超过5美金的不能验单到平邮 
                if (order.Amount > 5 && order.Platform == PlatformEnum.Aliexpress.ToString() && order.LogisticMode.Contains("平邮线上发货"))
                {
                    resultValue = false;
                    order.ErrorInfo += "smt超过$5不能发平邮线上";
                }
                order.AddressInfo = NSession.Get<OrderAddressType>(order.AddressId);
                //邮编07 35 38 51 52开头禁止验西班牙专线 
                if (order.AddressInfo.PostCode.Length >= 3)
                {
                    string code = order.AddressInfo.PostCode.Substring(0, 2);
                    if ((code == "07" || code == "35" || code == "38" || code == "51" || code == "52") && (order.LogisticMode == "西班牙专线") && (order.Country == "Spain"))
                    {
                        resultValue = false;
                        order.ErrorInfo += "邮编07 35 38 51 52开头勿发8dt西班牙专线";
                    }
                }
                foreach (var item in order.Products)
                {
                    if (item.SKU == null)
                    {
                        resultValue = false;
                        order.ErrorInfo += "SKU不符";
                        break;
                    }
                    List<ProductType> product = NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", item.SKU.Trim()).List<ProductType>().ToList();
                    if (product.Count == 0)
                    {
                        resultValue = false;
                        order.ErrorInfo += "SKU不符";
                        break;
                    }
                    else
                    {
                        if (product[0].Status == "停产")
                        {
                            order.IsStop = 1;
                            item.IsQue = 2;
                            NSession.SaveOrUpdate(item);
                            NSession.Flush();
                        }
                    }

                    // 原代码存档
                    //ProductType product = products.Find(p => p.SKU.Trim().ToUpper() == item.SKU.Trim().ToUpper());
                    //if (product == null)
                    //{
                    //    resultValue = false;
                    //    order.ErrorInfo += "SKU不符";
                    //    break;
                    //}
                    //else
                    //{
                    //    if (product.Status == "停产")
                    //    {
                    //        order.IsStop = 1;
                    //        item.IsQue = 2;
                    //        NSession.SaveOrUpdate(item);
                    //        NSession.Flush();
                    //    }
                    //}
                }

                object obj = NSession.CreateQuery("select count(Id) from OrderType where Status<>'待处理' and OrderExNo=:p and Account=:p2 and IsSplit =0 and IsRepeat=0").SetString("p", order.OrderExNo).SetString("p2", order.Account).UniqueResult();
                if (Convert.ToInt32(obj) > 0)
                {
                    resultValue = false;
                    order.ErrorInfo += " 订单重复";
                }

                if (resultValue)
                {
                    order.IsAudit = 1;
                    //SaveAmount(order, currencys, NSession);
                    if (order.IsStop == 0)
                    {
                        SetQueOrder(order, NSession);
                    }
                }
            }
            else
            {
                order.IsAudit = 1;
            }
            if (order.ErrorInfo == "")
            {
                if (order.IsMerger == 1)
                {
                    List<OrderType> orderlist = NSession.CreateQuery(" from OrderType where  MId=:p1 and Id <>:p1").SetInt32("p1", order.Id).List<OrderType>().ToList();
                    if (orderlist != null && orderlist.Count > 0)
                    {
                        foreach (OrderType or in orderlist)
                        {
                            or.Status = "已处理";
                            NSession.Update(or);
                            NSession.Flush();
                        }
                    }

                }
                order.Status = "已处理";
                order.ErrorInfo = "验证成功！";
                order.TrackCode = Utilities.GetTrackCode(NSession, order.LogisticMode);
                if (order.IsStop == 1)
                {
                    LoggerUtil.GetOrderRecord(order, "验证订单", "订单中有停产产品，自动设为停产订单。", NSession);
                }

                IList<EmailMessageType> messageTypes = NSession.CreateQuery("from EmailMessageType where OrderExNo='" + order.OrderExNo + "'").List<EmailMessageType>();
                foreach (EmailMessageType emailMessageType in messageTypes)
                {
                    order.BuyerMemo = emailMessageType.RserverDate + " 有买家留言<br>" + order.BuyerMemo;
                }
            }
            // 省为空时自动填充国家名称
            order.AddressInfo = NSession.Get<OrderAddressType>(order.AddressId);
            if (order.AddressInfo.Province == "" || order.AddressInfo.Province == null)
            {
                order.AddressInfo.Province = order.AddressInfo.Country;
            }
            // 计算订单财务数据
            OrderHelper.ReckonFinance(order, NSession);

            NSession.Clear();
            NSession.SaveOrUpdate(order);
            NSession.SaveOrUpdate(order.AddressInfo);
            NSession.Flush();
            LoggerUtil.GetOrderRecord(order, "验证订单", order.ErrorInfo, NSession);
            return resultValue;
        }

        public static void GetESTTrackCode(OrderType order, ISession NSession)
        {
            order.AddressInfo = NSession.Get<OrderAddressType>(order.AddressId);
            ExLogisticMode.createAndPreAlertOrderRequest request = new ExLogisticMode.createAndPreAlertOrderRequest();
            request.orderNo = order.OrderExNo;
            request.productCode = "A3";
            request.street = order.AddressInfo.Street;
            request.initialCountryCode = "CN";
            request.city = order.AddressInfo.City;
            request.stateOrProvince = order.AddressInfo.Province;
            request.consigneeName = order.AddressInfo.Addressee;
            request.consigneePostCode = order.AddressInfo.PostCode;
            request.consigneeTelephone = string.IsNullOrEmpty(order.AddressInfo.Phone)
                                             ? order.AddressInfo.Tel
                                             : order.AddressInfo.Phone;
            request.destinationCountryCode = order.AddressInfo.CountryCode.ToUpper();

            request.shipperCity = "HAERBIN";
            request.shipperStateOrProvince = "HEILONGJIANG";
            request.shipperAddress = "NO.7 TIANCHI ROAD PINGFANG DISTRICT";
            request.shipperCompanyName = "BaiShengTong";
            request.shipperName = "BaiShengTong";
            request.shipperTelephone = "045151922287";
            request.shipperPostCode = "150060";
            request.mctCode = "1";
            request.declareInvoice = new declareInvoice[] { new declareInvoice { eName = "Gift", unitPrice = "2" } };
            ExLogisticMode.createOrderService service = new createOrderService();
            ExLogisticMode.OrderOnlineServiceClient serviceClient = new OrderOnlineServiceClient();
            serviceClient.removeOrderService("AA397EBD8001B61377720B7C145CDD68", new string[] { "260273" });

            ExLogisticMode.createAndPreAlertOrderResponse[] responses =
                serviceClient.createAndPreAlertOrderService("AA397EBD8001B61377720B7C145CDD68", new createAndPreAlertOrderRequest[] { request });
            if (responses.Length > 0)
            {
                if (!string.IsNullOrEmpty(responses[0].trackingNumber))
                {
                    order.TrackCode = responses[0].trackingNumber;
                    NSession.Update(order);
                    NSession.Flush();
                }
            }

        }

        //public static void SetQueOrder(OrderType order, ISession NSession)
        //{
        //    if (order.OrderNo == "215568")
        //    {

        //    }
        //    //计算产品是否是缺货订单--确定定位：设置缺货标记，设置产品占位标记，根据订单审核时间设置。订单统一设置。
        //    if (order.Enabled == 0) return;
        //    bool isUse = true;
        //    bool isold = false;
        //    if (order.IsOutOfStock == 1)
        //        isold = true;
        //    if (order.Status == "已发货") return;

        //    if (order.Products == null)
        //    {
        //        order.Products = NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>().ToList();
        //    }
        //    if (order.Products.Count > 1)
        //    {
        //        order.IsCanSplit = 1;
        //    }
        //    foreach (var item in order.Products)
        //    {

        //        LoggerUtil.GetOrderRecord(order, "验证订单", "订单中有产品缺货,自动设置为缺货订单。", NSession);
        //    }
        //    else if (isold && order.IsOutOfStock == 0)
        //    {
        //        LoggerUtil.GetOrderRecord(order, "系统自动检验订单", "检验到产品有库存，缺货自动状态删除。", NSession);
        //    }

        //}

        /// <summary>
        /// 设置订单及商品缺货状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="NSession"></param>
        public static void SetQueOrder(OrderType order, ISession NSession)
        {

            if (order.Enabled != 0)
            {
                bool flag = true;
                bool flag2 = false;
                if (order.IsOutOfStock == 1)
                {
                    flag2 = true;
                }
                if (!(order.Status == "已发货"))
                {
                    if (order.Products == null)
                    {
                        order.Products = NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>().ToList();
                    }
                    if (order.Products.Count > 1)
                    {
                        order.IsCanSplit = 1;
                    }
                    foreach (OrderProductType current in order.Products)
                    {
                        // 非占位状态商品进行缺货计算
                        if (current.IsQue != 3)
                        {
                            current.IsQue = 0;
                            // 指定地区并根据地区获取指定仓库库存与该地区非海外仓订单占用库存商品数量进行对比
                            /*
                             1	NBW	宁波仓库	NULL	宁波
                             3	YWW	义乌仓库	1	    义乌
                             */
                            // 待测试
                            List<AccountType> account = NSession.CreateQuery("from AccountType where AccountName='" + order.Account + "'").List<AccountType>().ToList();
                            ////判断地区
                            int Wid = 0;
                            if (account[0].FromArea == "宁波")
                            {
                                Wid = 1;
                            }
                            else if (account[0].FromArea == "义乌")
                            {
                                Wid = 3;
                            }
                            // int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select SUM(Qty) from WarehouseStock where WId=" + Wid + " and WarehouseStock.SKU='" + current.SKU + "' ").UniqueResult());
                            // 由固定库存数量调整为实时获取仓库明细累计
                            int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select sum(NowQty) from WarehouseStockData  Where NowQty>0 and SKU='" + current.SKU + "' and WId=" + Wid + "").UniqueResult());
                            int num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from Orders O join OrderProducts OP on O.OrderNo=OP.OrderNo join Account A on O.Account=A.AccountName where A.FromArea='" + (Wid == 1 ? "宁波" : "义乌") + "' and OP.SKU='" + current.SKU + "' and OP.IsQue=3 and O.IsFBA=0").UniqueResult());
                            // 说明：IsFBA=0 计算非海外仓订单商品占用库存数量

                            // 原始脚本存档 Begin
                            //int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select SUM(Qty) from WarehouseStock where WarehouseStock.SKU='" + current.SKU + "' ").UniqueResult());
                            //int num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from OrderProducts OP where OP.SKU='" + current.SKU + "' and OP.IsQue=3").UniqueResult());
                            // 原始脚本存档 End
                            if (current.Qty > 1)
                            {
                                order.IsCanSplit = 1;
                            }
                            // 仓库数量-帐号占用库存数量<当前订单商品数量
                            if (num - num2 < current.Qty)
                            {
                                flag = false;
                                current.IsQue = 1;
                                order.IsOutOfStock = 1;
                            }
                            else
                            {
                                current.IsQue = 3;
                            }
                            NSession.SaveOrUpdate(current);
                            NSession.Flush();
                        }
                    }
                    if (flag)
                    {
                        order.IsOutOfStock = 0;
                        foreach (OrderProductType current in order.Products)
                        {
                            current.IsQue = 3;
                            NSession.SaveOrUpdate(current);
                            NSession.Flush();
                        }
                    }
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                    if (!flag2 && order.IsOutOfStock == 1)
                    {
                        // 测试时临时注消
                        LoggerUtil.GetOrderRecord(order, "验证订单", "订单中有产品缺货,自动设置为缺货订单。", NSession);
                    }
                    else
                    {
                        if (flag2 && order.IsOutOfStock == 0)
                        {
                            // 测试时临时注消
                            LoggerUtil.GetOrderRecord(order, "系统自动检验订单", "检验到产品有库存，缺货自动状态删除。", NSession);
                        }
                    }
                }
                else
                {
                    //订单触发利润更新（已发货）
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                }
            }

        }

        /// <summary>
        /// 设置订单及商品缺货状态 - 批量计算
        /// </summary>
        /// <param name="order"></param>
        /// <param name="NSession"></param>
        public static void SetQueOrderAuto(OrderType order, ISession NSession)
        {

            if (order.Enabled != 0)
            {
                bool flag = true;
                bool flag2 = false;
                if (order.IsOutOfStock == 1)
                {
                    flag2 = true;
                }
                if (!(order.Status == "已发货"))
                {
                    if (order.Products == null)
                    {
                        order.Products = NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>().ToList();
                    }
                    if (order.Products.Count > 1)
                    {
                        order.IsCanSplit = 1;
                    }
                    foreach (OrderProductType current in order.Products)
                    {
                        // 非占位状态商品进行缺货计算
                        if (current.IsQue != 3)
                        {
                            // 指定地区并根据地区获取指定仓库库存与该地区非海外仓订单占用库存商品数量进行对比
                            /*
                             1	NBW	宁波仓库	NULL	宁波
                             3	YWW	义乌仓库	1	    义乌
                             */
                            // 待测试
                            List<AccountType> account = NSession.CreateQuery("from AccountType where AccountName='" + order.Account + "'").List<AccountType>().ToList();
                            ////判断地区
                            int Wid = 0;
                            if (account[0].FromArea == "宁波")
                            {
                                Wid = 1;
                            }
                            else if (account[0].FromArea == "义乌")
                            {
                                Wid = 3;
                            }
                            // int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select SUM(Qty) from WarehouseStock where WId=" + Wid + " and WarehouseStock.SKU='" + current.SKU + "' ").UniqueResult());
                            // 由固定库存数量调整为实时获取仓库明细累计
                            int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select sum(NowQty) from WarehouseStockData  Where NowQty>0 and SKU='" + current.SKU + "' and WId=" + Wid + "").UniqueResult());
                            int num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from Orders O join OrderProducts OP on O.OrderNo=OP.OrderNo join Account A on O.Account=A.AccountName where A.FromArea='" + (Wid == 1 ? "宁波" : "义乌") + "' and OP.SKU='" + current.SKU + "' and OP.IsQue=3 and O.IsFBA=0").UniqueResult());
                            // 说明：IsFBA=0 计算非海外仓订单商品占用库存数量

                            // 原始脚本存档 Begin
                            //int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select SUM(Qty) from WarehouseStock where WarehouseStock.SKU='" + current.SKU + "' ").UniqueResult());
                            //int num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from OrderProducts OP where OP.SKU='" + current.SKU + "' and OP.IsQue=3").UniqueResult());
                            // 原始脚本存档 End
                            if (current.Qty > 1)
                            {
                                order.IsCanSplit = 1;
                            }
                            // 仓库数量-帐号占用库存数量<当前订单商品数量
                            if (num - num2 < current.Qty)
                            {
                                flag = false;
                                current.IsQue = 1;
                                order.IsOutOfStock = 1;
                            }
                            else
                            {
                                current.IsQue = 3;
                            }
                            NSession.SaveOrUpdate(current);
                            NSession.Flush();
                        }
                    }
                    if (flag)
                    {
                        order.IsOutOfStock = 0;
                        foreach (OrderProductType current in order.Products)
                        {
                            current.IsQue = 3;
                            NSession.SaveOrUpdate(current);
                            NSession.Flush();
                        }
                    }
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                    if (!flag2 && order.IsOutOfStock == 1)
                    {
                        // 测试时临时注消
                        //LoggerUtil.GetOrderRecord(order, "验证订单", "订单中有产品缺货,自动设置为缺货订单。", NSession);
                    }
                    else
                    {
                        if (flag2 && order.IsOutOfStock == 0)
                        {
                            // 测试时临时注消
                            //LoggerUtil.GetOrderRecord(order, "系统自动检验订单", "检验到产品有库存，缺货自动状态删除。", NSession);
                        }
                    }
                }
                else
                {
                    //订单触发利润更新（已发货）
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                }
            }

        }
        /// <summary>
        /// 设置订单及商品缺货状态-指定仓库
        /// </summary>
        /// <param name="order"></param>
        /// <param name="NSession"></param>
        public static void SetQueOrder(string f, OrderType order, ISession NSession)
        {
            int warehouseId = 0;
            string account = "";
            // 获取仓库ID
            switch (f)
            {
                case "KS":
                    //KS订单直接扣除库存 KS仓库(美西-宁波)
                    List<WarehouseType> warehouseTypesKS = NSession.CreateQuery("from WarehouseType where WCode='KS'").List<WarehouseType>().ToList();
                    if (warehouseTypesKS.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesKS[0].Id, item.SKU, item.Qty, "KS仓库(美西-宁波)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesKS[0].Id;
                        account = warehouseTypesKS[0].WCode;
                    }
                    break;
                case "KS-YW":
                    //KS-YW订单直接扣除库存 KS仓库(义乌)
                    List<WarehouseType> warehouseTypesKSYW = NSession.CreateQuery("from WarehouseType where WCode='KS-YW'").List<WarehouseType>().ToList();
                    if (warehouseTypesKSYW.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesKSYW[0].Id, item.SKU, item.Qty, "KS仓库(义乌)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesKSYW[0].Id;
                        account = warehouseTypesKSYW[0].WCode;
                    }
                    break;
                case "US-East":
                    //US-East订单直接扣除库存 美东(宁波)
                    List<WarehouseType> warehouseTypesUSEast = NSession.CreateQuery("from WarehouseType where WCode='US-East'").List<WarehouseType>().ToList();
                    if (warehouseTypesUSEast.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesUSEast[0].Id, item.SKU, item.Qty, "美东(宁波)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesUSEast[0].Id;
                        account = warehouseTypesUSEast[0].WCode;
                    }
                    break;
                case "CA":
                    //CA订单直接扣除库存 美东(义乌)
                    List<WarehouseType> warehouseTypesCA = NSession.CreateQuery("from WarehouseType where WCode='CA'").List<WarehouseType>().ToList();
                    if (warehouseTypesCA.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesCA[0].Id, item.SKU, item.Qty, "美东(义乌)出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesCA[0].Id;
                        account = warehouseTypesCA[0].WCode;
                    }
                    break;
                case "YWRU-AEA":
                    //YWRU-AEA订单直接扣除库存 YWRU-AEA仓库
                    List<WarehouseType> warehouseTypesYWRUAEA = NSession.CreateQuery("from WarehouseType where WCode='YWAEA'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWRUAEA.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesYWRUAEA[0].Id, item.SKU, item.Qty, "YWRU-AEA出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesYWRUAEA[0].Id;
                        account = warehouseTypesYWRUAEA[0].WCode;
                    }
                    break;
                case "YWRU-AEB":
                    //YWRU-AEB订单直接扣除库存 YWRU-AEB仓库
                    List<WarehouseType> warehouseTypesYWRUAEB = NSession.CreateQuery("from WarehouseType where WCode='YWAEB'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWRUAEB.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesYWRUAEA[0].Id, item.SKU, item.Qty, "YWRU-AEB出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesYWRUAEB[0].Id;
                        account = warehouseTypesYWRUAEB[0].WCode;
                    }
                    break;
                case "LAI":
                    //LAI替换CA,ebay和ywaz us和us01都会用
                    List<WarehouseType> warehouseTypesLAI = NSession.CreateQuery("from WarehouseType where WCode='LAI'").List<WarehouseType>().ToList();
                    if (warehouseTypesLAI.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesLAI[0].Id, item.SKU, item.Qty, "LAI出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesLAI[0].Id;
                        account = warehouseTypesLAI[0].WCode;
                    }
                    break;
                case "UKMAN":
                    //LAI替换CA,ebay和ywaz us和us01都会用
                    List<WarehouseType> warehouseTypesUKMAN = NSession.CreateQuery("from WarehouseType where WCode='UKMAN'").List<WarehouseType>().ToList();
                    if (warehouseTypesUKMAN.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesLAI[0].Id, item.SKU, item.Qty, "LAI出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesUKMAN[0].Id;
                        account = warehouseTypesUKMAN[0].WCode;
                    }
                    break;
                case "YWAZDE":
                    //ebay和ywaz us和us01都会用
                    List<WarehouseType> warehouseTypesYWAZDE = NSession.CreateQuery("from WarehouseType where WCode='YWAZDE'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWAZDE.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesLAI[0].Id, item.SKU, item.Qty, "LAI出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesYWAZDE[0].Id;
                        account = warehouseTypesYWAZDE[0].WCode;
                    }
                    break;
                case "FBA":
                    //FBA订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBA = NSession.CreateQuery("from WarehouseType where WCode='" + order.Account + "'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBA.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBA[0].Id, item.SKU, item.Qty, "FBA出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBA[0].Id;
                        account = warehouseTypesFBA[0].WCode;
                    }
                    break;
                case "FBA-NB01":
                    //FBA-NB01订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB01 = NSession.CreateQuery("from WarehouseType where WCode='nbaz01'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB01.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB01[0].Id, item.SKU, item.Qty, "FBA-NB01出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB01[0].Id;
                        account = warehouseTypesFBANB01[0].WCode;
                    }
                    break;
                case "FBA-NB02":
                    //FBA-NB02订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB02 = NSession.CreateQuery("from WarehouseType where WCode='nbaz02'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB02.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB02[0].Id, item.SKU, item.Qty, "FBA-NB02出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB02[0].Id;
                        account = warehouseTypesFBANB02[0].WCode;
                    }
                    break;
                case "FBA-NB03":
                    //FBA-NB03订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB03 = NSession.CreateQuery("from WarehouseType where WCode='nbaz03'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB03.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB03[0].Id, item.SKU, item.Qty, "FBA-NB03出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB03[0].Id;
                        account = warehouseTypesFBANB03[0].WCode;
                    }
                    break;
                case "FBA-NB04":
                    //FBA-NB04订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB04 = NSession.CreateQuery("from WarehouseType where WCode='nbaz04'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB04.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB04[0].Id, item.SKU, item.Qty, "FBA-NB04出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB04[0].Id;
                        account = warehouseTypesFBANB04[0].WCode;
                    }
                    break;
                case "FBA-NB05":
                    //FBA-NB05订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB05 = NSession.CreateQuery("from WarehouseType where WCode='nbaz05'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB05.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB05[0].Id, item.SKU, item.Qty, "FBA-NB05出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB05[0].Id;
                        account = warehouseTypesFBANB05[0].WCode;
                    }
                    break;
                case "FBA-NB06":
                    //FBA-NB06订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB06 = NSession.CreateQuery("from WarehouseType where WCode='nbaz06'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB06.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB06[0].Id, item.SKU, item.Qty, "FBA-NB06出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB06[0].Id;
                        account = warehouseTypesFBANB06[0].WCode;
                    }
                    break;
                case "FBA-NB07":
                    //FBA-NB07订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB07 = NSession.CreateQuery("from WarehouseType where WCode='nbaz07'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB07.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB07[0].Id, item.SKU, item.Qty, "FBA-NB07出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB07[0].Id;
                        account = warehouseTypesFBANB07[0].WCode;
                    }
                    break;
                case "FBA-NB08":
                    //FBA-NB08订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB08 = NSession.CreateQuery("from WarehouseType where WCode='nbaz08'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB08.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB08[0].Id, item.SKU, item.Qty, "FBA-NB08出库", CurrentUser.Realname,
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB08[0].Id;
                        account = warehouseTypesFBANB08[0].WCode;
                    }
                    break;
                case "FBA-NB09":
                    //FBA-NB09订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBANB09 = NSession.CreateQuery("from WarehouseType where WCode='nbaz09'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBANB09.Count > 0)
                    {
                        //foreach (var item in orderType.Products)
                        //{
                        //    Utilities.StockOut(warehouseTypesFBANB09[0].Id, item.SKU, item.Qty, "FBA-NB09出库", "admin",
                        //                       "", orderType.OrderNo, NSession);
                        //}
                        warehouseId = warehouseTypesFBANB09[0].Id;
                        account = warehouseTypesFBANB09[0].WCode;
                    }
                    break;
                case "FBC-NBCD01":
                    //FBC-NBCD01订单直接扣除库存
                    List<WarehouseType> warehouseTypesFBCNBCD01 = NSession.CreateQuery("from WarehouseType where WCode='nbcd01'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBCNBCD01.Count > 0)
                    {
                        warehouseId = warehouseTypesFBCNBCD01[0].Id;
                        account = warehouseTypesFBCNBCD01[0].WCode;
                    }
                    break;
                case "GCUS-East":
                    //谷仓美东（宁波）订单直接扣除库存
                    List<WarehouseType> warehouseTypesGCUSEast = NSession.CreateQuery("from WarehouseType where WCode='GCUS-East'").List<WarehouseType>().ToList();
                    if (warehouseTypesGCUSEast.Count > 0)
                    {
                        warehouseId = warehouseTypesGCUSEast[0].Id;
                        account = warehouseTypesGCUSEast[0].WCode;
                    }
                    break;
                case "GCUS-West":
                    //谷仓美西（宁波）订单直接扣除库存
                    List<WarehouseType> warehouseTypesGCUSWest = NSession.CreateQuery("from WarehouseType where WCode='GCUS-West'").List<WarehouseType>().ToList();
                    if (warehouseTypesGCUSWest.Count > 0)
                    {
                        warehouseId = warehouseTypesGCUSWest[0].Id;
                        account = warehouseTypesGCUSWest[0].WCode;
                    }
                    break;
                case "NBRU-AEA":
                    //NBRU-AEA订单直接扣除库存 NBRU-AEA仓库
                    List<WarehouseType> warehouseTypesNBRUAEA = NSession.CreateQuery("from WarehouseType where WCode='NBRU-AEA'").List<WarehouseType>().ToList();
                    if (warehouseTypesNBRUAEA.Count > 0)
                    {
                        warehouseId = warehouseTypesNBRUAEA[0].Id;
                        account = warehouseTypesNBRUAEA[0].WCode;
                    }
                    break;
                case "FBA-YWAZUS":
                    //FBA-YWAZUS订单直接扣除库存FBA-YWAZUS仓库
                    List<WarehouseType> warehouseTypesFBAYWAZUS = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWAZUS.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWAZUS[0].Id;
                        account = warehouseTypesFBAYWAZUS[0].WCode;
                    }
                    break;
                case "FBA-YWAZUS01":
                    //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                    List<WarehouseType> warehouseTypesFBAYWAZUS01 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us01'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWAZUS01.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWAZUS01[0].Id;
                        account = warehouseTypesFBAYWAZUS01[0].WCode;
                    }
                    break;
                case "FBA-YWAZUS02":
                    //FBA-YWAZUS02订单直接扣除库存FBA-YWAZUS02仓库
                    List<WarehouseType> warehouseTypesFBAYWAZUS02 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-us02'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWAZUS02.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWAZUS02[0].Id;
                        account = warehouseTypesFBAYWAZUS02[0].WCode;
                    }
                    break;
                case "FBA-YWAZUK":
                    //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                    List<WarehouseType> warehouseTypesFBAYWUK01 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-uk'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWUK01.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWUK01[0].Id;
                        account = warehouseTypesFBAYWUK01[0].WCode;
                    }
                    break;
                case "FBA-YWAZUK02":
                    //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                    List<WarehouseType> warehouseTypesFBAYWUK02 = NSession.CreateQuery("from WarehouseType where WCode='ywaz-uk02'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWUK02.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWUK02[0].Id;
                        account = warehouseTypesFBAYWUK02[0].WCode;
                    }
                    break;
                case "FBA-YWAZDE":
                    //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                    List<WarehouseType> warehouseTypesFBAYWAZDE = NSession.CreateQuery("from WarehouseType where WCode='ywaz-de'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWAZDE.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWAZDE[0].Id;
                        account = warehouseTypesFBAYWAZDE[0].WCode;
                    }
                    break;
                case "FBA-YWAZFR":
                    //FBA-YWAZUS01订单直接扣除库存FBA-YWAZUS01仓库
                    List<WarehouseType> warehouseTypesFBAYWAZFR = NSession.CreateQuery("from WarehouseType where WCode='ywaz-fr'").List<WarehouseType>().ToList();
                    if (warehouseTypesFBAYWAZFR.Count > 0)
                    {
                        warehouseId = warehouseTypesFBAYWAZFR[0].Id;
                        account = warehouseTypesFBAYWAZFR[0].WCode;
                    }
                    break;
                case "YWGCUS-West":
                    //谷仓美东（宁波）订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWGCUSWest = NSession.CreateQuery("from WarehouseType where WCode='YWGCUS-West'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWGCUSWest.Count > 0)
                    {
                        warehouseId = warehouseTypesYWGCUSWest[0].Id;
                        account = warehouseTypesYWGCUSWest[0].WCode;
                    }
                    break;
                case "YWGCUS-East":
                    //谷仓美东（宁波）订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWGCUSEast = NSession.CreateQuery("from WarehouseType where WCode='YWGCUS-East'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWGCUSEast.Count > 0)
                    {
                        warehouseId = warehouseTypesYWGCUSEast[0].Id;
                        account = warehouseTypesYWGCUSEast[0].WCode;
                    }
                    break;
                case "YWCA-WEST(DONG)":
                    //美西(董)（义乌）订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWCAWESTDONG = NSession.CreateQuery("from WarehouseType where WCode='YWCA-WEST(DONG)'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWCAWESTDONG.Count > 0)
                    {
                        warehouseId = warehouseTypesYWCAWESTDONG[0].Id;
                        account = warehouseTypesYWCAWESTDONG[0].WCode;
                    }
                    break;
                case "YWNJ-EAST(LEO)":
                    //美东(LEO)（义乌）订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWNJEASTLEO = NSession.CreateQuery("from WarehouseType where WCode='YWNJ-EAST(LEO)'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWNJEASTLEO.Count > 0)
                    {
                        warehouseId = warehouseTypesYWNJEASTLEO[0].Id;
                        account = warehouseTypesYWNJEASTLEO[0].WCode;
                    }
                    break;
                case "YWMRU-AEA":
                    //YWMRU-AEA订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWMRUAEA = NSession.CreateQuery("from WarehouseType where WCode='YWMRU-AEA'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWMRUAEA.Count > 0)
                    {
                        warehouseId = warehouseTypesYWMRUAEA[0].Id;
                        account = warehouseTypesYWMRUAEA[0].WCode;
                    }
                    break;
                case "YWMRU-AEB":
                    //YWMRU-AEB订单直接扣除库存
                    List<WarehouseType> warehouseTypesYWMRUAEB = NSession.CreateQuery("from WarehouseType where WCode='YWMRU-AEB'").List<WarehouseType>().ToList();
                    if (warehouseTypesYWMRUAEB.Count > 0)
                    {
                        warehouseId = warehouseTypesYWMRUAEB[0].Id;
                        account = warehouseTypesYWMRUAEB[0].WCode;
                    }
                    break;
                default:
                    break;
            }
            /////////////////////////////////////

            if (order.Enabled != 0)
            {
                bool flag = true;
                bool flag2 = false;
                if (order.IsOutOfStock == 1)
                {
                    flag2 = true;
                }
                if (!(order.Status == "已发货"))
                {
                    if (order.Products == null)
                    {
                        order.Products = NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>().ToList();
                    }
                    if (order.Products.Count > 1)
                    {
                        order.IsCanSplit = 1;
                    }
                    foreach (OrderProductType current in order.Products)
                    {
                        //if (current.IsQue != 3)
                        //{
                        current.IsQue = 0;
                        // 指定仓库，返回仓库内sku数量
                        //int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select SUM(Qty) from WarehouseStock where WId=" + warehouseId.ToString() + " and WarehouseStock.SKU='" + current.SKU + "' ").UniqueResult());
                        // 由固定库存数量调整为实时获取仓库明细累计
                        int num = System.Convert.ToInt32(NSession.CreateSQLQuery("select sum(NowQty) from WarehouseStockData  Where NowQty>0 and SKU='" + current.SKU + "' and WId=" + warehouseId.ToString() + "").UniqueResult());
                        //int num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from OrderProducts OP where OP.SKU='" + current.SKU + "' and OP.IsQue=3").UniqueResult());
                        //判断该仓库是不是有关联账户
                        bool IsHasAccount = Convert.ToInt32(NSession.CreateSQLQuery("select count(0) from Account,Warehouse where Account.AccountName=Warehouse.WCode and Warehouse.WName='" + f + "'").UniqueResult()) > 0;
                        int num2 = 0;
                        if (IsHasAccount)
                        {
                            // 指定帐号，返回帐号内sku占用库存数量
                            num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from OrderProducts OP join Orders O on op.OrderNo=O.OrderNo where OP.SKU='" + current.SKU + "' and OP.IsQue=3 and O.Account='" + account + "'").UniqueResult());

                        }
                        else
                        {
                            //// 非关联帐户指定该仓库查询占用库存
                            num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from Orders O join OrderProducts OP on O.OrderNo=OP.OrderNo join Account A on O.Account=A.AccountName where O.IsFBA=1 and O.FBABy='" + account + "' and OP.SKU='" + current.SKU + "' and OP.IsQue=3").UniqueResult());
                            // 说明：IsFBA=0 计算非海外仓订单商品占用库存数量，此处不加该条件：原因有走俄罗斯仓库的订单没有与订单关联

                            //// 宁波，义乌，小包，海外仓订单占用数量都合在一起此处有问题,且俄罗斯仓库的订单没有与订单关联20161012待修正 
                            //num2 = System.Convert.ToInt32(NSession.CreateSQLQuery("select isnull(sum(OP.Qty),0) from OrderProducts OP join Orders O on op.OrderNo=O.OrderNo where OP.SKU='" + current.SKU + "' and OP.IsQue=3 ").UniqueResult());

                        }
                        if (current.Qty > 1)
                        {
                            order.IsCanSplit = 1;
                        }
                        // 仓库数量-帐号占用库存数量<当前订单商品数量
                        if (num - num2 < current.Qty)
                        {
                            flag = false;
                            current.IsQue = 1;
                            order.IsOutOfStock = 1;
                        }
                        else
                        {
                            current.IsQue = 3;
                            //if (order.IsFBA == 1)
                            //{
                            // order.FBABy = account; // 记录第三仓库
                            //}
                        }
                        NSession.SaveOrUpdate(current);
                        NSession.Flush();
                        //}
                    }
                    if (flag)
                    {
                        order.IsOutOfStock = 0;
                        foreach (OrderProductType current in order.Products)
                        {
                            current.IsQue = 3;
                            NSession.SaveOrUpdate(current);
                            NSession.Flush();
                        }
                    }
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                    if (!flag2 && order.IsOutOfStock == 1)
                    {
                        LoggerUtil.GetOrderRecord(order, "验证订单", "订单中有产品缺货,自动设置为缺货订单。", NSession);
                    }
                    else
                    {
                        if (flag2 && order.IsOutOfStock == 0)
                        {
                            LoggerUtil.GetOrderRecord(order, "系统自动检验订单", "检验到产品有库存，缺货自动状态删除。", NSession);
                        }
                    }
                }
                else
                {
                    //订单触发利润更新（已发货）
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                }
            }

        }
        public static void UpdateAmount(OrderType order, ISession NSession)
        {
            OrderAmountType orderAmount = null;
            if (order.MId > 0)
            {
                IList<OrderAmountType> l =
                    NSession.CreateQuery("from OrderAmountType where OId=" + order.MId).SetMaxResults(1).List
                        <OrderAmountType>();
                if (l.Count > 0)
                    orderAmount = l[0];
            }
            else
            {
                IList<OrderAmountType> l = NSession.CreateQuery("from OrderAmountType where OId=" + order.Id).SetMaxResults(1).List<OrderAmountType>();
                if (l.Count > 0)
                    orderAmount = l[0];
            }
            if (orderAmount != null)
            {
                orderAmount.TotalFreight += order.Freight;
                object obj =
                       NSession.CreateQuery("select count(Id) from OrderType where Status <> '已发货' and (MId=" + order.Id + " or Id=" + order.Id + ")").UniqueResult();

                if (Convert.ToInt32(obj) > 0)
                {
                    orderAmount.Status = "未发货";
                }
                else
                {
                    orderAmount.Status = "已发货";
                }
                orderAmount.ScanningOn = order.ScanningOn;
                orderAmount.Profit = Math.Round(orderAmount.Profit - order.Freight, 5);
                LoggerUtil.GetOrderRecord(order, "财务信息计算[OrderHelpercs,UpdateAmount]", String.Format("{0}", orderAmount.Profit), NSession);
                orderAmount.CreateOn = order.CreateOn;
                orderAmount.UpdateOn = DateTime.Now;
                NSession.Update(orderAmount);
                NSession.Flush();
            }


        }

        public static void SaveAmount(OrderType order, ISession NSession)
        {
            List<CurrencyType> currencys = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            SaveAmount(order, currencys, NSession);
        }

        public static void SaveAmount(OrderType order, List<CurrencyType> currencys, ISession NSession)
        {
            NSession.Delete("from OrderAmountType where OId=" + order.Id);
            NSession.Flush();
            CurrencyType currency = currencys.Find(p => p.CurrencyCode.ToUpper() == order.CurrencyCode.ToUpper());

            if (order.Status == "待处理")
                order.Status = OrderStatusEnum.已处理.ToString();
            order.RMB = Math.Round(Convert.ToDouble(currency.CurrencyValue) * order.Amount, 4);
            OrderAmountType orderAmount = new OrderAmountType();
            orderAmount.Account = order.Account;
            orderAmount.OrderNo = order.OrderNo;
            orderAmount.OrderExNo = order.OrderExNo;
            orderAmount.OrderAmount = order.Amount;
            orderAmount.OId = order.Id;
            order.MId = order.MId;
            orderAmount.IsRepeat = order.IsRepeat;
            orderAmount.IsSplit = order.IsSplit;
            orderAmount.ExchangeRate = Convert.ToDouble(currency.CurrencyValue);
            //总成本确认
            object obj = NSession.CreateSQLQuery(
                "select SUM(OP.Qty*p.Price) from OrderProducts OP left join Products  P On OP.SKU=p.SKU where OId=" + order.Id).
                UniqueResult();
            orderAmount.TotalCosts = Convert.ToDouble(obj);

            IList<AccountFeeType> list = NSession.CreateQuery(string.Format("from AccountFeeType where AccountId in (select Id from AccountType where AccountName='{0}' ) and AmountBegin<={1} and AmountEnd>{1} ", order.Account, order.Amount)).List<AccountFeeType>();

            foreach (var feeType in list)
            {

                object d = new DataTable().Compute(feeType.FeeFormula.Replace("T", order.Amount.ToString()), "");
                if (feeType.FeeName == "交易费")
                {
                    if (order.OrderFees > 0)
                    {
                        orderAmount.TransactionFees = order.OrderFees;
                    }
                    else
                    {
                        orderAmount.TransactionFees = Convert.ToDouble(d);
                    }

                }
                if (feeType.FeeName == "手续费")
                {
                    orderAmount.Fee = Convert.ToDouble(d);
                }
                if (feeType.FeeName == "其他费")
                {
                    orderAmount.OtherFees = Convert.ToDouble(d);
                }
            }
            orderAmount.CreateOn = order.CreateOn;
            orderAmount.ScanningOn = order.ScanningOn;
            orderAmount.CurrencyCode = order.CurrencyCode;
            orderAmount.UpdateOn = DateTime.Now;

            orderAmount.Platform = order.Platform;
            orderAmount.Country = order.Country;
            orderAmount.RMB = order.RMB;
            orderAmount.Profit = Math.Round(order.RMB - orderAmount.TotalCosts - orderAmount.OtherFees * Convert.ToDouble(currency.CurrencyValue) - orderAmount.Fee * Convert.ToDouble(currency.CurrencyValue) -
                                 orderAmount.TransactionFees * Convert.ToDouble(currency.CurrencyValue), 5);
            LoggerUtil.GetOrderRecord(order, "财务信息计算[OrderHelpercs,SaveAmount]", String.Format("{0}", orderAmount.Profit), NSession);
            NSession.Save(orderAmount);
            NSession.Flush();
            if (orderAmount.IsRepeat == 1 && orderAmount.MId != 0)
            {
                NSession.CreateQuery("update OrderAmountType set SplitCount=SplitCount+1 where OId=" + orderAmount.MId).ExecuteUpdate();
                NSession.Flush();
            }
            if (orderAmount.IsSplit == 1 && orderAmount.MId != 0)
            {
                NSession.CreateQuery("update OrderAmountType set AgainCount=AgainCount+1 where OId=" + orderAmount.MId).ExecuteUpdate();
                NSession.Flush();
            }
            UpdateAmount(order, NSession);
        }
        #endregion

        #region 订单4项属性替换
        public static bool ReplaceBySKU(string ids, string newValue, ISession NSession)
        {

            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                ids = " and OId in(" + ids + ") ";
            }

            //sql = "update OrderProductType set SKU='{0}' where Id in(select Id from OrderType where Status='待处理')  {2}";
            sql = "update OrderProductType set SKU='{0}' where OId in(select Id from OrderType where Status='待处理')  {1}";
            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;

        }

        public static bool ReplaceByCountry(string ids, string newValue, ISession NSession)
        {

            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                ids = " and  Id in(" + ids + ")";

            }
            sql = "update OrderAddressType set Country='{0}',CountryCode='{0}' where 1=1 and Id in(select AddressId from OrderType where Status='待处理' {1})";
            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            Query.ExecuteUpdate();
            sql = "update OrderType set Country='{0}' where Status='待处理' {1}";
            sql = string.Format(sql, newValue, ids);
            Query = NSession.CreateQuery(sql);
            Query.ExecuteUpdate();

            return true;
        }

        public static bool ReplaceByCurrencyOrLogistic(string ids, string newValue, int type, ISession NSession)
        {

            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                ids = " and Id in(" + ids + ")  ";

            }

            if (type == 1)
                sql = "update OrderType set CurrencyCode='{0}' where  Status='待处理' {1}";
            else
                sql = "update OrderType set LogisticMode='{0}' where Status='待处理'  {1}";

            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;
        }
        #endregion
        #region 订单信息批量修改新的替换方法
        public static bool ReplaceBySKU(string type, string ids, string newValue, ISession NSession)
        {
            string sql = "update OrderProductType set SKU='{0}' where OId in(select Id from OrderType where {1} )  ";
            if (type == "OrderNo")
            {
                ids = " where  OrderNo in(" + ids + ") ";
                sql = "update OrderProductType set SKU='{0}' where OId in(select Id from OrderType {1})  ";

            }
            else if (type == "OrderExNo")
            {
                ids = "  OrderExNo in(" + ids + ") ";

            }
            else if (type == "TrackCode")
            {
                ids = "  TrackCode in(" + ids + ") ";
            }
            else
            {
                ids = "  TId in(" + ids + ") ";
            }

            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;

        }

        public static bool ReplaceByCountry(string type, string ids, string newValue, ISession NSession)
        {

            string sql = "";
            if (type == "OrderNo")
            {
                ids = " and OrderNo in(" + ids + ") ";
            }
            else if (type == "OrderExNo")
            {
                ids = " and OrderExNo in(" + ids + ") ";
            }
            else if (type == "TrackCode")
            {
                ids = " and TrackCode in(" + ids + ") ";
            }
            else
            {
                ids = " and TId in(" + ids + ") ";
            }

            sql = "update OrderAddressType set Country='{0}',CountryCode='{0}' where  Id in(select AddressId from OrderType where 1=1 {1})";
            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            Query.ExecuteUpdate();
            sql = "update OrderType set Country='{0}' where 1=1 {1}";
            sql = string.Format(sql, newValue, ids);
            Query = NSession.CreateQuery(sql);
            Query.ExecuteUpdate();

            return true;
        }

        public static bool ReplaceByCurrencyOrLogistic(string strtype, string ids, string newValue, int type, ISession NSession)
        {

            string sql = "";
            if (strtype == "OrderNo")
            {
                ids = " and OrderNo in(" + ids + ") ";
            }
            else if (strtype == "OrderExNo")
            {
                ids = " and OrderExNo in(" + ids + ") ";
            }
            else if (strtype == "TrackCode")
            {
                ids = " and TrackCode in(" + ids + ") ";
            }
            else
            {
                ids = " and TId in(" + ids + ") ";
            }
            if (type == 1)
                sql = "update OrderType set CurrencyCode='{0}' where 1=1  {1}";
            else
                sql = "update OrderType set LogisticMode='{0}' where 1=1  {1}";

            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;
        }
        public static bool ReplaceOrderStatus(string strtype, string ids, string newValue, int type, ISession NSession)
        {
            string sql = "";
            if (strtype == "OrderNo")
            {
                ids = " and OrderNo in(" + ids + ") ";
            }
            else if (strtype == "OrderExNo")
            {
                ids = " and OrderExNo in(" + ids + ") ";
            }
            else if (strtype == "TrackCode")
            {
                ids = " and TrackCode in(" + ids + ") ";
            }
            else
            {
                ids = " and TId in(" + ids + ") ";
            }
            sql = "update OrderType set Status='{0}' where 1=1  {1}";
            sql = string.Format(sql, newValue, ids);
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;
        }
        #endregion
        /// <summary>
        /// 根据组合商品表拆分订单
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <param name="productComposes"></param>
        /// <param name="NSession"></param>
        public static void SplitProduct(OrderProductType orderProduct, List<ProductComposeType> productComposes, ISession NSession)
        {
            int qty = orderProduct.Qty;
            int id = orderProduct.Id;
            foreach (ProductComposeType productComposeType in productComposes)
            {
                orderProduct.SKU = productComposeType.SrcSKU;
                orderProduct.Qty = productComposeType.SrcQty * qty;
                orderProduct.Id = 0;
                NSession.Clear();
                NSession.Save(orderProduct);
                NSession.Flush();
            }
            NSession.Clear();
            NSession.Delete(" from OrderProductType where Id=" + id);
            NSession.Flush();
        }

        public static Dictionary<string, string> GetDic(string fileName)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            CsvReader csv = new CsvReader(fileName);
            List<string[]> list = csv.ReadAllRow();
            foreach (var item in list)
            {
                if (item.Length == 2)
                {
                    dic.Add(item[0].Trim(), item[1].Trim());
                }
            }
            return dic;
        }

        public static string DownHtml(string Url, Encoding myEncoding)
        {
            try
            {
                WebClient client = new WebClient();
                StreamReader readerOfStream = new StreamReader(client.OpenRead(Url), myEncoding);
                return readerOfStream.ReadToEnd(); ;
            }
            catch
            {
                return string.Empty;
            }
        }

        // 计算订单财务数据
        public static void ReckonFinance(OrderType type, ISession NSession)
        {
            try
            {
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == type.CurrencyCode);

                // 未导入运费时使用预计运费
                if (type.IsFreight == 0 && type.Freight == 0)
                {
                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, NSession, 0M));
                }

                if (type.Status == "已发货")
                {
                    // 实际发货成本
                    type.ProductFees = Convert.ToDouble(OrderHelper.GerProductAmount(type.OrderNo, NSession));

                    // 未出货金额为0时获取预算商品成本
                    if (type.ProductFees == 0)
                    {
                        type.ProductFees = Convert.ToDouble(OrderHelper.GerProductAmountExpect(type.OrderNo, NSession));
                    }
                }
                else
                {
                    // 预算商品成本
                    type.ProductFees = Convert.ToDouble(OrderHelper.GerProductAmountExpect(type.OrderNo, NSession));
                    //预计重量
                    //如果有重量则不再需要取预估重量的值
                    type.Weight = type.Weight == 0 ? Convert.ToInt32(OrderHelper.GerProductWeightExpect(type.OrderNo, NSession)) : type.Weight;
                }

                if (type.FanState == 1)
                {
                    // 已收汇
                    type.Profit = Convert.ToDouble(Convert.ToDecimal(type.FanAmount) * Convert.ToDecimal(currencyType.CurrencyValue)) - type.ProductFees - type.Freight;

                    LoggerUtil.GetOrderRecord(type, "财务信息计算[实际]", String.Format("{0}*{1}-{2}-{3}={4}", type.FanAmount, currencyType.CurrencyValue, type.ProductFees, type.Freight, type.Profit), NSession);
                }
                else
                {
                    // 未收汇
                    type.Profit = Convert.ToDouble(Convert.ToDecimal(type.Amount) * Convert.ToDecimal(currencyType.CurrencyValue)) - type.ProductFees - type.Freight;
                    LoggerUtil.GetOrderRecord(type, "财务信息计算[预算]", String.Format("{0}*{1}-{2}-{3}={4}", type.Amount, currencyType.CurrencyValue, type.ProductFees, type.Freight, type.Profit), NSession);
                }
            }
            catch (Exception exc)
            {
                //Comm.LogInfo.WriteLog(String.Format("财务计算错误:{0},时间:{1}", exc.Message, DateTime.Now));
            }
            // 预计金额:Amount; 收汇金额:FanAmount
        }

        // 获取运费
        public static decimal GetFreight(double weight, string logisticMode, String countryCode, ISession NSession, decimal discount = 0)
        {

            IList<CountryType> c = NSession.CreateQuery("from CountryType where CCountry=:p1 or ECountry=:p1").SetString("p1", countryCode).List<CountryType>();
            if (c.Count > 0)
                return GetFreight(weight, logisticMode, c[0].Id, NSession, discount);
            else
            {
                return -1;//-1为国家错误
            }

        }
        // 获取商品预计金额合计
        public static decimal GerProductAmountExpect(string OrderNo, ISession NSession)
        {
            decimal count = 0;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    count += Convert.ToDecimal(product[0].Price) * Convert.ToDecimal(obj.Qty);
                }
            }
            //decimal count = List.Sum(x => x.Price);
            return count;
        }

        // 获取商品重量合计
        public static decimal GerProductWeightExpect(string OrderNo, ISession NSession)
        {
            decimal count = 0;
            IList<OrderProductType> List = NSession.CreateQuery("from OrderProductType where OrderNo=:p1").SetString("p1", OrderNo).List<OrderProductType>();
            foreach (OrderProductType obj in List)
            {
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku=:p1").SetString("p1", obj.SKU).List<ProductType>();
                if (product.Count > 0)
                {
                    count += Convert.ToDecimal(product[0].Weight) * Convert.ToDecimal(obj.Qty);
                }
            }
            return count;
        }

        // 获取商品发货金额合计
        public static decimal GerProductAmount(string OrderNo, ISession NSession)
        {
            IList<StockOutType> List = NSession.CreateQuery("from StockOutType where Enabled=0 and OrderNo=:p1").SetString("p1", OrderNo).List<StockOutType>();
            decimal amount = List.Sum(x => x.Price * x.Qty);
            return amount;
        }

        public static decimal GetFreight(double weight, string logisticMode, int country, ISession NSession, decimal discount = 0)
        {
            decimal ReturnFreight = 0;
            IList<LogisticsModeType> logmode = NSession.CreateQuery("from LogisticsModeType where LogisticsCode='" + logisticMode + "'").List<LogisticsModeType>();
            foreach (var item in logmode)
            {
                if (discount == 0)
                    discount = decimal.Parse(item.Discount.ToString());
                IList<LogisticsAreaType> areas = NSession.CreateQuery("from LogisticsAreaType where LId='" + item.ParentID + "'").List<LogisticsAreaType>();
                List<LogisticsAreaCountryType> AreaCountrys = NSession.CreateQuery("from LogisticsAreaCountryType where CountryCode='" + country + "' ").List<LogisticsAreaCountryType>().ToList();
                foreach (var foo in areas)
                {
                    LogisticsAreaCountryType tt = AreaCountrys.Find(x => x.AreaCode == foo.Id);
                    if (tt != null)
                    {
                        List<LogisticsFreightType> list = NSession.CreateQuery("from LogisticsFreightType where AreaCode=" + tt.AreaCode).List<LogisticsFreightType>().ToList();
                        LogisticsFreightType logisticsFreight =
                            list.Find(x => x.BeginWeight <= weight && x.EndWeight > weight);
                        if (logisticsFreight != null)
                        {
                            if (logisticsFreight.EveryFee != 0)
                            {
                                if (logisticsFreight.IsDiscountALL == 1)
                                {
                                    ReturnFreight = (Convert.ToDecimal(weight) * decimal.Parse(logisticsFreight.EveryFee.ToString()) + decimal.Parse(logisticsFreight.ProcessingFee.ToString())) * decimal.Parse(discount.ToString());
                                }
                                else
                                {
                                    ReturnFreight = Convert.ToDecimal(weight) * decimal.Parse(logisticsFreight.EveryFee.ToString()) * decimal.Parse(discount.ToString()) + decimal.Parse(logisticsFreight.ProcessingFee.ToString());
                                }

                            }
                            else
                            {
                                if (logisticsFreight.IsDiscountALL == 1)
                                {
                                    ReturnFreight = (decimal.Parse(logisticsFreight.FristFreight.ToString()) + decimal.Parse(logisticsFreight.ProcessingFee.ToString()) + Math.Ceiling((Convert.ToDecimal(weight) - decimal.Parse(logisticsFreight.FristWeight.ToString())) / decimal.Parse(logisticsFreight.IncrementWeight.ToString())) * decimal.Parse(logisticsFreight.IncrementFreight.ToString())) * decimal.Parse(discount.ToString());
                                }
                                else
                                {
                                    ReturnFreight = (decimal.Parse(logisticsFreight.FristFreight.ToString()) +
                                                     (Convert.ToDecimal(weight) -
                                                      decimal.Parse(logisticsFreight.FristWeight.ToString())) /
                                                     decimal.Parse(logisticsFreight.IncrementWeight.ToString()) *
                                                     decimal.Parse(logisticsFreight.IncrementFreight.ToString())) *
                                                    decimal.Parse(discount.ToString()) +
                                                    decimal.Parse(logisticsFreight.ProcessingFee.ToString());
                                }

                            }
                        }
                    }
                }
            }
            return ReturnFreight;
        }

        public static void GetOrderRecord(OrderType order, string recordType, string Content, string CreateBy, ISession NSession)
        {
            GetOrderRecord(order.Id, order.OrderNo, recordType, Content, CreateBy, NSession);

        }

        public static void SplitProduct(IList<OrderProductType> orders, ISession NSession)
        {
            IList<ProductComposeType> products = NSession.CreateQuery("from ProductComposeType").List<ProductComposeType>();

            foreach (OrderProductType orderProductType in orders)
            {
                List<ProductComposeType> compose =
                    products.Where(x => x.SKU.Trim().ToUpper() == orderProductType.SKU.Trim().ToUpper()).ToList();
                if (compose.Count > 0)
                    OrderHelper.SplitProduct(orderProductType, compose, NSession);
            }
        }
        public static void GetOrderRecord(int id, string orderNo, string recordType, string Content, string CreateBy, ISession NSession)
        {
            OrderRecordType orderRecord = new OrderRecordType();
            orderRecord.OId = id;
            orderRecord.OrderNo = orderNo;
            orderRecord.RecordType = recordType;
            orderRecord.CreateBy = CreateBy;
            orderRecord.Content = Content;
            orderRecord.CreateOn = DateTime.Now;
            NSession.Save(orderRecord);
            NSession.Flush();
        }

        /// <summary>
        ///  上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        public static void UploadTrackCode(OrderType o, ISession nSession)
        {

            // ebay 暂时取消上传跟踪码
            //// Ebay
            //if (((o.Platform == PlatformEnum.Ebay.ToString()) && (o.TrackCode != null)) && !o.TrackCode.StartsWith("LK"))
            //{
            //    EBayUtil.EbayUploadTrackCode(o.Account, o);
            //}
            // ebay 暂时取消上传跟踪码

            // Ebay义乌上传
            if (((o.Platform == PlatformEnum.Ebay.ToString()) && (o.TrackCode != null)) && (o.Status == "已发货") && (o.Account.IndexOf("yw") != -1) && (o.IsFBA == 1))
            {
                EBayUtil.EbayUploadTrackCode(o.Account, o);
            }
            // Wish
            if (o.Platform == PlatformEnum.Wish.ToString())
            {
                IList<logisticsSetupType> list = nSession.CreateQuery("from  logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='Wish'").List<logisticsSetupType>();
                if (list.Count > 0)
                {
                    IList<AccountType> list2 = nSession.CreateQuery("from AccountType where AccountName='" + o.Account + "'").SetMaxResults(1).List<AccountType>();
                    if (list2.Count > 0)
                    {
                        list2[0].ApiTokenInfo = WishUtil.RefreshToken(list2[0]);
                        WishUtil.UploadTrackNo(list2[0].ApiTokenInfo, o.OrderExNo, list[0].SetupName, o.TrackCode);
                    }
                }
            }
            // Aliexpress
            if (o.Platform == PlatformEnum.Aliexpress.ToString())
            {
                string serviceName = "";
                IList<logisticsSetupType> list = nSession.CreateQuery("from  logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='SMT'").List<logisticsSetupType>();
                if (list.Count > 0)
                {
                    serviceName = list[0].SetupName;
                }
                else
                {
                    serviceName = "CPAM";
                }
                IList<AccountType> list2 = nSession.CreateQuery("from AccountType where AccountName='" + o.Account + "'").SetMaxResults(1).List<AccountType>();
                if (list2.Count > 0)
                {
                    AccountType account = list2[0];
                    if (string.IsNullOrEmpty(account.ApiTokenInfo))
                    {
                        account.ApiTokenInfo = AliUtil.RefreshToken(account);
                        nSession.Save(account);
                        nSession.Flush();
                    }
                    if (AliUtil.sellerShipment(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, o.OrderExNo.Trim(), o.TrackCode, serviceName, true).IndexOf("Request need user authorized") != -1)
                    {
                        account.ApiTokenInfo = AliUtil.RefreshToken(account);
                        nSession.Save(account);
                        nSession.Flush();
                        AliUtil.sellerShipment(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, o.OrderExNo, o.TrackCode, serviceName, true);
                    }
                }
            }
        }



        /// <summary>
        ///  宁波Ebay上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        public static void NBEbayUpTrackCode(OrderType o, ISession nSession)
        {
            IList<logisticsSetupType> list = nSession.CreateQuery("from  logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='Ebay'").List<logisticsSetupType>();//Ebay平台需添加二级承运商
            if (list != null && list.Count > 0)
            {
                EBayUtil.EbayUploadTrackCode1(o.Account, o, list[0].SetupName);
            }


        }
        /// <summary>
        ///  宁波Amazon上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        public static string NBAmazonUpTrackCode(OrderType o, ISession NSession)
        {
            string UploadResult = "";
            List<ResultInfo> results = new List<ResultInfo>();
            IList<AccountType> accounts = NSession.CreateQuery("from AccountType where AccountName='" + o.Account + "'").SetMaxResults(1).
                   List<AccountType>();
            if (accounts.Count > 0)
            {
                // Developer AWS access key
                string accessKey = accounts[0].ApiKey;//"AKIAIFEICDUPSGL36SWA";

                // Developer AWS secret key
                string secretKey = accounts[0].ApiSecret;//"jRMN9OpFS5vFAETyTPzvidxwRd32+SemZWM5NAgX";

                // The client application name
                string appName = "bestore";

                // The client application version
                string appVersion = "1.0";

                // The endpoint for region service and version (see developer guide)
                // ex: https://mws.amazonservices.com
                string serviceURL = accounts[0].Email;


                FBAInboundServiceMWSConfig config = new FBAInboundServiceMWSConfig();
                config.ServiceURL = serviceURL;
                // Set other client connection configurations here if needed
                // Create the client itself
                FBAInboundServiceMWSClient client = new FBAInboundServiceMWSClient(accessKey, secretKey, appName, appVersion, config);


                try
                {
                    PutTransportContentRequest request = new PutTransportContentRequest();
                    CreateInboundShipmentPlanRequest crequest = new CreateInboundShipmentPlanRequest();
                    FBAInboundServiceMWS.Model.Address ShipFromAddress = new FBAInboundServiceMWS.Model.Address();
                    o.AddressInfo = NSession.Get<OrderAddressType>(o.AddressId);
                    ShipFromAddress.Name = o.AddressInfo.Addressee;
                    ShipFromAddress.AddressLine1 = o.AddressInfo.Street;
                    ShipFromAddress.City = o.AddressInfo.City;
                    ShipFromAddress.CountryCode = o.AddressInfo.CountryCode;
                    ShipFromAddress.PostalCode = o.AddressInfo.PostCode;
                    ShipFromAddress.StateOrProvinceCode = o.AddressInfo.Province;
                    crequest.ShipFromAddress = ShipFromAddress;
                    InboundShipmentPlanRequestItemList listitem = new InboundShipmentPlanRequestItemList();
                    o.Products = NSession.CreateQuery("from OrderProductType where OId='" + o.Id + "'").List<OrderProductType>().ToList();
                    foreach (OrderProductType orderpdt in o.Products)
                    {
                        InboundShipmentPlanRequestItem item = new InboundShipmentPlanRequestItem();
                        List<SKUItemType> sku = NSession.CreateQuery(" from SKUItemType where sku=:p and Account=:q").SetString("p", orderpdt.SKU).SetString("q", accounts[0].AccountName).List<SKUItemType>().ToList();
                        if (sku.Count > 0)
                        {
                            item.SellerSKU = sku[0].SKUEx;
                        }
                        item.Quantity = orderpdt.Qty;
                        listitem.member.Add(item);
                    }
                    if (listitem != null)
                    {
                        crequest.InboundShipmentPlanRequestItems = listitem;

                        crequest.SellerId = accounts[0].ApiToken;
                        //创建入库货件所需的信息
                        CreateInboundShipmentPlanResponse responseplan = client.CreateInboundShipmentPlan(crequest);
                        List<InboundShipmentPlan> plan = responseplan.CreateInboundShipmentPlanResult.InboundShipmentPlans.member;
                        request.ShipmentId = plan[0].ShipmentId;
                        request.IsPartnered = false;
                        request.ShipmentType = "SP";
                        request.SellerId = accounts[0].ApiToken;
                        NonPartneredSmallParcelDataInput pacrcelinput = new NonPartneredSmallParcelDataInput();
                        NonPartneredSmallParcelPackageInputList packageinputlist = new NonPartneredSmallParcelPackageInputList();
                        NonPartneredSmallParcelPackageInput packageinput = new NonPartneredSmallParcelPackageInput();
                        packageinput.TrackingId = o.TrackCode;
                        //获取运输方式
                        IList<logisticsSetupType> list = NSession.CreateQuery("from logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='Amazon'").List<logisticsSetupType>();//Ebay平台需添加二级承运商
                        if (list != null && list.Count > 0)
                        {
                            pacrcelinput.CarrierName = list[0].SetupName;
                            pacrcelinput.PackageList = packageinputlist;
                            packageinputlist.member.Add(packageinput);

                            TransportDetailInput i = new TransportDetailInput();
                            i.NonPartneredSmallParcelData = pacrcelinput;
                            request.TransportDetails = i;
                            //向亚马逊发送入库货件的运输信息。
                            PutTransportContentResponse r = client.PutTransportContent(request);
                            UploadResult = r.PutTransportContentResult.TransportResult.TransportStatus;
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }

            return UploadResult;


        }
    }
    #region

    public class Order
    {
        public Order() { }
        public Order(GoodsDataWare goodsDataWare, List<GoodsDataOrder> goodsDataOrderList)
        {
            this.GoodsDataOrderList = goodsDataOrderList;
            this.GoodsDataWare = goodsDataWare;
        }

        private GoodsDataWare goodsDataWare = new GoodsDataWare();
        public GoodsDataWare GoodsDataWare
        {
            get { return goodsDataWare; }
            set { goodsDataWare = value; }
        }
        private List<GoodsDataOrder> goodsDataOrderList = new List<GoodsDataOrder>();

        public List<GoodsDataOrder> GoodsDataOrderList
        {
            get { return goodsDataOrderList; }
            set { goodsDataOrderList = value; }
        }
    }

    public class GoodsDataWare
    {

        /// <summary>
        /// 商品编号
        /// </summary>
        private string _id = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        private string _errinfo = string.Empty;

        /// <summary>
        /// TotalDiscount
        /// </summary>
        private decimal _totaldiscount = new System.Decimal();

        /// <summary>
        /// TotalCost
        /// </summary>
        private decimal _totalcost = new System.Decimal();

        /// <summary>
        /// 数量
        /// </summary>
        private int _quantity = new System.Int32();

        /// <summary>
        /// 日期
        /// </summary>
        private System.DateTime _updatetime = System.Convert.ToDateTime("1800-1-1");

        /// <summary>
        /// EMS
        /// </summary>
        private string _ems = string.Empty;

        /// <summary>
        /// 是否已发货
        /// </summary>
        private bool _issend = new System.Boolean();

        /// <summary>
        /// 发货日期
        /// </summary>
        private System.DateTime _senddatatime = new System.DateTime();

        /// <summary>
        /// 保护资格
        /// </summary>
        private string _protectioneligibility = string.Empty;

        /// <summary>
        /// 地址状态
        /// </summary>
        private string _addressstatus = string.Empty;

        /// <summary>
        /// 付款人标识
        /// </summary>
        private string _payerid = string.Empty;

        /// <summary>
        /// 地址街
        /// </summary>
        private string _addressstreet = string.Empty;

        /// <summary>
        /// 付款状态
        /// </summary>
        private string _paymentstatus = string.Empty;

        /// <summary>
        /// 字符集
        /// </summary>
        private string _charset = string.Empty;

        /// <summary>
        /// 地址邮编
        /// </summary>
        private string _addresszip = string.Empty;

        /// <summary>
        /// 名字
        /// </summary>
        private string _firstname = string.Empty;

        /// <summary>
        /// 地址国家代码
        /// </summary>
        private string _addresscountrycode = string.Empty;

        /// <summary>
        /// 地址名称
        /// </summary>
        private string _addressname = string.Empty;

        /// <summary>
        /// 自定义
        /// </summary>
        private string _custom = string.Empty;

        /// <summary>
        /// 付款人状态
        /// </summary>
        private string _payerstatus = string.Empty;

        /// <summary>
        /// 商务
        /// </summary>
        private string _business = string.Empty;

        /// <summary>
        /// 地址国家
        /// </summary>
        private string _addresscountry = string.Empty;

        /// <summary>
        /// 地址城市
        /// </summary>
        private string _addresscity = string.Empty;

        /// <summary>
        /// 验证
        /// </summary>
        private string _verifysign = string.Empty;

        /// <summary>
        /// 付款人电邮
        /// </summary>
        private string _payeremail = string.Empty;

        /// <summary>
        /// 事务处理标识
        /// </summary>
        private string _txnid = string.Empty;

        /// <summary>
        /// 付款方式
        /// </summary>
        private string _paymenttype = string.Empty;

        /// <summary>
        /// 付款人的营业名称
        /// </summary>
        private string _payerbusinessname = string.Empty;

        /// <summary>
        /// 姓
        /// </summary>
        private string _lastname = string.Empty;

        /// <summary>
        /// 地址状态
        /// </summary>
        private string _addressstate = string.Empty;

        /// <summary>
        /// 接收电子邮件
        /// </summary>
        private string _receiveremail = string.Empty;

        /// <summary>
        /// 接收器标识
        /// </summary>
        private string _receiverid = string.Empty;

        /// <summary>
        /// 事务处理类型
        /// </summary>
        private string _txntype = string.Empty;

        /// <summary>
        /// 项目名称
        /// </summary>
        private string _itemname = string.Empty;

        /// <summary>
        /// 货币
        /// </summary>
        private string _mccurrency = string.Empty;

        /// <summary>
        /// 项目编号
        /// </summary>
        private string _itemnumber = string.Empty;

        /// <summary>
        /// 居住国家
        /// </summary>
        private string _residencecountry = string.Empty;

        /// <summary>
        /// 交易标题
        /// </summary>
        private string _transactionsubject = string.Empty;

        /// <summary>
        /// NotifyVersion
        /// </summary>
        private string _notifyversion = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        private string _memo = string.Empty;

        /// <summary>
        /// 联系电话
        /// </summary>
        private string _contactphone = string.Empty;

        /// <summary>
        /// Options
        /// </summary>
        private string _options = string.Empty;

        /// <summary>
        /// ReasonCode
        /// </summary>
        private string _reasoncode = string.Empty;

        /// <summary>
        /// ParentTxnId
        /// </summary>
        private string _parenttxnid = string.Empty;

        /// <summary>
        /// McGross
        /// </summary>
        private decimal _mcgross = new System.Decimal();

        /// <summary>
        /// 税
        /// </summary>
        private decimal _tax = new System.Decimal();

        /// <summary>
        /// McFee
        /// </summary>
        private decimal _mcfee = new System.Decimal();

        /// <summary>
        /// 数量
        /// </summary>
        private int _mcquantity = new System.Int32();

        /// <summary>
        /// PaymentFee
        /// </summary>
        private decimal _paymentfee = new System.Decimal();

        /// <summary>
        /// HandlingAmount
        /// </summary>
        private decimal _handlingamount = new System.Decimal();

        /// <summary>
        /// PaymentGross
        /// </summary>
        private decimal _paymentgross = new System.Decimal();

        /// <summary>
        /// 航运
        /// </summary>
        private decimal _shipping = new System.Decimal();

        /// <summary>
        /// 03:07:29 Nov 02, 2010 PDT
        /// </summary>
        private System.DateTime _paymentdate = new System.DateTime();

        /// <summary>
        /// 类 GoodsDataWare 的默认构造函数
        /// </summary>
        public GoodsDataWare() { }

        /// <summary>
        /// 类GoodsDataWare 的构造函数
        /// </summary>
        /// <param name="ErrInfo">错误信息</param>
        /// <param name="TotalDiscount">TotalDiscount</param>
        /// <param name="TotalCost">TotalCost</param>
        /// <param name="Quantity">数量</param>
        /// <param name="UpdateTime">日期</param>
        /// <param name="EMS">EMS</param>
        /// <param name="IsSend">是否已发货</param>
        /// <param name="SendDataTime">发货日期</param>
        /// <param name="ProtectionEligibility">保护资格</param>
        /// <param name="AddressStatus">地址状态</param>
        /// <param name="PayerId">付款人标识</param>
        /// <param name="AddressStreet">地址街</param>
        /// <param name="PaymentStatus">付款状态</param>
        /// <param name="Charset">字符集</param>
        /// <param name="AddressZip">地址邮编</param>
        /// <param name="FirstName">名字</param>
        /// <param name="AddressCountryCode">地址国家代码</param>
        /// <param name="AddressName">地址名称</param>
        /// <param name="Custom">自定义</param>
        /// <param name="PayerStatus">付款人状态</param>
        /// <param name="Business">商务</param>
        /// <param name="AddressCountry">地址国家</param>
        /// <param name="AddressCity">地址城市</param>
        /// <param name="VerifySign">验证</param>
        /// <param name="PayerEmail">付款人电邮</param>
        /// <param name="TxnId">事务处理标识</param>
        /// <param name="PaymentType">付款方式</param>
        /// <param name="PayerBusinessName">付款人的营业名称</param>
        /// <param name="LastName">姓</param>
        /// <param name="AddressState">地址状态</param>
        /// <param name="ReceiverEmail">接收电子邮件</param>
        /// <param name="ReceiverId">接收器标识</param>
        /// <param name="TxnType">事务处理类型</param>
        /// <param name="ItemName">项目名称</param>
        /// <param name="McCurrency">货币</param>
        /// <param name="ItemNumber">项目编号</param>
        /// <param name="ResidenceCountry">居住国家</param>
        /// <param name="TransactionSubject">交易标题</param>
        /// <param name="NotifyVersion">NotifyVersion</param>
        /// <param name="Memo">备注</param>
        /// <param name="ContactPhone">联系电话</param>
        /// <param name="Options">Options</param>
        /// <param name="ReasonCode">ReasonCode</param>
        /// <param name="ParentTxnId">ParentTxnId</param>
        /// <param name="McGross">McGross</param>
        /// <param name="Tax">税</param>
        /// <param name="McFee">McFee</param>
        /// <param name="McQuantity">数量</param>
        /// <param name="PaymentFee">PaymentFee</param>
        /// <param name="HandlingAmount">HandlingAmount</param>
        /// <param name="PaymentGross">PaymentGross</param>
        /// <param name="Shipping">航运</param>
        /// <param name="PaymentDate">03:07:29 Nov 02, 2010 PDT</param>
        public GoodsDataWare(
                    string ErrInfo,
                    decimal TotalDiscount,
                    decimal TotalCost,
                    int Quantity,
                    System.DateTime UpdateTime,
                    string EMS,
                    bool IsSend,
                    System.DateTime SendDataTime,
                    string ProtectionEligibility,
                    string AddressStatus,
                    string PayerId,
                    string AddressStreet,
                    string PaymentStatus,
                    string Charset,
                    string AddressZip,
                    string FirstName,
                    string AddressCountryCode,
                    string AddressName,
                    string Custom,
                    string PayerStatus,
                    string Business,
                    string AddressCountry,
                    string AddressCity,
                    string VerifySign,
                    string PayerEmail,
                    string TxnId,
                    string PaymentType,
                    string PayerBusinessName,
                    string LastName,
                    string AddressState,
                    string ReceiverEmail,
                    string ReceiverId,
                    string TxnType,
                    string ItemName,
                    string McCurrency,
                    string ItemNumber,
                    string ResidenceCountry,
                    string TransactionSubject,
                    string NotifyVersion,
                    string Memo,
                    string ContactPhone,
                    string Options,
                    string ReasonCode,
                    string ParentTxnId,
                    decimal McGross,
                    decimal Tax,
                    decimal McFee,
                    int McQuantity,
                    decimal PaymentFee,
                    decimal HandlingAmount,
                    decimal PaymentGross,
                    decimal Shipping,
                    System.DateTime PaymentDate) :
            this()
        {
            this._errinfo = ErrInfo;
            this._totaldiscount = TotalDiscount;
            this._totalcost = TotalCost;
            this._quantity = Quantity;
            this._updatetime = UpdateTime;
            this._ems = EMS;
            this._issend = IsSend;
            this._senddatatime = SendDataTime;
            this._protectioneligibility = ProtectionEligibility;
            this._addressstatus = AddressStatus;
            this._payerid = PayerId;
            this._addressstreet = AddressStreet;
            this._paymentstatus = PaymentStatus;
            this._charset = Charset;
            this._addresszip = AddressZip;
            this._firstname = FirstName;
            this._addresscountrycode = AddressCountryCode;
            this._addressname = AddressName;
            this._custom = Custom;
            this._payerstatus = PayerStatus;
            this._business = Business;
            this._addresscountry = AddressCountry;
            this._addresscity = AddressCity;
            this._verifysign = VerifySign;
            this._payeremail = PayerEmail;
            this._txnid = TxnId;
            this._paymenttype = PaymentType;
            this._payerbusinessname = PayerBusinessName;
            this._lastname = LastName;
            this._addressstate = AddressState;
            this._receiveremail = ReceiverEmail;
            this._receiverid = ReceiverId;
            this._txntype = TxnType;
            this._itemname = ItemName;
            this._mccurrency = McCurrency;
            this._itemnumber = ItemNumber;
            this._residencecountry = ResidenceCountry;
            this._transactionsubject = TransactionSubject;
            this._notifyversion = NotifyVersion;
            this._memo = Memo;
            this._contactphone = ContactPhone;
            this._options = Options;
            this._reasoncode = ReasonCode;
            this._parenttxnid = ParentTxnId;
            this._mcgross = McGross;
            this._tax = Tax;
            this._mcfee = McFee;
            this._mcquantity = McQuantity;
            this._paymentfee = PaymentFee;
            this._handlingamount = HandlingAmount;
            this._paymentgross = PaymentGross;
            this._shipping = Shipping;
            this._paymentdate = PaymentDate;
        }

        /// <summary>
        /// 类GoodsDataWare 的构造函数
        /// </summary>
        /// <param name="Id">商品编号</param>
        /// <param name="ErrInfo">错误信息</param>
        /// <param name="TotalDiscount">TotalDiscount</param>
        /// <param name="TotalCost">TotalCost</param>
        /// <param name="Quantity">数量</param>
        /// <param name="UpdateTime">日期</param>
        /// <param name="EMS">EMS</param>
        /// <param name="IsSend">是否已发货</param>
        /// <param name="SendDataTime">发货日期</param>
        /// <param name="ProtectionEligibility">保护资格</param>
        /// <param name="AddressStatus">地址状态</param>
        /// <param name="PayerId">付款人标识</param>
        /// <param name="AddressStreet">地址街</param>
        /// <param name="PaymentStatus">付款状态</param>
        /// <param name="Charset">字符集</param>
        /// <param name="AddressZip">地址邮编</param>
        /// <param name="FirstName">名字</param>
        /// <param name="AddressCountryCode">地址国家代码</param>
        /// <param name="AddressName">地址名称</param>
        /// <param name="Custom">自定义</param>
        /// <param name="PayerStatus">付款人状态</param>
        /// <param name="Business">商务</param>
        /// <param name="AddressCountry">地址国家</param>
        /// <param name="AddressCity">地址城市</param>
        /// <param name="VerifySign">验证</param>
        /// <param name="PayerEmail">付款人电邮</param>
        /// <param name="TxnId">事务处理标识</param>
        /// <param name="PaymentType">付款方式</param>
        /// <param name="PayerBusinessName">付款人的营业名称</param>
        /// <param name="LastName">姓</param>
        /// <param name="AddressState">地址状态</param>
        /// <param name="ReceiverEmail">接收电子邮件</param>
        /// <param name="ReceiverId">接收器标识</param>
        /// <param name="TxnType">事务处理类型</param>
        /// <param name="ItemName">项目名称</param>
        /// <param name="McCurrency">货币</param>
        /// <param name="ItemNumber">项目编号</param>
        /// <param name="ResidenceCountry">居住国家</param>
        /// <param name="TransactionSubject">交易标题</param>
        /// <param name="NotifyVersion">NotifyVersion</param>
        /// <param name="Memo">备注</param>
        /// <param name="ContactPhone">联系电话</param>
        /// <param name="Options">Options</param>
        /// <param name="ReasonCode">ReasonCode</param>
        /// <param name="ParentTxnId">ParentTxnId</param>
        /// <param name="McGross">McGross</param>
        /// <param name="Tax">税</param>
        /// <param name="McFee">McFee</param>
        /// <param name="McQuantity">数量</param>
        /// <param name="PaymentFee">PaymentFee</param>
        /// <param name="HandlingAmount">HandlingAmount</param>
        /// <param name="PaymentGross">PaymentGross</param>
        /// <param name="Shipping">航运</param>
        /// <param name="PaymentDate">03:07:29 Nov 02, 2010 PDT</param>
        public GoodsDataWare(
                    string Id,
                    string ErrInfo,
                    decimal TotalDiscount,
                    decimal TotalCost,
                    int Quantity,
                    System.DateTime UpdateTime,
                    string EMS,
                    bool IsSend,
                    System.DateTime SendDataTime,
                    string ProtectionEligibility,
                    string AddressStatus,
                    string PayerId,
                    string AddressStreet,
                    string PaymentStatus,
                    string Charset,
                    string AddressZip,
                    string FirstName,
                    string AddressCountryCode,
                    string AddressName,
                    string Custom,
                    string PayerStatus,
                    string Business,
                    string AddressCountry,
                    string AddressCity,
                    string VerifySign,
                    string PayerEmail,
                    string TxnId,
                    string PaymentType,
                    string PayerBusinessName,
                    string LastName,
                    string AddressState,
                    string ReceiverEmail,
                    string ReceiverId,
                    string TxnType,
                    string ItemName,
                    string McCurrency,
                    string ItemNumber,
                    string ResidenceCountry,
                    string TransactionSubject,
                    string NotifyVersion,
                    string Memo,
                    string ContactPhone,
                    string Options,
                    string ReasonCode,
                    string ParentTxnId,
                    decimal McGross,
                    decimal Tax,
                    decimal McFee,
                    int McQuantity,
                    decimal PaymentFee,
                    decimal HandlingAmount,
                    decimal PaymentGross,
                    decimal Shipping,
                    System.DateTime PaymentDate) :
            this()
        {
            this._id = Id;
            this._errinfo = ErrInfo;
            this._totaldiscount = TotalDiscount;
            this._totalcost = TotalCost;
            this._quantity = Quantity;
            this._updatetime = UpdateTime;
            this._ems = EMS;
            this._issend = IsSend;
            this._senddatatime = SendDataTime;
            this._protectioneligibility = ProtectionEligibility;
            this._addressstatus = AddressStatus;
            this._payerid = PayerId;
            this._addressstreet = AddressStreet;
            this._paymentstatus = PaymentStatus;
            this._charset = Charset;
            this._addresszip = AddressZip;
            this._firstname = FirstName;
            this._addresscountrycode = AddressCountryCode;
            this._addressname = AddressName;
            this._custom = Custom;
            this._payerstatus = PayerStatus;
            this._business = Business;
            this._addresscountry = AddressCountry;
            this._addresscity = AddressCity;
            this._verifysign = VerifySign;
            this._payeremail = PayerEmail;
            this._txnid = TxnId;
            this._paymenttype = PaymentType;
            this._payerbusinessname = PayerBusinessName;
            this._lastname = LastName;
            this._addressstate = AddressState;
            this._receiveremail = ReceiverEmail;
            this._receiverid = ReceiverId;
            this._txntype = TxnType;
            this._itemname = ItemName;
            this._mccurrency = McCurrency;
            this._itemnumber = ItemNumber;
            this._residencecountry = ResidenceCountry;
            this._transactionsubject = TransactionSubject;
            this._notifyversion = NotifyVersion;
            this._memo = Memo;
            this._contactphone = ContactPhone;
            this._options = Options;
            this._reasoncode = ReasonCode;
            this._parenttxnid = ParentTxnId;
            this._mcgross = McGross;
            this._tax = Tax;
            this._mcfee = McFee;
            this._mcquantity = McQuantity;
            this._paymentfee = PaymentFee;
            this._handlingamount = HandlingAmount;
            this._paymentgross = PaymentGross;
            this._shipping = Shipping;
            this._paymentdate = PaymentDate;
        }

        /// <summary>
        /// 商品编号
        /// </summary>
        [System.ComponentModel.DataObjectFieldAttribute(true, true)]
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrInfo
        {
            get
            {
                return this._errinfo;
            }
            set
            {
                this._errinfo = value;
            }
        }

        /// <summary>
        /// TotalDiscount
        /// </summary>
        public decimal TotalDiscount
        {
            get
            {
                return this._totaldiscount;
            }
            set
            {
                this._totaldiscount = value;
            }
        }

        /// <summary>
        /// TotalCost
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                return this._totalcost;
            }
            set
            {
                this._totalcost = value;
            }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity
        {
            get
            {
                return this._quantity;
            }
            set
            {
                this._quantity = value;
            }
        }

        /// <summary>
        /// 日期
        /// </summary>
        public System.DateTime UpdateTime
        {
            get
            {
                return this._updatetime;
            }
            set
            {
                this._updatetime = value;
            }
        }

        /// <summary>
        /// EMS
        /// </summary>
        public string EMS
        {
            get
            {
                return this._ems;
            }
            set
            {
                this._ems = value;
            }
        }

        /// <summary>
        /// 是否已发货
        /// </summary>
        public bool IsSend
        {
            get
            {
                return this._issend;
            }
            set
            {
                this._issend = value;
            }
        }

        /// <summary>
        /// 发货日期
        /// </summary>
        public System.DateTime SendDataTime
        {
            get
            {
                return this._senddatatime;
            }
            set
            {
                this._senddatatime = value;
            }
        }

        /// <summary>
        /// 保护资格
        /// </summary>
        public string ProtectionEligibility
        {
            get
            {
                return this._protectioneligibility;
            }
            set
            {
                this._protectioneligibility = value;
            }
        }

        /// <summary>
        /// 地址状态
        /// </summary>
        public string AddressStatus
        {
            get
            {
                return this._addressstatus;
            }
            set
            {
                this._addressstatus = value;
            }
        }

        /// <summary>
        /// 付款人标识
        /// </summary>
        public string PayerId
        {
            get
            {
                return this._payerid;
            }
            set
            {
                this._payerid = value;
            }
        }

        /// <summary>
        /// 地址街
        /// </summary>
        public string AddressStreet
        {
            get
            {
                return this._addressstreet;
            }
            set
            {
                this._addressstreet = value;
            }
        }

        /// <summary>
        /// 付款状态
        /// </summary>
        public string PaymentStatus
        {
            get
            {
                return this._paymentstatus;
            }
            set
            {
                this._paymentstatus = value;
            }
        }

        /// <summary>
        /// 字符集
        /// </summary>
        public string Charset
        {
            get
            {
                return this._charset;
            }
            set
            {
                this._charset = value;
            }
        }

        /// <summary>
        /// 地址邮编
        /// </summary>
        public string AddressZip
        {
            get
            {
                return this._addresszip;
            }
            set
            {
                this._addresszip = value;
            }
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string FirstName
        {
            get
            {
                return this._firstname;
            }
            set
            {
                this._firstname = value;
            }
        }

        /// <summary>
        /// 地址国家代码
        /// </summary>
        public string AddressCountryCode
        {
            get
            {
                return this._addresscountrycode;
            }
            set
            {
                this._addresscountrycode = value;
            }
        }

        /// <summary>
        /// 地址名称
        /// </summary>
        public string AddressName
        {
            get
            {
                return this._addressname;
            }
            set
            {
                this._addressname = value;
            }
        }

        /// <summary>
        /// 自定义
        /// </summary>
        public string Custom
        {
            get
            {
                return this._custom;
            }
            set
            {
                this._custom = value;
            }
        }

        /// <summary>
        /// 付款人状态
        /// </summary>
        public string PayerStatus
        {
            get
            {
                return this._payerstatus;
            }
            set
            {
                this._payerstatus = value;
            }
        }

        /// <summary>
        /// 商务
        /// </summary>
        public string Business
        {
            get
            {
                return this._business;
            }
            set
            {
                this._business = value;
            }
        }

        /// <summary>
        /// 地址国家
        /// </summary>
        public string AddressCountry
        {
            get
            {
                return this._addresscountry;
            }
            set
            {
                this._addresscountry = value;
            }
        }

        /// <summary>
        /// 地址城市
        /// </summary>
        public string AddressCity
        {
            get
            {
                return this._addresscity;
            }
            set
            {
                this._addresscity = value;
            }
        }

        /// <summary>
        /// 验证
        /// </summary>
        public string VerifySign
        {
            get
            {
                return this._verifysign;
            }
            set
            {
                this._verifysign = value;
            }
        }

        /// <summary>
        /// 付款人电邮
        /// </summary>
        public string PayerEmail
        {
            get
            {
                return this._payeremail;
            }
            set
            {
                this._payeremail = value;
            }
        }

        /// <summary>
        /// 事务处理标识
        /// </summary>
        public string TxnId
        {
            get
            {
                return this._txnid;
            }
            set
            {
                this._txnid = value;
            }
        }

        /// <summary>
        /// 付款方式
        /// </summary>
        public string PaymentType
        {
            get
            {
                return this._paymenttype;
            }
            set
            {
                this._paymenttype = value;
            }
        }

        /// <summary>
        /// 付款人的营业名称
        /// </summary>
        public string PayerBusinessName
        {
            get
            {
                return this._payerbusinessname;
            }
            set
            {
                this._payerbusinessname = value;
            }
        }

        /// <summary>
        /// 姓
        /// </summary>
        public string LastName
        {
            get
            {
                return this._lastname;
            }
            set
            {
                this._lastname = value;
            }
        }

        /// <summary>
        /// 地址状态
        /// </summary>
        public string AddressState
        {
            get
            {
                return this._addressstate;
            }
            set
            {
                this._addressstate = value;
            }
        }

        /// <summary>
        /// 接收电子邮件
        /// </summary>
        public string ReceiverEmail
        {
            get
            {
                return this._receiveremail;
            }
            set
            {
                this._receiveremail = value;
            }
        }

        /// <summary>
        /// 接收器标识
        /// </summary>
        public string ReceiverId
        {
            get
            {
                return this._receiverid;
            }
            set
            {
                this._receiverid = value;
            }
        }

        /// <summary>
        /// 事务处理类型
        /// </summary>
        public string TxnType
        {
            get
            {
                return this._txntype;
            }
            set
            {
                this._txntype = value;
            }
        }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ItemName
        {
            get
            {
                return this._itemname;
            }
            set
            {
                this._itemname = value;
            }
        }

        /// <summary>
        /// 货币
        /// </summary>
        public string McCurrency
        {
            get
            {
                return this._mccurrency;
            }
            set
            {
                this._mccurrency = value;
            }
        }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string ItemNumber
        {
            get
            {
                return this._itemnumber;
            }
            set
            {
                this._itemnumber = value;
            }
        }

        /// <summary>
        /// 居住国家
        /// </summary>
        public string ResidenceCountry
        {
            get
            {
                return this._residencecountry;
            }
            set
            {
                this._residencecountry = value;
            }
        }

        /// <summary>
        /// 交易标题
        /// </summary>
        public string TransactionSubject
        {
            get
            {
                return this._transactionsubject;
            }
            set
            {
                this._transactionsubject = value;
            }
        }

        /// <summary>
        /// NotifyVersion
        /// </summary>
        public string NotifyVersion
        {
            get
            {
                return this._notifyversion;
            }
            set
            {
                this._notifyversion = value;
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Memo
        {
            get
            {
                return this._memo;
            }
            set
            {
                this._memo = value;
            }
        }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone
        {
            get
            {
                return this._contactphone;
            }
            set
            {
                this._contactphone = value;
            }
        }

        /// <summary>
        /// Options
        /// </summary>
        public string Options
        {
            get
            {
                return this._options;
            }
            set
            {
                this._options = value;
            }
        }

        /// <summary>
        /// ReasonCode
        /// </summary>
        public string ReasonCode
        {
            get
            {
                return this._reasoncode;
            }
            set
            {
                this._reasoncode = value;
            }
        }

        /// <summary>
        /// ParentTxnId
        /// </summary>
        public string ParentTxnId
        {
            get
            {
                return this._parenttxnid;
            }
            set
            {
                this._parenttxnid = value;
            }
        }

        /// <summary>
        /// McGross
        /// </summary>
        public decimal McGross
        {
            get
            {
                return this._mcgross;
            }
            set
            {
                this._mcgross = value;
            }
        }

        /// <summary>
        /// 税
        /// </summary>
        public decimal Tax
        {
            get
            {
                return this._tax;
            }
            set
            {
                this._tax = value;
            }
        }

        /// <summary>
        /// McFee
        /// </summary>
        public decimal McFee
        {
            get
            {
                return this._mcfee;
            }
            set
            {
                this._mcfee = value;
            }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int McQuantity
        {
            get
            {
                return this._mcquantity;
            }
            set
            {
                this._mcquantity = value;
            }
        }

        /// <summary>
        /// PaymentFee
        /// </summary>
        public decimal PaymentFee
        {
            get
            {
                return this._paymentfee;
            }
            set
            {
                this._paymentfee = value;
            }
        }

        /// <summary>
        /// HandlingAmount
        /// </summary>
        public decimal HandlingAmount
        {
            get
            {
                return this._handlingamount;
            }
            set
            {
                this._handlingamount = value;
            }
        }

        /// <summary>
        /// PaymentGross
        /// </summary>
        public decimal PaymentGross
        {
            get
            {
                return this._paymentgross;
            }
            set
            {
                this._paymentgross = value;
            }
        }

        /// <summary>
        /// 航运
        /// </summary>
        public decimal Shipping
        {
            get
            {
                return this._shipping;
            }
            set
            {
                this._shipping = value;
            }
        }

        /// <summary>
        /// 03:07:29 Nov 02, 2010 PDT
        /// </summary>
        public System.DateTime PaymentDate
        {
            get
            {
                return this._paymentdate;
            }
            set
            {
                this._paymentdate = value;
            }
        }


    }


    /// <summary>
    /// 库存管理的实体类
    /// </summary>

    public class GoodsDataOrder
    {

        /// <summary>
        /// 商品编号
        /// </summary>
        private string _id = string.Empty;

        /// <summary>
        /// PId
        /// </summary>
        private string _pid = string.Empty;

        /// <summary>
        /// 商品名称
        /// </summary>
        private string _title = string.Empty;

        /// <summary>
        /// 链接
        /// </summary>
        private string _url = string.Empty;

        /// <summary>
        /// 图片
        /// </summary>
        private string _pic = string.Empty;

        /// <summary>
        /// 商品条码
        /// </summary>
        private string _itemid = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        private int _quantity = new System.Int32();

        /// <summary>
        /// 折扣
        /// </summary>
        private decimal _discount = new System.Decimal();

        /// <summary>
        /// 总价
        /// </summary>
        private decimal _total = new System.Decimal();

        /// <summary>
        /// 单价
        /// </summary>
        private decimal _price = new System.Decimal();

        /// <summary>
        /// 类 GoodsDataOrder 的默认构造函数
        /// </summary>
        public GoodsDataOrder() { }

        /// <summary>
        /// 类GoodsDataOrder 的构造函数
        /// </summary>
        /// <param name="PId">PId</param>
        /// <param name="Title">商品名称</param>
        /// <param name="Url">链接</param>
        /// <param name="Pic">图片</param>
        /// <param name="ItemID">商品条码</param>
        /// <param name="Quantity">数量</param>
        /// <param name="Discount">折扣</param>
        /// <param name="Total">总价</param>
        /// <param name="Price">单价</param>
        public GoodsDataOrder(string PId, string Title, string Url, string Pic, string ItemID, int Quantity, decimal Discount, decimal Total, decimal Price) :
            this()
        {
            this._pid = PId;
            this._title = Title;
            this._url = Url;
            this._pic = Pic;
            this._itemid = ItemID;
            this._quantity = Quantity;
            this._discount = Discount;
            this._total = Total;
            this._price = Price;
        }

        /// <summary>
        /// 类GoodsDataOrder 的构造函数
        /// </summary>
        /// <param name="Id">商品编号</param>
        /// <param name="PId">PId</param>
        /// <param name="Title">商品名称</param>
        /// <param name="Url">链接</param>
        /// <param name="Pic">图片</param>
        /// <param name="ItemID">商品条码</param>
        /// <param name="Quantity">数量</param>
        /// <param name="Discount">折扣</param>
        /// <param name="Total">总价</param>
        /// <param name="Price">单价</param>
        public GoodsDataOrder(string Id, string PId, string Title, string Url, string Pic, string ItemID, int Quantity, decimal Discount, decimal Total, decimal Price) :
            this()
        {
            this._id = Id;
            this._pid = PId;
            this._title = Title;
            this._url = Url;
            this._pic = Pic;
            this._itemid = ItemID;
            this._quantity = Quantity;
            this._discount = Discount;
            this._total = Total;
            this._price = Price;
        }

        /// <summary>
        /// 商品编号
        /// </summary>
        [System.ComponentModel.DataObjectFieldAttribute(true, true)]
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// PId
        /// </summary>
        public string PId
        {
            get
            {
                return this._pid;
            }
            set
            {
                this._pid = value;
            }
        }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }

        /// <summary>
        /// 链接
        /// </summary>
        public string Url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }

        /// <summary>
        /// 图片
        /// </summary>
        public string Pic
        {
            get
            {
                return this._pic;
            }
            set
            {
                this._pic = value;
            }
        }

        /// <summary>
        /// 商品条码
        /// </summary>
        public string ItemID
        {
            get
            {
                return this._itemid;
            }
            set
            {
                this._itemid = value;
            }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity
        {
            get
            {
                return this._quantity;
            }
            set
            {
                this._quantity = value;
            }
        }

        /// <summary>
        /// 折扣
        /// </summary>
        public decimal Discount
        {
            get
            {
                return this._discount;
            }
            set
            {
                this._discount = value;
            }
        }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal Total
        {
            get
            {
                return this._total;
            }
            set
            {
                this._total = value;
            }
        }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price
        {
            get
            {
                //hrhw69kfrnyjx
                return this._price;
            }
            set
            {
                this._price = value;
            }
        }

    }
    #endregion
    #region
    public class MyEvaluator
    {
        #region 构造函数

        /// <summary>   
        /// 可执行串的构造函数   
        /// </summary>   
        /// <param name="items">   
        /// 可执行字符串数组   
        /// </param>   
        public MyEvaluator(EvaluatorItem[] items)
        {
            ConstructEvaluator(items);      //调用解析字符串构造函数进行解析   
        }

        /// <summary>   
        /// 可执行串的构造函数   
        /// </summary>   
        /// <param name="returnType">返回值类型</param>   
        /// <param name="expression">执行表达式</param>   
        /// <param name="name">执行字符串名称</param>   
        public MyEvaluator(Type returnType, string expression, string name)
        {
            //创建可执行字符串数组   
            EvaluatorItem[] items = { new EvaluatorItem(returnType, expression, name) };
            ConstructEvaluator(items);      //调用解析字符串构造函数进行解析   
        }

        /// <summary>   
        /// 可执行串的构造函数   
        /// </summary>   
        /// <param name="item">可执行字符串项</param>   
        public MyEvaluator(EvaluatorItem item)
        {
            EvaluatorItem[] items = { item };//将可执行字符串项转为可执行字符串项数组   
            ConstructEvaluator(items);      //调用解析字符串构造函数进行解析   
        }

        /// <summary>   
        /// 解析字符串构造函数   
        /// </summary>   
        /// <param name="items">待解析字符串数组</param>   
        private void ConstructEvaluator(EvaluatorItem[] items)
        {
            //创建C#编译器实例
            CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");

            //过时了
            //ICodeCompiler comp = provider.CreateCompiler();

            //编译器的传入参数   
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("system.dll");              //添加程序集 system.dll 的引用   
            cp.ReferencedAssemblies.Add("system.data.dll");         //添加程序集 system.data.dll 的引用   
            cp.ReferencedAssemblies.Add("system.xml.dll");          //添加程序集 system.xml.dll 的引用   
            cp.GenerateExecutable = false;                          //不生成可执行文件   
            cp.GenerateInMemory = true;                             //在内存中运行   

            StringBuilder code = new StringBuilder();               //创建代码串   
            /*  
             *  添加常见且必须的引用字符串  
             */
            code.Append("using System; ");
            code.Append("using System.Data; ");
            code.Append("using System.Data.SqlClient; ");
            code.Append("using System.Data.OleDb; ");
            code.Append("using System.Xml; ");

            code.Append("namespace SSEC.Math { ");                  //生成代码的命名空间为EvalGuy，和本代码一样   

            code.Append("  public class _Evaluator { ");          //产生 _Evaluator 类，所有可执行代码均在此类中运行   
            foreach (EvaluatorItem item in items)               //遍历每一个可执行字符串项   
            {
                code.AppendFormat("    public {0} {1}() ",          //添加定义公共函数代码   
                                  item.ReturnType.Name,             //函数返回值为可执行字符串项中定义的返回值类型   
                                  item.Name);                       //函数名称为可执行字符串项中定义的执行字符串名称   
                code.Append("{ ");                                  //添加函数开始括号   
                code.AppendFormat("return ({0});", item.Expression);//添加函数体，返回可执行字符串项中定义的表达式的值   
                code.Append("}");                                 //添加函数结束括号   
            }
            code.Append("} }");                                 //添加类结束和命名空间结束括号   

            //得到编译器实例的返回结果   
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, code.ToString());//comp

            if (cr.Errors.HasErrors)                            //如果有错误   
            {
                StringBuilder error = new StringBuilder();          //创建错误信息字符串   
                error.Append("编译有错误的表达式: ");                //添加错误文本   
                foreach (CompilerError err in cr.Errors)            //遍历每一个出现的编译错误   
                {
                    error.AppendFormat("{0}", err.ErrorText);     //添加进错误文本，每个错误后换行   
                }
                throw new Exception("编译错误: " + error.ToString());//抛出异常   
            }
            Assembly a = cr.CompiledAssembly;                       //获取编译器实例的程序集   
            _Compiled = a.CreateInstance("SSEC.Math._Evaluator");     //通过程序集查找并声明 SSEC.Math._Evaluator 的实例   
        }
        #endregion

        #region 公有成员

        /// <summary>   
        /// 执行字符串并返回布尔型值   
        /// </summary>   
        /// <param name="name">执行字符串名称</param>   
        /// <returns>执行结果</returns>   
        public bool EvaluateBool(string name)
        {
            return (bool)Evaluate(name);
        }
        /// <summary>   
        /// 执行字符串并返 object 型值   
        /// </summary>   
        /// <param name="name">执行字符串名称</param>   
        /// <returns>执行结果</returns>   
        public object Evaluate(string name)
        {
            MethodInfo mi = _Compiled.GetType().GetMethod(name);//获取 _Compiled 所属类型中名称为 name 的方法的引用   
            return mi.Invoke(_Compiled, null);                  //执行 mi 所引用的方法   
        }
        #endregion

        #region 静态成员

        /// <summary>   
        /// 执行表达式并返回布尔型值   
        /// </summary>   
        /// <param name="code">要执行的表达式</param>   
        /// <returns>运算结果</returns>   
        static public bool EvaluateToBool(string code)
        {
            MyEvaluator eval = new MyEvaluator(typeof(bool), code, staticMethodName);//生成 Evaluator 类的对像   
            return (bool)eval.Evaluate(staticMethodName);                       //执行并返回布尔型数据   
        }

        #endregion

        #region 私有成员
        /// <summary>   
        /// 静态方法的执行字符串名称   
        /// </summary>   
        private const string staticMethodName = "__foo";
        /// <summary>   
        /// 用于动态引用生成的类，执行其内部包含的可执行字符串   
        /// </summary>   
        object _Compiled = null;
        #endregion
    }


    /// <summary>   
    /// 可执行字符串项（即一条可执行字符串）   
    /// </summary>   
    public class EvaluatorItem
    {
        /// <summary>   
        /// 返回值类型   
        /// </summary>   
        public Type ReturnType;
        /// <summary>   
        /// 执行表达式   
        /// </summary>   
        public string Expression;
        /// <summary>   
        /// 执行字符串名称   
        /// </summary>   
        public string Name;
        /// <summary>   
        /// 可执行字符串项构造函数   
        /// </summary>   
        /// <param name="returnType">返回值类型</param>   
        /// <param name="expression">执行表达式</param>   
        /// <param name="name">执行字符串名称</param>   
        public EvaluatorItem(Type returnType, string expression, string name)
        {
            ReturnType = returnType;
            Expression = expression;
            Name = name;
        }
    }

    #endregion
}
