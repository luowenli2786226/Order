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
using DDX.NHibernateHelper;

namespace DDX.OrderManagementSystem.App
{
    public class YWUtil
    {
        public static string UserId = "27000004";
        public static string UserYWId = "223757";
        public static string ServiceEndPoint = "HTTP://ONLINE.YW56.COM.CN/SERVICE";
        public static string ApiToken = "MjcwMDAwMDQ6MTExMTExMTE=";
        public static string ApiToken1 = "MjIzNzU3OjIyMzc1NzIyMzc1Nw==";
        public static string[] GetBodyXML(OrderType orderType, int t)
        {
            string ccc = "中邮北京平邮小包";
            switch (t)
            {
                case 0:
                    ccc = "中邮北京平邮小包";
                    break;
                case 1:
                    ccc = "中邮上海平邮小包";
                    break;
                case 2:
                    ccc = "中邮北京挂号小包";
                    break;
                case 3:
                    ccc = "新加坡邮政挂号小包";
                    break;
                case 4:
                    ccc = "中邮上海挂号小包";
                    break;
                case 5:
                    ccc = "中邮宁波平邮小包";
                    break;
                case 6:
                    ccc = "中邮杭州平邮小包";
                    break;
                case 7:
                    ccc = "马来西亚邮政挂号小包(新)";
                    break;
                case 8:
                    ccc = "中邮上海E邮宝(线下)";
                    break;
                default:
                    ccc = "中邮北京平邮小包";
                    break;

            }
            string str = "";
            string softname = "";
            string Cname = "";
            foreach (var orderProductType in orderType.Products)
            {
                str += orderProductType.SKU + "*" + orderProductType.Qty + " ";
            }

            ISession NSession = NhbHelper.GetCurrentSession();
            IList<ProductType> product = NSession.CreateQuery("from ProductType where sku='" + orderType.Products[0].SKU + "'").List<ProductType>();
            //IList<ProductCategoryType> category = NSession.CreateQuery("from ProductCategoryType where Category=':p1'").SetString("p1", product[0].Category).List<ProductCategoryType>();


            IList<ProductCategoryType> objList = NSession.CreateQuery("from ProductCategoryType").List<ProductCategoryType>();
            IList<ProductCategoryType> category = objList.Where(p => p.Name == product[0].Category).ToList();
            NSession.Flush();

            if (category.Count > 0)
            {
                softname = category[0].EName;
               // Cname = category[0].Name;//中文品名
            }
            if (product.Count > 0)
            {
                Cname = product[0].ProductName;
            }
            var postBody = new string[]
                                    {
                                        "<ExpressType>",
                                        string.Format("  <Epcode>{0}</Epcode>",string.Format("Test{0}", new Random().Next())),
                                        string.Format("  <Userid>{0}</Userid>",UserId),
                                         string.Format(   "  <Channel>{0}</Channel>",ccc),
                                        "  <UserOrderNumber>"+t+"-"+orderType.OrderNo+"</UserOrderNumber>",
                                        string.Format("  <SendDate>{0}T00:00:00</SendDate>",DateTime.Now.ToString("yyyy-MM-dd")),
                                        "  <Receiver>",
                                        string.Format("    <Userid>{0}</Userid>",UserId),
                                        "    <Name>"+orderType.AddressInfo.Addressee+"</Name>",
                                        "    <Phone>"+orderType.AddressInfo.Phone+"</Phone>",
                                        "    <Mobile>"+orderType.AddressInfo.Tel+"</Mobile>",
                                        "    <Email>"+orderType.AddressInfo.Email+"</Email>",
                                        "    <Company></Company>",
                                        "    <Country>"+orderType.AddressInfo.Country+"</Country>",
                                        "    <Postcode>"+orderType.AddressInfo.PostCode+"</Postcode>",
                                        "    <State>"+orderType.AddressInfo.Province+"</State>",
                                        "    <City>"+orderType.AddressInfo.City+"</City>",
                                        "    <Address1>"+orderType.AddressInfo.Street+"</Address1>",
                                        "    <Address2>"+orderType.AddressInfo.County+"</Address2>",
                                        "  </Receiver>",
                                        "  <Memo></Memo>",
                                        "  <Quantity>1</Quantity>",
                                        "  <GoodsName>",
                                        string.Format("    <Userid>{0}</Userid>",UserId),
                                        "    <NameCh>多媒体播放器</NameCh>",
                                        "    <NameEn>MedialPlayer</NameEn>",
                                        "    <Weight>213</Weight>",
                                        "    <DeclaredValue>5</DeclaredValue>",
                                        "    <DeclaredCurrency>USD</DeclaredCurrency>",
                                        "  </GoodsName>",
                                        "  <GoodsName>",
                                        string.Format("<Userid>{0}</Userid>",UserId),
                                        "    <MoreGoodsName>"+str+"</MoreGoodsName>",
                                        "    <NameCh>"+Cname+"</NameCh>",
                                        "    <NameEn>"+softname+"</NameEn>",
                                        "    <Weight>200</Weight>",
                                        "    <DeclaredValue>"+(orderType.Amount <= 20 ? (orderType.Amount <= 5 ? orderType.Amount : 5) : 10)+"</DeclaredValue>",
                                        "    <DeclaredCurrency>USD</DeclaredCurrency>",
                                        "  </GoodsName>",
                                        "</ExpressType>"
                                    };
            return postBody;
        }

        public static string[] GetBodyXML1(OrderType orderType, int t)
        {
            string ccc = "中邮北京平邮小包";
            switch (t)
            {
                case 0:
                    ccc = "中邮北京平邮小包";
                    break;
                case 1:
                    ccc = "中邮上海平邮小包";
                    break;
                case 2:
                    ccc = "中邮北京挂号小包";
                    break;
                case 3:
                    ccc = "新加坡邮政挂号小包";
                    break;
                case 6:
                    ccc = "中邮杭州平邮小包";
                    break;
                case 5:
                    ccc = "中邮宁波平邮小包";
                    break;
                default:
                    ccc = "中邮北京平邮小包";
                    break;

            }
            string str = "";
            string str1 = "";
            string softname = "";
            foreach (var orderProductType in orderType.Products)
            {
                str += orderProductType.SKU + "*" + orderProductType.Qty + " ";
                
            }

            ISession NSession = NhbHelper.GetCurrentSession();
            IList<ProductType> product = NSession.CreateQuery("from ProductType where sku='" + orderType.Products[0].SKU + "'").List<ProductType>();

            IList<ProductCategoryType> objList = NSession.CreateQuery("from ProductCategoryType").List<ProductCategoryType>();
            IList<ProductCategoryType> category = objList.Where(p => p.Name == product[0].Category).ToList();
            NSession.Flush();
            if (category.Count > 0)
            {
                softname = category[0].EName;
               // str1 = category[0].Name;//中文品名
            }
            if (product.Count > 0)
            {
                str1 = product[0].ProductName;
            }
            var postBody = new string[]
                                    {
                                        "<ExpressType>",
                                        string.Format("  <Epcode>{0}</Epcode>",string.Format("Test{0}", new Random().Next())),
                                        string.Format("  <Userid>{0}</Userid>",UserYWId),
                                         string.Format(   "  <Channel>{0}</Channel>",ccc),
                                        "  <UserOrderNumber>"+t+"-"+orderType.OrderNo+"</UserOrderNumber>",
                                        string.Format("  <SendDate>{0}T00:00:00</SendDate>",DateTime.Now.ToString("yyyy-MM-dd")),
                                        "  <Receiver>",
                                        string.Format("    <Userid>{0}</Userid>",UserYWId),
                                        "    <Name>"+orderType.AddressInfo.Addressee+"</Name>",
                                        "    <Phone>"+orderType.AddressInfo.Phone+"</Phone>",
                                        "    <Mobile>"+orderType.AddressInfo.Tel+"</Mobile>",
                                        "    <Email>"+orderType.AddressInfo.Email+"</Email>",
                                        "    <Company></Company>",
                                        "    <Country>"+orderType.AddressInfo.Country+"</Country>",
                                        "    <Postcode>"+orderType.AddressInfo.PostCode+"</Postcode>",
                                        "    <State>"+orderType.AddressInfo.Province+"</State>",
                                        "    <City>"+orderType.AddressInfo.City+"</City>",
                                        "    <Address1>"+orderType.AddressInfo.Street+"</Address1>",
                                        "    <Address2>"+orderType.AddressInfo.County+"</Address2>",
                                        "  </Receiver>",
                                        "  <Memo></Memo>",
                                        "  <Quantity>1</Quantity>",
                                        "  <GoodsName>",
                                        string.Format("    <Userid>{0}</Userid>",UserYWId),
                                        "    <NameCh>多媒体播放器</NameCh>",
                                        "    <NameEn>MedialPlayer</NameEn>",
                                        "    <Weight>213</Weight>",
                                        "    <DeclaredValue>5</DeclaredValue>",
                                        "    <DeclaredCurrency>USD</DeclaredCurrency>",
                                        "  </GoodsName>",
                                        "  <GoodsName>",
                                        string.Format("<Userid>{0}</Userid>",UserYWId),
                                        "    <MoreGoodsName>"+str+"</MoreGoodsName>",
                                        "    <NameCh>"+str1+"</NameCh>",
                                        "    <NameEn>"+softname+"</NameEn>",
                                        "    <Weight>200</Weight>",
                                        "    <DeclaredValue>5</DeclaredValue>",
                                        "    <DeclaredCurrency>USD</DeclaredCurrency>",
                                        "  </GoodsName>",
                                        "</ExpressType>"
                                    };
            return postBody;
        }


        public static string GetTrackCode(OrderType orderType, int t)
        {
            var url = string.Format("{0}/Users/{1}/Expresses", ServiceEndPoint, UserId);

            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, "basic " + ApiToken);
            client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8");
            string[] strs = GetBodyXML(orderType, t);
            string bodyText = "";
            foreach (var item in strs)
            {
                bodyText += item;
            }

            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(url, "POST", data);

            string PostResult = Encoding.UTF8.GetString(result);

            //string trackCode = XDocument.Parse(PostResult).Element("CreateExpressResponseType").Element("CreatedExpress").Element("Epcode").Value;
            string trackCode = "";
            try
            {
                trackCode = XDocument.Parse(PostResult).Element("CreateExpressResponseType").Element("CreatedExpress").Element("Epcode").Value;
            }
            catch
            {
                trackCode = XDocument.Parse(PostResult).Element("CreateExpressResponseType").Element("Response").Element("ReasonMessage").Value;
            }
            return trackCode;
        }

        public static string GetTrackCode1(OrderType orderType, int t)
        {
            var url = string.Format("{0}/Users/{1}/Expresses", ServiceEndPoint, UserYWId);

            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, "basic " + ApiToken1);
            client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8");
            string[] strs = GetBodyXML1(orderType, t);
            string bodyText = "";
            foreach (var item in strs)
            {
                bodyText += item;
            }

            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(url, "POST", data);

            string PostResult = Encoding.UTF8.GetString(result);

            string trackCode = XDocument.Parse(PostResult).Element("CreateExpressResponseType").Element("CreatedExpress").Element("Epcode").Value;
            return trackCode;
        }


        public static byte[] GetPDF(string ids, int p)
        {
            var labelSize = "A10x10LCI";
            var url = string.Format("{0}/Users/{1}/Expresses/{2}Label", ServiceEndPoint, UserId, labelSize);
            var client = new WebClient();
            //client.UseDefaultCredentials = true;
            client.Headers.Add(HttpRequestHeader.Authorization, "basic " + ApiToken);
            client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8");
            string bodyText = "<string>" + ids + "</string>";
            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(url, "POST", data);
            return result;
        }

        public static byte[] GetPDF1(string ids, int p)
        {
            var labelSize = "A10x10LCI";
            var url = string.Format("{0}/Users/{1}/Expresses/{2}Label", ServiceEndPoint, UserYWId, labelSize);
            var client = new WebClient();
            //client.UseDefaultCredentials = true;
            client.Headers.Add(HttpRequestHeader.Authorization, "basic " + ApiToken1);
            client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8");
            string bodyText = "<string>" + ids + "</string>";
            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(url, "POST", data);
            return result;
        }
        public static byte[] GetPDF2(string ids, int p)
        {
            var labelSize = "A10x10LC";
            var url = string.Format("{0}/Users/{1}/Expresses/{2}Label", ServiceEndPoint, UserId, labelSize);
            var client = new WebClient();
            //client.UseDefaultCredentials = true;
            client.Headers.Add(HttpRequestHeader.Authorization, "basic " + ApiToken);
            client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8");
            string bodyText = "<string>" + ids + "</string>";
            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(url, "POST", data);
            return result;
        }

    }
}