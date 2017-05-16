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
    public class WarehouseRackController : BaseController
    {
        //
        // GET: /WarehouseRack/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(WarehouseRackType obj)
        {
            try
            {
                obj.CreateOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.RackCode = Utilities.GetRackCode(base.NSession);
                IList<WarehouseLineType> list =
                    NSession.CreateQuery(" from WarehouseLineType where LineId=:p").SetInt32("p", obj.LineId).List<WarehouseLineType>();
                obj.LineCode = list[0].LineCode;
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
        public WarehouseRackType GetById(int Id)
        {
            WarehouseRackType obj = NSession.Get<WarehouseRackType>(Id);
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
            WarehouseRackType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseRackType obj)
        {

            try
            {
                string count = NSession.CreateSQLQuery("select COUNT(RackId) from WarehouseRack where RackCode=:p1 and RackId<>:p2").SetString("p1", obj.RackCode).SetInt32("p2", obj.RackId).UniqueResult().ToString();
                if (count == "0")
                {
                    IList<WarehouseLineType> list =
                        NSession.CreateQuery(" from WarehouseLineType where LineId=:p").SetInt32("p", obj.LineId).List<WarehouseLineType>();
                    obj.LineCode = list[0].LineCode;
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
                WarehouseRackType obj = GetById(id);
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
            IList<WarehouseRackType> objList = NSession.CreateQuery("from WarehouseRackType" + where )
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<WarehouseRackType>();
            object count = NSession.CreateQuery("select count(RackId) from WarehouseRackType " + where).UniqueResult();
            return Json(new { total = objList.Count, rows = objList });
        }

        public JsonResult QList()
        {
            IList<WarehouseLineType> objList = NSession.CreateQuery("from WarehouseLineType")
                .List<WarehouseLineType>();
            return Json(objList);

        }

        public JsonResult PrintCode(string id)
        {
            List<WarehouseRackType> warehouseRackTypes =
                NSession.CreateQuery("from WarehouseRackType where RackId in(" + id + ")").List<WarehouseRackType>().ToList();
            if (warehouseRackTypes != null)
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("RackCode");
                foreach (WarehouseRackType obj in warehouseRackTypes)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = obj.RackCode;
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

