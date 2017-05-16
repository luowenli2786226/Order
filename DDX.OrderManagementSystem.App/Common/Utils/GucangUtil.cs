using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DDX.OrderManagementSystem.App.com.gucang.wsvc;
using DDX.OrderManagementSystem.Domain;

namespace DDX.OrderManagementSystem.App.Common.Utils
{
    /// <summary>
    /// 谷仓OMS发货系统对接
    /// </summary>
    public class GucangUtil
    {
        public const string appToken = "2828f57e6440942a72126397aca492ef";
        public const string appKey = "b00be471aae9492365935343ba2d13d0";
        //宁波谷仓
        public const string nbappToken = "4a695d9f33f0d4220aad0b83de3298ea";
        public const string nbappKey = "9ec4774707d873ba70b34c109faedc20";
        public static string CreateOrder(OrderType OrderType,string Area)
        {
            if (OrderType.AddressInfo.Province.ToUpper() == "AK" || OrderType.AddressInfo.Province.ToUpper() == "HI" || OrderType.AddressInfo.Province.ToUpper() == "PO" || OrderType.AddressInfo.Province.ToUpper() == "BOX" || OrderType.AddressInfo.Province.ToUpper() == "PR")
            {
                return "{\"ask\":\"False\",\"message\":\"该州被屏蔽上传\",\"order_code\":\"\"}";
            }
            com.gucang.wsvc.Ec gc = new Ec();
            string service = "createOrder";
            ParamsJsonType paramsjson = new ParamsJsonType();
            paramsjson.platform =OrderType.Platform.Trim().ToUpper();
            if (OrderType.FBABy == "YWGCUS-West")
            {
                paramsjson.warehouse_code = "USWE";
            }
            if (OrderType.FBABy == "YWGCUS-East")
            {
                paramsjson.warehouse_code = "USEA";
            }           
            paramsjson.reference_no = OrderType.OrderExNo;
            //    paramsjson.order_desc = "\u8ba2\u5355\u63cf\u8ff0";
            paramsjson.country_code = OrderType.AddressInfo.CountryCode;
            paramsjson.province = OrderType.AddressInfo.Province;
            paramsjson.city = OrderType.AddressInfo.City;
            string dd = "";
            string dd2 = "";
            string dd3 = "";
            if (OrderType.AddressInfo.Street.Length >= 30)
            {
                String[] strs = OrderType.AddressInfo.Street.Split(' ');
                for (int i = 0; i < strs.Length; i++)
                {
                    if ((dd.Length < 20) && ((dd + strs[i]).Length<26))
                    {
                        dd = dd + strs[i] + " ";
                    }
                    else if ((dd2.Length < 20 )&& ((dd2 + strs[i]).Length <26))
                    {
                        dd2 = dd2 + strs[i] + " ";
                    }
                    else
                    {
                        dd3 = dd3 + strs[i] + " ";
                    }

                }
                paramsjson.address1 = dd.Trim();
                paramsjson.address2 = dd2.Trim();
                paramsjson.address3 = dd3.Trim();
            }
            else
            {
                paramsjson.address1 = OrderType.AddressInfo.Street;
                paramsjson.address2 ="";
                paramsjson.address3 = "";
            }
            paramsjson.zipcode = OrderType.AddressInfo.PostCode;
            paramsjson.doorplate = "";
            paramsjson.name = OrderType.AddressInfo.Addressee;
            paramsjson.phone = ((OrderType.AddressInfo.Tel != null || OrderType.AddressInfo.Tel != "") ? OrderType.AddressInfo.Tel : OrderType.AddressInfo.Phone);
            paramsjson.email = "";
            paramsjson.is_shipping_method_not_allow_update = "0";
            paramsjson.items = new List<ItemsList>();

            foreach (var orderProductType in OrderType.Products)
            {
                ItemsList itemlist = new ItemsList();
                if (orderProductType.SKU.Contains("SO76700CS"))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("SO76700CS", "SO76700CS-");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("SO76500CS"))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("SO76500CS", "SO76500CS-");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("SO76600CS"))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("SO76600CS", "SO76600CS-");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("SO76300CS "))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("SO76300CS ", "SO76300CS");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("SO76400CS "))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("SO76400CS ", "SO76400CS");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("HP904"))
                {
                    itemlist.product_sku = orderProductType.SKU;
                    paramsjson.shipping_method = "FEDEX-PACKAGE";
                }
                else if ((orderProductType.SKU.Contains("HP905")) || (orderProductType.SKU.Contains("HP906")))
                {
                    itemlist.product_sku = orderProductType.SKU;
                    paramsjson.shipping_method = "FEDEX-PACKAGE-B";
                }
                else if ((orderProductType.SKU.Contains("HP907")) || (orderProductType.SKU.Contains("HP908")))
                {
                    itemlist.product_sku = orderProductType.SKU;
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else if (orderProductType.SKU.Contains("HBS14900") || orderProductType.SKU.Contains("HBS14700") || orderProductType.SKU.Contains("HBS14800"))
                {
                    itemlist.product_sku = orderProductType.SKU.Replace("/", "-");
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                else
                {
                    itemlist.product_sku = orderProductType.SKU;
                    paramsjson.shipping_method = "FEDEX-LARGEPARCEL";
                }
                itemlist.quantity = orderProductType.Qty;
                paramsjson.items.Add(itemlist);
            }
            string paramsJson = Newtonsoft.Json.JsonConvert.SerializeObject(paramsjson);
            string c = gc.callService(paramsJson, appToken, appKey, service);
            if (Area == "nb")
            {
                c = gc.callService(paramsJson, nbappToken, nbappKey, service);
            }
            return c;
        }

        public static string GetOrderByRefCode(OrderType OrderType,string Area)
        {
            com.gucang.wsvc.Ec gc = new Ec();
            string service = "getOrderByRefCode";
            ParamsJsonType paramsjson = new ParamsJsonType();
            paramsjson.reference_no = OrderType.OrderExNo;
            string paramsJson = Newtonsoft.Json.JsonConvert.SerializeObject(paramsjson);
            string c = gc.callService(paramsJson, appToken, appKey, service);
            if (Area == "nb")
            {
                c = gc.callService(paramsJson, nbappToken, nbappKey, service);
            }
            return c;
        }
    }
    public class GCReturnType
    {
        public string ask { get; set; }
        public string message { get; set; }
        public string order_code { get; set; }
        public List<DataList> data { get; set; }
    }
    public class ParamsJsonType
    {
        public string platform { get; set; }
        public string warehouse_code { get; set; }
        public string shipping_method { get; set; }
        public string reference_no { get; set; }
        public string order_desc { get; set; }
        public string country_code { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string zipcode { get; set; }
        public string doorplate { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string verify { get; set; }
        public string forceVerify { get; set; }
        public string email { get; set; }
        public List<ItemsList> items { get; set; }
        public int pageSize { get; set; }
        public int page { get; set; }
        public string warehouseCode { get; set; }
        public double weight { get; set; }
        public string postcode { get; set; }
        public string order_code { get; set; }
        public string is_shipping_method_not_allow_update { get; set; }
    }

    public class ItemsList
    {
        public string product_sku { get; set; }
        public int quantity { get; set; }

    }
    public class DataList
    {
        public string order_code { get; set; }
        public string reference_no { get; set; }
        public string order_status { get; set; }
        public string tracking_no { get; set; }
    }
}