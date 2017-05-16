using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml.Linq;
using DDX.OrderManagementSystem.Domain;
using DDX.OrderManagementSystem.App.Common;
using NHibernate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DDX.OrderManagementSystem.App.TeaPostApi;
using DDX.NHibernateHelper;


namespace DDX.OrderManagementSystem.App
{
    /// <summary>
    /// 亚欧快运陆运挂号
    /// </summary>
    public class TeaPostUtil
    {
        /// <summary>
        /// 在线订单操作 URL
        /// </summary>
        public static string Url_api_order = "http://api.tea-post.com/OrderOnline/ws/OrderOnlineService.dll?wsdl";

        /// <summary>
        /// 在线订单工具 URL
        /// </summary>
        public static string Url_get_pdf = "http://api.tea-post.com/OrderOnlineTool/ws/OrderOnlineToolService.dll?wsdl";

        /// <summary>
        /// Token
        /// </summary>
        public static string Api_key = "EC8CEF01ACBA66B6D9D9EDCF1CDBA157";

        /// <summary>
        /// 获取订单XML结构
        /// </summary>
        /// <param name="orderType">订单</param>
        /// <param name="t">类型</param>
        /// <returns></returns>
        public static string[] GetBodyXML(OrderType orderType, int t)
        {
            string otype = "0"; //产品编码
            switch (t)
            {
                case 0:
                    otype = "AK"; //亚欧快运陆运挂号
                    break;
            }

            string TitleList = ""; // 商品名称
            string SkuList = ""; // 商品SKU
            int Count = 0; // 数量
            foreach (OrderProductType orderProduct in orderType.Products)
            {
                //TitleList += orderProduct.Title + " "; //不显示物品名称
                Count += orderProduct.Qty;
            }
            foreach (OrderProductType orderProduct in orderType.Products)
            {
                SkuList += "[" + orderProduct.SKU + "]x" + orderProduct.Qty + " ";
            }
            string TitleTotle = SkuList + TitleList;
            TitleTotle = (TitleTotle.Length > 85 ? TitleTotle.Substring(0, 85) + "..." : TitleList);
            ///////////////////////////

            var postBody = new string[]
                                    {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<CreateOrderService>",
            string.Format("<AuthToken>{0}</AuthToken>",Api_key),
            "<CreateOrderServiceRequestArray>",
            "<CreateOrderServiceRequest>",
            string.Format("<OrderNo>{0}</OrderNo>",orderType.OrderNo),
            //string.Format("<TrackingNumber>{0}</TrackingNumber>",Api_key),
            string.Format("<ProductCode>{0}</ProductCode>",otype),
            //string.Format("<CargoCode>{0}</CargoCode>",Api_key),
            //string.Format("<PaymentCode>{0}</PaymentCode>",Api_key),
            //string.Format("<InitialCountryCode>{0}</InitialCountryCode>",Api_key),
            string.Format("<DestinationCountryCode>{0}</DestinationCountryCode>",orderType.AddressInfo.CountryCode), // 目标国代码
            //string.Format("<Pieces>{0}</Pieces>",Api_key),
            //string.Format("<InsurType>{0}</InsurType>",Api_key),
            //string.Format("<InsurValue> {0} </InsurValue>",Api_key),
            //string.Format("<BuyerId>{0}</BuyerId>",Api_key),
            //string.Format("<ReturnSign>{0}</ReturnSign>",Api_key),
            //string.Format("<CustomerWeight> {0} </CustomerWeight>",Api_key),
            //string.Format("<TransactionId>{0}</TransactionId>",Api_key),
            //string.Format("<ShipperCompanyName>{0}</ShipperCompanyName>",Api_key),
            string.Format("<ShipperName>{0}</ShipperName>","Jing"),
            string.Format("<ShipperAddress>{0}</ShipperAddress>","CHINA ZHEJIANG NINGBO JUXIAN ROAD NO.399 B1 BULIDING 20TH"),
            string.Format("<ShipperTelephone>{0}</ShipperTelephone>","18505885815"),
            //string.Format("<ShipperFax>{0}</ShipperFax>",Api_key),
            //string.Format("<ShipperPostCode>{0}</ShipperPostCode>",Api_key),
            //string.Format("<ConsigneeCompanyName>{0}</ConsigneeCompanyName>",Api_key),
            string.Format("<ConsigneeName>{0}</ConsigneeName>",orderType.AddressInfo.Addressee),
            string.Format("<Street>{0}</Street>",orderType.AddressInfo.Street),
            string.Format("<City>{0}</City>",orderType.AddressInfo.City),
            string.Format("<StateOrProvince>{0}</StateOrProvince>",orderType.AddressInfo.Province),
            string.Format("<ConsigneeTelephone>{0}</ConsigneeTelephone>",orderType.AddressInfo.Phone),
            //string.Format("<ConsigneeFax>{0}</ConsigneeFax>",Api_key),
            string.Format("<ConsigneePostCode>{0}</ConsigneePostCode>",orderType.AddressInfo.PostCode),
            string.Format("<ConsigneeEmail>{0}</ConsigneeEmail>",orderType.AddressInfo.Email),
            //string.Format("<Note>{0}</Note>",Api_key),
            //string.Format("<DeclareInvoiceArray>",Api_key),
            //string.Format("<DeclareInvoice>",Api_key),
            string.Format("<EName>{0}</EName>",TitleList),
            //string.Format("<Name>{0}</Name>",Api_key),
            string.Format("<DeclareUnitCode>{0}</DeclareUnitCode>","PCE"),
            //string.Format("<DeclarePieces>{0}</DeclarePieces>",Api_key),
            string.Format("<UnitPrice>{0}</UnitPrice>",(orderType.Amount <= 20 ? (orderType.Amount <= 5 ? orderType.Amount : 5) : 10)),
            //string.Format("<DeclareNote>{0}</DeclareNote>",Api_key),
            "</DeclareInvoice>",
            "</DeclareInvoiceArray>",
            "</CreateOrderServiceRequest>",
            "</CreateOrderServiceRequestArray>",
            "</CreateOrderService>"};

            return postBody;
        }

        /// <summary>
        /// 获取跟踪码
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetTrackCode(OrderType orderType, int t)
        {
            ISession NSession = NhbHelper.OpenSession();
            //cn.com.webxml.webservice.ForexRmbRateWebService server = new cn.com.webxml.webservice.ForexRmbRateWebService();
            //TeaPostApi.modifyOrderServiceCompletedEventHandler order = new orderItem();OrderType orderType, int t
            IList<OrderType> list = NSession.CreateQuery("from OrderType where OrderNo='1153594'").List<OrderType>();
            createOrderRequest[] request;
            request = new createOrderRequest[1];
            OrderOnlineServiceImplService service = new OrderOnlineServiceImplService();
            request[0] = new createOrderRequest();
            //request[0].destinationCountryCode = list[0].AddressInfo.CountryCode;
            //request[0].shipperName = "Jing";
            //request[0].shipperAddress = "CHINA ZHEJIANG NINGBO JUXIAN ROAD NO.399 B1 BULIDING 20TH";
            //request[0].shipperTelephone = "18505885815";
            //request[0].
            //service.t

            service.UserAgent = Api_key;
            //service.createAndPreAlertOrderService
            //service.

            var obj = service.createOrderService(service.Url, request);

            return "";

            //WebClient client = new WebClient();
            //string[] strs = GetBodyXML(orderType, t);
            //string bodyText = "";
            //foreach (var item in strs)
            //{
            //    bodyText += item;
            //}

            //var data = Encoding.UTF8.GetBytes(bodyText);
            //var result = client.UploadData(Url_api_order, "POST", data);

            //string PostResult = Encoding.UTF8.GetString(result);
            //string trackCode = XDocument.Parse(PostResult).Element("root").Element("barcode").Value;
            //return trackCode;
        }

    }
}