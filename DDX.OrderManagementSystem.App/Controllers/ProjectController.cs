using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;

using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ProjectController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }


        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoAudit(int k, int a, string m)
        {
            try
            {
                ProjectType type;
                string errorMsg;
                type = base.Get<ProjectType>(k);

                type.IsAudit = a;
                type.AuditRemark += "--->" + GetCurrentAccount().Realname + ":" + m;
                if (!string.IsNullOrEmpty(type.AuditBy))
                    type.AuditBy += "--->";
                type.AuditBy += GetCurrentAccount().Realname;
                type.AuditOn = DateTime.Now;
                base.NSession.Update(type);
                base.NSession.Flush();

                //else
                //{
                //    return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
                //}

            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足 " });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoState(int k, string m)
        {
            try
            {
                ProjectType type;
                string errorMsg;
                type = base.Get<ProjectType>(k);
                type.LastOn = DateTime.Now;
                type.LastState = m;
                type.State = 1;
                if(m.IndexOf("完成")!=-1)
                {
                    type.State = 2;
                }
                ProjectStateType stateType = new ProjectStateType();
                stateType.PId = type.Id;
                stateType.CreateOn = DateTime.Now;
                stateType.CreateBy = GetCurrentAccount().Realname;
                stateType.Content = m;
                base.NSession.Save(stateType);
                base.NSession.Update(type);
                base.NSession.Flush();

                //else
                //{
                //    return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
                //}

            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足 " });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost]
        public JsonResult Create(ProjectType obj)
        {
            try
            {
                obj.CreateOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
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
        public ProjectType GetById(int Id)
        {
            ProjectType obj = NSession.Get<ProjectType>(Id);
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
            ProjectType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(ProjectType obj)
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
                ProjectType obj = GetById(id);
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
            IList<ProjectType> objList = NSession.CreateQuery("from ProjectType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<ProjectType>();

            object count = NSession.CreateQuery("select count(Id) from ProjectType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

    }
}

