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
    public class PurchasePlanController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }
        public ViewResult BIndex()
        {
            return View();
        }
        public ViewResult CIndex()
        {
            return View();
        }
        public ViewResult Import()
        {
            return View();
        }
        public ViewResult ProfitIndex()
        {
            return View();
        }

        public ViewResult Details(int Id)
        {
            ViewData["Id"] = Id;
            return View();
        }



        public ActionResult CreateByW(string Id)
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            object sum = NSession.CreateSQLQuery("select sum(Qty) from Orders O left join OrderProducts OP On O.Id=Op.OId  where Status in ('已处理','已发货') and SKU='" + Id + "' and DATEDIFF( DAY,CreateOn,GETDATE())<60").UniqueResult();
            // 由20天限制调整提高到60天

            ViewData["Qty"] = sum;
            if (sum == null)
                ViewData["Qty"] = 0;
            IList<PurchasePlanType> obj =
                  NSession.CreateQuery("from PurchasePlanType  where SKU=:sku order by Id desc").SetString("sku", Id)
                      .SetFirstResult(0)
                      .SetMaxResults(1).List<PurchasePlanType>();
            if (obj.Count > 0)
            {
                object min = NSession.CreateQuery("select Min(Price) from PurchasePlanType where SKU='" + obj[0].SKU + "' ").UniqueResult();

                obj[0].Price = Utilities.ToDouble(min);
                obj[0].Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + obj[0].SKU + "'")).UniqueResult());
                return View(obj[0]);
            }
            PurchasePlanType p = new PurchasePlanType();
            p.SKU = Id;

            p.CreateOn = p.SendOn = p.ExpectReceiveOn = p.ReceiveOn = p.BuyOn = p.MinDate = p.MinValiDate = DateTime.Now;
            //【缺货采购】数据由最开始订单进来的时候已经全部填完整 add by luoyunqing
            List<ProductType> productlist = NSession.CreateQuery(" From ProductType where sku='" + Id + "'").List<ProductType>().ToList();
            ProductType product = new ProductType();
            if (productlist != null && productlist.Count > 0)
            {
                product = productlist[0];
            }
            //商品名称
            p.ProductName = product.ProductName;
            //供应商
            p.Suppliers = product.Suppliers;
            //产品链接
            p.ProductUrl = product.Caigou1;
            //来源
            p.FromTo = product.FromTo;
            //产品图片
            p.PicUrl = product.PicUrl;
            p.Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + p.SKU + "'")).UniqueResult());
            return View(p);
        }

        public ActionResult CreateByX(string Id)
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            ViewData["area"] = NSession.CreateSQLQuery(
             " select FromArea  from Users where Realname ='" + GetCurrentAccount().Realname +
             "'").UniqueResult();

            object sum = NSession.CreateSQLQuery("select sum(Qty) from Orders O left join OrderProducts OP On O.Id=Op.OId  where Status in ('已处理','已发货') and SKU='" + Id + "' and DATEDIFF( DAY,CreateOn,GETDATE())<60").UniqueResult();
            // 由20天限制调整提高到60天

            ViewData["Qty"] = sum;
            if (sum == null)
                ViewData["Qty"] = 0;
            IList<PurchasePlanType> obj =
                  NSession.CreateQuery("from PurchasePlanType  where SKU=:sku order by Id desc").SetString("sku", Id)
                      .SetFirstResult(0)
                      .SetMaxResults(1).List<PurchasePlanType>();
            if (obj.Count > 0)
            {
                object min = NSession.CreateQuery("select Min(Price) from PurchasePlanType where SKU='" + obj[0].SKU + "' ").UniqueResult();

                obj[0].Price = Utilities.ToDouble(min);
                obj[0].Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + obj[0].SKU + "'")).UniqueResult());
                return View(obj[0]);
            }
            PurchasePlanType p = new PurchasePlanType();
            p.SKU = Id;

            p.CreateOn = p.SendOn = p.ExpectReceiveOn = p.ReceiveOn = p.BuyOn = p.MinDate = p.MinValiDate = DateTime.Now;
            //【缺货采购】数据由最开始订单进来的时候已经全部填完整 add by luoyunqing
            List<ProductType> productlist = NSession.CreateQuery(" From ProductType where sku='" + Id + "'").List<ProductType>().ToList();
            ProductType product = new ProductType();
            if (productlist != null && productlist.Count > 0)
            {
                product = productlist[0];
            }
            //商品名称
            p.ProductName = product.ProductName;
            //供应商
            p.Suppliers = product.Suppliers;
            //产品链接
            p.ProductUrl = product.Caigou1;
            //来源
            p.FromTo = product.FromTo;
            p.Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + p.SKU + "'")).UniqueResult());
            return View(p);
        }

        public ActionResult CreateByWBulk(string Id)
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            object sum = NSession.CreateSQLQuery("select sum(Qty) from Orders O left join OrderProducts OP On O.Id=Op.OId  where Status in ('已处理','已发货') and SKU='" + Id + "' and DATEDIFF( DAY,CreateOn,GETDATE())<60").UniqueResult();

            ViewData["Qty"] = sum;
            if (sum == null)
                ViewData["Qty"] = 0;
            IList<PurchasePlanType> obj =
                  NSession.CreateQuery("from PurchasePlanType  where SKU=:sku order by Id desc").SetString("sku", Id)
                      .SetFirstResult(0)
                      .SetMaxResults(1).List<PurchasePlanType>();
            if (obj.Count > 0)
            {
                object min = NSession.CreateQuery("select Min(Price) from PurchasePlanType where SKU='" + obj[0].SKU + "' ").UniqueResult();

                obj[0].Price = Utilities.ToDouble(min);
                obj[0].Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + obj[0].SKU + "'")).UniqueResult());
                return View(obj[0]);
            }
            PurchasePlanType p = new PurchasePlanType();
            p.SKU = Id;

            p.CreateOn = p.SendOn = p.ExpectReceiveOn = p.ReceiveOn = p.BuyOn = p.MinDate = p.MinValiDate = DateTime.Now;
            p.Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + p.SKU + "'")).UniqueResult());
            List<ProductType> productTypes =
        NSession.CreateQuery("from ProductType where SKU ='" + Id + "'").List<ProductType>().ToList();
            if (productTypes.Count > 0)
            {
                p.ProductUrl = productTypes[0].Caigou1;
            }
            return View(p);
        }

        [HttpPost]
        public ActionResult ImportPlan(string fileName)
        {
            DataTable dt = OrderHelper.GetDataTable(fileName);
            IList<WarehouseType> list = NSession.CreateQuery(" from WarehouseType").List<WarehouseType>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                PurchasePlanType p = new PurchasePlanType { CreateOn = DateTime.Now, BuyOn = DateTime.Now, ReceiveOn = DateTime.Now, SendOn = DateTime.Now, MinDate = DateTime.Now, MinValiDate = DateTime.Now };
                p.PlanNo = Utilities.GetNo(NSession, Utilities.PlanNo);
                p.SKU = dt.Rows[i]["SKU"].ToString();
                p.Price = Convert.ToDouble(dt.Rows[i]["单价"].ToString());
                p.Qty = Convert.ToInt32(dt.Rows[i]["Qty"].ToString());
                p.DaoQty = 0;
                p.ProductName = "";
                p.Freight = Convert.ToDouble(dt.Rows[i]["运费"].ToString());
                p.ProductUrl = dt.Rows[i]["产品链接"].ToString();
                p.PicUrl = dt.Rows[i]["图片链接"].ToString();
                p.Suppliers = dt.Rows[i]["供应商"].ToString();
                p.LogisticsMode = dt.Rows[i]["发货方式"].ToString();
                p.TrackCode = dt.Rows[i]["追踪码"].ToString();
                p.Status = dt.Rows[i]["状态"].ToString();
                p.Memo = dt.Rows[i]["备注"].ToString();

                NSession.Save(p);
                NSession.Flush();


            }
            return Json(new { IsSuccess = true });
        }

        public ActionResult Create()
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            ViewData["area"] = NSession.CreateSQLQuery(
             " select FromArea  from Users where Realname ='" + GetCurrentAccount().Realname +
             "'").UniqueResult();
            return View();
        }
        public ActionResult CreateB()
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            return View();
        }
        public ActionResult CreateC()
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            return View();
        }
        // 小包仓库
        public ActionResult CreatePacket()
        {
            ViewData["No"] = Utilities.GetNo(NSession, Utilities.PlanNo);
            return View();
        }

        [HttpPost]
        public JsonResult Create(PurchasePlanType obj)
        {
            try
            {
                List<PurchasePlanType> planno =
          NSession.CreateQuery("from PurchasePlanType where PlanNo ='" + obj.PlanNo + "'").List<PurchasePlanType>().ToList();
                if (planno.Count > 0)
                {
                    return Json(new { IsSuccess = false, ErrorMsg = "计划编号重复" });
                }
                if (obj.SendOn < Convert.ToDateTime("2000-01-01"))
                {
                    obj.SendOn = Convert.ToDateTime("2000-01-01");
                }
                if (obj.ReceiveOn < Convert.ToDateTime("2000-01-01"))
                {
                    obj.ReceiveOn = Convert.ToDateTime("2000-01-01");
                }
                if (obj.ReceiveOn < Convert.ToDateTime("2000-01-01"))
                {
                    obj.ReceiveOn = Convert.ToDateTime("2000-01-01");
                }

                List<ProductType> productTypes =
            NSession.CreateQuery("from ProductType where SKU ='" + obj.SKU + "'").List<ProductType>().ToList();
                if (productTypes.Count > 0)
                {
                    if (string.IsNullOrEmpty(obj.PicUrl))
                    {
                        obj.PicUrl = productTypes[0].PicUrl;
                    }
                    productTypes[0].Caigou4 = obj.ProductUrl;
                    NSession.Update(productTypes[0]);
                    NSession.Flush();
                }

                // 自动判断是否新品状态(根据主SKU判断)
                obj.IsFrist = Convert.ToInt32(base.NSession.CreateSQLQuery("select case when count(1)>0 then 0 else 1 end as resul from PurchasePlan where SKU in(select SKU from Products where OldSKU=(select OldSKU from Products where SKU='" + obj.SKU + "')) and Status in ('已采购','已发货','已收到')").UniqueResult());
                //obj.IsFrist = Convert.ToInt32(base.NSession.CreateSQLQuery("select case when count(1)>0 then 0 else 1 end as resul from PurchasePlan where SKU='" + obj.SKU + "' and Status in ('已采购','已发货','已收到')").UniqueResult());
                obj.CreateOn = DateTime.Now;
                obj.BuyOn = DateTime.Now;
                obj.MinDate = Convert.ToDateTime(base.NSession.CreateSQLQuery(string.Format("select isnull(MIN(O.CreateOn),'2000-01-01') as MinDate from Orders O left join OrderProducts OP On O.Id=OP.OId left join Products P on OP.SKU=P.SKU where  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null and OP.SKU='" + obj.SKU + "' and OP.IsQue=1 ")).UniqueResult());
                obj.MinValiDate = Convert.ToDateTime(base.NSession.CreateSQLQuery(string.Format("select isnull(MAX(R.CreateOn),'2000-01-01') as MinValiDate from Orders O left join OrderProducts OP On O.Id=OP.OId left join Products P on OP.SKU=P.SKU left join OrderRecord R on R.OrderNo=O.OrderNo where  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null and OP.SKU='" + obj.SKU + "' and OP.IsQue=1  and R.RecordType='验证订单' ")).UniqueResult());
                obj.CreateBy = CurrentUser.Realname;
                obj.BuyBy = CurrentUser.Realname;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
                //SaveSupplier(obj);
                LoggerUtil.GetPurchasePlanRecord(obj, "新建计划", "创建采购计划", CurrentUser, NSession);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public ViewResult GetPlanInfo(int Id)
        {
            // string orderby = Utilities.OrdeerBy(sort, order);
            PurchasePlanType plan = Get<PurchasePlanType>(Id);
            List<PurchasePlanType> list = NSession.CreateQuery("from PurchasePlanType where SKU='" + plan.SKU + "' and Status <>'异常' Order By CreateOn desc").SetMaxResults(3).List<PurchasePlanType>().ToList();

            return View(list);
        }

        private void SaveSupplier(PurchasePlanType obj)
        {
            object exit = NSession.CreateQuery("select count(Id) from SupplierType where SuppliersName='" + obj.Suppliers + "'").UniqueResult();
            if (Utilities.ToInt(exit) > 0)
            {
                SupplierType super = new SupplierType
                {
                    SuppliersName = obj.Suppliers
                };
                NSession.Save(super);
                NSession.Flush();
                SuppliersProductType product = new SuppliersProductType
                {
                    SId = super.Id,
                    SKU = obj.SKU,
                    Price = obj.Price,
                    Web = obj.ProductUrl
                };
                NSession.Save(product);
                NSession.Flush();
            }
            else
            {
                IList<SupplierType> list = NSession.CreateQuery("from SupplierType where SuppliersName='" + obj.Suppliers + "'").List<SupplierType>();
                object productexit = NSession.CreateQuery("from SuppliersProductType where SId='" + list[0].Id + "' and SKU='" + obj.SKU + "' ").UniqueResult();
                if (productexit == null)
                {
                    SuppliersProductType product = new SuppliersProductType
                    {
                        SId = list[0].Id,
                        SKU = obj.SKU,
                        Price = obj.Price,
                        Web = obj.ProductUrl
                    };
                    NSession.Save(product);
                    NSession.Flush();
                }
            }
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PurchasePlanType GetById(int Id)
        {
            PurchasePlanType obj = NSession.Get<PurchasePlanType>(Id);
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
            PurchasePlanType obj = GetById(id);
           
            ViewData["sku"] = obj.SKU;
            return View(obj);
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult EditB(int id)
        {
            PurchasePlanType obj = GetById(id);
            ViewData["sku"] = obj.SKU;
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(PurchasePlanType obj)
        {

            try
            {
                string str = "";
                PurchasePlanType obj2 = GetById(obj.Id);
                //避免由于时间问题报错，赋值
                if (obj.MinDate.ToString() == "0001/1/1 0:00:00")
                {
                    obj.MinDate =Convert.ToDateTime("2000-01-01");
                }
                if (obj.MinValiDate.ToString() == "0001/1/1 0:00:00")
                {
                    obj.MinValiDate = Convert.ToDateTime("2000-01-01");
                }
                str += Utilities.GetObjEditString(obj2, obj) + "<br>";
                NSession.Clear();
                NSession.Update(obj);
                NSession.Flush();
                LoggerUtil.GetPurchasePlanRecord(obj, "修改采购计划", str, CurrentUser, NSession);
                if (obj.ExamineId != 0)
                {
                    PurchasePlanExamineRecordType planExamine = Get<PurchasePlanExamineRecordType>(obj.ExamineId);
                    object amount = NSession.CreateQuery("select SUM(Price*Qty+Freight) from PurchasePlanType Where ExamineId =" + obj.ExamineId).UniqueResult();
                    planExamine.ExamineAmount = Math.Round(Utilities.ToDouble(amount.ToString()), 5);
                    NSession.SaveOrUpdate(planExamine);
                    NSession.Flush();
                }
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
                PurchasePlanType obj = GetById(id);
                if (obj.Status == "已采购")
                {
                    NSession.Delete(obj);
                    NSession.Flush();
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult ExportPlan(string search, string c_area, string c_overtime)
        {
            try
            {
                string orderby = "order by Id desc";
                string where = Utilities.SqlWhere(search);
                if (where.Length > 0)
                {
                    where += " and IsBei=0";
                }
                else
                {
                    where += " Where IsBei=0";
                }

                //PurchasePlan表Area有NULL值，无法直接查询
                if (c_area == "宁波" || c_area == "义乌")
                {
                    where += " and BuyBy in (select Realname from  UserType where FromArea ='" + c_area + "')";
                }
                if (c_overtime == "是")
                {
                    where += " and  (ReceiveOn= '2000-01-01 00:00:00.000' and  DATEDIFF(day,MinDate,getdate())>4) or (ReceiveOn>'2000-01-01 00:00:00.000' and DATEDIFF(day,MinDate,ReceiveOn)>4) ";
                }
                if (c_overtime == "否")
                {
                    where += " and  (ReceiveOn= '2000-01-01 00:00:00.000' and  DATEDIFF(day,MinDate,getdate())<=4) or (ReceiveOn>'2000-01-01 00:00:00.000' and DATEDIFF(day,MinDate,ReceiveOn)<=4) ";

                }
                List<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                    .List<PurchasePlanType>().ToList();
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((objList)));
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        public JsonResult ExportPlanB(string search)
        {
            try
            {
                List<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + Utilities.SqlWhere(search))
                    .List<PurchasePlanType>().ToList();
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((objList)));
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search, string c_area, string c_overtime)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and IsBei=0";
            }
            else
            {
                where += " Where IsBei=0";
            }

            //PurchasePlan表Area有NULL值，无法直接查询
            if (c_area == "宁波" || c_area == "义乌")
            {
                where += " and BuyBy in (select Realname from  UserType where FromArea ='" + c_area + "')";
            }
            if (c_overtime == "是")
            {
                where += " and  (ReceiveOn= '2000-01-01 00:00:00.000' and  DATEDIFF(day,MinDate,getdate())>4) or (ReceiveOn>'2000-01-01 00:00:00.000' and DATEDIFF(day,MinDate,ReceiveOn)>4) ";
            }
            if (c_overtime == "否")
            {
                where += " and  (ReceiveOn= '2000-01-01 00:00:00.000' and  DATEDIFF(day,MinDate,getdate())<=4) or (ReceiveOn>'2000-01-01 00:00:00.000' and DATEDIFF(day,MinDate,ReceiveOn)<=4) ";

            }

            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanType>();
            //最早缺货时间
            foreach (var PP in objList)
            {
                //PP.MinDate = Convert.ToDateTime(base.NSession.CreateSQLQuery(string.Format("select MIN(O.CreateOn) as MinDate from Orders O left join OrderProducts OP On O.Id=OP.OId left join Products P on OP.SKU=P.SKU where  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null and OP.SKU='" + PP.SKU+ "'")).UniqueResult());
                PP.singleweight = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select Weight as singleweight from Products P where P.SKU='" + PP.SKU + "'")).UniqueResult());
                PP.totalweight = Convert.ToDouble(PP.singleweight * PP.Qty);
                PP.ExamineStatus = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("select ExamineStatus from PurchasePlanExamineRecord where Id={0}", PP.ExamineId)).UniqueResult());
                if (PP.ReceiveOn == Convert.ToDateTime("2000-01-01"))
                {
                    TimeSpan timediff = DateTime.Now - PP.MinDate;
                    PP.Spandays = timediff.Days;
                }
                else
                {
                    TimeSpan timediff = PP.ReceiveOn - PP.MinDate;
                    PP.Spandays = timediff.Days;
                }


            }
            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType " + where).UniqueResult();
            object sum2 = NSession.CreateQuery("select SUM(Freight) from PurchasePlanType " + where).UniqueResult();
            object sum3 = NSession.CreateQuery("select avg(Price) from PurchasePlanType " + where).UniqueResult();
            object sum4 = NSession.CreateQuery("select SUM(Qty) from PurchasePlanType " + where).UniqueResult();
            object sum5 = NSession.CreateQuery("select SUM(DaoQty) from PurchasePlanType " + where).UniqueResult();
            object sum6 = NSession.CreateQuery("select SUM(TuiPrice) from PurchasePlanType " + where).UniqueResult();
            object sum7 = NSession.CreateQuery("select SUM(TuiFreight) from PurchasePlanType " + where).UniqueResult();
            object sum8 = NSession.CreateQuery("select SUM(Freight+Qty*Price) from PurchasePlanType " + where).UniqueResult();
            object sum9 = NSession.CreateQuery("select SUM(Qty*UnitFristPrice) from PurchasePlanType " + where).UniqueResult();

            List<object> footers = new List<object>();
            if (sum == null)
                sum = 0;
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(sum.ToString()), Freight = Math.Round(Utilities.ToDouble(sum2), 2), Price = Math.Round(Utilities.ToDouble(sum3), 2), Qty = Utilities.ToInt(sum4), DaoQty = Utilities.ToInt(sum5), TuiPrice = Utilities.ToDecimal(sum6), TuiFreight = Utilities.ToDecimal(sum7), TotalAmount = Utilities.ToDecimal(sum8), UnitFristPrice = Utilities.ToDecimal(sum9) });

            return Json(new { total = count, rows = objList, footer = footers });

        }

        public JsonResult BList(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and (IsBei=1 or IsBei=2)";
            }
            else
            {
                where += " Where (IsBei=1 or IsBei=2)";
            }
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanType>();
            foreach (var PP in objList)
            {
                PP.singleweight = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select Weight as singleweight from Products P where P.SKU='" + PP.SKU + "'")).UniqueResult());
                PP.totalweight = Convert.ToDouble(PP.singleweight * PP.Qty);
                PP.ExamineStatus = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("select ExamineStatus from PurchasePlanExamineRecord where Id={0}", PP.ExamineId)).UniqueResult());
            }
            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType " + where).UniqueResult();
            object sum2 = NSession.CreateQuery("select SUM(Freight) from PurchasePlanType " + where).UniqueResult();
            object sum3 = NSession.CreateQuery("select avg(Price) from PurchasePlanType " + where).UniqueResult();
            object sum4 = NSession.CreateQuery("select SUM(Qty) from PurchasePlanType " + where).UniqueResult();
            object sum5 = NSession.CreateQuery("select SUM(DaoQty) from PurchasePlanType " + where).UniqueResult();
            object sum6 = NSession.CreateQuery("select SUM(TuiPrice) from PurchasePlanType " + where).UniqueResult();
            object sum7 = NSession.CreateQuery("select SUM(TuiFreight) from PurchasePlanType " + where).UniqueResult();
            object sum8 = NSession.CreateQuery("select SUM(Freight+Qty*Price) from PurchasePlanType " + where).UniqueResult();
            object sum9 = NSession.CreateQuery("select SUM(Qty*UnitFristPrice) from PurchasePlanType " + where).UniqueResult();

            List<object> footers = new List<object>();
            if (sum == null)
                sum = 0;
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(sum.ToString()), Freight = Math.Round(Utilities.ToDouble(sum2), 2), Price = Math.Round(Utilities.ToDouble(sum3), 2), Qty = Utilities.ToInt(sum4), DaoQty = Utilities.ToInt(sum5), TuiPrice = Utilities.ToDecimal(sum6), TuiFreight = Utilities.ToDecimal(sum7), TotalAmount = Utilities.ToDecimal(sum8), UnitFristPrice = Utilities.ToDecimal(sum9) });
            return Json(new { total = count, rows = objList, footer = footers });

        }

        //海外仓采购备货
        public JsonResult CList(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and IsBei=2";
            }
            else
            {
                where += " Where IsBei=2";
            }
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanType>();
            foreach (var PP in objList)
            {
                PP.singleweight = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select Weight as singleweight from Products P where P.SKU='" + PP.SKU + "'")).UniqueResult());
                PP.totalweight = Convert.ToDouble(PP.singleweight * PP.Qty);
            }
            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType " + where).UniqueResult();
            object sum2 = NSession.CreateQuery("select SUM(Freight) from PurchasePlanType " + where).UniqueResult();
            object sum3 = NSession.CreateQuery("select avg(Price) from PurchasePlanType " + where).UniqueResult();
            object sum4 = NSession.CreateQuery("select SUM(Qty) from PurchasePlanType " + where).UniqueResult();
            object sum5 = NSession.CreateQuery("select SUM(DaoQty) from PurchasePlanType " + where).UniqueResult();
            object sum6 = NSession.CreateQuery("select SUM(TuiPrice) from PurchasePlanType " + where).UniqueResult();
            object sum7 = NSession.CreateQuery("select SUM(TuiFreight) from PurchasePlanType " + where).UniqueResult();
            object sum8 = NSession.CreateQuery("select SUM(Freight+Qty*Price) from PurchasePlanType " + where).UniqueResult();
            object sum9 = NSession.CreateQuery("select SUM(Qty*UnitFristPrice) from PurchasePlanType " + where).UniqueResult();

            List<object> footers = new List<object>();
            if (sum == null)
                sum = 0;
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(sum.ToString()), Freight = Math.Round(Utilities.ToDouble(sum2), 2), Price = Math.Round(Utilities.ToDouble(sum3), 2), Qty = Utilities.ToInt(sum4), DaoQty = Utilities.ToInt(sum5), TuiPrice = Utilities.ToDecimal(sum6), TuiFreight = Utilities.ToDecimal(sum7), TotalAmount = Utilities.ToDecimal(sum8), UnitFristPrice = Utilities.ToDecimal(sum9) });
            return Json(new { total = count, rows = objList, footer = footers });

        }

        public JsonResult NBList(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and CreateBy in(select Realname from  UserType where FromArea ='宁波') And Profit>0";
            }
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanType>();
            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType " + where).UniqueResult();
            List<object> footers = new List<object>();
            if (sum == null)
                sum = 0;
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(sum.ToString()) });
            return Json(new { total = count, rows = objList, footer = footers });
        }

        public JsonResult YWList(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            if (where.Length > 0)
            {
                where += " and CreateBy in(select Realname from  UserType where FromArea ='义乌') And Profit>0";
            }
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanType>();
            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType " + where).UniqueResult();
            List<object> footers = new List<object>();
            if (sum == null)
                sum = 0;
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(sum.ToString()) });
            return Json(new { total = count, rows = objList, footer = footers });
        }

        public JsonResult GetListByEId(int id, string sort, string order)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType where (Status<>'异常' and Status<>'失效') and ExamineId=" + id + orderby)
              .List<PurchasePlanType>();
            foreach (PurchasePlanType purchasePlanType in objList)
            {
                purchasePlanType.Rate =
                    Math.Round(
                        purchasePlanType.Profit / (purchasePlanType.Qty * purchasePlanType.Price + purchasePlanType.Freight),
                        2) * 100;
                purchasePlanType.singleweight = Convert.ToDouble(base.NSession.CreateSQLQuery(string.Format("select Weight as singleweight from Products P where P.SKU='" + purchasePlanType.SKU + "'")).UniqueResult());
                purchasePlanType.totalweight = Convert.ToDouble(purchasePlanType.singleweight * purchasePlanType.Qty);
                purchasePlanType.Totalmoney = Convert.ToDecimal(purchasePlanType.Price * purchasePlanType.Qty+purchasePlanType.Freight);
            }
            object count = NSession.CreateQuery("select SUM(Profit) from PurchasePlanType where FromTo not in('运费','耗材','广告费') and ExamineId=" + id).UniqueResult();
            if (count == null)
                count = 0;
            List<object> footers = new List<object>();
            footers.Add(new PurchasePlanType { Profit = Utilities.ToDouble(count.ToString()), Freight = objList.Sum(x => x.Freight) });
            return Json(new { total = objList.Count, rows = objList, footer = footers });
        }

        public JsonResult SearchSKU(string id, string wid = "0")
        {
            IList<PurchasePlanType> obj = NSession.CreateQuery("from PurchasePlanType where SKU=:sku order by Id desc").SetString("sku", id)
           .SetFirstResult(0)
                .SetMaxResults(1).List<PurchasePlanType>();



            string Standard = Convert.ToString(base.NSession.CreateSQLQuery(string.Format("select Standard as Standard from Products P where P.SKU='" + id + "'")).UniqueResult());
            if (obj.Count > 0)
            {
                // 获取销量数据
                //object sum = NSession.CreateSQLQuery("select sum(Qty) from Orders O left join OrderProducts OP On O.Id=Op.OId  where Status in ('已处理','已发货') and SKU='" + id + "' and DATEDIFF( DAY,CreateOn,GETDATE())<60").UniqueResult();
                // 由20天限制调整提高到60天
                List<DataDictionaryDetailType> list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day1'");
                string Day1 = list[0].DicValue;
                list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day2'");
                string Day2 = list[0].DicValue;
                list = GetList<DataDictionaryDetailType>("DicCode", "RetainValue", "FullName='Day3'");
                string Day3 = list[0].DicValue;
                object sum = NSession.CreateSQLQuery("select Convert(float,[dbo].[F_GetParc1](" + Day1 + "," + Day2 + "," + Day3 + ",60,'" + id + "'," + wid + "))").UniqueResult();

                ViewData["Qty"] = sum;
                if (sum == null)
                {
                    ViewData["Qty"] = 0;
                }

                // 从商品表内获取图片
                IList<ProductType> productlist = NSession.CreateQuery(" From ProductType where sku='" + id + "'").SetFirstResult(0)
                .SetMaxResults(1).List<ProductType>();
                if (productlist.Count() > 0)
                {
                    obj[0].PicUrl = ".." + productlist[0].PicUrl;
                }

                return Json(new { type = 1, result = obj, Standard = Standard, Qty = sum }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<ProductType> productlist = NSession.CreateQuery(" From ProductType where sku='" + id + "'").SetFirstResult(0)
                .SetMaxResults(1).List<ProductType>();
                return Json((new { type = 2, result = productlist, Standard = Standard }), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetSuppliers()
        {
            IList<SupplierType> obj = NSession.CreateQuery("from SupplierType").List<SupplierType>();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BatchUpdateOrderNo(string ids, string o)
        {
            List<PurchasePlanType> list =
                NSession.CreateQuery(string.Format("from PurchasePlanType where Id in({0})", ids)).List
                    <PurchasePlanType>().ToList();
            foreach (PurchasePlanType purchasePlanType in list)
            {
                purchasePlanType.OrderNo = o;
                NSession.Update(purchasePlanType);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult BatchUpdateFreight(string ids, decimal o)
        {
            List<PurchasePlanType> list =
                NSession.CreateQuery(string.Format("from PurchasePlanType where Id in({0})", ids)).List
                    <PurchasePlanType>().ToList();
            foreach (PurchasePlanType purchasePlanType in list)
            {
                purchasePlanType.Freight = Utilities.ToDouble(o);
                NSession.Update(purchasePlanType);
                NSession.Flush();
                if (purchasePlanType.ExamineId != 0)
                {
                    PurchasePlanExamineRecordType planExamine = Get<PurchasePlanExamineRecordType>(purchasePlanType.ExamineId);
                    object amount = NSession.CreateQuery("select SUM(Price*Qty+Freight) from PurchasePlanType Where ExamineId =" + purchasePlanType.ExamineId).UniqueResult();
                    planExamine.ExamineAmount = Math.Round(Utilities.ToDouble(amount.ToString()), 5);
                    NSession.SaveOrUpdate(planExamine);
                    NSession.Flush();
                }

            }
            return Json(new { IsSuccess = true });
        }



        public JsonResult BatchUpdateTrackCode(string f)
        {
            DataTable dt = OrderHelper.GetDataTable(f);
            if (dt.Columns.Count == 2)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    List<PurchasePlanType> list =
               NSession.CreateQuery(string.Format("from PurchasePlanType where OrderNo='{0}'", dr[0])).List
                   <PurchasePlanType>().ToList();
                    if (dr[1] != null)
                        foreach (PurchasePlanType purchasePlanType in list)
                        {
                            purchasePlanType.TrackCode = dr[1].ToString();
                            NSession.Update(purchasePlanType);
                            NSession.Flush();
                        }
                }
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult SetErrorType(int k, string e, string m, string h, string s, string q, string b, string m2)
        {
            PurchasePlanType purchasePlan = GetById(k);
            purchasePlan.Status = "异常";
            purchasePlan.ErrorType = e;
            purchasePlan.HandleType = h;
            purchasePlan.ErrorRemark += m;
            if (h == null) h = "";
            if (h.IndexOf("退款") == -1)
            {
                purchasePlan.Status = "失效";
            }
            NSession.Update(purchasePlan);
            NSession.Flush();
            if (b == "1")
            {
                if (!string.IsNullOrEmpty(s))
                    purchasePlan.SKU = s;
                if (!string.IsNullOrEmpty(q))
                    purchasePlan.Qty = Utilities.ToInt(q);
                purchasePlan.Status = "已采购";
                purchasePlan.DaoQty = 0;
                purchasePlan.IsBei = 1;
                purchasePlan.Id = 0;
                purchasePlan.Memo = m2;
                NSession.Clear();
                NSession.Save(purchasePlan);
                NSession.Flush();
            }
            return Json(new { IsSuccess = true });

        }

        public JsonResult SetHandleType(int k, string v)
        {
            if (GetCurrentAccount().Realname == "夏午君" || GetCurrentAccount().Realname == "王成栋" || GetCurrentAccount().Realname == "蒋芸芸" || GetCurrentAccount().Realname == "丁纬盛")
            {
                PurchasePlanType purchasePlan = GetById(k);
                purchasePlan.ErrorRemark += GetCurrentAccount().Realname + "确认退款  ";
                purchasePlan.TuiPrice = Utilities.ToDecimal(v);
                purchasePlan.IsTuiPrice = 1;
                if (purchasePlan.IsTuiFreight == 1)
                {
                    purchasePlan.Status = "已退款";
                }
                NSession.Update(purchasePlan);
                NSession.Flush();
                return Json(new { IsSuccess = true });
            }
            return Json(new { IsSuccess = false });

        }
        public JsonResult SetHandleType2(int k, string v)
        {
            PurchasePlanType purchasePlan = GetById(k);

            purchasePlan.TuiFreight = Utilities.ToDecimal(v);
            purchasePlan.ErrorRemark += GetCurrentAccount().Realname + "确认运费  ";
            purchasePlan.IsTuiFreight = 1;
            if (purchasePlan.IsTuiPrice == 1)
            {
                purchasePlan.Status = "已退款";
            }
            NSession.Update(purchasePlan);
            NSession.Flush();
            return Json(new { IsSuccess = true });

        }

        public JsonResult CheckRole()
        {
            if (base.CurrentUser.RoleName == "财务")
            {
                return Json(new { IsSuccess = true });
            }
            return Json(new { IsSuccess = false });
        }
    }
}

