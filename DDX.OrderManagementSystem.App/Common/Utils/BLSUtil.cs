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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App
{
    public class BLSUtli
    {
        public static string GetTrackCode(OrderType order)
        {
            string url = "http://42.121.252.25/api/LvsParcels";
            //string ccc = GetAnth();
            BLSRootObject root = new BLSRootObject();
            root.OrderNumber = order.OrderNo;
            root.ContractId = "1";
            root.RecipientName = order.AddressInfo.Addressee;
            root.RecipientStreet = order.AddressInfo.Street;
            root.RecipientCity = order.AddressInfo.City;
            root.RecipientZipCode = order.AddressInfo.PostCode;
            if (order.Country == "Great Britain")
            {
                order.AddressInfo.Country = "United Kingdom";
            }
            root.RecipientCountry = order.AddressInfo.Country;
            root.RecipientState = order.AddressInfo.Province;
            root.PhoneNumber = order.AddressInfo.Phone + "(" + order.AddressInfo.Tel + ")";
            root.Email = order.BuyerEmail;
            root.SenderName = "LeiGang";
            root.SenderSequence = "1";
            root.SenderAddress = "jingHua";

            root.RecipientBusnumber = "";
            root.RecipientHouseNumber = "";
            root.Customs = new List<BLSCustoms>();
            foreach (OrderProductType orderProductType in order.Products)
            {
                //EUR, GBP
                BLSCustoms blsCustoms = new BLSCustoms();
                blsCustoms.Sku = orderProductType.SKU;
                blsCustoms.ChineseContentDescription = "衣服";
                blsCustoms.ItemContent = orderProductType.SKU;
                blsCustoms.ItemCount = orderProductType.Qty.ToString();

                blsCustoms.Value = (order.Country == "Great Britain" || order.Country == "United Kingdom") ? (order.Amount > 14 ? "14" : order.Amount.ToString()) : (order.Amount > 21 ? "21" : order.Amount.ToString());
                blsCustoms.Currency = (order.Country == "Great Britain" || order.Country == "United Kingdom") ? "GBP" : "EUR";
                blsCustoms.Weight = "200";
                blsCustoms.SkuInInvoice = "";
                root.Customs.Add(blsCustoms);
                break;

            }

            string srcString = PostWebRequest(url, JsonConvert.SerializeObject(root));
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(srcString);
            return token["ProductBarcode"].ToString();
        }

        public static void GetPDF(string t, string path)
        {
            string url = "http://42.121.252.25//api/LvsLabels?productBarcode=" + t;

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, "basic YmVzdG9yZTpicG9zdA==");
            client.Headers.Add(HttpRequestHeader.ContentType, "text/json; charset=utf-8");
            client.DownloadFile(url, path);
        }

        public static string GetAnth()
        {
            string url = "http://42.121.252.25/api/GetToken";
            string str = "{\"UserId\":\"bestore \",\"Password\":\"bpost\"}";

            return PostWebRequest(url, str);
        }




        public static string PostWebRequest(string url, string paramData)
        {
            byte[] postData = Encoding.UTF8.GetBytes(paramData);//编码，尤其是汉字，事先要看下抓取网页的编码方式  


            try
            {
                WebClient webClient = new WebClient();
                //webClient.Headers.Add("Content-Type", "text/json; charset=utf-8");//采取POST方式必须加的header，如果改为GET方 
                // webClient.Headers.Add("Authentication", "emhlbmhhaTpicG9zdA==");
                webClient.Headers.Add(HttpRequestHeader.Authorization, "basic YmVzdG9yZTpicG9zdA==");
                webClient.Headers.Add(HttpRequestHeader.ContentType, "text/json; charset=utf-8");
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 

                return srcString;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}