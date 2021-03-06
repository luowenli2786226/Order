﻿using System;
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
    public class CountryController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(CountryType obj)
        {
            try
            {
                if(IsCreateOk(obj.ECountry))
                return Json(new { errorMsg = "编号已经存在" });
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
        }

        private bool IsCreateOk(string s)
        {
            object obj = NSession.CreateQuery("select count(Id) from CountryType where ECountry='" + s + "'").UniqueResult();
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CountryType GetById(int Id)
        {
            CountryType obj = NSession.Get<CountryType>(Id);
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
            CountryType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(CountryType obj)
        {
            try
            {
                if (IsOk(obj.Id,obj.ECountry))
                    return Json(new { errorMsg = "编号已经存在" });
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
        }

        private bool IsOk(int p, string s)
        {
            object obj = NSession.CreateQuery("select count(Id) from CountryType where ECountry='" + s + "' and Id <> " + p).UniqueResult();
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }
        [HttpPost]
        public ActionResult ToExcel()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select * from Country ";
            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql;
            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);
            // 设置编码和附件格式 
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return Json(new { IsSuccess = true });
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                CountryType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            IList<CountryType> objList = NSession.CreateQuery("from CountryType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<CountryType>();

            object count = NSession.CreateQuery("select count(Id) from CountryType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        public JsonResult ListALL(string q)
        {
            IList<CountryType> objList = NSession.CreateQuery("from CountryType where ECountry like '%" + q + "%' order by ECountry asc")
                .List<CountryType>();
            return Json(new { total = objList.Count, rows = objList });
        }

    }
}

