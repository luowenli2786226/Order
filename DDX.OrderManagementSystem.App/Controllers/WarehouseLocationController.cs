using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.Web.Controllers
{
    public class WarehouseLocationController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        public ActionResult LocationByScan()
        {
            return View();
        }
        public ActionResult SKUByScan()
        {
            return View();
        }

        public JsonResult PrintLocation(string c)
        {

            string[] cels = c.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            DataTable dt = new DataTable();
            dt.Columns.Add("code");

            foreach (string cel in cels)
            {
                DataRow dr = dt.NewRow();
                dr[0] = cel;

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

        [HttpPost]
        public JsonResult SetLocation(string c, string l, int j)
        {
            IList<SKUCodeType> list =
                NSession.CreateQuery("from SKUCodeType where Code=:p").SetString("p", c).SetMaxResults(1).List
                    <SKUCodeType>();
            if (list.Count > 0)
            {
                IList<ProductType> productTypes =
                    NSession.CreateQuery("from ProductType where SKU=:p").SetString("p", list[0].SKU).SetMaxResults(1).List<ProductType>();


                IList<ProductType> list2 =
                  NSession.CreateQuery("from ProductType where OldSKU=:p").SetString("p", productTypes[0].OldSKU).List<ProductType>();

                foreach (ProductType productType in list2)
                {
                    if (j == 1)
                    {
                        LoggerUtil.GetProductRecord(productType, "库位指定", productType.Location + "指定为:" + l, CurrentUser, NSession);
                        productType.Location = l;
                    }
                    else
                    {
                        LoggerUtil.GetProductRecord(productType, "库位指定", productType.Location + "指定为:" + productType.Location + "," + l, CurrentUser, NSession);
                        productType.Location = productType.Location + "," + l;
                    }
                    NSession.Update(productType);
                    NSession.Flush();
                }

                return Json(new { IsSuccess = true, Msg = list[0].SKU + " 设置库位为：" + l });
            }

            return Json(new { IsSuccess = true, Msg = "设置成功！" });
        }

        [HttpPost]
        public JsonResult Create(WarehouseLocationType obj)
        {
            try
            {
                WarehouseType warehouseType = NSession.Get<WarehouseType>(obj.Wid);
                obj.WName = warehouseType.WName;
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
        public WarehouseLocationType GetById(int Id)
        {
            WarehouseLocationType obj = NSession.Get<WarehouseLocationType>(Id);
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
            WarehouseLocationType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(WarehouseLocationType obj)
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
                WarehouseLocationType obj = GetById(id);
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
            //IList<WarehouseLocationType> objList = NSession.CreateQuery("from WarehouseLocationType " + where + orderby)
            //    .SetFirstResult(rows * (page - 1))
            //    .SetMaxResults(rows)
            //    .List<WarehouseLocationType>();

            string sql = "select WL.Id as Id,WName,Code,RackCode,Remark,SortCode from WarehouseLocation WL left join WarehouseRack WR on WL.ParentId=WR.RackId " + where + orderby;
            IList<object[]> objList = NSession.CreateSQLQuery(sql)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<object[]>();
            List<WarehouseLocationType> warehouseLocationTypes = new List<WarehouseLocationType>();
            foreach (var obj in objList)
            {
                WarehouseLocationType warehouseLocationType = new WarehouseLocationType();
                warehouseLocationType.Id = Convert.ToInt32(obj[0]);
                warehouseLocationType.WName = obj[1].ToString();
                warehouseLocationType.Code = obj[2].ToString();
                warehouseLocationType.RackCode = Convert.ToString(obj[3]);
                warehouseLocationType.Remark = Convert.ToString(obj[4]);
                warehouseLocationType.SortCode = Convert.ToInt32(obj[5]);
                warehouseLocationTypes.Add(warehouseLocationType);
            }
            //object count = NSession.CreateQuery("select count(Id) from WarehouseLocationType " + where).UniqueResult();
            object count = NSession.CreateSQLQuery("select count(Id) from WarehouseLocation WL left join WarehouseRack WR on WL.ParentId=WR.RackId" + where).UniqueResult();
            return Json(new { total = count, rows = warehouseLocationTypes });

        }

        public JsonResult ListQ(string q)
        {

            IList<WarehouseLocationType> objList = NSession.CreateQuery("from WarehouseLocationType where Code like '%" + q + "%'").SetMaxResults(20)
                .List<WarehouseLocationType>();
            return Json(new { total = objList.Count, rows = objList });
        }

    }
}

