using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class FixedRateController : BaseController
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
        public JsonResult Create(FixedRateType obj)
        {
            try
            {
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.CreateOn = DateTime.Now;

                IList<CurrencyType> list = NSession.CreateQuery("from CurrencyType where CurrencyCode='" + obj.CurrencyCode + "'").List<CurrencyType>();
                obj.CurrencyName = list[0].CurrencyName;

                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        public JsonResult UpdateRate()
        {
            try
            {
                cn.com.webxml.webservice.ForexRmbRateWebService server = new cn.com.webxml.webservice.ForexRmbRateWebService();
                DataSet ds = server.getForexRmbRate();


                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    FixedRateType c = new FixedRateType();
                    c.CurrencyCode = dr["Symbol"].ToString();
                    c.CurrencyName = dr["Name"].ToString();
                    NSession.Delete(" from FixedRateType where CurrencyCode='" + c.CurrencyCode + "'");
                    NSession.Flush();
                    //c.CurrencySign = "";
                    string str = dr["fBuyPrice"].ToString();
                    if (string.IsNullOrEmpty(str))
                    {
                        str = "0";
                    }
                    c.CurrencyValue = Math.Round(Convert.ToDecimal(str) / 100, 5);
                    c.CreateOn = DateTime.Now; ;
                    NSession.Save(c);
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult GetCurrency(string id)
        {
            List<DataDictionaryDetailType> list = GetList<DataDictionaryDetailType>("DicCode", id, "");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public FixedRateType GetById(int Id)
        {
            FixedRateType obj = NSession.Get<FixedRateType>(Id);
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
            FixedRateType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(FixedRateType obj)
        {

            try
            {
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.CreateOn = DateTime.Now;

                IList<CurrencyType> list = NSession.CreateQuery("from CurrencyType where CurrencyCode='" + obj.CurrencyCode + "'").List<CurrencyType>();
                obj.CurrencyName = list[0].CurrencyName;

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
                FixedRateType obj = GetById(id);
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
            IList<FixedRateType> objList = NSession.CreateQuery("from FixedRateType" + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows * page)
                .List<FixedRateType>();
            object count = NSession.CreateQuery("select count(Id) from FixedRateType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        public JsonResult QList()
        {

            IList<FixedRateType> objList = NSession.CreateQuery("from FixedRateType")

                .List<FixedRateType>();

            return Json(new { total = objList.Count, rows = objList });
        }
    }
}

