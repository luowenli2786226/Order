using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DDX.OrderManagementSystem.App.com.suyou.wsvc;
using DDX.OrderManagementSystem.Domain;

namespace DDX.OrderManagementSystem.App.Common.Utils
{
    /// <summary>
    /// 苏邮OMS发货系统对接
    /// </summary>
    public class SuYouUtil
    {
        public const string appToken = "996be8c42e6011f9b734c0f25b95e9e4";
        public const string appKey = "33749eef008fc48d6a8277c99eb04f11";
        public static string CreateOrder(OrderType OrderType)
        {
            //if (OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "ALTAIKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "ALTAIREPUBLIC" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "REPUBLICOFBURYATIA" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "IRKUTSKOBLAST" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KEMEROVOOBLAST" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KHANTY-MANSIAUTONOMOUS" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KRASNOYARSKKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "TOMSKOBLAST" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "YAMALO-NENETSAUTONOMOUSOKRUG" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "ZABAYKALSKYKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KRASNODARKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "AMUROBLAST" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "CHUKOTKAAUTONOMOUSOKRUG" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "JEWISHAUTONOMOUS" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KAMCHATKAKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "KHABAROVSKKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "MAGADANOBLAST" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "PRIMORSKYKRAI" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "SAKHAREPUBLIC" || OrderType.AddressInfo.Province.ToUpper().Replace(" ", "") == "SAKHALINOBLAST" )
            //{
            //    return "{\"ask\":\"False\",\"message\":\"该州被屏蔽上传\",\"order_code\":\"\"}";
            //}
            com.suyou.wsvc.Ec gc = new Ec();
            string service = "createOrder";
            ParamsJsonType paramsjson = new ParamsJsonType();
            paramsjson.platform =OrderType.Platform.Trim().ToUpper();
            if (OrderType.FBABy == "YWMRU-AEA" || OrderType.FBABy == "YWMRU-AEB")
            {
                paramsjson.warehouse_code = "RU";
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
            paramsjson.verify = "1";
            paramsjson.forceVerify = "1";
            paramsjson.is_shipping_method_not_allow_update = "0";
            paramsjson.items = new List<ItemsList>();

            foreach (var orderProductType in OrderType.Products)
            {
                ItemsList itemlist = new ItemsList();
                itemlist.product_sku = orderProductType.SKU;
                paramsjson.shipping_method = "2017_RP_LAND_0-10KG";
                itemlist.quantity = orderProductType.Qty;
                paramsjson.items.Add(itemlist);
            }
            string paramsJson = Newtonsoft.Json.JsonConvert.SerializeObject(paramsjson);
            string c = gc.callService(paramsJson, appToken, appKey, service);
            return c;
        }

        public static string GetOrderByRefCode(OrderType OrderType)
        {
            com.suyou.wsvc.Ec gc = new Ec();
            string service = "getOrderByRefCode";
            ParamsJsonType paramsjson = new ParamsJsonType();
            paramsjson.reference_no = OrderType.OrderExNo;
            string paramsJson = Newtonsoft.Json.JsonConvert.SerializeObject(paramsjson);
            string c = gc.callService(paramsJson, appToken, appKey, service);
            return c;
        }
    }

}