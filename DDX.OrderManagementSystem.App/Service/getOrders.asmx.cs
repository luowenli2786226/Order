using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using NHibernate;
using System.Web.Script.Serialization;
using System.Web.Services.Protocols;
using DDX.OrderManagementSystem.Domain.OMS.Entities;

namespace DDX.OrderManagementSystem.App.Service
{
    /// <summary>
    /// getOrders 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [SoapDocumentService(RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class getOrders : BaseController
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        /// <summary>
        /// 获取账户和时间
        /// </summary>
        /// <param name="SkuCode"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetOrders(string Platform, string Account, DateTime st, DateTime et)
        {

            int id = Convert.ToInt32(base.NSession.CreateSQLQuery(string.Format("select Id from Account where AccountName = '" + Account + "'")).UniqueResult());
            //将账号转换为数组
            int[] account = { id };
            AccountType accountType = base.NSession.Get<AccountType>(Convert.ToInt32(id));
            //防止因为账号问题程序停止
            if (Platform.ToString() == "Wish" && accountType.FromArea == "宁波")
            {
                return Account + " " + "暂时不获取Wish平台订单";
            }
            else if (Platform.ToString() == "Lazada" && accountType.ApiSecret == null)
            {
                return Account + "账户有问题";
            }
            else if (Platform.ToString() == "Aliexpress" || Platform.ToString() == "Amazon")
            {
                if (string.IsNullOrEmpty(accountType.ApiToken) && string.IsNullOrEmpty(accountType.ApiKey) && string.IsNullOrEmpty(accountType.ApiSecret))
                {
                    return Account + "账户有问题";
                }
            }
            OrderController orderController = new OrderController();
            //json转换为string,否则会报错
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string str = serializer.Serialize(orderController.Synchronous(Platform, account, st, et));
            String[] a = str.Split(',');
            return Account + " " + a[2] + a[3];


        }
    }
}
