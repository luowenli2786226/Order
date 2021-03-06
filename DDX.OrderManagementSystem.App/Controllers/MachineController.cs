﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class MachineController : BaseController
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
        public JsonResult Create(MachineType obj)
        {
            try
            {
                if (obj.StartDate < Convert.ToDateTime("2000-01-01"))
                {
                    obj.StartDate = Convert.ToDateTime("2000-01-01");
                }
                if (obj.EndDate < Convert.ToDateTime("2000-01-01"))
                {
                    obj.EndDate = Convert.ToDateTime("2000-01-01");
                }
                if (obj.BuyDate < Convert.ToDateTime("2000-01-01"))
                {
                    obj.BuyDate = Convert.ToDateTime("2000-01-01");
                }
                if (IsCreateOk(obj.Code))
                    return Json(new { errorMsg = "编号已经存在" });
                NSession.Save(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MachineType GetById(int Id)
        {
            MachineType obj = NSession.Get<MachineType>(Id);
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
            MachineType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(MachineType obj)
        {

            try
            {
                if (IsOk(obj.Id, obj.Code))
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
            object obj = NSession.CreateQuery("select count(Id) from MachineType where Code='" + s + "' and Id <> " + p).UniqueResult();
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
         }
        private bool IsCreateOk( string s)
        {
            object obj = NSession.CreateQuery("select count(Id) from MachineType where Code='" + s + "'").UniqueResult();
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                MachineType obj = GetById(id);
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
            IList<MachineType> objList = NSession.CreateQuery("from MachineType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<MachineType>();
            object count = NSession.CreateQuery("select count(Id) from MachineType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

    }
}

