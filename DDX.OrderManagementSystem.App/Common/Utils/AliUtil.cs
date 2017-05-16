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
using DDX.OrderManagementSystem.App.Common;
using NHibernate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App
{
    public class AliUtil
    {
        public static string RefreshToken(AccountType account)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("grant_type", "refresh_token");
            dic.Add("client_id", account.ApiKey);
            dic.Add("client_secret", account.ApiSecret);
            dic.Add("refresh_token", account.ApiToken);
            dic.Add("_aop_signature", SMTConfig.Sign(account.ApiKey, account.ApiSecret, SMTConfig.UrlRefreshToken, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.UrlRefreshToken + "/" + account.ApiKey, SMTConfig.GetParamUrl(dic));
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(c);
            if (token["access_token"] == null)
            {
                return "";
            }
            return token["access_token"].ToString().Replace("\"", "");
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

        public static string GetToken(string k, string s, string code)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("grant_type", "authorization_code");
            dic.Add("client_id", k);
            dic.Add("client_secret", s);
            dic.Add("redirect_uri", "http://127.0.0.1/");
            dic.Add("code", code);
            dic.Add("need_refresh_token", "true");
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.UrlGetToken + "/" + k, SMTConfig.GetParamUrl(dic));
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(c);
            return token["refresh_token"].ToString().Replace("\"", "");

        }

        public static AliOrderListType findOrderListQuery(string k, string s, string token, int pageIndex, string st, string et, string status = "WAIT_SELLER_SEND_GOODS")
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("orderStatus", status);
            dic.Add("pageSize", "50");
            if (st != null && et != null)
            {
                dic.Add("createDateStart", st);
                dic.Add("createDateEnd", et);
            }
            dic.Add("page", pageIndex.ToString());
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindOrderListQuery, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url + SMTConfig.ApifindOrderListQuery + "/" + k, SMTConfig.GetParamUrl(dic));
            return JsonConvert.DeserializeObject<AliOrderListType>(c);
        }

        public static EvaluationRoot findSellerEvaluationOrderList(string k, string s, string token)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiquerySellerEvaluationOrderList, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url + SMTConfig.ApiquerySellerEvaluationOrderList + "/" + k, SMTConfig.GetParamUrl(dic));
            return JsonConvert.DeserializeObject<EvaluationRoot>(c);
        }

        public static AliOrderType findOrderById(string k, string s, string token, string OId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("orderId", OId);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindOrderById, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url + SMTConfig.ApifindOrderById + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliOrderType>(c);
        }

        public static OrderMsgTypeList findOrderMsgByOrderId(string k, string s, string token, string OId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("currentPage", "1");
            dic.Add("pageSize", "200");
            dic.Add("channelId", OId);
            dic.Add("msgSources", "order_msg");
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiqueryOrderMsgListByOrderId, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url + SMTConfig.ApiqueryOrderMsgListByOrderId + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OrderMsgTypeList>(c);
        }

        public static string sellerShipment(string k, string s, string token, string orderExNo, string trackCode, string serviceName, bool isALL = false)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("serviceName", serviceName);
            dic.Add("logisticsNo", trackCode);
            if (isALL)
                dic.Add("sendType", "all");
            else
                dic.Add("sendType", "part");
            dic.Add("outRef", orderExNo);
            if (serviceName.ToLower() == "other")
            {
               // dic.Add("trackingWebsite", "http://www.852ex.com/");
                dic.Add("trackingWebsite", "http://www.edostavka.ru/track.html");
            }
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApisellerShipment, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url + SMTConfig.ApisellerShipment + "/" + k, SMTConfig.GetParamUrl(dic));
            return c;
        }

        public static AliMessageList QueryMessageList(string k, string s, string token, int pageIndex)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("startTime", DateTime.Now.AddDays(-11).ToString("MM/dd/yyyy HH:mm:ss"));
            dic.Add("endTime", DateTime.Now.AddDays(1).ToString("MM/dd/yyyy HH:mm:ss"));
            //dic.Add("startTime", "11/01/2013 00:00:00");
            //dic.Add("endTime", "11/14/2013 00:00:00");
            dic.Add("pageSize", "50");
            dic.Add("currentPage", pageIndex.ToString());
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiqueryMessageList, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApiqueryMessageList + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliMessageList>(c);
        }

        public static AliProductListRoot QueryProductList(string k, string s, string token, int pageIndex)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("productStatusType", "onSelling");
            dic.Add("pageSize", "50");
            dic.Add("currentPage", pageIndex.ToString());
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindProductInfoListQuery, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApifindProductInfoListQuery + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliProductListRoot>(c);
        }
        public static ALiProductRootObject FindAeProductById(string k, string s, string token, string pid)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("productId", pid);
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindAeProductById, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApifindAeProductById + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ALiProductRootObject>(c);
        }

        //public static AliMessageList queryOrderMsgList(string k, string s, string token, int pageIndex)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    dic.Add("startTime", DateTime.Now.AddDays(-11).ToString("MM/dd/yyyy HH:mm:ss"));
        //    dic.Add("endTime", DateTime.Now.AddDays(1).ToString("MM/dd/yyyy HH:mm:ss"));
        //    //dic.Add("startTime", "11/01/2013 00:00:00");
        //    //dic.Add("endTime", "11/14/2013 00:00:00");
        //    dic.Add("pageSize", "50");
        //    dic.Add("currentPage", pageIndex.ToString());

        //    dic.Add(SMTConfig.fieldAccessToken, token);
        //    dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiqueryOrderMsgList, dic));
        //    string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApiqueryOrderMsgList + "/" + k, SMTConfig.GetParamUrl(dic));
        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<AliMessageList>(c);
        //}

        public static string AddMessage(string k, string s, string token, string buyerId, string content)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("buyerId", buyerId);
            dic.Add("content", content);
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiaddMessage, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApiaddMessage + "/" + k, SMTConfig.GetParamUrl(dic));
            return c;
        }

        public static string AddOrderMessage(string k, string s, string token, string orderId, string content, string buyerId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("channelId", orderId);
            dic.Add("buyerId", buyerId);
            dic.Add("msgSources", "order_msg");
            
            dic.Add("content", HttpUtility.UrlEncodeUnicode(content));
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApiaddOrderMessage, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApiaddOrderMessage + "/" + k, SMTConfig.GetParamUrl(dic));
            return c;
        }

        /// <summary>
        /// 卖家对未评价的订单进行评价
        /// </summary>
        public static string SaveSellerFeedback(string k, string s, string token, string orderId, string content)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("orderId", orderId);
            dic.Add("feedbackContent", content);
            dic.Add("score", "5");

            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApisaveSellerFeedback, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApisaveSellerFeedback + "/" + k, SMTConfig.GetParamUrl(dic));
            return c;
        }

        public static LogisticsRootObject GetOnlineLogisticsServiceListByOrderId(string k, string s, string token, string orderId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("orderId", orderId);
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApigetOnlineLogisticsServiceListByOrderId, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApigetOnlineLogisticsServiceListByOrderId + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LogisticsRootObject>(c);
        }

        public static string GetAliPDF(string k, string s, string token, string t)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("warehouseOrderQueryDTOs", t);
            dic.Add("printDetail", "true");
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApigetPrintInfo, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApigetPrintInfo + "/" + k, SMTConfig.GetParamUrl(dic));
            return c;
        }

        public static AliTrackCodeRootObject GetOnlineLogisticsInfo(string k, string s, string token, string orderId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("orderId", orderId);
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApigetOnlineLogisticsInfo, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApigetOnlineLogisticsInfo + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliTrackCodeRootObject>(c);
        }
        public static AlicreateWarehouseOrderReturn GetWuyouCode(string k, string s, string token, string orderId, AddressOrderDTOs address, string warehouseCarrierService, List<AeopWlDeclareProductDTO> pList)
        {
            //AlicreateWarehouseOrderRequest r = new AlicreateWarehouseOrderRequest();
            //r.tradeOrderId = orderId;
            //r.tradeOrderFrom = "ESCROW";
           
            //r.warehouseCarrierService = warehouseCarrierService;
            //r.domesticLogisticsCompanyId = "-1";
            //r.domesticLogisticsCompany = "圆通速递";
            //r.domesticTrackingNo = orderId;
            //r.declareProductDTOs = pList;
            //r.addressDTOs = addresses;
            //r.undeliverableDecision = 0;
            //string a = JsonConvert.SerializeObject(r);
            //Dictionary<string,string> dic=JsonConvert.DeserializeObject<Dictionary<string, string>>(a);
            string a = "CPAM_WLB_CPAMHZ,YANWENJYT_WLB_CPAMHZ,CPAM_TZ,CPOSPP_TZ,CPAM_JY,CPOSPP_JY,CPOSPP_HF,CPAM_HF";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tradeOrderId", orderId);
            dic.Add("tradeOrderFrom", "ESCROW");
            dic.Add("domesticLogisticsCompanyId", "-1");
            if (a.Contains(warehouseCarrierService))
            {
                dic.Add("domesticLogisticsCompany", "圆通速递");
            }
            else
            {
                dic.Add("domesticLogisticsCompany", "上门揽收");
            }
            dic.Add("domesticTrackingNo", orderId);
            dic.Add("addressDTOs", JsonConvert.SerializeObject(address));
            dic.Add("warehouseCarrierService", warehouseCarrierService);
            dic.Add("declareProductDTOs", JsonConvert.SerializeObject(pList));
            dic.Add("undeliverableDecision", "0");

            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApicreateWarehouseOrder, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApicreateWarehouseOrder + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AlicreateWarehouseOrderReturn>(c);
        }
        public static AliCreateWarehouseOrderResultDTO GetWuyouCode_E(string k, string s, string token, string orderId, AddressDTOs addresses, string warehouseCarrierService, List<AeopWlDeclareProductDTO> pList)
        {
            AlicreateWarehouseOrderRequest r = new AlicreateWarehouseOrderRequest();
            r.tradeOrderId = orderId;
            r.tradeOrderFrom = "ESCROW";
            LogisticsRootObject OnlineLogisticsServiceList = GetOnlineLogisticsServiceListByOrderId(k, s, token, orderId);
            r.warehouseCarrierService = OnlineLogisticsServiceList.result[0].logisticsServiceId;
            r.domesticLogisticsCompanyId = "-1";
            r.domesticLogisticsCompany = "圆通速递";
            r.domesticTrackingNo = orderId;
            r.declareProductDTOs = pList;
            r.addressDTOs = addresses;
            r.undeliverableDecision = 0;
            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(r));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApicreateWarehouseOrder + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliCreateWarehouseOrderResultDTO>(c);
        }
        public static SellerAddressesList GetLogisticsSellerAddresses(string k, string s, string token)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("request", "[\"sender\",\"pickup\",\"refund\"]");
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url2 + SMTConfig.ApigetLogisticsSellerAddresses, dic));
            string c = PostWebRequest(SMTConfig.IP + SMTConfig.Url2 + SMTConfig.ApigetLogisticsSellerAddresses + "/" + k, SMTConfig.GetParamUrl(dic));
            SellerAddressesList list = Newtonsoft.Json.JsonConvert.DeserializeObject<SellerAddressesList>(c);
            //int result = list.senderSellerAddressesList[0].addressId;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<AliOrderAddressRootObject>(c);
            return list;
        }

        public static AliOrderSendRootObject CreateWarehouseOrder(string k, string s, string token, string orderId, AliOrderAddressRootObject addresses, string warehouseCarrierService, List<LogisticsGoods> pList)
        {
            //中国邮政挂号小包(杭州市下城区长城街22号中国跨境贸易电子商务产业园) ---CPAM_WLB_CPAMHZ
            //中国邮政平常小包+(杭州市下城区长城街22号中国跨境贸易电子商务产业园)--YANWENJYT_WLB_CPAMHZ
            string a = "CPAM_WLB_CPAMHZ,YANWENJYT_WLB_CPAMHZ,CPAM_TZ,CPOSPP_TZ,CPAM_JY,CPOSPP_JY,CPOSPP_HF,CPAM_HF";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tradeOrderId", orderId);
            dic.Add("tradeOrderFrom", "ESCROW");
            dic.Add("domesticLogisticsCompanyId", "-1");
            if (a.Contains(warehouseCarrierService))
            {
                dic.Add("domesticLogisticsCompany", "圆通速递");
            }
            else
            {
                dic.Add("domesticLogisticsCompany", "上门揽收");
            }
            dic.Add("domesticTrackingNo", orderId);
            dic.Add("addressDTOs", JsonConvert.SerializeObject(addresses));
            dic.Add("warehouseCarrierService", warehouseCarrierService);
            dic.Add("declareProductDTOs", JsonConvert.SerializeObject(pList));

            // 不可达处理(退回:0/销毁:1) 目前因部分ISV还未升级，系统文档中该参数当前设置为可选，默认值为-1。请ISV升级时，将参数设置为必选，默认值为1，否则将影响9月28日之后的发货功能
            dic.Add("undeliverableDecision", "0");

            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApicreateWarehouseOrder, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApicreateWarehouseOrder + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AliOrderSendRootObject>(c);
        }

        public static LoanRootObject FindLoanListQuery(string k, string s, string token, int page)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("page", page.ToString());
            dic.Add("pageSize", "100");
            dic.Add("loanStatus", "loan_ok");
            dic.Add("createDateStart", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"));

            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindLoanListQuery, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApifindLoanListQuery + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoanRootObject>(c);
        }
        public static LoanRootObject FindLoanListQuery(string k, string s, string token, DateTime st, DateTime et, int page)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("page", page.ToString());
            dic.Add("pageSize", "100");
            dic.Add("loanStatus", "loan_ok");
            dic.Add("createDateStart", st.ToString("yyyy-MM-dd"));
            dic.Add("createDateEnd", et.AddDays(1).ToString("yyyy-MM-dd"));
            dic.Add(SMTConfig.fieldAccessToken, token);
            dic.Add("_aop_signature", SMTConfig.Sign(k, s, SMTConfig.Url + SMTConfig.ApifindLoanListQuery, dic));
            string c = PostWebRequest(SMTConfig.IP2 + SMTConfig.Url + SMTConfig.ApifindLoanListQuery + "/" + k, SMTConfig.GetParamUrl(dic));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoanRootObject>(c);
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