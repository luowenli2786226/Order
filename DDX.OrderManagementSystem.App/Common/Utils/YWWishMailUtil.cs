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

namespace DDX.OrderManagementSystem.App
{
    public class YWWishMailUtil
    {
        public static string Url_api_order = "https://wishpost.wish.com/api/v2/create_order";   // 运单信息接收 API 
        public static string Url_get_pdf = " https://wishpost.wish.com/api/v2/generate_label";   // 获取 PDF  标签 API 
        public static string Api_key = "";  // API密钥
        public const string getRefreshtoken = "https://wishpost.wish.com/api/v2/oauth/access_token";

        /// <summary>
        /// 获取订单XML结构
        /// </summary>
        /// <param name="orderType">订单</param>
        /// <param name="t">类型</param>
        /// <returns></returns>
        public static string[] GetBodyXML(OrderType orderType, int t,string  pinfo)
        {
            int v = 0;
            if (orderType.LogisticMode == "wish平邮yw")
            {
                v = 4;
            }
            else
            {
                v = 1;
            }
            string otype = "0"; //WISH邮平邮
            switch (t)
            {
                case 0:
                    otype = "0"; //WISH邮平邮
                    break;
                case 1:
                    otype = "1"; //WISH邮挂号
                    break;
                case 2:
                    otype = "9-0"; //DLP平邮
                    break;
                case 3:
                    otype = "9-1"; //DLP挂号
                    break;
                case 4:
                    otype = "10-0"; //DLE
                    break;
                case 5:
                    otype = "11-0"; //E邮宝
                    break;
                case 6:
                    otype = "300-1"; //Wishpriority－US BETA
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
            //foreach (OrderProductType orderProduct in orderType.Products)
            //{
                SkuList += pinfo;
            //}
            string TitleTotle = SkuList + TitleList;
            TitleTotle = (TitleTotle.Length > 85 ? TitleTotle.Substring(0, 85) + "..." : TitleList);
            var postBody = new string[] {
                "</order>",
                "</orders>"
            };
            if (otype == "300-1")
            {
                postBody = new string[]
                                    {
                                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
                                        "<orders>",
                                        string.Format("<access_token>{0}</access_token>",Api_key),
                                        "<mark></mark>",
                                        "<bid>0</bid>",
                                        "<order>",
                                        string.Format("<guid>{0}</guid>",orderType.OrderNo),
                                        string.Format("<otype>{0}</otype>",otype),
                                        string.Format("<from>{0}</from>",""),
                                        string.Format("<sender_province>{0}</sender_province>",""),
                                        string.Format("<sender_city>{0}</sender_city>",""),
                                        string.Format("<sender_addres>{0}</sender_addres>",""),
                                        string.Format("<sender_phone>{0}</sender_phone>",""),
                                        string.Format("<to>{0}</to>",orderType.AddressInfo.Addressee),
                                        string.Format("<to_local>{0}</to_local>",orderType.AddressInfo.Addressee),
                                        string.Format("<recipient_addres>{0}</recipient_addres>",orderType.AddressInfo.Street),
                                        string.Format("<recipient_addres_local>{0}</recipient_addres_local>",orderType.AddressInfo.Street),
                                        string.Format("<recipient_country>{0}</recipient_country>",orderType.AddressInfo.Country),
                                        string.Format("<recipient_country_short>{0}</recipient_country_short>",orderType.AddressInfo.CountryCode),
                                        string.Format("<recipient_country_local>{0}</recipient_country_local>",orderType.AddressInfo.Country),
                                        string.Format("<recipient_province>{0}</recipient_province>",orderType.AddressInfo.Province),
                                        string.Format("<recipient_province_local>{0}</recipient_province_local>",orderType.AddressInfo.Province),
                                        string.Format("<recipient_city>{0}</recipient_city>",orderType.AddressInfo.City),
                                        string.Format("<recipient_city_local>{0}</recipient_city_local>",orderType.AddressInfo.City),
                                        string.Format("<recipient_postcode>{0}</recipient_postcode>",orderType.AddressInfo.PostCode),
                                        string.Format("<recipient_phone>{0}</recipient_phone>",orderType.AddressInfo.Phone == null ? "1" :orderType.AddressInfo.Phone.Replace(" ", "")),
                                        string.Format("<content>{0}</content>",SkuList),
                                        string.Format("<type_no>{0}</type_no>","4"), //1=礼品,2=文件,3=商品货样,4=其他
                                        string.Format("<weight>{0}</weight>",orderType.Weight==0 ? 0.1:(orderType.Weight/1000.00)), // 货品重量，千克（三位小数）
                                        string.Format("<num>{0}</num>",Count==0?1:Count),
                                        string.Format("<single_price>{0}</single_price>",(orderType.Amount <= 20 ? (orderType.Amount <= 5 ? orderType.Amount : 5) : 10)),
                                        string.Format("<from_country>{0}</from_country>","CHINA"),
                                        string.Format("<user_desc>{0}</user_desc>",""),
                                        string.Format("<trande_no>{0}</trande_no>",orderType.OrderExNo),
                                        string.Format("<trade_amount>{0}</trade_amount>",orderType.Amount),
                                        string.Format("<warehouse_code>{0}</warehouse_code>",v),
                                        string.Format("<product_url>{0}</product_url>","https://www.wish.com/c/579726aa9a99900b221cb2dc"),
                                        string.Format("<has_battery>{0}</has_battery>",0),
                                        "</order>",
                                        "</orders>"
                                    };
            }
            else
            {
                postBody = new string[]
                                    {
                                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
                                        "<orders>",
                                        string.Format("<access_token>{0}</access_token>",Api_key),
                                        "<mark></mark>",
                                        "<bid>0</bid>",
                                        "<order>",
                                        string.Format("<guid>{0}</guid>",orderType.OrderNo),
                                        string.Format("<otype>{0}</otype>",otype),
                                        string.Format("<from>{0}</from>",""),
                                        string.Format("<sender_province>{0}</sender_province>",""),
                                        string.Format("<sender_city>{0}</sender_city>",""),
                                        string.Format("<sender_addres>{0}</sender_addres>",""),
                                        string.Format("<sender_phone>{0}</sender_phone>",""),
                                        string.Format("<to>{0}</to>",orderType.AddressInfo.Addressee),
                                        string.Format("<to_local>{0}</to_local>",orderType.AddressInfo.Addressee),
                                        string.Format("<recipient_addres>{0}</recipient_addres>",orderType.AddressInfo.Street),
                                        string.Format("<recipient_addres_local>{0}</recipient_addres_local>",orderType.AddressInfo.Street),
                                        string.Format("<recipient_country>{0}</recipient_country>",orderType.AddressInfo.Country),
                                        string.Format("<recipient_country_short>{0}</recipient_country_short>",orderType.AddressInfo.CountryCode),
                                        string.Format("<recipient_country_local>{0}</recipient_country_local>",orderType.AddressInfo.Country),
                                        string.Format("<recipient_province>{0}</recipient_province>",orderType.AddressInfo.Province),
                                        string.Format("<recipient_province_local>{0}</recipient_province_local>",orderType.AddressInfo.Province),
                                        string.Format("<recipient_city>{0}</recipient_city>",orderType.AddressInfo.City),
                                        string.Format("<recipient_city_local>{0}</recipient_city_local>",orderType.AddressInfo.City),
                                        string.Format("<recipient_postcode>{0}</recipient_postcode>",orderType.AddressInfo.PostCode),
                                        string.Format("<recipient_phone>{0}</recipient_phone>",orderType.AddressInfo.Phone == null ? "1" :orderType.AddressInfo.Phone.Replace(" ", "")),
                                        string.Format("<content>{0}</content>",SkuList),
                                        string.Format("<type_no>{0}</type_no>","4"), //1=礼品,2=文件,3=商品货样,4=其他
                                        string.Format("<weight>{0}</weight>",(orderType.Weight/1000.00)), // 货品重量，千克（三位小数）
                                        string.Format("<num>{0}</num>",Count),
                                        string.Format("<single_price>{0}</single_price>",(orderType.Amount <= 20 ? (orderType.Amount <= 5 ? orderType.Amount : 5) : 10)),
                                        string.Format("<from_country>{0}</from_country>","CHINA"),
                                        string.Format("<user_desc>{0}</user_desc>",""),
                                        string.Format("<trande_no>{0}</trande_no>",orderType.OrderExNo),
                                        string.Format("<trade_amount>{0}</trade_amount>",orderType.Amount),
                                        string.Format("<warehouse_code>{0}</warehouse_code>",v),
                                        "</order>",
                                        "</orders>"
                                    };
            }
            return postBody;
        }
        //public static string[] GetBodyXML1(int t)
        //{
        //    var postBody = new string[]
        //                            {
        //                                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        //                                "<root>",
        //                                string.Format("<client_id>{0}</client_id>","586cc275f9db7196713ad1b7"),
        //                               string.Format("<client_secret>{0}</client_secret>","28bd81bf4bb345208a511d281e3e84f2"),
        //                               string.Format("<redirect_uri>{0}</redirect_uri>","https://127.0.0.1"),
        //                               string.Format("<grant_type>{0}</grant_type>","authorization_code"),
        //                                string.Format("<code>{0}</code>","fa5c3e4f6f374d6ba78e4f2c4fb2fc43"),
        //                                "</root>"
        //                            };
        //    return postBody;
        //}

        //public static string RefreshToken(int t)
        //{
        //    WebClient client = new WebClient();
        //    string[] strs = GetBodyXML1(t);
        //    string bodyText = "";
        //    foreach (var item in strs)
        //    {
        //        bodyText += item;
        //    }

        //    var data = Encoding.UTF8.GetBytes(bodyText);
        //    var result = client.UploadData(getRefreshtoken, "POST", data);

        //    string PostResult = Encoding.UTF8.GetString(result);
        //    string trackCode = XDocument.Parse(PostResult).Element("root").Element("access_token").Value;
        //    return trackCode;

        //}
       
        /// <summary>
        /// 获取跟踪码
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetTrackCode(OrderType orderType, int t, string pinfo, string key)
        {
            WebClient client = new WebClient();
            Api_key = key;
            string[] strs = GetBodyXML(orderType, t,pinfo);
            string bodyText = "";
            foreach (var item in strs)
            {
                bodyText += item;
            }

            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(Url_api_order, "POST", data);

            string PostResult = Encoding.UTF8.GetString(result);
            string trackCode = XDocument.Parse(PostResult).Element("root").Element("barcode").Value;
            return trackCode;
        }

        public static string GetPDF(string ids, int p)
        {
            var client = new WebClient();
            StringBuilder sb = new StringBuilder();
            for (int nI = 0; nI < ids.Split(',').Count(); nI++)
            {
                sb.Append(string.Format("<barcode>{0}</barcode>", ids.Split(',')[nI]));
            }
            var postBody = new string[]
                                    {
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            "<root>",
            string.Format("<access_token>{0}</access_token>",Api_key),
            "<printlang>1</printlang>",
            string.Format("<printcode>{0}</printcode>",p), // 标签格式(英寸)：1=A4，2=4×4 ，3=4×2(DLP专用)
            "<barcodes>",
            sb.ToString(),
            "</barcodes>",
            "</root>"
            };
            string bodyText = "";
            foreach (var item in postBody)
            {
                bodyText += item;
            }
            var data = Encoding.UTF8.GetBytes(bodyText);
            var result = client.UploadData(Url_get_pdf, "POST", data);
            string PdfResult = Encoding.UTF8.GetString(result);
            string PdfUrl = XDocument.Parse(PdfResult).Element("root").Element("PDF_URL").Value;
            return PdfUrl;
        }
    }
}