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
using Newtonsoft.Json;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class PackingScanController : BaseController
    {
        public ViewResult PackingScanV1()
        {
            return View();
        }
        public ViewResult WarehouseTurnoverDwon()
        {
            return View();
        }
        public ViewResult WarehouseTurnoverOn()
        {
            return View();
        }
        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OrderRecordType GetById(int Id)
        {
            OrderRecordType obj = NSession.Get<OrderRecordType>(Id);
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
            OrderRecordType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(OrderRecordType obj)
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
                if (type.Status != OrderStatusEnum.已处理.ToString() && type.Status != OrderStatusEnum.待包装.ToString())
                {
                    return base.Json(new { IsSuccess = false, Result = " 系统无法进行包装扫描！ 当前状态为：" + type.Status + "" });
                }
                if (type.IsOutOfStock == 1)
                {
                    return base.Json(new { IsSuccess = false, Result = " 系统无法进行包装扫描！ 当前订单为缺货状态" });
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
                    //str2 = "订单:" + type.OrderNo + ",运单号:" + type.TrackCode + " 当前状态：待发货，可以发货。<br>发货方式：<s id='logisticsMode'>" + type.LogisticMode + "</s>";

                    type.Products = base.NSession.CreateQuery("from OrderProductType where OId=" + type.Id).List<OrderProductType>().ToList<OrderProductType>();

                    //string str3 = "";
                    //foreach (OrderProductType type2 in list3)
                    //{
                    //    IList<ProductType> list4 = base.NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", type2.SKU.Trim()).SetMaxResults(1).List<ProductType>();
                    //    if ((list4.Count > 0) && ((list4[0].ProductAttribute != "普货") && (list4[0].ProductAttribute != "电子")))
                    //    {
                    //        string str4 = str3;
                    //        str3 = str4 + "   " + list4[0].SKU + ":" + list4[0].ProductAttribute;
                    //    }
                    //}
                    //if (str3.Length > 0)
                    //{
                    //    str2 = "<div><h3>订单中包含：" + str3 + " 的产品</h3></div>" + str2;
                    //}
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
        /// 获取sku code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult GetSKUByCode(string code)
        {
            IList<SKUCodeType> list =
                 NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", code).SetMaxResults(1).List<SKUCodeType>();
            if (list.Count > 0)
            {
                SKUCodeType sku = list[0];
                if (sku.IsOut == 0 && sku.IsScan == 0)
                {
                    return Json(new { IsSuccess = true, Result = sku.SKU.Trim() });
                }
                else
                {
                    return Json(new { IsSuccess = false, Result = "商品已出库或已配货！系统无法操作！" });
                }
            }
            return Json(new { IsSuccess = false, Result = "没有找到该商品！" });

        }

        /// <summary>
        /// 包装扫描待发货
        /// </summary>
        /// <param name="TrackCode">跟踪码</param>
        /// <param name="SkuCodeList">商品唯一标识码</param>
        /// <returns></returns>
        public JsonResult PackingScan(string TrackCode, string SkuCodeList, string Type)
        {
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode ='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where TrackCode='" + TrackCode + "'").UniqueResult();
                if (Utilities.ToInt(obj2) > 1)
                {
                    LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + TrackCode + "重复，已有订单使用！", base.CurrentUser, base.NSession);
                    return base.Json(new { IsSuccess = false, Result = "运单号" + TrackCode + "重复，已有订单使用！" });
                }
                if (((type.Status == OrderStatusEnum.待发货.ToString()) || (type.Status == OrderStatusEnum.待包装.ToString())) || (type.Status == OrderStatusEnum.已处理.ToString()))
                {
                    //string ttt = type.TrackCode;
                    ////type.TrackCode = t;
                    //// 判断是否手动添加跟踪码,当手动添加跟踪码时跟踪码记录
                    //if (type.TrackCode == "已用完")
                    //{
                    //    type.TrackCode = TrackCode;
                    //}
                    ////type.Weight = Convert.ToInt32(w);

                    type.ScanningOn = DateTime.Now;
                    type.Status = OrderStatusEnum.待发货.ToString();
                    type.ScanningBy = base.CurrentUser.Realname;
                    type.IsCanSplit = 0;
                    type.IsOutOfStock = 0;
                    base.NSession.Update(type);
                    base.NSession.Flush();

                    // 设置订单商品非缺货状态
                    base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                    // 订单商品出库
                    if (SkuCodeList != "")
                    {
                        // 标记包装扫描
                        base.NSession.CreateQuery("update SKUCodeType set IsScan=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + SkuCodeList.Replace(",", "','") + "')").ExecuteUpdate();
                        LoggerUtil.GetOrderRecord(type, "订单包装扫描！", "包装扫描完成！", base.CurrentUser, base.NSession);
                    }
                    type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));

                    //*包装计件*//
                    PackagingScanLogType logtype = new PackagingScanLogType();
                    logtype.OperationOn = DateTime.Now;
                    logtype.PackageType = Type;
                    logtype.OrderNo = type.OrderNo;
                    logtype.Operator = base.CurrentUser.Realname;
                    base.NSession.SaveOrUpdate(logtype);
                    base.NSession.Flush();
                    //*包装计件*//
                    return base.Json(new { IsSuccess = true, Result = "订单：" + type.OrderNo + "包装扫描结束，等待再次扫描跟踪码...", OId = type.Id });
                }
                return base.Json(new { IsSuccess = false, Result = " 系统无法操作！当前状态为：" + type.Status + "！" });
            }
            return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });
        }
        /// <summary>
        /// PDA包装扫描时获取订单信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult PDAgetOrderInfoBySKU(string code)
        {
            IList<SKUCodeType> skulist =
        NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", code).SetMaxResults(1).List<SKUCodeType>();
            string CurrentSKU = "";
            if (skulist.Count > 0)
            {
                SKUCodeType sku = skulist[0];
                CurrentSKU = sku.SKU;
                //if (sku.IsOut != 0 || sku.IsScan != 0)
                if (sku.IsOut != 0 || sku.IsScan == 0)
                {
                    return Json(new { IsSuccess = false, Result = "商品已出库或已配货！系统无法操作！" });

                }

            }
            //筛选出打印过的订单,避免出现code用完的情况出现
            object strorderno =
                 NSession.CreateSQLQuery("select Orders.OrderNo from Orders ,skucode where orders.OrderNo=SKUCode.OrderNo and skucode.Code=:p and isprint>=1").SetString("p", code).UniqueResult();
            if (strorderno == null)
            {
                string str = "请输入有效商品且商品对应的订单面单已打印！";
                return base.Json(new { IsSuccess = false, Result = str });
            }
            List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo='" + strorderno.ToString() + "'  ").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];
                string str2;
                if (type.Status != OrderStatusEnum.已处理.ToString() && type.Status != OrderStatusEnum.待包装.ToString())
                {
                    return base.Json(new { IsSuccess = false, Result = " 系统无法进行包装扫描！ 当前状态为：" + type.Status + "" });


                }
                //if (type.IsOutOfStock == 1)
                //{
                //    return base.Json(new { IsSuccess = false, Result = " 系统无法进行包装扫描！ 当前订单为缺货状态" });
                //}
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

                    return base.Json(new { IsSuccess = true, Result = type, Code = codeLength, ResultType = strType, SKU = CurrentSKU });

                }
                else
                {
                    str2 = "订单:" + type.OrderNo + ", 无法扫描，请拦截此包裹，原因：" + type.CutOffMemo;
                    return base.Json(new { IsSuccess = false, Result = str2 });
                }
            }
            else
            {
                return base.Json(new { IsSuccess = false, Result = "找不到该商品" });

            }
        }
        public ActionResult IsValidateSKU(string code)
        {
            IList<SKUCodeType> list =
                 NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", code).SetMaxResults(1).List<SKUCodeType>();
            if (list.Count > 0)
            {
                SKUCodeType sku = list[0];
                //if (sku.IsOut == 0 && sku.IsScan == 0)
                if (sku.IsOut == 0 && sku.IsScan == 1)
                {
                    return Json(new { IsSuccess = true, Result = sku.SKU.Trim() });
                }
                else
                {
                    return Json(new { IsSuccess = false, Result = "商品已出库或已配货！系统无法操作！" });
                }
            }
            return Json(new { IsSuccess = false, Result = "没有找到该商品！" });

        }


        public JsonResult SetPrintData2(string strids)
        {



            // 1、12小时内打印仓库入库人名 2、12小时外打印库位
            string format = "select [dbo].[F_GetSKU2](O.OrderNo) as '物品SKU2',[dbo].[F_GetProducts](O.OrderNo) as '产品名','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=40 then (case when Amount<=5 then Amount else 5 end) else 8 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo ";

            format = string.Format(format, strids.Replace(",", "','"));
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
                    base.NSession.CreateQuery("update OrderType set IsPrint=IsPrint+1 where  IsAudit=1 and  OrderNo IN('" + strids.Replace(",", "','") + "') ").ExecuteUpdate();
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
            object objtempid = NSession.CreateSQLQuery("select distinct p.id from  PrintTemplate p ,orders o,LogisticsMode l where p.Code=l.LogisticsCode+'10*10' and o.LogisticMode=l.LogisticsCode and o.OrderNo=:p").SetString("p", strids).UniqueResult();
            string tempid = string.Empty;
            if (objtempid != null)
            {
                tempid = objtempid.ToString();
            }
            return base.Json(new { IsSuccess = true, Result = type.Id, TmpID = tempid });

        }
        /// <summary>
        /// 包装扫描 V2 后打单模块
        /// </summary>
        /// <param name="TrackCode"></param>
        /// <param name="SkuCodeList"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public JsonResult PackingScan2(string TrackCode, string SkuCodeList, string Type)
        {
            // 事务处理
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode ='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
                    if (list.Count > 0)
                    {
                        OrderType type = list[0];
                        object obj2 = base.NSession.CreateQuery("select count(Id) from OrderType where TrackCode='" + TrackCode + "'").UniqueResult();
                        if (Utilities.ToInt(obj2) > 1)
                        {
                            LoggerUtil.GetOrderRecord(type, "订单扫描错误！", "运单号" + TrackCode + "重复，已有订单使用！", base.CurrentUser, base.NSession);
                            return base.Json(new { IsSuccess = false, Result = "运单号" + TrackCode + "重复，已有订单使用！" });
                        }
                        if (type.Status == OrderStatusEnum.已处理.ToString() || type.Status == OrderStatusEnum.待包装.ToString())
                        {
                            //string ttt = type.TrackCode;
                            ////type.TrackCode = t;
                            //// 判断是否手动添加跟踪码,当手动添加跟踪码时跟踪码记录
                            //if (type.TrackCode == "已用完")
                            //{
                            //    type.TrackCode = TrackCode;
                            //}
                            ////type.Weight = Convert.ToInt32(w);

                            type.ScanningOn = DateTime.Now;
                            //type.Status = OrderStatusEnum.待发货.ToString();
                            type.ScanningBy = base.CurrentUser.Realname;
                            type.IsCanSplit = 0;
                            type.IsOutOfStock = 0;
                            base.NSession.Update(type);
                            //base.NSession.Flush();
                            ///////////////////////////
                            // 订单号交换处理 Begin
                            // 实现功能：1、单品多件内商品单号交换；2、多品多件内不同Sku单号交换(实际操作“多品”情况通过订单单独放置避免)
                            // 目标：单品快扫（单品单件、单品多件）；多品多件 共用此模块

                            // 1.获取本次扫描skucodelist的非指定订单号skucode - 升序
                            List<SKUCodeType> codelist1 = NSession.CreateQuery("from SKUCodeType where OrderNo<>'" + type.OrderNo + "' and Code in(" + SkuCodeList + ")").List<SKUCodeType>().OrderBy(x => x.Code).ToList();

                            if (codelist1.Count > 0)
                            {
                                // 交换skucodelist内OrderNo
                                foreach (SKUCodeType objA in codelist1)
                                {
                                    // 2.获取非本次skucodelist的指定订单号的skucodelist - 升序
                                    List<SKUCodeType> objB = NSession.CreateQuery("from SKUCodeType where OrderNo='" + type.OrderNo + "' and Code not in(" + SkuCodeList + ") and sku='" + objA.SKU + "'").List<SKUCodeType>().OrderBy(x => x.Code).ToList();
                                    if (objB.Count > 0)
                                    {
                                        objB[0].OrderNo = objA.OrderNo;
                                        objA.OrderNo = type.OrderNo;
                                        NSession.Update(objA);
                                        NSession.Update(objB[0]);
                                    }
                                }
                                //NSession.Flush();
                            }
                            else
                            {
                                // 无需交换
                            }
                            // 订单号交换处理 End
                            ///////////////////////////

                            // 设置订单商品非缺货状态
                            base.NSession.CreateQuery("update OrderProductType set IsQue=0 where OId =" + type.Id).ExecuteUpdate();
                            // 订单商品出库
                            if (SkuCodeList != "")
                            {
                                // 标记包装扫描
                                base.NSession.CreateQuery("update SKUCodeType set IsScan=1,PeiOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',OrderNo='" + type.OrderNo + "' where Code in ('" + SkuCodeList.Replace(",", "','") + "')").ExecuteUpdate();
                                LoggerUtil.GetOrderRecord(type, "订单包装扫描！", "包装扫描完成！", base.CurrentUser, base.NSession);
                            }

                          
                            tx.Commit();

                            return base.Json(new { IsSuccess = true, Result = "订单：" + type.OrderNo + "包装扫描结束，等待扫描商品条码...", OId = type.Id });
                        }
                        return base.Json(new { IsSuccess = false, Result = " 系统无法操作！当前状态为：" + type.Status + "！" });
                        // }
                        //  return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });
                    }
                    return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });
                }
                catch (HibernateException e)
                {
                    tx.Rollback();
                    return base.Json(new { IsSuccess = false, Result = e.Message });
                }
            }
        }
        public ViewResult PackingScanV2()
        {
            return View();
        }

        //[HttpPost, ActionName("Delete")]
        //public JsonResult DeleteConfirmed(int id)
        //{

        //    try
        //    {
        //        OrderRecordType obj = GetById(id);
        //        NSession.Delete(obj);
        //        NSession.Flush();
        //    }
        //    catch (Exception ee)
        //    {
        //        return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
        //    }
        //    return Json(new { IsSuccess = true });
        //}

        //public JsonResult List(int page, int rows)
        //{
        //    IList<OrderRecordType> objList = NSession.CreateQuery("from OrderRecordType")
        //        .SetFirstResult(rows * (page - 1))
        //        .SetMaxResults(rows * page)
        //        .List<OrderRecordType>();

        //    return Json(new { total = objList.Count, rows = objList });
        //}



        public string GetLog(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };
            int Year = int.Parse(search.Split('&')[0]);
            int Month = int.Parse(search.Split('&')[1]);


            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[U_WarehouseTurnoverOn]";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = Year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }

        public string GetLogII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };
            int Year = int.Parse(search.Split('&')[0]);
            int Month = int.Parse(search.Split('&')[1]);


            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[U_WarehouseTurnoverDown]";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = Year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }



        /// <summary>
        /// 发货扫描
        /// </summary>
        /// <param name="TrackCode">跟踪码</param>
        /// <param name="Weight">包裹重量</param>
        /// <param name="WarehouseId">仓库编号</param>
        /// <param name="LogisticMode">物流方式</param>
        /// <returns></returns>
        public JsonResult Scan(string TrackCode, int Weight, int WarehouseId, string LogisticMode,string Type)
        {

            List<OrderType> list = base.NSession.CreateQuery("from OrderType where TrackCode ='" + TrackCode + "'").List<OrderType>().ToList<OrderType>();
            if (list.Count > 0)
            {
                OrderType type = list[0];

                if (type.Status == OrderStatusEnum.已处理.ToString() || type.Status == OrderStatusEnum.待包装.ToString())
                {
                   
                        try
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
                           // base.NSession.Flush();


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
                                           // NSession.Flush();
                                            LoggerUtil.GetProductRecord(product[0], "商品修改", "发货扫描重量设置为:" + Weight, CurrentUser, NSession);

                                        }
                                    }

                                }
                            }
                            //*包装计件*//
                            PackagingScanLogType logtype = new PackagingScanLogType();
                            logtype.OperationOn = DateTime.Now;
                            logtype.PackageType = Type;
                            logtype.OrderNo = type.OrderNo;
                            logtype.Operator = base.CurrentUser.Realname;
                            NSession.SaveOrUpdate(logtype);
                            //base.NSession.Flush();
                            //*包装计件*//

                            // 标记出库
                            base.NSession.CreateQuery("update SKUCodeType set IsSend=1,isout=1,SendOn='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' where OrderNo ='" + type.OrderNo + "'").ExecuteUpdate();
                            LoggerUtil.GetOrderRecord(type, "订单扫描发货！", "将订单扫描发货了！运单号:" + type.TrackCode + " 原单号:" + ttt, base.CurrentUser, base.NSession);
                            
                            type.Freight = Convert.ToDouble(OrderHelper.GetFreight((double)type.Weight, type.LogisticMode, type.Country, base.NSession, 0M));
                            string str = string.Concat(new object[] { "订单： ", type.OrderNo, "已经发货! 发货方式：", LogisticMode, "  重量：", Weight });
                            OrderHelper.ReckonFinance(type, base.NSession);

                           
                            
                            return base.Json(new { IsSuccess = true, Result = "订单：" + type.OrderNo + "发货扫描结束，等待扫描跟踪码...", OId = type.Id });


                        }
                        catch (Exception)
                        {

                           
                        }



                    
                }
                return base.Json(new { IsSuccess = false, Result = " 系统无法操作！当前状态为：" + type.Status + "！" });

            }


            return base.Json(new { IsSuccess = false, Result = "系统无法找到订单" });


        }
    }
}

