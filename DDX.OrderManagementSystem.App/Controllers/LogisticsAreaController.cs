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
    public class LogisticsAreaController : BaseController
    {
        public ViewResult Index(int id)
        {
             Session["lid"]= id;
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(LogisticsAreaType obj)
        {
            obj.LId = int.Parse(Session["lid"].ToString());
            try
            {
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
        public  LogisticsAreaType GetById(int Id)
        {
            LogisticsAreaType obj = NSession.Get<LogisticsAreaType>(Id);
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
            LogisticsAreaType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(LogisticsAreaType obj)
        {
            obj.LId = int.Parse(Session["lid"].ToString());
           
            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
           
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {
          
            try
            {
                LogisticsAreaType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true  });
        }

        public JsonResult List(int page, int rows)
        {
            IList<LogisticsAreaType> objList = NSession.CreateQuery("from LogisticsAreaType")
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<LogisticsAreaType>();
			
            object count = NSession.CreateQuery("select count(Id) from LogisticsAreaType ").UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        /// <summary>
        /// 根据LId获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetByLId()
        {
            IList<LogisticsAreaType> objList = NSession.CreateQuery("from LogisticsAreaType c where c.LId=:lid")
                .SetInt32("lid",int.Parse(Session["lid"].ToString()))
                .List<LogisticsAreaType>();
            return Json(objList,JsonRequestBehavior.AllowGet);
        }

    }
}

