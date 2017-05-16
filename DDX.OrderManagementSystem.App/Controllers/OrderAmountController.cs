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

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class OrderAmountController : BaseController
    {
        //update Orders set Freight=(Weight*0.0905+8)*0.85 where ScanningOn between '2014-01-01' and '2014-02-01'
        //update Orders set Freight=(Weight*0.0905+8)*0.8 where ScanningOn between '2014-02-01' and '2014-07-01'
        //update Orders set Freight=(Weight*0.0905+8)*0.78 where ScanningOn between '2014-07-01' and '2014-09-14'
        //update Orders set Freight=(Weight*0.0905+8)*0.85 where ScanningOn between '2014-09-14' and '2014-10-24'
        //update Orders set Freight=(Weight*0.0905+8)*0.82 where ScanningOn between '2014-10-24' and '2014-12-20'
        //update Orders set Freight=(Weight*0.0905+8)*0.8 where ScanningOn between '2014-10-20' and '2015-01-01'
        //update Orders set Freight=(Weight*0.0905+8)*0.78 where ScanningOn between '2015-01-01' and '2015-01-26'
        //update Orders set Freight=(Weight*0.0905+8)*0.8 where ScanningOn between '2015-01-26' and '2015-12-01'
        string sql = @"
update Orders set Weight=( select SUM(Weight*Qty) from OrderProducts left join Products On OrderProducts.SKU=Products.SKU where OrderProducts.OId=Orders.Id) where Status='已处理'
update Orders set  productFees= 
(select  Convert(decimal(18,2),isnull(SUM(OP.Qty*p.Price),0)) from OrderProducts OP left join Products  P On OP.SKU=p.SKU where OId=Orders.Id) where ScanningOn>'2015-12-01' and IsFBA=0



--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='宁波挂号' and ScanningOn between '2014-01-01' and '2014-02-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.8 where LogisticMode='宁波挂号' and ScanningOn between '2014-02-01' and '2014-07-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.78 where LogisticMode='宁波挂号' and ScanningOn between '2014-07-01' and '2014-09-14'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='宁波挂号' and ScanningOn between '2014-09-14' and '2014-10-24'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.82 where LogisticMode='宁波挂号' and ScanningOn between '2014-10-24' and '2014-12-20'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.8 where LogisticMode='宁波挂号' and ScanningOn between '2014-10-20' and '2015-01-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.78 where LogisticMode='宁波挂号' and ScanningOn between '2015-01-01' and '2015-01-26'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.8 where LogisticMode='宁波挂号' and ScanningOn between '2015-01-26' and '2015-09-01'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.9 where LogisticMode='宁波挂号' and ScanningOn between '2015-09-01' and '2015-12-01'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=44)))+8)where LogisticMode='挂号线上发货' and ScanningOn > '2015-12-01' 

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=44)))+8)where LogisticMode='挂号线上发货' and ScanningOn > '2015-12-01' 

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=45)))) where LogisticMode='平邮线上发货' and ScanningOn > '2015-12-01'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='荷兰挂号小包'

update Orders set 
Freight=(Weight*
(select top 1 EveryFee from LogisticsFreight where AreaCode in
 (select AreaCode from LogisticsAreaCountry where
  CountryCode in (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=17)))+
  (select top 1 ProcessingFee from LogisticsFreight where AreaCode in 
  (select AreaCode from LogisticsAreaCountry where CountryCode in 
  (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=17))))
where LogisticMode='荷兰挂号宁波' 

update Orders set 
Freight=(Weight*
(select top 1 EveryFee from LogisticsFreight where AreaCode in
 (select AreaCode from LogisticsAreaCountry where
  CountryCode in (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=31)))+
  (select top 1 ProcessingFee from LogisticsFreight where AreaCode in 
  (select AreaCode from LogisticsAreaCountry where CountryCode in 
  (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=31))))
where LogisticMode='义乌俄罗斯挂号小包（黑）' 

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='扬州小包' and ScanningOn between '2015-09-01' and '2015-12-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='福建小包' and ScanningOn between '2015-05-01' and '2015-12-01'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.88 where LogisticMode='金华邮政小包（王）'
--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.81 where LogisticMode='邮政普货小包（商）' and ScanningOn < '2015-03-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.8 where LogisticMode='邮政普货小包（商）' and ScanningOn > '2015-03-01'

--update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.8 where LogisticMode='邮政普货小包（商）' and ScanningOn > '2015-08-01'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8)*0.85 where LogisticMode='邮政普货小包（商）' and ScanningOn > '2015-08-24'

update Orders set Freight=(Weight*0.085+8) where LogisticMode='义乌邮政俄速通（哈尔滨）'
update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8) where LogisticMode='义乌邮政电子小包（北京）'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1)))+8) where LogisticMode='盐城小包'

update Orders set 
Freight=(Weight*
(select top 1 EveryFee from LogisticsFreight where AreaCode in
 (select AreaCode from LogisticsAreaCountry where
  CountryCode in (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=34)))+
  (select top 1 ProcessingFee from LogisticsFreight where AreaCode in 
  (select AreaCode from LogisticsAreaCountry where CountryCode in 
  (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=34))))
where LogisticMode='比利时邮政' 

update Orders set 
Freight=(Weight*
(select top 1 EveryFee from LogisticsFreight where AreaCode in
 (select AreaCode from LogisticsAreaCountry where
  CountryCode in (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=32)))+
  (select top 1 ProcessingFee from LogisticsFreight where AreaCode in 
  (select AreaCode from LogisticsAreaCountry where CountryCode in 
  (select Id from Country where ECountry=Orders.Country) 
  and AreaCode in (select Id  from LogisticsArea where LId=32))))
where LogisticMode='CNE全球特惠' 

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=1))))*1.03+9 where LogisticMode='北京小包'  and ScanningOn > '2015-09-14'

update Orders set Freight=(Weight*(select EveryFee from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=14)))+8) where LogisticMode='新加坡小包'

update Orders set Freight=((Weight-50)*(select IncrementFreight from LogisticsFreight where AreaCode in (select AreaCode from LogisticsAreaCountry where CountryCode in(select Id from Country where ECountry=Orders.Country) and AreaCode in (select Id  from LogisticsArea where LId=35)))+13)*0.85 where LogisticMode='成都小包'

update Orders set Freight=(Weight*0.0902) where LogisticMode='线下e邮宝'
update Orders set Freight=50  where LogisticMode='日本专线' and Status='已发货' and Weight<=500
update Orders set Freight=50+15*(CEILING(Weight/500)) where LogisticMode='日本专线' and Status='已发货' and Weight>500

update Orders set Freight=(Weight)*0.065+17.5 where LogisticMode='全球通小包' and Status='已发货'
update Orders set Freight=(Weight)*0.05+28 where LogisticMode='UK48' and Status='已发货' 
--update Orders set Freight=(Weight)*0.038+28 where LogisticMode='CNE全球特惠' and Status='已发货' 
update Orders set Freight=14+(ceiling(CAST(Weight as float)/100.0)-1)*4.5 where LogisticMode='欧亚速运'  and Weight<=500
  update Orders set Freight=37+(ceiling(CAST(Weight as float)/100.0)-6)*4 where LogisticMode='欧亚速运'  and Weight>500

 Update Orders set ProductFees=0 ,Freight=0 where IsXu=1
--Update Orders set profit= Convert(decimal(18,2),Amount*6.2*0.95-Freight-productFees) where ScanningOn<'2015-03-05' and Platform='Aliexpress' and CurrencyCode ='USD' 
Update Orders set profit= Convert(decimal(18,2),Amount*6.2*0.95-isnull(Freight,0)-productFees) where ScanningOn>='2014-03-05' and Platform='Aliexpress' and CurrencyCode ='USD' 

Update Orders set profit= Convert(decimal(18,2),Amount*6.2*0.85-isnull(Freight,0)-productFees) where ScanningOn>='2014-01-05' and Platform<>'Aliexpress' and CurrencyCode ='USD' 
Update Orders set profit= Convert(decimal(18,2),Amount*0.05*0.85-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='JPY' 
Update Orders set profit= Convert(decimal(18,2),Amount*9.43*0.85-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='GBP' 
Update Orders set profit= Convert(decimal(18,2),Amount*7.33*0.85-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='EUR' 
Update Orders set profit= Convert(decimal(18,2),FanAmount*6.2-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform='Aliexpress' and CurrencyCode ='USD'  and FanState=1
Update Orders set profit= Convert(decimal(18,2),FanAmount*6.2-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='USD' and FanState=1
Update Orders set profit= Convert(decimal(18,2),FanAmount*0.05-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='JPY' and FanState=1
Update Orders set profit= Convert(decimal(18,2),FanAmount*9.43-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='GBP' and FanState=1
Update Orders set profit= Convert(decimal(18,2),FanAmount*7.33-isnull(Freight,0)-productFees) where ScanningOn>='2015-03-05' and Platform<>'Aliexpress' and CurrencyCode ='EUR' and FanState=1
";
        public ViewResult Index()
        {
            //if (DateTime.Now.Hour < 12)
            //{
            //    NSession.CreateSQLQuery(sql).ExecuteUpdate();
            //}

            return View();
        }
        public ViewResult Ningbo()
        {

            //if (DateTime.Now.Hour < 12)
            //{
            //    NSession.CreateSQLQuery(sql).ExecuteUpdate();
            //}
            //NSession.CreateSQLQuery(sql).ExecuteUpdate();
            return View();
        }

        public ActionResult GetPeiKuan(string id)
        {
            ViewData["OrderNo"] = id;
            return View();
        }
        public ViewResult Yiwu()
        {
            //盈利明细表月底使用较频繁
            //if (DateTime.Now.Hour < 12)
            //{
            //    NSession.CreateSQLQuery(sql).ExecuteUpdate();
            //}
           // NSession.CreateSQLQuery(sql).ExecuteUpdate();
            if (GetCurrentAccount().Username == "admin")
            {
                NSession.CreateSQLQuery(sql).ExecuteUpdate();
            }
            return View();
        }
        public ViewResult StockIndex()
        {
            return View();
        }
        public ViewResult FreightIndex()
        {
            return View();
        }
        public ViewResult FreightCount()
        {
            return View();
        }

        public ViewResult OrderInfo()
        {

            return View();
        }

        public ViewResult OrderInfo2()
        {

            return View();
        }
        public ViewResult OrderInfo3()
        {

            return View();
        }
        public ViewResult GetFreightStatus()
        {

            return View();
        }
        [HttpGet]
        public ActionResult GetOrders(string Id)
        {
            ViewData["Id"] = Id;
            return View();
        }
        [HttpGet]
        public ActionResult GetFreights(string Id)
        {
            ViewData["Id"] = Id;
            return View();
        }
        [HttpGet]
        public ActionResult GetProducts(string Id)
        {
            ViewData["Id"] = Id;
            return View();
        }
        [HttpGet]
        public ActionResult GetProducts2(string Id)
        {
            ViewData["Id"] = Id;
            return View();
        }

        public ActionResult StockList(int page, int rows, string sort, string order, string search)
        {
            string where = "";
            string orderby = Utilities.OrdeerBy(sort, order);
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            IList<object[]> objList = NSession.CreateSQLQuery("select SKU,OldSKU,ProductName as Title,Price,(select COUNT(1) from SKUCode where IsOut=0 and SKU=P.SKU ) as Qty,(Price*(select COUNT(1) from SKUCode where IsOut=0 and SKU=P.SKU )) as TotalPrice from Products P " + where + " " + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<object[]>();
            List<ProductData> list = new List<ProductData>();
            foreach (object[] objectse in objList)
            {
                ProductData pd = new ProductData();
                pd.SKU = objectse[0].ToString();
                pd.Title = objectse[2].ToString();
                pd.Price = Convert.ToDouble(objectse[3]).ToString();
                pd.Qty = Convert.ToInt32(objectse[4]);
                pd.TotalPrice = Math.Round(Convert.ToDouble(objectse[5]), 2);

                list.Add(pd);
            }
            object count = NSession.CreateQuery("select count(Id) from ProductType " + where).UniqueResult();
            object total = NSession.CreateSQLQuery("select SUM(qty) from (select (Price*(select COUNT(1) from SKUCode where IsOut=0 and SKU=P.SKU)) as Qty from Products P " + where + ") as t").UniqueResult();
            List<object> footers = new List<object>();
            footers.Add(new { TotalPrice = Math.Round(Convert.ToDouble(total), 2), SKU = "合计：" });
            return Json(new { total = count, rows = list, footer = footers });
        }

        public ActionResult ResetFreight(string search, decimal z)
        {
            string orderby;
            var where = GetWhere(null, null, search, out @orderby);
            IList<OrderType> objList = NSession.CreateQuery("from OrderType " + where + orderby)
                .List<OrderType>();
            foreach (OrderType orderType in objList)
            {
                double d = 0;
                orderType.Freight = Convert.ToDouble(OrderHelper.GetFreight(orderType.Weight, orderType.LogisticMode, orderType.Country, NSession, z));
                NSession.SaveOrUpdate(orderType);
                NSession.Flush();
                OrderHelper.SaveAmount(orderType, NSession);
            }
            return Json(new { IsSuccess = true });
        }

        public ActionResult ExportStockList()
        {
            string sql =
                "select SKU,OldSKU,ProductName as Title,Price,(select COUNT(1) from SKUCode where IsOut=0 and SKU=P.SKU ) as Qty,(Price*(select COUNT(1) from SKUCode where IsOut=0 and SKU=P.SKU )) as TotalPrice from Products P";
            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql;
            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);
            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        public ActionResult GetFreightSumList(string search, int isa, int isl)
        {
            string orderby;
            var where = GetWhere("", "", search, out @orderby);
            string[] strs = new string[] { "", "", "", "" };
            string sql =
                "select COUNT(1) as count,SUM(Freight) as total {0} {1} from Orders where CreateOn between '2013-03-01' and '2013-04-01' and Status='已发货' group by {2} {3}";
            if (isa == 1)
            {
                strs[0] = ",platform,account";
                strs[2] = "platform,account";
            }
            if (isl == 1)
            {
                strs[1] = ",LogisticMode ";
                if (isa == 1)
                    strs[3] = ",LogisticMode";
                else
                    strs[3] = " LogisticMode";
            }
            sql = string.Format(sql, strs[0], strs[1], strs[2], strs[3]);
            IList<object[]> objList = NSession.CreateSQLQuery(sql)
                .List<object[]>();
            List<OrderData> os = new List<OrderData>();

            object count = NSession.CreateQuery("select count(Id) from OrderType " + where).UniqueResult();

            return Json(new { total = count, rows = os });
        }

        [HttpPost]
        public ActionResult GetFreightList(int page, int rows, string sort, string order, string search)
        {
            string orderby;
            var where = GetWhere(sort, order, search, out @orderby);
            IList<OrderType> objList = NSession.CreateQuery("from OrderType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<OrderType>();
            List<OrderData> os = new List<OrderData>();
            foreach (var o in objList)
            {
                AddToOrderData(o, os);
            }
            object count = NSession.CreateQuery("select count(Id) from OrderType " + where).UniqueResult();
            return Json(new { total = count, rows = os });
        }

        private static string GetWhere(string sort, string order, string search, out string @orderby)
        {
            string where = "";
            @orderby = " order by Id desc";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                @orderby = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                @where = Utilities.Resolve(search);
                if (@where.Length > 0)
                {
                    @where = " where Enabled=1 and  Status <> '待处理' and " + @where;
                }
            }
            if (@where.Length == 0)
            {
                @where = " where Enabled=1 and  Status <> '待处理'";
            }
            return @where;
        }

        private static void AddToOrderData(OrderType order, List<OrderData> os)
        {
            OrderData o = new OrderData();

            if (order != null)
            {
                o.OrderNo = order.OrderNo;
                o.OrderExNo = order.OrderExNo;
                o.TrackCode = order.TrackCode;
                o.Weight = order.Weight;
                o.RMB = order.RMB;
                o.Country = order.Country;
                o.CurrencyCode = order.CurrencyCode;
                o.LogisticMode = order.LogisticMode;
                o.OrderAmount = order.Amount;

                o.Status = order.Status;
                o.OrderType = "正常";
                if (order.IsRepeat == 1)
                    o.OrderType = "重发";
                if (order.IsSplit == 1)
                    o.OrderType += "拆包";
                o.Country = order.Country;
                o.SendOn = order.ScanningOn;
                o.Freight = order.Freight;
                o.Account = order.Account;
                o.Platform = order.Platform;
            }
            os.Add(o);
        }


        /// <summary>
        /// 订单表
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetOrderList(string Id)
        {
            IList<OrderAmountType> amountlist = NSession.CreateQuery("from OrderAmountType where OId=" + Id + " or  OId in(select Id  from OrderType where MId=" + Id + ")").List<OrderAmountType>();
            List<OrderType> orderList = NSession.CreateQuery("from OrderType where  MId=" + Id + " or Id=" + Id).List<OrderType>().ToList();
            List<OrderData> os = new List<OrderData>();
            foreach (var orderAmountType in amountlist)
            {
                OrderType order = orderList.Find(x => x.Id == orderAmountType.OId);
                AddToOrderData(order, os);
                os[os.Count - 1].TotalCost = orderAmountType.TotalCosts;
            }
            return Json(new { total = os.Count, rows = os });

        }

        /// <summary>
        /// 订单产品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetProductList(string Id)
        {
            IList<OrderProductType> list = NSession.CreateQuery("from OrderProductType where OId=" + Id).List<OrderProductType>();
            string ids = "";
            foreach (var orderProductType in list)
            {
                ids += orderProductType.SKU + ",";
            }
            if (ids.Length > 0)
            {
                ids = ids.Trim(',');
            }
            List<ProductType> products =
                NSession.CreateQuery("from ProductType where SKU in ('" + ids.Replace(",", "','") + "')").List<ProductType>().ToList();
            List<ProductData> ps = new List<ProductData>();
            double total = 0;
            foreach (var orderProductType in list)
            {
                ProductData p = new ProductData();
                p.SKU = orderProductType.SKU;
                p.Qty = orderProductType.Qty;
                ProductType product = products.Find(x => x.SKU.Trim().ToUpper() == orderProductType.SKU.Trim().ToUpper());
                if (product != null)
                {
                    p.Standard = orderProductType.Standard;
                    p.Status = product.Status;
                    p.Title = product.ProductName;
                    p.Price = product.Price.ToString();
                    p.Weight = product.Weight.ToString();
                    p.PicUrl = product.PicUrl;

                    p.TotalWeight = Utilities.ToDouble(product.Weight) * p.Qty;
                    p.TotalPrice = Utilities.ToDouble(p.Price) * p.Qty;
                    total += p.TotalPrice;
                }
                ps.Add(p);
            }
            List<object> footers = new List<object>();
            footers.Add(new { TotalPrice = total });
            return Json(new { total = ps.Count, rows = ps, footer = footers });
        }

        public JsonResult GetProduct(string o)
        {
            IList<OrderProductType> list = NSession.CreateQuery("from OrderProductType where OId=" + o).List<OrderProductType>();


            return Json(new { total = list.Count, rows = list });
        }

        public JsonResult GetOrderRecord(string o)
        {
            IList<OrderType> list = NSession.CreateQuery("from OrderType where MId=" + o).List<OrderType>();
            return Json(new { total = list.Count, rows = list });
        }
        public JsonResult GetOrderRecordAmount(string o)
        {
            IList<OrderAmountType> list = NSession.CreateQuery("from OrderAmountType where OId in( select Id  from OrderType where MId=" + o + ")").List<OrderAmountType>();
            return Json(new { total = list.Count, rows = list });
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(OrderAmountType obj)
        {
            try
            {
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
        public OrderAmountType GetById(int Id)
        {
            OrderAmountType obj = NSession.Get<OrderAmountType>(Id);
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
            OrderAmountType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(OrderAmountType obj)
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
                OrderAmountType obj = GetById(id);
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
            string orderby = Utilities.OrdeerBy(sort, order);
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where IsSplit=0 and IsRepeat=0 and " + where;
                }
                else
                {
                    where = " where IsSplit=0 and IsRepeat=0 ";
                }
            }
            IList<OrderAmountType> objList = NSession.CreateQuery("from OrderAmountType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<OrderAmountType>();
            object count = NSession.CreateQuery("select count(Id) from OrderAmountType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        [HttpPost]
        public ActionResult AccountFreightCount(DateTime st, DateTime et, string p, string a, string t)
        {
            IList<AccountFreigheCount> sores = new List<AccountFreigheCount>();
            string where = Where(st, et, p, a, t);
            IList<object[]> objectses = NSession.CreateQuery("select Platform,Account,Count(Id),Sum(Freight) from OrderType " + where + "  group by Account,Platform").List<object[]>();
            decimal sum = 0;
            decimal freight = 0;
            foreach (var item in objectses)
            {
                string pp = item[0].ToString();
                string aa = item[1].ToString();
                decimal co = Convert.ToDecimal(item[2]);
                decimal am = decimal.Round(decimal.Parse(item[3].ToString()), 2);
                sores.Add(new AccountFreigheCount { Platform = pp, Account = aa, Count = co, Amount = am });
                sum += co;
                freight += am;
            }
            List<object> footers = new List<object>();
            footers.Add(new { Count = sum, Amount = decimal.Round(freight, 2) });
            return Json(new { rows = sores.OrderByDescending(x => x.Amount), footer = footers, total = sores.Count });
        }
        public ActionResult TypeFreightCount(DateTime st, DateTime et, string p, string a, string t)
        {
            IList<AccountFreigheCount> sores = new List<AccountFreigheCount>();
            string where = Where(st, et, p, a, t);
            IList<object[]> objectses = NSession.CreateQuery("select IsRepeat,IsSplit,Count(Id),Sum(Freight) from OrderType " + where + "  group by IsRepeat,IsSplit").List<object[]>();
            decimal sum = 0;
            decimal freight = 0;
            foreach (var item in objectses)
            {
                string str = "正常";
                if (item[0].ToString() == "1")
                {
                    str = "重发";
                }
                if (item[1].ToString() == "1")
                {
                    str += "拆包";
                }
                string aa = str;
                decimal co = Convert.ToDecimal(item[2]);
                decimal am = decimal.Round(decimal.Parse(item[3].ToString()), 2);
                sores.Add(new AccountFreigheCount { Account = aa, Count = co, Amount = am });
                sum += co;
                freight += am;
            }
            List<object> footers = new List<object>();
            footers.Add(new { Count = sum, Amount = decimal.Round(freight, 2) });
            return Json(new { rows = sores.OrderByDescending(x => x.Amount), footer = footers, total = sores.Count });
        }

        public string Where(DateTime st, DateTime et, string p, string a, string t)
        {
            string where = "where Status='已发货' and CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59'";
            if (p != "ALL")
            {
                where += " and Platform='" + p + "'";
            }
            if (a != "ALL")
            {
                where += " and Account='" + a + "'";
            }
            if (t != "ALL")
            {
                if (t == "正常")
                {
                    where += " and IsRepeat=0 ";
                }
                if (t == "重发")
                {
                    where += " and IsRepeat=1 ";
                }
            }
            return where;
        }
        public JsonResult ToExcel(string search)
        {
            try
            {
                string sql = "select Account as '账户',OrderNo as '订单编号',OrderExNo as '平台订单号',TrackCode as '运单号',CurrencyCode as '货币',Amount as '订单金额',LogisticMode as '发货方式',Country  as '国家',Weight  as '重量',ProductFees as '商品成本',Freight as '运费',Profit as '利润', (case when Amount=0 then 0 else ROUND(Profit/Amount/6.1*100,2)  end) as '毛利率',CreateOn as '导入时间',ScanningOn as '发货时间',IsXu as '虚假' from Orders ";
                sql += Utilities.SqlWhere(search);
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        public JsonResult ToExcelfeight(string search,int v)
        {
            try
            {
                string sql = "";
                if (v == 0)
                {
                    sql = "select a.Account as '账户',OrderNo as '订单编号',OrderExNo as '平台订单号',TrackCode as '运单号',a.CurrencyCode as '货币',round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5),2)  as '订单金额RMB',case when CHARINDEX(a.Account, 'smt') > 0 then round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5*0.05),2) else round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5*0.15),2) end  as '平台费用RMB',LogisticMode as '发货方式',Country  as '国家',Weight  as '重量',ProductFees as '商品成本',Freight as '运费',(round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5),2)-ProductFees-Freight)  as '利润', (case when Amount=0 then 0 else ROUND((round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5),2)-ProductFees-Freight) /(round((a.Amount*b.CurrencyValue/c.CurrencyValue*6.5),2))*100,2)  end) as '毛利率',a.CreateOn as '导入时间',ScanningOn as '发货时间',IsXu as '虚假' from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) ";
                    sql += Utilities.SqlWhere(search);  
                }
                else
                {
                sql = "select a.Account as '账户',a.OrderNo as '订单编号',a.OrderExNo as '平台订单号',a.CreateOn as '导入时间',ScanningOn as '发货时间', (select d.ExamineAmountRmb from DisputeRecordType d where a.OrderExNo =d.OrderNo ) as '赔款金额RMB' ,(select d.CreateOn from DisputeRecordType d where a.OrderExNo =d.OrderNo ) as '赔款创建时间'from  Orders a ";
                sql += Utilities.SqlWhere(search) ;
                }
                
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession," ").Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        /// <summary>
        /// 赔款明细下载
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public JsonResult PaymentDetailsToExcel(string search)
        {
            try
            {
                string sql = "select Area as 地区,Account as 帐户,ExamineClass as 纠纷类型,DisputeState as 纠纷状态,OrderNo as 平台订单号,ExamineAmount as 赔款外币, examinecurrencycode as 货币类型,ExamineAmountRmb as 赔款人民币, ExamineOn as 处理时间,CreateOn as 创建时间 from DisputeRecordType ";
                sql += Utilities.SqlWhere(search) + " and (ExamineStatus=4 or ExamineStatus=6) and ExamineAmountRmb<>0 ";
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        public JsonResult ToExcel1(string search)
        {
            try
            {
                string where = "";
                if (!string.IsNullOrEmpty(search))
                {
                    where = Utilities.Resolve(search);
                    if (where.Length > 0)
                    {
                        where = " where IsSplit=0 and IsRepeat=0 and " + where;
                    }
                    else
                    {
                        where = " where IsSplit=0 and IsRepeat=0 ";
                    }
                }
                IList<OrderAmountType> objList = NSession.CreateQuery("from OrderAmountType " + where)
                    .List<OrderAmountType>();

                List<OrderProductType> objList2 = NSession.CreateQuery("from OrderProductType where OId in (select OId from OrderAmountType " + where + " ) ")
                    .List<OrderProductType>().ToList();

                DataTable dt = Utilities.FillDataTable((objList));
                dt.Columns.Add("产品信息");
                foreach (DataRow dr in dt.Rows)
                {
                    List<OrderProductType> foos = objList2.FindAll(p => p.OId == Convert.ToInt32(dr["OId"]));
                    StringBuilder sb = new StringBuilder();
                    foreach (OrderProductType foo in foos)
                    {
                        sb.AppendLine("SKU:" + foo.SKU + "    Qty:" + foo.Qty + "    Price:" + foo.Price);
                    }
                    dr["产品信息"] = sb.ToString();
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(dt);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        public JsonResult ToExcelAccount(DateTime st, DateTime et, string p, string a, string t)
        {
            try
            {
                IList<AccountFreigheCount> sores = new List<AccountFreigheCount>();
                string where = Where(st, et, p, a, t);
                IList<object[]> objectses = NSession.CreateQuery("select Platform,Account,Count(Id),Sum(Freight) from OrderType " + where + "  group by Account,Platform").List<object[]>();
                foreach (var item in objectses)
                {
                    string pp = item[0].ToString();
                    string aa = item[1].ToString();
                    decimal co = Convert.ToDecimal(item[2]);
                    decimal am = decimal.Round(decimal.Parse(item[3].ToString()), 2);
                    sores.Add(new AccountFreigheCount { Platform = pp, Account = aa, Count = co, Amount = am });
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((sores)));
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        public JsonResult ToExcelType(DateTime st, DateTime et, string p, string a, string t)
        {
            try
            {
                IList<AccountFreigheCount> sores = new List<AccountFreigheCount>();
                string where = Where(st, et, p, a, t);
                IList<object[]> objectses = NSession.CreateQuery("select IsRepeat,IsSplit,Count(Id),Sum(Freight) from OrderType " + where + "  group by IsRepeat,IsSplit").List<object[]>();
                foreach (var item in objectses)
                {
                    string str = "正常";
                    if (item[0].ToString() == "1")
                    {
                        str = "重发";
                    }
                    if (item[1].ToString() == "1")
                    {
                        str += "拆包";
                    }
                    string aa = str;
                    decimal co = Convert.ToDecimal(item[2]);
                    decimal am = decimal.Round(decimal.Parse(item[3].ToString()), 2);
                    sores.Add(new AccountFreigheCount { Account = aa, Count = co, Amount = am });
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((sores)));
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }

            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });



        }
     



    }
}

