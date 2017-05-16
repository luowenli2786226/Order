namespace DDX.OrderManagementSystem.App.Controllers
{
    using DDX.OrderManagementSystem.App;
    using DDX.OrderManagementSystem.Domain;
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.UI;

    public class JiangChengController : BaseController
    {
        public ActionResult Create()
        {
            return base.View();
        }

        [HttpPost]
        public JsonResult Create(JiangChengType obj)
        {
            try
            {
                obj.CreateOn = DateTime.Now;
                obj.CreateBy = base.GetCurrentAccount().Realname;
                obj.JCOn = DateTime.Now;
                base.NSession.SaveOrUpdate(obj);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                JiangChengType byId = this.GetById(id);
                base.NSession.Delete(byId);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        public ActionResult Details()
        {
            IList<JiangChengType> list = base.NSession.CreateQuery("from JiangChengType where Area='宁波' Order by Id Desc").SetMaxResults(1).List<JiangChengType>();
            IList<JiangChengType> list2 = base.NSession.CreateQuery("from JiangChengType where Area='义乌' Order by Id Desc").SetMaxResults(1).List<JiangChengType>();
            if (list.Count == 0)
            {
                list.Add(new JiangChengType());
            }
            if (list2.Count == 0)
            {
                list2.Add(new JiangChengType());
            }
            ModelData<JiangChengType> model = new ModelData<JiangChengType> {
                m0 = list[0],
                m1 = list2[0]
            };
            return base.View("Details", model);
        }

        public ActionResult DoJC(int id, string t, string m, string c)
        {
            if (((base.GetCurrentAccount().Realname == "邵纪银") || (base.GetCurrentAccount().Realname == "管理员")) || (base.GetCurrentAccount().Realname == "雷刚"))
            {
                JiangChengType byId = this.GetById(id);
                byId.JCBy = base.GetCurrentAccount().Realname;
                byId.JCContent = c;
                byId.JCMemo = m;
                byId.JCOn = DateTime.Now;
                byId.JCType = t;
                base.NSession.SaveOrUpdate(byId);
                base.NSession.Flush();
                return base.Json(new { IsSuccess = true });
            }
            return base.Json(new { IsSuccess = false, ErrorMsg = "没有权限!" });
        }

        [OutputCache(Location=OutputCacheLocation.None), HttpPost]
        public ActionResult Edit(JiangChengType obj)
        {
            try
            {
                base.NSession.Update(obj);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [OutputCache(Location=OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            JiangChengType byId = this.GetById(id);
            return base.View(byId);
        }

        public JiangChengType GetById(int Id)
        {
            JiangChengType type = base.NSession.Get<JiangChengType>(Id);
            if (type == null)
            {
                throw new Exception("返回实体为空");
            }
            return type;
        }

        public ViewResult Index()
        {
            return base.View();
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string str = "";
            string str2 = " order by Id desc ";
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where " + str;
                }
            }
            IList<JiangChengType> list = base.NSession.CreateQuery("from JiangChengType " + str + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<JiangChengType>();
            object obj2 = base.NSession.CreateQuery("select count(Id) from JiangChengType " + str).UniqueResult();
            return base.Json(new { total = obj2, rows = list });
        }
    }
}

