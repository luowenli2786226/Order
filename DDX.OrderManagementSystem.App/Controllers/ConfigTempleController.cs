using DDX.OrderManagementSystem.Domain.OMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ConfigTempleController : BaseController
    {
        //
        // GET: /ConfigTemple/

        public ActionResult Index()
        {
            //List<ConfigTempleType> cfgtype = base.NSession.CreateQuery("form ConfigTempleType").List<ConfigTempleType>().ToList();
            return base.View();
        }

        //
        // GET: /ConfigTemple/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }
        public JsonResult List(int page, int rows)
        {
            IList<ConfigTempleType> objList = NSession.CreateQuery("from ConfigTempleType")
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows * page)
                .List<ConfigTempleType>();

            return Json(new { total = objList.Count, rows = objList });
        }

        //
        // GET: /ConfigTemple/Create

        public ActionResult Create()
        {
         /*   List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(COnfigTempleCategory)))
            {
                //data.Add(new { id = str, text = str });
                data.Add(new { str });
            }
            ViewBag.Category =new SelectList(data);*/
            ViewData["Category"] = GenerateList();
            return View();
        }
        public static SelectList GenerateList()
        {

            List<SelectListItem> items = new List<SelectListItem>()
            {
                
                new SelectListItem(){Text="单品单件", Value="单品单件" },
                new SelectListItem(){Text="单品多件", Value="单品多件" },
                new SelectListItem(){Text="多品多件", Value="多品多件" }
            };

            SelectList generateList = new SelectList(items, "Value", "Text");
          
            return generateList;
        }
        public ActionResult GetCOnfigTempleCategory()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(COnfigTempleCategory)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        //
        // POST: /ConfigTemple/Create

        [HttpPost]
        public ActionResult Create(ConfigTempleType obj)
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

        //
        // GET: /ConfigTemple/Edit/5
        [OutputCache(Location = OutputCacheLocation.None)]

        public ActionResult Edit(int id)
        {            
            ConfigTempleType obj = GetById(id);
           /* List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(COnfigTempleCategory)))
            {
                data.Add(new { id = str, text = str });
            }
            ViewBag.Category = new SelectList(data);*/
            List<SelectListItem> items = new List<SelectListItem>()
            {
                
                new SelectListItem(){Text="单品单件", Value="单品单件" },
                new SelectListItem(){Text="单品多件", Value="单品多件" },
                new SelectListItem(){Text="多品多件", Value="多品多件" }
            };

            foreach (SelectListItem item in items)
            {
                if (item.Value == Convert.ToString(obj.Category))
                {
                    item.Selected = true;
                }
            }
            SelectList generateList = new SelectList(items, "Value", "Text");

            ViewData["Category"] = generateList;
            return View(obj);
        }

        //
        // POST: /ConfigTemple/Edit/5

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]

        public ActionResult Edit(ConfigTempleType obj, string DrpCategory)
        {
            try
            {
                obj.Category = DrpCategory;
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);

        }

             //
        // POST: /ConfigTemple/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                ConfigTempleType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ConfigTempleType GetById(long Id)
        {
            ConfigTempleType obj = NSession.Get<ConfigTempleType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }
    }
}
