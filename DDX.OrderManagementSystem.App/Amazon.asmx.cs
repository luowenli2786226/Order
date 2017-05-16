using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using DDX.NHibernateHelper;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App
{
    /// <summary>
    /// Amazon 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Amazon : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public UserType GetUser(string u, string p)
        {
            ISession NSession = NhbHelper.OpenSession();
            UserType userType = new UserType();
            IList<UserType> list = NSession.CreateQuery(" from  UserType where Username=:p1 and Password=:p2 and DeletionStateCode=0").SetString("p1", u).SetString("p2", p).List<UserType>();
            if (list.Count > 0)
            {   //登录成功
                userType = list[0];

            }
            NSession.Close();
            return userType;
        }

        [WebMethod]
        public List<AccountType> GetAccount()
        {
            ISession NSession = NhbHelper.OpenSession();
            UserType userType = new UserType();
            List<AccountType> list = NSession.CreateQuery(" from  AccountType where  Platform='Amazon'").List<AccountType>().ToList();

            return list;
        }

        [WebMethod]
        public bool CreateOrderByAmazon(OrderType ot)
        {
            ISession NSession = NhbHelper.OpenSession();
            bool isExist = OrderHelper.IsExist(ot.OrderExNo, NSession);
            if (!isExist)
            {
                NSession.Save(ot.AddressInfo);
                NSession.Flush();
                ot.AddressId = ot.AddressInfo.Id;
                NSession.Save(ot);
                NSession.Flush();
                foreach (var orderProductType in ot.Products)
                {
                    orderProductType.OId = ot.Id;
                    orderProductType.OrderNo = ot.OrderNo;
                    NSession.Save(orderProductType);
                    NSession.Flush();
                }
            }
            NSession.Close();
            return isExist;

        }

        [WebMethod]
        public bool IsOrderExist(string o)
        {
            ISession NSession = NhbHelper.OpenSession();
            bool isExist = OrderHelper.IsExist(o, NSession);
            NSession.Close();

            return isExist;
        }

        [WebMethod]
        public List<ResultInfo> Sync(AccountType account, DateTime st, DateTime et)
        {
            ISession NSession = NhbHelper.OpenSession();
            List<ResultInfo> list = OrderHelper.APIByAmazon(account, st, et, NSession);
            return list;
        }

        [WebMethod]
        public string GetOrderNo()
        {

            ISession NSession = NhbHelper.OpenSession();
            string code = Utilities.GetOrderNo(NSession);
            NSession.Close();

            return code;
        }
    }
}
