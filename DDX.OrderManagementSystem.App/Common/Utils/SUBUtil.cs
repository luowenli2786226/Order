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
    public class SUBUtil
    {
        public static string GetTrackCode(OrderType order,string t)
        {
            string url = "http://api.sunyou.hk/order/create_order.htm";
            //string ccc = GetAnth();
            SUBOrderRootObject rootObject = new SUBOrderRootObject();
            rootObject.fullName = order.AddressInfo.Addressee;
            rootObject.addr1 = order.AddressInfo.Street;
            rootObject.addr2 = "";
            rootObject.city = order.AddressInfo.City;
            rootObject.state = order.AddressInfo.Province;
            rootObject.country = order.AddressInfo.CountryCode;
            rootObject.email = order.AddressInfo.Email;
            rootObject.hasLabel = "true";
            rootObject.phone = order.AddressInfo.Phone;
            rootObject.weight = "0.2";
            rootObject.decValue = "5";
            rootObject.typeCode = t;
            rootObject.zip = order.AddressInfo.PostCode;
            rootObject.item = new List<SUBItem>();
            foreach (var orderProductType in order.Products)
            {
                if (orderProductType.Qty > 0)
                {
                    SUBItem item = new SUBItem();
                    item.proName = orderProductType.SKU;
                    item.sku = orderProductType.SKU;
                    item.qnt = orderProductType.Qty.ToString();
                    rootObject.item.Add(item);
                }
            }
            string srcString = PostWebRequest(url, JsonConvert.SerializeObject(rootObject));

            return srcString;
        }

        public static string GetPDF(string t, string f)
        {
            return null;
            string url = "http://api.sunyou.hk/order/download_label.htm";

            Object obj = new { trNums = t.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) };

            try
            {
                var client = new WebClient();
                client.Headers.Add("ThirdParty", "false");
                client.Headers.Add("SunYou-Token", "zTR7plrMBOywOX+HZbQN8g==");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                byte[] postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
                byte[] responseData = client.UploadData(url, "POST", postData);//得到返回字符流  
                client.DownloadFile(url, f);
                return f;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetAnth()
        {
            string url = "http://42.121.252.25/api/GetToken";
            string str = "{\"UserId\":\"zhenhai\",\"Password\":\"bpost\"}";

            return PostWebRequest(url, str);
        }




        public static string PostWebRequest(string url, string paramData)
        {
            byte[] postData = Encoding.UTF8.GetBytes(paramData);//编码，尤其是汉字，事先要看下抓取网页的编码方式  


            try
            {
                WebClient webClient = new WebClient();

                webClient.Headers.Add("SunYou-Token", "zoSPlLEJc/L8AMV6VHWzsQ==");
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
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