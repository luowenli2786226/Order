using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using DDX.OrderManagementSystem.Domain;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App.Common
{
    public class WishUtil
    {
        public const string getOrderUrl = "https://merchant.wish.com/api/v2/order/multi-get?";

        public const string getUploadTrackNoUrl = "https://merchant.wish.com/api/v2/order/fulfill-one";

        public const string getAccessToken = "https://merchant.wish.com/api/v2/oauth/access_token";

        public const string getRefreshtoken = "https://merchant.wish.com/api/v2/oauth/refresh_token";

        public static string RefreshToken(AccountType account)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("grant_type", "refresh_token");
            dic.Add("client_id", account.ApiKey);
            dic.Add("client_secret", account.ApiSecret);
            dic.Add("refresh_token", account.ApiToken);
            // dic.Add("_aop_signature", SMTConfig.Sign(account.ApiKey, account.ApiSecret, SMTConfig.UrlRefreshToken, dic));
            string c = PostWebRequest(getRefreshtoken, SMTConfig.GetParamUrl(dic));
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(c);
            return token["data"]["access_token"].ToString();
        }

        public static string GetToken(string k, string s, string code)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("client_id", k);
            dic.Add("client_secret", s);

            dic.Add("code", code);
            dic.Add("grant_type", "authorization_code");
            dic.Add("redirect_uri", "https://127.0.0.1");
            string c = PostWebRequest(getAccessToken, SMTConfig.GetParamUrl(dic));
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(c);
            return token["data"]["refresh_token"].ToString().Replace("\"", "");

        }

        public static string GetAuthUrl(string k, string s)
        {
            string url =
              "http://gw.api.alibaba.com/auth/authorize.htm?";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("client_id", k);
            dic.Add("site", "aliexpress");
            dic.Add("redirect_uri", HttpContext.Current.Request.Url.Authority + SMTConfig.SystemReturnUrl);
            dic.Add("state", "sss");
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, "", dic, false));
            return url + SMTConfig.GetParamUrl(dic);
        }

        public static string UploadTrackNo(string token, string id, string tracking_provider, string tracking_number )
        {
            //string param = string.Format("tracking_provider={1}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
            string param = string.Format("tracking_provider={1}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
            string str = PostWebRequest(getUploadTrackNoUrl, param);
            return str;

        }

        //public static string UploadTrackNo(string token, string id, string tracking_provider, string tracking_number)
        //{
        //    string param = string.Format("tracking_provider={1}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
        //    //string param = string.Format("tracking_provider={0}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
        //    string str = PostWebRequest(getUploadTrackNoUrl, param);
        //    return str;

        //}


        public static WishOrderList GetOrderList(string token, string since)
        {
            string url = getOrderUrl + "limit=100&since=" + since + "&access_token=" + token;
            string str = GetWebRequest(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<WishOrderList>(str);

        }
        public static WishOrderList GetOrderListByUrl(string url)
        {

            string str = GetWebRequest(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<WishOrderList>(str);

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
        public static bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public static string GetWebRequest(string postUrl)
        {
            string ret = string.Empty;
            try
            {

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                // 这个可以是改变的，也可以是下面这个固定的字符串
                // 创建request对象
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(postUrl);

                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();

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