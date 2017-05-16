using System;
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
    public class WarehouseController : BaseController
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
        public JsonResult Create(WarehouseType obj)
        {
            try
            {
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WarehouseType GetById(int Id)
        {
            WarehouseType obj = NSession.Get<WarehouseType>(Id);
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
            WarehouseType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                WarehouseType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);
        }

        public JsonResult List(string sort, string order, string search)
        {
            string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            IList<WarehouseType> objList = NSession.CreateQuery("from WarehouseType" + where + orderby)
                .List<WarehouseType>();
            return Json(new { total = objList.Count, rows = objList });
        }

        public JsonResult QList()
        {
            IList<WarehouseType> objList = NSession.CreateQuery("from WarehouseType")
                .List<WarehouseType>();
            return Json(objList);

        }

        /// <summary>
        /// 自营仓库
        /// </summary>
        /// <returns></returns>
        public JsonResult QListPrivateWarehouse()
        {
            IList<WarehouseType> objList = NSession.CreateQuery("from WarehouseType where id in (1,3)")
                .List<WarehouseType>();
            return Json(objList);

        }

        public JsonResult QListFilterPacket()
        {
            IList<WarehouseType> objList = NSession.CreateQuery("from WarehouseType where type='小包仓库'")
                .List<WarehouseType>();
            return Json(objList);

        }

        public JsonResult QListSearch()
        {
            IList<WarehouseType> objList = NSession.CreateQuery("from WarehouseType order by WName")
                .List<WarehouseType>();
            objList.Insert(0, new WarehouseType() { Id = 0, WName = "==请选择==" });
            return Json(objList);
        }

        public ActionResult GetArea()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(Area)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }
    }
}
