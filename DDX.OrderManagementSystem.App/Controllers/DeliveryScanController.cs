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
using System.Threading;
using DDX.OrderManagementSystem.App.Common;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class DeliveryScanController : BaseController
    {
        public ViewResult DeliveryScan()
        {
            return View();
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="TrackCode"></param>
        /// <returns></returns>
        public JsonResult GetOrderInfo(string TrackCode)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                string str2;
                OrderType type = list[0];
                //if (((type.Status != OrderStatusEnum.待发货.ToString()) && (type.Status != OrderStatusEnum.待包装.ToString())) && !(type.Status == OrderStatusEnum.已处理.ToString()))
                if (type.Status != OrderStatusEnum.待发货.ToString())
                {
                    return base.Json(new { IsSuccess = false, Result = " 系统无法进行发货扫描！ 当前状态为：" + type.Status + "" });
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

                    // 判断订单类型:单品单件，单品多件，多品多件
                    string strType = "";
                    if (type.Products.Count == 1)
                    {
                        if (type.Products[0].Qty == 1)
                        {
                            strType = "单品单件";
                        }
                        else if (type.Products[0].Qty > 1)
                        {
                            strType = "单品多件";
                        }
                        else
                        {
                            strType = "未知类型";
                        }
                    }
                    else if (type.Products.Count > 1)
                    {
                        strType = "多品多件";
                    }
                    else
                    {
                        strType = "未知类型";
                    }

                    return base.Json(new { IsSuccess = true, Result = type, Code = codeLength, ResultType = strType });
                }
                str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                return base.Json(new { IsSuccess = false, Result = str2 });
            }
            return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        }

        /// <summary>
        /// 发货扫描
        /// </summary>
        /// <param name="TrackCode">跟踪码</param>
        /// <param name="Weight">包裹重量</param>
        /// <param name="WarehouseId">仓库编号</param>
        /// <param name="LogisticMode">物流方式</param>
        /// <returns></returns>
        public JsonResult Scan(string TrackCode, int Weight, int WarehouseId, string LogisticMode)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode ='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];

                if (type.Status == OrderStatusEnum.已处理.ToString() || type.Status==OrderStatusEnum.待包装.ToString())
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
                    base.NSession.CreateQuery("update SKUCodeType set IsSend=1,isout=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
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
                    return base.Json(new { IsSuccess = true, Result = "订单：" + type.OrderNo + "发货扫描结束，等待扫描跟踪码...", OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 系统无法操作！当前状态为：" + type.Status + "！" });
            }
            return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });
        }

        private void TrackCodeUpLoad(object oo)
        {
            try
            {
                ISession nSession = NhbHelper.OpenSession();
                OrderType type = oo as OrderType;
                if (type != null)
                {
                    nSession.SaveOrUpdate(type);
                    nSession.Flush();
                    this.UploadTrackCode(type, nSession);
                }
                nSession.Close();
            }
            catch
            {
            }
        }
        /// <summary>
        ///  上传跟踪码
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nSession"></param>
        private void UploadTrackCode(OrderType o, ISession nSession)
        {
            // ebay 暂时取消上传跟踪码
            //// Ebay
            //if (((o.Platform == PlatformEnum.Ebay.ToString()) && (o.TrackCode != null)) && !o.TrackCode.StartsWith("LK"))
            //{
            //    EBayUtil.EbayUploadTrackCode(o.Account, o);
            //}
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
        ///// <summary>
        ///// 订单发货(新)
        ///// </summary>
        ///// <param name="o">系统编号</param>
        ///// <param name="t">跟踪码</param>
        ///// <param name="s">仓库</param>
        ///// <param name="l">发货方式</param>
        ///// <param name="w">重量</param>
        ///// <param name="skuCode">商品标识码</param>
        ///// <returns></returns>
        //public JsonResult OutStockBySendVali(string o, string t, int s, string l, double w, string skuCode)
        //{
        //    if (w == 0)
        //    {
        //        return base.Json(new { IsSuccess = false, Result = "重量不能为0" });
        //    }
        //    List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + o + "' Or TrackCode ='" + o + "'").List<OrderType>().ToList<OrderType>();
        //    if (list.Count > 0)
        //    {
        //        OrderType type = list[0];
        //        //object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where Status='已发货' and TrackCode='" + t + "'").UniqueResult();
        //        object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where TrackCode='" + t + "'").UniqueResult();
        //        if (Utilities.ToInt(obj2) > 1)
        //        {
        //            LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + t + "重复，之前已经有订单使用！", base.CurrentUser, base.NSession);
        //            return base.Json(new { IsSuccess = false, Result = "运单号" + t + "重复，之前已经有订单使用！" });
        //        }
        //        if (type.LogisticMode == "线上发货")
        //            if (t != type.TrackCode)
        //            {
        //                return base.Json(new { IsSuccess = false, Result = "运单号" + t + "和系统中的运单号" + type.TrackCode + "不一致" });
        //            }
        //        if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
        //        {
        //            string ttt = type.TrackCode;
        //            //type.TrackCode = t;
        //            // 判断是否手动添加跟踪码,当手动添加跟踪码时跟踪码记录
        //            if (type.TrackCode == "已用完")
        //            {
        //                type.TrackCode = t;
        //            }
        //            type.Weight = Convert.ToInt32(w);
        //            if (l != "")
        //            {
        //                type.LogisticMode = l;
        //            }

        //            type.ScanningOn = DateTime.Now;
        //            type.Status = OrderStatusEnum.已发货.ToString();
        //            type.ScanningBy = base.CurrentUser.Realname;
        //            type.IsCanSplit = 0;
        //            type.IsOutOfStock = 0;
        //            base.NSession.Update(type);
        //            base.NSession.Flush();
        //            ////再次update,避免某些订单重量为0，更新状态和扫描时间，已发货状态一定是已打印过的
        //            //base.NSession.CreateSQLQuery("update Orders set Status='已发货',Weight='" + Convert.ToInt32(w) + "',IsPrint=IsPrint+1，ScanningOn='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
        //            base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
        //            IList<OrderProductType> list2 = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>();
        //            foreach (OrderProductType type2 in list2)
        //            {
        //                Utilities.StockOut(s, type2.SKU, type2.Qty, "扫描出库", base.CurrentUser.Realname, "", type.OrderNo, base.NSession);
        //            }
        //            //更新重量
        //            if (list2.Count == 1 && list2[0].Qty == 1)
        //            {
        //                List<ProductType> product =
        //                    NSession.CreateQuery("from ProductType where SKU='" + list2[0].SKU + "'").List
        //                        <ProductType>().ToList();
        //                if (product.Count > 0)
        //                {
        //                    bool iscon = false;
        //                    if (product[0].Weight != 0)
        //                    {
        //                        if (product[0].Weight <= 200)
        //                        {
        //                            if ((product[0].Weight * 1.1) < Convert.ToDouble(w) ||
        //                                (product[0].Weight * 0.9) > Convert.ToDouble(w))
        //                                iscon = true;
        //                        }
        //                        else
        //                        {
        //                            if ((product[0].Weight * 1.05) < Convert.ToDouble(w) ||
        //                                (product[0].Weight * 0.95) > Convert.ToDouble(w))
        //                                iscon = true;
        //                        }
        //                        if (iscon)
        //                        {
        //                            product[0].Weight = Convert.ToInt32(w);
        //                            NSession.Update(product[0]);
        //                            NSession.Flush();
        //                            LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + w, CurrentUser, NSession);

        //                        }
        //                    }

        //                }
        //            }
        //            base.NSession.CreateQuery("update SKUCodeType set IsSend=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
        //            LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + type.TrackCode + " 原单号:" + ttt, base.CurrentUser, base.NSession);
        //            if (skuCode != "")
        //            {
        //                base.NSession.CreateQuery("update SKUCodeType set IsOut=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + skuCode.Replace(",", "','") + "')").ExecuteUpdate();
        //            }
        //            type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
        //            string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", l, "  重量：", w });
        //            try
        //            {
        //                new Thread(new ParameterizedThreadStart(this.TrackCodeUpLoad)) { IsBackground = true }.Start(type);
        //            }
        //            catch (Exception)
        //            {
        //            }
        //            finally
        //            {
        //                // 计算订单财务数据
        //                OrderHelper.ReckonFinance(type, base.NSession);
        //            }
        //            base.NSession.Update(type);
        //            base.NSession.Flush();
        //            return base.Json(new { IsSuccess = true, Result = str, OId = type.Id });
        //        }
        //        return base.Json(new { IsSuccess = false, Result = " 无法出库！ 当前状态为：" + type.Status + "，需要订单状态为“待发货”方可扫描！" });
        //    }
        //    return base.Json(new { IsSuccess = false, Result = "找不到该订单" });
        //}
    }
}

