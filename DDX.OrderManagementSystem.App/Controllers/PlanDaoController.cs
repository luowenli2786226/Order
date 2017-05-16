using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class PlanDaoController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create(int Id)
        {
            PurchasePlanType plan = NSession.Get<PurchasePlanType>(Id);
            ViewData["plan"] = plan;
            ViewData["uname"] = NSession.CreateSQLQuery(
                "select Purchaser from Products where SKU='" + plan.SKU + "'").UniqueResult();
            ViewData["pwd"] = NSession.CreateSQLQuery(
                " select Password from Users where Realname in (  select Purchaser from Products where SKU='" + plan.SKU +
                "')").UniqueResult();
            ViewData["area"] = NSession.CreateSQLQuery(
             " select FromArea  from Users where Realname ='" + plan.CreateBy +
             "'").UniqueResult();
            return View();
        }

        [HttpPost]
        public JsonResult Create(PlanDaoType obj)
        {
            try
            {
                // 海外仓库验证
                if (obj.Type != "小包仓库")
                {
                    if (obj.YsUMaxPrice <= 0)
                    {
                        return Json(new { IsSuccess = false, ErrorMsg = "优胜（UMAX）价格 必须大于0" });
                    }
                    if (obj.UnitFristPrice <= 0)
                    {
                        return Json(new { IsSuccess = false, ErrorMsg = "单位头程费用 必须大于0" });
                    }
                }
                ///当入库的仓库为宁波仓库时，只允许戚波，陈尔，吕晶晶有权限入库
                if (obj.WId == 1)
                {
                    bool CanStockIn = false;
                    IList<UserType> userlist = base.NSession.CreateQuery("from UserType where Realname in ('吕晶晶','陈尔','戚波')").List<UserType>();
                    foreach (var user in userlist)
                    {
                        if (GetCurrentAccount().Realname == user.Realname)
                        {
                            CanStockIn = true;
                        }
                    }
                    if (!CanStockIn)
                    {
                        return Json(new { IsSuccess = false, ErrorMsg = "当前登录人员没有权限，入库权限人员为戚波，陈尔，吕晶晶" });
                    }
                }

                IList<PurchasePlanType> plan =
                    NSession.CreateQuery("from PurchasePlanType where Id=:p").SetInt32("p", Convert.ToInt32(obj.PlanNo)).
                        SetMaxResults(1)
                        .List<PurchasePlanType>();
                if (plan.Count > 0)
                {
                    if (plan[0].Status != "已收到")
                    {
                        obj.PlanId = plan[0].Id;
                        obj.PlanNo = plan[0].PlanNo;
                        WarehouseType w = NSession.Get<WarehouseType>(obj.WId);
                        obj.WName = w.WName;

                        if (obj.Type == "海外仓库")
                        {
                            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                            CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == "USD");

                            // 成本= ([单位头程费美元]+[优胜（UMAX）价格美元]）*当天汇率
                            obj.Price = ((Utilities.ToDouble(obj.UnitTariff) + Utilities.ToDouble(obj.YsUMaxPrice) + Utilities.ToDouble(obj.UnitFristPrice)) * Convert.ToDouble(currencyType.CurrencyValue));
                            obj.ExchangeRate = currencyType.CurrencyValue;
                        }
                        else
                        {
                            // 小包仓库 默认当前人民币单价
                            obj.Price = Math.Round(plan[0].Price + plan[0].Freight / plan[0].Qty, 4);
                        }
                        // 脚本保留
                        //obj.Price = Math.Round(plan[0].Price + plan[0].Freight / plan[0].Qty, 4) + Utilities.ToDouble(obj.UnitTariff) + Utilities.ToDouble(obj.UnitFristPrice);
                        obj.DaoOn = DateTime.Now;
                        obj.SendOn = DateTime.Now;
                        obj.IsAudit = 0;
                        obj.CheckBy = GetCurrentAccount().Realname;
                        obj.ValiBy = GetCurrentAccount().Realname;
                        plan[0].UnitTariff = obj.UnitTariff;
                        plan[0].UnitFristPrice = obj.UnitFristPrice;
                        plan[0].YsUMaxPrice = obj.YsUMaxPrice;
                        plan[0].Status = obj.Status;
                        plan[0].ReceiveOn = DateTime.Now;
                        plan[0].DaoQty += obj.RealQty;
                        if (plan[0].Qty < plan[0].DaoQty)//2016-11-07
                        {
                            return Json(new { ErrorMsg = "实际到货数不得超过采购数", IsSuccess = false });
                        }//2016-11-07
                        if (plan[0].Qty > plan[0].DaoQty)
                        {
                            plan[0].Status = obj.Status = "部分到货";
                        }
                        else
                        {
                            plan[0].Status = obj.Status = "已收到";
                        }
                        NSession.SaveOrUpdate(obj);
                        NSession.Flush();
                        NSession.Update(plan[0]);
                        NSession.Flush();
                        LoggerUtil.GetPurchasePlanRecord(plan[0], "采购到货", "采购到货" + obj.Status + obj.RealQty, CurrentUser, NSession);
                        AuditDao(obj.Id);
                        return Json(new { IsSuccess = true, Id = obj.Id });
                    }
                }
                else
                {
                    return Json(new { ErrorMsg = "出错了", IsSuccess = false });
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public ActionResult CreatePacket(int Id)
        {
            PurchasePlanType plan = NSession.Get<PurchasePlanType>(Id);

            if (CurrentUser.FromArea == "宁波")
            {
                ViewData["WId"] = 1;
            }
            else if (CurrentUser.FromArea == "义乌")
            {
                ViewData["WId"] = 3;
            }

            //ViewData["type"] = "小包仓库";
            ViewData["plan"] = plan;
            ViewData["uname"] = NSession.CreateSQLQuery(
                "select Purchaser from Products where SKU='" + plan.SKU + "'").UniqueResult();
            ViewData["pwd"] = NSession.CreateSQLQuery(
                " select Password from Users where Realname in (  select Purchaser from Products where SKU='" + plan.SKU +
                "')").UniqueResult();
            return View();
        }

        //[HttpPost]
        //public JsonResult CreatePacket(PlanDaoType obj)
        //{
        //    try
        //    {
        //        //if (obj.YsUMaxPrice <= 0 && obj.Type == "海外仓库")
        //        //{
        //        //    return Json(new { IsSuccess = false, ErrorMsg = "优胜（UMAX）价格 必须大于0" });
        //        //}
        //        //if (obj.UnitFristPrice <= 0 && obj.Type == "海外仓库")
        //        //{
        //        //    return Json(new { IsSuccess = false, ErrorMsg = "单位头程费用 必须大于0" });
        //        //}


        //        IList<PurchasePlanType> plan =
        //            NSession.CreateQuery("from PurchasePlanType where Id=:p").SetInt32("p", Convert.ToInt32(obj.PlanNo)).
        //                SetMaxResults(1)
        //                .List<PurchasePlanType>();
        //        if (plan.Count > 0)
        //        {
        //            if (plan[0].Status != "已收到")
        //            {
        //                obj.PlanId = plan[0].Id;
        //                obj.PlanNo = plan[0].PlanNo;
        //                WarehouseType w = NSession.Get<WarehouseType>(obj.WId);
        //                obj.WName = w.WName;

        //                if (obj.Type == "海外仓库")
        //                {
        //                    List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
        //                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == "USD");

        //                    // 成本= ([单位头程费美元]+[优胜（UMAX）价格美元]）*当天汇率
        //                    obj.Price = ((Utilities.ToDouble(obj.UnitTariff) + Utilities.ToDouble(obj.YsUMaxPrice) + Utilities.ToDouble(obj.UnitFristPrice)) * Convert.ToDouble(currencyType.CurrencyValue));
        //                    obj.ExchangeRate = currencyType.CurrencyValue;
        //                }
        //                else
        //                {
        //                    // 小包仓库 默认当前人民币单价
        //                    obj.Price = Math.Round(plan[0].Price + plan[0].Freight / plan[0].Qty, 4);
        //                }
        //                // 脚本保留
        //                //obj.Price = Math.Round(plan[0].Price + plan[0].Freight / plan[0].Qty, 4) + Utilities.ToDouble(obj.UnitTariff) + Utilities.ToDouble(obj.UnitFristPrice);
        //                obj.DaoOn = DateTime.Now;
        //                obj.SendOn = DateTime.Now;
        //                obj.IsAudit = 0;
        //                obj.CheckBy = GetCurrentAccount().Realname;
        //                obj.ValiBy = GetCurrentAccount().Realname;
        //                plan[0].UnitTariff = obj.UnitTariff;
        //                plan[0].UnitFristPrice = obj.UnitFristPrice;
        //                plan[0].YsUMaxPrice = obj.YsUMaxPrice;
        //                plan[0].Status = obj.Status;
        //                plan[0].ReceiveOn = DateTime.Now;
        //                plan[0].DaoQty += obj.RealQty;
        //                if (plan[0].Qty > plan[0].DaoQty)
        //                {
        //                    plan[0].Status = obj.Status = "部分到货";
        //                }
        //                else
        //                {
        //                    plan[0].Status = obj.Status = "已收到";
        //                }
        //                NSession.SaveOrUpdate(obj);
        //                NSession.Flush();
        //                NSession.Update(plan[0]);
        //                NSession.Flush();
        //                LoggerUtil.GetPurchasePlanRecord(plan[0], "采购到货", "采购到货" + obj.Status + obj.RealQty, CurrentUser, NSession);
        //                AuditDao(obj.Id);
        //                return Json(new { IsSuccess = true, Id = obj.Id });
        //            }
        //        }
        //        else
        //        {
        //            return Json(new { ErrorMsg = "出错了", IsSuccess = false });
        //        }
        //    }
        //    catch (Exception ee)
        //    {
        //        return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
        //    }
        //    return Json(new { IsSuccess = true });
        //}

        public ActionResult CreateC(int Id)
        {
            PurchasePlanType plan = NSession.Get<PurchasePlanType>(Id);
            ViewData["plan"] = plan;
            ViewData["uname"] = NSession.CreateSQLQuery(
                "select Purchaser from Products where SKU='" + plan.SKU + "'").UniqueResult();
            ViewData["pwd"] = NSession.CreateSQLQuery(
                " select Password from Users where Realname in (  select Purchaser from Products where SKU='" + plan.SKU +
                "')").UniqueResult();
            return View();
        }

        [HttpPost]
        public JsonResult CreateC(PlanDaoType obj)
        {

            try
            {
                IList<PurchasePlanType> plan =
                    NSession.CreateQuery("from PurchasePlanType where Id=:p").SetInt32("p", Convert.ToInt32(obj.PlanNo)).
                        SetMaxResults(1)
                        .List<PurchasePlanType>();
                if (plan.Count > 0)
                {
                    if (plan[0].Status != "已收到")
                    {
                        obj.PlanId = plan[0].Id;
                        obj.PlanNo = plan[0].PlanNo;
                        WarehouseType w = NSession.Get<WarehouseType>(obj.WId);
                        obj.WName = w.WName;

                        obj.Price = Math.Round(plan[0].Price + plan[0].Freight / plan[0].Qty, 4) + Utilities.ToDouble(obj.UnitTariff) + Utilities.ToDouble(obj.UnitFristPrice);
                        obj.DaoOn = DateTime.Now;
                        obj.SendOn = DateTime.Now;
                        obj.IsAudit = 0;
                        obj.CheckBy = GetCurrentAccount().Realname;
                        obj.ValiBy = GetCurrentAccount().Realname;
                        plan[0].UnitTariff = obj.UnitTariff;
                        plan[0].UnitFristPrice = obj.UnitFristPrice;
                        plan[0].Status = obj.Status;
                        plan[0].ReceiveOn = DateTime.Now;
                        plan[0].DaoQty += obj.RealQty;
                        if (plan[0].Qty > plan[0].DaoQty)
                        {
                            plan[0].Status = obj.Status = "部分到货";
                        }
                        else
                        {
                            plan[0].Status = obj.Status = "已收到";
                        }
                        NSession.SaveOrUpdate(obj);
                        NSession.Flush();
                        NSession.Update(plan[0]);
                        NSession.Flush();
                        LoggerUtil.GetPurchasePlanRecord(plan[0], "海外仓采购", "采购到货" + obj.Status + obj.RealQty, CurrentUser, NSession);
                        AuditDao(obj.Id);
                        return Json(new { IsSuccess = true, Id = obj.Id });
                    }
                }
                else
                {
                    return Json(new { ErrorMsg = "出错了", IsSuccess = false });
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult AuditDao(int Id)
        {
            PlanDaoType obj = NSession.Get<PlanDaoType>(Id);
            if (obj != null)
            {
                if (obj.IsAudit == 0)
                {
                    obj.DaoOn = DateTime.Now;
                    obj.IsAudit = 1;
                    obj.SKUCode = Utilities.CreateSKUCode(obj.SKU, obj.RealQty, obj.PlanNo, "", NSession);

                    NSession.SaveOrUpdate(obj);
                    NSession.Flush();
                    Utilities.StockIn(obj.WId, obj.SKU, obj.RealQty, obj.Price, "采购到货", CurrentUser.Realname, obj.Memo, NSession, true);
                    if (obj.Price > 0)
                    {
                        //obj.Price = obj.Price + Utilities.ToDouble(obj.UnitFristPrice) + Utilities.ToDouble(obj.UnitTariff);
                        NSession.CreateSQLQuery(" update Products set Price=" + obj.Price + "  where  SKU='" +
                                                obj.SKU.Trim() + "'")
                            .UniqueResult();
                        NSession.Flush();
                    }
                    return Json(new { IsSuccess = true });
                }
                return Json(new { ErrorMsg = "已经审核了", IsSuccess = false });
            }
            return Json(new { ErrorMsg = "状态出错!", IsSuccess = false });
        }


        public JsonResult ExportDao(string st, string et)
        {
            string sql = @"select * from  PlanDao";
            sql += " where IsAudit=1 and DaoOn  between '" + st + "' and '" + et + "'";

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql + " order by DaoOn asc";
            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);
            DataTable dt = ds.Tables[0];
            List<string> list = new List<string>();
            string str = "";

            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }

        public JsonResult PrintSKU(string id)
        {
            List<PlanDaoType> planDaoTypes =
                NSession.CreateQuery("from PlanDaoType where Id in(" + id + ")").List<PlanDaoType>().ToList();
            if (planDaoTypes != null)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("sku");
                dt.Columns.Add("name");
                dt.Columns.Add("num");
                dt.Columns.Add("date");
                dt.Columns.Add("people");
                dt.Columns.Add("desc");
                dt.Columns.Add("code");
                dt.Columns.Add("库位");

                foreach (PlanDaoType obj in planDaoTypes)
                {
                    int i = 1;
                    IList<PurchasePlanType> plans = NSession.CreateQuery("from PurchasePlanType where PlanNo=:p and SKU=:p2").SetString("p", obj.PlanNo).SetString("p2", obj.SKU).SetMaxResults(1).List<PurchasePlanType>();
                    IList<ProductType> products = NSession.CreateQuery("from ProductType where SKU=:p2").SetString("p2", obj.SKU).SetMaxResults(1).List<ProductType>();
                    PurchasePlanType plan = plans[0];
                    ProductType product = products[0];
                    IList<SKUCodeType> list =
                         NSession.CreateQuery("from SKUCodeType where SKU=:p1 and PlanNo=:p2 and Code >=:p3 order by Id").SetString("p1", obj.SKU).
                             SetString("p2", obj.PlanNo).SetInt32("p3", obj.SKUCode).SetMaxResults(obj.RealQty).List<SKUCodeType>();

                    foreach (SKUCodeType skuCodeType in list)
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = plan.SKU;
                        dr[1] = plan.ProductName;
                        dr[2] = i + "/" + obj.RealQty;
                        dr[3] = plan.BuyOn;
                        dr[4] = plan.BuyBy;
                        dr[5] = plan.PlanNo;
                        dr[6] = skuCodeType.Code;
                        dr[7] = product.Location;
                        dt.Rows.Add(dr);
                        i++;
                    }
                }


                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                PrintDataType data = new PrintDataType();
                data.Content = ds.GetXml();
                data.CreateOn = DateTime.Now;
                NSession.Save(data);
                NSession.Flush();
                return Json(new { IsSuccess = true, Result = data.Id });
            }
            return Json(new { IsSuccess = false });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PlanDaoType GetById(int Id)
        {
            PlanDaoType obj = NSession.Get<PlanDaoType>(Id);
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
            PlanDaoType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(PlanDaoType obj)
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

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {

                PlanDaoType obj = GetById(id);
                PurchasePlanType plan =
                NSession.Get<PurchasePlanType>(obj.PlanId);
                if (plan != null)
                {
                    plan.Status = "已发货";
                    plan.DaoQty = plan.DaoQty - obj.RealQty;
                    NSession.Update(plan);
                    NSession.Flush();
                }
                NSession.Delete("from SKUCodeType where PlanNo='" + obj.PlanNo + "' and Code >=" + obj.SKUCode + " and Code < " + (obj.SKUCode + obj.RealQty));

                // 取消自动添加出库记录
                //Utilities.StockOut(obj.WId, obj.SKU, obj.RealQty, "到货删除", GetCurrentAccount().Realname, "", "", NSession);
                // 负数入库冲红操作
                Utilities.StockIn(obj.WId, obj.SKU, obj.RealQty * -1, obj.Price, "采购到货冲红", CurrentUser.Realname, obj.Memo, NSession, true, true, "DESC");

                NSession.Flush();
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            IList<PlanDaoType> objList = NSession.CreateQuery("from PlanDaoType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PlanDaoType>();

            object count = NSession.CreateQuery("select count(Id) from PlanDaoType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        [HttpPost]
        public JsonResult CheckArea(string p)
        {
            //新品到货需输入密码功能宁波专用
            if (GetCurrentAccount().FromArea == "宁波")
            {
                return Json(new { IsSuccess = true});
            }
            return Json(new { IsSuccess = false });
        }
        [HttpPost]
        public JsonResult CheckPass(string p)
        {
            //到货操作判断是否新品（系统自动），当是新品时提示需输入邬丽丽密码，输入正确流到下步处理
            IList<UserType> list = base.NSession.CreateQuery("from UserType where Realname ='邬丽丽'").List<UserType>();
            if (p == list[0].Password)
            {
                return Json(new { IsSuccess = true, Msg = "成功！" });
            }
            return Json(new { IsSuccess = false, Msg = "密码错误！" });
        }
    }
}

