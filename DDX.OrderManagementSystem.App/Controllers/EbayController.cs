﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using eBay.Service.Core.Sdk;
using eBay.Service.Call;
using eBay.Service.Core.Soap;
using System.Data.SqlClient;
using System.Data;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class EbayController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(EbayType obj)
        {
            try
            {
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { ErrorMsg = "出错了", IsSuccess = false });
            }
            return Json(new { IsSuccess = "true" });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public EbayType GetById(int Id)
        {
            EbayType obj = NSession.Get<EbayType>(Id);
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
            EbayType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(EbayType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { ErrorMsg = "出错了", IsSuccess = false });
            }
            return Json(new { IsSuccess = "true" });

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                EbayType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { ErrorMsg = "出错了", IsSuccess = false });
            }
            return Json(new { IsSuccess = "true" });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            List<EbayType> objList =
                NSession.CreateSQLQuery(
                    "select [Id],[ItemId],[ItemTitle],[Currency],[Price],[PicUrl],[StartNum],[NowNum],[ProductUrl],[StartTime],[CreateOn],[Account],[SKU],[Status],isnull((select top 1 COUNT(Id) from SKUCode where SKUCode.SKU = Ebay.SKU and IsOut=0 group by SKU),0) as UnPeiQty from Ebay " +
                    where + orderby).AddEntity(typeof(EbayType))
                    .SetFirstResult(rows*(page - 1))
                    .SetMaxResults(rows)
                    .List<EbayType>().ToList();
            object count = NSession.CreateQuery("select count(Id) from EbayType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        public JsonResult ToExcel(string search)
        {
            try
            {
                List<EbayType> objList = NSession.CreateQuery("from EbayType " + Utilities.SqlWhere(search))
                    .List<EbayType>().ToList();
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable((objList)));

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        public JsonResult Syn(string id)
        {
            try
            {
                if (id != "ALL")
                {
                    IList<AccountType> list = NSession.CreateQuery("from AccountType where Platform='Ebay' and AccountName<>''").List<AccountType>();
                    foreach (var item in list)
                    {
                        if (item.AccountName == id)
                        {
                            if (!string.IsNullOrEmpty(item.ApiToken))
                                EBayUtil.GetMyeBaySelling(item, NSession);
                            else
                                return Json(new { ErrorMsg = "该账号无 ApiToke！", IsSuccess = false });
                        }
                    }
                }
                else
                {
                    IList<AccountType> list = NSession.CreateQuery("from AccountType where Platform='Ebay' and AccountName<>'' and ApiToken<>''").List<AccountType>();
                    foreach (var item in list)
                    {
                        EBayUtil.GetMyeBaySelling(item, NSession);
                    }
                }
            }
            catch (Exception ee)
            {
                return Json(new { ErrorMsg = "出错了", IsSuccess = false });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "同步成功！" }, JsonRequestBehavior.AllowGet);
        }


    }
}

