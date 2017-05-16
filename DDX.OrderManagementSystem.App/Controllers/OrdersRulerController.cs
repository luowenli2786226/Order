using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class OrdersRulerController : BaseController
    {
        //
        // GET: /OrdersRulers/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /OrdersRulers/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }
        public ActionResult ChooseAccount()
        {

            return View();
        }
        public ActionResult ChooseWarehouse()
        {
            return View();
        }
        //
        // GET: /OrdersRulers/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /OrdersRulers/Create

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(string actionwarehouse, string actionLogic, string IsMinusStock, string OrderChoose, string RulersName, string OrderAction)
        {
            try
            {
                //actionwarehouse = Request["actionwarehouse"].ToString();
                //actionLogic = Request["actionLogic"].ToString();
                //IsMinusStock = Request["IsMinusStock"].ToString();
                //OrderChoose = Request["OrderChoose"].ToString();
                OrdersRulersType obj = new OrdersRulersType();
                obj.Logictis = actionLogic;
                obj.WareHouse = actionwarehouse;
                obj.OrderChoose = OrderChoose;
                obj.IsMinusStock = Convert.ToInt32(IsMinusStock);
                obj.IsUse = 1;
                obj.RulersName = RulersName;
                obj.OrderAction = OrderAction;
                base.NSession.Save(obj);
                base.NSession.Flush();
                object count = NSession.CreateQuery("select count(Id) from OrdersRulersType where OrderAction='"+obj.OrderAction+"'").UniqueResult();
                obj.Priority = Convert.ToInt32(count);
                NSession.Update(obj);
                NSession.Flush();
            }
            catch
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });

        }

        //
        // GET: /OrdersRulers/Edit/5

        public ActionResult Edit(int id)
        {
            OrdersRulersType type = GetById<OrdersRulersType>(id);
            return View(type);
        }

        //
        // POST: /OrdersRulers/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, string actionwarehouse, string actionLogic, string IsMinusStock, string OrderChoose, string RulersName, string OrderAction)
        {
            try
            {
                OrdersRulersType obj = GetById<OrdersRulersType>(id);
                obj.Logictis = actionLogic;
                obj.WareHouse = actionwarehouse;
                obj.OrderChoose = OrderChoose;
                obj.IsMinusStock = Convert.ToInt32(IsMinusStock);
                obj.RulersName = RulersName;
                obj.OrderAction = OrderAction;
                base.NSession.Save(obj);
                base.NSession.Flush();
            }
            catch
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = true });
        }


        //
        // POST: /OrdersRulers/Delete/5

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                OrdersRulersType obj = GetById<OrdersRulersType>(id);
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
            if (!string.IsNullOrEmpty(search))
            {
                search = "where " + search;
            }

            IList<OrdersRulersType> objList = NSession.CreateQuery("from OrdersRulersType " + search + " order by OrderAction,Priority asc")
                           .SetFirstResult(rows * (page - 1))
                           .SetMaxResults(rows)
                           .List<OrdersRulersType>();
            object count = NSession.CreateQuery("select count(Id) from OrdersRulersType " + search).UniqueResult();
            return Json(new { total = count, rows = objList });
        }
        /// <summary>
        /// 销售平台列表
        /// </summary>
        /// <returns></returns>
        public JsonResult PlatFormList()
        {
            List<DataDictionaryDetailType> platform = NSession.CreateQuery("from DataDictionaryDetailType where DicCode='SalePlatform'").List<DataDictionaryDetailType>().ToList();
            return Json(new { data = platform });
        }
        /// <summary>
        /// 国家列表
        /// </summary>
        /// <returns></returns>
        public JsonResult ParentCountryList()
        {
            IList<ParentCountryType> objList = NSession.CreateQuery("from ParentCountryType").List<ParentCountryType>();
            IList<CountryType> objList1 = NSession.CreateQuery("from CountryType").List<CountryType>();
            IList<ParentCountryType> fristList = objList.ToList();
            SystemTree tree = new SystemTree { id = "0", text = "根目录" };
            List<SystemTree> trees = new List<SystemTree>();
            foreach (ParentCountryType item in fristList)
            {
                List<CountryType> fooList = objList1.Where(p => p.ParentId == item.Id).ToList();
                item.children = fooList;
                List<SystemTree> tree2 = ConvertToTree(fooList);
                tree.children.Add(new SystemTree { id = item.Id.ToString(), text = item.Name, children = tree2 });
            }
            trees.Add(tree);
            return Json(trees);
        }
        /// <summary>
        /// 账号列表
        /// </summary>
        /// <param name="fooList"></param>
        /// <returns></returns>
        public JsonResult ParentAccountList()
        {
            IList<object> objList = NSession.CreateSQLQuery("select distinct Platform from Account ").List<object>();
            IList<AccountType> objList1 = NSession.CreateQuery("from AccountType").List<AccountType>();
            SystemTree tree = new SystemTree { id = "0", text = "根目录" };
            IList<object> fristList = objList.ToList();
            List<SystemTree> trees = new List<SystemTree>();
            foreach (object obj in fristList)
            {
                List<AccountType> fooList = objList1.Where(p => p.Platform == obj.ToString()).ToList();
                List<SystemTree> tree2 = ConvertToTree1(fooList);
                tree.children.Add(new SystemTree { id = obj.ToString(), text = obj.ToString(), children = tree2 });
            }
            trees.Add(tree);
            return Json(trees);
        }
        public List<SystemTree> ConvertToTree(List<CountryType> fooList)
        {
            List<SystemTree> tree = new List<SystemTree>();
            foreach (CountryType item in fooList)
            {
                tree.Add(new SystemTree { id = item.Id.ToString(), text = item.CCountry });
            }
            return tree;

        }
        public List<SystemTree> ConvertToTree1(List<AccountType> fooList)
        {
            List<SystemTree> tree = new List<SystemTree>();
            foreach (AccountType item in fooList)
            {
                tree.Add(new SystemTree { id = item.Id.ToString(), text = item.AccountName });
            }
            return tree;

        }
        /// <summary>
        /// 根据销售平台列表获取账户列表
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public JsonResult AccountList(string platform)
        {
            //string platform = "";
            List<AccountType> account = NSession.CreateQuery("from AccountType where platform='" + platform + "'").List<AccountType>().ToList();
            return Json(new { data = account });
        }
        /// <summary>
        /// 根据区域列表获取国家列表
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public JsonResult CountryList(int parentId)
        {
            List<CountryType> country = NSession.CreateQuery("from CountryType where ParentId='" + parentId + "'").List<CountryType>().ToList();
            return Json(new { data = country });
        }
        /// <summary>
        /// 获取数据字典中的可选条件
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUnChooseCondition()
        {
            List<DataDictionaryDetailType> dictionary = GetList<DataDictionaryDetailType>("DicCode", "ChooseCategory", "");
            foreach (DataDictionaryDetailType d in dictionary)
            {
                d.children = GetList<DataDictionaryDetailType>("DicCode", d.DicValue, "");
            }
            return Json(new { data = dictionary });
        }
        /// <summary>
        /// 获取可选条件下的详细信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetChooDeCOn(string where)
        {
            List<DataDictionaryDetailType> dictionary = GetList<DataDictionaryDetailType>("DicCode", where, "");
            return Json(new { data = dictionary });
        }
        /// <summary>
        /// 获取币种
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCurrency()
        {
            List<CurrencyType> currency = NSession.CreateQuery("from CurrencyType ").List<CurrencyType>().ToList();
            return Json(currency, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult UpdateState(int id)
        {
            try
            {
                OrdersRulersType obj = GetById<OrdersRulersType>(id);
                int IsUse = obj.IsUse;
                if (IsUse == 1)
                {
                    obj.IsUse = 0;
                }
                else { obj.IsUse = 1; }
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }
        /// <summary>
        /// 更新优先级
        /// </summary>
        /// <returns></returns>
        public JsonResult EditPriority(int type, int Id)
        {
            try
            {
                //获取当前ID对应的元素
                OrdersRulersType obj = GetById<OrdersRulersType>(Id);
                int currentpriority = obj.Priority;
                //升级
                if (type == 0)
                {
                     //获取上一优先级对应的元素
                    List<OrdersRulersType> objpre = NSession.CreateQuery(" from OrdersRulersType where OrderAction='" + obj.OrderAction + "' and Priority='" + (currentpriority - 1) + "'").List<OrdersRulersType>().ToList();
                    if (objpre.Count > 0)
                    {
                        objpre[0].Priority =currentpriority;
                        obj.Priority = currentpriority-1;
                        NSession.Update(obj);
                        NSession.Update(objpre[0]);
                        NSession.Flush();
                    }
                }
                else//降级
                {
                    //获取下一优先级对应的元素
                    List<OrdersRulersType> objnext = NSession.CreateQuery(" from OrdersRulersType where OrderAction='" + obj.OrderAction + "' and Priority='" + (currentpriority + 1) + "'").List<OrdersRulersType>().ToList();
                    if (objnext.Count > 0)
                    {
                        objnext[0].Priority = currentpriority ;
                        obj.Priority = currentpriority + 1;
                        NSession.Update(obj);
                        NSession.Update(objnext[0]);
                        NSession.Flush();

                    }
                }

            }
            catch (Exception ex)
            {

                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

    }
}
