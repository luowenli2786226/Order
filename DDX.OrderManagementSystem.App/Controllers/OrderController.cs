using DDX.NHibernateHelper;
using DDX.OrderManagementSystem.App.Common.Utils;
using DDX.OrderManagementSystem.App.asp.ebay.shipping;
using DDX.OrderManagementSystem.App.cn.chukou1.demo;
using DDX.OrderManagementSystem.App.Common;
using DDX.OrderManagementSystem.Domain;
using eBay.Service.Core.Sdk;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.UI;
using OrderType = DDX.OrderManagementSystem.Domain.OrderType;
using PickUpAddress = DDX.OrderManagementSystem.App.asp.ebay.shipping.PickUpAddress;
using ShipToAddress = DDX.OrderManagementSystem.App.asp.ebay.shipping.ShipToAddress;
using DDX.OrderManagementSystem.App.Common.json;
using System.Web;
using DDX.OrderManagementSystem.App.com.bdt.post;


namespace DDX.OrderManagementSystem.App.Controllers
{
    [ValidateInput(false)]

    public class OrderController : BaseController
    {
        [HttpGet]
        public JsonResult AAAA()
        {
            // List<OrderType> list = base.NSession.CreateQuery("from OrderType where ScanningOn>'2015-11-12 08:00:00'  and Status='已发货' and Account like '%yw%'").List<OrderType>().ToList<OrderType>();

            // List<OrderType> list = base.NSession.CreateQuery("from OrderType where Status='已发货'  and Enabled=1 and ScanningOn>'2015-08-20 08:00:00' ").List<OrderType>().ToList<OrderType>();
            //List<OrderType> list = base.NSession.CreateQuery("from OrderType where   ScanningOn>'2015-11-12 08:00:00'  and  ScanningOn<'2015-11-14 08:00:00' and Status='已发货' and LogisticMode='欧亚速运' ").List<OrderType>().ToList<OrderType>();
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderExNo in ('56596a91db7ec844f72ea404 ')").List<OrderType>().ToList<OrderType>();
            foreach (OrderType type in list)
            {
                try
                {
                    UploadTrackCode(type, NSession);
                }
                catch (Exception)
                {
                    continue;

                }

            }

            return base.Json(new { IsS = 1 });
        }

        public ViewResult Upload()
        {
            return View();
        }

        //订单编辑内“新建重复订单”需主管输入密码管控
        [HttpPost]
        public JsonResult CheckreOrderPass(string p, string username)
        {
            // 新建重复订单操作允许 梅剑 张林楠 曾学俊 毛宇慧 黄伟坤 张春平 张小雪 王红燕 管理员
            string uu = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Password from Users where Username = '" + username + "'")).
                UniqueResult());
            if (uu == p)
            {
                return Json(new { IsSuccess = true, Msg = "成功！" });
            }
            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }

        /// <summary>
        /// 转单导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public JsonResult ChangeTrackCode(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where TrackCode='" + row[0].ToString() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }
                    string str = @"
Dear Friend,
Thank you so much for your purchase in our store.
In order to make your parcel fast to arrivel, we have change the tracking number, the new tracking number is {0} 
please kindly note it, thanks.
Bestore";
                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "上传替换单号", "原单号：" + orderType.TrackCode + " 替换为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.TrackCode = row[1].ToString();
                        NSession.Update(orderType);
                        NSession.Flush();

                        if (orderType.Platform == PlatformEnum.Aliexpress.ToString())
                        {
                            List<AccountType> accountTypes =
                                NSession.CreateQuery("from AccountType where AccountName='" + orderType.Account +
                                                     "'").List<AccountType>().ToList();
                            if (accountTypes.Count > 0)
                            {
                                try
                                {
                                    accountTypes[0].ApiTokenInfo = AliUtil.RefreshToken(accountTypes[0]);
                                    AliOrderType ot = AliUtil.findOrderById(accountTypes[0].ApiKey, accountTypes[0].ApiSecret, accountTypes[0].ApiTokenInfo, orderType.OrderExNo);

                                    // 取消站内信
                                    //AliUtil.AddOrderMessage(accountTypes[0].ApiKey, accountTypes[0].ApiSecret, accountTypes[0].ApiTokenInfo, orderType.OrderExNo, string.Format(str, orderType.TrackCode), ot.buyerInfo.loginId);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }

                            }

                        }
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "已经替换", "成功"));
                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }
        public JsonResult GetOrderStatus()
        {
            List<object> data = new List<object>();

            foreach (string str in Enum.GetNames(typeof(OrderStatusEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }
        /// <summary>
        /// 结算运费导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public JsonResult ChangeFreight(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    if (row[0].ToString() == "")
                    {
                        continue;
                    }

                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where TrackCode='" + row[0].ToString() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }
                    foreach (var orderType in orderTypes)
                    {
                        // 暂时打开，之前导入运费有变动（未确定是系统导致还是人为操作）
                        //if (orderType.IsFreight == 1 && GetCurrentAccount().RoleId != 7)//同【已导入运费订单】修改操作权限
                        //{
                        //    resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号运费已导入系统中", "失败"));
                        //    continue;
                        //}

                        LoggerUtil.GetOrderRecord(orderType, "上传运费修改", "运费修改为" + Math.Round(Utilities.ToDouble(row[1]), 2).ToString(), CurrentUser, NSession);
                        orderType.Freight = Math.Round(Utilities.ToDouble(row[1]), 2);
                        orderType.IsFreight = 1; // 标记运费已导入
                        // 计算订单财务数据
                        OrderHelper.ReckonFinance(orderType, NSession);
                        NSession.Update(orderType);
                        NSession.Flush();
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "已经替换", "成功"));
                    }
                }
            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        /// 追踪号上传--非海外仓发货清单上传
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public JsonResult Replacetradecode(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where OrderExNo='" + row[0].ToString() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }
                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "上传替换单号", "原单号：" + orderType.TrackCode + " 替换为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.TrackCode = row[1].ToString();
                        // 计算订单财务数据
                        OrderHelper.ReckonFinance(orderType, NSession);
                        NSession.Update(orderType);
                        NSession.Flush();
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "已经替换", "成功"));
                    }
                }
            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        public FileResult GetExcelByStore(int id)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Parent Unique ID");
            dt.Columns.Add("*Unique ID");
            dt.Columns.Add("*Product Name");
            dt.Columns.Add("Color");
            dt.Columns.Add("Size");
            dt.Columns.Add("*Quantity");
            dt.Columns.Add("*Tags");
            dt.Columns.Add("Description");
            dt.Columns.Add("*Price");
            dt.Columns.Add("*Shipping");
            dt.Columns.Add("Shipping Time(enter without \" \", just the estimated days ");
            dt.Columns.Add("*Main Image URL");
            dt.Columns.Add("*Main Image URL2");
            dt.Columns.Add("*Main Image URL3");
            dt.Columns.Add("*Main Image URL4");
            dt.Columns.Add("*Main Image URL5");
            dt.Columns.Add("*Main Image URL6");
            dt.Columns.Add("*Main Image URL7");
            dt.Columns.Add("*Main Image URL8");

            AccountType accountType = NSession.Get<AccountType>(id);
            accountType.ApiTokenInfo = AliUtil.RefreshToken(accountType);
            int p = 1;
            Dictionary<string, string> dic1 = new Dictionary<string, string>();
            dic1.Add("36", "XS");
            dic1.Add("01", "S");
            dic1.Add("02", "M");
            dic1.Add("03", "L");
            dic1.Add("04", "XL");
            dic1.Add("05", "XXL");
            dic1.Add("06", "XXXL");
            dic1.Add("07", "XXXXL");
            dic1.Add("08", "XXXXXL");

            Dictionary<string, string> dic2 = new Dictionary<string, string>();
            dic2.Add("WH", "White");
            dic2.Add("BL", "Black");
            dic2.Add("RE", "Red");
            dic2.Add("BU", "Blue");
            dic2.Add("GE", "Green");
            dic2.Add("GR", "Grey");
            dic2.Add("BR", "Brown");
            dic2.Add("BE", "Beige");
            dic2.Add("MU", "Multicolor");
            dic2.Add("GO", "Gold");
            dic2.Add("PU", "Purple");
            dic2.Add("YE", "Yellow");
            dic2.Add("OR", "Orange");
            dic2.Add("PI", "Pink");

            while (true)
            {
                AliProductListRoot listRoot = AliUtil.QueryProductList(accountType.ApiKey, accountType.ApiSecret,
                                                                   accountType.ApiTokenInfo, p);
                p++;

                foreach (AeopAEProductDisplayDTOList aeopAeProductDisplayDtoList in listRoot.aeopAEProductDisplayDTOList)
                {
                    ALiProductRootObject aLiProductRootObject = AliUtil.FindAeProductById(accountType.ApiKey,
                                                                                          accountType.ApiSecret,
                                                                                          accountType.ApiTokenInfo,
                                                                                          aeopAeProductDisplayDtoList.
                                                                                              productId);
                    string[] str = aLiProductRootObject.imageURLs.Split(';');

                    foreach (AeopAeProductSKUs aeopAeProductSkUse in aLiProductRootObject.aeopAeProductSKUs)
                    {
                        DataRow dr = dt.NewRow();
                        if (string.IsNullOrEmpty(aeopAeProductSkUse.skuCode) || aeopAeProductSkUse.skuCode.Length <= 4)
                        {
                            continue;
                        }
                        dr[0] = aeopAeProductSkUse.skuCode.Substring(0, aeopAeProductSkUse.skuCode.Length - 4);
                        string ccc = aeopAeProductSkUse.skuCode.Substring(aeopAeProductSkUse.skuCode.Length - 4);
                        dr[1] = aeopAeProductSkUse.skuCode;
                        dr[2] = aLiProductRootObject.subject;
                        if (ccc.Length == 4)
                        {
                            string ccc1 = ccc.Substring(0, 2);
                            string ccc2 = ccc.Substring(2, 2);
                            if (dic1.ContainsKey(ccc1))
                                dr[4] = dic1[ccc1];
                            if (dic2.ContainsKey(ccc2))
                                dr[3] = dic2[ccc2];

                        }

                        dr[5] = "200";
                        dr[6] = aLiProductRootObject.productMoreKeywords1 + "," + aLiProductRootObject.productMoreKeywords2;
                        dr[7] = "";
                        dr[8] = aeopAeProductSkUse.skuPrice;
                        dr[9] = "4";
                        dr[10] = "\"3-7\"";

                        int i = 11;
                        foreach (string s in str)
                        {
                            dr[i] = s;
                            i++;
                        }
                        dt.Rows.Add(dr);

                    }
                }

                if (p > listRoot.totalPage)
                    break;
            }

            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            System.Web.HttpContext.Current.Response.Charset = "utf-8";
            System.Web.HttpContext.Current.Response.AppendHeader("Content-Disposition",
                                                                 "attachment;filename=" +
                                                                 DateTime.Now.ToString("yyyy-MM-dd") + ".xls");

            return File(Encoding.UTF8.GetBytes(ExcelHelper.GetExcelXml(dt)), "attachment;filename=" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");


        }

        public ActionResult AliSendScan()
        {
            return base.View();
        }
        public ActionResult ExportSMT()
        {
            return base.View();
        }

        public ViewResult AliTools()
        {
            return View();
        }

        public ActionResult BeforePeiScan()
        {
            return base.View();
        }

        public object CloneObjectEx(object ObjectInstance)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream();
            formatter.Serialize(serializationStream, ObjectInstance);
            serializationStream.Seek(0L, SeekOrigin.Begin);
            return formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 关联订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult Connect(int id)
        {
            IList<OrderType> data = new List<OrderType>();
            OrderType byId = this.GetById(id);
            if (byId.MId != 0)
            {
                data = base.NSession.CreateQuery("from OrderType where (Id=:p1 or MId=:p1) and Id <> :p2").SetInt32("p1", byId.MId).SetInt32("p2", byId.Id).List<OrderType>();
            }
            else
            {
                data = base.NSession.CreateQuery("from OrderType where (Id=:p1 or MId=:p1) and Id <> :p1").SetInt32("p1", byId.Id).List<OrderType>();
            }
            return base.Json(data);
        }

        public ActionResult Create()
        {
            base.ViewData["OrderNO"] = Utilities.GetOrderNo(base.NSession);
            return base.View();
        }

        [HttpPost]
        public JsonResult Create(OrderType obj)
        {
            try
            {
                AccountType type = base.NSession.Get<AccountType>(Utilities.ToInt(obj.Account));
                if (type != null)
                {
                    obj.Account = type.AccountName;
                }
                if ((obj.OrderExNo.Trim() == "") || OrderHelper.IsExist(obj.OrderExNo.Trim(), base.NSession, obj.Account))
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "平台订单号为空或者重复" });
                }
                if (Convert.ToInt32(base.NSession.CreateQuery("select count(Id) from OrderType where OrderNo='" + obj.OrderNo + "'").UniqueResult()) > 0)
                {
                    obj.OrderNo = Utilities.GetOrderNo(base.NSession);
                }
                List<CountryType> countryTypes =
                    NSession.CreateQuery("from CountryType where ECountry='" + obj.Country.Replace("'", "''") + "'").
                        List<CountryType>().ToList();
                if (countryTypes.Count > 0)
                    obj.AddressInfo.CountryCode = countryTypes[0].CountryCode;
                else
                {
                    obj.AddressInfo.CountryCode = obj.Country;
                }

                obj.AddressInfo.Email = obj.BuyerEmail;
                base.NSession.Save(obj.AddressInfo);
                obj.AddressId = obj.AddressInfo.Id;
                obj.Country = obj.AddressInfo.Country;
                obj.Status = OrderStatusEnum.待处理.ToString();
                obj.GenerateOn = obj.ScanningOn = obj.CreateOn = DateTime.Now;
                List<OrderProductType> list = JsonConvert.DeserializeObject<List<OrderProductType>>(obj.rows);
                obj.Enabled = 1;
                base.NSession.Save(obj);
                foreach (OrderProductType type2 in list)
                {
                    type2.OId = obj.Id;
                    type2.OrderNo = obj.OrderNo;
                    base.NSession.Save(type2);
                }
                base.NSession.Flush();
                LoggerUtil.GetOrderRecord(obj, "新建订单", "创建订单", base.CurrentUser, base.NSession);
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 获取新订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public OrderType CreateNewOrder(OrderType order, int t)
        {
            order.Amount = 0.0;
            order.IsPrint = 0;
            order.RMB = 0.0;
            order.TrackCode = "";
            order.Freight = 0.0;
            order.Weight = 0;
            order.OrderNo = Utilities.GetOrderNo(base.NSession);
            order.TrackCode = Utilities.GetTrackCode(base.NSession, order.LogisticMode);
            order.IsCanSplit = 0;
            if (order.MId == 0)
            {
                order.MId = order.Id;
            }
            order.Id = 0;
            order.IsAudit = 1;
            order.IsOutOfStock = 0;
            switch (t)
            {
                case 0:
                    order.IsSplit = 1;

                    break;

                case 1:
                    order.IsRepeat = 1;
                    order.IsAudit = 1;
                    order.CreateOn = DateTime.Now;
                    order.ScanningOn = DateTime.Now;
                    order.Status = OrderStatusEnum.已处理.ToString();
                    break;
            }
            base.NSession.Clear();
            base.NSession.Save(order);
            base.NSession.Flush();
            return order;
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(string o)
        {
            try
            {
                base.NSession.Delete(" from OrderType where Id in(" + o + ")");      
                IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
                foreach (OrderType type in list)
                {
                    LoggerUtil.GetOrderRecord(type, "删除订单！", "删除订单！", base.CurrentUser, base.NSession);
                }
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }

        public ActionResult Details(int id)
        {
            OrderType byId = this.GetById(id);
            byId.AddressInfo = base.NSession.Get<OrderAddressType>(byId.AddressId);
            base.ViewData["id"] = id;
            return base.View(byId);
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost, ValidateInput(false)]
        public ActionResult Edit(OrderType obj)
        {
            try
            {
                if (obj.IsFreight == 1 && GetCurrentAccount().RoleId != 7)
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "本订单已导入运费，禁止再次操作!" }, "text/html", JsonRequestBehavior.AllowGet);
                }
                if (obj.Status == "已发货")
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "禁止对已发货订单操作!" }, "text/html", JsonRequestBehavior.AllowGet);
                }

                obj.Enabled = 1;
                obj.Country = obj.AddressInfo.Country;
                OrderType byId = this.GetById(obj.Id);
                List<CountryType> countryTypes =
                 NSession.CreateQuery("from CountryType where ECountry='" + obj.Country.Replace("'", "''") + "'").
                     List<CountryType>().ToList();
                if (countryTypes.Count > 0)
                    obj.AddressInfo.CountryCode = countryTypes[0].CountryCode;
                else
                {
                    obj.AddressInfo.CountryCode = obj.Country;
                }
                base.NSession.Update(obj.AddressInfo);
                base.NSession.Flush();
                base.NSession.Clear();

                string objEditString = Utilities.GetObjEditString(byId, obj);
                base.NSession.Update(obj);
                base.NSession.Flush();
                base.NSession.Clear();
                List<OrderProductType> list = JsonConvert.DeserializeObject<List<OrderProductType>>(obj.rows);
                List<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + obj.Id).List<OrderProductType>().ToList<OrderProductType>();
                if (list.Count != list2.Count)
                {
                    objEditString = objEditString + "组合产品由<br>";
                    foreach (OrderProductType type2 in list2)
                    {
                        objEditString = objEditString + this.Zu1(type2);
                    }
                    objEditString = objEditString + "修改为<br> ";
                    foreach (OrderProductType type2 in list)
                    {
                        objEditString = objEditString + this.Zu1(type2);
                    }
                    objEditString = objEditString + "<br>";
                }
                else
                {
                    foreach (OrderProductType type3 in list2)
                    {
                        int num = 0;
                        foreach (OrderProductType type4 in list)
                        {
                            if (((((type3.ExSKU == type4.ExSKU) && (type3.Title == type4.Title)) && ((type3.SKU == type4.SKU) && (type3.Qty == type4.Qty))) && (((type3.Standard == type4.Standard) && (type3.Price == type4.Price)) && (type3.Url == type4.Url))) && (type3.Remark == type4.Remark))
                            {
                                num = 1;
                            }
                        }
                        if (num != 1)
                        {
                            objEditString = objEditString + "组合产品由<br>";
                            foreach (OrderProductType type2 in list2)
                            {
                                objEditString = objEditString + this.Zu1(type2);
                            }
                            objEditString = objEditString + "修改为<br> ";
                            foreach (OrderProductType type2 in list)
                            {
                                objEditString = objEditString + this.Zu1(type2);
                            }
                            objEditString = objEditString + "<br>";
                        }
                    }
                }
                base.NSession.CreateQuery("delete from OrderProductType where OId=" + obj.Id).ExecuteUpdate();
                foreach (OrderProductType type5 in list)
                {
                    if (type5.Qty != 0)
                    {
                        type5.OId = obj.Id;
                        type5.OrderNo = obj.OrderNo;
                        type5.IsQue = 0;
                        base.NSession.Save(type5);
                        base.NSession.Flush();
                    }
                }
                // 计算订单财务数据
                OrderHelper.ReckonFinance(obj, base.NSession);
                LoggerUtil.GetOrderRecord(obj, "修改订单", objEditString, base.CurrentUser, base.NSession);
                OrderHelper.SetQueOrder(obj, NSession);
            }
            catch (Exception exc)
            {
                string error = exc.Message;
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            OrderType byId = this.GetById(id);
            byId.AddressInfo = base.NSession.Get<OrderAddressType>(byId.AddressId);
            base.ViewData["id"] = id;
            base.ViewData["UserRole"] = GetCurrentAccount().RoleId;
            return base.View(byId);
        }

        public ActionResult EditLogisticsAllocation(string ids)
        {
            IList<LogisticsAllocationType> list = base.NSession.CreateQuery("from LogisticsAllocationType order by SortCode").List<LogisticsAllocationType>();
            foreach (LogisticsAllocationType type in list)
            {
                object obj2 = base.NSession.CreateSQLQuery("update Orders set LogisticMode='" + type.LogisticsMode + "' where Id in(" + ids + ") and " + type.QuerySql).UniqueResult();
            }
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 一键拆包
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditOneKeySplitOrder(string o)
        {
            IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
            List<ResultInfo> list2 = new List<ResultInfo>();
            foreach (OrderType type in list)
            {
                string orderNo = type.OrderNo;
                if (type.IsCanSplit == 1)
                {
                    IList<OrderProductType> list3 = base.NSession.CreateQuery(" from OrderProductType where OId in(" + type.Id + ")").List<OrderProductType>();
                    if (list3.Count == 1)
                    {
                        ResultInfo item = new ResultInfo
                        {
                            Key = orderNo,
                            Result = "缺货分包失败！",
                            Info = "产品只有一个",
                            CreateOn = DateTime.Now
                        };
                        list2.Add(item);
                    }
                    else
                    {
                        bool flag = true;
                        foreach (OrderProductType type2 in list3)
                        {
                            if (type2.IsQue != 1)
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            ResultInfo info2 = new ResultInfo
                            {
                                Key = orderNo,
                                Result = "缺货分包失败！",
                                Info = "产品全部都是缺货的",
                                CreateOn = DateTime.Now
                            };
                            list2.Add(info2);
                        }
                        else
                        {
                            type.IsSplit = 1;
                            base.NSession.Update(type);
                            base.NSession.Flush();
                            LoggerUtil.GetOrderRecord(type, "拆分订单！", "将订单拆分！", base.CurrentUser, base.NSession);
                            //////////////////////////////
                            string OrderExNoTmp = type.OrderExNo;
                            if (OrderExNoTmp.IndexOf("*") > 0)
                            {
                                OrderExNoTmp = OrderExNoTmp.Substring(0, OrderExNoTmp.IndexOf("*"));
                            }
                            string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + OrderExNoTmp + "%' order by id desc")).UniqueResult());
                            //string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + this.GetById(Utilities.ToInt(o)).OrderExNo + "%' order by id desc")).UniqueResult());
                            if (strNo.IndexOf("*") == -1)
                            {
                                // 未拆分过默认流水号为1
                                strNo = "1";
                                type.OrderExNo = type.OrderExNo + "*" + strNo;  // 查询拆分订单号并按流水号拼接
                                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                                if (type.Platform == "Ebay")
                                {
                                    type.TId = type.TId + "*" + strNo;
                                }
                                type.FanState = 1;//被拆分订单自动标记为已收汇
                                type.FanAmount = Convert.ToDecimal(0.001);

                            }
                            else
                            {
                                strNo = strNo.Substring(strNo.IndexOf("*") + 1);
                                strNo = (Int32.Parse(strNo) + 1).ToString();
                                type.OrderExNo = type.OrderExNo.Substring(0, type.OrderExNo.IndexOf("*") == -1 ? type.OrderExNo.Length : type.OrderExNo.IndexOf("*")) + "*" + strNo;
                                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                                if (type.Platform == "Ebay")
                                {
                                    type.TId = type.TId.Substring(0, type.TId.IndexOf("*") == -1 ? type.TId.Length : type.TId.IndexOf("*")) + "*" + strNo; // 
                                }
                                type.FanState = 1; //被拆分订单自动标记为已收汇
                                type.FanAmount = Convert.ToDecimal(0.001);
                            }
                            //////////////////////////////
                            base.NSession.Clear();
                            OrderType order = this.CreateNewOrder(type, 0);
                            foreach (OrderProductType type2 in list3)
                            {
                                if (type2.IsQue != 1)
                                {
                                    type2.OId = order.Id;
                                    type2.OrderNo = order.OrderNo;
                                    type2.IsQue = 3;
                                    base.NSession.Update(type2);
                                    base.NSession.Flush();
                                }
                            }
                            LoggerUtil.GetOrderRecord(order, "拆分订单！", "拆分新建！", base.CurrentUser, base.NSession);
                            ResultInfo info3 = new ResultInfo
                            {
                                Key = orderNo,
                                Result = "缺货分包成功！",
                                Info = "分成两个包裹，生成了新的包裹:" + order.OrderNo,
                                Field1 = order.OrderNo,
                                CreateOn = DateTime.Now
                            };
                            list2.Add(info3);
                        }
                    }
                }
                else
                {
                    ResultInfo info4 = new ResultInfo
                    {
                        Key = orderNo,
                        Result = "缺货分包失败！",
                        Info = "订单不可拆分",
                        CreateOn = DateTime.Now
                    };
                    list2.Add(info4);
                }
            }
            base.Session["Results"] = list2;
            return base.Json(new { IsSuccess = true });
        }

        [HttpPost]
        public ActionResult EditOrderHoldUp(string o, string t, string d, int s)
        {
            IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
            foreach (OrderType type in list)
            {
                if (type.Status != OrderStatusEnum.已发货.ToString())
                {
                    base.NSession.Clear();
                    type.IsError = 1;
                    type.CutOffMemo = t + " " + d;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    string subject = "拦截";
                    if (s == 1)
                    {
                        subject = "拦截-重置产品入库";
                    }
                    this.SetQuestionOrder(subject, type, "");
                    LoggerUtil.GetOrderRecord(type, "拦截订单！", "将订单拦截，原因：" + t + " " + d, base.CurrentUser, base.NSession);
                }
            }
            return base.Json(new { IsSuccess = true });
        }

        public ActionResult EditOrderMerger()
        {
            OrderType current;
            IEnumerator<OrderType> enumerator;
            //IList<OrderType> source = base.NSession.CreateQuery("from OrderType where Status='待处理' and Platform='Ebay' and Enabled=1 and BuyerName in (select BuyerName from OrderType where Status='待处理'  and Platform='Ebay' and Enabled=1   group by BuyerName,Country,Account having count (BuyerName)>1)").List<OrderType>();
            //去掉Ebay平台限制
            IList<OrderType> source = base.NSession.CreateQuery("from OrderType where Status='待处理' and Enabled=1 and BuyerName in (select BuyerName from OrderType where Status='待处理'  and Enabled=1   group by BuyerName,Country,Account having count (BuyerName)>1)").List<OrderType>();
            string str = "";
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    str = str + current.AddressId + ",";
                }
            }
            List<OrderAddressType> list2 = base.NSession.CreateQuery("from OrderAddressType where Id in(" + str.Trim(new char[] { ',' }) + ")").List<OrderAddressType>().ToList<OrderAddressType>();
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Predicate<OrderAddressType> match = null;
                    OrderType o = enumerator.Current;
                    if (match == null)
                    {
                        match = x => x.Id == o.AddressId;
                    }
                    o.AddressInfo = list2.Find(match);
                }
            }
            List<int> list3 = new List<int>();
            List<OrderType> list4 = new List<OrderType>(source.ToArray<OrderType>());
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<OrderType, bool> predicate = null;
                    OrderType o = enumerator.Current;
                    if (!list3.Contains(o.Id))
                    {
                        if (predicate == null)
                        {
                            predicate = x => (((x.BuyerName == o.BuyerName) && (x.Country == o.Country)) && (x.Account == o.Account)) && (x.AddressInfo.Street == o.AddressInfo.Street);
                        }
                        List<OrderType> list5 = list4.Where<OrderType>(predicate).ToList<OrderType>();
                        current = new OrderType();
                        if (list5.Count > 1)
                        {
                            base.NSession.Clear();
                            current = this.CloneObjectEx(o) as OrderType;
                            current.Id = 0;
                            current.OrderNo = Utilities.GetOrderNo(base.NSession);
                            current.Amount = 0.0;
                            current.IsMerger = 1;
                            current.Enabled = 1;
                            base.NSession.SaveOrUpdate(current);
                            base.NSession.Flush();
                            foreach (OrderType type2 in list5)
                            {
                                list3.Add(type2.Id);
                                current.Amount += type2.Amount;
                                current.OrderExNo = current.OrderExNo + "|" + type2.OrderExNo;
                                current.TId = current.TId + "|" + type2.TId;
                                type2.MId = current.Id;
                                type2.IsMerger = 1;
                                type2.Enabled = 0;
                                base.NSession.Clear();
                                base.NSession.SaveOrUpdate(type2);
                                base.NSession.Flush();
                                IList<OrderProductType> list6 = base.NSession.CreateQuery(" from OrderProductType where OId=" + type2.Id).List<OrderProductType>();
                                foreach (OrderProductType type3 in list6)
                                {
                                    type3.Id = 0;
                                    type3.OId = current.Id;
                                    type3.OrderNo = current.OrderNo;
                                    base.NSession.Clear();
                                    base.NSession.SaveOrUpdate(type3);
                                    base.NSession.Flush();
                                }
                            }
                            base.NSession.Clear();
                            base.NSession.SaveOrUpdate(current);
                            base.NSession.Flush();
                        }
                    }
                }
            }
            return base.Json(new { IsSuccess = true });
        }
        //待处理订单合并订单
        public ActionResult unhandOrderMerger(string ids)
        {
            OrderType current;
            IEnumerator<OrderType> enumerator;
            IList<OrderType> source = base.NSession.CreateQuery("from OrderType where Id in (" + ids + ")").List<OrderType>();
            string str = "";
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    str = str + current.AddressId + ",";
                }
            }
            List<OrderAddressType> list2 = base.NSession.CreateQuery("from OrderAddressType where Id in(" + str.Trim(new char[] { ',' }) + ")").List<OrderAddressType>().ToList<OrderAddressType>();
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Predicate<OrderAddressType> match = null;
                    OrderType o = enumerator.Current;
                    if (match == null)
                    {
                        match = x => x.Id == o.AddressId;
                    }
                    o.AddressInfo = list2.Find(match);
                }
            }
            List<int> list3 = new List<int>();
            List<OrderType> list4 = new List<OrderType>(source.ToArray<OrderType>());
            using (enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<OrderType, bool> predicate = null;
                    OrderType o = enumerator.Current;
                    if (!list3.Contains(o.Id))
                    {
                        if (predicate == null)
                        {
                            predicate = x => (((x.BuyerName == o.BuyerName) && (x.Country == o.Country)) && (x.Account == o.Account)) && (x.AddressInfo.Street == o.AddressInfo.Street);
                        }
                        List<OrderType> list5 = list4.Where<OrderType>(predicate).ToList<OrderType>();
                        current = new OrderType();
                        if (list5.Count > 1)
                        {
                            base.NSession.Clear();
                            current = this.CloneObjectEx(o) as OrderType;
                            current.Id = 0;
                            current.OrderNo = Utilities.GetOrderNo(base.NSession);
                            current.Amount = 0.0;
                            current.IsMerger = 1;
                            current.Enabled = 1;
                            current.OrderExNo = current.OrderExNo + "*2";
                            base.NSession.SaveOrUpdate(current);
                            base.NSession.Flush();
                            foreach (OrderType type2 in list5)
                            {
                                list3.Add(type2.Id);
                                current.Amount += type2.Amount;
                                if (current.OrderExNo != type2.OrderExNo)
                                {
                                    // current.OrderExNo = current.OrderExNo + "*" + type2.OrderExNo;
                                    //current.OrderExNo = current.OrderExNo + "*2";多次加*2拆分的时候会报错
                                    current.TId = current.TId + "|" + type2.TId;
                                }
                                type2.MId = current.Id;
                                type2.IsMerger = 1;
                                type2.Enabled = 0;
                                base.NSession.Clear();
                                base.NSession.SaveOrUpdate(type2);
                                base.NSession.Flush();
                                IList<OrderProductType> list6 = base.NSession.CreateQuery(" from OrderProductType where OId=" + type2.Id).List<OrderProductType>();
                                foreach (OrderProductType type3 in list6)
                                {
                                    type3.Id = 0;
                                    type3.OId = current.Id;
                                    type3.OrderNo = current.OrderNo;
                                    base.NSession.Clear();
                                    base.NSession.SaveOrUpdate(type3);
                                    base.NSession.Flush();
                                }
                            }
                            base.NSession.Clear();
                            base.NSession.SaveOrUpdate(current);
                            base.NSession.Flush();
                        }
                    }
                }
            }
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 订单属性变更
        /// </summary>
        /// <param name="o"></param>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditOrderProperty(string o, int t, string c)
        {
            //if (this.GetById(Utilities.ToInt(o)).Status == "已发货")
            //{
            //    return base.Json(new { IsSuccess = false, Message = "禁止对已发货订单操作！" });
            //}

            string str = "";
            string recordType = "";
            string content = "";
            switch (t)
            {
                case 0:
                    str = " IsStop=0 ";
                    recordType = "撤销订单的停售状态！";
                    content = "将订单的停售标记删除";
                    break;

                case 1:
                    str = " IsOutOfStock=0 ";
                    recordType = "撤销订单的缺货状态！";
                    content = "将订单的缺货标记删除！";
                    base.NSession.CreateQuery(" Update OrderProductType set IsQue=0 where OId in(" + o + ")").ExecuteUpdate();
                    break;

                case 2:
                    str = " Status='" + OrderStatusEnum.已处理.ToString() + "',IsError=0 ";
                    recordType = "订单重置";
                    content = "将订单状态设置为已处理，并标记订单正常";
                    break;

                case 3:
                    str = "Status='" + OrderStatusEnum.作废订单.ToString() + "', IsAudit=1,SellerMemo='" + c + "',IsOutOfStock=0 ";
                    recordType = "订单作废";
                    content = "将订单状态设置为作废订单";
                    break;
            }
            int num = base.NSession.CreateQuery(" Update OrderType set " + str + " where Id in(" + o + ")").ExecuteUpdate();
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType where Id In (" + o + ")").List<OrderType>();
            if (list !=null && list.Count > 0)
            {
                foreach (OrderType type in list)
                {
                    if (t == 3)
                    {
                        SetQuestionOrder("作废订单-重置包裹入库", type, "");
                        NSession.CreateQuery(" Update OrderProductType set IsQue=0 where OId in(" + o + ")").ExecuteUpdate();
                        IList<OrderProductType> orderProductTypes =
                            NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                        if (orderProductTypes != null && orderProductTypes.Count > 0)
                        {
                            foreach (OrderProductType orderProductType in orderProductTypes)
                            {
                                IList<OrderType> list2 = base.NSession.CreateQuery(" from OrderType where Id in(select OId from OrderProductType where SKU ='" + orderProductType.SKU + "' ) and  Status in ('已处理')").List<OrderType>();
                                if (list2 != null && list2.Count > 0)
                                {
                                    foreach (OrderType order in list2)
                                    {
                                        OrderHelper.SetQueOrder(order, base.NSession);
                                    }
                                }
                               
                            }
                        }
                        

                    }
                    LoggerUtil.GetOrderRecord(type, recordType, content, base.CurrentUser, base.NSession);
                }
            }
           
            if (num > 0)
            {
                return base.Json(new { IsSuccess = true });
            }
            return base.Json(new { IsSuccess = false });
        }
        /// <summary>
        /// 重新计算订单缺货状态
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult CalculateQue(string ids)
        {
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType where Id In (" + ids + ")").List<OrderType>();
            bool result = true;
            try
            {
                foreach (OrderType type in list)
                {
                    IList<OrderProductType> orderProductTypes =
                        NSession.CreateQuery("from OrderProductType where OId=" + ids).List<OrderProductType>();
                    foreach (OrderProductType orderProductType in orderProductTypes)
                    {
                        IList<OrderType> list2 = base.NSession.CreateQuery(" from OrderType where Id in(select OId from OrderProductType where SKU ='" + orderProductType.SKU + "' ) and  Status in ('已处理') and Enabled=1").List<OrderType>();

                        foreach (OrderType order in list2)
                        {
                            OrderHelper.SetQueOrder(order, base.NSession);
                        }
                    }


                    LoggerUtil.GetOrderRecord(type, "缺货重算", "将订单缺货状态重新计算", base.CurrentUser, base.NSession);

                }
            }
            catch (Exception ex)
            {


            }
            return base.Json(new { IsSuccess = true });

        }
        [HttpPost]
        public ActionResult EditOrderReplace(string ids, string newField, string fieldName)
        {
            string str = fieldName;
            List<OrderType> orderTypes =
                NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            UserType curruser = GetCurrentAccount();
            if (str != null)
            {
                if (!(str == "Country"))
                {
                    if (str == "SKU")
                    {
                        OrderHelper.ReplaceBySKU(ids, newField, base.NSession);
                        foreach (OrderType orderType in orderTypes)
                        {
                            LoggerUtil.GetOrderRecord(orderType, "批量修改SKU", "批量将SKU设置为" + newField, curruser, NSession);
                        }

                    }
                    else if (str == "CurrencyCode")
                    {
                        OrderHelper.ReplaceByCurrencyOrLogistic(ids, newField, 1, base.NSession);
                    }
                    else if (str == "LogisticMode")
                    {
                        //if (result == true)
                        //{
                        foreach (OrderType orderType in orderTypes)
                        {
                            //// 获取号码池跟踪码
                            //orderType.TrackCode = Utilities.GetTrackCode(NSession, orderType.LogisticMode);
                            //NSession.Update(orderType);
                            //NSession.Flush();
                            LoggerUtil.GetOrderRecord(orderType, "批量修改物流方式", "批量将物流方式设置为" + newField, curruser, NSession);
                        }
                        //}
                        bool result = OrderHelper.ReplaceByCurrencyOrLogistic(ids, newField, 0, base.NSession);
                    }
                }
                else
                {
                    OrderHelper.ReplaceByCountry(ids, newField, base.NSession);
                }
            }
            return base.Json(new { IsSuccess = true });
        }
        [HttpPost]
        public ActionResult EditOrders(string ids, string fieldType, string newField, string fieldName)
        {
            string strtype = fieldType;
            string str = fieldName;

            if (ids == "''" || string.IsNullOrEmpty(str))
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "替换值或者输入框不能为空" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<OrderType> orderTypes = new List<OrderType>();
                //不允许对已发货订单进行修改，查找时过滤已发货订单
                if (strtype == "OrderNo")
                {
                    orderTypes = NSession.CreateQuery("from OrderType where OrderNo in(" + ids + ") and Status<>'已发货'").List<OrderType>().ToList();
                }
                else if (strtype == "OrderExNo")
                {
                    orderTypes = NSession.CreateQuery("from OrderType where OrderExNo in(" + ids + ") and Status<>'已发货'").List<OrderType>().ToList();
                }
                else if (strtype == "TrackCode")
                {
                    orderTypes = NSession.CreateQuery("from OrderType where TrackCode in(" + ids + ") and Status<>'已发货'").List<OrderType>().ToList();
                }
                else
                {
                    orderTypes = NSession.CreateQuery("from OrderType where TId in(" + ids + ") and Status<>'已发货'").List<OrderType>().ToList();
                }
                UserType curruser = GetCurrentAccount();
                if (str != null)
                {
                    if (!(str == "Country"))
                    {
                        if (str == "SKU")
                        {
                            OrderHelper.ReplaceBySKU(strtype, ids, newField, base.NSession);
                            foreach (OrderType orderType in orderTypes)
                            {
                                LoggerUtil.GetOrderRecord(orderType, "批量修改SKU", "批量将SKU设置为" + newField, curruser, NSession);
                            }

                        }
                        else if (str == "CurrencyCode")
                        {
                            OrderHelper.ReplaceByCurrencyOrLogistic(strtype, ids, newField, 1, base.NSession);
                            foreach (OrderType orderType in orderTypes)
                            {
                                LoggerUtil.GetOrderRecord(orderType, "批量修改货币", "批量将货币设置为" + newField, curruser, NSession);
                            }

                        }
                        else if (str == "LogisticMode")
                        {
                            OrderHelper.ReplaceByCurrencyOrLogistic(strtype, ids, newField, 0, base.NSession);
                            foreach (OrderType orderType in orderTypes)
                            {
                                LoggerUtil.GetOrderRecord(orderType, "批量修改物流方式", "批量将物流方式设置为" + newField, curruser, NSession);
                            }
                        }
                        else if (str == "OrderStatusinput")
                        {
                            OrderHelper.ReplaceOrderStatus(strtype, ids, newField, 0, base.NSession);
                            foreach (OrderType orderType in orderTypes)
                            {
                                LoggerUtil.GetOrderRecord(orderType, "批量修改订单状态", "批量将订单状态设置为" + newField, curruser, NSession);
                            }

                        }
                    }
                    else
                    {
                        OrderHelper.ReplaceByCountry(strtype, ids, newField, base.NSession);
                        foreach (OrderType orderType in orderTypes)
                        {

                            LoggerUtil.GetOrderRecord(orderType, "批量修改国家", "批量将订单国家设置为" + newField, curruser, NSession);
                        }
                    }
                }
                return base.Json(new { IsSuccess = true });

            }
        }
        [HttpPost]
        public JsonResult CheckPass(string p)
        {
            IList<UserType> list = base.NSession.CreateQuery(" from UserType where Username='admin'").List<UserType>();

            //if (p == GetCurrentAccount().Password)
            if (p == list[0].Password)
            {
                return Json(new { IsSuccess = true, Msg = "成功！" });
            }

            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }
        /// <summary>
        /// 设置海外仓
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditOrderByFBA(string ids, string f)
        {
            List<OrderType> orderTypes =
                NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            UserType curruser = GetCurrentAccount();
            // 判断是否缺货订单并禁止发货
            foreach (OrderType type in orderTypes)
            {
                OrderHelper.SetQueOrder(f, type, NSession);
                if (type.IsOutOfStock == 1)
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
                    return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单为缺货状态！" });
                }
            }

            foreach (OrderType orderType in orderTypes)
            {
                if (orderType.IsFBA == 0 || orderType.Status == "待处理" || orderType.Status == "已处理")
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
                        orderType.ScanningOn = DateTime.Now;
                        base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + orderType.Id).ExecuteUpdate();
                        LoggerUtil.GetOrderRecord(orderType, "设置海外仓订单", "设置为海外仓订单", curruser, NSession);

                        ////FBA订单直接扣除库存
                        //List<WarehouseType> warehouseTypes = NSession.CreateQuery("from WarehouseType where WCode='" + orderType.Account + "'").List<WarehouseType>().ToList();
                        //if (warehouseTypes.Count > 0)
                        //{
                        //    if (f == "FBA")
                        //    {
                        //        foreach (var item in orderType.Products)
                        //        {
                        //            Utilities.StockOut(warehouseTypes[0].Id, item.SKU, item.Qty, "FBA出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesKS[0].Id, item.SKU, item.Qty, "KS仓库(美西-宁波)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesKSYW[0].Id, item.SKU, item.Qty, "KS仓库(义乌)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesUSEast[0].Id, item.SKU, item.Qty, "美东(宁波)出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesCA[0].Id, item.SKU, item.Qty, "美东(义乌)出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWRUAEA[0].Id, item.SKU, item.Qty, "YWRU-AEA出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesYWRUAEB[0].Id, item.SKU, item.Qty, "YWRU-AEB出库", CurrentUser.Realname, "", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesLAI[0].Id, item.SKU, item.Qty, "LAI出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesUKMAN[0].Id, item.SKU, item.Qty, "UKMAN出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWAZDE[0].Id, item.SKU, item.Qty, "YWAZDE出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBA[0].Id, item.SKU, item.Qty, "FBA出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB01[0].Id, item.SKU, item.Qty, "FBA-NB01出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB02[0].Id, item.SKU, item.Qty, "FBA-NB02出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB03[0].Id, item.SKU, item.Qty, "FBA-NB03出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB04[0].Id, item.SKU, item.Qty, "FBA-NB04出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB05[0].Id, item.SKU, item.Qty, "FBA-NB05出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB06[0].Id, item.SKU, item.Qty, "FBA-NB06出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB07[0].Id, item.SKU, item.Qty, "FBA-NB07出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB08[0].Id, item.SKU, item.Qty, "FBA-NB08出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBANB09[0].Id, item.SKU, item.Qty, "FBA-NB09出库", CurrentUser.Realname,
                                                           "", orderType.OrderNo, NSession);
                                    }
                                }
                                break;
                            case "FBC-NBCD01":
                                //FBC-NBCD01订单直接扣除库存
                                List<WarehouseType> warehouseTypesFBCNBCD01 = NSession.CreateQuery("from WarehouseType where WCode='nbcd01'").List<WarehouseType>().ToList();
                                if (warehouseTypesFBCNBCD01.Count > 0)
                                {
                                    foreach (var item in orderType.Products)
                                    {
                                        Utilities.StockOut(warehouseTypesFBCNBCD01[0].Id, item.SKU, item.Qty, "FBC-NBCD01出库", CurrentUser.Realname,"", orderType.OrderNo, NSession);
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
                                        Utilities.StockOut(warehouseTypesGCUSEast[0].Id, item.SKU, item.Qty, "谷仓美东（宁波）出库", CurrentUser.Realname,
                                                           "", orderType.OrderNo, NSession);
                                    }
                                }
                                break;
                            case "GCUS-West":
                                //谷仓美西（宁波）订单直接扣除库存
                                List<WarehouseType> warehouseTypesGCUSWest = NSession.CreateQuery("from WarehouseType where WCode='GCUS-West'").List<WarehouseType>().ToList();
                                if (warehouseTypesGCUSWest.Count > 0)
                                {
                                    foreach (var item in orderType.Products)
                                    {
                                        Utilities.StockOut(warehouseTypesGCUSWest[0].Id, item.SKU, item.Qty, "谷仓美西（宁波）出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesNBRUAEA[0].Id, item.SKU, item.Qty, "NBRU-AEA出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWAZUS[0].Id, item.SKU, item.Qty, "FBA-YWAZUS出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWAZUS01[0].Id, item.SKU, item.Qty, "FBA-YWAZUS01出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWAZUS02[0].Id, item.SKU, item.Qty, "FBA-YWAZUS02出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWUK01[0].Id, item.SKU, item.Qty, "FBA-YWAZUK出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWUK02[0].Id, item.SKU, item.Qty, "FBA-YWAZUK02出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWAZDE[0].Id, item.SKU, item.Qty, "FBA-YWAZDE出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesFBAYWAZFR[0].Id, item.SKU, item.Qty, "FBA-YWAZFR出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWGCUSWest[0].Id, item.SKU, item.Qty, "谷仓美西（义乌）出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWGCUSEast[0].Id, item.SKU, item.Qty, "谷仓美东（义乌）出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWCAWESTDONG[0].Id, item.SKU, item.Qty, "美西(董)（义乌）出库", CurrentUser.Realname,
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
                                        Utilities.StockOut(warehouseTypesYWNJEASTLEO[0].Id, item.SKU, item.Qty, "美东(LEO)（义乌）出库", CurrentUser.Realname,
                                                           "", orderType.OrderNo, NSession);
                                    }
                                }
                                break;
                            case "YWMRU-AEA":
                                //美东(LEO)（义乌）订单直接扣除库存
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
                                //美东(LEO)（义乌）订单直接扣除库存
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

                    }
                    // 计算订单财务信息
                    OrderHelper.ReckonFinance(orderType, NSession);
                    NSession.Update(orderType);
                    NSession.Flush();
                }
                else
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单已设为海外仓！" });
                }
            }
            return base.Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 鑫弈仓库扣库存判断,当前订单的省对应哪个仓库，则调用仓库地址中对应的仓库
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult editOrderXinyi(string ids)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            bool IsMatch = true;
            StringBuilder ErrorInfo = new StringBuilder();
            UserType curruser = GetCurrentAccount();

            foreach (OrderType type in orders)
            {
                type.AddressInfo = NSession.Get<OrderAddressType>(type.AddressId);
                string sql = " from WarehouseProInfoType where Province='" + type.AddressInfo.Province + "'";
                List<WarehouseProInfoType> info = NSession.CreateQuery(sql).List<WarehouseProInfoType>().ToList();
                if (info.Count > 0)//符合鑫弈中州对应的规则
                {
                    List<WarehouseType> warehouse = NSession.CreateQuery("from WarehouseType where Address='" + info[0].WareHouse + "'").List<WarehouseType>().ToList();
                    if (warehouse.Count > 0)//找到了对应的仓库
                    {
                        if (warehouse[0].WCode == "KS")
                        {
                            // 判断是否缺货订单并禁止发货
                            OrderHelper.SetQueOrder("KS", type, NSession);
                            if (type.IsOutOfStock == 1)
                            {
                                //return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单为缺货状态！" });
                                IsMatch = false;
                                type.ErrorInfo = "订单" + type.OrderNo + "无法出库！ 当前订单为缺货状态！"; ;
                                NSession.Update(type);
                                NSession.Flush();
                                ErrorInfo.Append("订单" + type.OrderNo + "无法出库！ 当前订单为缺货状态！/");
                            }
                            else
                            {
                                if (type.IsFBA == 0 || type.Status == "待处理" || type.Status == "已处理")
                                {
                                    bool iscon = true;
                                    type.Products = NSession.CreateQuery("from OrderProductType where OId='" + type.Id + "'").List<OrderProductType>().ToList();

                                    foreach (var item in type.Products)
                                    {
                                        if (item.SKU == null)
                                        {

                                            type.ErrorInfo += "SKU不符";
                                            break;
                                        }
                                        List<ProductType> products = NSession.CreateQuery("from ProductType where SKU='" + item.SKU + "'").List<ProductType>().ToList();
                                        if (products.Count == 0)
                                        {
                                            iscon = false;
                                            type.ErrorInfo += "SKU不符";
                                            break;

                                        }
                                        else
                                        {

                                        }
                                    }
                                    if (iscon)
                                    {
                                        type.IsFBA = 1;
                                        type.FBABy = "KS";
                                        type.Status = "已发货";
                                        type.Enabled = 1;
                                        type.IsAudit = 1;
                                        type.ScanningOn = DateTime.Now;
                                        base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                                        LoggerUtil.GetOrderRecord(type, "设置海外仓订单", "设置为海外仓订单", curruser, NSession);

                                        //KS订单直接扣除库存 KS仓库(美西-宁波)
                                        List<WarehouseType> warehouseTypesKS = NSession.CreateQuery("from WarehouseType where WCode='KS'").List<WarehouseType>().ToList();
                                        if (warehouseTypesKS.Count > 0)
                                        {
                                            foreach (var item in type.Products)
                                            {
                                                Utilities.StockOut(warehouseTypesKS[0].Id, item.SKU, item.Qty, "KS仓库(美西-宁波)出库", CurrentUser.Realname, "", type.OrderNo, NSession);
                                            }
                                        }

                                    }
                                    type.ErrorInfo = "";
                                    OrderHelper.ReckonFinance(type, NSession);
                                    NSession.Update(type);
                                    NSession.Flush();

                                }
                                else
                                {
                                    //return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单已设为海外仓！" });
                                    IsMatch = false;
                                    type.ErrorInfo = "订单" + type.OrderNo + "无法出库！ 当前订单已设为海外仓！"; ;
                                    NSession.Update(type);
                                    NSession.Flush();
                                    ErrorInfo.Append("订单" + type.OrderNo + "无法出库！ 当前订单已设为海外仓！/");
                                }
                            }
                        }
                        else if (warehouse[0].WCode == "US-East")
                        {
                            // 判断是否缺货订单并禁止发货
                            OrderHelper.SetQueOrder("US-East", type, NSession);
                            if (type.IsOutOfStock == 1)
                            {
                                IsMatch = false;
                                type.ErrorInfo = "订单" + type.OrderNo + "无法出库！ 当前订单为缺货状态！"; ;
                                NSession.Update(type);
                                NSession.Flush();
                                ErrorInfo.Append("订单" + type.OrderNo + "无法出库！ 当前订单为缺货状态！/");
                            }
                            else
                            {
                                if (type.IsFBA == 0 || type.Status == "待处理" || type.Status == "已处理")
                                {
                                    bool iscon = true;
                                    type.Products = NSession.CreateQuery("from OrderProductType where OId='" + type.Id + "'").List<OrderProductType>().ToList();

                                    foreach (var item in type.Products)
                                    {
                                        if (item.SKU == null)
                                        {

                                            type.ErrorInfo = "SKU不符";
                                            break;
                                        }
                                        List<ProductType> products = NSession.CreateQuery("from ProductType where SKU='" + item.SKU + "'").List<ProductType>().ToList();
                                        if (products.Count == 0)
                                        {
                                            iscon = false;
                                            type.ErrorInfo += "SKU不符";
                                            break;

                                        }
                                        else
                                        {

                                        }
                                    }
                                    if (iscon)
                                    {
                                        type.IsFBA = 1;
                                        type.FBABy = "US-East";
                                        type.Status = "已发货";
                                        type.Enabled = 1;
                                        type.IsAudit = 1;
                                        type.ScanningOn = DateTime.Now;
                                        base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                                        LoggerUtil.GetOrderRecord(type, "设置海外仓订单", "设置为海外仓订单", curruser, NSession);

                                        //US-East订单直接扣除库存 美东(宁波)
                                        List<WarehouseType> warehouseTypesUSEast = NSession.CreateQuery("from WarehouseType where WCode='US-East'").List<WarehouseType>().ToList();
                                        if (warehouseTypesUSEast.Count > 0)
                                        {
                                            foreach (var item in type.Products)
                                            {
                                                Utilities.StockOut(warehouseTypesUSEast[0].Id, item.SKU, item.Qty, "美东(宁波)出库", CurrentUser.Realname, "", type.OrderNo, NSession);
                                            }
                                        }

                                    }
                                    type.ErrorInfo = "";
                                    OrderHelper.ReckonFinance(type, NSession);
                                    NSession.Update(type);
                                    NSession.Flush();
                                }
                                else
                                {
                                    // return base.Json(new { IsSuccess = false, ErrorMsg = "无法出库！ 当前订单已设为海外仓！" });
                                    IsMatch = false;
                                    type.ErrorInfo += "订单" + type.OrderNo + "无法出库！ 当前订单已设为海外仓！"; ;
                                    NSession.Update(type);
                                    NSession.Flush();
                                    ErrorInfo.Append("订单" + type.OrderNo + "无法出库！ 当前订单已设为海外仓！/");
                                }

                            }
                        }
                        else
                        {
                            IsMatch = false;

                        }
                    }
                    else
                    {
                        IsMatch = false;
                    }

                }
                else
                {
                    IsMatch = false;
                    type.ErrorInfo = "不符合鑫弈仓库规则";
                    NSession.Update(type);
                    NSession.Flush();
                    ErrorInfo.Append("订单" + type.OrderNo + "不符合鑫弈仓库规则/");

                }
            }

            if (IsMatch)
            {
                return base.Json(new { IsSuccess = true });

            }
            else
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = ErrorInfo.ToString() });
            }

        }
        /// <summary>
        /// 撤回海外仓
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReSetFBA(int id)
        {
            List<OrderType> orderTypes =
                NSession.CreateQuery("from OrderType where Id in(" + id + ")").List<OrderType>().ToList();
            UserType curruser = GetCurrentAccount();
            foreach (OrderType orderType in orderTypes)
            {
                if (orderType.IsDao > 0 && (orderType.FBABy == "YWRU-AEA" || orderType.FBABy == "YWRU-AEB"))
                {
                    return base.Json(new { IsSuccess = true ,Msg="订单已导出，撤回先与跟单确认！"});
                }
                bool iscon = true;
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId='" + orderType.Id + "'").List<OrderProductType>().ToList();

                orderType.IsFBA = 0;
                orderType.FBABy = "";
                orderType.Status = "已处理";
                orderType.Enabled = 1;
                orderType.IsAudit = 1;
                orderType.IsFreight = 0;//2016-10-25
                //base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + orderType.Id).ExecuteUpdate();
                OrderHelper.SetQueOrder(orderType, NSession);
                LoggerUtil.GetOrderRecord(orderType, "海外仓订单撤回", "海外仓订单撤回", curruser, NSession);

                List<StockOutType> StockOutList =
                       NSession.CreateQuery("from StockOutType where qty>0 and Enabled=0 and OrderNo='" + orderType.OrderNo + "'").List<StockOutType>().ToList();
                if (StockOutList.Count > 0)
                {
                    foreach (StockOutType obj in StockOutList)
                    {
                        Utilities.StockOut(obj.WId, obj.SKU, obj.Qty * -1, "退件入库冲红", CurrentUser.Realname, obj.Memo, obj.OrderNo, NSession);

                        // 标记出库记录已被冲红
                        obj.Enabled = 1;
                        NSession.Update(obj);
                        NSession.Flush();
                    }
                    //Utilities.StockOut(StockOutList[0].WId, StockOutList[0].SKU, StockOutList[0].Qty * -1, "退件入库冲红", CurrentUser.Realname, StockOutList[0].Memo, StockOutList[0].OrderNo, NSession);
                }

                NSession.Update(orderType);
                NSession.Flush();
            }
            return base.Json(new { IsSuccess = true, Msg = "成功！" });
        }

        /// <summary>
        /// 验证订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult EditOrderVali(string ids)
        {
            List<CountryType> countrys = base.NSession.CreateQuery("from CountryType").List<CountryType>().ToList<CountryType>();
            //List<ProductType> products = base.NSession.CreateQuery("from ProductType where SKU is not null").List<ProductType>().ToList<ProductType>(); // 商品数量目前超30W行运行到此处数据相当慢
            List<CurrencyType> currencys = base.NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList<CurrencyType>();
            List<LogisticsModeType> logistics = base.NSession.CreateQuery("from LogisticsModeType").List<LogisticsModeType>().ToList<LogisticsModeType>();
            IList<OrderType> list5 = base.NSession.CreateQuery(" from OrderType where Status='待处理' and Id in (" + ids + ")").List<OrderType>();
            foreach (OrderType type in list5)
            {
                OrderHelper.ValiOrder(type, countrys, null, currencys, logistics, base.NSession);
            }
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 新建重复订单
        /// </summary>
        /// <param name="o"></param>
        /// <param name="rows"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditReOrder(string o, string rows, string m)
        {
            base.NSession.Clear();
            OrderType byId = this.GetById(Utilities.ToInt(o));
            byId.SellerMemo = m + byId.SellerMemo;
            byId.IsRepeat = 0;
            //byId.IsFreight = 0; // 初始化运费导入状态
            base.NSession.Update(byId);
            base.NSession.Flush();
            List<OrderProductType> list = JsonConvert.DeserializeObject<List<OrderProductType>>(rows);
            base.NSession.Clear();
            // 修改平台单号后加附加码
            //////////////////////////////
            string OrderExNoTmp = this.GetById(Utilities.ToInt(o)).OrderExNo;
            if (OrderExNoTmp.IndexOf("#") > 0)
            {
                OrderExNoTmp = OrderExNoTmp.Substring(0, OrderExNoTmp.IndexOf("#"));
            }
            string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + OrderExNoTmp + "%' order by id desc")).UniqueResult());
            if (strNo.IndexOf("#") == -1)
            {
                // 未拆分过默认流水号为1
                strNo = "1";
                byId.OrderExNo = byId.OrderExNo + "#" + strNo;  // 查询拆分订单号并按流水号拼接
                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做#1#2的设置
                if (byId.Platform == "Ebay")
                {
                    byId.TId = byId.TId + "#" + strNo;
                }
                byId.FanState = 1;  // 标记已收汇
                byId.FanAmount = Convert.ToDecimal(0.01);
                byId.IsFreight = 0;
            }
            else
            {
                strNo = strNo.Substring(strNo.IndexOf("#") + 1);
                strNo = (Int32.Parse(strNo) + 1).ToString();
                byId.OrderExNo = byId.OrderExNo.Substring(0, byId.OrderExNo.IndexOf("#") == -1 ? byId.OrderExNo.Length : byId.OrderExNo.IndexOf("#")) + "#" + strNo;
                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做#1#2的设置
                if (byId.Platform == "Ebay")
                {
                    byId.TId = byId.TId.Substring(0, byId.TId.IndexOf("#") == -1 ? byId.TId.Length : byId.TId.IndexOf("#")) + "#" + strNo; // 
                }
                byId.FanState = 1;
                byId.FanAmount = Convert.ToDecimal(0.01);
                byId.IsFreight = 0;
            }
            //////////////////////////////
            this.CreateNewOrder(byId, 1);
            byId.IsRepeat = 1;
            foreach (OrderProductType type2 in list)
            {
                type2.OId = byId.Id;
                type2.OrderNo = byId.OrderNo;
                base.NSession.Save(type2);
                base.NSession.Flush();
            }


            this.SetQuestionOrder("订单重发", byId, "");
            // 计算缺货状态
            OrderHelper.SetQueOrder(byId, NSession);
            // 计算订单财务数据
            OrderHelper.ReckonFinance(byId, NSession);
            LoggerUtil.GetOrderRecord(byId, "重发！", "将订单重发！原因:" + m, base.CurrentUser, base.NSession);
            return base.Json(new { IsSuccess = true });
        }

        [HttpPost]
        public JsonResult SendByXuJia(int a)
        {
            OrderType orderType = Get<OrderType>(a);
            if (orderType != null)
            {
                if (orderType.Status != "已发货")
                {
                    orderType.Status = "已发货";
                    orderType.IsOutOfStock = 0;
                    orderType.IsXu = 1;
                    using (ITransaction tx = NSession.BeginTransaction())
                    {
                        try
                        {
                            base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + a).ExecuteUpdate();
                            base.NSession.CreateQuery("update OrderType set  IsFreight=1,Freight=0 where id=" + a).ExecuteUpdate();
                            tx.Commit();
                        }
                        catch (HibernateException)
                        {
                            tx.Rollback();
                            return base.Json(new { IsSuccess = false, Msg = "订单虚假发货失败" }); ;
                        }
                    }
                    LoggerUtil.GetOrderRecord(orderType, "虚假发货！", "将订单虚假发货！", base.CurrentUser, base.NSession);
                    UploadTrackCode(orderType, NSession);
                    return base.Json(new { IsSuccess = true, Msg = "订单虚假发货成功" });
                }
                return base.Json(new { IsSuccess = false, Msg = "订单已经发货" });
            }
            return base.Json(new { IsSuccess = false, Msg = "订单没有找到" });
        }

        /// <summary>
        /// 重新发货
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditReSend(string o)
        {
            // 只允许仓库或admin操作
            if (GetCurrentAccount().Username != "admin" || GetCurrentAccount().RoleName != "仓管")
            {
                IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
                foreach (OrderType type in list)
                {
                    if (type.Status == OrderStatusEnum.已发货.ToString())
                    {
                        LoggerUtil.GetOrderRecord(type, "重新发货！", "将订单从已发货的订单中转为 已处理,重新发货！", base.CurrentUser, base.NSession);
                        if (type.IsXu == 1)
                        {
                            LoggerUtil.GetOrderRecord(type, "虚假发货取消！", "因为重新发货 所以订单需要取消虚假发货状态！", base.CurrentUser, base.NSession);
                            type.IsXu = 0;
                        }
                        else if (type.IsFBA == 1)
                        {
                            LoggerUtil.GetOrderRecord(type, "FBA发货取消！", "因为重新发货 所以订单需要取消FBA发货！", base.CurrentUser, base.NSession);
                            type.IsFBA = 0;
                            type.FBABy = "";
                        }
                        else
                        {
                            IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                            foreach (OrderProductType type2 in list2)
                            {
                                IList<StockOutType> stockOutTypes = NSession.CreateQuery(" from StockOutType where Enabled=0 and OrderNo='" + type.OrderNo + "'").List<StockOutType>();
                                type2.IsQue = 3;
                                base.NSession.SaveOrUpdate(type2);
                                base.NSession.Flush();

                                foreach (var stockOutType in stockOutTypes)
                                {
                                    stockOutType.Enabled = 1;
                                    base.NSession.Update(stockOutType);
                                    base.NSession.Flush();
                                    int sid = Utilities.StockIn(stockOutType.WId, stockOutType.SKU.Trim(), stockOutType.Qty, Utilities.ToDouble(stockOutType.Price), "重新发货", base.CurrentUser.Realname, "",
                                                        base.NSession, true, false);
                                    Utilities.CreateSKUCode(stockOutType.SKU.Trim(), stockOutType.Qty, "", sid.ToString(), NSession);
                                }

                            }
                        }

                        type.Status = OrderStatusEnum.已处理.ToString();
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }
                    // 计算缺货状态
                    OrderHelper.SetQueOrder(type, NSession);
                }
                return base.Json(new { IsSuccess = true });
            }
            else
            {
                return base.Json(new { IsSuccess = false, Message = "您无操作权限!" });
            }
        }

        [HttpPost]
        public JsonResult EditUnPrint(string o)
        {
            IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
            foreach (OrderType type in list)
            {


                LoggerUtil.GetOrderRecord(type, "设置为未打印！", "将订单设置为未打印！", base.CurrentUser, base.NSession);
                type.IsPrint = 0;
                base.NSession.Save(type);
                base.NSession.Flush();

            }
            return base.Json(new { IsSuccess = true });
        }

        [HttpPost]
        public JsonResult EditUnIsDao(string o)
        {
            IList<OrderType> list = base.NSession.CreateQuery(" from OrderType where Id in(" + o + ")").List<OrderType>();
            foreach (OrderType type in list)
            {


                LoggerUtil.GetOrderRecord(type, "同意撤回海外仓！", "将订单设置为允许撤回海外仓！", base.CurrentUser, base.NSession);
                type.IsDao= 0;
                base.NSession.Save(type);
                base.NSession.Flush();

            }
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 速卖通催款
        /// </summary>
        /// <param name="Platform"></param>
        /// <param name="Account"></param>
        /// <param name="st"></param>
        /// <param name="et"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DoAliCui(int a, DateTime st, DateTime et, string c, string s)
        {
            var account = NSession.Get<AccountType>(Convert.ToInt32(a));
            if (s == null)
                s = "PLACE_ORDER_SUCCESS";
            string token = AliUtil.RefreshToken(account);
            AliOrderListType aliOrderList = null;
            int page = 1;
            do
            {
                try
                {
                    aliOrderList = AliUtil.findOrderListQuery(account.ApiKey, account.ApiSecret, token, page, st.ToString("yyyy-MM-dd HH:mm:ss"), et.ToString("yyyy-MM-dd HH:mm:ss"), s);
                    if (aliOrderList.totalItem != 0)
                    {

                        foreach (var o in aliOrderList.orderList)
                        {
                            AliUtil.AddOrderMessage(account.ApiKey, account.ApiSecret, token, o.orderId, c, o.buyerLoginId);
                        }
                        page++;
                    }
                }
                catch (Exception)
                {

                    continue;
                }

            } while (aliOrderList.totalItem > (page - 1) * 50);
            return Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        /// 缺货拆分
        /// </summary>
        /// <param name="o"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditSplitOrder(string o, string rows)
        {
            base.NSession.Clear();
            OrderType byId = this.GetById(Utilities.ToInt(o));
            byId.IsSplit = 1;
            base.NSession.Update(byId);
            base.NSession.Flush();
            List<OrderProductType> list = JsonConvert.DeserializeObject<List<OrderProductType>>(rows);
            LoggerUtil.GetOrderRecord(byId, "拆分订单！", "将订单拆分！", base.CurrentUser, base.NSession);
            //////////////////////////////
            string OrderExNoTmp = this.GetById(Utilities.ToInt(o)).OrderExNo;
            if (OrderExNoTmp.IndexOf("*") > 0)
            {
                OrderExNoTmp = OrderExNoTmp.Substring(0, OrderExNoTmp.IndexOf("*"));
            }
            string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + OrderExNoTmp + "%' order by id desc")).UniqueResult());
            //string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + this.GetById(Utilities.ToInt(o)).OrderExNo + "%' order by id desc")).UniqueResult());
            if (strNo.IndexOf("*") == -1)
            {
                // 未拆分过默认流水号为1
                strNo = "1";
                byId.OrderExNo = byId.OrderExNo + "*" + strNo;  // 查询拆分订单号并按流水号拼接
                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                if (byId.Platform == "Ebay")
                {
                    byId.TId = byId.TId + "*" + strNo;
                }
                byId.FanState = 1;//被拆分订单自动标记为已收汇
                byId.FanAmount = Convert.ToDecimal(0.001);

            }
            else
            {
                strNo = strNo.Substring(strNo.IndexOf("*") + 1);
                strNo = (Int32.Parse(strNo) + 1).ToString();
                byId.OrderExNo = byId.OrderExNo.Substring(0, byId.OrderExNo.IndexOf("*") == -1 ? byId.OrderExNo.Length : byId.OrderExNo.IndexOf("*")) + "*" + strNo;
                //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                if (byId.Platform == "Ebay")
                {
                    byId.TId = byId.TId.Substring(0, byId.TId.IndexOf("*") == -1 ? byId.TId.Length : byId.TId.IndexOf("*")) + "*" + strNo; // 
                }
                byId.FanState = 1; //被拆分订单自动标记为已收汇
                byId.FanAmount = Convert.ToDecimal(0.001);
            }
            //////////////////////////////
            base.NSession.Clear();
            this.CreateNewOrder(byId, 0);
            //OrderHelper.SaveAmount(byId, base.NSession);
            foreach (OrderProductType type2 in list)
            {
                base.NSession.Clear();
                OrderProductType type3 = base.NSession.Get<OrderProductType>(type2.Id);
                type3.Qty = type3.Qty - type2.Qty;
                if (type3.Qty != 0)
                {
                    base.NSession.Update(type3);
                }
                else
                {
                    base.NSession.Delete(type3);
                }
                base.NSession.Flush();
                base.NSession.Clear();
                type2.Id = 0;
                type2.OId = byId.Id;
                type2.OrderNo = byId.OrderNo;
                base.NSession.Save(type2);
                base.NSession.Flush();
            }
            // 获取流水号

            // 计算订单财务数据
            OrderHelper.ReckonFinance(byId, base.NSession);
            // 计算缺货状态
            OrderHelper.SetQueOrder(byId, NSession);
            // 计算订单财务数据
            OrderType oldbyId = this.GetById(Utilities.ToInt(o));
            OrderHelper.ReckonFinance(oldbyId, base.NSession);
            // 计算缺货状态
            OrderHelper.SetQueOrder(oldbyId, NSession);
            //oldbyId.OrderExNo = oldbyId.OrderExNo + "-1";

            OrderHelper.SetQueOrder(GetById(Utilities.ToInt(o)), NSession);
            OrderHelper.SetQueOrder(byId, NSession);
            LoggerUtil.GetOrderRecord(byId, "拆分订单！", "拆分新建！", base.CurrentUser, base.NSession);
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 发货拆分
        /// </summary>
        /// <param name="o"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditSplitSendOrder(string o, int c)
        {
            base.NSession.Clear();
            OrderType byId = this.GetById(Utilities.ToInt(o));
            byId.IsSplit = 1;
            base.NSession.Update(byId);
            base.NSession.Flush();
            for (int i = 0; i < c; i++)
            {
                base.NSession.Clear();
                byId.IsSplit = 1;
                byId.RMB = 0.0;
                byId.Amount = 0.0;
                byId.OrderNo = Utilities.GetOrderNo(base.NSession);
                byId.TrackCode = Utilities.GetTrackCode(base.NSession, byId.LogisticMode);
                byId.MId = Utilities.ToInt(o);

                //////////////////////////////
                string OrderExNoTmp = byId.OrderExNo;
                if (OrderExNoTmp.IndexOf("*") > 0)
                {
                    OrderExNoTmp = OrderExNoTmp.Substring(0, OrderExNoTmp.IndexOf("*"));
                }
                string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + OrderExNoTmp + "%' order by id desc")).UniqueResult());
                //string strNo = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select top 1 OrderExNo from Orders where OrderExNo like '%" + this.GetById(Utilities.ToInt(o)).OrderExNo + "%' order by id desc")).UniqueResult());
                if (strNo.IndexOf("*") == -1)
                {
                    // 未拆分过默认流水号为1
                    strNo = "1";
                    byId.OrderExNo = byId.OrderExNo + "*" + strNo;  // 查询拆分订单号并按流水号拼接
                    //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                    if (byId.Platform == "Ebay")
                    {
                        byId.TId = byId.TId + "*" + strNo;
                    }
                    byId.FanState = 1;//被拆分订单自动标记为已收汇
                    byId.FanAmount = Convert.ToDecimal(0.001);

                }
                else
                {
                    strNo = strNo.Substring(strNo.IndexOf("*") + 1);
                    strNo = (Int32.Parse(strNo) + 1).ToString();
                    byId.OrderExNo = byId.OrderExNo.Substring(0, byId.OrderExNo.IndexOf("*") == -1 ? byId.OrderExNo.Length : byId.OrderExNo.IndexOf("*")) + "*" + strNo;
                    //ebay订单来“流水交易号”遇拆分时“外部订单号”数据项后面做*1*2的设置
                    if (byId.Platform == "Ebay")
                    {
                        byId.TId = byId.TId.Substring(0, byId.TId.IndexOf("*") == -1 ? byId.TId.Length : byId.TId.IndexOf("*")) + "*" + strNo; // 
                    }
                    byId.FanState = 1; //被拆分订单自动标记为已收汇
                    byId.FanAmount = Convert.ToDecimal(0.001);
                }
                //////////////////////////////
                // 计算订单财务数据
                OrderHelper.ReckonFinance(byId, base.NSession);
                base.NSession.Save(byId);
                base.NSession.Flush();
                LoggerUtil.GetOrderRecord(byId, "发货拆分订单！", "拆分新建！", base.CurrentUser, base.NSession);
            }
            return base.Json(new { IsSuccess = true });
        }

        public ActionResult EditSplitZu(string o)
        {
            OrderHelper.SplitProduct(base.NSession.CreateQuery("from OrderProductType where OId In (" + o + ")").List<OrderProductType>(), base.NSession);
            return base.Json(new { IsSuccess = true });
        }


        [HttpPost]
        public ActionResult ExportOrder3(string o, string dd, int c, string search)
        {
            var sb = new StringBuilder();
            string sql = "";
            if (c == 0)
            {
                /*  sql =
                       @"select '' as '记录号',O.OrderNo,OrderExNo,O.TId as 流水交易号,CurrencyCode,Amount,OrderCurrencyCode,OrderFees,OrderCurrencyCode2,OrderFees2,TId,BuyerName,BuyerEmail,LogisticMode,Country,O.Weight,TrackCode,OP.SKU,OP.Qty,0.00 as 'Price',OP.Standard,0.00 as 'TotalPrice',O.Freight,O.CreateOn,O.ScanningOn,O.ScanningBy,O.Account,cast(O.IsSplit as nvarchar) as '拆分',cast(O.IsRepeat as nvarchar) as '重发' ,O.BuyerName,(select top 1 AreaName from [LogisticsArea] where LId = 
  (select top 1 ParentID from LogisticsMode where LogisticsCode=O.LogisticMode)  
  and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode 
   and CountryCode in (select ID from Country where ECountry=O.Country))) as 'AreaName' from Orders O left join OrderProducts OP ON O.Id =OP.OId ";*/
                sql =
                    @"select '' as '记录号',O.OrderNo,SUBSTRING(OrderExNo,CHARINDEX('_',orderexno)+1,len(orderexno)) as OrderExNo,OrderExNo as '带店铺号的外部订单号',O.TId as 流水交易号,CurrencyCode,Amount,OrderCurrencyCode,OrderFees,OrderCurrencyCode2,OrderFees2,TId,BuyerName as'买家姓名',BuyerEmail,LogisticMode,
OA.Country as'收件人国家',OA.City as'收件人城市',OA.Street as'收件人街道',OA.Province as'收件人省州', OA.PostCode as '收件人邮编',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.Addressee as '收件人',O.Weight,TrackCode,OP.ExSKU as '物品平台编号',OP.SKU,OP.ImgUrl as '物品图片网址',OP.Qty as '物品数量',0.00 as 'Price',OP.Standard,0.00 as 'TotalPrice',O.Freight,O.CreateOn,O.ScanningOn,O.ScanningBy,O.Account,cast(O.IsSplit as nvarchar) as '拆分',cast(O.IsRepeat as nvarchar) as '重发' ,(select top 1 AreaName from [LogisticsArea] where LId = 
(select top 1 ParentID from LogisticsMode where LogisticsCode=O.LogisticMode)  
and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode 
 and CountryCode in (select ID from Country where ECountry=O.Country))) as 'AreaName',
   (select top 1 Price  from  StockOut S  where S.OrderNo=O.OrderNo and S.SKU=OP.SKU) as '成本',
'Status:'+ case IsQue when 1 then '缺货 库存:' + rtrim((select isnull(sum(Qty),0) from WarehouseStock where SKU =OP.SKU)) + ' ' + (case when (select count(1) from PurchasePlan where SKU =OP.SKU and Status not in('异常','已收到','失效','已退款'))>0 then (select top 1 convert(varchar(10),ExpectReceiveOn,120) from PurchasePlan where SKU =OP.SKU and Status not in('异常','已收到','失效','已退款')  Order By ExpectReceiveOn desc) + ' 预计到货:'+ rtrim((select sum(Qty) from PurchasePlan where SKU =OP.SKU and Status not in('异常','已收到','失效','已退款'))) else '近期没有物品到货' end) when 2 then '停产' when 3 then '占用库存' else ''end as '备注'
from Orders O left join OrderProducts OP ON O.Id =OP.OId left join OrderAddress OA on O.AddressId=OA.Id";
            }
            if (c == 1)
            {
                sql =
                 @"select  
                    O.OrderExNo as '内单号',
                    O.TrackCode as '转单号',
                    'RU' as  '目的地',
                    'RU' as  '收件国家',
                    OA.Province as '收件省州',
                    OA.City as '收件城市',
                    OA.Addressee as '收件人',
                    OA.PostCode as '收件邮编', 
                    isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',
                    isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话', 
                    (select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述',         
                    case when O.Weight =0  then 0.3 else O.Weight/1000.0  end as '重量',    
                    OP.Title as '物品别名',
                    OP.Qty as '物品数量',
                    O.ProductFees as '声明价值', 
                    O.ProductFees as '投保价值',           
                    isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话'
                    from Orders O  
                    left join OrderProducts OP on O.Id=OP.OId  
                    left join OrderAddress OA on O.AddressId=OA.Id 
                    left join Country C On O.Country=C.ECountry";

            }
            if (c == 2)
            {
                sql =
               @"select  
               OP.SKU as 'SKU',
			   sum(OP.Qty) as '销售数',
			   count(O.OrderNo) as '订单数',
			   P.Location as '库位',
			   P.ProductName as '商品名称'
                    from Orders O  
                    left join OrderProducts OP on O.Id=OP.OId  
					left join Products P on  P.SKU=OP.SKU
					where O.IsError =0 and O.Account like 'yw%'  
					and  O.Status='已处理' and O.IsOutOfStock=0 and O.Enabled = 1  and  O.IsPrint<=0
					group by OP.SKU,P.Location,P.ProductName order by sum(OP.Qty) desc";
                DataSet dss = GetOrderExportDe(sql);

                // 设置编码和附件格式 
                Session["ExportDown"] = ExcelHelper.GetExcelXml(dss);
                return Json(new { IsSuccess = true });

            }
            if (c == 3)
            {
                sql = @"select o.orderno '订单号',Freight 运费,0 as '保费', Addressee 收货人姓名, tel 联系手机,d.Country 国家,d.Province 省,d.City 市,	 Street 收货地址,o.Weight 毛重,d.Country '运抵国（地区）','' as '指运港',p.SKU as '商品备案号',p.Qty as '数量',pr.InvoicePrice as '商品单价',o.CreateOn '订单创建时间',o.CreateOn as '订单付款时间', account as '店铺名称',0 as '优惠金额',o.LogisticMode as '物流公司',d.PostCode as '邮编' ,TrackCode as '运单号',1 as '件数',o.CurrencyCode as '商品币种','人民币' as '运费币种'  from Orders o left join OrderAddress d on o.AddressId=d.id 
 left join OrderProducts p on o.OrderNo=p.OrderNo 
 left join Products pr on p.SKU= pr.SKU  ";
            }
            if (string.IsNullOrEmpty(o))
            {
                /*if (!string.IsNullOrEmpty(s))
                {
                    sql += " where O.IsError =0 and  O.Enabled=1 and O.Status='" + s + "'  and  O." + dd + " between '" + st + "' and '" + et + "'";
                    if (!string.IsNullOrEmpty(a))
                    {
                        sql += "and O.Account='" + a + "'";
                    }
                    if (!string.IsNullOrEmpty(p))
                    {
                        sql += "and O.Platform='" + p + "'";
                    }
                    if (!string.IsNullOrEmpty(que))
                    {
                        sql += "and O.IsOutOfStock='" + que + "'";
                    }
                }
                else
                {
                    sql += " where O.IsError =0 and  O.Enabled=1 and   O." + dd + " between '" +
                           st + "' and '" + et + "'";
                    if (!string.IsNullOrEmpty(a))
                    {
                        sql += "and O.Account='" + a + "'";
                    }
                    if (!string.IsNullOrEmpty(que))
                    {
                        sql += "and O.IsOutOfStock='" + que + "'";
                    }
                }*/
                //add by luoyunqing 20160926订单导出增加查询条件，同搜索的查询条件一样
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Replace('=', '&');
                    string str = Utilities.Resolve(search, true);
                    // 过滤地区条件
                    string area = "全部";
                    if (str.Contains("宁波"))
                    {
                        area = "宁波";
                    }
                    if (str.Contains("义乌"))
                    {
                        area = "义乌";
                    }
                    // 移除where语句内条件
                    str = str.Replace(" and FromArea = '宁波'", "").Replace(" and FromArea = '义乌'", "");
                    //str = str;

                    string acs = "";
                    foreach (AccountType ac in GetCurrentAccount().Accounts)
                    {
                        if (area != "全部")
                        {
                            if (ac.FromArea == area)
                            {
                                acs += "'" + ac.AccountName + "',";
                            }
                        }
                        else
                        {
                            acs += "'" + ac.AccountName + "',";
                        }
                    }
                    acs = acs.Trim(',');

                    if (acs.Length > 0)
                    {
                        str += " and Account in (" + acs + ")";
                    }
                    else if (acs.Length == 0)
                    {
                        str += " and Account in ('')";
                    }


                    if (str.Length > 0)
                    {
                        sql += " where O.IsError =0  and  O.OrderNo in (select OrderNo from orders where Enabled=1  and " + str + ")";
                    }
                    if (sql.Contains("OrderProductType"))
                    {
                        sql = sql.Replace("OrderProductType", "OrderProducts");
                    }
                    if (c == 3)
                    {
                        sql += " and  pr.IsOpenInvoice=1 and o.Status='已发货'";
                    }
                }

            }
            else
            {
                sql += " where O.IsError =0 and   O.Id in(" + o + ")";
            }
            DataSet ds = GetOrderExport(sql);

            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 待处理订单导出
        /// </summary>
        /// <param name="type"></param>
        /// <param name="StrNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExportUnhandler(string o, string search)
        {
            //string sql = "select OrderNo as 订单编号  ,Status 订单状态  ,CurrencyCode 	as 货币,Weight 预估重量 ,BuyerName 买家  ,Country 国家,LogisticMode 发货方式 ,GenerateOn 时间  ,Account 账户  ,ErrorInfo 错误信息   from Orders O";
            string sql = "select CONVERT(varchar(100), O.GenerateOn, 23)as'日期',P.ProductName as '品名', OP.SKU as 'SKU',O.OrderExNo as '订单号',OP.Qty as '数量',case when O.Amount=0 then 0 else OP.Price end as'售卖单价',O.Amount as '订单金额',O.CurrencyCode as'订单货币种类', OA.Province as '所在州',OA.PostCode as '邮编',O.Account as '销售账户',O.FBABy as'发货仓库',O.OrderNo as '系统单号',O.TId as'流水交易号' from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU";
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace('=', '&');
                string str = Utilities.Resolve(search, true);

                string acs = "";
                foreach (AccountType ac in GetCurrentAccount().Accounts)
                {
                    acs += "'" + ac.AccountName + "',";
                }
                acs = acs.Trim(',');
                if (acs.Length > 0)
                {
                    if (str.Length > 0)
                    {
                        str += " and Account in (" + acs + ")";
                    }
                    else
                    {
                        str += "  Account in (" + acs + ")";
                    }
                }
                else if (acs.Length == 0)
                {
                    if (str.Length > 0)
                    {
                        str += " and Account in ('')";
                    }
                    else
                    {
                        str += "  Account in ('')";
                    }
                }


                if (str.Length > 0)
                {
                    sql += " where O.IsError =0  and  O.Enabled=1  and   O.Status='待处理' and " + str;
                }
            }
            else
            {
                sql += " where O.IsError =0 and     O.Enabled=1 and  O.Status='待处理' and O.Id in(" + o + ")";
            }
            DataSet ds = Utilities.GetDataSet1(sql, NSession);

            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return base.Json(new { IsSuccess = true });


        }
        [HttpPost]
        public ActionResult ExportOrderInput(string type, string StrNo)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder builder = new StringBuilder();
            str.Append("select  Platform 平台,Account 帐号,OrderNo 系统单号,OrderExNo 平台单号,TrackCode 跟踪码,BuyerName 买家,Country 国家,Status 订单状态 from Orders ");
            if (type == "OrderNo")
            {
                str.Append(" where OrderNo in(" + StrNo + ")");
            }
            else if (type == "OrderExNo")
            {
                str.Append(" where OrderExNo in (" + StrNo + ")");
            }
            else if (type == "TrackCode")
            {
                str.Append(" where TrackCode in (" + StrNo + ")");
            }
            else
            {
                str.Append(" where TId in (" + StrNo + ")");
            }
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str.ToString();
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(dataSet);

            return base.Json(new { IsSuccess = true });
        }
        [HttpPost]
        public ActionResult ExportBiLiShi(string o)
        {
            StringBuilder builder = new StringBuilder();
            string str = "select  * from Orders where O.Id in(" + o + ")";
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(dataSet);
            return base.Json(new { IsSuccess = true });
        }

        [HttpGet]
        public ActionResult ExportDown(string Id)
        {
            string str = "";
            object sb = Session["ExportDown"];
            if (sb != null)
            {
                str = sb.ToString();
            }
            if (Id == null)
            {
                System.Web.HttpContext.Current.Response.ContentType = "text/plain";
                System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                System.Web.HttpContext.Current.Response.Charset = "gb2312";
                System.Web.HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=zm.txt");
                return File(Encoding.UTF8.GetBytes(str), "attachment;filename=zm.txt");
            }
            else
            {
                System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                System.Web.HttpContext.Current.Response.Charset = "gb2312";
                System.Web.HttpContext.Current.Response.AppendHeader("Content-Disposition",
                                                                     "attachment;filename=" +
                                                                     DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                return File(Encoding.UTF8.GetBytes(str),
                            "attachment;filename=" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="st">开始时间</param>
        /// <param name="et">结束时间</param>
        /// <param name="d">数据</param>
        /// <param name="f">字段</param>
        /// <param name="t">类型</param>
        /// <param name="a">地区</param>
        /// <param name="c">物流类型</param>
        /// <param name="s">查询字段</param>
        /// <param name="p">平台</param>
        /// <param name="aa">账户</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportOrder(DateTime st, DateTime et, string d, string f, int t, int a, int c, string s, string p, string aa)
        {
            string str3;
            string sql = "";
            string sql2 = "";
            if (c == 1)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '寄达国家（中文）',O.Country as '寄达国家（英文）',OA.Province as '州名',OA.City as '城市名',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',OA.Addressee as '收件人姓名',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话','High-tech zone，Juxian Road 399, Building B1 20th, Ningbo, ZheJiang,China' as '寄件人详细地址（英文）','VIKI' as '寄件人姓名','0574-27903940' as '寄件人电话','1' as '内件类型代码',isnull(OA.PostCode,'') as '邮政编码' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select TrackCode as '跟踪号','物品' as '物品中文名称',OP.Title as '物品英文名称(不能超过50个字符）',OP.Qty as '数量',o.weight as '单件重量',10 as '单价','China' as '原产地' from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 2)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '目的地','1' as '件数',OA.Addressee as '收件人',isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',OA.PostCode as '收件邮编',O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市','' as '物品描述','1' as '物品数量','8' as '物品单价','8' as '声明价值',O.Weight as '重量' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
            }
            else if (c == 3)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '寄达国家（中文）',O.Country as '寄达国家（英文）',OA.Province as '州名',OA.City as '城市名',OA.PostCode as '邮编',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',OA.Addressee as '收件人姓名',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话','High-tech zone，Juxian Road 399, Building B1 20th, Ningbo, ZheJiang,China' as '寄件人详细地址（英文）','VIKI' as '寄件人姓名','0574-27903940' as '寄件人电话','1' as '内件类型代码' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select TrackCode as '跟踪号','物品' as '物品中文名称',OP.Title as '物品英文名称',OP.Qty as '数量',o.weight as '单件重量',10 as '单价','China' as '原产地' from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 4)
            {
                //	收件人姓名		城市	收件人地址	收件人电话	收件人邮编	海关报关品名1	申报品数量1

                sql = "select  O.OrderExNo as '客户单号',O.TrackCode as '服务商单号', '俄速通' as '运输方式', 'russian federation' as '目的国家','' as '寄件人公司名','' as '寄件人姓名','' as '寄件人地址','' as '寄件人电话','' as '寄件人邮编','' as '寄件人传真','' as '收件人公司名',OA.Addressee as '收件人姓名',OA.Province as '州 \\ 省',OA.City as '城市',isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'')  as '收件人地址', isnull(OA.Tel, '')+'/'+ isnull(OA.Phone,'') as '收件人电话',OA.Email as '收件人邮箱',OA.PostCode as '收件人邮编', '' as '收件人传真',O.BuyerName as '买家ID', '' as '交易ID','' as '保险类型', '' as '投保金额', '' as '订单备注',O.Weight as '重量', '' as '是否退件','gift' as '海关报关品名1','' as '配货信息1','5' as '申报价值1','1' as '申报品数量1' ,'' as '配货备注1','' as '海关报关品名2','' as '配货信息2','' as '申报价值2','' as '申报品数量2' ,'' as '配货备注2','' as '海关报关品名3','' as '配货信息3','' as '申报价值3','' as '申报品数量3' ,'' as '配货备注3','' as '海关报关品名4','' as '配货信息4','' as '申报价值4','' as '申报品数量4' ,'' as '配货备注4','' as '海关报关品名5','' as '配货信息5','' as '申报价值5','' as '申报品数量5' ,'' as '配货备注5','' as '英文名'from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry  ";

            }
            else if (c == 5)
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话','俄罗斯联邦' as '目的地', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址','挂号小包' as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述',O.Weight/1000.0 as '重量','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 6)//福建小包
            {
                //																	
                sql = "select  O.TrackCode as '记录序号（为订单号，唯一值）',OA.PostCode as '寄达局邮编',C.CCountry as '寄达局名称' ,OA.Addressee as '收件人姓名(国际邮件填英文)',OA.Street+' '+OA.City+' '+OA.Province+' '+OA.Country as '收件人地址(国际邮件填英文)',isnull( OA.Tel,'')+'/'+isnull(OA.Phone,'') as '收件人电话',O.Weight as '邮件重量', '' as '邮件备注', OA.Country as '英文国家名',OA.Province as '英文州名',OA.City as '英文城市名',OA.PostCode as '收件人邮编','1' as '内件类型代码',P.Category as '内件名称',OP.Title as '内件英文名称',O.Weight as '内件重量', '1' as '内件数量','5' as '单价','CHINA' as '产地', '' as '物品英文说明','' as '内件成分说明' from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Products P On OP.SKU=P.SKU left join Country C On O.Country=C.ECountry  ";

            }
            else if (c == 7)//义乌小包 （商）
            {
                sql = "select  O.OrderNo as '自编号',O.TrackCode as '运单号',C.CCountry as '目的地',OA.Addressee as '收件人', OA.Street as '收件地址',isnull( OA.Tel,'')+'('+isnull(OA.Phone,'')+')' as '收件电话',OA.PostCode as '收件邮编', OA.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市', (select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU)) as '物品描述' ,1 as '物品数量', 2 as '物品单价',OP.Title as '物品别名'from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry ";

            }
            else if (c == 8)//荷兰小包
            {
                sql = "select O.OrderNo as '参考号',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU)) as '物品描述',OA.Addressee as '收件人',O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.Street as '收件地址',OA.PostCode as '收件邮编',isnull( OA.Tel,'')+'('+isnull(OA.Phone,'')+')' as '收件电话', '荷兰挂号小包' as '网络渠道',5 as '声明价值' ,'NLPG' as '简码',O.TrackCode as '运单号' from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry ";

            }
            else if (c == 9)//荷兰小包
            {
                sql = "select TrackCode,CONVERT(varchar(10) , ScanningOn, 120 ),Country,Weight,Freight from Orders O ";
            }
            else if (c == 10)//欧亚速运
            {
                sql = @"select O.OrderNo as '订单号','http://122.227.179.98:848'+P.PicUrl as '产品图片',P.SKU as '产品编码',P.ProductName as '中文品名','' as '材质',OP.Qty as '数量',10 as '报关金额',p.Weight as '重量',OA.Addressee as '收货人名称',
isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'') as '收货地址',
isnull(OA.PostCode,'') as '邮编',
OA.Tel as '联系电话',OA.Phone as '手机',O.TrackCode as '实际发货物流:运单号'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry";

            }
            else if (c == 11)//EUB
            {
                sql = @"select O.OrderNo as '客户单号',O.TrackCode as '转单号', '苏邮（EUB）'as '运输方式' ,O.Country as '目的国家', OA.Addressee as '收件人姓名',
OA.Province as '州,省',OA.City as '城市', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+OA.PostCode+','+isnull(OA.Country,'') as '联系地址',
OA.Tel as '收件人电话','' as '收件人邮箱', OA.PostCode as '收件人邮编',O.Weight as '重量', PC.EName as '海关报关品名1', PC.Name '配货信息1',5 as '申报价值1', 1 as '申报品数量1'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join ProductCategory  PC On P.Category=PC.Name ";

            }
            else if (c == 12)//KS仓库发货数据
            {
                //                sql = @" SELECT O.OrderNo as 系统编号,O.TId as 流水交易号,CONVERT(VARCHAR(10),O.GenerateOn,120) as '日期/date', OP.SKU as '货号/SKU', O.OrderExNo as 'order-ID' ,
                //'' as 'User Id',OA.Addressee as 'Buyer Fullname' ,OA.Phone as 'Buyer Phone Number' ,OA.Street as 'Buyer Address' ,
                //OA.City as 'Buyer City',OA.Province as 'State',OA.PostCode as 'Buyer Zip',OA.Country as 'Buyer Country', P.Weight*OP.Qty as 'Gross Weight (LB)',
                //'' as 'Tracking numnber','' as '备注',OP.Qty as '数量'
                // from Orders O 
                // left join OrderProducts OP on O.Id=OP.OId  
                // left join Products P on OP.SKU=P.SKU  
                // left join OrderAddress OA on O.AddressId=OA.Id ";
                //                //left join ProductCategory  PC On P.Category=PC.Name "; //联接后有重复，原因分类里有重复比如:"家居"

                sql = @"SELECT '' as 订单来源,'' as 店铺名,o.buyerid as 买家ID,
substring(OrderExNo,CHARINDEX('_',OrderExNo)+1,LEN(OrderExNo)) as 店铺单号,OrderExNo as '带店铺号的外部订单号',O.OrderNo as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址, '' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,P.SKU AS SKU,op.Qty AS 数量,o.Amount 订单金额,o.TId 流水交易号
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 13)//俄罗斯海外仓
            {
                sql = @"select P.SKU as 'Артикул / Article','http://122.227.179.98:848'+P.PicUrl as 'Фото / Photo',OP.Qty as 'Количество товара / Quantity of goods',isnull(OP.Remark,'') as 'Примечание / Remarks',O.OrderExNo as 'Номер заказа / Order number',isnull(OA.City,'') as 'Город / City',isnull(OA.Street,'') as 'Улица, дом, квартира / Street, home, appartment',isnull(OA.Province,'') as 'Область, район. / region, district',OA.Addressee as 'Ф.И.О. / Name',isnull(OA.PostCode,'') as 'Индекс / Postcode',
OA.Tel as 'Телефон / Telephone number',OA.Phone as 'Телефон / Telephone number',O.Account as 'Store',O.TrackCode as 'TrackCode'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry ";
            }
            else if (c == 14) //C组海外仓日销
            {
                sql = @"select  replace(CONVERT(varchar, O.GenerateOn, 102 ),'.','-') as '日期',P.ProductName as'品名',OP.SKU as 'SKU',O.OrderExNo as '订单号',O.OrderNo as '自编号',OP.Qty as '数量', OP.Price as '售卖单价' ,O.Amount  as '订单金额', 
OA.Province as '所在州', OA.PostCode as '邮编',O.Freight as '运费','' as '平台费','' as '到仓价', O.profit as '利润',
O.ProfitRate as '利润率', O.FBABy as '备注',O.Account as '平台'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join ProductCategory  PC On P.Category=PC.Name ";
            }
            else if (c == 15)//线上发货导出
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',O.LogisticMode as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述',O.Weight/1000.0 as '重量','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                //  sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 16) // 亚欧快运（宁波）
            {
                sql = "select O.TrackCode as '客户单号',''as '服务商单号','AK' as 运输方式,O.Country as '目的国家','' as '寄件人公司名','lvjingjing' as 寄件人姓名,'zhejiang' as '寄件人省','ningbo' as '寄件人城市','3F,NO.4 Building,JUNSHENG Group,1266,Juxian Road, National Hi-Tech Zone' as '寄件人地址','15988173792' as '寄件人电话','' as '寄件人邮编','' as '寄件人传真','' as '收件人公司名',OA.Addressee as '收件人姓名',OA.Province as '州 \\ 省',OA.City as '城市', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+OA.PostCode+','+isnull(OA.Country,'') as '联系地址',OA.Tel as '收件人电话',OA.Phone as '收件人手机',OA.Email as '收件人邮箱', OA.PostCode as '收件人邮编','' as '收件人传真','' as '买家ID','' as '交易ID','' as '保险类型','' as '保险价值','' as '订单备注','' as '重量','' as '是否退件', '5' as '申报类型',PC.EName as '海关报关品名1','' as '配货信息1',5 as '申报价值1', 1 as '申报品数量1','' as '配货备注1','' as '海关报关品名2','' as '配货信息2','' as '申报价值2','' as '申报品数量2','' as '配货备注2','' as '海关报关品名3','' as '配货信息3','' as '申报价值3','' as '申报品数量3','' as '配货备注3','' as '海关报关品名4','' as '配货信息4','' as '申报价值4','' as '申报品数量4','' as '配货备注4','' as '海关报关品名5','' as '配货信息5','' as '申报价值5','' as '申报品数量5','' as 配货备注5 from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join Products P on OP.SKU=P.SKU   left join OrderAddress OA on O.AddressId=OA.Id  left join ProductCategory  PC On P.Category=PC.Name";
            }
            else if (c == 17) // 义乌海外仓发货清单导出
            {
                sql = @"SELECT 
                        CONVERT(VARCHAR(10),O.GenerateOn,120) as 日期,
                        O.OrderExNo as 订单号,
                        O.OrderNo as 自编号,
                        ProductName as 品名,
                        P.SKU AS SKU,
                        op.Qty AS 数量,
                        op.Price 售卖单价,
                        O.amount as 订单金额,
                        OA.Province AS 所在州,
                        OA.PostCode AS 邮编,
                        cast(isnull(O.Freight,0) as money) 运费,
                        cast(a.Tax*O.amount as money) 平台费,
                        cast(p.Price/b.CurrencyValue as money) 到仓单价,
                        cast(p.Price/b.CurrencyValue*op.Qty as money) 到仓总成本,
                        cast(O.amount-O.Freight-a.Tax*O.amount-p.Price/6.5*op.Qty as money) 利润,
                        case when O.amount=0 then '0%' else convert(varchar(50),cast((O.amount-O.Freight-a.Tax*O.amount-p.Price/b.CurrencyValue*op.Qty)/O.amount*100 as money)) + '%'end 利润率,
                        FBABy 仓库,
                        Account 账号
                         from Orders O 
                         left join OrderProducts OP on O.Id=OP.OId  
                         left join Products P on OP.SKU=P.SKU  
                         left join OrderAddress OA on O.AddressId=OA.Id
                         join Account a on o.Account=a.AccountName  
                         join FixedRate b on O.CurrencyCode=b.CurrencyCode AND b.Year=(DATEPART(YEAR,O.GenerateOn)) AND b.Month=(DATEPART(Month,O.GenerateOn)) ";
            }
            else if (c == 18)//CDEK导出模板
            {
                sql = @"select  
                        O.TrackCode as '运单号',
                        O.OrderExNo as 订单号,
                        O.Weight/1000.0 as '重量',
                        (select sum(OP.Qty) from OrderProducts OP where O.Id=OP.OId) as '产品数量',  
                        O.Country as '中文品名',
                        (select top 1 Category from Products P where P.SKU=OP.SKU)  as '中文品名',
                        CONVERT(varchar(10) , O.ScanningOn, 120 ) as '发货时间' ,
                        '5' as '申报价值'
                        from Orders O 
                        left join OrderProducts OP on O.Id=OP.OId ";
            }
            else if (c == 19)//美东海外仓（CA）
            {
                sql = @"SELECT 
O.Account as 店铺,O.OrderNo as 交易ID,O.OrderExNo as 外部编号,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,'' as 收货人国家中文,'' as 币种,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址, '' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,'' AS 付款人邮箱,O.SellerMemo AS 备注,P.SKU AS SKU,'' AS 中文报关名称,'' AS 商品金额,'' AS 英文报关名称,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,
'' AS 买家ID,
'' AS 店铺运输方式,
'' AS 运费总额,
'' AS 商品itemID,
O.TId AS ebay交易ID
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }

            else if (c == 20)//美西托盘海外仓（LAI）
            {
                sql = @"SELECT 
O.OrderExNo as 订单号,O.OrderNo as ERP编号,O.Account as 店铺,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS TEL,P.SKU AS SKU,op.Qty AS 数量,O.SellerMemo AS 备注
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 21)//美西海外仓（LAI）--仅限导出一次
            {
                sql = @"SELECT 
P.SKU AS 'Item SKU',op.Qty AS 'Quantity',P.SKU + '*' +cast(op.Qty AS varchar) AS 'Label Quantity',O.TrackCode AS 'Note（Fedex跟踪号）',
O.OrderExNo as 'ERP订单号',O.OrderNo as '订单编号'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  ";
            }

            else if (c == 22)//英国仓库UKMAN
            {
                sql = @"SELECT 
O.OrderExNo as 订单号,O.OrderNo as ERP订单号,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS '收货人电话',P.SKU AS SKU,op.Qty AS 数量
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 23)//宁波美东谷仓(Gcus-East)
            {
                sql = @"SELECT 
'Gcus-East' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',(case when REVERSE(left(REVERSE(P.sku),2))='BL'  then REVERSE('KB'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))  
 when  REVERSE(left(REVERSE(P.sku),2))='WH'  then REVERSE('TW'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))
else P.SKU end) AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 24)//宁波美东(US-East)
            {
                sql = @"SELECT 
(case when O.Platform='Ebay' then  O.TId else O.OrderExNo end) as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,P.sku AS SKU,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id   ";
            }

            else if (c == 25)//宁波KS
            {
                sql = @"SELECT 
O.OrderExNo as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,(case when REVERSE(left(REVERSE(P.sku),2))='BL'  then REVERSE('KB'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))  
 when  REVERSE(left(REVERSE(P.sku),2))='WH'  then REVERSE('TW'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))
else P.SKU end) AS SKU,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id   ";
            }
            else if (c == 26)//俄罗斯海外仓旧（宁波）
            {
                sql = @"select P.SKU as 'Артикул / Article','http://122.227.179.98:848'+P.PicUrl as 'Фото / Photo',OP.Qty as 'Количество товара / Quantity of goods',isnull(OP.Remark,'') as 'Примечание / Remarks',O.TrackCode as '跟踪',O.Account as '店铺',O.CreateOn as'同步时间',O.OrderExNo as 'Номер заказа / Order number',isnull(OA.City,'') as 'Город / City',isnull(OA.Street,'') as 'Улица, дом, квартира / Street, home, appartment',isnull(OA.Province,'') as 'Область, район. / region, district',OA.Addressee as 'Ф.И.О. / Name',isnull(OA.PostCode,'') as 'Индекс / Postcode',
OA.Tel as 'Телефон / Telephone number',OA.Phone as 'Телефон / Telephone number'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry ";
            }
            else if (c == 27)
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话','俄罗斯联邦' as '目的地', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址','挂号小包' as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述',O.Weight/1000.0 as '重量','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 28)//义乌美西(董)导出--YWCA-WEST(DONG) 
            {
                sql = @"SELECT 
'USWE' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',P.SKU  AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 29)//义乌美东(LEO)导出--YWNJ-EAST(LEO)
            {
                sql = @"SELECT 
'USEA' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',P.SKU  AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id ";
            }
            else if (c == 30)//俄罗斯海外仓（宁波）
            {
                sql = @"select '' as '客户',P.SKU as 'SKU',OP.Qty as '数量',O.OrderExNo as '订单号',O.TrackCode as '跟踪',O.LogisticMode as '渠道',isnull(OA.PostCode,'') as '邮编', OA.Addressee as '收件人姓名',O.Country as '国家',OA.Province as '省州',OA.City as '城市',OA.Street as '街','' as '住房','' as '楼','' as '房子\办公室',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收货人的联系方式(手机号)',OA.Email as '收货人的邮箱','' as '额外信息',o.CreateOn as '同步时间'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id ";
            }
            else if (c == 31)
            {
                sql = "select ''as '记录序号',''as '大宗用户编号',''as '用户自编号', isnull(OA.PostCode,'') as '寄达局邮编',C.CCountry as '寄达局名称',OA.Addressee as '收件人姓名',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话', o.weight as '邮件重量',TrackCode as '邮件号码',''as '邮件备注',''as '保险金额',''as '保价金额',''as '保值金额',''as '总资费',''as '封发标志',''as '已贴票金额',''as '回持单号码',O.Country as '英文国家名',OA.Province as '英文州名',OA.City as '英文城市名','VIKI' as '寄件人姓名（英文）','ZheJiang' as '寄件人省名（英文）','Ningbo' as '寄件人城市名（英文）','High-tech zone，Juxian Road 399, Building B1 20th,Ningbo, ZheJiang,China' as '寄件人地址（英文）','0574-27903940' as '寄件人电话','1' as '内件类型代码',''as '验关报关标志',''as '验关物品类型',''as '内件名称',''as '内件数量',''as '单件重量',''as '单价',''as '产地',''as '协调号',''as '物品英文说明' ,''as '内件成分说明',''as '图像文件名' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select ''as '记录序号', TrackCode as '邮件号码','1'as '内件序号', '物品' as '内件名称',OP.Title as '内件英文名称',o.weight as '单件重量',OP.Qty as '内件数量',10 as '单价','China' as '产地' ,'' as '物品英文说明' ,'' as '内件成分说明' ,'' as '协调号码'  from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            if (t == 1)
            {
                str3 = sql;
                if (c == 13)//俄罗斯海外仓例外（追踪号为 null 导出）（有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
                else if (c == 20 || c == 22)//美西托盘海外仓（LAI）（追踪号为 null 导出，没有发货方式）、英国海外仓UKMAN
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 19)//美东海外仓（CA）,不需要选时间  （追踪号为 null 导出，没有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') ";
                }
                else if (c == 23 || c == 24 || c == 25 || c == 28 || c == 29)//宁波美东谷仓(Gcus-East)、宁波美东(US-East)结束时间选择去除 （追踪号为 null 导出，没有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn >= '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 21)//美西海外仓（LAI）--仅限导出一次--->有单号
                {
                    sql = str3 + " where  O.Status='已发货' and O.TrackCode is not null and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 27)//导出义乌俄罗斯挂号小包(已处理)
                {
                    sql = str3 + " where  O.Status='已处理' and  O.TrackCode is not null  and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else
                {
                    sql = str3 + " where  O.Status='已发货' and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
            }
            else
            {
                str3 = sql;
                if (f == "Id")
                    sql = str3 + " where  O." + f + " in (" + d + ")";
                else
                {
                    sql = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
                }
            }
            if (a == 1)
            {
                sql = sql + " and O.Account not like 'yw%'";
            }
            else if (a == 2)
            {
                sql = sql + " and O.Account like 'yw%'";
            }
            if (p != "0" && p != "ALL")
            {
                sql = sql + " and O.Platform ='" + p + "'";
            }
            if (aa != "===请选择===" && aa != "ALL")
            {
                sql = sql + " and O.Account='" + aa + "'";
            }
            if (c == 13)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'YWRU-AEA','YWRU-AEB') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 26)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'NBRU-AEA') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 30)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'NBRU-AEA') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 19)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'CA' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 23) //宁波美东谷仓(Gcus-East)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'Gcus-East' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 24)//宁波美东(US-East)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'US-East' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 28) //义乌美西(董)【YWCA-WEST(DONG)】
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'YWCA-WEST(DONG)' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 29)//义乌美东(LEO)【YWNJ-EAST(LEO)】
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'YWNJ-EAST(LEO)' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 25)//宁波KS
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'KS' and O.Enabled = 1 and O.IsAudit = 1  and O.Account  not like 'yw%'";
            }
            if (c == 20)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and  O.Amount>600";
            }
            if (c == 21) //美西海外仓（LAI）--仅限导出一次--->有单号
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and IsDao =0 ";

            }
            if (c == 22)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy = 'UKMAN' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if ((c == 15) && (s.Length > 0))
            {
                sql = sql + " and O.LogisticMode like '%" + s + "%'";
            }
            DataSet orderExport = this.GetOrderExport(sql);
            if (sql2.Length > 5)
            {
                if (t == 1)
                {
                    str3 = sql2;
                    sql2 = str3 + " where  O.Status='已发货' and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
                else
                {
                    str3 = sql2;
                    sql2 = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
                }
                if (a == 1)
                {
                    sql2 = sql2 + " and O.Account not like 'yw%'";
                }
                else if (a == 2)
                {
                    sql2 = sql2 + " and O.Account like 'yw%'";
                }
                if (aa != "===请选择===" && aa != "ALL")
                {
                    sql2 = sql2 + " and O.Account='" + aa + "'";
                }
                DataSet set2 = this.GetOrderExport(sql2);
                DataTable table = set2.Tables[0].Clone();

                List<string> liststr = new List<string>();
                foreach (DataRow row in set2.Tables[0].Rows)
                {
                    if (c == 1)
                    {
                        if (row["物品英文名称(不能超过50个字符）"].ToString().Length > 0x30)
                        {
                            row["物品英文名称(不能超过50个字符）"] = row["物品英文名称(不能超过50个字符）"].ToString().Substring(0, 0x2d);
                        }
                    }
                    if (c == 31)
                    {
                        if (row["内件英文名称"].ToString().Length > 0x30)
                        {
                            row["内件英文名称"] = row["内件英文名称"].ToString().Substring(0, 0x2d);
                        }
                    }
                    if (!liststr.Contains(row.ToString()))
                    {
                        liststr.Add(row[0].ToString());
                        table.Rows.Add(row.ItemArray);
                    }
                }
                table.TableName = "Sheet2";
                orderExport.Tables.Add(table);
                orderExport.Tables[0].TableName = "Sheet1";
            }

            try
            {
                List<string> liststr = new List<string>();

                foreach (DataRow dr in orderExport.Tables[0].Rows)
                {
                    if (c == 12 || c == 13 || c == 14 || c == 17 || c == 19 || c == 20 || c == 21 || c == 22 || c == 23 || c == 24 || c == 25 || c == 26 || c == 28 || c == 29 || c == 30)
                    {
                        break;

                    }
                    if (c == 31)
                    {
                        if (liststr.Contains(dr[9].ToString()))
                        {
                            dr.Delete();
                        }
                        else
                        {
                            liststr.Add(dr[9].ToString());
                        }
                    }
                    else
                    {
                        // 删除已存在日期行?
                        if (liststr.Contains(dr[0].ToString()))
                        {
                            dr.Delete();
                        }
                        else
                        {
                            liststr.Add(dr[0].ToString());
                        }
                    }
                }

                orderExport.Tables[0].AcceptChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(orderExport, base.GetCurrentAccount().FromArea);
            if (c == 21 )//仅导出一次
            {
                base.NSession.CreateQuery("update OrderType O set IsDao=IsDao+1 where   O.Status='已发货' and O.TrackCode is not null and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and IsDao =0 ").ExecuteUpdate();
            }
            else if (c == 13)
            {
                base.NSession.CreateQuery("update OrderType O set IsDao=IsDao+1   where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'  and O.IsFBA = 1 and O.FBABy in ( 'YWRU-AEA','YWRU-AEB') and O.Enabled = 1 and O.IsAudit = 1").ExecuteUpdate();
            }

            return base.Json(new { IsSuccess = true });
        }

        public ActionResult ExportOrderII(DateTime st, DateTime et, string d, string f, int t, int a, int c, string s, string p, string aa)
        {
            string str3;
            string sql = "";
            string sql2 = "";
            if (c == 1)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '寄达国家（中文）',O.Country as '寄达国家（英文）',OA.Province as '州名',OA.City as '城市名',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',OA.Addressee as '收件人姓名',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话','High-tech zone，Juxian Road 399, Building B1 20th, Ningbo, ZheJiang,China' as '寄件人详细地址（英文）','VIKI' as '寄件人姓名','0574-27903940' as '寄件人电话','1' as '内件类型代码',isnull(OA.PostCode,'') as '邮政编码' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select TrackCode as '跟踪号','物品' as '物品中文名称',OP.Title as '物品英文名称(不能超过50个字符）',OP.Qty as '数量',10 as '单价','China' as '原产地' from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 2)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '目的地','1' as '件数',OA.Addressee as '收件人',isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',OA.PostCode as '收件邮编',O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市','' as '物品描述','1' as '物品数量','8' as '物品单价','8' as '声明价值' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
            }
            else if (c == 3)
            {
                sql = "select  TrackCode as '运单码',C.CCountry as '寄达国家（中文）',O.Country as '寄达国家（英文）',OA.Province as '州名',OA.City as '城市名',OA.PostCode as '邮编',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',OA.Addressee as '收件人姓名',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话','High-tech zone，Juxian Road 399, Building B1 20th, Ningbo, ZheJiang,China' as '寄件人详细地址（英文）','VIKI' as '寄件人姓名','0574-27903940' as '寄件人电话','1' as '内件类型代码' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select TrackCode as '跟踪号','物品' as '物品中文名称',OP.Title as '物品英文名称',OP.Qty as '数量,10 as '单价','China' as '原产地' from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 4)
            {
                //	收件人姓名		城市	收件人地址	收件人电话	收件人邮编	海关报关品名1	申报品数量1

                sql = "select  O.OrderExNo as '客户单号',O.TrackCode as '服务商单号', '俄速通' as '运输方式', 'russian federation' as '目的国家','' as '寄件人公司名','' as '寄件人姓名','' as '寄件人地址','' as '寄件人电话','' as '寄件人邮编','' as '寄件人传真','' as '收件人公司名',OA.Addressee as '收件人姓名',OA.Province as '州 \\ 省',OA.City as '城市',isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'')  as '收件人地址', isnull(OA.Tel, '')+'/'+ isnull(OA.Phone,'') as '收件人电话',OA.Email as '收件人邮箱',OA.PostCode as '收件人邮编', '' as '收件人传真',O.BuyerName as '买家ID', '' as '交易ID','' as '保险类型', '' as '投保金额', '' as '订单备注', '' as '是否退件','gift' as '海关报关品名1','' as '配货信息1','5' as '申报价值1','1' as '申报品数量1' ,'' as '配货备注1','' as '海关报关品名2','' as '配货信息2','' as '申报价值2','' as '申报品数量2' ,'' as '配货备注2','' as '海关报关品名3','' as '配货信息3','' as '申报价值3','' as '申报品数量3' ,'' as '配货备注3','' as '海关报关品名4','' as '配货信息4','' as '申报价值4','' as '申报品数量4' ,'' as '配货备注4','' as '海关报关品名5','' as '配货信息5','' as '申报价值5','' as '申报品数量5' ,'' as '配货备注5','' as '英文名'from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry  ";

            }
            else if (c == 5)
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话','俄罗斯联邦' as '目的地', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址','挂号小包' as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 6)//福建小包
            {
                //																	
                sql = "select  O.TrackCode as '记录序号（为订单号，唯一值）',OA.PostCode as '寄达局邮编',C.CCountry as '寄达局名称' ,OA.Addressee as '收件人姓名(国际邮件填英文)',OA.Street+' '+OA.City+' '+OA.Province+' '+OA.Country as '收件人地址(国际邮件填英文)',isnull( OA.Tel,'')+'/'+isnull(OA.Phone,'') as '收件人电话', '' as '邮件备注', OA.Country as '英文国家名',OA.Province as '英文州名',OA.City as '英文城市名',OA.PostCode as '收件人邮编','1' as '内件类型代码',P.Category as '内件名称',OP.Title as '内件英文名称', '1' as '内件数量','5' as '单价','CHINA' as '产地', '' as '物品英文说明','' as '内件成分说明' from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Products P On OP.SKU=P.SKU left join Country C On O.Country=C.ECountry  ";

            }
            else if (c == 7)//义乌小包 （商）
            {
                sql = "select  O.OrderNo as '自编号',O.TrackCode as '运单号',C.CCountry as '目的地',OA.Addressee as '收件人', OA.Street as '收件地址',isnull( OA.Tel,'')+'('+isnull(OA.Phone,'')+')' as '收件电话',OA.PostCode as '收件邮编', OA.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市', (select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU)) as '物品描述' ,1 as '物品数量', 2 as '物品单价',OP.Title as '物品别名'from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry ";

            }
            else if (c == 8)//荷兰小包
            {
                sql = "select O.OrderNo as '参考号',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU)) as '物品描述',OA.Addressee as '收件人',O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.Street as '收件地址',OA.PostCode as '收件邮编',isnull( OA.Tel,'')+'('+isnull(OA.Phone,'')+')' as '收件电话', '荷兰挂号小包' as '网络渠道',5 as '声明价值' ,'NLPG' as '简码',O.TrackCode as '运单号' from Orders O left join OrderAddress OA on O.AddressId=OA.Id left join OrderProducts OP on O.Id=OP.OId left join Country C On O.Country=C.ECountry ";

            }
            else if (c == 9)//荷兰小包
            {
                sql = "select TrackCode,CONVERT(varchar(10) , ScanningOn, 120 ),Country,Freight from Orders O ";
            }
            else if (c == 10)//欧亚速运
            {
                sql = @"select O.OrderNo as '订单号','http://122.227.179.98:848'+P.PicUrl as '产品图片',P.SKU as '产品编码',P.ProductName as '中文品名','' as '材质',OP.Qty as '数量',10 as '报关金额',OA.Addressee as '收货人名称',
isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'') as '收货地址',
isnull(OA.PostCode,'') as '邮编',
OA.Tel as '联系电话',OA.Phone as '手机',O.TrackCode as '实际发货物流:运单号'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry";

            }
            else if (c == 11)//EUB
            {
                sql = @"select O.OrderNo as '客户单号',O.TrackCode as '转单号', '苏邮（EUB）'as '运输方式' ,O.Country as '目的国家', OA.Addressee as '收件人姓名',
OA.Province as '州,省',OA.City as '城市', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+OA.PostCode+','+isnull(OA.Country,'') as '联系地址',
OA.Tel as '收件人电话','' as '收件人邮箱', OA.PostCode as '收件人邮编', PC.EName as '海关报关品名1', PC.Name '配货信息1',5 as '申报价值1', 1 as '申报品数量1'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join ProductCategory  PC On P.Category=PC.Name ";

            }
            else if (c == 12)//KS仓库发货数据
            {
                //                sql = @" SELECT O.OrderNo as 系统编号,O.TId as 流水交易号,CONVERT(VARCHAR(10),O.GenerateOn,120) as '日期/date', OP.SKU as '货号/SKU', O.OrderExNo as 'order-ID' ,
                //'' as 'User Id',OA.Addressee as 'Buyer Fullname' ,OA.Phone as 'Buyer Phone Number' ,OA.Street as 'Buyer Address' ,
                //OA.City as 'Buyer City',OA.Province as 'State',OA.PostCode as 'Buyer Zip',OA.Country as 'Buyer Country', P.Weight*OP.Qty as 'Gross Weight (LB)',
                //'' as 'Tracking numnber','' as '备注',OP.Qty as '数量'
                // from Orders O 
                // left join OrderProducts OP on O.Id=OP.OId  
                // left join Products P on OP.SKU=P.SKU  
                // left join OrderAddress OA on O.AddressId=OA.Id ";
                //                //left join ProductCategory  PC On P.Category=PC.Name "; //联接后有重复，原因分类里有重复比如:"家居"

                sql = @"SELECT '' as 订单来源,'' as 店铺名,o.buyerid as 买家ID,
substring(OrderExNo,CHARINDEX('_',OrderExNo)+1,LEN(OrderExNo)) as 店铺单号,OrderExNo as '带店铺号的外部订单号',O.OrderNo as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址, '' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,P.SKU AS SKU,op.Qty AS 数量,o.Amount 订单金额,o.TId 流水交易号
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id";
            }
            else if (c == 13)//俄罗斯海外仓
            {
                sql = @"select P.SKU as 'Артикул / Article','http://122.227.179.98:848'+P.PicUrl as 'Фото / Photo',OP.Qty as 'Количество товара / Quantity of goods',isnull(OP.Remark,'') as 'Примечание / Remarks',O.OrderExNo as 'Номер заказа / Order number',isnull(OA.City,'') as 'Город / City',isnull(OA.Street,'') as 'Улица, дом, квартира / Street, home, appartment',isnull(OA.Province,'') as 'Область, район. / region, district',OA.Addressee as 'Ф.И.О. / Name',isnull(OA.PostCode,'') as 'Индекс / Postcode',
OA.Tel as 'Телефон / Telephone number',OA.Phone as 'Телефон / Telephone number',O.Account as 'Store',O.TrackCode as 'TrackCode'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry ";
            }
            else if (c == 14) //C组海外仓日销
            {
                sql = @"select  replace(CONVERT(varchar, O.GenerateOn, 102 ),'.','-') as '日期',P.ProductName as'品名',OP.SKU as 'SKU',O.OrderExNo as '订单号',O.OrderNo as '自编号',OP.Qty as '数量', OP.Price as '售卖单价' ,O.Amount  as '订单金额', 
OA.Province as '所在州', OA.PostCode as '邮编',O.Freight as '运费','' as '平台费','' as '到仓价', O.profit as '利润',
O.ProfitRate as '利润率', O.FBABy as '备注',O.Account as '平台'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join ProductCategory  PC On P.Category=PC.Name ";
            }
            else if (c == 15)//线上发货导出
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',O.LogisticMode as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                //  sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 16) // 亚欧快运（宁波）
            {
                sql = "select O.TrackCode as '客户单号',''as '服务商单号','AK' as 运输方式,O.Country as '目的国家','' as '寄件人公司名','lvjingjing' as 寄件人姓名,'zhejiang' as '寄件人省','ningbo' as '寄件人城市','3F,NO.4 Building,JUNSHENG Group,1266,Juxian Road, National Hi-Tech Zone' as '寄件人地址','15988173792' as '寄件人电话','' as '寄件人邮编','' as '寄件人传真','' as '收件人公司名',OA.Addressee as '收件人姓名',OA.Province as '州 \\ 省',OA.City as '城市', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+OA.PostCode+','+isnull(OA.Country,'') as '联系地址',OA.Tel as '收件人电话',OA.Phone as '收件人手机',OA.Email as '收件人邮箱', OA.PostCode as '收件人邮编','' as '收件人传真','' as '买家ID','' as '交易ID','' as '保险类型','' as '保险价值','' as '订单备注','' as '重量','' as '是否退件', '5' as '申报类型',PC.EName as '海关报关品名1','' as '配货信息1',5 as '申报价值1', 1 as '申报品数量1','' as '配货备注1','' as '海关报关品名2','' as '配货信息2','' as '申报价值2','' as '申报品数量2','' as '配货备注2','' as '海关报关品名3','' as '配货信息3','' as '申报价值3','' as '申报品数量3','' as '配货备注3','' as '海关报关品名4','' as '配货信息4','' as '申报价值4','' as '申报品数量4','' as '配货备注4','' as '海关报关品名5','' as '配货信息5','' as '申报价值5','' as '申报品数量5','' as 配货备注5 from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join Products P on OP.SKU=P.SKU   left join OrderAddress OA on O.AddressId=OA.Id  left join ProductCategory  PC On P.Category=PC.Name";
            }
            else if (c == 17) // 义乌海外仓发货清单导出
            {
                sql = @"SELECT 
                        CONVERT(VARCHAR(10),O.GenerateOn,120) as 日期,
                        O.OrderExNo as 订单号,
                        O.OrderNo as 自编号,
                        ProductName as 品名,
                        P.SKU AS SKU,
                        op.Qty AS 数量,
                        op.Price 售卖单价,
                        O.amount as 订单金额,
                        OA.Province AS 所在州,
                        OA.PostCode AS 邮编,
                        cast(isnull(O.Freight,0) as money) 运费,
                        cast(a.Tax*O.amount as money) 平台费,
                        cast(p.Price/b.CurrencyValue as money) 到仓单价,
                        cast(p.Price/b.CurrencyValue*op.Qty as money) 到仓总成本,
                        cast(O.amount-O.Freight-a.Tax*O.amount-p.Price/6.5*op.Qty as money) 利润,
                        case when O.amount=0 then '0%' else convert(varchar(50),cast((O.amount-O.Freight-a.Tax*O.amount-p.Price/b.CurrencyValue*op.Qty)/O.amount*100 as money)) + '%'end 利润率,
                        FBABy 仓库,
                        Account 账号
                         from Orders O 
                         left join OrderProducts OP on O.Id=OP.OId  
                         left join Products P on OP.SKU=P.SKU  
                         left join OrderAddress OA on O.AddressId=OA.Id
                         join Account a on o.Account=a.AccountName  
                         join FixedRate b on O.CurrencyCode=b.CurrencyCode AND b.Year=(DATEPART(YEAR,O.GenerateOn)) AND b.Month=(DATEPART(Month,O.GenerateOn)) ";
            }
            else if (c == 18)//CDEK导出模板
            {
                sql = @"select  
                        O.TrackCode as '运单号',
                        O.OrderExNo as 订单号,
                        (select sum(OP.Qty) from OrderProducts OP where O.Id=OP.OId) as '产品数量',  
                        O.Country as '中文品名',
                        (select top 1 Category from Products P where P.SKU=OP.SKU)  as '中文品名',
                        CONVERT(varchar(10) , O.ScanningOn, 120 ) as '发货时间' ,
                        '5' as '申报价值'
                        from Orders O 
                        left join OrderProducts OP on O.Id=OP.OId ";
            }
            else if (c == 19)//美东海外仓（CA）
            {
                sql = @"SELECT 
O.Account as 店铺,O.OrderNo as 交易ID,O.OrderExNo as 外部编号,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,'' as 收货人国家中文,'' as 币种,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址, '' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,'' AS 付款人邮箱,O.SellerMemo AS 备注,P.SKU AS SKU,'' AS 中文报关名称,'' AS 商品金额,'' AS 英文报关名称,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,
'' AS 买家ID,
'' AS 店铺运输方式,
'' AS 运费总额,
'' AS 商品itemID,
O.TId AS ebay交易ID
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }

            else if (c == 20)//美西托盘海外仓（LAI）
            {
                sql = @"SELECT 
O.OrderExNo as 订单号,O.OrderNo as ERP编号,O.Account as 店铺,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS TEL,P.SKU AS SKU,op.Qty AS 数量,O.SellerMemo AS 备注
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 21)//美西海外仓（LAI）--仅限导出一次
            {
                sql = @"SELECT 
P.SKU AS 'Item SKU',op.Qty AS 'Quantity',P.SKU + '*' +cast(op.Qty AS varchar) AS 'Label Quantity',O.TrackCode AS 'Note（Fedex跟踪号）',
O.OrderExNo as 'ERP订单号',O.OrderNo as '订单编号'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  ";
            }

            else if (c == 22)//英国仓库UKMAN
            {
                sql = @"SELECT 
O.OrderExNo as 订单号,O.OrderNo as ERP订单号,OA.Country as 收货国家名称,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS '收货人电话',P.SKU AS SKU,op.Qty AS 数量
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 23)//宁波美东谷仓(Gcus-East)
            {
                sql = @"SELECT 
'Gcus-East' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',(case when REVERSE(left(REVERSE(P.sku),2))='BL'  then REVERSE('KB'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))  
 when  REVERSE(left(REVERSE(P.sku),2))='WH'  then REVERSE('TW'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))
else P.SKU end) AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 24)//宁波美东(US-East)
            {
                sql = @"SELECT 
(case when O.Platform='Ebay' then  O.TId else O.OrderExNo end) as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,P.sku AS SKU,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id   ";
            }

            else if (c == 25)//宁波KS
            {
                sql = @"SELECT 
O.OrderExNo as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,(case when REVERSE(left(REVERSE(P.sku),2))='BL'  then REVERSE('KB'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))  
 when  REVERSE(left(REVERSE(P.sku),2))='WH'  then REVERSE('TW'+right(REVERSE(P.sku),len(REVERSE(P.sku))-2))
else P.SKU end) AS SKU,op.Qty AS 数量,'' AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id   ";
            }
            else if (c == 26)//俄罗斯海外仓旧（宁波）
            {
                sql = @"select P.SKU as 'Артикул / Article','http://122.227.179.98:848'+P.PicUrl as 'Фото / Photo',OP.Qty as 'Количество товара / Quantity of goods',isnull(OP.Remark,'') as 'Примечание / Remarks',O.TrackCode as '跟踪',O.Account as '店铺',O.CreateOn as'同步时间',O.OrderExNo as 'Номер заказа / Order number',isnull(OA.City,'') as 'Город / City',isnull(OA.Street,'') as 'Улица, дом, квартира / Street, home, appartment',isnull(OA.Province,'') as 'Область, район. / region, district',OA.Addressee as 'Ф.И.О. / Name',isnull(OA.PostCode,'') as 'Индекс / Postcode',
OA.Tel as 'Телефон / Telephone number',OA.Phone as 'Телефон / Telephone number'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id 
 left join Country C On O.Country=C.ECountry ";
            }
            else if (c == 27)
            {
                sql = "select  TrackCode as '运单号',OA.Addressee as '收件人',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话','俄罗斯联邦' as '目的地', O.Country as '收件国家',OA.Province as '收件省州',OA.City as '收件城市',OA.PostCode as '收件邮编', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址','挂号小包' as '货运方式','china post air mail' as 'Shipping Service',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','衣服' as '物品别名','5' as '物品价值' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";
                sql2 = "select  TrackCode as '运单号',OA.Addressee as '收件人', O.Country as '收件国家', isnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件地址',OA.City as '收件城市',OA.Province as '收件省州',OA.PostCode as '收件邮编',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件电话',(select top 1 EName from ProductCategory where Name in (select top 1 Category from Products P where P.SKU=OP.SKU))  as '物品描述','1' as '数量' from Orders O  left join OrderProducts OP on O.Id=OP.OId  left join OrderAddress OA on O.AddressId=OA.Id left join Country C On O.Country=C.ECountry";

            }
            else if (c == 28)//义乌美西(董)导出--YWCA-WEST(DONG) 
            {
                sql = @"SELECT 
'USWE' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',P.SKU  AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id  ";
            }
            else if (c == 29)//义乌美东(LEO)导出--YWNJ-EAST(LEO)
            {
                sql = @"SELECT 
'USEA' as '仓库代码/Warehouse Code',O.OrderExNo as '参考编号/Reference Code',O.LogisticMode as '派送方式/Delivery Style',(case when O.LogisticMode is null then '否' else '是' end) as '是否为指定的派送方式',O.Platform as '销售平台/Sales Platform',OA.Addressee as '收件人姓名/Consignee Name',OA.Country as '收件人国家/Consignee Country',OA.CountryCode as '收货国家代码',OA.Province AS '州/Province',OA.City AS '城市/City',OA.PostCode AS '邮编/Zip Cod',OA.Street AS '地址1/Street1', '' AS '地址2/Street2','' AS '门牌号/Doorplate','' AS '收件人公司/Consignee Company',OA.PostCode AS '收件人Email/Consignee Email',OA.Phone AS '收件人电话/Consignee Phone','' AS '签名服务/Signature','' AS '保险服务/Insurance','' AS '投保金额/Insurance Amount',O.SellerMemo AS '备注/Remark',P.SKU  AS SKU,op.Qty AS 数量,O.Account as 店铺
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id ";
            }
            else if (c == 30)//俄罗斯海外仓（宁波）
            {
                sql = @"select '' as '客户',P.SKU as 'SKU',OP.Qty as '数量',O.OrderExNo as '订单号',O.TrackCode as '跟踪',O.LogisticMode as '渠道',isnull(OA.PostCode,'') as '邮编', OA.Addressee as '收件人姓名',O.Country as '国家',OA.Province as '省州',OA.City as '城市',OA.Street as '街','' as '住房','' as '楼','' as '房子\办公室',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收货人的联系方式(手机号)',OA.Email as '收货人的邮箱','' as '额外信息',o.CreateOn as '同步时间'
 from Orders O 
 left join OrderProducts OP on O.Id=OP.OId  
 left join Products P on OP.SKU=P.SKU  
 left join OrderAddress OA on O.AddressId=OA.Id ";
            }
            else if (c == 31)
            {
                sql = "select ''as '记录序号',''as '大宗用户编号',''as '用户自编号', isnull(OA.PostCode,'') as '寄达局邮编',C.CCountry as '寄达局名称',OA.Addressee as '收件人姓名',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话', TrackCode as '邮件号码',''as '邮件备注',''as '保险金额',''as '保价金额',''as '保值金额',''as '总资费',''as '封发标志',''as '已贴票金额',''as '回持单号码',O.Country as '英文国家名',OA.Province as '英文州名',OA.City as '英文城市名','VIKI' as '寄件人姓名（英文）','ZheJiang' as '寄件人省名（英文）','Ningbo' as '寄件人城市名（英文）','High-tech zone，Juxian Road 399, Building B1 20th,Ningbo, ZheJiang,China' as '寄件人地址（英文）','0574-27903940' as '寄件人电话','1' as '内件类型代码',''as '验关报关标志',''as '验关物品类型',''as '内件名称',''as '内件数量',''as '单件重量',''as '单价',''as '产地',''as '协调号',''as '物品英文说明' ,''as '内件成分说明',''as '图像文件名' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";
                sql2 = "select ''as '记录序号', TrackCode as '邮件号码','1'as '内件序号', '物品' as '内件名称',OP.Title as '内件英文名称',OP.Qty as '内件数量',10 as '单价','China' as '产地' ,'' as '物品英文说明' ,'' as '内件成分说明' ,'' as '协调号码'  from Orders O\r\nleft join OrderProducts OP on O.Id=OP.OId\r\nleft join OrderAddress OA on O.AddressId=OA.Id";
            }
            if (t == 1)
            {
                str3 = sql;
                if (c == 13)//俄罗斯海外仓例外（追踪号为 null 导出）（有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
                else if (c == 20 || c == 22)//美西托盘海外仓（LAI）（追踪号为 null 导出，没有发货方式）、英国海外仓UKMAN
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 19)//美东海外仓（CA）,不需要选时间  （追踪号为 null 导出，没有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') ";
                }
                else if (c == 23 || c == 24 || c == 25 || c == 28 || c == 29)//宁波美东谷仓(Gcus-East)、宁波美东(US-East)结束时间选择去除 （追踪号为 null 导出，没有发货方式）
                {
                    sql = str3 + " where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn >= '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 21)//美西海外仓（LAI）--仅限导出一次--->有单号
                {
                    sql = str3 + " where  O.Status='已发货' and O.TrackCode is not null and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (c == 27)//导出义乌俄罗斯挂号小包(已处理)
                {
                    sql = str3 + " where  O.Status='已处理' and  O.TrackCode is not null  and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else
                {
                    sql = str3 + " where  O.Status='已发货' and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
            }
            else
            {
                str3 = sql;
                if (f == "Id")
                    sql = str3 + " where  O." + f + " in (" + d + ")";
                else
                {
                    sql = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
                }
            }
            if (a == 1)
            {
                sql = sql + " and O.Account not like 'yw%'";
            }
            else if (a == 2)
            {
                sql = sql + " and O.Account like 'yw%'";
            }
            if (p != "0" && p != "ALL")
            {
                sql = sql + " and O.Platform ='" + p + "'";
            }
            if (aa != "===请选择===" && aa != "ALL")
            {
                sql = sql + " and O.Account='" + aa + "'";
            }
            if (c == 13)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'YWRU-AEA','YWRU-AEB') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 26)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'NBRU-AEA') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 30)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy in ( 'NBRU-AEA') and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 19)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'CA' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 23) //宁波美东谷仓(Gcus-East)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'Gcus-East' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 24)//宁波美东(US-East)
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'US-East' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 28) //义乌美西(董)【YWCA-WEST(DONG)】
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'YWCA-WEST(DONG)' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 29)//义乌美东(LEO)【YWNJ-EAST(LEO)】
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'YWNJ-EAST(LEO)' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if (c == 25)//宁波KS
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'KS' and O.Enabled = 1 and O.IsAudit = 1  and O.Account  not like 'yw%'";
            }
            if (c == 20)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and  O.Amount>600";
            }
            if (c == 21) //美西海外仓（LAI）--仅限导出一次--->有单号
            {
                sql = sql + " and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and IsDao =0 ";

            }
            if (c == 22)
            {
                sql = sql + "and O.IsFBA = 1 and O.FBABy = 'UKMAN' and O.Enabled = 1 and O.IsAudit = 1";
            }
            if ((c == 15) && (s.Length > 0))
            {
                sql = sql + " and O.LogisticMode like '%" + s + "%'";
            }
            DataSet orderExport = this.GetOrderExport(sql);
            if (sql2.Length > 5)
            {
                if (t == 1)
                {
                    str3 = sql2;
                    sql2 = str3 + " where  O.Status='已发货' and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
                }
                else
                {
                    str3 = sql2;
                    sql2 = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
                }
                if (a == 1)
                {
                    sql2 = sql2 + " and O.Account not like 'yw%'";
                }
                else if (a == 2)
                {
                    sql2 = sql2 + " and O.Account like 'yw%'";
                }
                if (aa != "===请选择===" && aa != "ALL")
                {
                    sql2 = sql2 + " and O.Account='" + aa + "'";
                }
                DataSet set2 = this.GetOrderExport(sql2);
                DataTable table = set2.Tables[0].Clone();

                List<string> liststr = new List<string>();
                foreach (DataRow row in set2.Tables[0].Rows)
                {
                    if (c == 1)
                    {
                        if (row["物品英文名称(不能超过50个字符）"].ToString().Length > 0x30)
                        {
                            row["物品英文名称(不能超过50个字符）"] = row["物品英文名称(不能超过50个字符）"].ToString().Substring(0, 0x2d);
                        }
                    }
                    if (c == 31)
                    {
                        if (row["内件英文名称"].ToString().Length > 0x30)
                        {
                            row["内件英文名称"] = row["内件英文名称"].ToString().Substring(0, 0x2d);
                        }
                    }
                    if (!liststr.Contains(row.ToString()))
                    {
                        liststr.Add(row[0].ToString());
                        table.Rows.Add(row.ItemArray);
                    }
                }
                table.TableName = "Sheet2";
                orderExport.Tables.Add(table);
                orderExport.Tables[0].TableName = "Sheet1";
            }

            try
            {
                List<string> liststr = new List<string>();

                foreach (DataRow dr in orderExport.Tables[0].Rows)
                {
                    if (c == 12 || c == 13 || c == 14 || c == 17 || c == 19 || c == 20 || c == 21 || c == 22 || c == 23 || c == 24 || c == 25 || c == 26 || c == 28 || c == 29 || c == 30)
                    {
                        break;

                    }
                    if (c == 31)
                    {
                        if (liststr.Contains(dr[9].ToString()))
                        {
                            dr.Delete();
                        }
                        else
                        {
                            liststr.Add(dr[9].ToString());
                        }
                    }
                    else
                    {
                        // 删除已存在日期行?
                        if (liststr.Contains(dr[0].ToString()))
                        {
                            dr.Delete();
                        }
                        else
                        {
                            liststr.Add(dr[0].ToString());
                        }
                    }
                }

                orderExport.Tables[0].AcceptChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(orderExport, base.GetCurrentAccount().FromArea);
            if (c == 21)//仅导出一次
            {
                base.NSession.CreateQuery("update OrderType O set IsDao=IsDao+1 where   O.Status='已发货' and O.TrackCode is not null and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.IsFBA = 1 and O.FBABy = 'LAI' and O.Enabled = 1 and O.IsAudit = 1 and IsDao =0 ").ExecuteUpdate();
            }
            else if (c == 13)
            {
                base.NSession.CreateQuery("update OrderType O set IsDao=IsDao+1   where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'  and O.IsFBA = 1 and O.FBABy in ( 'YWRU-AEA','YWRU-AEB') and O.Enabled = 1 and O.IsAudit = 1").ExecuteUpdate();
            }

            return base.Json(new { IsSuccess = true });
        }


        [HttpPost]
        public ActionResult ExportOrder2(DateTime st, DateTime et, string d, string f, int t, int a, string s, string p, string aa)
        {
            string str3;
            string sql = " ";

            sql = "select O.OrderNo,OrderExNo,Amount,Country,TrackCode,LogisticMode,Weight,Account,ScanningOn,ScanningBy from Orders O ";

            if (t == 1)
            {
                str3 = sql;
                sql = str3 + " where  O.Status='已发货' and  O.ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
            }
            else
            {
                str3 = sql;
                sql = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
            }
            if (a == 1)
            {
                sql = sql + " and O.Account not like 'yw%'";
            }
            else if (a == 2)
            {
                sql = sql + " and O.Account like 'yw%'";
            }
            if (aa != "===请选择===")
            {
                sql = sql + " and O.Account='" + aa + "'";
            }
            DataSet orderExport = this.GetOrderExport(sql);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(orderExport);
            return base.Json(new { IsSuccess = true });
        }
        [HttpPost]
        public ActionResult ExportOrderII2(DateTime st, DateTime et, string d, string f, int t, int a, string s, string p, string aa)
        {
            string str3;
            string sql = " ";

            sql = "select O.OrderNo,OrderExNo,Amount,Country,TrackCode,LogisticMode,Account,ScanningOn,ScanningBy from Orders O ";

            if (t == 1)
            {
                str3 = sql;
                sql = str3 + " where  O.Status='已发货' and  O.ScanningOn between '" + st.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + et.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.LogisticMode like '%" + (string.IsNullOrEmpty(s) ? "" : s) + "%'";
            }
            else
            {
                str3 = sql;
                sql = str3 + " where  O." + f + " in ('" + d.Replace(" ", "").Replace("\r", "").Trim().Replace("\n", "','").Replace("''", "") + "')";
            }
            if (a == 1)
            {
                sql = sql + " and O.Account not like 'yw%'";
            }
            else if (a == 2)
            {
                sql = sql + " and O.Account like 'yw%'";
            }
            if (aa != "===请选择===")
            {
                sql = sql + " and O.Account='" + aa + "'";
            }
            DataSet orderExport = this.GetOrderExport(sql);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(orderExport);
            return base.Json(new { IsSuccess = true });
        }

        [HttpPost]
        public ActionResult ExportZM(string o)
        {
            string str3;
            string sql = "";
            string sql2 = "";

            sql = "select  TrackCode as '运单码',C.CCountry as '寄达国家（中文）',O.Country as '寄达国家（英文）',OA.Province as '州名',OA.City as '城市名',\r\nisnull(oa.Street,'')+','+isnull(oa.City,'')+','+isnull(OA.Province,'')+','+isnull(OA.Country,'')+','+isnull(OA.PostCode,'') as '收件人详细地址',OA.Addressee as '收件人姓名',isnull(oa.Phone,'')+'('+isnull(oa.Tel,'')+')' as '收件人电话','High-tech zone，Juxian Road 399, Building B1 20th, Ningbo, ZheJiang,China' as '寄件人详细地址（英文）','VIKI' as '寄件人姓名','0574-27903940' as '寄件人电话','1' as '内件类型代码' from Orders O\r\nleft join OrderAddress OA on O.AddressId=OA.Id\r\nleft join Country C On O.Country=C.ECountry";



            StringBuilder builder = new StringBuilder();

            base.Session["ExportDown"] = builder.ToString();
            return base.Json(new { IsSuccess = true });
        }

        public OrderType GetById(int Id)
        {
            OrderType type = base.NSession.Get<OrderType>(Id);
            if (type == null)
            {
                throw new Exception("返回实体为空");
            }
            return type;
        }

        public JsonResult GetNotQueList()
        {
            IList<object[]> list = base.NSession.CreateSQLQuery(string.Format("select * from (\r\nselect SKU,SUM(Qty) as Qty,(select isnull(SUM(Qty),0) from WarehouseStock where SKU=OP.SKU ) as NowQty,(select count(Id) from SKUCode where SKU=OP.SKU and IsOut=0) as unPeiQty,COUNT(O.Id) as'OrderQty' from Orders O left join OrderProducts OP On O.Id=OP.OId where O.IsOutOfStock=1 and OP.IsQue=1 and O.Status<>'作废订单' group by SKU\r\n) as tbl  where NowQty>0", new object[0])).List<object[]>();
            List<QueCount> list2 = new List<QueCount>();
            foreach (object[] objArray in list)
            {
                QueCount item = new QueCount
                {
                    SKU = objArray[0].ToString(),
                    Qty = Utilities.ToInt(objArray[1]),
                    NowQty = Utilities.ToInt(objArray[2]),
                    UnPeiQty = Utilities.ToInt(objArray[3]),
                    OrderQty = Utilities.ToInt(objArray[4])
                };
                list2.Add(item);
            }
            return base.Json(new { total = list2.Count, rows = list2 });
        }

        public JsonResult GetOrderByAliSend(string o, int f)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                string str = "";
                OrderType type = list[0];
                if (string.IsNullOrEmpty(type.TrackCode2))
                {
                    str = "订单:" + type.OrderNo + ", 没有提前设置过追踪号!可以设置条码";
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                if (f == 1)
                {
                    str = "订单:" + type.OrderNo + ", 已经设置覆盖扫描!可以设置条码";
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                str = "订单:" + type.OrderNo + ", 已经扫描过!";
                return base.Json(new { IsSuccess = false, Result = str });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult GetOrderByBeforePei(string orderNo)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + orderNo + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                if (type.Status == OrderStatusEnum.已处理.ToString())
                {
                    string str;
                    if (!((type.IsError != 1) && string.IsNullOrEmpty(type.CutOffMemo)))
                    {
                        str = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    if (type.IsAudit == 0)
                    {
                        str = "订单:" + type.OrderNo + ", 需要审核";
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    string str2 = "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='300px' >图片</th><td width='200px'>SKU*数量</td><td>规格</td>\r\n                                                        </tr>";
                    string format = "<tr style='font-weight:bold; font-size:30px;' name='tr_{0}' code='{3}' qty='{1}' cqty='{4}'><td><img width='220px' src='/imgs/pic/{0}/1.jpg' /></td><td>{0}*{1}</td><td>{2}</td><td><span></td></tr>";
                    type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();
                    foreach (OrderProductType type2 in type.Products)
                    {
                        IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU).SetMaxResults(1).List<ProductType>();
                        if (list2.Count > 0)
                        {
                            if (list2[0].IsScan == 1)
                            {
                                str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, 0 });
                            }
                            else
                            {
                                str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, type2.Qty });
                            }
                        }
                    }
                    str2 = str2 + "</table>";
                    return base.Json(new { IsSuccess = true, Result = str2 });
                }
                return base.Json(new { IsSuccess = false, Result = "订单状态不符！此订单的状态是：" + type.Status + " 只有已处理订单 才能扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult GetOrderByPei(string orderNo)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + orderNo + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                if ((type.Status == OrderStatusEnum.待拣货.ToString()) || (!Config.IsJi && (type.Status == OrderStatusEnum.已处理.ToString())))
                {
                    string str;
                    if (!((type.IsError != 1) && string.IsNullOrEmpty(type.CutOffMemo)))
                    {
                        str = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    if (type.IsAudit == 0)
                    {
                        str = "订单:" + type.OrderNo + ", 需要审核";
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    string str2 = "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='300px' >图片</th><td width='200px'>SKU*数量</td><td>规格</td><td>扫描次数</td>\r\n                                                        </tr>";
                    string format = "<tr style='font-weight:bold; font-size:30px;' name='tr_{0}' code='{3}' qty='{1}' cqty='{4}'><td><img width='220px' src='/imgs/pic/{0}/1.jpg' /></td><td>{0}*{1}</td><td>{2}({5})</td><td><span><span id='r_{3}' style='color:red'>{4}</span>/<span style='color:green'>{1}</span></td></tr>";
                    type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();
                    string str4 = "";
                    foreach (OrderProductType type2 in type.Products)
                    {
                        IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                        if (list2.Count > 0)
                        {
                            if (list2[0].IsScan == 1)
                            {
                                str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, 0, list2[0].ProductAttribute });
                            }
                            else
                            {
                                str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, type2.Qty, list2[0].ProductAttribute });
                            }
                            if (!(!(list2[0].ProductAttribute != "普货") || string.IsNullOrEmpty(list2[0].ProductAttribute)))
                            {
                                str4 = str4 + " " + list2[0].ProductAttribute;
                            }
                        }
                    }
                    str2 = str2 + "</table>";
                    if (str4.Length > 0)
                    {
                        str2 = "<span><h2>该订单中包含" + str4 + " 的产品</h2></span>" + str2;
                    }
                    return base.Json(new { IsSuccess = true, Result = str2 });
                }
                return base.Json(new { IsSuccess = false, Result = "订单状态不符！此订单的状态是：" + type.Status + " 需要配货前扫描后 方能配货扫描" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult GetOrderByQue(string orderNo)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + orderNo + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                if (!(type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    return base.Json(new { IsSuccess = false, Result = "订单状态不符！" });
                }
                if (type.IsOutOfStock != 1)
                {
                    string str;
                    if (!((type.IsError != 1) && string.IsNullOrEmpty(type.CutOffMemo)))
                    {
                        str = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    if (type.IsAudit == 0)
                    {
                        str = "订单:" + type.OrderNo + ", 需要审核";
                        return base.Json(new { IsSuccess = false, Result = str });
                    }
                    string str2 = "<table width='100%' border='1'><tr><td width='100px' align='right'><b>选择</b></td><td width='120px'><b>SKU</b></td><td  width='120px'><b>Qty</b></td><td  width='120px'><b>库存</b></td><td><b>Desc</b></td></tr>";
                    foreach (OrderProductType type2 in base.NSession.CreateQuery(" from OrderProductType where OId=" + type.Id).List<OrderProductType>())
                    {
                        object obj2 = base.NSession.CreateQuery("select count(Id) from SKUCodeType where SKU='" + type2.SKU + "' and IsOut=0").UniqueResult();
                        if (obj2 == null)
                        {
                            obj2 = 0;
                        }
                        str2 = str2 + string.Format("<tr><td align='right'><input type='checkbox'  name='ck_{0}' code='{0}' checked=checked /></td><td>{1}</td><td>{2}</td><td>{4}</td><td>{3}</td></tr>", new object[] { type2.Id, type2.SKU, type2.Qty, type2.Standard, obj2 });
                    }
                    str2 = str2 + "</table>";
                    return base.Json(new { IsSuccess = true, Result = str2 });
                }
                return base.Json(new { IsSuccess = false, Result = "已经是缺货订单！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult GetOrderBySend(string o, string w)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                string str2;
                OrderType type = list[0];
                if (((type.Status != OrderStatusEnum.待发货.ToString()) && (type.Status != OrderStatusEnum.待包装.ToString())) && !(type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
                }
                if (type.IsOutOfStock == 1)
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前订单为缺货状态" });
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
                    str2 = "订单:" + type.OrderNo + ",运单号:" + type.TrackCode + " 当前状态：待发货，可以发货。<br>发货方式：<s id='logisticsMode'>" + type.LogisticMode + "</s>";
                    List<OrderProductType> list3 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();


                    foreach (OrderProductType orderProductType in list3)
                    {
                        List<ProductType> product =
                            NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List
                                <ProductType>().ToList();

                        if (product.Count > 0)
                        {

                            if (string.IsNullOrEmpty(w) || list3.Count == 1)
                            {
                                if (list3[0].Qty == 1)
                                {

                                    bool iscon = false;
                                    if (product[0].Weight != 0)
                                    {
                                        if (product[0].Weight <= 200)
                                        {
                                            if ((product[0].Weight * 1.1) < Convert.ToDouble(w) ||
                                                (product[0].Weight * 0.9) > Convert.ToDouble(w))
                                                iscon = true;
                                        }
                                        else
                                        {
                                            if ((product[0].Weight * 1.05) < Convert.ToDouble(w) ||
                                                (product[0].Weight * 0.95) > Convert.ToDouble(w))
                                                iscon = true;
                                        }
                                        if (iscon)
                                        {
                                            string html =
                                                string.Format(
                                                    "<span style='color:red'>产品:{0} 重量为{1} ，现在重量为{2},请检查包裹产品和包装</span><a href=\"javascript:setWeight('{0}',{2})\">点此修改重量</a></span><br/>",
                                                    product[0].SKU, product[0].Weight, w);
                                            return Json(new { IsSuccess = false, Result = html });
                                        }
                                    }
                                }
                            }

                        }
                    }

                    string str3 = "";
                    foreach (OrderProductType type2 in list3)
                    {
                        IList<ProductType> list4 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                        if ((list4.Count > 0) && ((list4[0].ProductAttribute != "普货") && (list4[0].ProductAttribute != "电子")))
                        {
                            string str4 = str3;
                            str3 = str4 + "   " + list4[0].SKU + ":" + list4[0].ProductAttribute;
                        }
                    }
                    if (str3.Length > 0)
                    {
                        str2 = "<div><h3>订单中包含：" + str3 + " 的产品</h3></div>" + str2;
                    }
                    return base.Json(new { IsSuccess = true, Result = str2, Code = codeLength });
                }
                str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                return base.Json(new { IsSuccess = false, Result = str2 });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }


        public JsonResult GetOrderBySendVali(string o, string w)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode ='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                string str2;
                OrderType type = list[0];
                if (((type.Status != OrderStatusEnum.待发货.ToString()) && (type.Status != OrderStatusEnum.待包装.ToString())) && !(type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
                }
                if (type.IsOutOfStock == 1)
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前订单为缺货状态" });
                }
                if (type.IsAudit == 0)
                {
                    string str = "订单:" + type.OrderNo + ", 需要审核";
                    return base.Json(new { IsSuccess = false, Result = str });
                }
                if ((type.IsError == 0))
                {
                    int codeLength = -1;
                    IList<LogisticsModeType> logisticsModeTypes = base.NSession.CreateQuery("from LogisticsModeType where LogisticsCode='" + type.LogisticMode + "'").List<LogisticsModeType>();

                    if (logisticsModeTypes.Count > 0)
                    {
                        if (logisticsModeTypes[0].CodeLength != 0)
                            codeLength = logisticsModeTypes[0].CodeLength;
                        else
                        {
                            LogisticsType logisticsType = NSession.Get<LogisticsType>(logisticsModeTypes[0].ParentID);
                            codeLength = logisticsType.CodeLength;
                        }
                    }

                    str2 = "订单:" + type.OrderNo + ",运单号:" + type.TrackCode + " 当前状态：待发货，可以发货。<br>发货方式：<s id='logisticsMode'>" + type.LogisticMode + "</s><br />";
                    type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();



                    str2 += "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='300px' >图片</th><td width='200px'>SKU*数量</td><td>规格</td><td>扫描次数</td>\r\n                                                        </tr>";
                    string format = "<tr style='font-weight:bold; font-size:30px;' name='tr_{0}' code='{3}' qty='{1}' cqty='{4}'><td><img width=180px' src='{6}' /></td><td>{0}*{1}</td><td>{2}({5})</td><td><span><span id='r_{3}' style='color:red'>{4}</span>/<span style='color:green'>{1}</span></td></tr>";
                    string str4 = "";
                    foreach (OrderProductType type2 in type.Products)
                    {
                        IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                        if (list2.Count > 0)
                        {

                            str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, 0, list2[0].ProductAttribute, list2[0].PicUrl });


                        }
                    }
                    str2 = str2 + "</table>";

                    return base.Json(new { IsSuccess = true, Result = str2, Code = codeLength });
                }
                str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                return base.Json(new { IsSuccess = false, Result = str2 });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }
        //2016-11-02 义乌扫描模块
        public JsonResult GetOrderBySendVali1(string o, string w)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where Enabled=1 and  (OrderNo='" + o + "' Or TrackCode ='" + o + "') ").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                string str2;
                OrderType type = list[0];
                object obj1 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and  IsMerger=1  and MId='" + type.Id + "'").UniqueResult();
                if (Utilities.ToInt(obj1) > 1)
                {
                    return base.Json(new { IsSuccess = false, Result = "该合并子订单有已发货订单，重新配货！" });
                }
                if (((type.Status != OrderStatusEnum.待发货.ToString()) && (type.Status != OrderStatusEnum.待包装.ToString())) && !(type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
                }
                if (type.IsOutOfStock == 1)
                {
                    return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前订单为缺货状态" });
                }
                if (type.IsAudit == 0)
                {
                    string str = "订单:" + type.OrderNo + ", 需要审核";
                    return base.Json(new { IsSuccess = false, Result = str });
                }
                if ((type.IsError == 0))
                {
                    int codeLength = -1;
                    IList<LogisticsModeType> logisticsModeTypes = base.NSession.CreateQuery("from LogisticsModeType where LogisticsCode='" + type.LogisticMode + "'").List<LogisticsModeType>();

                    if (logisticsModeTypes.Count > 0)
                    {
                        if (logisticsModeTypes[0].CodeLength != 0)
                            codeLength = logisticsModeTypes[0].CodeLength;
                        else
                        {
                            LogisticsType logisticsType = NSession.Get<LogisticsType>(logisticsModeTypes[0].ParentID);
                            codeLength = logisticsType.CodeLength;
                        }
                    }

                    str2 = "订单:" + type.OrderNo + ",运单号:" + type.TrackCode + " 当前状态：待发货，可以发货。<br>发货方式：<s id='logisticsMode'>" + type.LogisticMode + "</s><br />";
                    type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();



                    str2 += "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='300px' >图片</th><td width='200px'>SKU*数量</td><td>规格</td><td>扫描次数</td>\r\n                                                        </tr>";
                    string format = "<tr style='font-weight:bold; font-size:30px;' name='tr_{0}' code='{3}' qty='{1}' cqty='{4}'><td><img width=180px' src='{6}' /></td><td>{0}*{1}</td><td>{2}({5})</td><td><span><span id='r_{3}' style='color:red'>{4}</span>/<span style='color:green'>{1}</span></td></tr>";
                    string str4 = "";
                    foreach (OrderProductType type2 in type.Products)
                    {
                        IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                        if (list2.Count > 0)
                        {

                            str2 = str2 + string.Format(format, new object[] { type2.SKU.Trim().ToUpper(), type2.Qty, type2.Standard, type2.Id, 0, list2[0].ProductAttribute, list2[0].PicUrl });


                        }
                    }
                    str2 = str2 + "</table>";

                    return base.Json(new { IsSuccess = true, Result = str2, Code = codeLength });
                }
                str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                return base.Json(new { IsSuccess = false, Result = str2 });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }
        //内销扫描获取商品信息
        public JsonResult GetOrderByDirect(string o, string skuCode)
        {
            string str2_ByDirect = "";
            string str4 = "";
            List<SKUCodeType> list = base.NSession.CreateQuery("from SKUCodeType where Code='" + o + "' or SKU ='" + o + "'").List<SKUCodeType>().ToList<SKUCodeType>();
            if (list.Count > 0)
            {
                SKUCodeType type = list[0];
                str2_ByDirect += "  <table width='100%' class='dataTable'>\r\n                                                        <tr class='dataTableHead'>\r\n                                                            <th width='150px' style='font-size:10px;'>图片</th><td width='250px' style='font-size:10px;'>SKU*数量</td><td></td><td style='font-size:10px;width:100px'>价格</td><td style='font-size:10px;'>扫描次数</td>\r\n                                                        </tr>";
                string format = "<tr style='font-weight:bold; font-size:8px;' name='tr_{7}' code='{3}' qty='{4}' cqty='{4}' price='{2}'><td><img width=100px' src='{6}' /></td><td width='250' >{7}*{4}</td><td width='40'></td><td>{2}</td><td><span><span id='r_{3}' style='color:red'>{4}</span></td></tr>";
                IList<ProductType> list1 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type.SKU).SetMaxResults(1).List<ProductType>();
                if (list1.Count > 0)
                {
                    ProductType type1 = list1[0];
                    str2_ByDirect = str2_ByDirect + string.Format(format, new object[] { o.Trim(), 1, Math.Round(type1.Price) * 0.8, type1.Id, 1, list1[0].ProductAttribute, list1[0].PicUrl, type1.SKU.ToUpper().Trim() });
                    str4 = type1.SKU.Trim();
                }
                str2_ByDirect = str2_ByDirect + "</table>";
                return base.Json(new { IsSuccess = false, Result = str2_ByDirect, sku = str4.Trim(), sku1 = o.Trim() });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        private DataSet GetOrderExport(string sql)
        {
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = sql + " order by O.OrderExNo,O.OrderNo asc";
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return dataSet;
        }
        private DataSet GetOrderExportDe(string sql)
        {
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = sql;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return dataSet;
        }

        [HttpPost]
        public ActionResult GExport(string f)
        {
            try
            {
                int intColCount = 0;
                var mydt = new DataTable("myTableName");
                DataColumn mydc;
                DataRow mydr;
                int col = 0;

                var csvReader = new CsvReader(f, Encoding.Default);
                List<string[]> liststrs = csvReader.ReadAllRow();
                string ids = "";
                for (int i = 0; i < liststrs.Count; i++)
                {
                    string[] aryline = liststrs[i];
                    if (i == 0)
                    {
                        for (int j = 0; j < aryline.Length; j++)
                        {
                            if (aryline[j] == "Cart no.")
                            {
                                col = j;
                            }
                            mydc = new DataColumn(aryline[j]);
                            mydt.Columns.Add(mydc);
                        }
                    }
                    else
                    {
                        mydr = mydt.NewRow();
                        for (int j = 0; j < mydt.Columns.Count; j++)
                        {
                            if (j == col)
                            {
                                ids += aryline[j] + ",";
                            }
                            if (aryline.Length > j)
                                mydr[j] = aryline[j];
                        }
                        mydt.Rows.Add(mydr);
                    }
                }


                ids = ids.Trim(',');
                ids = ids.Replace(",", "','");
                List<OrderType> list =
                    NSession.CreateQuery("from OrderType where OrderExNo in('" + ids + "')").List<OrderType>().ToList();
                foreach (DataRow dataRow in mydt.Rows)
                {
                    OrderType order =
                        list.Find(p => p.OrderExNo.Trim().ToUpper() == dataRow["Cart no."].ToString().Trim().ToUpper());
                    if (order != null)
                    {
                        dataRow["Tracking no."] = order.TrackCode;
                        if (string.IsNullOrEmpty(order.TrackCode))
                        {
                            dataRow["Tracking no."] = order.TrackCode2;
                        }
                        dataRow["Delivery company"] = "Chinapost registered airmail";
                    }
                }
                var ds = new DataSet();
                ds.Tables.Add(mydt);
                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
                return Json(new { IsSuccess = true });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, ErrorMsg = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Import(FormCollection form)
        {
            try
            {
                string str = form["Platform"];
                string str2 = form["Account"];
                AccountType account = base.NSession.Get<AccountType>(Convert.ToInt32(str2));
                string fileName = form["hfile"];
                List<ResultInfo> list = new List<ResultInfo>();
                switch (((PlatformEnum)Enum.Parse(typeof(PlatformEnum), str)))
                {
                    case PlatformEnum.WebSite:
                        list = OrderHelper.ImportByB2C(account, fileName, base.NSession);
                        break;

                    case PlatformEnum.Gmarket:
                        list = OrderHelper.ImportByGmarket(account, fileName, base.NSession);
                        break;

                    case PlatformEnum.Amazon:
                        list = OrderHelper.ImportByAmazon(account, fileName, base.NSession);
                        break;

                    case PlatformEnum.Aliexpress:
                        list = OrderHelper.ImportBySMT(account, fileName, base.NSession);
                        break;
                    case PlatformEnum.Lazada:
                        list = OrderHelper.ImportByLazada(account, fileName, base.NSession);
                        break;
                    case PlatformEnum.Wish:
                        list = OrderHelper.ImportByWish(account, fileName, base.NSession);
                        break;
                    case PlatformEnum.Cdiscount:
                        list = OrderHelper.ImportByCdiscount(account, fileName, base.NSession);
                        break;
                }
                base.Session["Results"] = list;
                return base.Json(new { IsSuccess = true, Info = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = exception.Message, Info = true });
            }
        }

        [HttpPost]
        public ActionResult ImportAmount(FormCollection form)
        {
            string str = form["Platform"];
            string str2 = form["Account"];
            AccountType account = base.NSession.Get<AccountType>(Convert.ToInt32(str2));
            string fileName = form["hfile"];
            OrderHelper.ImportByAmount(account, fileName, base.NSession);
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 海外仓发货清单导入
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="wid"></param>
        /// <returns></returns>
        public ActionResult ImportFBAData(string filename, int wid)
        {
            // test
            //Utilities.SetComposeStock("BS12400YE", base.CurrentUser.Realname, base.NSession);

            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where OrderExNo='" + row[0].ToString().Trim() + "' Or TId='" + row[0].ToString().Trim() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }

                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "海个仓订单跟踪码导入", "设置发货单号为：" + orderType.TrackCode + " 替换为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.TrackCode = row[1].ToString();
                        //orderType.Status = "已发货";
                        orderType.IsFreight = 1;
                        orderType.Freight = 0.01;
                        NSession.Update(orderType);
                        NSession.Flush();
                        // 取消海外仓发货导入扣库存2016-05-04
                        //List<OrderProductType> orderProductTypes =
                        //    NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List
                        //        <OrderProductType>().ToList();
                        //foreach (var orderProductType in orderProductTypes)
                        //{
                        //    Utilities.StockOut(wid, orderProductType.SKU, orderProductType.Qty, "海外仓发货",
                        //                       GetCurrentAccount().Realname, "", orderType.OrderNo, NSession);
                        //}
                        // 上传跟踪码
                        UploadTrackCode(orderType, NSession);

                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "订单标记发货", "成功"));
                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        public ActionResult ImportOrderProductFees(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where OrderNo='" + row[0].ToString().Trim() + "' Or OrderExNo='" + row[0].ToString().Trim() + "' Or TId='" + row[0].ToString().Trim() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;
                    }

                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "订单产品费用上传", "设置订单产品费用为：" + orderType.TrackCode, CurrentUser, NSession);
                        orderType.ProductFees = Utilities.ToDouble(row[1]);

                        NSession.Update(orderType);
                        NSession.Flush();

                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }
        public JsonResult ImportDispute(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            //判断是哪个平台的数据
            Hashtable ht = new Hashtable();
            DataTable dt1 = new DataTable();
            string platform = dt.Rows[0][0].ToString();
            if (platform == "smt" || platform == "lazada")
            {
                //合并相同纠纷金额
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (ht.ContainsKey(dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString()))
                    {

                        //获取行索引
                        int index = (int)ht[dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString()];
                        //获取最近一次的值(对应values)
                        float value = float.Parse(dt.Rows[index][3].ToString());
                        //累计
                        dt.Rows[index][3] = value + float.Parse(dt.Rows[i][3].ToString());
                        //删除重复行
                        dt.Rows.RemoveAt(i);
                        //调整索引减1
                        i--;

                    }
                    else
                    {
                        //保存名称以及行索引
                        ht.Add(dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString(), i);
                    }
                }
            }
            if (dt.Columns.Count >= 3)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    string orderno = row[1].ToString().Trim();
                    string Sku = row[2].ToString().Trim();
                    decimal ExamineAmount = Convert.ToDecimal(row[3].ToString().Trim());
                    string ExamineCurrencyCode = row[4].ToString().Trim();
                    string Remark = row[5].ToString().Trim();
                    string RefundDate = row[6].ToString().Trim();
                    string Area = row[7].ToString().Trim();
                    List<DisputeRecordType> disputetypes = NSession.CreateQuery("from DisputeRecordType where OrderNo='" + orderno + "'").List<DisputeRecordType>().ToList();
                    //DisputeRecordType disputetype = disputetypes.Find(x => x.OrderNo == orderno);
                    int flag = 1;
                    //如果纠纷表数据为空,需要再次插入数据
                    if (disputetypes != null || disputetypes.Count > 0)
                    {
                        foreach (DisputeRecordType disputetype in disputetypes)
                        {
                            //如果纠纷表有数据而且日期不一致的话才能导入而且金额叠加
                            if (disputetype.RefundDate == RefundDate)
                            {
                                flag = 0;
                                resultInfos.Add(OrderHelper.GetResult(orderno, "该订单已经导入纠纷金额，禁止再次导入", "失败"));
                                break;
                            }
                        }
                    }



                    if (flag == 1)
                    {
                        DisputeRecordType recordtype = new DisputeRecordType();
                        recordtype = new DisputeRecordType();
                        recordtype.OrderNo = orderno;
                        recordtype.SKU = Sku;
                        recordtype.Remark = Remark;
                        recordtype.ExamineStatus = 5;
                        recordtype.Area = GetCurrentAccount().FromArea;
                        recordtype.CreateOn = DateTime.Now;
                        recordtype.CreateBy = GetCurrentAccount().Realname;
                        recordtype.Area = Area;
                        recordtype.DisputeState = "未处理,平台付款";
                        recordtype.RefundDate = RefundDate;
                        recordtype.ExamineAmount = ExamineAmount;
                        List<OrderType> order = NSession.CreateQuery("from OrderType where OrderExNo='" + orderno + "'").List<OrderType>().ToList();
                        if (order.Count <= 0)
                        {
                            resultInfos.Add(OrderHelper.GetResult(orderno, "该单号不在系统中", "失败"));
                        }
                        else
                        {
                            double orderamount = 0;
                            //如果存在多笔订单的情况，订单金额取最大的一笔
                            if (order.Count > 1)
                            {
                                foreach (OrderType order1 in order)
                                {

                                    if (orderamount < order1.Amount)
                                    {
                                        orderamount = order1.Amount;
                                    }
                                }

                            }
                            else
                            {
                                orderamount = order[0].Amount;
                            }
                            recordtype.Account = order[0].Account;
                            recordtype.OrderAmount2 = Convert.ToDecimal(orderamount);
                            recordtype.OrderAmount = Convert.ToDecimal(orderamount);
                            LoggerUtil.GetOrderRecord(order[0], "订单添加纠纷", " 备注：" + Remark + " SKU：" + order[0].TrackCode, CurrentUser, NSession);
                            resultInfos.Add(OrderHelper.GetResult(row[1].ToString(), "插入数据", "成功"));
                            decimal examount = recordtype.ExamineAmount;
                            if (ExamineCurrencyCode != "CNY")
                            {
                                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == ExamineCurrencyCode);
                                recordtype.ExamineAmountRmb = recordtype.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                            }
                            else
                            {
                                //导入币种为人民币的时候，将订单金额转成美元，计算比例
                                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order[0].CurrencyCode);
                                examount = recordtype.ExamineAmount / Convert.ToDecimal(currencyType.CurrencyValue);
                                recordtype.ExamineAmountRmb = recordtype.ExamineAmount;
                            }
                            recordtype.ExamineOn = DateTime.Now;
                            recordtype.ExamineCurrencyCode = ExamineCurrencyCode;
                            recordtype.IsImport = 1;
                            if (orderamount != 0)
                            {
                                recordtype.Rate = Math.Round(examount / Convert.ToDecimal(orderamount) * 100, 2);
                                recordtype.DisputeState = "部分退款(平台) ";
                                if (platform == "smt")//导入纠纷数据平台信息列“smt”与平台账户表“Aliexpress”不匹配，导致数据报表II 平台纠纷统计不符【修复纠纷导入】
                                {
                                    recordtype.Platform = "Aliexpress";
                                }
                                else
                                {
                                    recordtype.Platform = platform;
                                }
                                if (platform == "ebay" || platform == "lazada" && recordtype.Rate >= 90)
                                {

                                    recordtype.DisputeState = "全部退款(平台) ";

                                }
                                else if (platform == "smt" && recordtype.Rate >= 94)
                                {
                                    recordtype.DisputeState = "全部退款(平台) ";
                                }
                                else if (platform == "wish" && recordtype.Rate >= 84)
                                {
                                    recordtype.DisputeState = "全部退款(平台) ";
                                }
                                else if (platform == "amazon" && recordtype.Rate >= 84)
                                {
                                    recordtype.DisputeState = "全部退款(平台) ";
                                }
                            }
                            //if (recordtype.OrderAmount2 <= recordtype.ExamineAmount)
                            //{
                            //    recordtype.DisputeState = "全部退款(平台) ";
                            //}
                            //else
                            //{
                            //    recordtype.DisputeState = "部分退款(平台) ";

                            //}
                            NSession.SaveOrUpdate(recordtype);


                            NSession.Flush();

                        }
                    }




                }
            }



            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });

        }

        public JsonResult ImportDisputeebay(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            Hashtable ht = new Hashtable();
            DataTable dt1 = new DataTable();
            //合并相同纠纷金额
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (ht.ContainsKey(dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString()))
                {

                    //获取行索引
                    int index = (int)ht[dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString()];
                    //获取最近一次的值(对应values)
                    float value = float.Parse(dt.Rows[index][3].ToString());
                    //累计
                    dt.Rows[index][3] = value + float.Parse(dt.Rows[i][3].ToString());
                    //删除重复行
                    dt.Rows.RemoveAt(i);
                    //调整索引减1
                    i--;

                }
                else
                {
                    //保存名称以及行索引
                    ht.Add(dt.Rows[i][1].ToString() + dt.Rows[i][6].ToString(), i);
                }
            }

            if (dt.Columns.Count >= 3)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    string orderno = row[1].ToString().Trim();
                    string Sku = row[2].ToString().Trim();
                    decimal ExamineAmount = Convert.ToDecimal(row[3].ToString().Trim());
                    string ExamineCurrencyCode = row[4].ToString().Trim();
                    string Remark = row[5].ToString().Trim();
                    string RefundDate = row[6].ToString().Trim();
                    string Area = row[7].ToString().Trim();
                    List<DisputeRecordType> disputetypes = NSession.CreateQuery("from DisputeRecordType where OrderNo='" + orderno + "'").List<DisputeRecordType>().ToList();
                    //DisputeRecordType disputetype = disputetypes.Find(x => x.OrderNo == orderno);
                    int flag = 1;
                    //如果纠纷表数据为空,需要再次插入数据
                    if (disputetypes != null || disputetypes.Count > 0)
                    {
                        foreach (DisputeRecordType disputetype in disputetypes)
                        {
                            //如果纠纷表有数据而且日期不一致的话才能导入而且金额叠加
                            if (disputetype.RefundDate == RefundDate)
                            {
                                flag = 0;
                                resultInfos.Add(OrderHelper.GetResult(orderno, "该订单已经导入纠纷金额，禁止再次导入", "失败"));
                                break;
                            }
                        }
                    }



                    if (flag == 1)
                    {
                        DisputeRecordType recordtype = new DisputeRecordType();
                        recordtype = new DisputeRecordType();
                        recordtype.OrderNo = orderno;
                        recordtype.SKU = Sku;
                        recordtype.Remark = Remark;
                        recordtype.ExamineStatus = 5;
                        recordtype.Area = GetCurrentAccount().FromArea;
                        recordtype.CreateOn = DateTime.Now;
                        recordtype.CreateBy = GetCurrentAccount().Realname;
                        recordtype.Area = Area;
                        recordtype.DisputeState = "未处理,平台付款";
                        recordtype.RefundDate = RefundDate;
                        recordtype.ExamineAmount = ExamineAmount;
                        List<OrderType> order = NSession.CreateQuery("from OrderType where TId='" + orderno + "'").List<OrderType>().ToList();
                        if (order.Count <= 0)
                        {
                            resultInfos.Add(OrderHelper.GetResult(orderno, "该单号不在系统中", "失败"));
                        }
                        else
                        {
                            double orderamount = 0;
                            //如果存在多笔订单的情况，订单金额取最大的一笔
                            if (order.Count > 1)
                            {
                                foreach (OrderType order1 in order)
                                {

                                    if (orderamount < order1.Amount)
                                    {
                                        orderamount = order1.Amount;
                                    }
                                }

                            }
                            else
                            {
                                orderamount = order[0].Amount;
                            }
                            recordtype.Account = order[0].Account;
                            recordtype.OrderAmount2 = Convert.ToDecimal(orderamount);
                            recordtype.OrderAmount = Convert.ToDecimal(orderamount);
                            LoggerUtil.GetOrderRecord(order[0], "订单添加纠纷", " 备注：" + Remark + " SKU：" + order[0].TrackCode, CurrentUser, NSession);
                            resultInfos.Add(OrderHelper.GetResult(row[1].ToString(), "插入数据", "成功"));
                            decimal examount = recordtype.ExamineAmount;
                            if (ExamineCurrencyCode != "CNY")
                            {
                                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == ExamineCurrencyCode);
                                recordtype.ExamineAmountRmb = recordtype.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                            }
                            else
                            {
                                //导入币种为人民币的时候，将订单金额转成美元，计算比例
                                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order[0].CurrencyCode);
                                examount = recordtype.ExamineAmount / Convert.ToDecimal(currencyType.CurrencyValue);

                                recordtype.ExamineAmountRmb = recordtype.ExamineAmount;
                            }
                            recordtype.ExamineOn = DateTime.Now;
                            recordtype.ExamineCurrencyCode = ExamineCurrencyCode;
                            recordtype.IsImport = 1;
                            if (orderamount != 0)
                            {
                                recordtype.Rate = Math.Round(examount / Convert.ToDecimal(orderamount) * 100, 2);
                                recordtype.DisputeState = "部分退款(平台) ";
                                recordtype.Platform = "ebay";
                                if (recordtype.Rate >= 90)
                                {

                                    recordtype.DisputeState = "全部退款(平台) ";

                                }
                            }
                            //if (recordtype.OrderAmount2 <= recordtype.ExamineAmount)
                            //{
                            //    recordtype.DisputeState = "全部退款(平台) ";
                            //}
                            //else
                            //{
                            //    recordtype.DisputeState = "部分退款(平台) ";

                            //}
                            NSession.SaveOrUpdate(recordtype);


                            NSession.Flush();

                        }
                    }




                }
            }



            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });

        }

        /// <summary>
        ///  收汇导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult ImportFanAmount(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);

            Hashtable ht = new Hashtable();
            //合并相同订单收汇金额
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (ht.ContainsKey(dt.Rows[i][0]))
                {
                    //获取行索引
                    int index = (int)ht[dt.Rows[i][0]];
                    //获取最近一次的值(对应values)
                    float value = float.Parse(dt.Rows[index][1].ToString());
                    //累计
                    dt.Rows[index][1] = value + float.Parse(dt.Rows[i][1].ToString());
                    //删除重复行
                    dt.Rows.RemoveAt(i);
                    //调整索引减1
                    i--;
                }
                else
                {
                    //保存名称以及行索引
                    ht.Add(dt.Rows[i][0], i);
                }
            }

            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        if (row[0] == null)
                    {
                        continue;
                    }
                    //List<OrderType> orderTypes =
                    //NSession.CreateQuery("from OrderType where IsFBA=1 and OrderExNo='" + row[0].ToString().Trim() + "'").List
                    //<OrderType>().ToList();
                    NSession.Clear();
                    //List<OrderType> orderTypes =
                    //    NSession.CreateQuery("from OrderType where OrderExNo='" + row[0].ToString().Trim() + "' or Tid='" + row[0].ToString().Trim() + "'").List<OrderType>().ToList();

                    // 增加平台单号前加前缀判断单号2017-05-03
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where OrderExNo='" + row[0].ToString().Trim() + "' or OrderExNo=Account+'_'+'" + row[0].ToString().Trim() + "' or Tid='" + row[0].ToString().Trim() + "'").List<OrderType>().ToList();

                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }

                    // 同时需要判断关联订单内是否导入,并标记关联订单导入状态??
                    // 订单已导入过时禁止再次导入
                    if (orderTypes[0].FanAmount > Convert.ToDecimal(0.01))
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号已导入收汇金额禁止再次导入", "失败"));
                        continue;
                    }

                    // 判断是否被合并订单，并将收汇金额合并至主合并订单金额内??

                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "收汇导入", "设置收汇金额为：" + orderType.FanAmount + " 更新为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.FanAmount = decimal.Parse(row[1].ToString());
                        orderType.FanState = 1;
                        if (orderType.IsFBA == 1 && orderType.IsFreight == 0) //满足海外仓订单 且 运费从未导入过
                        {
                            orderType.Freight = 0; // FBA收汇订单导入自动运费标记为零
                        }
                        // 计算订单财务数据
                        //base.NSession.CreateSQLQuery("UP_ReckonFinance '" + type.OrderNo + "'").ExecuteUpdate();
                        OrderHelper.ReckonFinance(orderType, NSession);
                        NSession.Update(orderType);
                        NSession.Flush();

                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "收汇导入", "成功"));
                    }
                
                    }
                    catch (Exception exc)
                    {

                        continue;
                    }

                }
            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        ///  通途wish订单导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult ImportWishOrder(string filename)
        {

            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 61 && dt.Rows.Count >= 10)
            {
                foreach (DataRow row in dt.Rows)
                {
                    //判断是否是包含订单数据的行
                    if (row[0] == null || row[16].ToString() == "" || row[16].ToString() == "订单号")
                    {
                        continue;
                    }
                    //去掉订单号前缀"WISH0XT-"
                    string OrderNo = row[16].ToString().Trim();
                    if (OrderNo.Contains('-'))
                    {
                        string[] OrderNos = OrderNo.Split('-');
                        OrderNo = OrderNos[1];
                    }

                    // 移除拆分订单号后缀_1，收汇导平台单号与通途单号匹配
                    if (OrderNo.Contains("_1"))
                    {
                        OrderNo = OrderNo.Replace("_1", "");
                    }

                    List<DeliveryListWishType> deliverylistwishs = NSession.CreateQuery("from DeliveryListWishType where OrderNo='" + OrderNo + "'").List<DeliveryListWishType>().ToList();
                    //如果wish订单表数据为空,需要再次插入数据
                    if (deliverylistwishs.Count > 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(OrderNo, "该订单已经导入，禁止再次导入", "失败"));
                    }
                    else
                    {
                        try
                        {
                            DeliveryListWishType wishtype = new DeliveryListWishType();
                            //把日期和时间拼合
                            wishtype.SendOn = Convert.ToDateTime(Convert.ToDateTime(row[0].ToString().Trim()).ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(row[1].ToString().Trim()).ToString("HH:mm:ss"));
                            wishtype.SendBy = row[2].ToString().Trim();
                            wishtype.IsReissue = row[3].ToString().Trim();
                            wishtype.SKU = row[4].ToString().Trim();
                            wishtype.Qty = Convert.ToDouble(row[5].ToString().Trim());
                            wishtype.Weight = Convert.ToDouble(row[6].ToString().Trim());
                            wishtype.Platform = row[7].ToString().Trim();
                            wishtype.Account = row[8].ToString().Trim();
                            wishtype.SalesSite = row[9].ToString().Trim();
                            wishtype.Warehouse = row[10].ToString().Trim();
                            wishtype.PackageNo = row[11].ToString().Trim();
                            wishtype.Logistics = row[12].ToString().Trim();
                            wishtype.TotalWeight = Convert.ToDouble(row[13].ToString().Trim());
                            wishtype.TotalFreight = Convert.ToDecimal(row[14].ToString().Trim());
                            wishtype.TrackCode = row[15].ToString().Trim();
                            wishtype.OrderNo = OrderNo;
                            wishtype.ItemId = row[17].ToString().Trim();
                            wishtype.ItemTitle = row[18].ToString().Trim();
                            wishtype.BuyerId = row[19].ToString().Trim();
                            wishtype.BuyerName = row[20].ToString().Trim();
                            wishtype.Country = row[21].ToString().Trim();
                            wishtype.Address1 = row[22].ToString().Trim();
                            wishtype.Address2 = row[23].ToString().Trim();
                            wishtype.City = row[24].ToString().Trim();
                            wishtype.Province = row[25].ToString().Trim();
                            wishtype.PostCode = row[26].ToString().Trim();
                            wishtype.Phone = row[27].ToString().Trim();
                            wishtype.Telphone = row[28].ToString().Trim();
                            wishtype.FullAddress = row[29].ToString().Trim();
                            wishtype.PayOn = Convert.ToDateTime(Convert.ToDateTime(row[30].ToString().Trim()).ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(row[31].ToString().Trim()).ToString("HH:mm:ss"));
                            wishtype.SaleOn = Convert.ToDateTime(Convert.ToDateTime(row[32].ToString().Trim()).ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(row[33].ToString().Trim()).ToString("HH:mm:ss"));
                            wishtype.CollectionPayPal = row[34].ToString().Trim();
                            wishtype.PaymentPayPal = row[35].ToString().Trim();
                            wishtype.Merchandiser = row[36].ToString().Trim();
                            wishtype.ProductDeveloper = row[37].ToString().Trim();
                            wishtype.InquiryClerk = row[38].ToString().Trim();
                            wishtype.PurchasingAgent = row[39].ToString().Trim();
                            wishtype.CurrencyCode = row[40].ToString().Trim();
                            wishtype.TotalPrice = Convert.ToDecimal(row[41].ToString().Trim());
                            wishtype.TotalPriceRMB = Convert.ToDecimal(row[42].ToString().Trim());
                            wishtype.Price = Convert.ToDecimal(row[43].ToString().Trim());
                            wishtype.PriceRMB = Convert.ToDecimal(row[44].ToString().Trim());
                            wishtype.Cost = Convert.ToDecimal(row[45].ToString().Trim());
                            wishtype.PlatformDealCurrency = row[46].ToString().Trim();
                            wishtype.PlatformDealFee = Convert.ToDecimal(row[47].ToString().Trim());
                            wishtype.PlatformDealFeeRMB = Convert.ToDecimal(row[48].ToString().Trim());
                            wishtype.PayPalFeeCurrency = row[49].ToString().Trim();
                            wishtype.PayPalFee = Convert.ToDecimal(row[50].ToString().Trim());
                            wishtype.PayPalFeeRMB = Convert.ToDecimal(row[51].ToString().Trim());
                            wishtype.PlatformFee = Convert.ToDecimal(row[52].ToString().Trim());
                            wishtype.FirstLegLogistics = row[53].ToString().Trim();
                            wishtype.FirstLegFreight = Convert.ToDecimal(row[54].ToString().Trim());
                            wishtype.FirstLegDeclarationFee = Convert.ToDecimal(row[55].ToString().Trim());
                            wishtype.PackingMaterial = row[56].ToString().Trim();
                            wishtype.PackagingFee = Convert.ToDecimal(row[57].ToString().Trim());
                            wishtype.Freight = Convert.ToDecimal(row[58].ToString().Trim());
                            wishtype.Profit = Convert.ToDecimal(row[59].ToString().Trim());
                            //去掉百分号
                            string ProfitRateStr = (row[60].ToString().Trim());
                            ProfitRateStr = ProfitRateStr.Substring(0, ProfitRateStr.Length - 1);
                            wishtype.ProfitRate = Convert.ToDouble(ProfitRateStr) / 100;
                            wishtype.Area = "宁波";
                            wishtype.Operator = GetCurrentAccount().Realname;
                            wishtype.OperateTime = DateTime.Now.ToString();
                            resultInfos.Add(OrderHelper.GetResult(OrderNo, "插入数据", "成功"));

                            NSession.SaveOrUpdate(wishtype);
                            NSession.Flush();
                        }
                        catch (Exception e)
                        {

                            // 失败
                            resultInfos.Clear();
                            resultInfos.Add(OrderHelper.GetResult(OrderNo, "该订单格式错误，无法导入", "失败"));
                            LoggerUtil.GetOrderRecord(0, OrderNo, "通途数据导入", e.Message.ToString(), GetCurrentAccount(), NSession);
                            continue;

                        }
                    }
                }
            }


            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });

        }

        /// <summary>
        ///  通途wish运费导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult ImportWishFreight(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<DeliveryListWishType> deliverylistwishs = NSession.CreateQuery("from DeliveryListWishType where TrackCode='" + row[0].ToString() + "'").List<DeliveryListWishType>().ToList();
                    if (deliverylistwishs.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该订单不在系统中", "失败"));
                        continue;
                    }

                    foreach (var wishtype in deliverylistwishs)
                    {
                        if (wishtype.IsFreight == 1)
                        {
                            resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号运费已导入系统中", "失败"));
                            continue;
                        }
                        wishtype.ActualFreight = decimal.Parse(row[1].ToString());
                        wishtype.IsFreight = 1; // 标记运费已导入
                        NSession.Update(wishtype);
                        NSession.Flush();
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "运费导入", "成功"));

                    }
                }
            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        ///  通途wish收汇导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult ImportWishFanAmount(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    string OrderNo = row[0].ToString().Trim();
                    List<DeliveryListWishType> deliverylistwishs = NSession.CreateQuery("from DeliveryListWishType where OrderNo='" + OrderNo + "'").List<DeliveryListWishType>().ToList();
                    if (deliverylistwishs.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(OrderNo, "该单号不在系统中", "失败"));
                        continue;
                    }
                    foreach (var wishtype in deliverylistwishs)
                    {
                        if (wishtype.IsFan == 1)
                        {
                            resultInfos.Add(OrderHelper.GetResult(OrderNo, "该单号已导入收汇金额禁止再次导入", "失败"));
                            continue;
                        }
                        wishtype.FanAmount = decimal.Parse(row[1].ToString());
                        wishtype.IsFan = 1; // 标记收汇已导入
                        NSession.Update(wishtype);
                        NSession.Flush();
                        resultInfos.Add(OrderHelper.GetResult(OrderNo, "收汇导入", "成功"));

                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        public JsonResult ImportWishDispute(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 4)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    string orderno = row[0].ToString().Trim();
                    decimal ExamineAmount = Convert.ToDecimal(row[1].ToString().Trim());
                    string Area = row[2].ToString().Trim();
                    string Account = row[3].ToString().Trim();
                    string ExamineOn = row[4].ToString().Trim();

                    List<WishDisputeRecordType> disputetypes = NSession.CreateQuery("from WishDisputeRecordType where OrderNo='" + orderno + "'").List<WishDisputeRecordType>().ToList();

                    //如果纠纷表数据为空,需要再次插入数据
                    if (disputetypes.Count > 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(orderno, "该订单已经导入纠纷金额，禁止再次导入", "失败"));
                        continue;
                    }
                    WishDisputeRecordType recordtype = new WishDisputeRecordType();
                    recordtype = new WishDisputeRecordType();
                    recordtype.OrderNo = orderno;
                    recordtype.Area = Area;
                    recordtype.Account = Account;
                    recordtype.ExamineStatus = "6";
                    recordtype.ExamineAmount = ExamineAmount;
                    recordtype.CreateOn = DateTime.Now;
                    recordtype.CreateBy = GetCurrentAccount().Realname;
                    recordtype.ExamineOn = Convert.ToDateTime(ExamineOn);
                    List<DeliveryListWishType> order = NSession.CreateQuery("from DeliveryListWishType where OrderNo='" + orderno + "'").List<DeliveryListWishType>().ToList();
                    if (order.Count <= 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(orderno, "该单号不在系统中", "失败"));
                    }
                    else
                    {
                        decimal orderamount = order[0].TotalPrice;
                        resultInfos.Add(OrderHelper.GetResult(orderno, "插入数据", "成功"));
                        if (ExamineAmount - (decimal.Multiply(orderamount, (decimal)0.85)) < 0)
                        {
                            recordtype.DisputeState = "部分退款(平台) ";
                        }
                        else
                        {
                            recordtype.DisputeState = "全部退款(平台) ";
                        }
                        NSession.SaveOrUpdate(recordtype);
                        NSession.Flush();
                    }
                }
            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }



        /// <summary>
        /// 订单重量导入
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="wid"></param>
        /// <returns></returns>
        public ActionResult ImportWeight(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where TrackCode='" + row[0].ToString() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }

                    foreach (var orderType in orderTypes)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "订单重量更新", "设置重量为：" + orderType.Weight + " 替换为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.Weight = int.Parse(row[1].ToString());
                        NSession.Update(orderType);
                        NSession.Flush();
                        //  UploadTrackCode(orderType, NSession);

                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "订单重量更新", "成功"));
                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        /// 其它费用导入
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="wid"></param>
        /// <returns></returns>
        public ActionResult ImportOtherExpenses(string filename)
        {
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }

                    OtherExpensesType OtherExpenses = new OtherExpensesType
                    {
                        Platform = row[0].ToString(),
                        Account = row[1].ToString(),
                        Currency = row[2].ToString(),
                        Amount = Convert.ToDouble(row[3].ToString()),
                        Remarks = row[4].ToString(),
                        ProcessDate = Convert.ToDateTime(row[5].ToString())
                    };
                    NSession.Save(OtherExpenses);
                    NSession.Flush();
                    resultInfos.Add(OrderHelper.GetResult(row[1].ToString(), "其它费用导入", "成功"));
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }

        /// <summary>
        ///  义乌邮局挂号交运
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult ImportNBGHUtil(string filename)
        {
            int t = 2;
            DataTable dt = OrderHelper.GetDataTable(filename);
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 1)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where TrackCode='" + row[0].ToString() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }
                    foreach (var orderType in orderTypes)
                    {
                        orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                        orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                        string kk = row[0].ToString().Substring(0, 2);
                        if (row[0].ToString().Substring(0, 2) == "LF")
                        {
                            t = 3;
                        }
                        else if (row[0].ToString().Substring(0, 2) == "RU")
                        {
                            t = 2;
                        }
                        else
                        {
                            t = 4;
                        }
                        string o = NBGHUtil.CreateOrder(orderType, t);

                        if (o != null)
                        {
                            if (o == "true")
                            {
                                resultInfos.Add(OrderHelper.GetResult(orderType.OrderNo, "上传成功", "成功"));
                            }
                            else
                            {
                                // orderType.TrackCode = o;
                                if (o == "falseB08")
                                {
                                    resultInfos.Add(OrderHelper.GetResult(orderType.OrderNo, "已经上传，不能再次上传", "成功"));
                                }
                                else
                                {
                                    resultInfos.Add(OrderHelper.GetResult(orderType.OrderNo, "上传失败", "失败"));
                                }
                                NSession.Update(orderType);
                                NSession.Flush();
                            }

                        }
                    }
                }

            }
            base.Session["Results"] = resultInfos;
            return base.Json(new { IsSuccess = true, Info = true });
        }
        public ActionResult Index()
        {

            return View();

        }

        public ActionResult IsNotQue()
        {
            return base.View();
        }

        public ActionResult JiScan()
        {
            return base.View();
        }

        public ActionResult ReBJScan()
        {
            return base.View();
        }



        #region 外部物流

        /// <summary>
        /// 获得淼信跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public JsonResult GetMiaoxinTrackCode(string ids)
        {
            IdentifyReturnType t = SHMXUtil.Identify();
            string OrderItemIds = ""; // 获取OrderItemsId
            string OrderItemIds1 = ""; // 获取OrderItemsId 第一个
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                OrderReturnType o = new OrderReturnType();
                o = SHMXUtil.GetOrderReturn(t, orderType);
                if (o != null)
                {
                    string trackcode = o.tracking_number;
                    if (o.ack)
                    {
                        orderType.TrackCode = trackcode.Split(' ')[0].ToString();  // TrackingCode
                        orderType.OrderExNo = o.order_id; // PackageId
                    }
                    else
                    {

                    }
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 获得燕文跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetYWTrackCode(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string str = YWUtil.GetTrackCode(orderType, t);

                if (str != null && str != "您的订单号不可重复")
                {
                    string trackcode = str;
                    orderType.TrackCode = trackcode;
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 获得wish邮跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetWishMailTrackCode(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string str = WishMailUtil.GetTrackCode(orderType, t);

                if (str != null)
                {
                    string trackcode = str;
                    orderType.TrackCode = trackcode;
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 获得义乌wish邮跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetYWWishMailTrackCode(string ids, int t)
        {
            
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                //  string aa = YWWishMailUtil.RefreshToken(1);
                foreach (var orderProductType in orderType.Products)
                {
                    IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();
                    pinfo += "[" + orderProductType.SKU + "](" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")x" + orderProductType.Qty + " ";
                }
                string key = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Email from Account where  AccountName='ywwish01'")).UniqueResult());
                string str = YWWishMailUtil.GetTrackCode(orderType, t, pinfo, key);

                if (str != null)
                {
                    string trackcode = str;
                    if (orderType.TrackCode != "已用完")
                    {
                        LoggerUtil.GetOrderRecord(orderType, "wish邮获取", "单号" + orderType.TrackCode + "替换为" + trackcode,base.CurrentUser, base.NSession);
                    }
                    orderType.TrackCode = trackcode;    
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 获得Lazada跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetLazadaTrackCode(string ids, int t)
        {
            string OrderItemIds = ""; // 获取OrderItemsId
            string OrderItemIds1 = ""; // 获取OrderItemsId 第一个
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            foreach (OrderType orderType in orders)
            {
                List<AccountType> account = base.NSession.CreateQuery("from AccountType where AccountName='" + orders[0].Account + "'").List<AccountType>().ToList();
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                foreach (OrderProductType product in orderType.Products)
                {
                    //if (orderType.Products.IndexOf(product) == 0)
                    //{
                    //    // 第一个itemid
                    //    OrderItemIds1 = product.ImgUrl;
                    //}
                    //else
                    //{
                    // 剩于itemid
                    OrderItemIds += "," + product.ImgUrl;
                    //}
                }
                // 使用send_to_warehouse测试
                string str = LazadaUtil.GetTrackCode(account[0], orderType, OrderItemIds);
                if (str != null)
                {
                    string trackcode = str;
                    orderType.TrackCode = trackcode.Split(' ')[0].ToString();  // TrackingCode
                    orderType.TrackCode2 = trackcode.Split(' ')[1].ToString(); // PackageId
                    NSession.Update(orderType);
                    NSession.Flush();
                }
                //// 设置其它商品
                //if (OrderItemIds.Length > 0)
                //{
                //    OrderItemIds = OrderItemIds.Substring(1, OrderItemIds.Length - 1);
                //    LazadaUtil.SetTrackCode(account[0], orderType, OrderItemIds);
                //}
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 燕文物流(宁波)
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public JsonResult GetYWTrackCode1(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string str = YWUtil.GetTrackCode1(orderType, t);

                if (str != null && str != "您的订单号不可重复")
                {
                    string trackcode = str;
                    orderType.TrackCode = trackcode;
                    NSession.Update(orderType);
                    NSession.Flush();
                }

            }
            return Json(new { IsSuccess = true });
        }

        public ActionResult GetYWPDF(string t, int p)
        {
            byte[] bytes = YWUtil.GetPDF(t, p);

            Session["pdf"] = bytes;
            return null;

        }
        public JsonResult GetWishMailPDF(string t, int p)
        {
            string url = WishMailUtil.GetPDF(t, p);

            return Json(new { url = url });
        }

        public JsonResult GetYWWishMailPDF(string t, int p,string tt)
        {
            string url = YWWishMailUtil.GetPDF(tt, p);
            string idss = "'" + t.Replace(",", "','") + "'";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + idss + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.IsPrint = 1;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印WISH邮", base.CurrentUser, base.NSession);
                NSession.Update(orderType);
                NSession.Flush();
            }
            return Json(new { url = url });
        }
        public ActionResult GetYWPDF1(string t, int p)
        {
            byte[] bytes = YWUtil.GetPDF1(t, p);

            Session["pdf"] = bytes;
            return null;

        }
        public ActionResult GetYWPDF2(string t, int p)
        {
            byte[] bytes = YWUtil.GetPDF2(t, p);

            Session["pdf"] = bytes;
            return null;

        }

        /// <summary>
        /// 比利时
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetBLSTrackCode(string ids)
        {

            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string str = BLSUtli.GetTrackCode(orderType);

                if (str != null && str.StartsWith("BLVS"))
                {
                    string trackcode = str;
                    orderType.TrackCode = trackcode;
                    NSession.Update(orderType);
                    NSession.Flush();
                }

            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult GetBLSPDF(string t)
        {

            string filePath = "/Uploads/PDF/";
            filePath = filePath + DateTime.Now.ToString("yyyyMMdd") + "/";
            string filePath2 = Server.MapPath("~" + filePath);
            if (!Directory.Exists(filePath2))
            {
                Directory.CreateDirectory(filePath2);
            }
            string fileName = DateTime.Now.Ticks + ".pdf";
            filePath += fileName;
            filePath2 += fileName;
            BLSUtli.GetPDF(t, filePath2);

            return Json(new { IsSuccess = true, f = filePath });

        }

        public ActionResult GetCNETrackCode(string ids, string t)
        {

            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            string chukouyiOrder = "";
            string category = "";
            string productname = "";//商品名称
            string ccountry = "";
            string pinfo = "";
            foreach (OrderType orderType in orders)
            {
                pinfo = "";
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products =
                    NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List<OrderProductType>().
                        ToList();
                //获取某一商品类别
                if (orderType.Account.IndexOf("yw") != -1)//义乌面单增加库位
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();

                        pinfo += "[" + orderProductType.SKU + "(" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")," + orderProductType.Qty + "]";
                        foreach (ProductType type in list)
                        {
                            category = type.Category;
                            productname = type.ProductName;
                        }
                    }
                }
                else
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();
                        pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                        foreach (ProductType type in list)
                        {
                            category = type.Category;
                            productname = type.ProductName;
                        }
                    }

                }

                ccountry = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select CCountry from Country where ECountry='" + orderType.AddressInfo.Country + "'")).UniqueResult());
                CNEResultRootObject rootObject = CNEUtil.GetTracoCode(orderType, t, category, productname, ccountry, pinfo, NSession);
                if (rootObject != null)
                {
                    if (rootObject.ErrList.Count > 0)
                    {
                        string s = rootObject.ErrList[0].cNum;
                        orderType.TrackCode = s;
                        NSession.Update(orderType);
                        NSession.Flush();

                    }
                }
            }
            return Json(new { IsSuccess = true });
        }
        public ActionResult GetCNEPDF(string ids,string ids1)
        {

            string url = CNEUtil.GetPDF(ids1);
            //CNE打单标记打印符号
            string idss = "'" + ids.Replace(",", "','") + "'";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + idss + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.IsPrint = 1;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印CNE", base.CurrentUser, base.NSession);
                NSession.Update(orderType);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true, f = url });
        }



        public ActionResult GetJCPDF(string ids)
        {

            string url = JCUtil.GetPDF(ids);
            //欧亚打单标记打印符号
            string idss = "'" + ids.Replace(",", "','") + "'";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id  in(" + idss + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.IsPrint = 1;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印欧亚速运", base.CurrentUser, base.NSession);
                NSession.Update(orderType);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true, f = url });
        }

        /// <summary>
        /// 出口易
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult GetChuKouLogistics(string ids, string t)
        {
            try
            {
                List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

                string chukouyiOrder = "";
                cn.chukou1.demo.CK1 ck = new CK1();
                foreach (OrderType orderType in orders)
                {
                    orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                    orderType.Products =
                        NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List<OrderProductType>().ToList();

                    ExpressAddPackageNewRequest request = new ExpressAddPackageNewRequest();
                    request.Token = "1C1D66499731BFBD08EDD36BFC04DF3D";
                    request.UserKey = "kuf8dn5u9c";

                    request.ExpressType = ExpressType.UNKNOWN;
                    request.ExpressTypeNew = t;
                    request.IsTracking = true;
                    request.PickupType = 0;
                    request.Location = "SH";
                    //request.PickUpAddress = new PickUpAddress();
                    request.PackageDetail = new ExpressPackage();

                    request.PackageDetail.Weight = 200;
                    request.PackageDetail.Packing = new Packing { Width = 1, Height = 1, Length = 1 };
                    request.PackageDetail.Status = OrderExpressState.Submitted;
                    request.PackageDetail.RefNo = orderType.OrderNo;
                    request.PackageDetail.ShipToAddress = new cn.chukou1.demo.ShipToAddress { City = orderType.AddressInfo.City, Company = "", Contact = orderType.AddressInfo.Addressee, Country = orderType.AddressInfo.Country, Email = orderType.BuyerEmail, Phone = orderType.AddressInfo.Phone, PostCode = orderType.AddressInfo.PostCode, Province = orderType.AddressInfo.Province, Street1 = orderType.AddressInfo.Street, Street2 = "" };
                    request.PackageDetail.ProductList = new ExpressProduct[orderType.Products.Count];
                    string skuinfo = "";
                    for (int i = 0; i < orderType.Products.Count; i++)
                    {
                        request.PackageDetail.ProductList[i] = new ExpressProduct();
                        request.PackageDetail.ProductList[i].SKU = orderType.Products[i].SKU;
                        request.PackageDetail.ProductList[i].DeclareValue = 2;
                        request.PackageDetail.ProductList[i].Weight = 200;
                        request.PackageDetail.ProductList[i].Quantity = orderType.Products[i].Qty;
                        request.PackageDetail.ProductList[i].CustomsTitleEN = orderType.Products[i].SKU;
                        skuinfo += "[" + orderType.Products[i].SKU + "," + orderType.Products[i].Qty + "],";

                    }
                    skuinfo = skuinfo.Trim(',');
                    request.PackageDetail.Custom = request.PackageDetail.Remark = orderType.OrderNo + "  " + skuinfo;
                    ExpressAddPackageResponse response = ck.ExpressAddPackageNew(request);
                    if (response.OrderSign != null)
                    {
                        chukouyiOrder = response.OrderSign;
                        orderType.TrackCode = response.ItemSign;
                        NSession.Update(orderType);
                        NSession.Flush();
                        LoggerUtil.GetOrderRecord(orderType, "获取出口易" + t + "运单号！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }
                }

                ExpressCompleteOrderRequest expressCompleteOrderRequest = new ExpressCompleteOrderRequest();
                expressCompleteOrderRequest.OrderSign = chukouyiOrder;
                expressCompleteOrderRequest.Token = "1C1D66499731BFBD08EDD36BFC04DF3D";
                expressCompleteOrderRequest.UserKey = "kuf8dn5u9c";

                expressCompleteOrderRequest.ActionType = EnumActionType.Submit;
                ExpressCompleteOrderResponse expressCompleteOrderResponse = ck.ExpressCompleteOrder(expressCompleteOrderRequest);
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public ActionResult GetChuKouTrackCode(string ids)
        {

            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            string chukouyiOrder = "";
            cn.chukou1.demo.CK1 ck = new CK1();
            foreach (OrderType orderType in orders)
            {


                ExpressGetPackageRequest request = new ExpressGetPackageRequest();
                request.Token = "1C1D66499731BFBD08EDD36BFC04DF3D";
                request.UserKey = "kuf8dn5u9c";
                orderType.TrackCode2 = orderType.TrackCode;
                request.ItemSign = orderType.TrackCode;

                ExpressGetPackageResponse response = ck.ExpressGetPackage(request);
                if (response.OrderSign != null)
                {
                    if (response.PackageDetail.TrackCode != "")
                    {
                        orderType.TrackCode = response.PackageDetail.TrackCode;
                        NSession.Update(orderType);
                        NSession.Flush();

                        LoggerUtil.GetOrderRecord(orderType, "出口易-获得运单号！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }


                }
            }


            return null;
        }
        /// <summary>
        /// 获得线下E邮宝单号
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult SetRUEUB(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                if (orderType.Account.IndexOf("yw") != -1)//义乌面单增加库位
                {
                    foreach (OrderProductType orderProductType in orderProductTypes)
                    {
                        pinfo += "[" + orderProductType.SKU + "(" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")," + orderProductType.Qty + "]";
                    }
                }
                else
                {
                    foreach (OrderProductType orderProductType in orderProductTypes)
                    {
                        pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                    }

                }
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                string ss = EubUtil.GenerationEubTrackCode(orderType, pinfo, t);
                if (ss != null && ss.Length > 10)
                {
                    orderType.TrackCode = ss;
                    NSession.SaveOrUpdate(orderType);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(orderType, "线下E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                }
            }
            return null;
        }

        /// <summary>
        /// 线下eub 以色列 乌克兰
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult SetRUEUB2(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                foreach (OrderProductType orderProductType in orderProductTypes)
                {
                    pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                }
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                string ss = EubUtil.GenerationEubTrackCode2(orderType, pinfo, t);
                if (ss != null && ss.Length > 10)
                {
                    orderType.TrackCode = ss;
                    NSession.SaveOrUpdate(orderType);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(orderType, "线下E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                }
            }
            return null;
        }
        /// <summary>
        /// 顺邮宝
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public JsonResult GetSUBTrackCode(string ids, string t)
        {

            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string str = SUBUtil.GetTrackCode(orderType, t);
                SUBOrderResult result = JsonConvert.DeserializeObject<SUBOrderResult>(str);
                if (result.result != null && result.result.obj != null)
                {
                    string trackcode = result.result.obj.trNum;
                    orderType.TrackCode = trackcode;
                    NSession.Update(orderType);
                    NSession.Flush();
                }
                else
                {
                    //return Json(new { IsSuccess = false, Msg = str });
                }

            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 单号上传
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult XuSend(string ids)
        {


            List<OrderType> orders =
                NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (var orderType in orders)
            {
                 //宁波跟踪码上传
                if (((orderType.Platform == PlatformEnum.Ebay.ToString()) &&(orderType.TrackCode != null)) && (orderType.Account.IndexOf("yw") == -1))
                {
                    OrderHelper.NBEbayUpTrackCode(orderType, NSession);
                }
                    //宁波亚马逊跟踪码上传
                else if (((orderType.Platform == PlatformEnum.Amazon.ToString()) && (orderType.TrackCode != null)) && (orderType.Account.IndexOf("yw") == -1))
                {
                    OrderHelper.NBAmazonUpTrackCode(orderType, NSession);
                }
                else
                {
                    UploadTrackCode(orderType, NSession);
                }
                //orderType.IsXu = 1; // 标记虚假发货
                //base.NSession.Update(orderType);
                //base.NSession.Flush();

                LoggerUtil.GetOrderRecord(orderType, "手动虚假发货！", "上传运单号" + orderType.TrackCode + " 到平台", base.CurrentUser, base.NSession);

            }
            return Json(new { isSuccess = true });

        }

        public ActionResult GetTradeno(string ids)
        {
            List<OrderType> orders =
               NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (var orderType in orders)
            {
                orderType.TrackCode = Utilities.GetTrackCode(NSession, orderType.LogisticMode);
                base.NSession.Update(orderType);
                base.NSession.Flush();

                LoggerUtil.GetOrderRecord(orderType, "手动获取单号！", "运单号" + orderType.TrackCode + "获取", base.CurrentUser, base.NSession);

            }
            return Json(new { isSuccess = true });
        }
        public ActionResult GCxusend(string ids,string l)
        {
            List<OrderType> orders =
                NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (var orderType in orders)
            {
                UploadTrackCode2(orderType,l, NSession);
                //orderType.IsXu = 1; // 标记虚假发货
                //base.NSession.Update(orderType);
                //base.NSession.Flush();

                LoggerUtil.GetOrderRecord(orderType, "手动虚假发货！", "上传运单号" + orderType.TrackCode + " 到平台", base.CurrentUser, base.NSession);

            }
            return Json(new { isSuccess = true });

        }

        //admin手动标记发货
        public ActionResult setstatus(string ids)
        {
            if (GetCurrentAccount().Username == "admin" || GetCurrentAccount().Username == "tzq")
            {
                List<OrderType> orders =
                    NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
                foreach (var orderType in orders)
                {
                    // UploadTrackCode(orderType, NSession);
                    base.NSession.CreateSQLQuery("update Orders set Status='已发货',ScanningOn='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where Id in(" + ids + ")").ExecuteUpdate();
                    // LoggerUtil.GetOrderRecord(orderType, "手动标记发货！", "上传运单号" + orderType.TrackCode + " 到平台", base.CurrentUser, base.NSession);

                }
                return Json(new { isSuccess = true });
            }
            else
            {
                return Json(new { isSuccess = false });
            }

        }

        /// <summary>
        /// 线下eub 南京
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult SetRUEUB3(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                foreach (OrderProductType orderProductType in orderProductTypes)
                {
                    pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                }
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                string ss = EubUtil.GenerationEubTrackCode3(orderType, pinfo, t);
                if (ss != null && ss.Length > 10)
                {
                    orderType.TrackCode = ss;
                    NSession.SaveOrUpdate(orderType);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(orderType, "线下E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                }
            }
            return null;
        }

        //联捷E邮宝--汇鑫物流杭州E邮宝
        public ActionResult SetRUEUB4(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                if (orderType.LogisticMode=="上海E邮宝(汇)")
                {
                    t = 3;
                }
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                if (orderType.Account.IndexOf("yw") != -1)//义乌面单增加库位
                {
                    foreach (OrderProductType orderProductType in orderProductTypes)
                    {
                        pinfo += "[" + orderProductType.SKU + "(" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")," + orderProductType.Qty + "]";
                    }
                }
                else
                {
                    foreach (OrderProductType orderProductType in orderProductTypes)
                    {
                        pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                    }

                }
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                string ss = EubUtil.GenerationEubTrackCode4(orderType, pinfo, t);
                if (ss != null && ss.Length > 10)
                {
                    orderType.TrackCode = ss;
                    NSession.SaveOrUpdate(orderType);
                    NSession.Flush();
                    if (t == 0)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "联捷E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }
                    if (t == 1)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "联捷广州E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }
                    if (t == 2)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "汇鑫杭州E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }
                    if (t == 3)
                    {
                        LoggerUtil.GetOrderRecord(orderType, "汇鑫上海E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                    }
                }
            }
            return null;
        }

        //e邮宝（电子）
        public ActionResult SetRUEUB5(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                string pinfo = "";
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                foreach (OrderProductType orderProductType in orderProductTypes)
                {
                    pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                }
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                string ss = EubUtil.GenerationEubTrackCode5(orderType, pinfo, t);
                if (ss != null && ss.Length > 10)
                {
                    orderType.TrackCode = ss;
                    NSession.SaveOrUpdate(orderType);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(orderType, "联捷E邮宝设置！", "订单的挂号设置为：" + orderType.TrackCode, base.CurrentUser, base.NSession);
                }
            }
            return null;
        }

        /// <summary>
        /// 获得欧亚速运单号
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult JCLogistics(string ids)
        {
            string softname = "";
            string Cname = "";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (var orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products =
                    NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List<OrderProductType>().
                        ToList();
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku='" + orderType.Products[0].SKU + "'").List<ProductType>();
                IList<ProductCategoryType> objList = NSession.CreateQuery("from ProductCategoryType").List<ProductCategoryType>();
                IList<ProductCategoryType> category = objList.Where(p => p.Name == product[0].Category).ToList();

                if (category.Count > 0)
                {
                    softname = category[0].EName;//内件物品英文类目名称
                    Cname = category[0].Name;//内件物品中文类目名称
                }
                string s = JCUtil.GetJCTrackCode(orderType, softname, Cname);
                if (s.Length > 0)
                {
                    orderType.TrackCode = s;
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }

            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 获得国际E邮宝单号
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult GetEubTtackCode(string ids)
        {

            asp.ebay.shipping.ApacShippingService client = new ApacShippingService();
            string tid = "";
            String eid = "";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                NSession.CreateQuery("from AccountType ").List<AccountType>().ToList();
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products =
                    NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List<OrderProductType>().ToList();
                if (orderType.OrderExNo.IndexOf("-") != -1)
                {
                    eid = orderType.OrderExNo.Substring(0, orderType.OrderExNo.IndexOf("-"));
                    tid = orderType.OrderExNo.Substring(orderType.OrderExNo.IndexOf("-") + 1);
                }
                else
                {
                    eid = orderType.Products[0].ExSKU;
                    tid = orderType.Products[0].Remark;

                }
                AccountType accountType = accountTypes.Find(x => x.AccountName == orderType.Account);

                AddAPACShippingPackageRequest request = new AddAPACShippingPackageRequest();
                ApiContext context = AppSettingHelper.GetGenericApiContext("US");
                request.APISellerUserID = accountType.ApiKey;
                request.APISellerUserToken = accountType.ApiToken;
                request.APIDevUserID = context.ApiCredential.ApiAccount.Developer;
                request.AppID = context.ApiCredential.ApiAccount.Application;
                request.AppCert = context.ApiCredential.ApiAccount.Certificate;
                request.Version = "4.0.0";
                request.Carrier = "CNPOST";
                request.OrderDetail = new OrderDetail();
                if (accountType.AccountName.IndexOf("yw") != -1)
                {
                    request.OrderDetail.PickUpAddress = new PickUpAddress();
                    request.OrderDetail.PickUpAddress.Contact = "杨小科";
                    request.OrderDetail.PickUpAddress.Company = "宁波优胜国际贸易有限公司义乌总部";
                    request.OrderDetail.PickUpAddress.Street = "宗泽北路531号3楼";
                    request.OrderDetail.PickUpAddress.District = "330782";
                    request.OrderDetail.PickUpAddress.City = "330700";
                    request.OrderDetail.PickUpAddress.Province = "330000";
                    request.OrderDetail.PickUpAddress.CountryCode = "CN";
                    request.OrderDetail.PickUpAddress.Mobile = "057985096083";
                    request.OrderDetail.PickUpAddress.Phone = "18248584958";
                    request.OrderDetail.PickUpAddress.Postcode = "322000";
                    request.OrderDetail.PickUpAddress.Email = "971038989@qq.com ";
                    request.OrderDetail.ReturnAddress = new ReturnAddress();
                    request.OrderDetail.ReturnAddress.Contact = "杨小科";
                    request.OrderDetail.ReturnAddress.Company = "宁波优胜国际贸易有限公司义乌总部";
                    request.OrderDetail.ReturnAddress.Street = "宗泽北路531号3楼";
                    request.OrderDetail.ReturnAddress.District = "330782";
                    request.OrderDetail.ReturnAddress.City = "330700";
                    request.OrderDetail.ReturnAddress.Province = "浙江";
                    request.OrderDetail.ReturnAddress.CountryCode = "CN";
                    request.OrderDetail.ReturnAddress.Postcode = "322000";
                    request.OrderDetail.ShipFromAddress = new ShipFromAddress();
                    request.OrderDetail.ShipFromAddress.Contact = "YANG XIAO KE";
                    request.OrderDetail.ShipFromAddress.Company = "UNIONSOURCE";
                    request.OrderDetail.ShipFromAddress.Street = "ZHONGZHEBEILU 531 -3";
                    request.OrderDetail.ShipFromAddress.District = "YIWU";
                    request.OrderDetail.ShipFromAddress.City = "JINHUA";
                    request.OrderDetail.ShipFromAddress.Province = "ZHEJIANG";
                    request.OrderDetail.ShipFromAddress.CountryCode = "CN";
                    request.OrderDetail.ShipFromAddress.Mobile = "18248584958";
                    request.OrderDetail.ShipFromAddress.Postcode = "322000";
                    request.OrderDetail.ShipFromAddress.Email = "971038989@qq.com ";
                }
                else
                {



                    request.OrderDetail.PickUpAddress = new PickUpAddress();
                    //request.OrderDetail.PickUpAddress.Contact = "小朱";
                    //request.OrderDetail.PickUpAddress.Company = "宁波优胜国际贸易有限公司";
                    //request.OrderDetail.PickUpAddress.Street = "南京经济技术开发区龙潭街道疏港路1号龙潭物流基地A一96号";
                    //request.OrderDetail.PickUpAddress.District = "320113";
                    //request.OrderDetail.PickUpAddress.City = "320100";
                    //request.OrderDetail.PickUpAddress.Province = "320000";
                    //request.OrderDetail.PickUpAddress.CountryCode = "CN";
                    //request.OrderDetail.PickUpAddress.Mobile = "0574-27906684";
                    //request.OrderDetail.PickUpAddress.Phone = "13162255335";
                    //request.OrderDetail.PickUpAddress.Postcode = "210058";
                    //request.OrderDetail.PickUpAddress.Email = "sales@bestore.com.cn";
                    //request.OrderDetail.ReturnAddress = new ReturnAddress();
                    //request.OrderDetail.ReturnAddress.Contact = "小朱";
                    //request.OrderDetail.ReturnAddress.Company = "宁波优胜国际贸易有限公司";
                    //request.OrderDetail.ReturnAddress.Street = "南京经济技术开发区龙潭街道疏港路1号龙潭物流基地A一96号";
                    //request.OrderDetail.ReturnAddress.District = "320113";
                    //request.OrderDetail.ReturnAddress.City = "320100";
                    //request.OrderDetail.ReturnAddress.Province = "江苏";
                    //request.OrderDetail.ReturnAddress.CountryCode = "CN";
                    //request.OrderDetail.ReturnAddress.Postcode = "210058";
                    //request.OrderDetail.ShipFromAddress = new ShipFromAddress();
                    //request.OrderDetail.ShipFromAddress.Contact = "Xiao ZHU";
                    //request.OrderDetail.ShipFromAddress.Company = "NINGBO UNIONSOURCE";
                    //request.OrderDetail.ShipFromAddress.Street = "QIXIA DISTRICT NO.1PORT ROAD LONGTAN LOGISTICS BASE NO A 96";
                    //request.OrderDetail.ShipFromAddress.District = "XiaXi";
                    //request.OrderDetail.ShipFromAddress.City = "NanJing";
                    //request.OrderDetail.ShipFromAddress.Province = "JiangSu";
                    //request.OrderDetail.ShipFromAddress.CountryCode = "CN";
                    //request.OrderDetail.ShipFromAddress.Mobile = "13162255335";
                    //request.OrderDetail.ShipFromAddress.Postcode = "210058";
                    //request.OrderDetail.ShipFromAddress.Email = "sales@bestore.com.cn";


                    request.OrderDetail.PickUpAddress.Contact = "吕晶晶";
                    request.OrderDetail.PickUpAddress.Company = "宁波优胜国际贸易有限公司";
                    request.OrderDetail.PickUpAddress.Street = "聚贤路399号研发园B1楼20层";
                    request.OrderDetail.PickUpAddress.District = "330204";
                    request.OrderDetail.PickUpAddress.City = "330200";
                    request.OrderDetail.PickUpAddress.Province = "330000";
                    request.OrderDetail.PickUpAddress.CountryCode = "CN";
                    request.OrderDetail.PickUpAddress.Mobile = "0574-27906684";
                    request.OrderDetail.PickUpAddress.Phone = "18505885815";
                    request.OrderDetail.PickUpAddress.Postcode = "315040";
                    request.OrderDetail.PickUpAddress.Email = "sales@bestore.com.cn";
                    request.OrderDetail.ReturnAddress = new ReturnAddress();
                    request.OrderDetail.ReturnAddress.Contact = "吕晶晶";
                    request.OrderDetail.ReturnAddress.Company = "宁波优胜国际贸易有限公司";
                    request.OrderDetail.ReturnAddress.Street = "聚贤路399号研发园B1楼20层";
                    request.OrderDetail.ReturnAddress.District = "330204";
                    request.OrderDetail.ReturnAddress.City = "330200";
                    request.OrderDetail.ReturnAddress.Province = "浙江";
                    request.OrderDetail.ReturnAddress.CountryCode = "CN";
                    request.OrderDetail.ReturnAddress.Postcode = "315040";
                    request.OrderDetail.ShipFromAddress = new ShipFromAddress();
                    request.OrderDetail.ShipFromAddress.Contact = "VIKI";
                    request.OrderDetail.ShipFromAddress.Company = "NINGBO UNIONSOURCE";
                    request.OrderDetail.ShipFromAddress.Street = "3F,NO.4 Building,JUNSHENG Group,1266,Juxian Road";
                    request.OrderDetail.ShipFromAddress.District = "JIANGDONG";
                    request.OrderDetail.ShipFromAddress.City = "NINGBO";
                    request.OrderDetail.ShipFromAddress.Province = "ZHEJIANG";
                    request.OrderDetail.ShipFromAddress.CountryCode = "CN";
                    request.OrderDetail.ShipFromAddress.Mobile = "15988173792";
                    request.OrderDetail.ShipFromAddress.Postcode = "315000";
                    request.OrderDetail.ShipFromAddress.Email = "sales@bestore.com.cn";
                    //                    VIKI
                    //3F,NO.4 Building,JUNSHENG Group,1266,Juxian Road,NINGBO ZHEJIANG China 315000 15988173792
                    //request.OrderDetail.ShipFromAddress.Contact = "ANSWER";
                    //request.OrderDetail.ShipFromAddress.Company = "NINGBO UNIONSOURCE";
                    //request.OrderDetail.ShipFromAddress.Street = "JUXIAN ROAD NO.399 B1 BULIDING 20TH";
                    //request.OrderDetail.ShipFromAddress.District = "JIANGDONG";
                    //request.OrderDetail.ShipFromAddress.City = "NINGBO";
                    //request.OrderDetail.ShipFromAddress.Province = "ZHEJIANG";
                    //request.OrderDetail.ShipFromAddress.CountryCode = "CN";
                    //request.OrderDetail.ShipFromAddress.Mobile = "18505885815";
                    //request.OrderDetail.ShipFromAddress.Postcode = "315040";
                    //request.OrderDetail.ShipFromAddress.Email = "sales@bestore.com.cn";

                }


                request.OrderDetail.ShipToAddress = new ShipToAddress();
                request.OrderDetail.ShipToAddress.Contact = orderType.AddressInfo.Addressee;
                request.OrderDetail.ShipToAddress.Company = "";
                request.OrderDetail.ShipToAddress.Street = orderType.AddressInfo.Street + " " + orderType.AddressInfo.County;

                request.OrderDetail.ShipToAddress.City = orderType.AddressInfo.City;
                request.OrderDetail.ShipToAddress.Province = orderType.AddressInfo.Province;
                //request.OrderDetail.ShipToAddress.CountryCode = orderType.AddressInfo.Country;
                request.OrderDetail.ShipToAddress.CountryCode = orderType.AddressInfo.CountryCode;
                if (request.OrderDetail.ShipToAddress.CountryCode == "UK")
                    request.OrderDetail.ShipToAddress.CountryCode = "GB";
                request.OrderDetail.ShipToAddress.Phone = orderType.AddressInfo.Phone;
                request.OrderDetail.ShipToAddress.Postcode = orderType.AddressInfo.PostCode;
                request.OrderDetail.ShipToAddress.Email = orderType.BuyerEmail;


                request.OrderDetail.ItemList = new Item[1];
                request.OrderDetail.ItemList[0] = new Item();
                request.OrderDetail.ItemList[0].EBayBuyerID = orderType.BuyerName;
                request.OrderDetail.ItemList[0].ReceivedAmount = 1;
                request.OrderDetail.ItemList[0].SoldPrice = 2;
                request.OrderDetail.ItemList[0].EBayItemID = eid;
                request.OrderDetail.ItemList[0].EBayTransactionID = tid;
                request.OrderDetail.ItemList[0].SoldQTY = 1;
                request.OrderDetail.ItemList[0].PostedQTY = 1;
                request.OrderDetail.ItemList[0].SKU = new SKU();
                string skus = "";
                foreach (OrderProductType orderProduct in orderType.Products)
                {
                    skus += "[" + orderProduct.SKU + "," + orderProduct.Qty + "] ";
                }
                //  request.OrderDetail.ItemList[0].SKU.CustomsTitle = orderType.OrderNo;//2016-9-19中文品名为订单编号
                IList<ProductType> product = NSession.CreateQuery("from ProductType where sku='" + orderType.Products[0].SKU + "'").List<ProductType>();
                if (product.Count > 0)
                {
                    request.OrderDetail.ItemList[0].SKU.CustomsTitle = product[0].Category;
                }
                request.OrderDetail.ItemList[0].SKU.CustomsTitleEN = skus;
                request.OrderDetail.ItemList[0].SKU.DeclaredValue = 5;
                request.OrderDetail.ItemList[0].SKU.OriginCountryCode = "CN";
                request.OrderDetail.ItemList[0].SKU.OriginCountryName = "China";
                request.OrderDetail.ItemList[0].SKU.Weight = 0.1M;
                try
                {
                    AddAPACShippingPackageResponse response = client.AddAPACShippingPackage(request);
                    if (!string.IsNullOrEmpty(response.TrackCode))
                    {
                        orderType.TrackCode = response.TrackCode;
                        NSession.Update(orderType);
                        NSession.Flush();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

            return null;
            //client.AddAPACShippingPackage();
        }

        /// <summary>
        /// 获得国际E邮宝PDF
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult GetGJEubPDF(string ids)
        {
            List<AccountType> accountTypes =
               NSession.CreateQuery("from AccountType ").List<AccountType>().ToList();

            List<OrderType> orderTypes =
             NSession.CreateQuery("from OrderType where Id in (" + ids + ") ").List<OrderType>().ToList();

            if (orderTypes.Count == 0)
            {
                return null;
            }

            try
            {
                foreach (IGrouping<string, OrderType> grouping in orderTypes.GroupBy(x => x.Account))
                {
                    AccountType accountType = accountTypes.Find(x => x.AccountName == grouping.Key);
                    asp.ebay.shipping.ApacShippingService client = new ApacShippingService();
                    GetAPACShippingLabelsRequest request = new GetAPACShippingLabelsRequest();
                    ApiContext context = AppSettingHelper.GetGenericApiContext("US");
                    request.APISellerUserID = accountType.ApiKey;
                    request.APISellerUserToken = accountType.ApiToken;
                    request.APIDevUserID = context.ApiCredential.ApiAccount.Developer;
                    request.AppID = context.ApiCredential.ApiAccount.Application;
                    request.AppCert = context.ApiCredential.ApiAccount.Certificate;
                    request.Version = "4.0.0";
                    request.Carrier = "CNPOST";
                    request.PageSize = 2;
                    request.TrackCodeList = new string[grouping.Count()];
                    OrderType[] orders = grouping.ToArray();
                    for (int i = 0; i < orders.Count(); i++)
                    {
                        request.TrackCodeList[i] = orders[i].TrackCode;
                    }
                    GetAPACShippingLabelsResponse response = client.GetAPACShippingLabels(request);
                    var mimeType = "application/pdf";
                    var fileDownloadName = DateTime.Now.Ticks.ToString() + ".pdf";
                    //LoggerUtil.GetOrderRecord(orderType, "订单打印", "国际E邮宝打印！", base.CurrentUser, base.NSession);

                    foreach (OrderType orderType in orders)
                    {
                        orderType.IsPrint = 1;
                        LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印国际E邮宝", base.CurrentUser, base.NSession);
                        NSession.Update(orderType);
                        NSession.Flush();
                    }
                    Session["pdf"] = response.Label;
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;

        }

        public ActionResult AliLogistics(string ids)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            return View();
        }

        public ActionResult GetAliPDF(string id)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + id + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                 NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            string jsonString = "";
            List<object> objects = new List<object>();
            foreach (OrderType order in orders)
            {
                objects.Add(new { internationalLogisticsId = order.TrackCode });
                order.IsPrint = 1;
                LoggerUtil.GetOrderRecord(order, "订单打印", "打印速卖通", base.CurrentUser, base.NSession);
                NSession.Update(order);
                NSession.Flush();
            }
            AccountType account = accountTypes.Find(x => x.AccountName == orders[0].Account);
            account.ApiTokenInfo = AliUtil.RefreshToken(account);
            jsonString = JsonConvert.SerializeObject(objects);
            AliPDFRootObject pdf =
                JsonConvert.DeserializeObject<AliPDFRootObject>(AliUtil.GetAliPDF(account.ApiKey, account.ApiSecret,
                                                                                  account.ApiTokenInfo,
                                                                                  jsonString));
            byte[] bytes = Convert.FromBase64String(pdf.body);

            var mimeType = "application/pdf";
            var fileDownloadName = "ali.pdf";
            return File(bytes, mimeType, fileDownloadName);
        }
        [HttpPost]
        public ActionResult GetESTTrackCode(string ids)
        {
            List<OrderType> orderTypes = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orderTypes)
            {
                OrderHelper.GetESTTrackCode(orderType, NSession);
            }
            return base.Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 获得速卖通运单号
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult GetAliTrackCode(string ids)
        {


            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                 NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            string orderExno = "";
            foreach (OrderType order in orders)
            {
                AccountType account = accountTypes.Find(x => x.AccountName == order.Account);
                account.ApiTokenInfo = AliUtil.RefreshToken(account);
                orderExno = order.OrderExNo;
                if (order.OrderExNo.Contains("*") && order.OrderExNo.Contains("#"))
                {
                    orderExno = order.OrderExNo.Replace("#", "*");
                    orderExno = order.OrderExNo.Substring(0, orderExno.IndexOf("*"));
                }
                else
                {
                    if (order.OrderExNo.IndexOf("*") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("*"));
                    }
                    if (order.OrderExNo.IndexOf("#") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("#"));
                    }
                }
                AliTrackCodeRootObject t = AliUtil.GetOnlineLogisticsInfo(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, orderExno);
                if (t.success.ToLower() == "true" && t.result.Count > 0)
                {
                    order.TrackCode = t.result[0].internationallogisticsId;
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();
                    LoggerUtil.GetOrderRecord(order, "获取" + order.LogisticMode + "运单号！", "订单的跟踪码设置为：" + order.TrackCode, base.CurrentUser, base.NSession);

                }
            }



            return null;
        }
        public JsonResult createWuyouOrder(string ids,string type)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                 NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            foreach (OrderType order in orders)
            {
                List<OrderProductType> orderProductTypes = NSession.CreateQuery("from OrderProductType where Oid ='" + order.Id + "'").List<OrderProductType>().ToList();
               
               
               
                order.AddressInfo = NSession.Get<OrderAddressType>(order.AddressId);
                AddressDTOs addresses = new AddressDTOs();

                AccountType account = accountTypes.Find(x => x.AccountName == order.Account);
                account.ApiTokenInfo = AliUtil.RefreshToken(account);
                if (order.OrderExNo.Contains("*") && order.OrderExNo.Contains("#"))
                {
                    order.OrderExNo = order.OrderExNo.Replace("#", "*");
                    order.OrderExNo = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("*"));
                }
                else
                {
                    if (order.OrderExNo.IndexOf("*") != -1)
                    {
                        order.OrderExNo = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("*"));
                    }
                    if (order.OrderExNo.IndexOf("#") != -1)
                    {
                        order.OrderExNo = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("#"));
                    }
                }

                SellerAddressesList addressList = AliUtil.GetLogisticsSellerAddresses(account.ApiKey, account.ApiSecret, account.ApiTokenInfo);
                List<AeopWlDeclareProductDTO> goodses;
                var address = GetWuyouAddress(order, addressList, type,out goodses);
                List<LogisticsResult> logisticsresltlist = AliUtil.GetOnlineLogisticsServiceListByOrderId(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, order.OrderExNo).result;
                //LogisticsResult logisticsreslt = new LogisticsResult();
                string warehouseCarrierService = string.Empty;
                if (logisticsresltlist.Count > 0)
                {
                    foreach (LogisticsResult logisticsreslt in logisticsresltlist)
                    {
                        if (order.Account.IndexOf("yw") != -1)
                        {
                            //{"logisticsServiceName":"AliExpress 无忧物流-标准","trialResult":"CN¥88.00","warehouseName":"燕文义乌仓-标准无忧","isExpressLogisticsService":false,"logisticsTimeliness":"15-45天","otherFees":[],"logisticsServiceId":"CAINIAO_STANDARD_YANWENYW","deliveryAddress":"浙江省金华市义乌市北苑街道\t雪峰东路6号"}
                            //"logisticsServiceName":"AliExpress 无忧物流-优先","trialResult":"CN¥176.05","warehouseName":"燕文义乌仓-优先","isExpressLogisticsService":false,"logisticsTimeliness":"8-15天","otherFees":[],"logisticsServiceId":"CAINIAO_PREMIUM_YANWENYW","deliveryAddress":"浙江省金华市义乌市北苑街道\t雪峰东路6号"}
                            if (order.LogisticMode.Contains("标准"))
                            {
                                if (logisticsreslt.deliveryAddress.Contains("雪峰东路6号") && logisticsreslt.logisticsServiceName.Contains("无忧物流") && logisticsreslt.logisticsServiceName.Contains("标准"))
                                {
                                    warehouseCarrierService = logisticsreslt.logisticsServiceId;
                                    break;
                                }
                            }
                            else if (order.LogisticMode.Contains("优先"))
                            {
                                if (logisticsreslt.deliveryAddress.Contains("雪峰东路6号") && logisticsreslt.logisticsServiceName.Contains("无忧物流") && logisticsreslt.logisticsServiceName.Contains("优先"))
                                {
                                    warehouseCarrierService = logisticsreslt.logisticsServiceId;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (logisticsreslt.deliveryAddress.Contains("泰安中路456号盈升大厦1楼东侧"))
                            {
                                warehouseCarrierService = logisticsreslt.logisticsServiceId;
                                break;
                            }
                        }
                    }
                   
                }
                if (!string.IsNullOrEmpty(warehouseCarrierService))
                {
                    AlicreateWarehouseOrderReturn sendRoot = AliUtil.GetWuyouCode(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, order.OrderExNo, address, warehouseCarrierService, goodses);
                    if (sendRoot.Success)
                    {                                
                        if (order.Account.IndexOf("yw") != -1)
                        {
                            order.TrackCode = sendRoot.result.intlTrackingNo;
                            NSession.SaveOrUpdate(order);
                            Thread.Sleep(2000);//延迟一秒
                            AliTrackCodeRootObject t = AliUtil.GetOnlineLogisticsInfo(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, order.OrderExNo);
                            if (t.success.ToLower() == "true" && t.result.Count > 0)
                            {
                                order.TrackCode = t.result[0].internationallogisticsId;
                                NSession.SaveOrUpdate(order);
                                //NSession.Flush();
                            }
                        }
                        else
                        {
                            order.TrackCode = sendRoot.result.intlTrackingNo;
                            NSession.SaveOrUpdate(order);
                        }
                        NSession.Flush();
                        LoggerUtil.GetOrderRecord(order, "获取"+order.LogisticMode+"运单号！", "订单的跟踪码设置为：" + order.TrackCode, base.CurrentUser, base.NSession);
                    }
                    else
                    {
                        return base.Json(new { IsSuccess = false, errorinfo = "订单号" + order.OrderNo + "获取失败，原因：" + sendRoot.result.errorDesc });
                    }
                }

               
            }

            return null;
        }
        public ActionResult GetAliLogistics(int id)
        {

            //List<OrderType> orders = NSession.CreateQuery("from OrderType where OrderNo='223197' in(" + ids + ")").List<OrderType>().ToList();
            //List<OrderType> orders = NSession.CreateQuery("from OrderType where id").List<OrderType>().ToList();

            OrderType order = Get<OrderType>(id);
            List<AccountType> accountTypes =
                NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            string orderExno = order.OrderExNo;
            if (order != null)
            {
                AccountType account = accountTypes.Find(x => x.AccountName == order.Account);
                account.ApiTokenInfo = AliUtil.RefreshToken(account);
                if (order.OrderExNo.Contains("*") && order.OrderExNo.Contains("#"))
                {
                    orderExno = order.OrderExNo.Replace("#", "*");
                    orderExno = order.OrderExNo.Substring(0, orderExno.IndexOf("*"));
                }
                else
                {
                    if (order.OrderExNo.IndexOf("*") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("*"));
                    }
                    if (order.OrderExNo.IndexOf("#") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("#"));
                    }
                }
                LogisticsRootObject logisticsRootObject = AliUtil.GetOnlineLogisticsServiceListByOrderId(account.ApiKey, account.ApiSecret, account.ApiTokenInfo,
                                                                 orderExno);
                List<object> list = new List<object>();
                foreach (LogisticsResult result in logisticsRootObject.result.OrderByDescending(x => x.logisticsServiceName))
                {
                    list.Add(new { Id = result.logisticsServiceId, Text = result.logisticsServiceName + "(" + result.deliveryAddress + ")" });
                }
                return Json(list);
            }
            return null;
        }


        public ActionResult SetAliSendOver(string ids)
        {

            string l = "CPAM_WLB_CPAMNB";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                 NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            foreach (OrderType order in orders)
            {
                List<LogisticsGoods> goodses;

                AccountType account = accountTypes.Find(x => x.AccountName == order.Account);
                account.ApiTokenInfo = AliUtil.RefreshToken(account);
                SellerAddressesList addressList = AliUtil.GetLogisticsSellerAddresses(account.ApiKey, account.ApiSecret, account.ApiTokenInfo);

                var address = GetAliAddress(order, addressList, out goodses);
                AliOrderSendRootObject sendRoot = AliUtil.CreateWarehouseOrder(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, order.OrderExNo, address, l, goodses);
                //AliTrackCodeRootObject t = AliUtil.GetOnlineLogisticsInfo(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, order.OrderExNo);
                if (sendRoot.success.ToLower() == "true")
                {
                    order.TrackCode = sendRoot.result.warehouseOrderId;
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();


                    LoggerUtil.GetOrderRecord(order, "获取" + order.LogisticMode + "运单号！", "订单的跟踪码设置为：" + order.TrackCode, base.CurrentUser, base.NSession);

                }
            }

            return null;
        }

        public ActionResult SetAliSend(string ids, string l)
        {

            string a = "CPAM_WLB_CPAMHZ,YANWENJYT_WLB_CPAMHZ";
            string orderExno = "";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            List<AccountType> accountTypes =
                 NSession.CreateQuery("from AccountType").List<AccountType>().ToList();
            foreach (OrderType order in orders)
            {
                List<LogisticsGoods> goodses;

                AccountType account = accountTypes.Find(x => x.AccountName == order.Account);
                account.ApiTokenInfo = AliUtil.RefreshToken(account);


                SellerAddressesList addressList = AliUtil.GetLogisticsSellerAddresses(account.ApiKey, account.ApiSecret, account.ApiTokenInfo);
                AliOrderAddressRootObject address = new AliOrderAddressRootObject();
                if (a.Contains(l))//杭州中邮仓地址
                {
                    address = GetAliAddresshz(order, addressList, out goodses);
                }
                else
                {
                    address = GetAliAddress(order, addressList, out goodses);
                }
                orderExno = order.OrderExNo;
                if (order.OrderExNo.Contains("*") && order.OrderExNo.Contains("#"))
                {
                    orderExno = order.OrderExNo.Replace("#", "*");
                    orderExno = order.OrderExNo.Substring(0, orderExno.IndexOf("*"));
                }
                else
                {
                    if (order.OrderExNo.IndexOf("*") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("*"));
                    }
                    if (order.OrderExNo.IndexOf("#") != -1)
                    {
                        orderExno = order.OrderExNo.Substring(0, order.OrderExNo.IndexOf("#"));
                    }
                }
                AliOrderSendRootObject sendRoot = AliUtil.CreateWarehouseOrder(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, orderExno, address, l, goodses);

                if (sendRoot.success.ToLower() == "true")
                {
                    order.TrackCode = sendRoot.result.warehouseOrderId;
                    NSession.SaveOrUpdate(order);
                    NSession.Flush();

                }
            }

            return null;
        }

        private AliOrderAddressRootObject GetAliAddress(OrderType order, SellerAddressesList Address, out List<LogisticsGoods> goodses)
        {
            order.AddressInfo = Get<OrderAddressType>(order.AddressId);
            order.Products =
                NSession.CreateQuery("from OrderProductType where OId=" + order.Id).List<OrderProductType>().ToList();

            AliOrderAddressRootObject address = new AliOrderAddressRootObject();
            address.receiver = new AliReceiver();
            address.receiver.name = order.AddressInfo.Addressee;
            address.receiver.streetAddress = order.AddressInfo.Street;
            address.receiver.city = order.AddressInfo.City;
            address.receiver.province = order.AddressInfo.Province;
            address.receiver.country = order.AddressInfo.CountryCode;
            address.receiver.fax = "";
            address.receiver.mobile = order.AddressInfo.Tel;
            address.receiver.phone = order.AddressInfo.Phone;
            address.receiver.postcode = order.AddressInfo.PostCode;
            if (order.Account.IndexOf("yw") != -1)
            {
                // 	揽收人信息
                address.pickup = new AliPickup();
                address.pickup.name = "雷刚";
                address.pickup.streetAddress = "北苑街道宗泽北路531号";
                address.pickup.county = "义乌市";
                address.pickup.city = "金华市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "13750970859";
                address.pickup.postcode = "322000";
                address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;

                // 	发货人信息
                address.sender = new AliSender();
                address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;
                address.sender.name = "Lei Gang";
                address.sender.streetAddress = "Zongze Road 531";
                address.sender.county = "YiWu";
                address.sender.city = "JinHua";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "13750970859";
                address.sender.postcode = "322000";

                // 退货地址信息
                address.refund = new AliRefund();
                address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
            }
            else
            {
                // 	揽收人信息
                address.pickup = new AliPickup();
                address.pickup.name = "吕晶晶";
                address.pickup.streetAddress = "聚贤路399号B1楼20层";
                address.pickup.county = "高新区";
                address.pickup.city = "宁波市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "18505885815";
                address.pickup.postcode = "315040";
                address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;

                // 	发货人信息
                address.sender = new AliSender();
                address.sender.name = "VIKI";
                address.sender.streetAddress = "3F,NO.4 Building,JUNSHENG Group,1266";
                address.sender.county = "High-tech Zone";
                address.sender.city = "NingBo";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "15988173792";
                address.sender.postcode = "315000 ";
                address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;

                // 退货地址信息
                address.refund = new AliRefund();
                address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
            }
            goodses = new List<LogisticsGoods>();
            foreach (OrderProductType product in order.Products)
            {
                List<ProductType> productTypes =
                    NSession.CreateQuery("from ProductType where SKU ='" + product.SKU + "'").List<ProductType>().ToList();
                if (productTypes.Count > 0)
                {
                    List<ProductCategoryType> categoryTypes =
                        NSession.CreateQuery("from ProductCategoryType where Name ='" + productTypes[0].Category + "'").List
                            <ProductCategoryType>().ToList();

                    LogisticsGoods goods = new LogisticsGoods();
                    goods.categoryCnDesc = productTypes[0].Category;
                    goods.categoryEnDesc = categoryTypes.Count > 0
                                               ? string.IsNullOrEmpty(categoryTypes[0].EName) ? "Light" : categoryTypes[0].EName
                                               : "Light";
                    goods.isContainsBattery = "0";
                    goods.productId = product.ExSKU;
                    goods.productNum = product.Qty.ToString();
                    goods.productWeight = "0.1";
                    goods.productDeclareAmount = "2";
                    goodses.Add(goods);
                }
            }
            return address;
        }

        private AddressOrderDTOs GetWuyouAddress(OrderType order, SellerAddressesList Address,string type, out List<AeopWlDeclareProductDTO> goodses)
        {
            order.AddressInfo = Get<OrderAddressType>(order.AddressId);
            order.Products =
                NSession.CreateQuery("from OrderProductType where OId=" + order.Id).List<OrderProductType>().ToList();

            AddressOrderDTOs address = new AddressOrderDTOs();
            //收货人信息
            address.receiver = new AliReceiver();
            address.receiver.name = order.AddressInfo.Addressee;
            address.receiver.streetAddress = order.AddressInfo.Street;
            address.receiver.city = order.AddressInfo.City;
            address.receiver.province = order.AddressInfo.Province;
            address.receiver.country = order.AddressInfo.CountryCode;
            address.receiver.fax = "";
            address.receiver.mobile = order.AddressInfo.Tel;
            address.receiver.phone = order.AddressInfo.Phone;
            address.receiver.postcode = order.AddressInfo.PostCode;
            if (order.Account.IndexOf("yw") != -1)
            {
                // 	揽收人信息
                address.pickup = new AliPickup();
                address.pickup.name = "雷刚";
                address.pickup.streetAddress = "北苑街道宗泽北路531号";
                address.pickup.county = "义乌市";
                address.pickup.city = "金华市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "13750970859";
                address.pickup.postcode = "322000";
                if (Address.pickupSellerAddressesList != null)
                {
                    address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }

                // 	发货人信息
                address.sender = new AliSender();
                if (Address.senderSellerAddressesList != null)
                {
                    address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }
                address.sender.name = "Lei Gang";
                address.sender.streetAddress = "Zongze Road 531";
                address.sender.county = "YiWu";
                address.sender.city = "JinHua";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "13750970859";
                address.sender.postcode = "322000";

                // 退货地址信息
                address.refund = new AliRefund();
                if (Address.refundSellerAddressesList != null)
                {
                    address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                    address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                    address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                    address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                    address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                    address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                    address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                    address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                    address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }
            }
            else
            {
                // 	揽收人信息
                address.pickup = new AliPickup();
                address.pickup.name = "吕晶晶";
                address.pickup.streetAddress = "聚贤路399号B1楼20层";
                address.pickup.county = "高新区";
                address.pickup.city = "宁波市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "18505885815";
                address.pickup.postcode = "315040";
                if (Address.pickupSellerAddressesList != null)
                {
                    address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }

                // 	发货人信息
                address.sender = new AliSender();
                address.sender.name = "VIKI";
                address.sender.streetAddress = "3F,NO.4 Building,JUNSHENG Group,1266";
                address.sender.county = "High-tech Zone";
                address.sender.city = "NingBo";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "15988173792";
                address.sender.postcode = "315000 ";
                if (Address.senderSellerAddressesList != null)
                {
                    address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }

                // 退货地址信息
                address.refund = new AliRefund();
                if (Address.refundSellerAddressesList != null)
                {
                    address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                    address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                    address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                    address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                    address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                    address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                    address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                    address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                    address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
                }
            }
            goodses = new List<AeopWlDeclareProductDTO>();
            foreach (OrderProductType product in order.Products)
            {
                List<ProductType> productTypes =
                    NSession.CreateQuery("from ProductType where SKU ='" + product.SKU + "'").List<ProductType>().ToList();
                if (productTypes.Count > 0)
                {
                    List<ProductCategoryType> categoryTypes =
                        NSession.CreateQuery("from ProductCategoryType where Name ='" + productTypes[0].Category + "'").List
                            <ProductCategoryType>().ToList();

                    AeopWlDeclareProductDTO goods = new AeopWlDeclareProductDTO();
                    goods.categoryCnDesc = productTypes[0].Category;
                    goods.categoryEnDesc = categoryTypes.Count > 0
                                               ? string.IsNullOrEmpty(categoryTypes[0].EName) ? "Light" : categoryTypes[0].EName
                                               : "Light";
                    
                    goods.productId = product.ExSKU;
                    goods.productNum = product.Qty;
                    goods.productWeight = 0.1;
                    goods.productDeclareAmount = 2;
                    goods.isOnlyBattery = 0;
                    goods.isContainsBattery = 0;
                    //普货
                    if (type == "CAINIAO_STANDARD")
                    {
                        goods.isAneroidMarkup = 0;
                    }
                    else
                    {
                     goods.isAneroidMarkup = 1;
                    }
                    goodses.Add(goods);
                }
            }
            return address;
        }

        private AliOrderAddressRootObject GetAliAddresshz(OrderType order, SellerAddressesList Address, out List<LogisticsGoods> goodses)
        {
            order.AddressInfo = Get<OrderAddressType>(order.AddressId);
            order.Products =
                NSession.CreateQuery("from OrderProductType where OId=" + order.Id).List<OrderProductType>().ToList();
            AliOrderAddressRootObject address = new AliOrderAddressRootObject();
            address.receiver = new AliReceiver();
            address.receiver.name = order.AddressInfo.Addressee;
            address.receiver.streetAddress = order.AddressInfo.Street;
            address.receiver.city = order.AddressInfo.City;
            address.receiver.province = order.AddressInfo.Province;
            address.receiver.country = order.AddressInfo.CountryCode;
            address.receiver.fax = "";
            address.receiver.mobile = order.AddressInfo.Tel;
            address.receiver.phone = order.AddressInfo.Phone;
            address.receiver.postcode = order.AddressInfo.PostCode;
            if (order.Account.IndexOf("yw") != -1)
            {
                address.pickup = new AliPickup();
                address.pickup.name = "陈祖麒(ZJF-LJ-8765)";
                address.pickup.streetAddress = "东方花城2幢-2单元-803室";
                address.pickup.county = "滨江区";
                address.pickup.city = "杭州市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "17855897905";
                address.pickup.postcode = "310000";
                address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;

                address.sender = new AliSender();
                address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;
                address.sender.name = "Chen Zu Qi(ZJF-LJ-8765)";
                address.sender.streetAddress = "East Huacheng 2-2-803";
                address.sender.county = "Binjiang District";
                address.sender.city = "Hangzhou";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "17855897905";
                address.sender.postcode = "310000";

                // 退货地址信息
                address.refund = new AliRefund();
                address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
            }
            else
            {
                address.pickup = new AliPickup();
                address.pickup.name = "钱丁一";
                address.pickup.streetAddress = "聚贤路399号B1楼20层";
                address.pickup.county = "高新区";
                address.pickup.city = "宁波市";
                address.pickup.province = "浙江省";
                address.pickup.country = "中国";
                address.pickup.phone = "18505885815";
                address.pickup.postcode = "315040";
                address.pickup.addressId = Address.pickupSellerAddressesList.Find(x => x.isDefault == true).addressId;

                address.sender = new AliSender();
                address.sender.name = "Qian Dingyi";
                address.sender.streetAddress = "Juxian Road 399 B1 20th";
                address.sender.county = "High-tech Zone";
                address.sender.city = "NingBo";
                address.sender.province = "ZheJiang";
                address.sender.country = "CN";
                address.sender.phone = "18505885815";
                address.sender.postcode = "315040";
                address.sender.addressId = Address.senderSellerAddressesList.Find(x => x.isDefault == true).addressId;


                // 退货地址信息
                address.refund = new AliRefund();
                address.refund.name = Address.refundSellerAddressesList.Find(x => x.isDefault == true).name;
                address.refund.streetAddress = Address.refundSellerAddressesList.Find(x => x.isDefault == true).streetAddress;
                address.refund.county = Address.refundSellerAddressesList.Find(x => x.isDefault == true).county;
                address.refund.city = Address.refundSellerAddressesList.Find(x => x.isDefault == true).city;
                address.refund.province = Address.refundSellerAddressesList.Find(x => x.isDefault == true).province;
                address.refund.country = Address.refundSellerAddressesList.Find(x => x.isDefault == true).country;
                address.refund.phone = Address.refundSellerAddressesList.Find(x => x.isDefault == true).phone;
                address.refund.postcode = Address.refundSellerAddressesList.Find(x => x.isDefault == true).postcode;
                address.refund.addressId = Address.refundSellerAddressesList.Find(x => x.isDefault == true).addressId;
            }
            goodses = new List<LogisticsGoods>();
            foreach (OrderProductType product in order.Products)
            {
                List<ProductType> productTypes =
                    NSession.CreateQuery("from ProductType where SKU ='" + product.SKU + "'").List<ProductType>().ToList();
                if (productTypes.Count > 0)
                {
                    List<ProductCategoryType> categoryTypes =
                        NSession.CreateQuery("from ProductCategoryType where Name ='" + productTypes[0].Category + "'").List
                            <ProductCategoryType>().ToList();

                    LogisticsGoods goods = new LogisticsGoods();
                    goods.categoryCnDesc = productTypes[0].Category;
                    goods.categoryEnDesc = categoryTypes.Count > 0
                                               ? string.IsNullOrEmpty(categoryTypes[0].EName) ? "Light" : categoryTypes[0].EName
                                               : "Light";
                    goods.isContainsBattery = "0";
                    goods.productId = product.ExSKU;
                    goods.productNum = product.Qty.ToString();
                    goods.productWeight = "0.1";
                    goods.productDeclareAmount = "2";
                    goodses.Add(goods);
                }
            }
            return address;
        }

        /// <summary>
        /// 获得线下E邮宝获得打印pdf
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public string GetEubPdf(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            StringBuilder sb = new StringBuilder();
            bool isyi = false;
            foreach (OrderType orderType in orders)
            {
                sb.AppendLine("<order><mailnum>" + orderType.TrackCode + "</mailnum></order>");
                orderType.IsPrint = 1;
                if (orderType.Account.IndexOf("yw") != -1)
                    isyi = true;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印线下E邮宝", base.CurrentUser, base.NSession);
            }
            string url = "";
            if (t == 0)
                url = EubUtil.getEubPdfUrl(sb.ToString(), "03", isyi);
            else if (t == 1)
            {
                url = EubUtil.getEubPdfUrl(sb.ToString(), "01", isyi);
            }
            else
                url = EubUtil.getEubPdfUrl(sb.ToString(), "00", true);

            return url;
        }

        //联捷E邮宝打印---打印地址
        public string GetEubPdf1(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            StringBuilder sb = new StringBuilder();
            bool isyi = false;
            foreach (OrderType orderType in orders)
            {
                sb.AppendLine("<order><mailnum>" + orderType.TrackCode + "</mailnum></order>");
                orderType.IsPrint = 1;
                if (orderType.Account.IndexOf("yw") != -1)
                    isyi = true;
                if (t == 0 || t == 1)
                {
                    LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印联捷E邮宝", base.CurrentUser, base.NSession);
                }
                else if (t == 2 || t == 3)
                {
                    LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印联捷广州E邮宝", base.CurrentUser, base.NSession);
                }
                else if (t == 4 || t == 5)
                {
                    if (orderType.LogisticMode == "上海E邮宝(汇)")
                    {
                        if (t == 4)
                        {
                            t = 6;
                        }
                        else
                        {
                            t = 7;
                        }
                        LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印汇鑫上海E邮宝", base.CurrentUser, base.NSession);
                    }
                    else
                    {
                        LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印汇鑫杭州E邮宝", base.CurrentUser, base.NSession);
                    }
                    
                }
            }
            string url = "";
            if (t == 0 || t == 2 || t == 4 || t==6)//(10*15)
                url = EubUtil.getEubPdfUrl1(sb.ToString(), "03", isyi, t);
            else if (t == 1 || t == 3 || t == 5 || t == 7)//(10*10)
            {
                url = EubUtil.getEubPdfUrl1(sb.ToString(), "01", isyi, t);
            }
            else
                url = EubUtil.getEubPdfUrl1(sb.ToString(), "00", true, t);

            return url;
        }

        //(e邮宝（电子）)打印pdf
        public string GetEubPdf3(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            StringBuilder sb = new StringBuilder();
            bool isyi = false;
            foreach (OrderType orderType in orders)
            {
                sb.AppendLine("<order><mailnum>" + orderType.TrackCode + "</mailnum></order>");
                orderType.IsPrint = 1;
                if (orderType.Account.IndexOf("yw") != -1)
                    isyi = true;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "(e邮宝（电子）)打印pdf", base.CurrentUser, base.NSession);
            }
            string url = "";
            if (t == 0)
                url = EubUtil.getEubPdfUrl3(sb.ToString(), "03", isyi);
            else if (t == 1)
            {
                url = EubUtil.getEubPdfUrl3(sb.ToString(), "01", isyi);
            }
            else
                url = EubUtil.getEubPdfUrl3(sb.ToString(), "00", true);

            return url;
        }
        /// <summary>
        /// 上海淼信PDF打印
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public string GetSHMXPdf(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            StringBuilder sb = new StringBuilder();
            bool isyi = false;
            foreach (OrderType orderType in orders)
            {
                sb.Append(orderType.OrderExNo + ",");
                orderType.IsPrint = 1;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "(上海淼信)打印pdf", base.CurrentUser, base.NSession);
            }
            string url = "";
            string orderinfo = sb.ToString();
            int length = orderinfo.Length;
            orderinfo = orderinfo.Substring(0, orderinfo.Length - 1);


            byte[] bytes = SHMXUtil.GetSHMXpdfUrl(orderinfo, "0");

            Session["pdf"] = bytes;
            return null;

        }
        #endregion

        public JsonResult List(int page, int rows, string sort, string order, string search, string v, int isUn = 0)
        {
            string str2 = " order by Id desc";
            string str = "";
            string cc = "";
            List<DataDictionaryDetailType> listdate = GetList<DataDictionaryDetailType>("DicCode", "AllowDeliveryDay", "FullName='day1'");
            string day1 = listdate[0].DicValue;
            string str3 = "<>";
            if (isUn == 1)
            {
                str3 = "=";
            }
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where Enabled=1 and  Status" + str3 + "'待处理' and " + str;
                }
            }
            if (str.Length == 0)
            {
                str = " where Enabled=1 and  Status" + str3 + "'待处理'";
            }
            // 过滤地区条件
            string area = "全部";
            if (str.Contains("宁波"))
            {
                area = "宁波";
            }
            if (str.Contains("义乌"))
            {
                area = "义乌";
            }
            // 移除where语句内条件
            str = str.Replace(" and FromArea = '宁波'", "").Replace(" and FromArea = '义乌'", "");
            //str = str;

            string acs = "";
            foreach (AccountType c in GetCurrentAccount().Accounts)
            {
                if (area != "全部")
                {
                    if (c.FromArea == area)
                    {
                        acs += "'" + c.AccountName + "',";
                    }
                }
                else
                {
                    acs += "'" + c.AccountName + "',";
                }
            }

            //string acs = "";
            //foreach (AccountType c in GetCurrentAccount().Accounts)
            //{
            //    acs += "'" + c.AccountName + "',";
            //}
            acs = acs.Trim(',');

            if (acs.Length > 0)
            {
                str += " and Account in (" + acs + ")";
            }
            else if (acs.Length == 0)
            {
                str += " and Account in ('')";
            }


            //同买家可合并订单显示
            if (!(string.IsNullOrEmpty(v)) && v.Contains("1"))
            {

                List<object[]> listcc = base.NSession.CreateSQLQuery("select Account,BuyerName from Orders  where Status='待处理'  and Enabled=1  and Account in (" + acs + ") group by  BuyerName,Account having count(BuyerName) >1").List<object[]>().ToList();
                foreach (object[] fooo in listcc)
                {
                    cc += "'" + fooo[1].ToString().ToString() + "',";
                }
                if (cc.Length > 0)
                {
                    str2 = " and BuyerName in (" + cc.Trim(',') + ")   order by BuyerName  ";
                }

            }
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType " + str + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<OrderType>();

            // 判断是否合并订单并返回主订单
            if (list.Count == 0 && isUn == 0)
            {
                list = base.NSession.CreateQuery("from OrderType " + str.Replace("Enabled=1 and ", "") + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<OrderType>();

                // 合并订单
                if (list.Count > 0 && list[0].IsMerger == 1)
                {
                    list = base.NSession.CreateQuery("from OrderType Where Id=:p1").SetInt32("p1", list[0].MId).List<OrderType>();
                }
            }

            string ids = "";
            foreach (OrderType orderType in list)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Province = orderType.AddressInfo.Province;
                orderType.ReFanAmount = Math.Round(orderType.Amount * 0.95, 4);
                ids += "'" + orderType.OrderExNo + "',";

                // 根据审批状态与限定天数控制订单打印
                TimeSpan span = DateTime.Now - orderType.CreateOn;
                int datediff = span.Days;
                if (datediff > Convert.ToInt32(day1) && orderType.AllowDelivery == 1)
                {
                    // 大于15天同时已审批允许打印
                    orderType.IsAllowDelivery = true;
                }
                else if (datediff <= Convert.ToInt32(day1))
                {
                    // 小于等于15天允许打印
                    orderType.IsAllowDelivery = true;
                }
                else
                {
                    // 禁止打印
                    orderType.IsAllowDelivery = false;
                }
            }
            ids = ids.Trim(',');
            if (ids.Length > 10)
            {
                List<ServiceExaminationType> list2 =
                NSession.CreateQuery("from ServiceExaminationType where OrderNo in (" + ids + ")").List<ServiceExaminationType>().ToList();
                foreach (OrderType orderType in list)
                {
                    orderType.PeiKuan = list2.Sum(x => x.ExamineAmount);
                    orderType.Freight = Math.Round(orderType.Freight, 2);
                    //try
                    //{
                    //    orderType.BuyUnreason = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format(" select Count(distinct D.OrderNo) from Orders O  left join DisputeRecordType D on O.OrderExNo =D.OrderNo where D.ExamineClass='客户无理取闹' and O.BuyerName='" + orderType.BuyerName.Replace("'", " ").Trim() + "'")).UniqueResult());
                    //}
                    //catch
                    //{
                        orderType.BuyUnreason = 0;
                    //}
                }
            }

            object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType " + str).UniqueResult();
            return base.Json(new { total = obj2, rows = list });
        }


        public JsonResult HeList(int page, int rows, string sort, string order, string search, int isUn = 0)
        {
            string str = "";
            string str2 = " order by Id desc";
            string str3 = "<>";
            if (isUn == 1)
            {
                str3 = "=";
            }
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                if (sort == "OrderExNo")
                {
                    sort = "(case when Amount=0 then 0 else Profit/Amount end)";
                }
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where Enabled=1 and  Status" + str3 + "'待处理' and " + str;
                }
            }
            if (str.Length == 0)
            {
                str = " where Enabled=1 and  Status" + str3 + "'待处理'";
            }
            string acs = "";
            foreach (AccountType c in GetCurrentAccount().Accounts)
            {
                acs += "'" + c.AccountName + "',";
            }
            acs = acs.Trim(',');
            if (acs.Length > 0)
            {
                str += " and Account in (" + acs + ")";
            }
            else if (acs.Length == 0)
            {
                str += " and Account in ('')";
            }

            List<KeyValue> queryDic = Utilities.StringToDictionary(search);
            KeyValue kv1 = queryDic.Find(x => x.Key == "ScanningOn_st");
            KeyValue kv2 = queryDic.Find(x => x.Key == "ScanningOn_et");
            KeyValue kv3 = queryDic.Find(x => x.Key == "Platform_es");
            KeyValue kv4 = queryDic.Find(x => x.Key == "Account_es");
            string where2 = "where 1=1 ";
            if (kv1 != null)
                //where2 += " and CreateOn >= '" + kv1.Value + "' ";
                where2 += " and ExamineOn >= '" + kv1.Value + "' ";
            if (kv2 != null)
            {

                DateTime date = Convert.ToDateTime(kv2.Value);
                if (date.Hour == 0 && date.Minute == 0)
                    //where2 += " and CreateOn <= '" + date.ToString("yyyy-MM-dd 23:59:59") + "'";
                    where2 += " and ExamineOn <= '" + date.ToString("yyyy-MM-dd 23:59:59") + "'";
                else
                    //where2 += " and CreateOn <= '" + kv2.Value + "'";
                    where2 += " and ExamineOn <= '" + kv2.Value + "'";
            }
            if (kv3 != null)
            {
                where2 += " and Platform = '" + kv3.Value + "'";
            }
            if (kv4 != null)
            {
                where2 += " and Account = '" + kv4.Value + "'";
            }
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType " + str + " " + str2 + "").SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<OrderType>();
            //IList<OrderType> list = base.NSession.CreateQuery("from OrderType " + str + " and mid=0 " + str2 + "").SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<OrderType>();
            string ids = "";
            CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
            foreach (OrderType orderType in list)
            {
                // 当FBA订单设置运费为0
                if (orderType.FBABy == "FBA")
                {
                    orderType.Freight = 0;
                }
                if (orderType.CurrencyCode != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == orderType.CurrencyCode);

                    if (currencyType != null || currencyType2 != null)
                    {
                        orderType.FanAmount =
                            Math.Round(orderType.FanAmount * Convert.ToDecimal(currencyType.CurrencyValue) / Convert.ToDecimal(currencyType2.CurrencyValue),
                                       2);
                    }

                }
                orderType.ReFanAmount = Math.Round(orderType.Amount * 0.95, 4);
                if (orderType.ReFanAmount > 0)
                    orderType.ReProfitRate = Convert.ToDecimal(Math.Round((orderType.ReFanAmount * 6.2 - orderType.ProductFees - orderType.Freight) /
                                            (orderType.ReFanAmount * 6.2), 6)) * 100;
                if (orderType.FanAmount > 0)
                    orderType.ProfitRate = Convert.ToDecimal(Math.Round((Convert.ToDouble(orderType.FanAmount) * 6.2 - orderType.ProductFees - orderType.Freight) / ((Convert.ToDouble(orderType.FanAmount) * 6.2)), 6)) * 100;
                ids += "'" + orderType.OrderExNo + "',";
                orderType.Freight = Math.Round(orderType.Freight, 6);

            }
            ids = ids.Trim(',');
            object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType " + str).UniqueResult();
            // object totalAmount = base.NSession.CreateQuery("select CurrencyCode,SUM(Amount) from OrderType " + str).UniqueResult();//总订单金额
            IList<object[]> listAmount = base.NSession.CreateQuery("select CurrencyCode,SUM(Amount) from OrderType " + str + " group by CurrencyCode").List<object[]>();
            object totalProfit = base.NSession.CreateQuery("select SUM(Profit) from OrderType " + str).UniqueResult();//总订单金额
            //object totalRefanAmount = base.NSession.CreateQuery("select SUM(ReFanAmount) from OrderType " + str).UniqueResult();//总预计收款
            //object totalFanAmount = base.NSession.CreateQuery("select SUM(FanAmount) from OrderType " + str).UniqueResult();// 总实际收款
            IList<object[]> listFanAmount = base.NSession.CreateQuery("select CurrencyCode,SUM(FanAmount) from OrderType " + str + " group by CurrencyCode").List<object[]>();
            object totalProductFees = base.NSession.CreateQuery("select SUM(ProductFees) from OrderType " + str).UniqueResult();//总产品
            object totalFreight = base.NSession.CreateQuery("select SUM(Freight) from OrderType " + str + " and FBABy<>'FBA'").UniqueResult();//总运费

            // 总赔款
            //object totalPeiKuan = base.NSession.CreateQuery("select SUM(ExamineAmount) from DisputeRecordType " + where2).UniqueResult();//总赔款

            // 总赔款
            decimal totalPeiKuan = 0;
            // 计算赔款并累计折合人民币
            // ExamineStatus:4(已审批,已付款);6(已处理,平台付款)
            List<DisputeRecordType> DisputeRecordList = NSession.CreateQuery("from DisputeRecordType " + where2 + " and (ExamineStatus=4 or ExamineStatus=6) ").List<DisputeRecordType>().ToList();

            foreach (DisputeRecordType DisputeRecord in DisputeRecordList)
            {
                //CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == DisputeRecord.ExamineCurrencyCode);

                //if (currencyType != null || currencyType2 != null)
                //{
                //    totalPeiKuan += DisputeRecord.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                //}
                totalPeiKuan += DisputeRecord.ExamineAmountRmb;
                //if (DisputeRecord.ExamineAmountRmb != 0)
                //{
                //    // 当设置过人民币时统计人民币
                //    totalPeiKuan += DisputeRecord.ExamineAmountRmb;
                //}
                //else
                //{
                //    // 未折换人民币时按6.4汇率平均计算
                //    totalPeiKuan += DisputeRecord.ExamineAmount * Convert.ToDecimal(6.3);
                //}
            }

            totalPeiKuan = Math.Round(totalPeiKuan, 2);

            decimal totalAmount = 0;
            decimal totalFanAmount = 0;

            // 当货币非美元时反向推算为人美显示
            foreach (object[] objectse in listAmount)
            {
                if (objectse[0].ToString() != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == objectse[0].ToString());

                    if (currencyType != null || currencyType2 != null)
                    {
                        totalAmount +=
                             Math.Round(Convert.ToDecimal(objectse[1]) * Convert.ToDecimal(currencyType.CurrencyValue) / Convert.ToDecimal(currencyType2.CurrencyValue),
                                        2);
                    }

                }
                else
                {
                    totalAmount += Convert.ToDecimal(objectse[1]);
                }
            }
            // 当货币非美元时反向推算为人美显示
            foreach (object[] objectse in listFanAmount)
            {
                if (objectse[0].ToString() != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == objectse[0].ToString());

                    if (currencyType != null || currencyType2 != null)
                    {
                        totalFanAmount +=
                             Math.Round(Convert.ToDecimal(objectse[1]) * Convert.ToDecimal(currencyType.CurrencyValue) / Convert.ToDecimal(currencyType2.CurrencyValue),
                                        2);
                    }

                }
                else
                {
                    totalFanAmount += Convert.ToDecimal(objectse[1]);
                }
            }
            // object obj2 = base.NSession.CreateQuery("select SUM(ExamineAmount) from ServiceExaminationType " + str).UniqueResult();//总赔款
            List<OrderType> Footorders = new List<OrderType>();
            OrderType orderfoot = new OrderType
                                  {
                                      Amount = Math.Round(Utilities.ToDouble(totalAmount), 2),
                                      FanAmount = Utilities.ToDecimal(totalFanAmount),
                                      //ReFanAmount = Math.Round(Utilities.ToDouble(totalRefanAmount), 2),
                                      PeiKuan = Utilities.ToDecimal(totalPeiKuan),
                                      ProductFees = Math.Round(Utilities.ToDouble(totalProductFees), 2),
                                      Freight = Math.Round(Utilities.ToDouble(totalFreight), 2),
                                      Profit = Math.Round(Utilities.ToDouble(Utilities.ToDecimal(totalProfit) - totalPeiKuan), 2)  // 总利润-总赔款
                                  };
            orderfoot.ReFanAmount = Math.Round(orderfoot.Amount * 0.95, 4);
            if (orderfoot.ReFanAmount > 0)
                orderfoot.ReProfitRate = Convert.ToDecimal(Math.Round((orderfoot.ReFanAmount * 6.2 - orderfoot.ProductFees - orderfoot.Freight) /
                                        (orderfoot.ReFanAmount * 6.2), 6)) * 100;
            if (orderfoot.FanAmount > 0)
                orderfoot.ProfitRate = Convert.ToDecimal(Math.Round((Convert.ToDouble(orderfoot.FanAmount) * 6.2 - orderfoot.ProductFees - orderfoot.Freight) / ((Convert.ToDouble(orderfoot.FanAmount) * 6.2)), 6)) * 100;
            Footorders.Add(orderfoot);
            return base.Json(new { total = obj2, rows = list, footer = Footorders });
        }

        public JsonResult ListQ(string q)
        {
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo like '%" + q + "%'").SetFirstResult(0).SetMaxResults(10).List<OrderType>();
            return base.Json(new { total = list.Count, rows = list });
        }

        /// <summary>
        /// 合并订单
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public JsonResult MergerOrder(int a, string b)
        {
            OrderType byId = this.GetById(a);
            OrderType entity = null;
            List<OrderType> list = base.GetList<OrderType>("OrderNo", b, "");
            if (list.Count > 0)
            {
                entity = list[0];
                if (entity.Status == "已处理")
                {
                    List<OrderProductType> list2 = base.GetList<OrderProductType>("OId", entity.Id.ToString(), "");
                    foreach (OrderProductType type3 in list2)
                    {
                        base.NSession.Evict(type3);
                        type3.Id = 0;
                        type3.OId = byId.Id;
                        type3.OrderNo = byId.OrderNo;
                        this.Save<OrderProductType>(type3);
                    }
                    entity.Enabled = 0;
                    entity.MId = byId.Id;
                    entity.IsMerger = 1;
                    byId.IsMerger = 1;
                    byId.IsPrint = 0;
                    byId.Amount += entity.Amount;  // 订单金额合并
                    byId.ProductFees += entity.ProductFees; // 产品费用合并
                    this.Update<OrderType>(byId);
                    this.Update<OrderType>(entity);

                    // 计算订单财务数据
                    OrderHelper.ReckonFinance(byId, base.NSession);

                    // 计算订单财务数据
                    OrderHelper.ReckonFinance(entity, base.NSession);
                    LoggerUtil.GetOrderRecord(byId, "订单合并！", "当前订单和" + entity.OrderNo + "合并为一个订单！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = true });
                }
                return base.Json(new { IsSuccess = false, Msg = "订单状态不符！" });
            }
            return base.Json(new { IsSuccess = false, Msg = "订单编号错误" });
        }

        public ActionResult OrderExport()
        {
            return base.View();
        }

        public ActionResult OrderExportII()
        {
            return base.View();
        }

        public ActionResult OrderImport()
        {
            return base.View();
        }

        public ActionResult OrderReturnList()
        {
            return base.View();
        }

        public ActionResult OrderSend()
        {
            return base.View();
        }

        public ActionResult OrderSendByVali()
        {
            return base.View();
        }
        public ActionResult OrderSendDirect()
        {
            return base.View();
        }
        public ActionResult OrderSendByVali1()
        {
            return base.View();
        }
        public ActionResult OrderSendByVali2()
        {
            return base.View();
        }
        public ActionResult OrderSendByVali3()
        {
            return base.View();
        }

        public JsonResult ReBJScanData(string o)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode ='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType orderType = list[0];
                if (orderType.LogisticMode == "江西小包")
                {
                    orderType.LogisticMode = "北京小包";
                    orderType.TrackCode = Utilities.GetTrackCode(NSession, orderType.LogisticMode);
                    NSession.Update(orderType);
                    NSession.Flush();

                    string format = "select O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.TrackCode as '追踪码', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',P.Location as '库位' from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU   left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id={0}  where  O.IsOutOfStock=0 and O.OrderNo in('{1}') Order By O.OrderNo  ";
                    format = string.Format(format, 5, orderType.OrderNo);
                    IDbCommand command = base.NSession.Connection.CreateCommand();
                    command.CommandText = format;
                    SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    List<string> list2 = new List<string>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        object obj = NSession.CreateSQLQuery("select top 1 AreaName from [LogisticsArea] where LId = (select top 1 ParentID from LogisticsMode where LogisticsCode='" + row["订单发货方式"] + "')  and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode  and CountryCode in (select ID from Country where ECountry=N'" + row["收件人国家"].ToString().Replace("'", "''") + "') )").UniqueResult();
                        row["分区"] = obj;

                        if (!list2.Contains(row["订单编号"].ToString()))
                        {
                            LoggerUtil.GetOrderRecord(Convert.ToInt32(row["Id"]), row["订单编号"].ToString(), "订单打印", CurrentUser.Realname + "订单打印！", CurrentUser, NSession);
                            list2.Add(row["订单编号"].ToString());
                        }
                    }
                    dataSet.Tables[0].DefaultView.Sort = " 订单编号 Asc";
                    base.NSession.CreateQuery("update OrderType set IsPrint=IsPrint+1 where  IsAudit=1 and  OrderNo IN('" + orderType.OrderNo + "') ").ExecuteUpdate();
                    string xml = dataSet.GetXml();
                    PrintDataType type = new PrintDataType
                    {
                        Content = xml,
                        CreateOn = DateTime.Now
                    };
                    base.NSession.Save(type);
                    base.NSession.Flush();

                    return Json(new { IsSuccess = true, PrintId = type.Id, Msg = "转单成功！" });
                }
                return Json(new { IsSuccess = false, PrintId = 0, Msg = "转单失败！" });
            }
            return Json(new { IsSuccess = false, PrintId = 0, Msg = "转单失败！" });
        }



        public JsonResult OutStockByAliSend(string o, string t, int f)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                string str = "";
                if (string.IsNullOrEmpty(type.TrackCode2))
                {
                    type.TrackCode2 = t;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    LoggerUtil.GetOrderRecord(type, "订单挂号提前扫描！", "订单挂号提前扫描！", base.CurrentUser, base.NSession);
                    str = "订单:" + type.OrderNo + ", 设置成功，追踪号为：" + t;
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                if (f == 1)
                {
                    type.TrackCode2 = t;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    LoggerUtil.GetOrderRecord(type, "订单挂号提前扫描！", "订单挂号提前扫描！", base.CurrentUser, base.NSession);
                    str = "订单:" + type.OrderNo + ", 设置成功，追踪号为：" + t;
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                str = "订单:" + type.OrderNo + ", 已经扫描过!";
                return base.Json(new { IsSuccess = false, Result = str });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult OutStockByBeforePei(string p1, string o)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                if ((type.Status == OrderStatusEnum.待拣货.ToString()) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    type.Status = "待拣货";
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    BeforePeiScanType type2 = new BeforePeiScanType
                    {
                        OId = type.Id,
                        OrderNo = type.OrderNo,
                        PeiBy = p1,
                        CreatBy = base.CurrentUser.Realname,
                        CreateOn = DateTime.Now
                    };
                    base.NSession.Save(type2);
                    base.NSession.Flush();
                    LoggerUtil.GetOrderRecord(type, "订单配货前扫描！", "将订单配货前扫描，" + p1 + "待拣货！", base.CurrentUser, base.NSession);
                    string str = "订单： " + type.OrderNo + "开始拣货！配货人：" + p1;
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                return base.Json(new { IsSuccess = false, Result = "订单状态不符！现在的订单状态为：" + type.Status + " 将订单状态设置为“已处理”才能配货前扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult OutStockByJi(string p, string o)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType order = list[0];
                if (order.Status == OrderStatusEnum.待包装.ToString())
                {
                    LoggerUtil.GetOrderRecord(order, "订单计件扫描！", "将订单 包装疾计件！", base.CurrentUser, base.NSession);
                    order.Status = OrderStatusEnum.待发货.ToString();
                    base.NSession.Update(order);
                    base.NSession.Flush();
                    this.SaveRecord(order, p);
                    string str = "订单： " + order.OrderNo + "计件成功！包装人：" + p;
                    return base.Json(new { IsSuccess = true, Result = str });
                }
                return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + order.Status + "，需要订单状态为“待发货”方可扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult OutStockByPei(string p1, string p2, string o, string skuCode)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count <= 0)
            {
                return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
            }
            OrderType type = list[0];
            if ((type.Status != OrderStatusEnum.待拣货.ToString()) && !(type.Status == OrderStatusEnum.已处理.ToString()))
            {
                return base.Json(new { IsSuccess = false, Result = "订单状态不符！现在的订单状态为：" + type.Status + " 将订单状态设置为“已处理”才能配货扫描！" });
            }
            bool flag = false;
            OrderPeiRecordType type2 = new OrderPeiRecordType
            {
                OrderNo = type.OrderNo,
                PeiBy = p1,
                ValiBy = p2,
                CreateOn = DateTime.Now,
                OId = type.Id,
                ScanBy = base.CurrentUser.Realname
            };
            base.NSession.Save(type2);
            base.NSession.Flush();
            type.Status = OrderStatusEnum.待包装.ToString();
            if (type.IsOutOfStock == 1)
            {
                flag = true;
            }
            type.IsCanSplit = 0;
            type.IsOutOfStock = 0;
            base.NSession.Update(type);
            base.NSession.Flush();
            base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
            if (skuCode != "")
            {
                base.NSession.CreateQuery("update SKUCodeType set IsOut=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + skuCode.Replace(",", "','") + "')").ExecuteUpdate();
            }
            string str = "订单：" + type.OrderNo + " 配货完成！";
            if (flag)
            {
                LoggerUtil.GetOrderRecord(type, "订单配货扫描！", "将订单配货扫描，订单的缺货状态删除！", base.CurrentUser, base.NSession);
                IList<OrderOutRecordType> list2 = base.NSession.CreateQuery("from OrderOutRecordType where OId='" + type.Id + "'").List<OrderOutRecordType>();
                foreach (OrderOutRecordType type3 in list2)
                {
                    type3.IsRestoration = 1;
                    type3.RestorationBy = base.CurrentUser.Realname;
                    type3.RestorationOn = DateTime.Now;
                    base.NSession.Update(type3);
                    base.NSession.Flush();
                }
            }
            else
            {
                LoggerUtil.GetOrderRecord(type, "订单配货扫描！", "将订单配货扫描！", base.CurrentUser, base.NSession);
            }
            return base.Json(new { IsSuccess = true, Result = str });
        }

        public JsonResult OutStockByQue(string o, string ids)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType order = list[0];
                if (order.Status == OrderStatusEnum.已处理.ToString())
                {
                    LoggerUtil.GetOrderRecord(order, "缺货扫描", base.CurrentUser.Realname + "将订单添加到 添加到缺货订单中！", base.CurrentUser, base.NSession);
                    order.IsOutOfStock = 1;
                    order.IsPrint = 0;
                    base.NSession.Update(order);
                    base.NSession.Flush();
                    string str = "";
                    foreach (OrderProductType type2 in base.NSession.CreateQuery(" from OrderProductType where OId=" + order.Id).List<OrderProductType>())
                    {
                        if (ids.IndexOf(type2.Id.ToString()) != -1)
                        {
                            object obj2 = str;
                            str = string.Concat(new object[] { obj2, "SKU:", type2.SKU, " Qty:", type2.Qty });
                            type2.IsQue = 1;
                            base.NSession.Update(type2);
                            base.NSession.Flush();
                        }
                    }
                    OrderOutRecordType type3 = new OrderOutRecordType
                    {
                        OId = order.Id,
                        OrderNo = order.OrderNo,
                        OrderExNo = order.OrderExNo,
                        RestorationBy = base.CurrentUser.Realname,
                        RestorationOn = DateTime.Now,
                        CreateBy = base.CurrentUser.Realname,
                        CreateOn = DateTime.Now,
                        IsRestoration = 0,
                        Remark = str
                    };
                    base.NSession.Save(type3);
                    base.NSession.Flush();
                    string str2 = "订单：" + order.OrderNo + " 添加到缺货！";
                    return base.Json(new { IsSuccess = true, Result = str2 });
                }
                return base.Json(new { IsSuccess = false, Result = "订单状态不符！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        public JsonResult OutStockBySend(string o, string t, int s, string l, double w)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and TrackCode='" + t + "'").UniqueResult();
                if (Utilities.ToInt(obj2) > 0)
                {
                    LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + t + "重复，之前已经有订单使用！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "重复，之前已经有订单使用！" });
                }

                if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    type.TrackCode = t;
                    type.Weight = Convert.ToInt32(w);
                    if (l != "")
                    {
                        type.LogisticMode = l;

                    }
                    IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                    foreach (OrderProductType orderProductType in list2)
                    {
                        List<ProductType> product =
                          NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List
                              <ProductType>().ToList();
                        if (product != null)
                        {

                        }
                    }

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.已发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    type.IsFreight = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();

                    foreach (OrderProductType type2 in list2)
                    {
                        Utilities.StockOut(s, type2.SKU, type2.Qty, "扫描出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                    }
                    base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！", base.CurrentUser, base.NSession);
                    string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", l, "  重量：", w });
                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                    try
                    {
                        new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad)) { IsBackground = true }.Start(type);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        // 计算订单财务数据
                        OrderHelper.ReckonFinance(type, base.NSession);
                    }

                    base.NSession.Update(type);
                    base.NSession.Flush();
                    return base.Json(new { IsSuccess = true, Result = str, OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        /// <summary>
        /// 订单发货(新)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="w"></param>
        /// <param name="skuCode"></param>
        /// <returns></returns>
        public JsonResult OutStockBySendVali(string o, string t, int s, string l, double w, string skuCode, string packageMan)
        {
            if (w == 0)
            {
                return base.Json(new { IsSuccess = false, Result = "重量不能为0" });
            }
           
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode ='" + o + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                //object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and TrackCode='" + t + "'").UniqueResult();
                //object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where MId <> 0 and TrackCode='" + t + "'").UniqueResult();//被合并订单不计入判断追踪号重复数量
                //所有类型的订单都计入重复计算中
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where  TrackCode='" + t + "'").UniqueResult();
                if (Utilities.ToInt(obj2) > 1)
                {
                    LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + t + "重复，之前已经有订单使用！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "重复，之前已经有订单使用！" });
                }
                if (type.LogisticMode == "线上发货")
                    if (t != type.TrackCode)
                    {
                        return base.Json(new { IsSuccess = false, Result = "运单号" + t + "和系统中的运单号" + type.TrackCode + "不一致" });
                    }
                if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    string ttt = type.TrackCode;
                    //type.TrackCode = t;
                    // 判断是否手动添加跟踪码,当手动添加跟踪码时跟踪码记录
                    if (type.TrackCode == "已用完")
                    {
                        type.TrackCode = t;
                    }
                    type.Weight = Convert.ToInt32(w);
                    if (l != "")
                    {
                        type.LogisticMode = l;
                    }

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.已发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    type.IsFreight = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    ////再次update,避免某些订单重量为0，更新状态和扫描时间，已发货状态一定是已打印过的
                    //base.NSession.CreateSQLQuery("update Orders set Status='已发货',Weight='" + Convert.ToInt32(w) + "',IsPrint=IsPrint+1，ScanningOn='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                    IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                    foreach (OrderProductType type2 in list2)
                    {
                        Utilities.StockOut(s, type2.SKU, type2.Qty, "扫描出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                    }
                    //更新重量
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
                                    if ((product[0].Weight * 1.1) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.9) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                else
                                {
                                    if ((product[0].Weight * 1.05) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.95) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                if (iscon)
                                {
                                    product[0].Weight = Convert.ToInt32(w);
                                    NSession.Update(product[0]);
                                    NSession.Flush();
                                    LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + w, CurrentUser, NSession);

                                }
                            }

                        }
                    }
                    base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + type.TrackCode + " 原单号:" + ttt, base.CurrentUser, base.NSession);
                    if (skuCode != "")
                    {
                        base.NSession.CreateQuery("update SKUCodeType set IsOut=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + skuCode.Replace(",", "','") + "')").ExecuteUpdate();
                    }
                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                    string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", l, "  重量：", w });
                    try
                    {
                        new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad)) { IsBackground = true }.Start(type);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        // 计算订单财务数据
                        OrderHelper.ReckonFinance(type, base.NSession);
                    }
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    //将包装工信息计入计价表
                    PackagingScanLogType packagelog = new PackagingScanLogType();
                    packagelog.PackageType = "混合";
                    packagelog.Operator = packageMan;
                    packagelog.OperationOn = DateTime.Now;
                    packagelog.OrderNo = o;
                    NSession.SaveOrUpdate(packagelog);
                    NSession.Flush();
                    return base.Json(new { IsSuccess = true, Result = str, OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        /// <summary>
        /// 扫描发货(新)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="w"></param>
        /// <param name="skuCode"></param>
        /// <returns></returns>
        public JsonResult OutStockBySendVali1(string o, string t, int s, string l, double w, string skuCode, int IsWeight)
        {
            bool IsSku = false;
            if (w == 0)
            {
                return base.Json(new { IsSuccess = false, Result = "重量不能为0" });
            }
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where Enabled=1 and (OrderNo='" + o + "' Or TrackCode ='" + o + "')").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                //object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and TrackCode='" + t + "'").UniqueResult();
                //if (type.MId != 0 && type.Enabled == 0)
                //{
                //    return base.Json(new { IsSuccess = false, Result = "该订单已被合并，重新配货！" });
                //}
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Enabled=1 and TrackCode='" + t + "'").UniqueResult();
                //if (type.LogisticMode == "e邮宝（eBay）" && w >= 2000)
                //{
                //    return base.Json(new { IsSuccess = false, Result = "e邮宝（eBay）重量不能超过2KG" });
                //}
                if (IsWeight == 1 && w >= 2000)
                {
                    return base.Json(new { IsSuccess = false, Result = "重量不能超过2KG" });
                }
                if (Utilities.ToInt(obj2) > 1)
                {
                    LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + t + "重复，之前已经有订单使用！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "重复，之前已经有订单使用！" });
                }
                //if (type.LogisticMode == "线上发货")
                if (t != type.TrackCode)
                {
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "和系统中的运单号" + type.TrackCode + "不一致" });
                }
                if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    string ttt = type.TrackCode;
                    // type.TrackCode = t;
                    //  type.Weight = Convert.ToInt32(w);
                    if (l != "")
                    {
                        type.LogisticMode = l;
                    }

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.已发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    type.IsPrint = type.IsPrint + 1;
                    type.IsFreight = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    ////再次update,避免某些订单重量为0，更新状态和扫描时间，已发货状态一定是已打印过的
                    //base.NSession.CreateSQLQuery("update Orders set Status='已发货',Weight='" + Convert.ToInt32(w) + "',IsPrint=IsPrint+1，ScanningOn='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                    IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                    foreach (OrderProductType type2 in list2)
                    {
                        if (type2.SKU.Contains("WK206"))
                        {
                            IsSku = true;
                        }
                        Utilities.StockOut(s, type2.SKU, type2.Qty, "面单出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                    }
                    if (IsSku)
                    {
                        w = Convert.ToInt32(OrderHelper.GerProductWeightExpect(type.OrderNo, NSession));
                    }

                    //更新重量
                    if (list2.Count == 1 && list2[0].Qty == 1 && (!IsSku))
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
                                    if ((product[0].Weight * 1.1) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.9) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                else
                                {
                                    if ((product[0].Weight * 1.05) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.95) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                if (iscon)
                                {
                                    //if (!IsSku)
                                    //{
                                    product[0].Weight = Convert.ToInt32(w);
                                    NSession.Update(product[0]);
                                    NSession.Flush();
                                    LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + w, CurrentUser, NSession);
                                    //}

                                }
                            }

                        }
                    }
                    base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + t + " 原单号:" + type.TrackCode, base.CurrentUser, base.NSession);
                    if (skuCode != "")
                    {
                        base.NSession.CreateQuery("update SKUCodeType set IsOut=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + skuCode.Replace(",", "','") + "')").ExecuteUpdate();
                    }
                    string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", l, "  重量：", w });
                    //type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                    try
                    {
                        new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad1)) { IsBackground = true }.Start(type);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        // 计算订单财务数据 ---试用关闭一段时间
                        List<OrderType> listC = base.NSession.CreateQuery("from OrderType where Enabled=0 and MId=" + type.Id).List<OrderType>().ToList<OrderType>();
                        if (listC.Count == 0)
                        {
                            OrderHelper.ReckonFinance(type, base.NSession);
                        }
                        else//合并订单计算预计运费
                        {
                            type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)w, type.LogisticMode, type.Country, base.NSession, 0M));
                        }
                        //   OrderHelper.ReckonFinance(type, base.NSession);
                    }
                    type.Weight = Convert.ToInt32(w);
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    return base.Json(new { IsSuccess = true, Result = str, OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        /// <summary>
        /// 扫描发货只扫描追踪号出库，不需要SKUCode--义乌主WK206将有1w数量按照此发货方式发货---2016-10-13
        /// </summary>
        /// <param name="o"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="w"></param>
        /// <param name="skuCode"></param>
        /// <returns></returns>
        public JsonResult OutStockBySendVali2(string o, string t, int s, string l, double w, int IsWeight)
        {
            bool IsSku = false;
            if (w == 0)
            {
                return base.Json(new { IsSuccess = false, Result = "重量不能为0" });
            }
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where Enabled=1 and (OrderNo='" + o + "' Or TrackCode ='" + o + "')").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                //object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and TrackCode='" + t + "'").UniqueResult();
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Enabled=1 and TrackCode='" + t + "'").UniqueResult();
                //if (type.MId != 0 && type.Enabled == 0)
                //{
                //    return base.Json(new { IsSuccess = false, Result = "该订单已被合并，重新配货！" });
                //}
                //if (type.LogisticMode == "e邮宝（eBay）" && w >= 2000)
                //{
                //    return base.Json(new { IsSuccess = false, Result = "e邮宝（eBay）重量不能超过2KG" });
                //}
                if (IsWeight == 1 && w >= 2000)
                {
                    return base.Json(new { IsSuccess = false, Result = "重量不能超过2KG" });
                }
                if (Utilities.ToInt(obj2) > 1)
                {
                    LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + t + "重复，之前已经有订单使用！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "重复，之前已经有订单使用！" });
                }
                //if (type.LogisticMode == "线上发货")
                if (t != type.TrackCode)
                {
                    return base.Json(new { IsSuccess = false, Result = "运单号" + t + "和系统中的运单号" + type.TrackCode + "不一致" });
                }
                if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    string ttt = type.TrackCode;
                    //  type.TrackCode = t;
                    //    type.Weight = Convert.ToInt32(w);
                    if (l != "")
                    {
                        type.LogisticMode = l;
                    }

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.已发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    type.IsPrint = type.IsPrint + 1;
                    type.IsFreight = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    ////再次update,避免某些订单重量为0，更新状态和扫描时间，已发货状态一定是已打印过的
                    //base.NSession.CreateSQLQuery("update Orders set Status='已发货',Weight='" + Convert.ToInt32(w) + "',IsPrint=IsPrint+1，ScanningOn='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                    IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
                    foreach (OrderProductType type2 in list2)
                    {
                        if (type2.SKU.Contains("WK206"))
                        {
                            IsSku = true;
                        }
                        //if (IsSku)
                        //{
                        //    w = Convert.ToInt32(OrderHelper.GerProductWeightExpect(type.OrderNo, NSession));
                        Utilities.StockOut(s, type2.SKU, type2.Qty, "直接出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                        //}
                        //else
                        //{
                        //    //return base.Json(new { IsSuccess = false, Result = " 无法出库！ SKU不满足特殊主SKU：" + "WK206" + "，请确认！" });
                        //    Utilities.StockOut(s, type2.SKU, type2.Qty, "直接出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
                        //}
                    }
                    if (IsSku)
                    {
                        w = Convert.ToInt32(OrderHelper.GerProductWeightExpect(type.OrderNo, NSession));
                    }
                    //更新重量
                    if (list2.Count == 1 && list2[0].Qty == 1 && (!IsSku))
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
                                    if ((product[0].Weight * 1.1) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.9) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                else
                                {
                                    if ((product[0].Weight * 1.05) < Convert.ToDouble(w) ||
                                        (product[0].Weight * 0.95) > Convert.ToDouble(w))
                                        iscon = true;
                                }
                                if (iscon)
                                {
                                    //if (!IsSku)
                                    //{
                                    product[0].Weight = Convert.ToInt32(w);
                                    NSession.Update(product[0]);
                                    NSession.Flush();
                                    LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + w, CurrentUser, NSession);
                                    //}

                                }
                            }

                        }
                    }
                    //base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                    LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + t + " 原单号:" + type.TrackCode, base.CurrentUser, base.NSession);
                    //if (skuCode != "")
                    //{
                    //    base.NSession.CreateQuery("update SKUCodeType set IsOut=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + skuCode.Replace(",", "','") + "')").ExecuteUpdate();
                    //}
                    string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", l, "  重量：", w });
                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                    try
                    {
                        new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad1)) { IsBackground = true }.Start(type);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        // 计算订单财务数据
                        //  OrderHelper.ReckonFinance(type, base.NSession);
                    }
                    type.Weight = Convert.ToInt32(w);
                    base.NSession.Update(type);
                    base.NSession.Flush();
                    return base.Json(new { IsSuccess = true, Result = str, OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        //内销直接扫描记录保存（不减库存）
        public JsonResult OutStockByDirect(int wus, int s, string skuCode)
        {
            string wus_name = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Realname from users where  Id='" + wus + "'")).UniqueResult());//经手人姓名
            string s_name = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select WName from Warehouse where  Id='" + s + "'")).UniqueResult());//仓库名称
            string[] str = skuCode.Split(',');
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    List<ProductType> list = base.NSession.CreateQuery("from ProductType where id ='" + str[i] + "'").List<ProductType>().ToList<ProductType>();
                    foreach (var type in list)
                    {
                        if (wus == 0)
                        {
                            Utilities.StockOutDirect(s_name, wus, type.SKU, 1, "内销扫描出库", base.CurrentUser.Realname, "", "", Convert.ToDecimal(type.Price), base.NSession);
                        }
                        else
                        {
                            Utilities.StockOutDirect(s_name, wus, type.SKU, 1, "内销扫描出库", wus_name, "", "", Convert.ToDecimal(type.Price), base.NSession);
                        }
                    }
                }
                double _amount = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select sum(Price) from StockOut where OutType='内销扫描出库'")).UniqueResult());
                return base.Json(new { IsSuccess = true, Result = "操作成功！", Cc = (Math.Round(_amount) * 0.8).ToString(), count = str.Length });
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, Result = "操作失败！" });
            }
            double _amount1 = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select sum(Price) from StockOut where OutType='内销扫描出库'")).UniqueResult());
            return base.Json(new { IsSuccess = false, Result = "操作失败！", Cc = (Math.Round(_amount1) * 0.8).ToString() });
        }

        public ActionResult PeiScan()
        {
            return base.View();
        }

        public ActionResult QueScan()
        {
            return base.View();
        }

        public ActionResult QuestionOrderIndex()
        {
            return base.View();
        }

        public ActionResult QuestionScan()
        {
            return base.View();
        }

        public JsonResult Record(int id)
        {
            IList<OrderRecordType> list = base.NSession.CreateQuery("from OrderRecordType where Oid='" + id + "'").List<OrderRecordType>();
            return base.Json(from p in list
                             orderby p.CreateOn descending
                             select p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnScan()
        {
            return base.View();
        }

        public void SaveRecord(OrderType order, string p)
        {
            IList<OrderProductType> list = base.NSession.CreateQuery("from OrderProductType where OId='" + order.Id + "'").List<OrderProductType>();
            double packCoefficient = 0.0;
            string sKU = "";
            if (list.Count == 0)
            {
                packCoefficient = 3.0;
            }
            foreach (OrderProductType type in list)
            {
                IList<ProductType> list2 = base.NSession.CreateQuery("from ProductType where SKU='" + type.SKU + "'").List<ProductType>();
                if (list2.Count != 0)
                {
                    if (list2[0].PackCoefficient > packCoefficient)
                    {
                        packCoefficient = list2[0].PackCoefficient;
                        sKU = list2[0].SKU;
                    }
                }
                else
                {
                    packCoefficient = 1.0;
                }
            }
            OrderPackRecordType type2 = new OrderPackRecordType
            {
                OId = order.Id,
                OrderNo = order.OrderNo,
                PackBy = p,
                PackOn = DateTime.Now,
                ScanBy = base.CurrentUser.Realname,
                PackCoefficient = packCoefficient,
                SKU = sKU
            };
            base.NSession.Save(type2);
            base.NSession.Flush();
        }

        public JsonResult ScanBySend(string o, string l, int w, string m, string c)
        {
            List<OrderType> list = base.GetList<OrderType>("OrderNo", o, "");
            StringBuilder builder = new StringBuilder();
            if (list.Count > 0)
            {
                list[0].Status = "已发货";
                list[0].ScanningOn = DateTime.Now;
                list[0].ScanningBy = base.CurrentUser.Realname;
                list[0].LogisticMode = (m != "") ? m : list[0].LogisticMode;
                list[0].Weight = w;
                list[0].TrackCode = l;
                this.SaveOrUpdate<OrderType>(list[0]);
                base.NSession.Flush();
                builder.AppendFormat("订单：{0} ，已经发货了，发货方式为{1}，重量：{2} ，追踪条码：{3}", new object[] { list[0].OrderNo, list[0].LogisticMode, list[0].Weight, list[0].TrackCode });
                IList<AccountType> list2 = base.NSession.CreateQuery("from AccountType where AccountName='" + list[0].Account + "'").List<AccountType>();
                if (list2.Count != 0)
                {
                    string token = AliUtil.RefreshToken(list2[0]);
                    AliUtil.sellerShipment(list2[0].ApiKey, list2[0].ApiSecret, token, list[0].OrderExNo, l, "CPAM", true);
                }
                return base.Json(new { IsSuccess = true, Result = builder.ToString() });
            }
            return base.Json(new { IsSuccess = false, Result = builder.ToString() });
        }

        public ActionResult ScanExport()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult ScanExport(DateTime st, DateTime et, string u, string key)
        {
            if (u != "")
            {
                u = "ScanningBy= '" + u + "' and";
            }
            if (key != "")
            {
                key = "and ScanningBy= '" + key + "' ";
            }
            List<LogisticsModeType> list = base.NSession.CreateQuery("from LogisticsModeType").List<LogisticsModeType>().ToList<LogisticsModeType>();
            string format = "select OrderNo as 'PackageNo',OrderExNo as 'OrderExNo',Weight as 'PackageWeight',ScanningBy,TrackCode as 'TrackCode',ScanningOn as 'ShippedTime',LogisticMode as 'LogisticsMode',Country,(select top 1 CCountry from Country C where C.ECountry=Orders.Country) as '中文名称',(select top 1  AreaName from [LogisticsArea] where LId = (select top 1 ParentID from LogisticsMode where LogisticsCode=Orders.LogisticMode) and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode  and CountryCode in (select ID from Country where ECountry=Orders.Country) )) as '分区' from Orders where Status in ('已发货','已完成') and {0}  ScanningOn  between '{1}' and '{2}' {3}  order by ScanningOn asc ";
            format = string.Format(format, new object[] { u, st.ToString("yyyy/MM/dd HH:mm:ss"), et.ToString("yyyy/MM/dd HH:mm:ss"), key });
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = format;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
            {
                LogisticsModeType mode = list.Find(p => p.LogisticsCode == dataRow["LogisticsMode"].ToString().Trim());
                if (mode != null)
                    dataRow["LogisticsMode"] = mode.LogisticsName.Trim();
            }
            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            System.Web.HttpContext.Current.Response.Charset = "gb2312";
            base.Response.AppendHeader("Content-Disposition", "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
            return base.File(Encoding.UTF8.GetBytes(ExcelHelper.GetExcelXml(dataSet)), "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
        }

        [HttpPost]
        public ActionResult ScanExport2(DateTime st, DateTime et, string u, string key)
        {
            string str = string.Format("select OrderNo,OrderExNo,TrackCode,TrackCode2,TId,Account,CreateOn from Orders where TrackCode2 is not null and TrackCode2 <>''  and CreateOn  between '{0}' and '{1}'", st.ToString("yyyy/MM/dd HH:mm:ss"), et.ToString("yyyy/MM/dd HH:mm:ss"));
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            System.Web.HttpContext.Current.Response.Charset = "gb2312";
            base.Response.AppendHeader("Content-Disposition", "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
            return base.File(Encoding.UTF8.GetBytes(ExcelHelper.GetExcelXml(dataSet)), "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
        }

        [HttpPost]
        public ActionResult ScanExport3(DateTime st, DateTime et, string u, string key)
        {
            string str = string.Format("select OrderNo,OrderExNo,TrackCode,TrackCode2,TId,Account,ScanningOn from Orders where Status in ('已发货','已完成') and TrackCode<> TrackCode2 and TrackCode2 is not null and ScanningOn  between '{0}' and '{1}'", st.ToString("yyyy/MM/dd HH:mm:ss"), et.ToString("yyyy/MM/dd HH:mm:ss"));
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            System.Web.HttpContext.Current.Response.Charset = "gb2312";
            base.Response.AppendHeader("Content-Disposition", "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
            return base.File(Encoding.UTF8.GetBytes(ExcelHelper.GetExcelXml(dataSet)), "attachment;filename=" + st.ToShortDateString() + "-" + et.ToShortDateString() + ".xls");
        }

        private void SetQuestionOrder(string subject, OrderType orderType, string content = "")
        {
            QuestionOrderType type = new QuestionOrderType
            {
                OId = orderType.Id,
                OrderNo = orderType.OrderNo,
                Status = 0,
                Subjest = subject
            };
            if (string.IsNullOrEmpty(content))
            {
                type.Content = orderType.CutOffMemo;
            }
            else
            {
                type.Content = content;
            }
            type.CreateBy = base.CurrentUser.Realname;
            type.CreateOn = DateTime.Now;
            type.SolveOn = DateTime.Now;
            base.NSession.Save(type);
            base.NSession.Flush();
        }

        public ActionResult SplitNoSend()
        {
            return base.View();
        }

        public JsonResult SplitNoSendList(string sort, string order, string search)
        {
            string str = " where Status<>'已发货' and MId<>0 ";
            string str2 = " order by Id desc";
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where Status<>'已发货' and MId<>0 and " + str;
                }
            }
            IList<OrderType> list = base.NSession.CreateQuery("from OrderType " + str + str2).List<OrderType>();
            for (int i = 0; i < list.Count; i++)
            {
                if (this.GetById(list[i].MId).IsSplit != 1)
                {
                    list.Remove(list[i]);
                }
            }
            return base.Json(new { total = list.Count, rows = list });
        }

        public ActionResult StopIndex()
        {
            return base.View();
        }

        /// <summary>
        /// 同步订单
        /// </summary>
        /// <param name="Platform"></param>
        /// <param name="Account"></param>
        /// <param name="st"></param>
        /// <param name="et"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Synchronous(string Platform, int[] Account, DateTime st, DateTime et)
        {

            List<ResultInfo> list = new List<ResultInfo>();
            foreach (int id in Account)
            {
                AccountType account = base.NSession.Get<AccountType>(Convert.ToInt32(id));

                switch (((PlatformEnum)Enum.Parse(typeof(PlatformEnum), Platform)))
                {
                    case PlatformEnum.DH:
                        //list.AddRange( OrderHelper.APIByB2C(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Amazon:
                        //list.AddRange(OrderHelper.APIByAmazonStock(account, NSession));
                        list.AddRange(OrderHelper.APIByAmazon(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Ebay:
                        list.AddRange(OrderHelper.APIByEbay(account, st, et, base.NSession));
                        break;

                    case PlatformEnum.Aliexpress:
                        list.AddRange(OrderHelper.APIBySMT(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Wish:
                        list.AddRange(OrderHelper.APIByWish(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Bellabuy:
                        list.AddRange(OrderHelper.APIByBellaBuy(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Lazada:
                        list.AddRange(OrderHelper.APIByLazada(account, st, et, base.NSession));
                        break;
                    case PlatformEnum.Cdiscount:
                        list.AddRange(OrderHelper.APIByCdisount(account, st, et, base.NSession));
                        break;
                    default:
                        return base.Json(new { IsSuccess = false, ErrorMsg = "该平台没有同步功能！" });
                }
            }
            if (Session != null)
            {
                base.Session["Results"] = list;
            }
            return base.Json(new { IsSuccess = true, Info = true });
        }

        public JsonResult TimeJi(DateTime st, DateTime et)
        {
            try
            {
                List<ProductType> source = base.NSession.CreateQuery("from ProductType").List<ProductType>().ToList<ProductType>();
                List<OrderPackRecordType> list2 = base.NSession.CreateQuery("from OrderPackRecordType where PackOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59'").List<OrderPackRecordType>().ToList<OrderPackRecordType>();
                using (ITransaction transaction = base.NSession.BeginTransaction())
                {
                    foreach (OrderPackRecordType type in list2)
                    {
                        double packCoefficient = 0.0;
                        List<OrderProductType> list3 = base.NSession.CreateQuery("from OrderProductType where OrderNo='" + type.OrderNo + "'").List<OrderProductType>().ToList<OrderProductType>();
                        OrderType type2 = base.NSession.Get<OrderType>(type.OId);
                        if ((list3.Count == 0) || (type2.IsSplit == 1))
                        {
                            type.PackCoefficient = 3.0;
                        }
                        else
                        {
                            using (List<OrderProductType>.Enumerator enumerator2 = list3.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    Func<ProductType, bool> predicate = null;
                                    OrderProductType product = enumerator2.Current;
                                    if (predicate == null)
                                    {
                                        predicate = p => p.SKU.ToString().ToUpper() == product.SKU.ToString().ToUpper();
                                    }
                                    List<ProductType> list4 = source.Where<ProductType>(predicate).ToList<ProductType>();
                                    if (list4.Count != 0)
                                    {
                                        if (list4[0].PackCoefficient > packCoefficient)
                                        {
                                            packCoefficient = list4[0].PackCoefficient;
                                        }
                                    }
                                    else
                                    {
                                        packCoefficient = 0.0;
                                    }
                                }
                            }
                            type.PackCoefficient = packCoefficient;
                        }
                        base.NSession.Update(type);
                    }
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }

        public ActionResult TodayIndex()
        {
            return base.View();
        }

        public JsonResult ToExcel()
        {
            string str = string.Format("select * from (\r\nselect SKU,SUM(Qty) as Qty,(select isnull(SUM(Qty),0) from WarehouseStock where SKU=OP.SKU ) as NowQty,(select count(Id) from SKUCode where SKU=OP.SKU and IsOut=0) as unPeiQty,COUNT(O.Id) as'OrderQty' from Orders O left join OrderProducts OP On O.Id=OP.OId where O.IsOutOfStock=1 and OP.IsQue=1 and O.Status<>'作废订单' group by SKU\r\n) as tbl  where NowQty>0", new object[0]);
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(dataSet);
            return base.Json(new { IsSuccess = true });
        }

        private void TrackCodeUpLoad(object oo)
        {
            try
            {
                ISession nSession = NhbHelper.OpenSession();
                OrderType type = oo as OrderType;
                if (type != null)
                {
                    //type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, nSession, 0M));
                    //// 计算订单财务数据
                    //OrderHelper.ReckonFinance(type, base.NSession);
                    nSession.SaveOrUpdate(type);
                    nSession.Flush();
                    //OrderHelper.UpdateAmount(type, nSession);
                    this.UploadTrackCode(type, nSession);
                }
                nSession.Close();
            }
            catch
            {
            }
        }
        private void TrackCodeUpLoad1(object oo)
        {
            try
            {
                ISession nSession = NhbHelper.OpenSession();
                OrderType type = oo as OrderType;
                if (type != null)
                {
                    //type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, nSession, 0M));
                    //// 计算订单财务数据
                    //OrderHelper.ReckonFinance(type, base.NSession);
                    //nSession.SaveOrUpdate(type);
                    //nSession.Flush();
                    //OrderHelper.UpdateAmount(type, nSession);
                    this.UploadTrackCode1(type, nSession);
                }
                nSession.Close();
            }
            catch
            {
            }
        }

        public ActionResult UnHandIndex()
        {
            return base.View();
        }

        public JsonResult UnHandleList(int page, int rows, string sort, string order, string search, string v)
        {
            return this.List(page, rows, sort, order, search, v, 1);
        }

        /// <summary>
        ///  上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        private void UploadTrackCode(OrderType o, ISession nSession)
        {
            //先判断该订单是否是合并订单,是合并订单的话则上传多个子订单的跟踪号
            if (o.IsMerger == 1)
            {

                List<OrderType> orderlist = NSession.CreateQuery(" from OrderType where  MId=:p1 and Id <>:p1").SetInt32("p1", o.Id).List<OrderType>().ToList();
                if (orderlist != null && orderlist.Count > 0)
                {
                    foreach (OrderType or in orderlist)
                    {
                        or.LogisticMode = o.LogisticMode;
                        or.TrackCode = o.TrackCode;
                        OrderHelper.UploadTrackCode(or, nSession);
                    }
                }

            }
            else
            {
                OrderHelper.UploadTrackCode(o, nSession);

            }

            //    // ebay 暂时取消上传跟踪码
            //    //// Ebay
            //    //if (((o.Platform == PlatformEnum.Ebay.ToString()) && (o.TrackCode != null)) && !o.TrackCode.StartsWith("LK"))
            //    //{
            //    //    EBayUtil.EbayUploadTrackCode(o.Account, o);
            //    //}
            //    // Wish
            //    if (o.Platform == PlatformEnum.Wish.ToString())
            //    {
            //        IList<logisticsSetupType> list = nSession.CreateQuery("from  logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='Wish'").List<logisticsSetupType>();
            //        if (list.Count > 0)
            //        {
            //            IList<AccountType> list2 = nSession.CreateQuery("from AccountType where AccountName='" + o.Account + "'").SetMaxResults(1).List<AccountType>();
            //            if (list2.Count > 0)
            //            {
            //                list2[0].ApiTokenInfo = WishUtil.RefreshToken(list2[0]);
            //                WishUtil.UploadTrackNo(list2[0].ApiTokenInfo, o.OrderExNo, list[0].SetupName, o.TrackCode);
            //            }
            //        }
            //    }
            //    // Aliexpress
            //    if (o.Platform == PlatformEnum.Aliexpress.ToString())
            //    {
            //        string serviceName = "";
            //        IList<logisticsSetupType> list = nSession.CreateQuery("from  logisticsSetupType where LId in (select ParentID from LogisticsModeType where LogisticsCode='" + o.LogisticMode + "') and Platform='SMT'").List<logisticsSetupType>();
            //        if (list.Count > 0)
            //        {
            //            serviceName = list[0].SetupName;
            //        }
            //        else
            //        {
            //            serviceName = "CPAM";
            //        }
            //        IList<AccountType> list2 = nSession.CreateQuery("from AccountType where AccountName='" + o.Account + "'").SetMaxResults(1).List<AccountType>();
            //        if (list2.Count > 0)
            //        {
            //            AccountType account = list2[0];
            //            if (string.IsNullOrEmpty(account.ApiTokenInfo))
            //            {
            //                account.ApiTokenInfo = AliUtil.RefreshToken(account);
            //                nSession.Save(account);
            //                nSession.Flush();
            //            }
            //            if (AliUtil.sellerShipment(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, o.OrderExNo.Trim(), o.TrackCode, serviceName, true).IndexOf("Request need user authorized") != -1)
            //            {
            //                account.ApiTokenInfo = AliUtil.RefreshToken(account);
            //                nSession.Save(account);
            //                nSession.Flush();
            //                AliUtil.sellerShipment(account.ApiKey, account.ApiSecret, account.ApiTokenInfo, o.OrderExNo, o.TrackCode, serviceName, true);
            //            }
            //        }
            //    }
        }
        //扫描发货义乌新合并订单上传单号测试2016-11-25---【扫描发货合并订单卡住】
        private void UploadTrackCode1(OrderType o, ISession nSession)
        {
            ////先判断该订单是否是合并订单,是合并订单的话则上传多个子订单的跟踪号
            //if (o.IsMerger == 1)
            //{

            //    List<OrderType> orderlist = NSession.CreateQuery(" from OrderType where  MId=:p1 and Id <>:p1").SetInt32("p1", o.Id).List<OrderType>().ToList();
            //    if (orderlist != null && orderlist.Count > 0)
            //    {
            //        foreach (OrderType or in orderlist)
            //        {
            //            or.LogisticMode = o.LogisticMode;
            //            or.TrackCode = o.TrackCode;
            //            OrderHelper.UploadTrackCode(or, nSession);
            //        }
            //    }

            //}
            //else
            //{
            OrderHelper.UploadTrackCode(o, nSession);

            //}
        }
        /// <summary>
        ///  wish上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        private void UploadTrackCode2(OrderType o, string l ,ISession nSession)
        {
           
          //   ebay 暂时取消上传跟踪码
            // Ebay
            if (((o.Platform == PlatformEnum.Ebay.ToString()) && (o.TrackCode != null)) && !o.TrackCode.StartsWith("LK"))
            {
                EBayUtil.EbayUploadTrackCode1(o.Account, o,l);
            }
           
        }

        public string Zu1(OrderProductType item)
        {
            return string.Concat(new object[] { 
                " ExSKU:", item.ExSKU, " 名称:", item.Title, " SKU:", item.SKU, " 数量:", item.Qty, " 规格:", item.Standard, " 价格：", item.Price, " 网址：", item.Url, " 描述：", item.Remark, 
                "<br>"
             });
        }
        /// <summary>
        /// 判断该订单列是否是延迟15天以上订单，能否发货
        /// </summary>
        /// <param name="OrderNoList"></param>
        /// <returns></returns>
        public string AllowDelivery(string OrderNoList)
        {
            //int allowday = Config.AllowDeliveryDay;  // 数据字典获取允许发货的天数
            List<DataDictionaryDetailType> listdate = GetList<DataDictionaryDetailType>("DicCode", "AllowDeliveryDay", "FullName='day1'");
            string day1 = listdate[0].DicValue;
            int allowday = Convert.ToInt32(day1);
            //string[] orderno = OrderNoList.Split(',');
            StringBuilder newlist = new StringBuilder();
            //for (int i = 0; i < orderno.Length; i++)
            //{
            //    List<OrderType> list = NSession.CreateQuery("from OrderType where OrderNo='" + orderno[i] + "' and DATEDIFF(day,CreateOn,GETDATE())<=" + allowday + " or DATEDIFF(day,CreateOn,GETDATE())>" + allowday + " and AllowDelivery=1").List<OrderType>().ToList();
            //    if (list.Count == 1)
            //    {
            //        if (i == orderno.Length - 1)
            //        {
            //            newlist.Append(orderno[i]);
            //        }
            //        else
            //        {
            //            newlist.Append(orderno[i] + ",");
            //        }
            //    }
            //}
            List<OrderType> list = NSession.CreateQuery("from OrderType where OrderNo in ('" + string.Join("','", OrderNoList.Split(',')) + "')").List<OrderType>().ToList();
            foreach (OrderType orderType in list)
            {
                // 根据审批状态与限定天数控制订单打印
                TimeSpan span = DateTime.Now - orderType.CreateOn;
                int datediff = span.Days;
                if (datediff > Convert.ToInt32(allowday) && orderType.AllowDelivery == 1)
                {
                    // 大于15天同时已审批允许打印
                    orderType.IsAllowDelivery = true;
                }
                else if (datediff <= Convert.ToInt32(allowday))
                {
                    // 小于等于15天允许打印
                    orderType.IsAllowDelivery = true;
                }
                else
                {
                    // 禁止打印
                    orderType.IsAllowDelivery = false;
                    newlist.Append(orderType.OrderNo + ",");
                }
            }

            if (newlist.ToString().Length > 0)
            {
                return newlist.ToString().Substring(0, newlist.Length - 1);
            }
            else
            {
                return "";
            }

        }
        /// <summary>
        /// 批量审批超时未发货订单
        /// </summary>
        /// <param name="OrderList"></param>
        /// <returns></returns>
        public bool AuditOrders(string OrderList)
        {
            OrderList = OrderList.Replace(",", "','");
            string sql = "update OrderType set AllowDelivery=1 where OrderNo in ('" + OrderList + "')";
            IQuery Query = NSession.CreateQuery(sql);
            return Query.ExecuteUpdate() > 0;
        }

        /// <summary>
        /// 获得八达通跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public JsonResult GetBdtTrackCode(string ids, int t)
        {
            //IdentifyReturnType t = SHMXUtil.Identify();
            //string OrderItemIds = ""; // 获取OrderItemsId
            //string OrderItemIds1 = ""; // 获取OrderItemsId 第一个
            //   string category = "";
            string productname = "";
            string pinfo = "";
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            foreach (OrderType orderType in orders)
            {
                pinfo = "";
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                if (orderType.Account.IndexOf("yw") != -1)//义乌面单增加库位
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();

                        pinfo += orderProductType.SKU + "(" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")," + orderProductType.Qty + ";";
                        foreach (ProductType type in list)
                        {
                            //   category = type.Category;
                            productname = type.ProductName;
                        }
                    }
                }
                else
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();
                        pinfo += orderProductType.SKU + "," + orderProductType.Qty + ";";
                        foreach (ProductType type in list)
                        {
                            //   category = type.Category;
                            productname = type.ProductName;
                        }
                    }

                }


                returnObject[] o = BatUtil.GetBdtTrackCode(orderType, productname, t, pinfo);
                if (o != null)
                {
                    if (o[0].success == true)
                    {
                        orderType.TrackCode = o[0].trackingNo;
                    }
                    else
                    {
                        LoggerUtil.GetOrderRecord(orderType, "8dt单号获取", "原单号：" + orderType.TrackCode + "替换为" + o[0].errorMsg, base.CurrentUser, base.NSession);
                        orderType.TrackCode = o[0].errorMsg;
                    }
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 恢复八达通跟踪码
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public JsonResult GetBdtTrackCode1(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                parcel[] o = BatUtil.queryParcelByRefNoService(orderType, t);
                if (o != null)
                {
                    if (o[0].aptrackingNumber.Length > 0)
                    {
                        orderType.TrackCode = o[0].aptrackingNumber;
                    }
                }
                else
                {
                    orderType.TrackCode = "";
                }
                NSession.Update(orderType);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true });
        }
        /// <summary>
        /// 八达通PDF打印
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult GetBdtPDF(string ids, int t)
        {
            string tradenos = "";
            //八达通打单标记打印符号
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            foreach (OrderType orderType in orders)
            {
                tradenos += orderType.TrackCode + ",";
            }
            string url = BatUtil.GetBatpdfUrl(tradenos.TrimEnd(','), t);
            foreach (OrderType orderType in orders)
            {
                orderType.IsPrint = 1;
                LoggerUtil.GetOrderRecord(orderType, "订单打印", "打印八达通", base.CurrentUser, base.NSession);
                NSession.Update(orderType);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true, f = url });

        }

        public JsonResult ToGCorder(string ids,int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string o = "";
                if (t == 1)
                {
                    o = GucangUtil.CreateOrder(orderType,"");
                }
                else if (t == 3)
                {
                    o = GucangUtil.CreateOrder(orderType, "nb");
                }
                else
                {
                    o = SuYouUtil.CreateOrder(orderType);
                }
                List<GCReturnType> oo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GCReturnType>>("[" + o + "]");
                if (oo != null)
                {
                    if (oo[0].ask == "Success")
                    {
                        orderType.TrackCode = oo[0].ask;  // TrackingCode
                    }
                    else
                    {
                        orderType.TrackCode = oo[0].message;  // TrackingCode
                    }
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult GetOrderByRefCode(string ids,int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();

            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                string o = "";
                if (t == 1)
                {
                    o = GucangUtil.GetOrderByRefCode(orderType,"");
                }
                else if (t == 3)
                {
                    o = GucangUtil.GetOrderByRefCode(orderType, "nb");
                }
                else
                {
                    o = SuYouUtil.GetOrderByRefCode(orderType);
                }
                o = o.Replace("\"data\":{", "\"data\":[{");
                o = o.Replace("\"Error\":{", "\"Error\":[{");
                o = o.Replace("\"fee_details\":{", "\"fee_details\":[{");
                o = o.Replace("\"},\"it", "\"}],\"it");
                //o = o.Replace("\"orderBoxInfo\":[", "\"orderBoxInfo\":[{");
                o = o.Replace("},\"orderBoxInfo\":", "}],\"orderBoxInfo\":");
                //o = o.Replace("\"orderBoxInfo\":[", "\"orderBoxInfo\":\"");
                //o = o.Replace("],\"items\":", "}],\"items\":");
                o = o.Replace("\"}]}}", "\"}]}]}");
                o = "[" + o + "]";
                List<GCReturnType> oo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GCReturnType>>(o);
                if (oo != null)
                {
                    if (oo[0].ask == "Success" && oo[0].data.Count > 0)
                    {
                        foreach (DataList result in oo[0].data)
                        {
                            if (result.tracking_no.Length > 0)
                            {
                                orderType.TrackCode = result.tracking_no;
                            }
                        }
                    }
                    else
                    {
                        orderType.TrackCode = "尚无追踪号";
                    }
                    NSession.Update(orderType);
                    NSession.Flush();
                }
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 获取宁波挂号追踪号
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetNBGH(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            string Msg = "成功";
            foreach (OrderType orderType in orders)
            {
                NBGHUtil n = new NBGHUtil();
                if (orderType.LogisticMode == "义乌邮局挂号")
                {
                    t = 2;
                }
                else if (orderType.LogisticMode.Contains("义乌增强小包"))
                {
                    t = 3;
                }
                else if (orderType.LogisticMode.Contains("义乌邮局平邮"))
                {
                    t = 4;
                }
                NBGHReturnType oo = n.GetreturnMsg(orderType, t);
                if (oo != null)
                {
                    if (oo.return_success == "true" && oo.barCodeList.Count > 0)
                    {
                        foreach (barCodeList result in oo.barCodeList)
                        {
                            if (result.bar_code.Length > 0)
                            {
                                orderType.TrackCode = result.bar_code;      
                            } 
                        }
                    }
                    NSession.Update(orderType);
                    NSession.Flush();
                }
                if (orderType.Account.IndexOf("yw") != -1)
                {
                    try
                    {
                        orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                        orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                        string o = NBGHUtil.CreateOrder(orderType, t);
                        if (o != null)
                        {
                            if (o == "true")
                            {

                                Msg += orderType.OrderNo + "上传成功.";
                            }
                            else
                            {
                                // orderType.TrackCode = o;
                                if (o == "falseB08")
                                {
                                    Msg += orderType.OrderNo + "已经上传，不能再次上传";
                                }
                                else
                                {
                                    Msg += orderType.OrderNo + "上传失败." + o;
                                }
                            }

                        }
                    }
                    catch (Exception)
                    { }
                }
            }
            return Json(new { IsSuccess = true, Msg = Msg });
        }
        /// <summary>
        /// 上传订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult ToNBGHorder(string ids, int t)
        {
            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            string Msg = "";
            foreach (OrderType orderType in orders)
            {
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products = NSession.CreateQuery("from OrderProductType where OId ='" + orderType.Id + "'").List<OrderProductType>().ToList();
                if (orderType.LogisticMode == "义乌邮局挂号")
                {
                    t = 2;
                }
                else if (orderType.LogisticMode.Contains("义乌增强小包"))
                {
                    t = 3;
                }
                string o = NBGHUtil.CreateOrder(orderType, t);

                if (o != null)
                {
                    if (o == "true")
                    {

                        Msg += orderType.OrderNo + "上传成功.";
                    }
                    else
                    {
                        // orderType.TrackCode = o;
                        if (o == "falseB08")
                        {
                            Msg += orderType.OrderNo + "已经上传，不能再次上传";
                        }
                        else
                        {
                            Msg += orderType.OrderNo + "上传失败.";
                        }
                        NSession.Update(orderType);
                        NSession.Flush();
                    }

                }
            }
            return Json(new { IsSuccess = true, Msg = Msg });
        }

        public ActionResult GetLianJieTrackCode(string ids, string t)
        {

            List<OrderType> orders = NSession.CreateQuery("from OrderType where Id in(" + ids + ")").List<OrderType>().ToList();
            string chukouyiOrder = "";
            string category = "";
            string productname = "";//商品名称
            string ccountry = "";
            string pinfo = "";
            string Msg = "";
            foreach (OrderType orderType in orders)
            {
                pinfo = "";
                orderType.AddressInfo = NSession.Get<OrderAddressType>(orderType.AddressId);
                orderType.Products =
                    NSession.CreateQuery("from OrderProductType where OId=" + orderType.Id).List<OrderProductType>().
                        ToList();
                //获取某一商品类别
                if (orderType.Account.IndexOf("yw") != -1)//义乌面单增加库位
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();

                        pinfo += "[" + orderProductType.SKU + "(" + Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select  Location from Products P where P.SKU='" + orderProductType.SKU + "'")).UniqueResult()) + ")," + orderProductType.Qty + "]";
                        foreach (ProductType type in list)
                        {
                            category = type.Category;
                            productname = type.ProductName;
                        }
                    }
                }
                else
                {
                    foreach (var orderProductType in orderType.Products)
                    {
                        IList<ProductType> list = base.NSession.CreateQuery("from ProductType where SKU='" + orderProductType.SKU + "'").List<ProductType>();
                        pinfo += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                        foreach (ProductType type in list)
                        {
                            category = type.Category;
                            productname = type.ProductName;
                        }
                    }

                }

                ccountry = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select CCountry from Country where ECountry='" + orderType.AddressInfo.Country + "'")).UniqueResult());

                LianJieResultRootObject rootObject = LianJieUtil.GetTracoCode(orderType, t, category, productname, ccountry, pinfo, NSession);
                if (rootObject != null)
                {
                    if (rootObject.ErrList.Count > 0)
                    {
                        string s = rootObject.ErrList[0].cNo;
                        if (s.Length > 0)
                        {
                            if (orderType.IsPrint == 0)
                            {
                                orderType.TrackCode = s;
                                Msg += orderType.OrderNo + "成功.";
                            }
                            else
                            {
                                Msg += orderType.OrderNo + "面单已打印.";
                            }

                        }
                        else
                        {
                            Msg += orderType.OrderNo + " " + rootObject.ErrList[0].cMess + ".";
                        }
                        NSession.Update(orderType);
                        NSession.Flush();
                         
                    }
                }
                //LianJieResultRootObject rootObject = LianJieUtil.DoGetNo(orderType, t, category, productname, ccountry, pinfo, NSession);

                //if (rootObject1 != null)
                //{
                //    if (Convert.ToInt32(rootObject1.ReturnValue) >0)
                //    {

                //        Msg += orderType.OrderNo + "上传成功.";
                //    }
                //    else
                //    {
                //       Msg += orderType.OrderNo + "上传失败.";
                //    }
                //}
            }
            return Json(new { IsSuccess = true, Msg = Msg });
        }

    }
}