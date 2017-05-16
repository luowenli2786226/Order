using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ExpressRecordController : BaseController
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
        public JsonResult Create(ExpressRecordType obj)
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
        public ExpressRecordType GetById(int Id)
        {
            ExpressRecordType obj = NSession.Get<ExpressRecordType>(Id);
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
            ExpressRecordType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(ExpressRecordType obj)
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
                ExpressRecordType obj = GetById(id);
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
            IList<ExpressRecordType> objList = NSession.CreateQuery("from ExpressRecordType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<ExpressRecordType>();

            object count = NSession.CreateQuery("select count(Id) from ExpressRecordType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        [HttpPost]
        public JsonResult ScanCode(string o)
        {

            ExpressRecordType express = new ExpressRecordType();
            express.CreateBy = GetCurrentAccount().Realname;
            express.CreateOn = DateTime.Now;
            express.IsVail = 0;
            express.TrackCode = o;
            NSession.Save(express);
            NSession.Flush();

            return Json(new { IsSuccess = true,Msg="快递："+o+" 记录成功！请扫描下一个！" });
        }


    }
}

