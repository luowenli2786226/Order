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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace DDX.OrderManagementSystem.App.Common.Utils
{
    public class SHMXUtil
    {
        const string IdentifyUrl = "http://222.73.27.186:8082/selectAuth.htm?";
        const string CreateOrderUrl = "http://222.73.27.186:8082/createOrderApi.htm?";
        public static IdentifyReturnType Identify()
        {
            string username = "YSMY";
            string password = "123456";
            string request = "username=" + username + "&password=" + password;
            string str = WishUtil.GetWebRequest(IdentifyUrl + request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IdentifyReturnType>(str);

        }
        public static OrderReturnType GetOrderReturn(IdentifyReturnType i, OrderType ordertype)
        {

            OrderRquestParam orderparam = new OrderRquestParam();
            orderparam.buyerid = Convert.ToString(ordertype.BuyerId);
            orderparam.consignee_address = ordertype.AddressInfo.Street;
            orderparam.consignee_city = ordertype.AddressInfo.City;
            orderparam.consignee_mobile = ordertype.AddressInfo.Phone;
            orderparam.consignee_name = ordertype.BuyerName;
            orderparam.trade_type = "ZYXT";
            orderparam.consignee_postcode = ordertype.AddressInfo.PostCode;
            orderparam.consignee_state = ordertype.AddressInfo.Province;
            orderparam.consignee_telephone = ordertype.AddressInfo.Tel;
            orderparam.country = ordertype.AddressInfo.CountryCode;
            orderparam.customer_id = i.customer_id;
            orderparam.customer_userid = i.customer_userid;
            List<orderInvoiceParam> list = new List<orderInvoiceParam>();
            foreach (OrderProductType product in ordertype.Products)
            {

                orderInvoiceParam invoiceparam = new orderInvoiceParam();
                invoiceparam.invoice_amount = Convert.ToString(product.Price);
                invoiceparam.invoice_pcs = Convert.ToString(product.Qty);
                invoiceparam.invoice_title = product.Title.Substring(0,40);
                invoiceparam.invoice_weight = "";
                invoiceparam.item_id = Convert.ToString(product.OId);
                invoiceparam.item_transactionid = Convert.ToString(product.Id);
                invoiceparam.sku = product.SKU;
                list.Add(invoiceparam);
            }


            orderparam.orderInvoiceParam = list;
            orderparam.order_customerinvoicecode ="YSMY"+ ordertype.OrderExNo;
            orderparam.product_id = "3921";
            orderparam.weight = Convert.ToString(ordertype.Weight*0.001);
            string request = Newtonsoft.Json.JsonConvert.SerializeObject(orderparam);
            string c = PostWebRequest(CreateOrderUrl, request);


            return Newtonsoft.Json.JsonConvert.DeserializeObject<OrderReturnType>(c);
        }



        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受    
            return true;
        }
        public static string PostWebRequest(string strUrl, string strParam)
        {
            try
            {
                strParam = "param=" + strParam.Replace('&', ',');
                System.Net.HttpWebRequest request;
                request = (System.Net.HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";
                string paraUrlCoded = strParam;
                byte[] payload;
                payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                request.ContentLength = payload.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                System.Net.HttpWebResponse response;

                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s;
                s = response.GetResponseStream();
                string StrDate = "";
                string strValue = "";
                StreamReader Reader = new StreamReader(s, Encoding.GetEncoding("UTF-8"));
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate + "\r\n";
                }
                if (strValue.Substring(8, 1) == "\"")
                {
                    Reader.Close();
                    strValue = "";
                }
                else
                {
                    Reader.Close();
                }
                return strValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static byte[] GetSHMXpdfUrl(string orderInfo, string printcode)
        {

            string strUrl = "http://222.73.27.186/order/FastRpt/PDF_NEW.aspx?Format=lbl_10.frx&PrintType=1&order_id=" + orderInfo;
              int m_Timeout = 99999;
        CookieContainer cookiecontaniner = new CookieContainer();

            
        //    try
        //    {
        //        HttpData hd = HttpData.Create(new Uri(strUrl), cookiecontaniner);
        //        hd.Timeout = m_Timeout;
        //        hd.Response();
        //        System.IO.MemoryStream msrm = hd.GetResponseStream();
        //        StreamReader reader = new StreamReader( msrm );

        //        string srcString = reader.ReadToEnd();//解码 
        //        XElement root = XElement.Parse(srcString);
        //        return root.Element("url").Value;
        //    }
        //    catch (Exception ex)
        //    {

        //        return ex.Message;
        //    }
        var url = "http://222.73.27.186/order/FastRpt/PDF_NEW.aspx?Format=lbl_10.frx&PrintType=1&order_id=" + orderInfo;
            HttpData hd = HttpData.Create(new Uri(strUrl), cookiecontaniner);
            hd.Timeout = m_Timeout;
            hd.Response();
            System.IO.MemoryStream msrm = hd.GetResponseStream();
            byte[] result = StreamToBytes(msrm);
            return result;
        }
        public string MD5Encrypt(string strPwd)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strPwd, "MD5").ToLower();
        }
        public static byte[] StreamToBytes(MemoryStream stream)
        {
            byte[] bytes = stream.ToArray();
            return bytes;
        }
        

    }
    public class HttpData
    {
        public HttpWebRequest request;
        private HttpWebResponse response;
        private MemoryStream responseStream;
        private bool isDisposed;

        public HttpData()
        {
            this.isDisposed = false;
        }

        internal HttpData(HttpWebRequest request)
            : this()
        {
            this.request = request;
            if (string.Compare(this.request.RequestUri.Scheme, "https", true) == 0)
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CertificateValidation);
        }

        protected virtual bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
            if (this.response != null)
            {
                this.response.Close();
                this.response = null;
            }
            if (this.responseStream != null)
            {
                this.responseStream.Dispose();
                this.responseStream = null;
            }
            this.request = null;
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return "HttpData";
        }

        public static HttpData Create(Uri address)
        {
            return Create(address, null, null);
        }

        public static HttpData Create(Uri address, string referer)
        {
            return Create(address, referer, null);
        }

        public static HttpData Create(Uri address, CookieContainer cookies)
        {
            return Create(address, null, cookies);
        }

        public static HttpData Create(Uri address, string referer, CookieContainer cookies)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = 20000;
            request.Referer = referer;
            request.CookieContainer = cookies;

            request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-shockwave-flash, */*";
            request.Headers.Add("Accept-Language: en-AU,zh-CN;en-CA;q=0.5");
            request.Headers.Add("Cache-Control:no-cache");
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322)";

            return new HttpData(request);
        }

        public string Accept
        {
            get
            {
                if (this.request != null)
                    return this.request.Accept;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.Accept = value;
            }
        }

        public Uri Address
        {
            get
            {
                if (this.request != null)
                    return this.request.Address;
                return null;
            }
        }

        public bool AllowAutoRedirect
        {
            get
            {
                if (this.request != null)
                    return this.request.AllowAutoRedirect;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.AllowAutoRedirect = value;
            }
        }

        public bool AllowWriteStreamBuffering
        {
            get
            {
                if (this.request != null)
                    return this.request.AllowWriteStreamBuffering;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.AllowWriteStreamBuffering = value;
            }
        }

        public string Connection
        {
            get
            {
                if (this.request != null)
                    return this.request.Connection;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.Connection = value;
            }
        }

        public string ConnectionGroupName
        {
            get
            {
                if (this.request != null)
                    return this.request.ConnectionGroupName;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.ConnectionGroupName = value;
            }
        }

        public long ContentLength
        {
            get
            {
                if (this.request != null)
                    return this.request.ContentLength;
                return -1;
            }
            set
            {
                if (this.request != null)
                    this.request.ContentLength = value;
            }
        }

        public string ContentType
        {
            get
            {
                if (this.request != null)
                    return this.request.ContentType;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.ContentType = value;
            }
        }

        public object CookieContainer
        {
            get
            {
                if (this.request != null)
                    return this.request.CookieContainer;
                return null;
            }
            set
            {
                if (this.request != null && (value == null || value as CookieContainer != null))
                    this.request.CookieContainer = (CookieContainer)value;
            }
        }

        public string Expect
        {
            get
            {
                if (this.request != null)
                    return this.request.Expect;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.Expect = value;
            }
        }

        public bool HaveResponse
        {
            get
            {
                if (this.request != null)
                    return this.request.HaveResponse;
                return false;
            }
        }

        public DateTime IfModifiedSince
        {
            get
            {
                if (this.request != null)
                    return this.request.IfModifiedSince;
                return DateTime.MinValue;
            }
            set
            {
                if (this.request != null)
                    this.request.IfModifiedSince = value;
            }
        }

        public bool KeepAlive
        {
            get
            {
                if (this.request != null)
                    return this.request.KeepAlive;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.KeepAlive = value;
            }
        }

        public int MaximumAutomaticRedirections
        {
            get
            {
                if (this.request != null)
                    return this.request.MaximumAutomaticRedirections;
                return -1;
            }
            set
            {
                if (this.request != null)
                    this.request.MaximumAutomaticRedirections = value;
            }
        }

        public int MaximumResponseHeadersLength
        {
            get
            {
                if (this.request != null)
                    return this.request.MaximumResponseHeadersLength;
                return -1;
            }
            set
            {
                if (this.request != null)
                    this.request.MaximumResponseHeadersLength = value;
            }
        }

        public string MediaType
        {
            get
            {
                if (this.request != null)
                    return this.request.MediaType;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.MediaType = value;
            }
        }

        public string Method
        {
            get
            {
                if (this.request != null)
                    return this.request.Method;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.Method = value;
            }
        }

        public bool Pipelined
        {
            get
            {
                if (this.request != null)
                    return this.request.Pipelined;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.Pipelined = value;
            }
        }

        public bool PreAuthenticate
        {
            get
            {
                if (this.request != null)
                    return this.request.PreAuthenticate;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.PreAuthenticate = value;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                if (this.request != null)
                    return this.request.ProtocolVersion;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.ProtocolVersion = value;
            }
        }

        public int ReadWriteTimeout
        {
            get
            {
                if (this.request != null)
                    return this.request.ReadWriteTimeout;
                return -1;
            }
            set
            {
                if (this.request != null)
                    this.request.ReadWriteTimeout = value;
            }
        }

        public string Referer
        {
            get
            {
                if (this.request != null)
                    return this.request.Referer;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.Referer = value;
            }
        }

        public Uri RequestUri
        {
            get
            {
                if (this.request != null)
                    return this.request.RequestUri;
                return null;
            }
        }

        public bool SendChunked
        {
            get
            {
                if (this.request != null)
                    return this.request.SendChunked;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.SendChunked = value;
            }
        }

        public int Timeout
        {
            get
            {
                if (this.request != null)
                    return this.request.Timeout;
                return -1;
            }
            set
            {
                if (this.request != null)
                    this.request.Timeout = value;
            }
        }

        public string TransferEncoding
        {
            get
            {
                if (this.request != null)
                    return this.request.TransferEncoding;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.TransferEncoding = value;
            }
        }

        public bool UnsafeAuthenticatedConnectionSharing
        {
            get
            {
                if (this.request != null)
                    return this.request.UnsafeAuthenticatedConnectionSharing;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.UnsafeAuthenticatedConnectionSharing = value;
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                if (this.request != null)
                    return this.request.UseDefaultCredentials;
                return false;
            }
            set
            {
                if (this.request != null)
                    this.request.UseDefaultCredentials = value;
            }
        }

        public string UserAgent
        {
            get
            {
                if (this.request != null)
                    return this.request.UserAgent;
                return null;
            }
            set
            {
                if (this.request != null)
                    this.request.UserAgent = value;
            }
        }


        public string ResponseCharacterSet
        {
            get
            {
                if (this.response != null)
                    return this.response.CharacterSet;
                return null;
            }
        }

        public string ResponseContentEncoding
        {
            get
            {
                if (this.response != null)
                    return this.response.ContentEncoding;
                return null;
            }
        }

        public long ResponseContentLength
        {
            get
            {
                if (this.response != null)
                    return this.response.ContentLength;
                return -1;
            }
        }

        public string ResponseContentType
        {
            get
            {
                if (this.response != null)
                    return this.response.ContentType;
                return null;
            }
        }

        public bool ResponseIsFromCache
        {
            get
            {
                if (this.response != null)
                    return this.response.IsFromCache;
                return false;
            }
        }

        public bool ResponseIsMutuallyAuthenticated
        {
            get
            {
                if (this.response != null)
                    return this.response.IsMutuallyAuthenticated;
                return false;
            }
        }

        public string ResponseMethod
        {
            get
            {
                if (this.response != null)
                    return this.response.Method;
                return null;
            }
        }

        public Version ResponseProtocolVersion
        {
            get
            {
                if (this.response != null)
                    return this.response.ProtocolVersion;
                return null;
            }
        }

        public DateTime ResponseLastModified
        {
            get
            {
                if (this.response != null)
                    return this.response.LastModified;
                return DateTime.MinValue;
            }
        }

        public Uri ResponseUri
        {
            get
            {
                if (this.response != null)
                    return this.response.ResponseUri;
                return null;
            }
        }

        public string ResponseServer
        {
            get
            {
                if (this.response != null)
                    return this.response.Server;
                return null;
            }
        }

        public string ResponseStatusDescription
        {
            get
            {
                if (this.response != null)
                    return this.response.StatusDescription;
                return null;
            }
        }

        public void SetHeader(string name, string value)
        {
            if (this.request != null)
            {
                this.request.Headers[name] = value;
            }
        }

        public void SetPost(string data)
        {
            this.SetPost(data, null);
        }

        public void SetPost(string data, string characterSet)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString());
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            if (!string.IsNullOrEmpty(data))
            {
                byte[] datas = null;
                if (string.IsNullOrEmpty(characterSet))
                {
                    datas = Encoding.Default.GetBytes(data);
                }
                else
                {
                    datas = Encoding.GetEncoding(characterSet).GetBytes(data);
                }

                request.ContentLength = datas.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(datas, 0, datas.Length);
                    stream.Close();
                }
            }
        }

        public void SetPost(Hashtable items)
        {
            this.SetPost(items, null);
        }

        public void SetPost(Hashtable items, string characterSet)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString());
            StringBuilder post = new StringBuilder();
            foreach (DictionaryEntry dict in items)
            {
                if (post.Length == 0)
                    post.Append(string.Format("{0}={1}", dict.Key, dict.Value));
                else
                    post.Append(string.Format("&{0}={1}", dict.Key, dict.Value));
            }
            this.SetPost(post.ToString(), characterSet);
        }

        public int Response()
        {
            return this.Response(true);
        }

        public int Response(bool autoRedirect)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            this.request.AllowAutoRedirect = autoRedirect;
            this.response = (HttpWebResponse)this.request.GetResponse();

            //this.response.Cookies.Add(context.Cookies);

            this.ReaderResponseStream();
            return (int)(this.response.StatusCode);
        }

        private void ReaderResponseStream()
        {
            if (this.response != null)
            {
                this.responseStream = new MemoryStream();
                Stream stream = this.response.GetResponseStream();
                int count;
                byte[] buffer = new byte[0x100];
                try
                {
                    do
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        responseStream.Write(buffer, 0, count);
                    }
                    while (count > 0);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        public MemoryStream GetResponseStream()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString());
            return this.responseStream;
        }

        public string GetResponseText()
        {
            return this.GetResponseText(this.response.CharacterSet);
        }

        public string GetResponseText(string encode)
        {
            if (string.IsNullOrEmpty(encode))
                throw new ArgumentNullException();
            return this.GetResponseText(Encoding.GetEncoding(encode));
        }

        public string GetResponseText(Encoding encode)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString());
            if (this.responseStream != null)
                return encode.GetString(this.responseStream.GetBuffer());
            return null;
        }


        public string GetResponseHeader(string name)
        {
            if (this.response != null)
                return this.response.Headers[name];
            return null;
        }
    }
}
