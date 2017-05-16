using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using DDX.OrderManagementSystem.App.Common.Utils;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using Newtonsoft.Json;
using System.Collections;
using System.Web.UI;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class PackagingOperationsController : BaseController
    {
        //
        // GET: /PackagingOperations/

        public ViewResult Index()
        {
            return View();
        }
        public JsonResult List(int page, int rows)
        {
            List<PickingListType> list =
            base.NSession.CreateQuery("from PickingListType  P where State='正在包装'").SetFirstResult(rows * (page - 1))
               .SetMaxResults(rows)
               .List<PickingListType>().ToList();
            foreach (PickingListType picklist in list)
            {
                int i = Convert.ToInt32(NSession.CreateSQLQuery("select count(0) from OrderPackage where IsPrint=1 and Pickingno='" + picklist.PickingNo + "'").UniqueResult());
                picklist.PrintOrderCount = i + "/" + picklist.OrderCount;
                int j = Convert.ToInt32(NSession.CreateSQLQuery("select sum(Qty) from OrderProducts where OId in (select Id from Orders where  PickId='" + picklist.PickingNo + "' and OrderNo in(select OrderNo from OrderPackage where IsPrint=1 and  Pickingno='" + picklist.PickingNo + "') )").UniqueResult());
                picklist.ScanProduct = j + "/" + picklist.SKUcount;
            }
            return base.Json(new { total = list.Count, rows = list });
        }
        public JsonResult EndList(string PickingNo)
        {
            List<OrderPackageType> list = NSession.CreateQuery(" from OrderPackageType where PickingNo=:p and IsPrint=0").SetString("p", PickingNo).List<OrderPackageType>().ToList();
          
            return base.Json(new { total = list.Count, rows = list });
        }
        [HttpPost]
        public JsonResult Search(string PickingNo)
        {
            try
            {
                IList<PickingListType> p = NSession.CreateQuery("from PickingListType  P where PickingNo=" + PickingNo).List<PickingListType>();
                if (p == null)
                {
                    return Json(new { IsSuccess = false});
                }
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }
         [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Scan(string Id)
        {
            IList<PickingListType> plist = NSession.CreateQuery("from PickingListType  P where PickingNo='" + Id + "'").List<PickingListType>();
            PickingListType p = plist[0];
            p.State = PickingListStateEnum.正在包装.ToString();
            p.Partner = GetCurrentAccount().Realname;
            NSession.Update(p);
            NSession.Flush();
            //将数据插入到拣货单订单关联表
            List<OrderType> orderlist = NSession.CreateQuery(" from OrderType  where PickId='" + Id + "'").List<OrderType>().ToList();
            foreach (OrderType order in orderlist)
            {
                List<OrderPackageType> orderpacklist = NSession.CreateQuery(" from OrderPackageType  where OrderNo='" + order.OrderNo + "' and PickingNo='" + Id + "'").List<OrderPackageType>().ToList();
                if(orderpacklist.Count<1)
                {
                OrderPackageType orderpack = new OrderPackageType();
                orderpack.IsPrint = 0;
                orderpack.PickingNo = Id;
                orderpack.OrderNo = order.OrderNo;
                NSession.Save(orderpack);
                NSession.Flush();
                }
            }
            return View(p);
        }
        /// <summary>
        /// 显示已扫描的订单
        /// </summary>
        /// <param name="PickingNo"></param>
        /// <returns></returns>
        public JsonResult ScanGrid(string PickingNo)
        {
            IList<PickingListType> plist = NSession.CreateQuery("from PickingListType  P where PickingNo='" + PickingNo + "'").List<PickingListType>();
            if (plist.Count > 0)
            {
                PickingListType list = plist[0];
                list.orderpack = NSession.CreateQuery("from OrderPackageType  P where PickingNo='" + PickingNo + "' and Isprint=1").List<OrderPackageType>().ToList();
                object obj2 = base.NSession.CreateQuery("select count(Id) from OrderPackageType P where PickingNo='" + PickingNo + "'  and  Isprint=1").UniqueResult();
                return base.Json(new { total = obj2, rows = list.orderpack });
            }
            return base.Json(new { total = 0, rows = new List<OrderPackageType>() });
        }
        /// <summary>
        /// 结束包装界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult EndPackage(string id)
        {
            ViewData["PickingNo"] = id;
            //OrderPackageType p = NSession.Get<OrderPackageType>(Id);
            List<OrderPackageType> p = NSession.CreateQuery(" from OrderPackageType where PickingNo=:p and IsPrint=0").SetString("p", id).List<OrderPackageType>().ToList();
            if (p.Count > 0)
            {
                return View(p);
            }
            return View();
        }
        /// <summary>
        /// 强制结束
        /// </summary>
        /// <returns></returns>
        public JsonResult ForceEnd(string PickingNo)
        {
            NSession.CreateSQLQuery(" update Orders set PickId=0 where OrderNo not in (select orderno from orderpackage where PickingNo=:p and IsPrint=1) and PickId=:p").SetString("p", PickingNo).ExecuteUpdate();
            NSession.Delete(" from OrderPackageType where PickingNo='"+PickingNo+"' and IsPrint=0 ");
            NSession.Flush();
            NSession.CreateQuery(" update PickingListType set state='已包装' where  PickingNo=:p").SetString("p", PickingNo).ExecuteUpdate();
            return Json(new { IsSuccess = true });
        }
    }
}
