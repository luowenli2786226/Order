using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using DDX.OrderManagementSystem.Domain;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App.Common
{
    public class LazadaUtil
    {
        /// <summary>
        /// 获取订单跟踪码
        /// </summary>
        /// <param name="account"></param>
        /// <param name="Order"></param>
        /// <returns></returns>
        public static string GetTrackCode(AccountType account, OrderType Order, string OrderItemIds)
        {
            if (OrderItemIds.Length > 1)
            {
                OrderItemIds = OrderItemIds.Substring(1, OrderItemIds.Length - 1);

                // 订单的状态更新为准备出货
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Lazada_Request.Request.SetStatusToReadyToShip(account.UserName, account.ApiSecret, "[" + OrderItemIds + "]", account.Phone, account.Email));
                request.Method = "POST";
                request.ContentLength = 0;// 远程服务器返回错误: (411) 所需的长度 报错解决
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string ret = sr.ReadToEnd();
                sr.Close();
                response.Close();

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(ret);
                //XmlNodeList nodeList = doc.SelectSingleNode("SuccessResponse/Body/OrderItems").ChildNodes;
            }

            // 获取跟踪码
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(Lazada_Request.Request.GetMultipleOrderItems(account.UserName, account.ApiSecret, "[" + Order.TId + "]", account.Phone));
            //request1.Method = "POST";
            HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();

            System.IO.StreamReader sr1 = new System.IO.StreamReader(response1.GetResponseStream(), System.Text.Encoding.UTF8);
            string ret1 = sr1.ReadToEnd();
            sr1.Close();
            response1.Close();

            XmlDocument doc1 = new XmlDocument();
            doc1.LoadXml(ret1);

            XmlNodeList nodeList1 = doc1.SelectSingleNode("SuccessResponse/Body/Orders/Order/OrderItems").ChildNodes;

            string results = "";
            foreach (XmlNode obj in nodeList1)
            {
                results = obj["TrackingCode"].InnerText + " " + obj["PackageId"].InnerText;
            }
            return results;
        }

        /// <summary>
        /// 设置商品状态与跟踪码
        /// </summary>
        /// <param name="account"></param>
        /// <param name="Order"></param>
        /// <returns></returns>
        public static string SetTrackCode(AccountType account, OrderType Order, string OrderItemIds)
        {
            if (OrderItemIds.Length > 1)
            {
                // 订单的状态更新为准备出货
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Lazada_Request.Request.SetStatusToReadyToShipPlus(account.UserName, account.ApiSecret, "[" + OrderItemIds + "]", account.Phone, account.Email, Order.TrackCode));
                request.Method = "POST";
                request.ContentLength = 0;// 远程服务器返回错误: (411) 所需的长度 报错解决
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string ret = sr.ReadToEnd();
                sr.Close();
                response.Close();

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(ret);
                //XmlNodeList nodeList = doc.SelectSingleNode("SuccessResponse/Body/OrderItems").ChildNodes;
            }

            string results = "";
            //foreach (XmlNode obj in nodeList1)
            //{
            //    results = obj["TrackingCode"].InnerText + " " + obj["PackageId"].InnerText;
            //}
            return results;
        }
    }
}