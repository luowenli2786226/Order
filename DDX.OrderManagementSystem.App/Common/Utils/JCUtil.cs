using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using DDX.OrderManagementSystem.App.Common;
using DDX.OrderManagementSystem.Domain;
using Newtonsoft.Json;


namespace DDX.OrderManagementSystem.App
{
    public class CJPackageInfo
    {
        public string name;//内件物品英文名称
        public string count;
        public string cost;
        public string unit;
        public string currency;
        public string name_cn;//内件物品中文名称
        public string category;//内件物品英文类目名称
        public string category_cn;//内件物品中文类目名称
        public string weight;//单件重量
    }

    public class JCData
    {
        public string dispatchNumber { get; set; }
        public string shopNumber { get; set; }
        public string success { get; set; }
        public string message { get; set; }
    }

    public class JCRootObject
    {
        public string ret { get; set; }
        public string message { get; set; }
        public JCData data { get; set; }
    }

    public class JCRootPdfObject
    {
        public string ret { get; set; }
        public string message { get; set; }
        public JCDPdfata data { get; set; }
    }
    public class JCDPdfata
    {
        public string LabelURL { get; set; }
        public string numbers { get; set; }
        public string success { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// 欧亚速运
    /// </summary>
    public class JCUtil
    {
        const string apiPackageCreate = "http://www.852ex.cn/Rest1.1/Order/Create";
        const string apiPackageLabel = "http://www.852ex.cn/Rest1.1/Order/Label";

        public static string GetJCTrackCode(OrderType order, string softname, string Cname)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("account", "harborqiu@hotmail.com");
            dic.Add("secure", "188c030a22113");
            dic.Add("rettype", "json");
            dic.Add("shopNumber", order.OrderNo);
            //dic.Add("transType", "852Express");//2016-9-21 此运输方式无法获取单号
            dic.Add("transType", "OYA-UNION");
            dic.Add("addServices", "");
            dic.Add("remarks", order.OrderNo);
            List<CJPackageInfo> list = new List<CJPackageInfo>();

            foreach (var orderProductType in order.Products)
            {
                CJPackageInfo foo = new CJPackageInfo();
                foo.name = orderProductType.SKU;
                foo.currency = "USD";
                foo.unit = "lot";
                foo.count = orderProductType.Qty.ToString();
                foo.cost = "2";
                foo.category = softname;
                foo.category_cn = Cname;
                foo.name_cn = Cname;
                foo.weight = "0.1";
                list.Add(foo);
            }
            dic.Add("package_content_articles", JsonConvert.SerializeObject(list));
            dic.Add("package_weight", "0.2");
            dic.Add("package_dimension", "");
            dic.Add("package_content_remarks", "");
            dic.Add("from_name", "吕晶晶");
            dic.Add("from_tel", "0574-27904940");
            dic.Add("from_mobile", "13586559264");
            dic.Add("from_countrycode", "CN");
            dic.Add("from_province", "浙江省");
            dic.Add("from_city", "宁波市");
            dic.Add("from_address", "宁波市高新区聚贤路399号研发园1号楼20F电商部");
            dic.Add("from_postcode", "315400");

            dic.Add("to_name", order.AddressInfo.Addressee);
            dic.Add("to_tel", order.AddressInfo.Tel);
            dic.Add("to_mobile", order.AddressInfo.Phone);
            dic.Add("to_countrycode", order.AddressInfo.CountryCode);
            dic.Add("to_province", order.AddressInfo.Province);
            dic.Add("to_city", order.AddressInfo.City);
            dic.Add("to_address", order.AddressInfo.Street);
            dic.Add("to_postcode", order.AddressInfo.PostCode);



            string c = PostWebRequest(apiPackageCreate, GetParamUrl(dic));
            JCRootObject root = JsonConvert.DeserializeObject<JCRootObject>(c);
            if (root.ret == "0")
                return root.data.dispatchNumber;
            return "";
        }

        public static string GetParamUrl(Dictionary<string, string> paramDic)
        {
            string tmp = "";
            foreach (KeyValuePair<string, string> kv in paramDic)
            {
                tmp += kv.Key + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
            }
            tmp = tmp.Trim('&');
            return tmp;
        }

        public static string GetPDF(string ids)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("account", "harborqiu@hotmail.com");
            dic.Add("secure", "188c030a22113");
            dic.Add("numbers", ids);
            dic.Add("rettype", "json");
            string url = PostWebRequest(apiPackageLabel, GetParamUrl(dic));
            JCRootPdfObject root = JsonConvert.DeserializeObject<JCRootPdfObject>(url);
            if (root.ret == "0")
                return root.data.LabelURL;
            return "";

        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受    
            return true;
        }
        public static string PostWebRequest(string postUrl, string paramData, bool isFile = false, byte[] stream = null)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                // 这个可以是改变的，也可以是下面这个固定的字符串
                // 创建request对象
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(postUrl);
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                webReq.ContentType = "application/x-www-form-urlencoded";
                Stream newStream = null;
                if (isFile)
                {
                    string boundary = "—————————7d930d1a850658";
                    webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
                    webReq.ContentLength = stream.Length;
                    newStream = webReq.GetRequestStream();
                    newStream.Write(stream, 0, stream.Length);
                    newStream.Close();
                }
                else
                {
                    webReq.ContentLength = byteArray.Length;
                    newStream = webReq.GetRequestStream();
                    newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                    newStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    StreamReader sr = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                    ret = sr.ReadToEnd();
                }
            }
            return ret;
        }
    }
}