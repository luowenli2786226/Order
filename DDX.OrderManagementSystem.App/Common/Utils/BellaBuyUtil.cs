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

namespace DDX.OrderManagementSystem.App
{
    public class BellaBuyUtil
    {

        public const string getOrderUrl = "http://vendor.9-9buy.com/devorder/multiOrder?";

        public const string getUploadTrackNoUrl = "http://vendor.9-9buy.com/devorder/modifyTracking";



        public static string UploadTrackNo(string token, string id, string tracking_provider, string tracking_number)
        {
            //string param = string.Format("tracking_provider={1}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
            string param = string.Format("tracking_provider={1}&tracking_number={2}&id={3}&access_token={0}", token, tracking_provider, tracking_number, id);
            string str = PostWebRequest(getUploadTrackNoUrl, param);
            return str;

        }

        public static BellaBuyRootObject GetOrderList(string token, int page, DateTime st, DateTime et)
        {
            string url = getOrderUrl + string.Format("access_token={0}&page={3}&start_time={1}&end_time={2}", token, st.ToString("yyyy-MM-dd"), et.ToString("yyyy-MM-dd"), page);
            string str = GetWebRequest(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<BellaBuyRootObject>(str);

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