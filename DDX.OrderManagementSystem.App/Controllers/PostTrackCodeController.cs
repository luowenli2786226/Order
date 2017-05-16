namespace DDX.OrderManagementSystem.App.Controllers
{
    using DDX.OrderManagementSystem.App;
    using DDX.OrderManagementSystem.Domain;
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.UI;

    public class PostTrackCodeController : BaseController
    {
        public ActionResult Create()
        {
            return base.View();
        }

        public ActionResult Details()
        {
            IList<object[]> objectses =
                NSession.CreateSQLQuery(
                    "select LogisticMode,count(1) as count  from PostTrackCode where isUse=0 group by LogisticMode").
                    List<object[]>();

            string html = "<ul>";

            foreach (object[] objects in objectses)
            {
                html +="<li><b>"+ objects[0] + "：" + objects[1] + "</b></li>";
            }
            html += "</ul>";
            ViewData["html"] = html;
            return base.View();
        }

        [HttpPost]
        public JsonResult Create(PostTrackCodeType obj)
        {
            try
            {
                string[] strs = obj.Code.Replace("\r", "").Split(new string[] { "\n" },
                                                                 StringSplitOptions.RemoveEmptyEntries);
                string a = GetCurrentAccount().Realname;
                DateTime b = DateTime.Now;
                foreach (string str in strs)
                {

                    object ooo = NSession.CreateQuery("select count(Id) from PostTrackCodeType where Code=:p").SetString("p", str.Trim()).UniqueResult();
                    if (Convert.ToInt32(ooo) > 0)
                    {
                        continue;
                    }
                    PostTrackCodeType postTrackCodeType = new PostTrackCodeType();
                    postTrackCodeType.Code = str.Trim();
                    postTrackCodeType.LogisticMode = obj.LogisticMode;
                    postTrackCodeType.CreateBy = GetCurrentAccount().Realname;
                    postTrackCodeType.CreateOn = b;
                    postTrackCodeType.IsUse = 0;
                    postTrackCodeType.UseOn = b;
                    NSession.SaveOrUpdate(postTrackCodeType);
                    NSession.Flush();
                }

            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        [ActionName("Delete"), HttpPost]
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                PostTrackCodeType byId = this.GetById(id);
                base.NSession.Delete(byId);
                base.NSession.Flush();
            }
            catch (Exception)
            {
                return base.Json(new { errorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost, OutputCache(Location=OutputCacheLocation.None)]
        public ActionResult Edit(PostTrackCodeType obj)
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
            PostTrackCodeType byId = this.GetById(id);
            return base.View(byId);
        }

        public PostTrackCodeType GetById(int Id)
        {
            PostTrackCodeType type = base.NSession.Get<PostTrackCodeType>(Id);
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
            IList<PostTrackCodeType> list = base.NSession.CreateQuery("from PostTrackCodeType " + str + str2).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<PostTrackCodeType>();
            object obj2 = base.NSession.CreateQuery("select count(Id) from PostTrackCodeType " + str).UniqueResult();
            return base.Json(new { total = obj2, rows = list });
        }
    }
}

