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
    public class UserSetupController : BaseController
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
        public JsonResult Create(UserSetupType obj)
        {
            try
            {
                obj.CreateOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;



                RoleEnum displayType = (RoleEnum)System.Enum.Parse(typeof(RoleEnum), obj.SetupID.ToString());
                obj.SetupName = displayType.ToString();
                UserType user = NSession.Get<UserType>(obj.UId);
                obj.RealName = user.Realname;
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
        public  UserSetupType GetById(int Id)
        {
            UserSetupType obj = NSession.Get<UserSetupType>(Id);
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
            UserSetupType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(UserSetupType obj)
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
                UserSetupType obj = GetById(id);
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
            IList<UserSetupType> objList = NSession.CreateQuery("from UserSetupType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<UserSetupType>();

            object count = NSession.CreateQuery("select count(Id) from UserSetupType " + where ).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

    }
}

