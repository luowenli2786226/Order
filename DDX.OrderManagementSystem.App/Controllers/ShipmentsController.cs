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
    public class ShipmentsController : BaseController
    {
        //
        // GET: /ShipmentsOverseas/

        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(ShipmentsType obj)
        {
            try
            {
                obj.FirstQty = obj.PackageNo * obj.Ctn;
                obj.Qty = obj.FirstQty;
                //如果超出了不能创建
                if (IsBeyondQty(obj))
                {
                    return Json(new { Issuccess = false, Message = "请检查包装*箱数是否大于（实际库存-已经创建出货明细的数量）" }, "text/html", JsonRequestBehavior.AllowGet);
                }
                else if (IsSameSKU(obj))
                {
                    return Json(new { Issuccess = false, Message = "请检查是否已经创建相同SKU和外销合同的出货明细" }, "text/html", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (obj.WeightGross == 0)
                    {
                        obj.WeightGross = Convert.ToDouble(NSession.CreateSQLQuery("select weight from Products where SKU='" + obj.Sku + "'").UniqueResult()) * 0.001;
                    }
                    //大富物流
                    double price = Convert.ToDouble(NSession.CreateSQLQuery("select top 1 Amount  from WarehouseStockData where  WName='大富物流' and   NowQty>0  and SKU='" + obj.Sku + "'").UniqueResult());
                    //优胜价格$=（YWZZC商品价格-YWZZC商品价格/1.17*退税率）/汇率*1.12
                    obj.YouShengPrice = Math.Round((price - price / 1.17 * obj.TaxRate) / 6.5 * 1.12, 2);
                    //单个SKU运费$=头程费用￥*单个SKU毛重
                    double Freight = Math.Round(Convert.ToDouble(obj.HeadloadCharges) * obj.WeightGross / 6.5, 2);
                    //UMAX价格($)=优胜价格$ + 单个SKU运费$
                    obj.PriceUMax = (obj.YouShengPrice + Freight).ToString();
                    obj.CreateOn = DateTime.Now;
                    obj.CreateBy = CurrentUser.Realname;
                    obj.Qty = obj.FirstQty;
                    //客人含佣金总价=UMAX价格($)*实际数量
                    obj.TotalPrice = Convert.ToDouble(obj.PriceUMax) * obj.Qty;
                    obj.UpdateTime = DateTime.Now;
                    NSession.Clear();
                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                }
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }
        public ActionResult Details(int id)
        {
            ShipmentsType byId = this.GetById(id);
            base.ViewData["id"] = id;
            return base.View(byId);
        }
        //计算义乌仓库还有多少库存，如果包装*箱数>库存-占用库存-已经创建出货明细的数量，不能创建
        public bool IsBeyondQty(ShipmentsType obj)
        {
            //义乌仓库库存
            string sql = "select Qty from WarehouseStock where SKU='{0}' and Warehouse='大富物流'";
            sql = string.Format(sql, obj.Sku);
            int stockqty = Convert.ToInt32(NSession.CreateSQLQuery(sql).UniqueResult());//库存数量
            ////占用库存
            //sql = "select sum(Qty) from OrderProducts where SKU='{0}' and IsQue=3";
            //sql = string.Format(sql, obj.Sku);
            //int UseQty = Convert.ToInt32(NSession.CreateSQLQuery(sql).UniqueResult());
            //已经创建出货清单的
            sql = "select sum(qty) from Shipments where Sku='{0}'and (ShipmentslistId =0 or  ShipmentslistId in (  select Id FROM Shipmentslist  where IsExa in('确认通过II','确认通过I')) ) ";
            sql = string.Format(sql, obj.Sku, obj.Id);
            int ShipmentsCount = Convert.ToInt32(NSession.CreateSQLQuery(sql).UniqueResult());
            //如果超出了，不能创建
            if (obj.Qty > (stockqty - ShipmentsCount))
            {
                return true;
            }
            return false; ;
        }
        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ShipmentsType GetById(int Id)
        {
            ShipmentsType obj = NSession.Get<ShipmentsType>(Id);
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
            ShipmentsType obj = GetById(id);
            return View(obj);
        }

        /// <summary>
        /// 增加编辑类型，是编辑还是复制
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]

        public ActionResult Edit(ShipmentsType obj, int type)
        {

            try
            {
                if (type == 0)//编辑
                {
                    if (IsCanEdit(obj.Id))
                    {
                        //如果修改了数量则需要判断是否超出
                        if (obj.FirstQty != obj.Qty)
                        {
                            if (IsBeyondQty(obj))
                            {
                                return Json(new { IsSuccess = false, Message = "请检查包装*箱数是否大于（实际库存-已经创建出货明细的数量）" });
                            }
                        }
                        else if (IsSameSKU(obj))
                        {
                            return Json(new { Issuccess = false, Message = "请检查是否已经创建相同SKU和外销合同的出货明细" }, "text/html", JsonRequestBehavior.AllowGet);
                        }
                        if (IsBeyondQty(obj))
                        {
                            return Json(new { IsSuccess = false, Message = "请检查包装*箱数是否大于（实际库存-已经创建出货明细的数量）" });
                        }
                        //单个SKU运费$=头程费用￥*单个SKU毛重
                        double Freight = Math.Round(Convert.ToDouble(obj.HeadloadCharges) * obj.WeightGross / 6.5, 2);
                        //UMAX价格($)=优胜价格$ + 单个SKU运费$
                        obj.PriceUMax = (obj.YouShengPrice + Freight).ToString();
                        //客人含佣金总价=UMAX价格($)*实际数量
                        obj.TotalPrice = Convert.ToDouble(obj.PriceUMax) * obj.Qty;
                        obj.UpdateTime = DateTime.Now;
                        obj.UpdateBy = GetCurrentAccount().Realname;
                        NSession.Clear();
                        NSession.Update(obj);
                        NSession.Flush();
                        return Json(new { IsSuccess = true });

                    }
                    else
                    {
                        return Json(new { IsSuccess = false, Message = "当前状态不能编辑" });

                    }
                }
                else//复制
                {
                    //判断实际数量是否超出,超出了不能复制
                    obj.Qty = obj.PackageNo * obj.Ctn;
                    if (IsBeyondQty(obj))
                    {
                        return Json(new { IsSuccess = false, Message = "请检查包装*箱数是否大于（实际库存-已经已经创建出货明细的数量）" });
                    }
                    //YWZZC仓库
                    double price = Convert.ToDouble(NSession.CreateSQLQuery("select top 1 Amount  from WarehouseStockData where  WName='YWZZC' and   NowQty>0  and SKU='" + obj.Sku + "'").UniqueResult());
                    //优胜价格$=（YWZZC商品价格-YWZZC商品价格/1.17*退税率）/汇率*1.12
                    obj.YouShengPrice = Math.Round((price - price / 1.17 * obj.TaxRate) / 6.5 * 1.12, 2);
                    //单个SKU运费$=头程费用￥*单个SKU毛重
                    double Freight = Math.Round(Convert.ToDouble(obj.HeadloadCharges) * obj.WeightGross / 6.5, 2);
                    //UMAX价格($)=优胜价格$ + 单个SKU运费$
                    obj.PriceUMax = (obj.YouShengPrice + Freight).ToString();
                    //客人含佣金总价=UMAX价格($)*实际数量
                    obj.TotalPrice = Convert.ToDouble(obj.PriceUMax) * obj.Qty;
                    obj.CreateOn = DateTime.Now;
                    obj.CreateBy = CurrentUser.Realname;
                    obj.UpdateTime = DateTime.Now;
                    obj.Id = 0;
                    obj.ShipmentslistId = 0;
                    NSession.Clear();
                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                    return Json(new { IsSuccess = true });
                }
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(string ids)
        {

            try
            {
                string[] strid = ids.Split(',');
                bool IsDelete = true;
                foreach (string id in strid)
                {
                    //如果有一条不能删除就全部不能删除
                    if (!IsCanEdit(Convert.ToInt32(id)))
                    {
                        IsDelete = false;

                    }
                }
                if (IsDelete)
                {
                    string sql = " from ShipmentsType where Id in({0})";
                    sql = string.Format(sql, ids);
                    NSession.Clear();
                    bool b = NSession.Delete(sql) > 0;
                    NSession.Flush();
                    return Json(new { IsSuccess = b });
                    //ShipmentsType obj = GetById(Convert.ToInt32(id));
                    //    NSession.Delete(obj);
                    //    NSession.Flush();
                    //    return Json(new { IsSuccess = false });

                }
                return Json(new { IsSuccess = false, Message = "当前选择的有一条或者多条所属状态不能被删除" });

            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string where = " ";
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
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<ShipmentsType>();
            foreach (ShipmentsType shipmentsType in objList)
            {
                //shipmentsType.Freight = Math.Round(Convert.ToDouble(shipmentsType.HeadloadCharges) / 6.5 * shipmentsType.WeightGross, 2);
                shipmentsType.YouShengtotal = Math.Round(Convert.ToDouble(shipmentsType.YouShengPrice) * shipmentsType.Qty, 2);
                shipmentsType.PriceFactorytotal = Math.Round(Convert.ToDouble(shipmentsType.PriceFactory) * shipmentsType.Qty, 2);
                shipmentsType.WeightGrosstotal = Math.Round(Convert.ToDouble(shipmentsType.WeightGross) * shipmentsType.Qty, 2);
                shipmentsType.WeightNettotal = Math.Round(Convert.ToDouble(shipmentsType.WeightNet) * shipmentsType.Qty, 2);
                shipmentsType.HeadloadCharges1 = Math.Round(Convert.ToDouble(shipmentsType.HeadloadCharges) * shipmentsType.WeightGross / 6.5, 2);

            }
            #region SUM值求取过程
            object SUM = NSession.CreateQuery("select    Round(SUM(cast(PriceUMax as float)),2)  from ShipmentsType " + where).UniqueResult();//UMAX价格($)
            object SUM1 = NSession.CreateQuery("select Round(SUM(Qty),2) from ShipmentsType " + where).UniqueResult();//实际数量
            object SUM2 = NSession.CreateQuery("select Round(SUM(cast(HeadloadCharges as float)),2) from ShipmentsType " + where).UniqueResult();//头程(元/公斤)
            object SUM3 = NSession.CreateQuery("select Round(SUM(cast(YouShengPrice as float)),2) from ShipmentsType " + where).UniqueResult();//优胜价格
            object SUM4 = NSession.CreateQuery("select Round(SUM(cast(TotalPrice as float)),2) from ShipmentsType " + where).UniqueResult();//客人含佣金总价
            object SUM5 = NSession.CreateQuery("select Round(SUM(cast(PackageNo as float)),2) from ShipmentsType " + where).UniqueResult();//包装数
            object SUM6 = NSession.CreateQuery("select Round(SUM(cast(Ctn as float)),2) from ShipmentsType " + where).UniqueResult();//箱数
            object SUM7 = NSession.CreateQuery("select Round(SUM(cast(WeightGross as float)),2) from ShipmentsType " + where).UniqueResult();//毛重（KG）  
            object SUM8 = NSession.CreateQuery("select Round(SUM(cast(WeightNet as float)),2) from ShipmentsType " + where).UniqueResult();//净重（KG）
            object SUM9 = NSession.CreateQuery("select Round(SUM(cast(TotalVolume as float)),2) from ShipmentsType " + where).UniqueResult();//总体积
            object SUM10 = NSession.CreateQuery("select Round(SUM(cast(PriceFactory as float)),2) from ShipmentsType " + where).UniqueResult();//工厂价格

            //总备货额
            object sum1 = NSession.CreateQuery("select Round(SUM(cast(PriceFactory as float)*Qty/6.5),2) from ShipmentsType ").UniqueResult();//总工厂价格=工厂价格/6.5*数量
            object sum2 = NSession.CreateQuery("select Round(SUM(cast(YouShengPrice as float)*Qty),2) from ShipmentsType  ").UniqueResult();//总优胜价格=优胜价格*数量
            object sum3 = NSession.CreateQuery("select Round(SUM(cast(TotalPrice as float)),2) from ShipmentsType ").UniqueResult();//总UMAX即客户佣金价格
            object sum4 = NSession.CreateQuery("select Round(SUM(cast(WeightGross as float)*Qty),2) from ShipmentsType  ").UniqueResult();//总毛重=毛重*数量
            object sum5 = NSession.CreateQuery("select Round(SUM(cast(WeightNet as float)*Qty),2) from ShipmentsType  ").UniqueResult();//总净重=净重*数量


            //已出货额
            object sum6 = NSession.CreateQuery("select Round(SUM(cast(PriceFactory as float)*Qty/6.5),2) from ShipmentsType where ShipmentslistId>0 ").UniqueResult();//总工厂价格=工厂价格/6.5*数量
            object sum7 = NSession.CreateQuery("select Round(SUM(cast(YouShengPrice as float)*Qty),2) from ShipmentsType where ShipmentslistId>0 ").UniqueResult();//总优胜价格=优胜价格*数量
            object sum8 = NSession.CreateQuery("select Round(SUM(cast(TotalPrice as float)),2) from ShipmentsType where ShipmentslistId>0 ").UniqueResult();//总UMAX即客户佣金价格
            object sum9 = NSession.CreateQuery("select Round(SUM(cast(WeightGross as float)*Qty),2) from ShipmentsType  where ShipmentslistId>0  ").UniqueResult();//总毛重=毛重*数量
            object sum10 = NSession.CreateQuery("select Round(SUM(cast(WeightNet as float)*Qty),2) from ShipmentsType  where ShipmentslistId>0  ").UniqueResult();//总净重=净重*数量


            //未出货额
            object sum11 = NSession.CreateQuery("select Round(SUM(cast(PriceFactory as float)*Qty/6.5),2) from ShipmentsType where ShipmentslistId=0 ").UniqueResult();//总工厂价格=工厂价格/6.5*数量
            object sum12 = NSession.CreateQuery("select Round(SUM(cast(YouShengPrice as float)*Qty),2) from ShipmentsType where ShipmentslistId=0 ").UniqueResult();//总优胜价格=优胜价格*数量
            object sum13 = NSession.CreateQuery("select Round(SUM(cast(TotalPrice as float)),2) from ShipmentsType where ShipmentslistId=0 ").UniqueResult();//总UMAX即客户佣金价格
            object sum14 = NSession.CreateQuery("select Round(SUM(cast(WeightGross as float)*Qty),2) from ShipmentsType  where ShipmentslistId=0  ").UniqueResult();//总毛重=毛重*数量
            object sum15 = NSession.CreateQuery("select Round(SUM(cast(WeightNet as float)*Qty),2) from ShipmentsType  where ShipmentslistId=0  ").UniqueResult();//总净重=净重*数量

            #endregion

            #region SUM空值处理
            if (SUM == null) { SUM = 0; }
            if (SUM1 == null) { SUM1 = 0; }
            if (SUM2 == null) { SUM2 = 0; }
            if (SUM3 == null) { SUM3 = 0; }
            if (SUM4 == null) { SUM4 = 0; }
            if (SUM5 == null) { SUM5 = 0; }
            if (SUM6 == null) { SUM6 = 0; }
            if (SUM7 == null) { SUM7 = 0; }
            if (SUM8 == null) { SUM8 = 0; }
            if (SUM9 == null) { SUM9 = 0; }
            if (SUM10 == null) { SUM10 = 0; }
            if (sum1 == null) { sum1 = 0; }
            if (sum2 == null) { sum2 = 0; }
            if (sum3 == null) { sum3 = 0; }
            if (sum4 == null) { sum4 = 0; }
            if (sum5 == null) { sum5 = 0; }
            if (sum6 == null) { sum6 = 0; }
            if (sum7 == null) { sum7 = 0; }
            if (sum8 == null) { sum8 = 0; }
            if (sum9 == null) { sum9 = 0; }
            if (sum10 == null) { sum10 = 0; }
            if (sum11 == null) { sum11 = 0; }
            if (sum12 == null) { sum12 = 0; }
            if (sum13 == null) { sum13 = 0; }
            if (sum14 == null) { sum14 = 0; }
            if (sum15 == null) { sum15 = 0; }
            #endregion

            List<ShipmentsType> list1 = new List<ShipmentsType> {
                new ShipmentsType{
                    Sku ="合计栏",
                    PriceUMax =(SUM.ToString()),//UMAX价格($)
                    Qty=Convert.ToDouble(SUM1.ToString()),//实际数量
                    HeadloadCharges=SUM2.ToString(),//头程（元/公斤）
                    YouShengPrice=Convert.ToDouble(SUM3.ToString()),//优胜价格
                    TotalPrice = Convert.ToDouble(SUM4.ToString()),//客人含佣金总价
                    PackageNo = Convert.ToInt32(SUM5.ToString()),//包装数
                    Ctn = Convert.ToInt32(SUM6.ToString()),//箱数
                    WeightGross = Convert.ToDouble(SUM7.ToString()),//毛重（KG）
                    WeightNet = Convert.ToDouble(SUM8.ToString()),//净重（KG）
                    TotalVolume = Convert.ToDouble(SUM9.ToString()),///总体积
                    PriceFactory = SUM10.ToString()//工厂价格
                }
                };
            list1.Add(new ShipmentsType
            {
                Sku = "总备货额",
                PriceFactorytotal = Convert.ToDouble(sum1.ToString()),//总备货额
                YouShengtotal = Convert.ToDouble(sum2.ToString()),//总优胜价格
                TotalPrice = Convert.ToDouble(sum3.ToString()),//总佣金
                WeightGrosstotal = Convert.ToDouble(sum4.ToString()),//总毛重
                WeightNettotal = Convert.ToDouble(sum5.ToString())//总净重
            });
            list1.Add(new ShipmentsType
            {
                Sku = "总已出货额",
                PriceFactorytotal = Convert.ToDouble(sum6.ToString()),//已出货额
                YouShengtotal = Convert.ToDouble(sum7.ToString()),//总优胜价格
                TotalPrice = Convert.ToDouble(sum8.ToString()),//总佣金
                WeightGrosstotal = Convert.ToDouble(sum9.ToString()),//总毛重
                WeightNettotal = Convert.ToDouble(sum10.ToString())//总净重
            });
            list1.Add(new ShipmentsType
            {
                Sku = "总未出货额",
                PriceFactorytotal = Convert.ToDouble(sum11.ToString()),//未出货额
                YouShengtotal = Convert.ToDouble(sum12.ToString()),//总优胜价格
                TotalPrice = Convert.ToDouble(sum13.ToString()),//总佣金
                WeightGrosstotal = Convert.ToDouble(sum14.ToString()),//总毛重
                WeightNettotal = Convert.ToDouble(sum15.ToString())//总净重
            });
            object count = NSession.CreateQuery("select count(Id) from ShipmentsType " + where).UniqueResult();
            return Json(new { total = count, rows = objList, footer = list1 });
        }
        public JsonResult CreateShipmentsList(string ShipmentsIds)
        {
            if (IsCanList(ShipmentsIds))
            {
                ShipmentslistType list = new ShipmentslistType();
                list.AppliBy = GetCurrentAccount().Realname;
                list.AppliTime = DateTime.Now;
                list.IsExa = Shipmentapproval.审核中.ToString();
                object o = NSession.Save(list);
                int id = Convert.ToInt32(o);
                string sql = "update ShipmentsType set ShipmentslistId={0} where Id in ({1})";
                sql = string.Format(sql, id, ShipmentsIds);
                IQuery query = NSession.CreateQuery(sql);
                bool result = query.ExecuteUpdate() > 0;
                return Json(new { IsSuccess = result });

            }
            else
            {
                return Json(new { IsSuccess = false });
            }
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

        /// <summary>
        /// 是否可以编辑和删除
        /// </summary>
        /// <param name="ShipmentId"></param>
        /// <returns></returns>
        public bool IsCanEdit(int ShipmentId)
        {
            string sql = " from ShipmentsType where (ShipmentslistId in (select id from ShipmentslistType where IsExa in ('" + Shipmentapproval.确认拒绝I + "','" + Shipmentapproval.确认拒绝II + "','" + Shipmentapproval.审核拒绝 + "') ) or ShipmentslistId=0) and Id={0}";
            sql = string.Format(sql, ShipmentId);
            List<ShipmentsType> shipments = NSession.CreateQuery(sql).List<ShipmentsType>().ToList();
            //如果是没有生成过清单或者是被拒绝的，就可以编辑和删除
            if (shipments != null && shipments.Count > 0)
            {
                return true;
            }
            return false;

        }
        /// <summary>
        /// 是否是相同的SKU和外销合同号
        /// </summary>
        /// <returns></returns>
        public bool IsSameSKU(ShipmentsType obj)
        {
            string sql = "select count(0) from Shipments where Sku='{0}' and ExportNo='{1}' and id<>{2} ";
            sql = string.Format(sql, obj.Sku, obj.ExportNo, obj.Id);
            int count = Convert.ToInt32(NSession.CreateSQLQuery(sql).UniqueResult());
            if (count > 0)
            {
                return true;
            }
            return false;
        }
    }
}
