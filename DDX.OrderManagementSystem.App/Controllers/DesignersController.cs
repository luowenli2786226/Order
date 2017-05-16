using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;

namespace DDX.OrderManagementSystem.App.Controllers
{
   //美工项目管理
    public class DesignersController : BaseController
    {
        //
        // GET: /Designers/

        public ActionResult Index()
        {
            HttpContext.Session["login"] = base.CurrentUser.Realname;
            HttpContext.Session["RoleName"] = base.CurrentUser.RoleName;
            return View();
        }

        //
        // GET: /Designers/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Designers/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Designers/Create


        [HttpPost]
        public JsonResult Create(string contentitle, DateTime lasttime)
        {
            try
            {
                DesignerType designer = new DesignerType();
                designer.Contentitle = contentitle;
                designer.Lasttime = lasttime;
                designer.Apllicant = base.CurrentUser.Realname;
                designer.Apllicantime = DateTime.Now;
                designer.Audittime = SqlDateTime.MinValue.Value;
                designer.Expectedtime = SqlDateTime.MinValue.Value;
                designer.Finishtime = SqlDateTime.MinValue.Value;
                designer.Receivingtime = SqlDateTime.MinValue.Value;
                if (ModelState.IsValid)
                {
                    NSession.SaveOrUpdate(designer);
                    NSession.Flush();
                }
            }

            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }


        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            DesignerType obj = GetById(id);
            return View(obj);
        }

     
        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public JsonResult Edit(string contentitle, DateTime lasttime, int designerid)
        {
            try
            {
                DesignerType designer = base.Get<DesignerType>(designerid);
                designer.Contentitle = contentitle;
                designer.Lasttime = lasttime;
                //designer.Audittime = SqlDateTime.MinValue.Value;
                //designer.Expectedtime = SqlDateTime.MinValue.Value;
                //designer.Finishtime = SqlDateTime.MinValue.Value;
                //designer.Receivingtime = SqlDateTime.MinValue.Value;
                base.NSession.Update(designer);
                base.NSession.Update(designer);
                base.NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        [HttpPost]
        public JsonResult Edit_auditnotes(string auditnotes, int designerid, int m)
        {

            try
            {
                DesignerType designer = GetById(designerid);
                designer.Auditor = base.CurrentUser.Realname;
                designer.Audittime = DateTime.Now;
                designer.Auditnotes = auditnotes;
                if (m == 1)
                {
                    designer.Auditstatus = "同意";
                }
                if (m == 2)
                {
                    if (designer.Auditstatus == null)
                    {
                        designer.Auditstatus = "拒绝";
                    }


                }
                if (m == 3)
                {
                    if (designer.Receiptor == null)
                    {
                        designer.Auditstatus = null;
                        designer.Auditor = null;
                        designer.Audittime = SqlDateTime.MinValue.Value;
                        designer.Auditnotes = null;
                    }
                }

                NSession.Update(designer);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }



        [HttpPost]
        public JsonResult Edit_receiptor(string receivenotes, int designerid, int m, DateTime expectedtime)
        {

            try
            {
                DesignerType designer = GetById(designerid);
                designer.Receiptor = base.CurrentUser.Realname;
                designer.Expectedtime = expectedtime;
                designer.Receivingtime = DateTime.Now;
                designer.Receivenotes = receivenotes;
                if (m == 1)
                {
                    designer.Receivingsate = "已领取";
                }
                if (m == 2)
                {
                    designer.Receivingsate = "拒绝领取";
                }
                NSession.Update(designer);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        [HttpPost]
        public JsonResult Edit_finish(int designerid, int m)
        {
            try
            {
                DesignerType designer = GetById(designerid);
                if (m == 1)
                {
                    designer.Finishtime = DateTime.Now;
                }
                if (m == 2)
                {

                }
                NSession.Update(designer);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        //
        // POST: /Designers/Delete/5

        [HttpPost]
        public ActionResult Delete(int designerid)
        {
            try
            {
                DesignerType obj = GetById(designerid);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        public DesignerType GetById(int Id)
        {
            DesignerType obj = NSession.Get<DesignerType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }

        [HttpGet]
        public JsonResult About(string search, int page, int rows)
        {
            string where = "";
            string orderby = " order by Auditstatus desc, Audittime desc ";
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            IList<DesignerType> objList = NSession.CreateQuery("from DesignerType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<DesignerType>();
            foreach (var obj in objList)
            {
                if (obj.Lasttime == SqlDateTime.MinValue.Value || obj.Lasttime == DateTime.MinValue)
                { obj.Lasttime = null; }
                if (obj.Audittime == SqlDateTime.MinValue.Value || obj.Audittime == DateTime.MinValue)
                { obj.Audittime = null; }
                if (obj.Apllicantime == SqlDateTime.MinValue.Value || obj.Apllicantime == DateTime.MinValue)
                { obj.Apllicantime = null; }
                if (obj.Expectedtime == SqlDateTime.MinValue.Value || obj.Expectedtime == DateTime.MinValue)
                { obj.Expectedtime = null; }
                if (obj.Receivingtime == SqlDateTime.MinValue.Value || obj.Receivingtime == DateTime.MinValue)
                { obj.Receivingtime = null; }
                if (obj.Finishtime == SqlDateTime.MinValue.Value || obj.Finishtime == DateTime.MinValue)
                { obj.Finishtime = null; }
            }

            object count = NSession.CreateQuery("select count(DesignerID) from DesignerType " + where).UniqueResult();
            return Json(new { total = count, rows = objList }, JsonRequestBehavior.AllowGet);
        }
    }
}
