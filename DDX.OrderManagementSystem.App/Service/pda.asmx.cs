using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using NHibernate;
using System.Web.Script.Serialization;
using System.Web.Services.Protocols;
using DDX.OrderManagementSystem.Domain.OMS.Entities;
using NHibernate.Mapping;

namespace DDX.OrderManagementSystem.App.Service
{
    /// <summary>
    /// pda1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [SoapDocumentService(RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class pda1 : BaseController
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        /// <summary>
        /// PDA用户登录
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        [WebMethod]
        public string Login(string Uid, string Pwd)
        {
            List<UserType> users = NSession.CreateQuery("from UserType where Username=:p1 and Password=:p2").SetString("p1", Uid.Trim()).SetString("p2", Pwd.Trim()).List<UserType>().ToList();
            //string obj = serializer.Serialize(new { IsSuccess = true, Result = users });
            if (users.Count > 0)
            {
                // 成功
                return serializer.Serialize(new { IsSuccess = true, Result = users });
            }
            else
            {
                // 失败
                return serializer.Serialize(new { IsSuccess = false }); ;
            }
        }

        /// <summary>
        /// 获取SKU相关订单列表(增加类型，0是所有，1是单品单件，2是单品多件)
        /// </summary>
        /// <param name="SkuCode"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetSkuOrders(string SkuCode, Boolean IsOutOfStock, string type = "0")
        {
            ///////////////////////////////////////////////////////////////////
            // 需排队指定承运商订单，因承运商不允许打印面单无法确保分拣码准备性；被排队承运商:CNE
            ///////////////////////////////////////////////////////////////////


            // 获取SKU
            List<SKUCodeType> skucodelist = NSession.CreateQuery("from SKUCodeType where Code=:p1").SetString("p1", SkuCode).List<SKUCodeType>().ToList();

            if (skucodelist.Count > 0)
            {
                // 获取订单 IsOutOfStock=1(缺货) 货架扫描前允许不增加该条件 and IsPrint>0 暂时移除,过滤问题订单IsError=0
                /*
                 * // “物流方式”与“打印模板”关联 Code=b.LogisticsCode+'10*10'
                 select OrderNo from Orders c join LogisticsMode b on c.LogisticMode=b.LogisticsCode join PrintTemplate a on a.Code=b.LogisticsCode+'10*10' where IsOutOfStock=0 and Status='已处理'and IsError=0 and IsPrint>0
                 * 
                 * // 未关联Sql
                 * select OrderNo from Orders c join LogisticsMode b on c.LogisticMode=b.LogisticsCode join PrintTemplate a on a.Code=b.LogisticsCode+'10*10' where IsOutOfStock=0 and Status='已处理'and IsError=0 and IsPrint>0
                 */
                List<OrderProductType> orders = new List<OrderProductType>();
                string where = string.Empty;//判断类型
                switch (type)
                {

                    case "1": where = " and Qty=1"; break;
                    case "2": where = " and Qty>1"; break;
                    default: break;
                }
                //目前只允许出现平邮和852的两种发货方式的订单
                orders = NSession.CreateSQLQuery("select * from OrderProducts a where SKU='" + skucodelist[0].SKU + "' and OrderNo in (select OrderNo from Orders c join LogisticsMode b on c.LogisticMode=b.LogisticsCode join PrintTemplate a on a.Code=b.LogisticsCode+'10*10' where IsOutOfStock=" + (IsOutOfStock == true ? "1" : "0") + " and Status='已处理' and Enabled=1 and IsError=0 and IsPrint>0 and (b.LogisticsCode='平邮线上发货' or  b.LogisticsCode='852俄罗斯小包')) and (select count(1) from OrderProducts c where c.OrderNo=a.OrderNo)=1 and a.OrderNo not in (select OrderNo from SKUCode where SKU='" + skucodelist[0].SKU + "' and IsScan=1)" + where + "").AddEntity(typeof(OrderProductType)).List<OrderProductType>().ToList();
                // 成功
                //return serializer.Serialize(new { IsSuccess = true, Result = orders });
                if (orders.Count != 0)
                {
                    // 成功
                    return serializer.Serialize(new { IsSuccess = true, Result = orders });
                }
                else
                {
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, Result = "失败!无" + skucodelist[0].SKU + "订单." });
                }
            }
            else
            {
                // 失败
                return serializer.Serialize(new { IsSuccess = false, Result = "失败!无法获取该条码SKU." });
            }
        }

        /// <summary>
        /// 验证扫描Sku是否与订单匹配
        /// </summary>
        /// <param name="OrderSKU"></param>
        /// <param name="ScanSkuCode"></param>
        /// <returns></returns>
        [WebMethod]
        public string CheckScanSku(string OrderSku, string ScanSkuCode)
        {
            List<SKUCodeType> skucodelist = NSession.CreateQuery("from SKUCodeType where IsScan=0 and Code=:p1").SetString("p1", ScanSkuCode).List<SKUCodeType>().ToList();

            if (skucodelist.Count > 0)
            {
                if (OrderSku.ToUpper() == skucodelist[0].SKU.ToUpper())
                {
                    // 成功
                    return serializer.Serialize(new { IsSuccess = true });
                }
                else
                {
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, result = "错误.扫描商品Sku与订单不匹配." });
                }
            }
            else
            {
                // 失败
                return serializer.Serialize(new { IsSuccess = false, result = "错误.未查询到商品." });
            }
        }

        /// <summary>
        /// 单品快扫-(是否满足多品多件待测试)匹配订单
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="SkuCodeList"></param>
        /// <returns></returns>
        [WebMethod]
        public string MatchOrder(string OrderNo, string SkuCodeList)
        {
            string[] codelist = SkuCodeList.Split(',');
            bool bStatus = false;
            // 事务控制
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    for (int nI = 0; nI < codelist.Length; nI++)
                    {
                        List<SKUCodeType> skucodelist = NSession.CreateQuery("from SKUCodeType where IsScan=0 and Code=:p1").SetString("p1", codelist[nI]).List<SKUCodeType>().ToList();

                        if (skucodelist.Count > 0)
                        {
                            skucodelist[0].OrderNo = OrderNo;
                            skucodelist[0].PeiOn = DateTime.Now.ToString();
                            skucodelist[0].IsScan = 1;
                            NSession.Update(skucodelist[0]);
                            // 成功
                            bStatus = true;
                        }
                        else
                        {
                            // 失败
                            bStatus = false;
                        }
                    }
                    if (bStatus == true)
                    {
                        // 拣货结束修改订单状态
                        List<OrderType> list = base.NSession.CreateQuery("from OrderType where OrderNo ='" + OrderNo + "'").List<OrderType>().ToList<OrderType>();
                        list[0].Status = OrderStatusEnum.待包装.ToString();
                        NSession.Update(list[0]);

                        tx.Commit();
                        // 成功
                        return serializer.Serialize(new { IsSuccess = true, result = "订单: " + OrderNo + " 拣货扫描结束." });
                    }
                    else
                    {
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, result = "订单: " + OrderNo + " 拣货扫描失败. 条码: " + SkuCodeList + " 错误." });
                    }
                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false });
                }
            }
        }
        /// <summary>
        /// 货位货架关联-将多个相同货架的货位批量更新
        /// </summary>
        /// <param name="LocationId"></param>
        /// <param name="RackId"></param>
        /// <returns></returns>
        [WebMethod]
        public string UpdateRackByLocationID(string code, string Rackcode, int wid)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    string Message = "成功";
                    code = DisposeCode(code.Split(','));
                    string sql2 = "select RackId from WarehouseRack where RackCode='{0}'";
                    sql2 = string.Format(sql2, Rackcode);
                    object RackId = NSession.CreateSQLQuery(sql2).UniqueResult();
                    if (RackId != null)
                    {
                        string sql = "update WarehouseLocationType set ParentId={0} where Code in ({1}) and Wid={2}";
                        sql = string.Format(sql, RackId, code, wid);
                        IQuery Query = NSession.CreateQuery(sql);
                        Query.ExecuteUpdate();
                        tx.Commit();
                        // 成功
                        return serializer.Serialize(new { IsSuccess = true, ReturnMsg = Message });
                    }
                    else
                    {
                        Message = "无当前货架";
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                    }
                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, ReturnMsg = "报错了" });
                }
            }
        }
        /// <summary>
        /// 货架排位关联--将相同排位的货架批量更新
        /// </summary>
        /// <param name="RackId"></param>
        /// <param name="LineId"></param>
        /// <returns></returns>
        [WebMethod]
        public string UpdateLineByRackID(string RackCode, string LineCode)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    string Message = "成功";
                    RackCode = DisposeCode(RackCode.Split(','));
                    object LineId = NSession.CreateSQLQuery(string.Format("select LineId from WarehouseLine where LineCode='{0}'", LineCode)).UniqueResult();
                    if (LineId != null)
                    {
                        string sql = "update WarehouseRackType set LineId={0},LineCode='{1}',CreateOn=getdate() where RackCode in ({2})";
                        sql = string.Format(sql, LineId, LineCode, RackCode);
                        IQuery Query = NSession.CreateQuery(sql);
                        Query.ExecuteUpdate();
                        tx.Commit();
                        // 成功
                        return serializer.Serialize(new { IsSuccess = true, ReturnMsg = Message });
                    }
                    else
                    {
                        Message = "无当前排位";
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                    }
                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, ReturnMsg = "报错了" });
                }
            }
        }

        /// <summary>
        /// 排位区域关联--将相同区域的排位批量更新
        /// </summary>
        /// <param name="LineId"></param>
        /// <param name="AreaId"></param>
        /// <returns></returns>
        [WebMethod]
        public string UpdateAreaByLineID(string LineCode, string AreaCode)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    string Message = "成功";
                    LineCode = DisposeCode(LineCode.Split(','));
                    object AreaId = NSession.CreateSQLQuery(string.Format("select AreaId from WarehouseArea where AreaCode='{0}'", AreaCode)).UniqueResult();
                    if (AreaId != null)
                    {
                        string sql = "update WarehouseLineType set AreaId={0},AreaCode='{1}',CreateOn=getdate() where LineCode in ({2})";
                        sql = string.Format(sql, AreaId, AreaCode, LineCode);
                        IQuery Query = NSession.CreateQuery(sql);
                        Query.ExecuteUpdate();
                        tx.Commit();
                        // 成功
                        return serializer.Serialize(new { IsSuccess = true, ReturnMsg = Message });
                    }
                    else
                    {
                        Message = "无当前区位";
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                    }

                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, ReturnMsg = "报错了" });
                }
            }
        }

        /// <summary>
        /// 商品上架
        /// </summary>
        /// <param name="LocationId"></param>
        /// <param name="SkuCode"></param>
        /// <param name="CreateBy"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [WebMethod]
        public string InsertSkuByLocationId(string LocationCode, string SkuCodeList, string CreateBy, int Qty, int Status)
        {
            //List<WarehouseLocationType> location = base.NSession.CreateQuery("from WarehouseLocationType where ").List<WarehouseLocationType>().ToList();
            // 事务控制
            using (ITransaction tx = NSession.BeginTransaction())
            {
                string Message = "";
                //判断货位是否存在
                List<WarehouseLocationType> locationtype = NSession.CreateQuery(string.Format("from WarehouseLocationType where Code='{0}'", LocationCode)).List<WarehouseLocationType>().ToList();
                if (locationtype != null && locationtype.Count > 0)
                {
                    try
                    {
                        for (int nI = 0; nI < SkuCodeList.Split(',').Length; nI++)
                        {
                            //判断商品是否已经上架
                            //上架的数量
                            object TurnoverOn = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=1", SkuCodeList.Split(',')[nI])).UniqueResult();


                            //下架的数量
                            object TurnoverDown = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=0", SkuCodeList.Split(',')[nI])).UniqueResult();
                            if (Convert.ToInt32(TurnoverOn) - Convert.ToInt32(TurnoverDown) == 0)
                            {
                                //判断商品是否存在
                                object skucodetype = NSession.CreateSQLQuery(string.Format("select count(0) from SKUCode where Code={0}", SkuCodeList.Split(',')[nI])).UniqueResult();
                                if (Convert.ToInt32(skucodetype) > 0)
                                {
                                    WarehouseTurnoverType warehouseTurnoverType = new WarehouseTurnoverType();
                                    warehouseTurnoverType.LocationCode = locationtype[0].Id;
                                    warehouseTurnoverType.SkuCode = SkuCodeList.Split(',')[nI];
                                    warehouseTurnoverType.CreateBy = CreateBy;
                                    warehouseTurnoverType.CreateOn = DateTime.Now;
                                    warehouseTurnoverType.Status = Status;
                                    warehouseTurnoverType.Qty = Qty;
                                    base.NSession.Save(warehouseTurnoverType);
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                //已经上过架就不操作
                                Message += SkuCodeList.Split(',')[nI] + "已经上过架，不能再次上架.";

                            }
                        }
                        tx.Commit();
                        if (string.IsNullOrEmpty(Message))
                        {
                            // 成功
                            return serializer.Serialize(new { IsSuccess = true, ReturnMsg = "成功" });
                        }
                        else
                        {
                            return serializer.Serialize(new { IsSuccess = false, ReturnMsg =Message });
                        }
                        
                    }
                    catch (HibernateException)
                    {
                        tx.Rollback();
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                    }
                }
                else
                {
                    Message = "无当前货位";
                    return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                }

            }
        }
        //public string 
        public string DisposeCode(string[] codelist)
        {
            string strcode = "";
            for (int i = 0; i < codelist.Length; i++)
            {
                codelist[i] = "'" + codelist[i] + "'";
                if (i < codelist.Length - 1)
                {
                    strcode = strcode + codelist[i] + ",";
                }
                else
                {
                    strcode = strcode + codelist[i];
                }
            }
            return strcode;
        }
        /// <summary>
        /// 商品是否上过架
        /// </summary>
        /// <param name="SkuCode"></param>
        /// <returns></returns>
        [WebMethod]
        public string IsProductImport(string SkuCode)
        {
            //是否合法的SKUCODE
            List<SKUCodeType> skucode = NSession.CreateQuery(string.Format("from SKUCodeType where Code={0}", SkuCode)).List<SKUCodeType>().ToList();
            if (skucode != null && skucode.Count > 0)
            {

                //上架的数量
                object TurnoverOn = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=1", SkuCode)).UniqueResult();
                //下架的数量
                object TurnoverDown = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=0", SkuCode)).UniqueResult();
                //判断是否是上过架的商品,如果上架数量-下架数量=0，则可以上架
                if (Convert.ToInt32(TurnoverOn) - Convert.ToInt32(TurnoverDown) == 0)
                {
                    return serializer.Serialize(new { IsSKU = true, IsSuccess = false, Result = skucode });
                }
                else
                {
                    //不可以上架，已经上过架
                    return serializer.Serialize(new { IsSKU = true, IsSuccess = true });
                }

            }
            else
            {
                return serializer.Serialize(new { IsSKU = false });
            }

        }
        /// <summary>
        /// 判断货位是否有效
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string IsCorrectLocation(string LocationCode)
        {
            //判断货位是否存在
            object Location = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseLocation where Code='{0}'", LocationCode)).UniqueResult();
            if (Convert.ToInt32(Location) > 0)
            {
                return serializer.Serialize(new { IsSuccess = true });
            }
            else
            {
                return serializer.Serialize(new { IsSuccess = false });
            }
        }
        /// <summary>
        /// 区域管理-添加
        /// </summary>
        /// <param name="WarehouseId">仓库Id</param>
        /// <param name="AreaCount">创建区域数量</param>
        /// <returns></returns>
        public string ManagementAreaAdd(int WarehouseId, int AreaCount)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    List<ResultInfo> resultInfos = new List<ResultInfo>();
                    WarehouseType WarehouseObj = NSession.Get<WarehouseType>(WarehouseId);
                    if (WarehouseObj == null)
                    {
                        // 失败
                        return serializer.Serialize(new { IsSuccess = false, result = "无法获取仓库." });
                    }

                    // 添加区域
                    for (int nI = 0; nI < AreaCount; nI++)
                    {
                        WarehouseAreaType WarehouseAreaObj = new WarehouseAreaType();

                        WarehouseAreaObj.CreateOn = DateTime.Now;
                        WarehouseAreaObj.CreateBy = GetCurrentAccount().Realname;
                        WarehouseAreaObj.AreaCode = Utilities.GetAreaCode(base.NSession);
                        WarehouseAreaObj.WName = WarehouseObj.WName;

                        NSession.SaveOrUpdate(WarehouseAreaObj);
                        NSession.Flush();

                        resultInfos.Add(OrderHelper.GetResult(WarehouseAreaObj.AreaCode, WarehouseAreaObj.WName, "成功"));
                    }
                    tx.Commit();
                    // 成功
                    return serializer.Serialize(new { IsSuccess = true, result = Json(resultInfos, "text/html", JsonRequestBehavior.AllowGet) });
                }
                catch (Exception ee)
                {
                    tx.Rollback();
                    return serializer.Serialize(new { IsSuccess = false, result = ee.Message });
                }
            }
        }

        /// <summary>
        /// 区域管理-编辑
        /// </summary>
        /// <param name="WarehouseId">仓库Id</param>
        /// <param name="AreaCodeList">区域编辑列表</param>
        /// <returns></returns>
        public string ManagementAreaEdit(int WarehouseId, string AreaCodeList)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    List<ResultInfo> resultInfos = new List<ResultInfo>();
                    WarehouseType WarehouseObj = NSession.Get<WarehouseType>(WarehouseId);
                    string[] area = AreaCodeList.Split(',');

                    // 重新关联区域与仓库
                    for (int nI = 0; nI < area.Length; nI++)
                    {
                        List<WarehouseAreaType> WarehouseAreaObj = NSession.CreateQuery("from WarehouseAreaType where Areacode=:p1").SetString("p1", area[nI]).List<WarehouseAreaType>().ToList();

                        if (WarehouseAreaObj.Count() > 0)
                        {
                            WarehouseAreaObj[0].WId = WarehouseObj.Id;
                            WarehouseAreaObj[0].WName = WarehouseObj.WName;

                            NSession.Update(WarehouseAreaObj);
                            NSession.Flush();
                            resultInfos.Add(OrderHelper.GetResult(WarehouseAreaObj[0].AreaCode, WarehouseAreaObj[0].WName, "成功"));
                        }
                    }
                    tx.Commit();
                    // 成功
                    return serializer.Serialize(new { IsSuccess = true, result = Json(resultInfos, "text/html", JsonRequestBehavior.AllowGet) });
                }
                catch (Exception ee)
                {
                    tx.Rollback();
                    return serializer.Serialize(new { IsSuccess = false, result = ee.Message });
                }
            }
        }
        /// <summary>
        /// 获取与货架关联货位内有订单的单品商品，并返回相关订单数据
        /// </summary>
        /// <param name="RackCode"></param>
        /// <returns></returns>
         [WebMethod]
        public string GetOrderInfoByRack(string RackCode)
        {
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    //先查询关联的货位
                    List<WarehouseLocationType> warehouselocationlist = NSession.CreateSQLQuery(string.Format("select * from WarehouseLocation where ParentId=(select RackId from WarehouseRack where RackCode='{0}') ", RackCode)).AddEntity(typeof(WarehouseLocationType)).List<WarehouseLocationType>().ToList();
                    if (warehouselocationlist != null && warehouselocationlist.Count > 0)
                    {
                        //遍历货位去寻找货位上的商品
                        foreach (WarehouseLocationType warehouselocation in warehouselocationlist)
                        {

                            //查找当前货位上的SKU
                            List<object> skucodelist = NSession.CreateSQLQuery(string.Format("select distinct sku  from SKUCode where Code in (select SkuCode from WarehouseTurnover where LocationCode='{0}')", warehouselocation.Id)).List<object>().ToList();
                            //当前货架上有产品
                            if (skucodelist != null && skucodelist.Count > 0)
                            {
                                foreach (object o in skucodelist)
                                {
                                    //通过产品关联订单
                                    List<object[]> objList = NSession.CreateSQLQuery(string.Format("select OrderNo,qty from OrderProducts where orderno in (select OrderNo from OrderProducts where OrderNo in (select o.OrderNo from Orders o join OrderProducts p on        o.OrderNo=p.OrderNo where p.SKU='{0}'and Status='已处理' and IsOutOfStock=0 and  isque=3 and IsAudit=1 and IsError=0 and Enabled=1    and IsPrint>=1 and IsFBA=0) group by Orderno having count(OrderNo)=1) order by qty asc", Convert.ToString(o))).List<object[]>().ToList();
                                    //有订单
                                    if (objList != null && objList.Count > 0)
                                    {
                                        foreach (var  obj in objList)
                                        {
             //1个货架对应多个货位，多个货位对应多个SKUCOde，如果多个SKUCode全部属于一个SKU，一个SKU只对应一条订单，那么只返回一条订单数据
                                            resultInfos.Add(OrderHelper.GetResult("订单信息", "", "成功", warehouselocation.Code, o.ToString(), obj[0].ToString(), obj[1].ToString()));
                                        }
                                    }
                                    else //无订单
                                    {

                                    }

                                }
                            }
                            else
                            {

                            }


                        }
                        tx.Commit();
                        return serializer.Serialize(new { IsSuccess = true, result = Json(resultInfos, "text/html", JsonRequestBehavior.AllowGet) });
                    }
                    else
                    {
                        //当前货架没有对应的货位
                        return serializer.Serialize(new { IsSuccess = false, result = "当前货架没有对应的货位" });
                    }
                }
                catch (Exception ee)
                {

                    tx.Rollback();
                    return serializer.Serialize(new { IsSuccess = false, result = ee.Message });
                }
            }



        }
        /// <summary>
        /// 判断商品是否未配货
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string ProductIsScan(string Code)
        {
            string sql = " from SKUCodeType where Code='{0}'";
            List<SKUCodeType> skucode = NSession.CreateQuery(string.Format(sql, Code)).List<SKUCodeType>().ToList();
            if (skucode != null && skucode.Count > 0)
            {
                //判断商品是否配货
                if (skucode[0].IsScan == 0)
                {
                    //未配货
                    return serializer.Serialize(new { IsSuccess = true, result = skucode[0].SKU });
                }
                else
                {
                    //已配货，终止
                    return serializer.Serialize(new { IsSuccess = false, result = "该商品已配货" });
                }
            }
            else
            {
                //不是有效的CODE
                return serializer.Serialize(new { IsSuccess = false, result = "请输入合法的SKUCode" });
            }

        }
        /// <summary>
        /// 标记SKUCOde
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string UpdateSkuCode(string OrderNo, string CodeList, string LocationCode, string CreateBy, int Qty, int Status)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {

                try
                {
                    string Message = "";
                    
                    //判断货位是否存在
                    List<WarehouseLocationType> locationtype = NSession.CreateQuery(string.Format("from WarehouseLocationType where Code='{0}'", LocationCode)).List<WarehouseLocationType>().ToList();
                    if (locationtype != null && locationtype.Count > 0)
                    {
                        try
                        {
                            for (int nI = 0; nI < CodeList.Split(',').Length; nI++)
                            {
                                //判断商品是否已经上架
                                //上架的数量
                                object TurnoverOn = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=1", CodeList.Split(',')[nI])).UniqueResult();


                                //下架的数量
                                object TurnoverDown = NSession.CreateSQLQuery(string.Format("select count(0) from WarehouseTurnover where SkuCode={0} and Status=0", CodeList.Split(',')[nI])).UniqueResult();
                                if (Convert.ToInt32(TurnoverOn) - Convert.ToInt32(TurnoverDown) == 1)//上架比下架多一次
                                {
                                    //判断商品是否存在
                                    object skucodetype = NSession.CreateSQLQuery(string.Format("select count(0) from SKUCode where Code={0}", CodeList.Split(',')[nI])).UniqueResult();
                                    if (Convert.ToInt32(skucodetype) > 0)
                                    {
                                        WarehouseTurnoverType warehouseTurnoverType = new WarehouseTurnoverType();
                                        warehouseTurnoverType.LocationCode = locationtype[0].Id;
                                        warehouseTurnoverType.SkuCode = CodeList.Split(',')[nI];
                                        warehouseTurnoverType.CreateBy = CreateBy;
                                        warehouseTurnoverType.CreateOn = DateTime.Now;
                                        warehouseTurnoverType.Status = Status;
                                        warehouseTurnoverType.Qty = Qty;
                                        base.NSession.Save(warehouseTurnoverType);
                                        string sql = "update SKUCodeType set IsScan=1,OrderNo='{0}',PeiOn=CONVERT(varchar(100), GETDATE(), 25) where Code='{1}'  ";
                                        sql = string.Format(sql, OrderNo, CodeList.Split(',')[nI]);
                                        NSession.CreateQuery(sql).ExecuteUpdate();
                                        sql = "update OrderType set Status='待包装' where OrderNo='{0}'";
                                        sql = string.Format(sql, OrderNo);
                                        NSession.CreateQuery(sql).ExecuteUpdate();
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    //没有上过架就不能进行下架操作
                                    
                                    Message += CodeList.Split(',')[nI]+"没有上过架，不能进行下架操作";

                                }
                            }
                            tx.Commit();
                            // 成功
                            if (string.IsNullOrEmpty(Message))
                            {
                                Message = "成功";
                                return serializer.Serialize(new { IsSuccess = true, ReturnMsg = Message });
                            }
                            //失败
                            else
                            {
                                return serializer.Serialize(new { IsSuccess = false, ReturnMsg = Message });
                            }
                           
                        }
                        catch (HibernateException)
                        {
                            tx.Rollback();
                            // 失败
                            return serializer.Serialize(new { IsSuccess = false, ReturnMsg = "失败" });
                        }
                    }
                    else
                    {
                        Message = "无当前货位";
                        return serializer.Serialize(new { IsSuccess = false, result = Message });
                    }
                    tx.Commit();
                    return serializer.Serialize(new { IsSuccess = true });
                }
                catch (Exception ex)
                {

                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, result = "报错了" });
                }

            }
        }
        /// <summary>
        /// 标记缺货订单
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string OutofstockOrder(string orderno)
        {
            using (ITransaction tx = NSession.BeginTransaction())
            {
                try
                {
                    string Message = "成功";
                    //标记未打印
                    string sql = "update OrderType set isoutofstock='1',isprint=0  where orderno='{0}'";
                    sql = string.Format(sql, orderno);
                    IQuery Query = NSession.CreateQuery(sql);
                    Query.ExecuteUpdate();

                    string sql1 = "update OrderProductType set IsQue=1 where orderno='{0}'";
                    sql1 = string.Format(sql1, orderno);
                    IQuery Query1 = NSession.CreateQuery(sql1);
                    Query1.ExecuteUpdate();
                    tx.Commit();
                    // 成功
                    return serializer.Serialize(new { IsSuccess = true, result = Message });
                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    // 失败
                    return serializer.Serialize(new { IsSuccess = false, result = "报错了" });
                }
            }
        }

        /// <summary>
        /// 获取商品上下架信息
        /// </summary>
        /// <param name="SkuCode"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetWarehouseTurnOver(string SkuCode)
        {
             string sql = " from SKUCodeType where Code='{0}'";
             List<SKUCodeType> skucode = NSession.CreateQuery(string.Format(sql, SkuCode)).List<SKUCodeType>().ToList();
            if (skucode != null && skucode.Count > 0)
            {
                List<ResultInfo> resultInfos = new List<ResultInfo>();
                List<object[]> warehouse = NSession.CreateSQLQuery(string.Format("select top 1 CreateBy,CreateOn from WarehouseTurnover where SkuCode='{0}'  and Status=1 order by CreateOn desc", SkuCode)).List<object[]>().ToList();
                if (warehouse != null && warehouse.Count > 0)
                {

                    resultInfos.Add(OrderHelper.GetResult("商品上架信息", "", "成功", warehouse[0][0].ToString(), warehouse[0][1].ToString(), "", ""));


                }
                List<object[]> warehouse2 = NSession.CreateSQLQuery(string.Format("select top 1 CreateBy,CreateOn from WarehouseTurnover where SkuCode='{0}'  and Status=0 order by CreateOn desc", SkuCode)).List<object[]>().ToList();
                if (warehouse2 != null && warehouse2.Count > 0)
                {
                    resultInfos.Add(OrderHelper.GetResult("商品下架信息", "", "成功", warehouse2[0][0].ToString(), warehouse2[0][1].ToString(), "", ""));
                }
                return serializer.Serialize(new { IsSuccess = true, result = Json(resultInfos, "text/html", JsonRequestBehavior.AllowGet) });
            }
            else
            {
                //不是有效的CODE
                return serializer.Serialize(new { IsSuccess = false, result = "请输入合法的SKUCode" });
            }

            

        }
    }
}
