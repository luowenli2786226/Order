using DDX.OrderManagementSystem.App.Common.json;
using DDX.OrderManagementSystem.Domain;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace DDX.OrderManagementSystem.App.Common.Utils
{
    public class NBGHUtil
    {
        const string GetCodeUrl = "http://shipping.11185.cn:9000/produceWeb/barCodesAssgineServlet";
        const string CreateOrderUrl = "http://shipping.11185.cn:8000/mqrysrv/OrderImportMultiServlet";
        const string GetConfigUrl = "http://shipping.11185.cn:8000/mqrysrv/OrderImportGetDataServlet?queryType=queryCompany";
        const string LogisticsBizUrl = "http://shipping.11185.cn:8000/mqrysrv/OrderImportGetDataServlet?queryType=queryBusinessType";
        const string key = "Yypv3PE10744b19afdpC";
        const string ecCompanyId = "33020101576000|31516249";
        const string key1 = "821riJZAQqWC1sW54ozC";
        const string ecCompanyId1 = "33020101707000|31512743";
        const string key2 = "rD475mo5x2GFv6Y5k1l6";
        const string ecCompanyId2 = "80000002024693|32207685";
        /// <summary>
        /// 获取配置数据
        /// </summary>
        public static LogisticsCompany GetConfigInfo()
        {
            string LogisticsCompany = PostWebRequest(GetConfigUrl, "");
            LogisticsCompany c = Newtonsoft.Json.JsonConvert.DeserializeObject<LogisticsCompany>(LogisticsCompany);
            return c;
        }
        public static string GetLogisticsBiz()
        {
            string LogisticsCompany = PostWebRequest(LogisticsBizUrl, "");
            return LogisticsCompany;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<LogisticsCompany>(LogisticsCompany);
        }
        public static string CreateOrder(OrderType OrderType, int t)
        {
            string postString = "";
            if (OrderType.Account.IndexOf("yw") != -1)
            {
                postString = @"<logisticsEventsRequest>
<logisticsEvent>
<eventHeader>
<eventType>LOGISTICS_BATCH_SEND</eventType>
<eventTime>{0}</eventTime>
<eventSource>YOULIAN</eventSource>
<eventTarget>CPG</eventTarget>
</eventHeader>
<eventBody>
<order>
<orderInfos>
<product>
<productNameCN>毛巾</productNameCN>
<productNameEN>towel</productNameEN>
<productQantity>1</productQantity>
<productCateCN>日常用品</productCateCN>
<productCateEN>daily supplies</productCateEN>
<productId>180231254</productId>
<producingArea>CN</producingArea>
<productWeight>1000</productWeight>
<productPrice>15</productPrice>
</product>
<product>
<productNameCN>牙刷</productNameCN>
<productNameEN> toothbrush </productNameEN>
<productQantity>2</productQantity>
<productCateCN>日常用品</productCateCN>
<productCateEN>daily supplies</productCateEN>
<productId>180231255</productId>
<producingArea>CN</producingArea>
<productWeight>2000</productWeight>
<productPrice>30</productPrice>
</product>
</orderInfos>
<ecCompanyId>{1}</ecCompanyId>
<whCode></whCode>
<logisticsOrderId>{2}</logisticsOrderId>
<isItemDiscard>true</isItemDiscard>
<tradeId></tradeId>
<mailNo>{3}</mailNo>
<LogisticsCompany>POST</LogisticsCompany>
<LogisticsBiz>{11}</LogisticsBiz>
<ReceiveAgentCode>123345</ReceiveAgentCode>
<Rcountry>{4}</Rcountry>
<Rprovince>{5}</Rprovince>
<Rcity>{6}</Rcity>
<Raddress>{7}</Raddress>
<Rpostcode>{8}</Rpostcode>
<Rname>{9}</Rname>
<Rphone>{10}</Rphone>
<Sname>BaoJun</Sname>
<Sprovince>ZheJiang</Sprovince>
<Scity>YiWu</Scity>
<Saddress>Seller's Union Group,No.531,North Zongze Road</Saddress>
<Sphone>18329076923</Sphone>
<Spostcode>322000</Spostcode>
<insureValue>1</insureValue>
<insuranceValue>1</insuranceValue>
<remark></remark>
<channel></channel>
<Itotleweight>1000</Itotleweight>
<Itotlevalue>1000</Itotlevalue>
<totleweight>1100</totleweight>
<hasBattery>0</hasBattery>
<country>CN</country>
<mailKind>3</mailKind>
<mailClass>l</mailClass>
<batchNo>231254412333</batchNo>
<mailType>YOULIAN</mailType>
<faceType>2</faceType>
<undeliveryOption>2</undeliveryOption>
</order>
</eventBody>
</logisticsEvent>
</logisticsEventsRequest>";
            }
            else
            {
                postString = @"<logisticsEventsRequest>
<logisticsEvent>
<eventHeader>
<eventType>LOGISTICS_BATCH_SEND</eventType>
<eventTime>{0}</eventTime>
<eventSource>YOUSHENG</eventSource>
<eventTarget>CPG</eventTarget>
</eventHeader>
<eventBody>
<order>
<orderInfos>
<product>
<productNameCN>毛巾</productNameCN>
<productNameEN>towel</productNameEN>
<productQantity>1</productQantity>
<productCateCN>日常用品</productCateCN>
<productCateEN>daily supplies</productCateEN>
<productId>180231254</productId>
<producingArea>CN</producingArea>
<productWeight>1000</productWeight>
<productPrice>1500</productPrice>
</product>
<product>
<productNameCN>牙刷</productNameCN>
<productNameEN> toothbrush </productNameEN>
<productQantity>2</productQantity>
<productCateCN>日常用品</productCateCN>
<productCateEN>daily supplies</productCateEN>
<productId>180231255</productId>
<producingArea>CN</producingArea>
<productWeight>2000</productWeight>
<productPrice>3000</productPrice>
</product>
</orderInfos>
<ecCompanyId>{1}</ecCompanyId>
<whCode></whCode>
<logisticsOrderId>{2}</logisticsOrderId>
<isItemDiscard>true</isItemDiscard>
<tradeId></tradeId>
<mailNo>{3}</mailNo>
<LogisticsCompany>POST</LogisticsCompany>
<LogisticsBiz>04</LogisticsBiz>
<ReceiveAgentCode>123345</ReceiveAgentCode>
<Rcountry>{4}</Rcountry>
<Rprovince>{5}</Rprovince>
<Rcity>{6}</Rcity>
<Raddress>{7}</Raddress>
<Rpostcode>{8}</Rpostcode>
<Rname>{9}</Rname>
<Rphone>{10}</Rphone>
<Sname>吕晶晶</Sname>
<Sprovince>浙江</Sprovince>
<Scity>宁波</Scity>
<Saddress>宁波市鄞州区聚贤路1226号均胜集团4号3层</Saddress>
<Sphone>15988173792</Sphone>
<Spostcode>315101</Spostcode>
<insureValue>1</insureValue>
<insuranceValue>1</insuranceValue>
<remark></remark>
<channel></channel>
<Itotleweight>1000</Itotleweight>
<Itotlevalue>1000</Itotlevalue>
<totleweight>1100</totleweight>
<hasBattery>0</hasBattery>
<country>CN</country>
<mailKind>3</mailKind>
<mailClass>l</mailClass>
<batchNo>231254412333</batchNo>
<mailType>YOUSHENG</mailType>
<faceType>2</faceType>
<undeliveryOption>2</undeliveryOption>
</order>
</eventBody>
</logisticsEvent>
</logisticsEventsRequest>";
            }
            if (t == 2)
            {
                postString = string.Format(postString, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ecCompanyId2, OrderType.OrderExNo, OrderType.TrackCode, OrderType.AddressInfo.CountryCode == null ? "" : OrderType.AddressInfo.CountryCode.Replace("UK", "GB"), OrderType.AddressInfo.Province == null ? "" : OrderType.AddressInfo.Province.Replace("'", " "), OrderType.AddressInfo.City.Replace("'", " ").Replace("-", " "), OrderType.AddressInfo.Street.Replace(" ", "").Replace("'", " ").Replace("&", " "), OrderType.AddressInfo.PostCode.ToString().Replace("'", " ").Trim(), OrderType.AddressInfo.Addressee.Replace("'", " "), OrderType.AddressInfo.Phone == null ? "" : OrderType.AddressInfo.Phone.Replace(" ", "").Replace("+", ""), "04");
            }
            else if (t == 3)
            {
                postString = string.Format(postString, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ecCompanyId2, OrderType.OrderExNo, OrderType.TrackCode, OrderType.AddressInfo.CountryCode == null ? "" : OrderType.AddressInfo.CountryCode.Replace("UK", "GB"), OrderType.AddressInfo.Province == null ? "" : OrderType.AddressInfo.Province.Replace("'", " "), OrderType.AddressInfo.City.Replace("'", " "), OrderType.AddressInfo.Street.Replace(" ", "").Replace("'", " ").Replace("&", " "), OrderType.AddressInfo.PostCode.ToString().Replace("'", " ").Trim(), OrderType.AddressInfo.Addressee.Replace("'", " "), OrderType.AddressInfo.Phone == null ? "" : OrderType.AddressInfo.Phone.Replace(" ", "").Replace("+", ""), "253");
            }
            else if (t == 4)
            {
                postString = string.Format(postString, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ecCompanyId2, OrderType.OrderExNo, OrderType.TrackCode, OrderType.AddressInfo.CountryCode == null ? "" : OrderType.AddressInfo.CountryCode.Replace("UK", "GB"), OrderType.AddressInfo.Province == null ? "" : OrderType.AddressInfo.Province.Replace("'", " "), OrderType.AddressInfo.City.Replace("'", " "), OrderType.AddressInfo.Street.Replace(" ", "").Replace("'", " ").Replace("&", " "), OrderType.AddressInfo.PostCode.ToString().Replace("'", " ").Trim(), OrderType.AddressInfo.Addressee.Replace("'", " "), OrderType.AddressInfo.Phone == null ? "" : OrderType.AddressInfo.Phone.Replace(" ", "").Replace("+", ""), "05");
            }
            else
            {
                if (OrderType.Account.IndexOf("yw") != -1)
                {
                    postString = string.Format(postString, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ecCompanyId1, OrderType.OrderExNo, OrderType.TrackCode, OrderType.AddressInfo.CountryCode == null ? "" : OrderType.AddressInfo.CountryCode, OrderType.AddressInfo.Province == null ? "" : OrderType.AddressInfo.Province, OrderType.AddressInfo.City, OrderType.AddressInfo.Street.Replace(" ", "").Replace("'", " ").Replace("&", " "), OrderType.AddressInfo.PostCode.ToString().Replace("'", " ").Trim(), OrderType.AddressInfo.Addressee.Replace("'", " "), OrderType.AddressInfo.Phone == null ? "" : OrderType.AddressInfo.Phone.Replace(" ", ""));
                }
                else
                    postString = string.Format(postString, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ecCompanyId, OrderType.OrderExNo, OrderType.TrackCode, OrderType.AddressInfo.CountryCode == null ? "" : OrderType.AddressInfo.CountryCode, OrderType.AddressInfo.Province, OrderType.AddressInfo.City, OrderType.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), OrderType.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(), OrderType.AddressInfo.Addressee.Replace("'", "&apos;"), OrderType.AddressInfo.Phone);
                {
                }
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("logistics_interface", postString);
            string md5request = "";
            if (t == 2 || t == 3 || t ==4)
            {
                md5request = ChMd5Encrypt(postString + key2);
                dic.Add("ecCompanyId", ecCompanyId2);
            }
            else
            {
                if (OrderType.Account.IndexOf("yw") != -1)
                {
                    md5request = ChMd5Encrypt(postString + key1);
                    dic.Add("ecCompanyId", ecCompanyId1);
                }
                else
                {
                    md5request = ChMd5Encrypt(postString + key);
                    dic.Add("ecCompanyId", ecCompanyId);
                }
            }
            dic.Add("data_digest", md5request);
            dic.Add("msg_type", "B2C_TRADE");
            dic.Add("version", "2.0");
            string retuemsg = PostWebRequest(CreateOrderUrl, SMTConfig.GetParamUrl(dic));
            XElement root = XElement.Parse(retuemsg);
            string o = root.Value;
            return root.Value;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<NBGHReturnType>(retuemsg);
            // return retuemsg;
        }

        public NBGHReturnType GetreturnMsg(OrderType order, int t)
        {
            LogisticsCompany c = GetConfigInfo();


            RequestParamType param = new RequestParamType();
            if (t == 2 || t ==3 || t==4)
            {
                param.ecCompanyId = ecCompanyId2;
                param.mailType = "YOULIAN";
            }
            else
            {
                if (order.Account.IndexOf("yw") != -1)
                {
                    param.ecCompanyId = ecCompanyId1;
                    param.mailType = "YOULIAN";
                }
                else
                {
                    param.ecCompanyId = ecCompanyId;
                    param.mailType = "YOUSHENG";
                }
            }
            param.eventTime = DateTime.Now.ToString();
            param.whCode = "";
            param.logisticsOrderId = order.OrderExNo; ;

            param.tradeId = "";
            param.LogisticsCompany = "POST";
            if (t == 3)
            {
                param.LogisticsBiz = "253";
            }
            else if (t == 4)
            {
                param.LogisticsBiz = "05";
            }
            else
            {
                param.LogisticsBiz = "04";
            }
            param.faceType = 1;

            string request = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            request = "{\"order\":[" + request + "]}";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("logisticsOrder", request);
            string md5request = "";
            if (t == 2 || t == 3 || t == 4)
            {
                md5request = ChMd5Encrypt(request + key2);
                dic.Add("ecCompanyId", ecCompanyId2);
            }
            else
            {
                if (order.Account.IndexOf("yw") != -1)
                {
                    md5request = ChMd5Encrypt(request + key1);
                    dic.Add("ecCompanyId", ecCompanyId1);
                }
                else
                {
                    md5request = ChMd5Encrypt(request + key);
                    dic.Add("ecCompanyId", ecCompanyId);
                }
            }
            dic.Add("data_digest", md5request);
            dic.Add("msg_type", "B2C_TRADE");
            dic.Add("version", "1.0");

            string retuemsg = PostWebRequest(GetCodeUrl, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<NBGHReturnType>(retuemsg);
        }


        /// <summary>   
        /// MD5加密   
        /// </summary>   
        /// <param name="str"></param>   
        /// <returns></returns>   
        public static string Md5Encrypt(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return Convert.ToBase64String(md5.ComputeHash(UTF8Encoding.Default.GetBytes(str)));
        }
        public static string ChMd5Encrypt(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return Convert.ToBase64String(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str)));
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
                webReq.ContentType = " application/x-www-form-urlencoded; charset=UTF-8";
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

        public static byte[] StreamToBytes(MemoryStream stream)
        {
            byte[] bytes = stream.ToArray();
            return bytes;
        }


    }
}