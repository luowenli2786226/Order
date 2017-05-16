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
    public class WarehouseAreaController : BaseController
    {
        //
        // GET: /WarehouseArea/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(WarehouseAreaType obj)
        {
            try
            {
                // 获取仓库名称
                IList<WarehouseType> list = NSession.CreateQuery(" from WarehouseType where Id=:p").SetInt32("p", obj.WId).List<WarehouseType>();

                obj.CreateOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.AreaCode = Utilities.GetAreaCode(base.NSession);
                obj.WName = list[0].WName; // 设置仓库名称

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
        public WarehouseAreaType GetById(int Id)
        {
            WarehouseAreaType obj = NSession.Get<WarehouseAreaType>(Id);
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
            WarehouseAreaType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseAreaType obj)
        {

            try
            {
                string count = NSession.CreateSQLQuery("select COUNT(AreaId) from WarehouseArea where AreaCode=:p1 and AreaId<>:p2").SetString("p1", obj.AreaCode).SetInt32("p2", obj.AreaId).UniqueResult().ToString();
                if (count == "0")
                {
                    IList<WarehouseType> list =
                    NSession.CreateQuery(" from WarehouseType where Id=:p").SetInt32("p", obj.WId).List<WarehouseType>();
                    base.NSession.CreateSQLQuery("update WarehouseLine  set AreaCode=:p1 where AreaId=:p2").SetString("p1", obj.AreaCode).SetInt32("p2", obj.AreaId).ExecuteUpdate();
                    obj.WName = list[0].WName;
                    obj.CreateOn = DateTime.Now;
                    obj.CreateBy = GetCurrentAccount().Realname;
                    NSession.Update(obj);
                    NSession.Flush();
                }
                else return Json(new { IsSuccess = false, ErrorMsg = "编码已存在！" }, "text/html", JsonRequestBehavior.AllowGet);
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
                WarehouseAreaType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            //string orderby = Utilities.OrdeerBy(sort, order);
            string where = Utilities.SqlWhere(search);
            IList<WarehouseAreaType> objList = NSession.CreateQuery("from WarehouseAreaType" + where)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<WarehouseAreaType>();
            object count = NSession.CreateQuery("select count(AreaId) from WarehouseAreaType " + where).UniqueResult();
            return Json(new { total = objList.Count, rows = objList });
        }

        public JsonResult PrintCode(string id)
        {
            List<WarehouseAreaType> warehouseAreaTypes =
                NSession.CreateQuery("from WarehouseAreaType where AreaId in(" + id + ")").List<WarehouseAreaType>().ToList();
            if (warehouseAreaTypes != null)
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("AreaCode");
                foreach (WarehouseAreaType obj in warehouseAreaTypes)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = obj.AreaCode;
                    dt.Rows.Add(dr);
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
    }
}

