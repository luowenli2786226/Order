using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using DDX.OrderManagementSystem.Domain;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using DDX.OrderManagementSystem.App.com.cdiscount.wsvc;

namespace DDX.OrderManagementSystem.App.Common
{
    public static class CdiscountUtil
    {

        public class SecuredService
        {
            //private static string Url_GetOrderList = "http://www.cdiscount.com/IMarketplaceAPIService/GetOrderList";
            //private static string _username = "meilaike01@163.com-api";
            //private static string _password = "@p?hR!vZn#x&lr";

            public static string GetTokenInteg(string username, string password)
            {
                const string svcIssue = "https://sts.cdiscount.com/users/httpIssue.svc";
                const string svcToCall = "https://wsvc.cdiscount.com/FulfillmentAPIService.svc";
                string tokenId = null;

                // validation du certificat coté client
                ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

                string encoded = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", username, password)));

                var stsUri = new Uri(string.Format("{0}/?realm={1}", svcIssue, svcToCall));
                var request = (HttpWebRequest)WebRequest.Create(stsUri);
                request.Headers.Add("Authorization", string.Format("Basic={0}", encoded));
                request.Method = "GET";

                var response = request.GetResponse();
                var tokenStream = response.GetResponseStream();

                if (tokenStream != null)
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(string));
                    tokenId = (string)dataContractSerializer.ReadObject(tokenStream);
                }

                return tokenId;
            }

            public static OrderListMessage GetOrderList(string _token, DateTime _BeginCreationDate, DateTime _EndCreationDate)
            {
                //_marketplaceApiServiceSecured
                com.cdiscount.wsvc.MarketplaceAPIService _marketplaceApiServiceSecured = new MarketplaceAPIService();
                var header = new HeaderMessage
                {
                    Context = new ContextMessage
                    {
                        CatalogID = 1,
                        CustomerId = 123,
                        CustomerPoolID = 1,
                        CustomerNumber = "122",
                        SiteID = 100
                    },
                    Localization = new LocalizationMessage
                    {
                        Country = Country.Fr,
                        Currency = com.cdiscount.wsvc.Currency.Eur,
                        DecimalPosition = 2,
                        Language = Language.Fr
                    },
                    Version = "1.0",
                    Security = new SecurityContext
                    {
                        TokenId = _token
                    }
                };
                var orderFilter = new OrderFilter
                {
                    BeginCreationDate = _BeginCreationDate,
                    //BeginModificationDate = _BeginCreationDate,
                    EndCreationDate = _EndCreationDate,
                    //EndModificationDate = _EndCreationDate,
                    FetchOrderLines = true,
                    States = new OrderStateEnum[] { 
                        OrderStateEnum.CancelledByCustomer, 
                        OrderStateEnum.WaitingForSellerAcceptation, 
                        OrderStateEnum.AcceptedBySeller, 
                        OrderStateEnum.PaymentInProgress, 
                        OrderStateEnum.WaitingForShipmentAcceptation , 
                        OrderStateEnum.Shipped, 
                        OrderStateEnum.RefusedBySeller, 
                        OrderStateEnum.AutomaticCancellation, 
                        OrderStateEnum.PaymentRefused, 
                        OrderStateEnum.ShipmentRefusedBySeller, 
                        OrderStateEnum.RefusedNoShipment 
                    }
                };
                OrderListMessage OrderList = _marketplaceApiServiceSecured.GetOrderList(header, orderFilter);

                return OrderList;
            }
        }
    }
}