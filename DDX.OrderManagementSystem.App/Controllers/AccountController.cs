using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App.Common;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class AccountController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ViewResult AmountIndex()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public JsonResult Create(AccountType obj)
        {
            try
            {
                if (IsCreateOk(obj.AccountName, obj.Platform))
                    return Json(new { errorMsg = "此账号已存在！" });

                if (obj.Platform == "Aliexpress")
                {
                    if (!string.IsNullOrEmpty(obj.ApiToken) && obj.ApiToken.Trim().Length > 0)
                        obj.ApiToken = AliUtil.GetToken(obj.ApiKey, obj.ApiSecret, obj.ApiToken.Trim());
                }
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        private bool IsCreateOk(string name, string platform)
        {
            object obj = NSession.CreateQuery("select count(Id) from  AccountType where AccountName='" + name + "' and Platform='" + platform + "'").UniqueResult();
            if (Convert.ToInt32(obj) > 0)
            {
                return true;
            }
            return false;
        }
        public JsonResult UpdateAPI(int id, string code)
        {
            try
            {
                AccountType accountType = Get<AccountType>(id);
                accountType.ApiToken = AliUtil.GetToken(accountType.ApiKey, accountType.ApiSecret, code.Trim());
                NSession.SaveOrUpdate(accountType);
                NSession.Flush();
                return Json(new { IsSuccess = true, Result = AliUtil.GetAuthUrl(accountType.ApiKey, accountType.ApiSecret) });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }


        public JsonResult GetWishLoginUrlById(int id)
        {
            try
            {
                AccountType accountType = Get<AccountType>(id);
                return Json(new { IsSuccess = true, Result = "https://merchant.wish.com/oauth/authorize?client_id=" + accountType.ApiKey });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }

        public JsonResult GetAliLoginUrlById(int id)
        {
            try
            {
                AccountType accountType = Get<AccountType>(id);
                return Json(new { IsSuccess = true, Result = AliUtil.GetAuthUrl(accountType.ApiKey, accountType.ApiSecret) });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }

        public JsonResult GetAliLoginUrl(string k, string s)
        {
            try
            {
                return Json(new { IsSuccess = true, Result = AliUtil.GetAuthUrl(k, s) });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }

        public JsonResult GetAccountListByPlatform(string id, string t)
        {
            IList<AccountType> objList = NSession.CreateQuery("from AccountType where Platform=:p1").SetString("p1", id)
              .List<AccountType>();
            if (t == "1")
            {
                objList.Insert(0, new AccountType { AccountName = "===请选择===", Id = 0 });
            }
            else if (t == "2")
            {
                objList.Insert(0, new AccountType { AccountName = "===请选择===", Id = 0 });
                objList.Insert(1, new AccountType { AccountName = "ALL", Id = 1 });
            }
            return Json(new { total = objList.Count, rows = objList });
        }

        public JsonResult EditPic(int pid, string pic)
        {
            AccountType productType = Get<AccountType>(pid);
            productType.AgreementPic = pic;
            NSession.Update(productType);
            NSession.Flush();
            return Json(new { IsSuccess = true });
        }

        public JsonResult GetEbayLoginUrl()
        {
            try
            {
                string url = "https://signin.ebay.com/ws/eBayISAPI.dll?SignIn&RuName={0}&SessID={1}";
                string code = EBayUtil.GetSessionID();
                url = string.Format(url, AppSettingHelper.GetGenericApiContext("US").RuName, code);
                return Json(new { IsSuccess = true, Result = url, Code = code });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了：" + ee.Message });
            }
        }
        public JsonResult UpdateWishAPI(int id, string code)
        {
            try
            {
                AccountType accountType = NSession.Get<AccountType>(id);
                accountType.ApiToken = WishUtil.GetToken(accountType.ApiKey, accountType.ApiSecret, code.Trim());
                NSession.SaveOrUpdate(accountType);
                NSession.Flush();
                return Json(new { IsSuccess = true, Result = WishUtil.GetAuthUrl(accountType.ApiKey, accountType.ApiSecret) });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }
        public JsonResult GetEbaySession(string o)
        {
            try
            {
                return Json(new { IsSuccess = true, Result = EBayUtil.GetToKen(o) });
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AccountType GetById(int Id)
        {
            AccountType obj = NSession.Get<AccountType>(Id);
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
            AccountType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(AccountType obj)
        {

            try
            {
                //EBayUtil.GetMyeBaySelling(obj);
                if (!IsOk(obj.Id, obj.AccountName, obj.Platform))
                    return Json(new { errorMsg = "此账号已存在！" });
                NSession.Update(obj);
                NSession.Flush();
                LoggerUtil.GetOrderRecord(0, obj.AccountName, "修改账户", "key：" + obj.ApiKey + "S：" + obj.ApiSecret + " T：" + obj.ApiToken, GetCurrentAccount(), NSession);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });

        }

        private bool IsOk(int id, string name, string platform)
        {
            object obj = NSession.CreateQuery("select count(Id) from  AccountType where AccountName='" + name + "' and Platform='" + platform + "' and Id<>'" + id + "'").UniqueResult();
            if (Convert.ToInt32(obj) == 0)
            {
                return true;
            }
            return false;
        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                AccountType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            string orderby = " Order By AccountName asc";
            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {

                orderby = " order by " + sort + " " + order;
            }
            string where = Utilities.SqlWhere(search);
            IList<AccountType> objList = NSession.CreateQuery("from AccountType" + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows * page)
                .List<AccountType>();
            object count = NSession.CreateQuery("select count(Id) from AccountType" + where).UniqueResult();
            IList<object[]> listAmount = base.NSession.CreateQuery("select CurrencyCode,SUM(Amount),Account from OrderType where  Enabled=1 and  Status not in( '待处理','作废订单') and Account <> 'su-smt'  group by Account,CurrencyCode").List<object[]>();
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
            foreach (AccountType accountType in objList)
            {
                accountType.Amount1 = 0;
                foreach (object[] objectse in listAmount)
                {
                    if (objectse[2].ToString() == accountType.AccountName)
                    {
                        if (objectse[0].ToString() != "USD")
                        {
                            CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == objectse[0].ToString());

                            if (currencyType != null || currencyType2 != null)
                            {
                                accountType.Amount1 +=
                                    Math.Round(Utilities.ToDecimal(objectse[1]) * Convert.ToDecimal(currencyType.CurrencyValue) / Convert.ToDecimal(currencyType2.CurrencyValue),
                                               2);
                            }

                        }
                        else
                        {
                            accountType.Amount1 += Utilities.ToDecimal(objectse[1]);
                        }

                    }
                }
                accountType.Amount1 = Math.Round(accountType.Amount1, 2);
                NSession.Update(accountType);
                NSession.Flush();
            }
            List<AccountType> footer = new List<AccountType>();
            footer.Add(new AccountType { Amount1 = objList.Sum(x => x.Amount1), Amount2 = objList.Sum(x => x.Amount2), Amount3 = objList.Sum(x => x.Amount3), Amount4 = objList.Sum(x => x.Amount4), Amount5 = objList.Sum(x => x.Amount5), });
            return Json(new { total = count, rows = objList, footer = footer });
        }
        public JsonResult ListQ(string q)
        {
            if (string.IsNullOrEmpty(q))
                q = "";
            IList<AccountType> objList = NSession.CreateQuery("from AccountType where AccountName like '%" + q + "%'").List<AccountType>();

            return Json(objList.OrderBy(x => x.AccountName));
        }

        public JsonResult SelectList(int Id)
        {
            IList<AccountType> objList = NSession.CreateQuery("from AccountType").List<AccountType>();
            //获得这个类型的菜单权限
            List<PermissionScopeType> scopeList = NSession.CreateQuery("from PermissionScopeType where ResourceCategory=:p1 and ResourceId=:p2 and TargetCategory =:p3").SetString("p1", ResourceCategoryEnum.User.ToString()).SetInt32("p2", Id).SetString("p3", TargetCategoryEnum.Account.ToString()).List<PermissionScopeType>().ToList<PermissionScopeType>();
            List<SystemTree> tree = new List<SystemTree>(); ;
            SystemTree root = new SystemTree { id = "0", text = "所有账户" };
            List<DataDictionaryDetailType> list = GetList<DataDictionaryDetailType>("DicCode", "SalePlatform", "");
            foreach (DataDictionaryDetailType item in list)
            {
                List<AccountType> fooList = objList.Where(p => p.Platform == item.DicValue).OrderByDescending(p => p.AccountName).ToList();
                List<SystemTree> tree2 = ConvertToTree(fooList, scopeList);

                if (scopeList.FindIndex(p => p.TargetId == Convert.ToInt32((PlatformEnum)Enum.Parse(typeof(PlatformEnum), item.DicValue))) >= 0)
                {

                    root.children.Add(new SystemTree { id = Convert.ToInt32((PlatformEnum)Enum.Parse(typeof(PlatformEnum), item.DicValue)).ToString(), text = item.DicValue, children = tree2, @checked = true });
                }
                else
                {
                    root.children.Add(new SystemTree { id = Convert.ToInt32((PlatformEnum)Enum.Parse(typeof(PlatformEnum), item.DicValue)).ToString(), text = item.DicValue, children = tree2 });
                }
            }

            tree.Add(root);
            return Json(tree);
        }

        public List<SystemTree> ConvertToTree(List<AccountType> fooList, List<PermissionScopeType> scopeList = null)
        {
            List<SystemTree> tree = new List<SystemTree>();
            foreach (AccountType item in fooList)
            {
                if (scopeList == null)
                    tree.Add(new SystemTree { id = item.Id.ToString(), text = item.AccountName });
                else
                {
                    if (scopeList.FindIndex(p => p.TargetId == item.Id) >= 0)
                    {
                        tree.Add(new SystemTree { id = item.Id.ToString(), text = item.AccountName, @checked = true });
                    }
                    else
                    {
                        tree.Add(new SystemTree { id = item.Id.ToString(), text = item.AccountName });
                    }
                }
            }
            return tree;
        }
        /// <summary>
        /// 当前登录人是否是乐方妍，是则可以编辑
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public JsonResult IsEditTixian()
        {
            if (string.Equals(GetCurrentAccount().Realname,"乐方妍"))
            {
                return Json(new { IsSuccess = true, Msg = "成功！" });
            }

            return Json(new { IsSuccess = false, Msg = "异常！" });
        }
    }
}

