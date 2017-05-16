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
    public class WarehouseLineController : BaseController
    {
        //
        // GET: /WarehouseLine/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(WarehouseLineType obj)
        {
            try
            {
                obj.CreateOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.LineCode = Utilities.GetLineCode(base.NSession);
                IList<WarehouseAreaType> list =
                    NSession.CreateQuery(" from WarehouseAreaType where AreaId=:p").SetInt32("p", obj.AreaId).List<WarehouseAreaType>();
                obj.AreaCode = list[0].AreaCode;
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
        public WarehouseLineType GetById(int Id)
        {
            WarehouseLineType obj = NSession.Get<WarehouseLineType>(Id);
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
            WarehouseLineType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseLineType obj)
        {

            try
            {
                string count = NSession.CreateSQLQuery("select COUNT(LineId) from WarehouseLine where LineCode=:p1 and LineId<>:p2").SetString("p1", obj.LineCode).SetInt32("p2", obj.LineId).UniqueResult().ToString();
                if (count == "0")
                {
                    IList<WarehouseAreaType> list =
                        NSession.CreateQuery(" from WarehouseAreaType where AreaId=:p").SetInt32("p", obj.AreaId).List<WarehouseAreaType>();
                    base.NSession.CreateSQLQuery("update WarehouseRack  set LineCode=:p1 where LineId=:p2").SetString("p1", obj.LineCode).SetInt32("p2", obj.LineId).ExecuteUpdate();
                    obj.AreaCode = list[0].AreaCode;
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
                WarehouseLineType obj = GetById(id);
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
            IList<WarehouseLineType> objList = NSession.CreateQuery("from WarehouseLineType" + where )
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<WarehouseLineType>();
            object count = NSession.CreateQuery("select count(LineId) from WarehouseLineType " + where).UniqueResult();
            return Json(new { total = objList.Count, rows = objList });
        }

        public JsonResult QList()
        {
            IList<WarehouseAreaType> objList = NSession.CreateQuery("from WarehouseAreaType")
                .List<WarehouseAreaType>();
            return Json(objList);

        }

        public JsonResult PrintCode(string id)
        {
            List<WarehouseLineType> warehouseLineTypes =
                NSession.CreateQuery("from WarehouseLineType where LineId in(" + id + ")").List<WarehouseLineType>().ToList();
            if (warehouseLineTypes != null)
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("LineCode");
                foreach (WarehouseLineType obj in warehouseLineTypes)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = obj.LineCode;
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

