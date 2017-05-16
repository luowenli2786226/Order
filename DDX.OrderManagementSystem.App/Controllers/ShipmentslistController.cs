using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ShipmentslistController : BaseController
    {
        public ViewResult Index()
        {
            string sql = @"
  delete from  Shipmentslist where Id not in (select ShipmentslistId from Shipments)";
            NSession.CreateSQLQuery(sql).ExecuteUpdate();
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        public ActionResult AuditShipmentslist()
        {
            return View();
        }
        public ActionResult ConfirmShipmentslist1()
        {
            return View();
        }
        public ActionResult ConfirmShipmentslist2()
        {
            return View();
        }
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Confirm2Detail(int id)
        {
            ShipmentslistType obj = GetById(id);
            return View(obj);
        }
        [HttpPost]
        public JsonResult Create(ShipmentslistType obj, string c)
        {
            try
            {
                if (IsCanList(c))
                {
                    ShipmentslistType list = new ShipmentslistType();
                    list.AppliBy = GetCurrentAccount().Realname;
                    list.AppliTime = DateTime.Now;
                    list.IsExa = Shipmentapproval.审核中.ToString();
                    object o = NSession.Save(list);
                    int id = Convert.ToInt32(o);
                    string sql = "update ShipmentsType set ShipmentslistId={0} where Id in ({1})";
                    sql = string.Format(sql, id, c);
                    IQuery query = NSession.CreateQuery(sql);
                    bool result = query.ExecuteUpdate() > 0;
                    return Json(new { IsSuccess = result });

                }
                else
                {
                    return Json(new { IsSuccess = false });
                }
                obj.AppliTime = DateTime.Now;
                obj.IsExa = "审核中";
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ShipmentslistType GetById(int Id)
        {
            ShipmentslistType obj = NSession.Get<ShipmentslistType>(Id);
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
            ShipmentslistType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(ShipmentslistType obj)
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
                ShipmentslistType obj = GetById(id);
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
            double sum1 = 0, sum2 = 0, sum3 = 0, sum4 = 0, sum5 = 0, sum6 = 0, sum7 = 0;
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
            IList<ShipmentslistType> objList = NSession.CreateQuery("from ShipmentslistType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<ShipmentslistType>();
            foreach (ShipmentslistType shipmentslistType in objList)
            {
                shipmentslistType.PriceFactorytotal = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(cast(PriceFactory as float)*Qty/6.5),2) from ShipmentsType where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总工厂价格
                shipmentslistType.YouShengtotal = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(cast(YouShengPrice as float)*Qty),2) from ShipmentsType where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总优胜价格
                shipmentslistType.TotalPrice = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(cast(TotalPrice as float)),2) from ShipmentsType where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总UMAX即客户佣金价格
                shipmentslistType.WeightGrosstotal = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(cast(WeightGross as float)*Qty),2) from ShipmentsType  where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总毛重
                shipmentslistType.WeightNettotal = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(cast(WeightNet as float)*Qty),2) from ShipmentsType  where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总净重
                shipmentslistType.Qty = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(Qty),2) from ShipmentsType  where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总数量
                shipmentslistType.TotalVolume = Convert.ToDouble(NSession.CreateQuery("select Round(SUM(TotalVolume),2) from ShipmentsType  where ShipmentslistId=" + shipmentslistType.Id).UniqueResult());//总体积
                sum1 += shipmentslistType.PriceFactorytotal;//总工厂价格
                sum2 += shipmentslistType.YouShengtotal;//总优胜价格
                sum3 += shipmentslistType.TotalPrice;//总UMAX即客户佣金价格
                sum4 += shipmentslistType.WeightGrosstotal;//总毛重
                sum5 += shipmentslistType.WeightNettotal;//总净重
                sum6 += shipmentslistType.Qty;//总数量
                sum7 += shipmentslistType.TotalVolume;//总体积

            }
            List<ShipmentslistType> list1 = new List<ShipmentslistType> {
                new ShipmentslistType{
                    ContractPNo ="合计栏",
                    PriceFactorytotal =Math.Round(sum1,2),//总工厂价格
                    YouShengtotal=Math.Round(sum2,2),//总优胜价格
                    TotalPrice=Math.Round(sum3,2),//总UMAX即客户佣金价格
                    WeightGrosstotal=Math.Round(sum4,2),//总毛重
                    WeightNettotal = Math.Round(sum5,2),//总净重
                    Qty = Math.Round(sum6,2),//总数量
                    TotalVolume = Math.Round(sum7,2)//总体积
                }
                };
            object count = NSession.CreateQuery("select count(Id) from ShipmentslistType " + where).UniqueResult();
            return Json(new { total = count, rows = objList, footer = list1 });
        }
        public JsonResult GetShipments(int ShipmentlistId)
        {
            string sql = "from ShipmentsType where ShipmentslistId={0}";
            sql = string.Format(sql, ShipmentlistId);
            IList<ShipmentsType> list = NSession.CreateQuery(sql).List<ShipmentsType>();
            foreach (ShipmentsType shipmentsType in list)
            {
                shipmentsType.YouShengtotal = Math.Round(Convert.ToDouble(shipmentsType.YouShengPrice) * shipmentsType.Qty, 2);
                shipmentsType.PriceFactorytotal = Math.Round(Convert.ToDouble(shipmentsType.PriceFactory) * shipmentsType.Qty, 2);
                shipmentsType.WeightGrosstotal = Math.Round(Convert.ToDouble(shipmentsType.WeightGross) * shipmentsType.Qty, 2);
                shipmentsType.WeightNettotal = Math.Round(Convert.ToDouble(shipmentsType.WeightNet) * shipmentsType.Qty, 2);

            }
            return Json(new { total = list.Count, rows = list });
        }
        /// <summary>
        /// 审批清单列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult DoAudit(int id, int type, string remark)
        {
            string sql = "update ShipmentslistType set AgreeBy='{0}',ExaTime='{1}',IsExa='{2}',Remark='{4}' where id={3}";
            if (CurrentUser.Realname == "雷刚" || CurrentUser.Realname == "管理员")
            {
                string Remark = remark + "<--" + Convert.ToString(NSession.CreateSQLQuery("select Remark from Shipmentslist where Id='" + id + "'").UniqueResult());
                //同意
                if (type == 1)
                {
                    sql = string.Format(sql, GetCurrentAccount().Realname, DateTime.Now, Shipmentapproval.审核通过, id, Remark);
                }
                else
                {
                    sql = string.Format(sql, GetCurrentAccount().Realname, DateTime.Now, Shipmentapproval.审核拒绝, id, Remark);

                }
                IQuery Query = NSession.CreateQuery(sql);
                bool result = Query.ExecuteUpdate() > 0;
                return Json(new { IsSuccess = result });
            }
            return Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
        }
        /// <summary>
        /// 审批拒绝后再次提交申请
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult SumbitApprove(int id)
        {
            string sql = "update ShipmentslistType set IsExa='{0}',AppliTime=getdate(),AppliBy='{1}' where id={2}";
            sql = string.Format(sql, Shipmentapproval.审核中, GetCurrentAccount().Realname, id);
            IQuery Query = NSession.CreateQuery(sql);
            bool result = Query.ExecuteUpdate() > 0;
            return Json(new { IsSuccess = result });
        }
        /// <summary>
        /// 第一次确认
        /// </summary>
        /// <returns></returns>
        public JsonResult DoConfirm1(int id, int type,string  remark)
        {
            if (CurrentUser.Realname == "雷刚" || CurrentUser.Realname == "管理员" || CurrentUser.Realname == "许东旭")
            {
                List<ShipmentsType> Shipments = NSession.CreateSQLQuery("select * from Shipments where ShipmentslistId=" + id + "").AddEntity(typeof(ShipmentsType)).List<ShipmentsType>().ToList();
                if (type == 1)//确认,遍历出货清单中的出货明细，将义乌仓库的库存减去
                {
                    // 事务控制
                    using (ITransaction tx = NSession.BeginTransaction())
                    {
                        try
                        {
                            //计算优胜价格
                            double CurrencyValue = Convert.ToDouble(NSession.CreateSQLQuery("select CurrencyValue from FixedRate where CurrencyCode='USD' and Year='" + DateTime.Now.Year + "' and Month='" + DateTime.Now.Month + "' ").UniqueResult());
                            if (CurrencyValue == 0)
                            {
                                CurrencyValue = 6.5;
                            }
                            //List<ShipmentsType> Shipments = NSession.CreateSQLQuery("select * from Shipments where ShipmentslistId=" + id + "").AddEntity(typeof(ShipmentsType)).List<ShipmentsType>().ToList();
                            foreach (ShipmentsType Shipment in Shipments)
                            {
                                //单个SKU运费$=头程费用￥*单个SKU毛重
                                double freight = Convert.ToDouble(Shipment.HeadloadCharges) / CurrencyValue * Convert.ToDouble(Shipment.WeightGross);
                                //计算商品价格
                                //大富物流
                                double price = Convert.ToDouble(NSession.CreateSQLQuery("select top 1 Amount  from WarehouseStockData where  WName='大富物流' and   NowQty>0  and SKU='" + Shipment.Sku + "'").UniqueResult());
                                //计算优胜价格
                                Shipment.YouShengPrice = (price - (price / 1.17 * Shipment.TaxRate)) / CurrencyValue * 1.12;
                                ////UMAX价格($)=优胜价格$ + 单个SKU运费$
                                Shipment.PriceUMax = (Shipment.YouShengPrice + freight).ToString();
                                NSession.SaveOrUpdate(Shipment);
                                NSession.Flush();
                                //大富物流仓库库存
                                int stockqty = Convert.ToInt32(NSession.CreateSQLQuery("select Qty from WarehouseStock where SKU='" + Shipment.Sku + "' and Warehouse='大富物流'").UniqueResult());//库存数量
                                if (stockqty < Shipment.Qty)
                                {
                                    return Json(new { IsSuccess = false, ErrorMsg = Shipment.Sku + "实际库存小于出货清单数量,大富物流无法出库" });
                                }
                                int WId1 = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("  select Id from Warehouse where Wname='YWZZC'")).UniqueResult());
                                int WId2 = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("  select Id from Warehouse where Wname='大富物流'")).UniqueResult());
                                //YWZZC入库
                                Utilities.StockIn(WId1, Shipment.Sku, Convert.ToInt32(Shipment.Qty), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单1入库", CurrentUser.Realname, "确认1入库YWZZC", NSession, true);
                                //大富物流出库
                                Utilities.StockIn(WId2, Shipment.Sku, Convert.ToInt32(Shipment.Qty) * (-1), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单1出库", CurrentUser.Realname, "确认1出库大富物流", NSession, true);
                            }
                            //string sql = "update WarehouseStock set Qty=Qty-(select Qty from Shipments where id=" + id + ") where SKU=(select Sku from Shipments where id=" + id + ") and Warehouse='义乌仓库'";
                            //NSession.CreateSQLQuery(sql).ExecuteUpdate();
                            //NSession.Flush(); 
                            string sql = "update ShipmentslistType set IsExa='" + Shipmentapproval.确认通过I + "',OkBy1='" + GetCurrentAccount().Realname + "',OverTime1='" + DateTime.Now + "'where id=" + id;
                            NSession.CreateQuery(sql).ExecuteUpdate();
                            //tx.Commit();
                            return Json(new { IsSuccess = true, ErrorMsg = "成功" });
                        }

                        catch (Exception ex)
                        {

                            tx.Rollback();
                            return Json(new { IsSuccess = false, ErrorMsg = "失败" });

                        }
                    }
                }
                //}
                else//拒绝
                {
                    //foreach (ShipmentsType Shipment in Shipments)
                    //{
                    //    //大富物流仓库库存
                    //    int stockqty = Convert.ToInt32(NSession.CreateSQLQuery("select Qty from WarehouseStock where SKU='" + Shipment.Sku + "' and Warehouse='YWZZC'").UniqueResult());//库存数量
                    //    if (stockqty < Shipment.Qty)
                    //    {
                    //        return Json(new { IsSuccess = false, ErrorMsg = Shipment.Sku + "实际库存小于出货清单数量,YWZZC无法出库" });
                    //    }
                    //    int WId1 = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("  select Id from Warehouse where Wname='YWZZC'")).UniqueResult());
                    //    int WId2 = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("  select Id from Warehouse where Wname='大富物流'")).UniqueResult());
                    //    //恢复YWZZC库存--出库
                    //    Utilities.StockIn(WId1, Shipment.Sku, Convert.ToInt32(Shipment.Qty) * (-1), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单1拒绝出库", CurrentUser.Realname, "拒绝1出库YWZZC", NSession, true);
                    //    //恢复大富物流库存--入库
                    //    Utilities.StockIn(WId2, Shipment.Sku, Convert.ToInt32(Shipment.Qty), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单1拒绝入库", CurrentUser.Realname, "拒绝1入库大富物流", NSession, true);
                    //}
                    string Remark = remark + "<--" + Convert.ToString(NSession.CreateSQLQuery("select Remark from Shipmentslist where Id='" + id + "' ").UniqueResult());
                    string sql = "update ShipmentslistType set IsExa='" + Shipmentapproval.确认拒绝I + "',Remark='" + Remark + "' where id=" + id;
                    NSession.CreateQuery(sql).ExecuteUpdate();
                    return Json(new { IsSuccess = true });
                }
            }
            return Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
        }
        /// <summary>
        /// 第二次确认
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Confirm2Detail(ShipmentslistType obj)
        {
            if (CurrentUser.Realname == "雷刚" || CurrentUser.Realname == "管理员" || CurrentUser.Realname == "许东旭" || CurrentUser.Realname == "叶珍珍" || CurrentUser.Realname == "周晓伟")
            {
                List<ShipmentsType> Shipments = NSession.CreateSQLQuery("select * from Shipments where ShipmentslistId=" + obj.Id + "").AddEntity(typeof(ShipmentsType)).List<ShipmentsType>().ToList();
                try
                {
                    foreach (ShipmentsType Shipment in Shipments)
                    {
                        int stockqty = Convert.ToInt32(NSession.CreateSQLQuery("select Qty from WarehouseStock where SKU='" + Shipment.Sku + "' and Warehouse='YWZZC'").UniqueResult());//库存数量
                        if (stockqty < Shipment.Qty)
                        {
                            return Json(new { IsSuccess = false, ErrorMsg = Shipment.Sku + "实际库存小于出货清单数量,YWZZC无法出库" });
                        }
                        int WId1 = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("  select Id from Warehouse where Wname='YWZZC'")).UniqueResult());
                        string WId2 = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("  select Wname from Warehouse where Id='" + obj.WareHouse + "'")).UniqueResult());
                        //YWZZC库存--出库
                        Utilities.StockIn(WId1, Shipment.Sku, Convert.ToInt32(Shipment.Qty) * (-1), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单2确认出库", CurrentUser.Realname, "确认2出库YWZZC", NSession, true);
                        //选择海外仓--入库
                        Utilities.StockIn(Convert.ToInt32(obj.WareHouse), Shipment.Sku, Convert.ToInt32(Shipment.Qty), Math.Round(Convert.ToDouble(Shipment.PriceUMax) * 6.5, 2), "清单2确认入库", CurrentUser.Realname, "确认2入库" + WId2, NSession, true);
                    }
                    obj.OkBy2 = GetCurrentAccount().Realname;
                    obj.OverTime2 = Convert.ToString(DateTime.Now);
                    obj.IsExa = Shipmentapproval.确认通过II.ToString();
                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                }
                catch (Exception ee)
                {
                    return Json(new { errorMsg = "出错了" });
                }
                return Json(new { IsSuccess = "true" });
            }
            return Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
        }
        /// <summary>
        /// 获取审批进程字段
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult GetShipmentapproval(string Id)
        {
            List<object> data = new List<object>();
            if (Id == "1")
            {
                data.Add(new { id = "ALL", text = "ALL" });
            }
            foreach (string str in Enum.GetNames(typeof(Shipmentapproval)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }


        public JsonResult Getship(string sort, string order, string search)
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
            IList<ShipmentsType> objList = NSession.CreateQuery("from ShipmentsType " + where + orderby)
                .List<ShipmentsType>();

            object count = NSession.CreateQuery("select count(Id) from ShipmentsType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        /// <summary>
        /// 是否可以生成清单
        /// </summary>
        /// <param name="ShipmentId"></param>
        /// <returns></returns>
        public bool IsCanList(string ShipmentId)
        {
            string[] ids = ShipmentId.Split(',');
            string sql = " from ShipmentsType where (ShipmentslistId in (select id from ShipmentslistType where IsExa in ('" + Shipmentapproval.确认拒绝I + "','" + Shipmentapproval.确认拒绝II + "','" + Shipmentapproval.审核拒绝 + "')  ) or ShipmentslistId=0) and Id in ({0})";
            sql = string.Format(sql, ShipmentId);

            List<ShipmentsType> shipments = NSession.CreateQuery(sql).List<ShipmentsType>().ToList();
            //最后得到的值和传进的ID的数量相等，说明全部满足listid为0或者财审拒绝，说明可以生成清单
            if (shipments != null && shipments.Count == ids.Length)
            {
                return true;
            }
            //没有值，说明不能再次生成清单
            return false;
        }

        public JsonResult ToExcelTotal(string id)
        {
            try
            {
                string sql = "select '' as '长','' as '宽','' as '高', ExportNo as '我司合同号码', '' as '客户合同号码','' as '客人货号',Sku as '我司货号',DescribeEn as '英文描述',DescribeCn as '中文描述','￥'+convert(varchar(50),cast(PriceFactory as money)) as '工厂价格（RMB）',Unit as '单位',PackageNo as '包装',Ctn  as '箱数','' as '包装尺寸' ,Qty*WeightGross as '总毛重（GK）',Qty*WeightNet  as '总净重（KG）',Qty as '总数量',TotalVolume as '总体积','$'+ convert(varchar(50),cast(Qty*PriceUMax as money)) as '客人含佣金总价','￥'+convert(varchar(50),cast(Qty*PriceFactory as money)) as '工厂总金额','' as '厂家', case when IsCustoms=0 then '' else '是' end '报关' , case when IsCustoms=0 then '' else TaxRate end '增值税率','' as '唛头', '' as '工厂电话','' as '合同交期',Ratio as '比值',CreateBy as '业务人',CreatePlanBy as '采购员',CreatetTrackBy as '跟单员','' as '付款方式','$'+ convert(varchar(50),cast(PriceUMax as money))  as 'UMAX价格', '$'+ convert(varchar(50),Round(cast(HeadloadCharges as float)*cast(WeightGross as float)/6.5,2)) as '头程费用' from Shipments where ShipmentslistId=" + id;
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });

        }
    }
}
