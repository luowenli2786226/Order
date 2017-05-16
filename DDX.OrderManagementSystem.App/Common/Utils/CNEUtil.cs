using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DDX.OrderManagementSystem.Domain;
using NHibernate;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App
{
    /// <summary>
    /// CNE物流对接-全球特惠
    /// </summary>
    public class CNEUtil
    {
        public static CNEResultRootObject GetTracoCode(OrderType orderType, string t, string category, string productname,string ccountry,string  pinfo, ISession session)
        {
            string url = "http://api.cnexps.com/cgi-bin/EmsData.dll?DoApi";
            string key = "aPsH1wEt2abfBMj";
            string cid = "12120";
            // TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            CNERootObject rootObject = new CNERootObject();
            rootObject.TimeStamp = GetTimeStamp();
            rootObject.RequestName = "PreInputSet";
            rootObject.cEmsKind = t;
            rootObject.icID = cid;
            rootObject.RecList = new List<CNERecList>();
            CNERecList cneRec = new CNERecList();
            if (orderType.AddressInfo.Country == "Germany")
            {
                cneRec.cDes = "德国";
            }
            else if (orderType.AddressInfo.Country == "France")
            {
                cneRec.cDes = "法国";
            }
            else if (orderType.AddressInfo.Country == "Great Britain")
            {
                cneRec.cDes = "英国";
            }
            else
            {
                cneRec.cDes = ccountry ;
            }
           // cneRec.cDes = orderType.AddressInfo.Country == "Germany" ? "德国" : "英国";

            cneRec.cRCity = orderType.AddressInfo.City;
            cneRec.cRAddr = orderType.AddressInfo.Street;
            cneRec.cRCountry = orderType.AddressInfo.Country;
           // cneRec.cREMail = orderType.BuyerEmail;
            cneRec.cREMail = "bestore64@hotmail.com";
            cneRec.cRNo = "";
            cneRec.cRPhone = orderType.AddressInfo.Phone + "(" + orderType.AddressInfo.Tel + ")";
            cneRec.cRPostcode = orderType.AddressInfo.PostCode;
            cneRec.cRProvince = orderType.AddressInfo.Province;
            cneRec.cReceiver = orderType.AddressInfo.Addressee;
            cneRec.fWeight = "0.1";
            cneRec.iID = "0";
            //CNE全球特惠新增一个国家 奥地利Austria
            cneRec.cMoney = ((orderType.AddressInfo.Country == "Germany" || orderType.AddressInfo.Country == "France" || orderType.AddressInfo.Country == "Austria") ? "EUR" : "GBP");
            cneRec.fDValue = ((orderType.AddressInfo.Country == "Germany" || orderType.AddressInfo.Country == "France" || orderType.AddressInfo.Country == "Austria") ? (orderType.Amount < 21 ? orderType.Amount.ToString() : "21") : (orderType.Amount < 14 ? orderType.Amount.ToString() : "14"));

            cneRec.nItemType = "1";
            cneRec.cTransNote = productname;//托运备注
            cneRec.GoodsList = new List<CNEGoodsList>();
            object obj = orderType.OrderNo;
            //原先对接内容不对
            ////foreach (var orderProductType in orderType.Products)
            ////{
            ////    CNEGoodsList foo = new CNEGoodsList();
            ////    foo.cxGoods = orderProductType.SKU;
            ////    //  foo.cxGoods = category;
            ////    foo.fxPrice = "5";
            ////    foo.ixQuantity = "1";
            ////    cneRec.GoodsList.Add(foo);
            ////    cneRec.cGoods += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
            ////    if (obj != null)
            ////        cneRec.cGoods += obj + " ";
            //// //   cneRec.cGoods = category; 
            ////}
            //内容对接
            foreach (var orderProductType in orderType.Products)
            {
                CNEGoodsList foo = new CNEGoodsList();
                //foo.cxGCodeA += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                foo.cxGCodeA = pinfo;
                foo.cxGoods = category;
                foo.cxGoodsA = category;
                foo.fxPrice = "5";
                foo.ixQuantity = orderProductType.Qty.ToString();
                cneRec.GoodsList.Add(foo);
                //cneRec.cGCodeA += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                cneRec.cGCodeA = pinfo;
                if (obj != null)
                    cneRec.cGCodeA += obj + " ";
            }
           // cneRec.cMemo = cneRec.cGoods;
            cneRec.cMemo = productname;//托运备注
            rootObject.RecList.Add(cneRec);

            rootObject.MD5 = MD5Encrypt(cid + rootObject.TimeStamp + key);



            //string xml = string.Format(postXML, cid, orderType.OrderNo + orderType.OrderNo, orderType.AddressInfo.Addressee,
            //                           orderType.AddressInfo.County, orderType.AddressInfo.Street,
            //                           orderType.AddressInfo.Phone, orderType.AddressInfo.Tel);

            //xml += "<MD5>" + MD5Encrypt(xml) + "</MD5>";

            string str = PostWebRequest(url, Newtonsoft.Json.JsonConvert.SerializeObject(rootObject));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CNEResultRootObject>(str);

        }

        public static string GetTimeStamp()
        {
           // string url = "http://www.cnexps.com/cgi-bin/EmsData.dll?DoApp";//这个地址本地运行出错
            string url = "http://api.cnexps.com/cgi-bin/EmsData.dll?DoApi";

            string poststr = "{\"RequestName\":\"TimeStamp\"}";

            string str = PostWebRequest(url, poststr);
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(str);
            return token["ReturnValue"].ToString();


        }
        public static string GetPDF(string ids)
        {
            string key = "aPsH1wEt2abfBMj";
            string cid = "12120";


            string MD5 = MD5Encrypt(cid + ids + key);
            string url = string.Format("http://label.cnexps.com/CnePrint?icID={0}&cNums={1}&ptemp=label10x10_1&signature={2}", cid, ids, MD5);

            return url;

            //string xml = string.Format(postXML, cid, orderType.OrderNo + orderType.OrderNo, orderType.AddressInfo.Addressee,
            //                           orderType.AddressInfo.County, orderType.AddressInfo.Street,
            //                           orderType.AddressInfo.Phone, orderType.AddressInfo.Tel);

            //xml += "<MD5>" + MD5Encrypt(xml) + "</MD5>";

            //  return PostWebRequest(url, Newtonsoft.Json.JsonConvert.SerializeObject(rootObject));

        }
        public static string MD5Encrypt(string strPwd)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strPwd, "MD5").ToLower();
        }


        public static string PostWebRequest(string url, string param)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            // string param = "ie=utf-8&source=txt&query=hello&t=1327829764203&token=8a7dcbacb3ed72cad9f3fb079809a127&from=auto&to=auto";
            //encoding.GetBytes(postData);
            byte[] bs = encoding.GetBytes(param);
            string responseData = String.Empty;
            req.Method = "POST";

            req.KeepAlive = true;
            req.ContentType = "application/x-www-form-urlencoded";
            //req.Timeout = 20;  

            req.ContentLength = bs.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    responseData = reader.ReadToEnd().ToString();
                    return responseData;
                }

            }
        }
    }



    public class CNEGoodsList
    {
        public string cxGoods { get; set; }
        public string ixQuantity { get; set; }
        public string fxPrice { get; set; }
        public string cxGCodeA { get; set; }
        public string cxGoodsA { get; set; }
    }

    public class CNERecList
    {
        public string iID { get; set; }
        public string nItemType { get; set; }
        public string cRNo { get; set; }
        public string cDes { get; set; }
        public string fWeight { get; set; }
        public string fDValue { get; set; }
        public string cMoney { get; set; }
        public string cGoods { get; set; }
        public string cMemo { get; set; }


        public string cReceiver { get; set; }
        public string cRPhone { get; set; }
        public string cREMail { get; set; }
        public string cRPostcode { get; set; }
        public string cRCountry { get; set; }
        public string cRProvince { get; set; }
        public string cRCity { get; set; }
        public string cRAddr { get; set; }
        public string cGCodeA { get; set; }
        public string cTransNote { get; set; }//托运备注
        public List<CNEGoodsList> GoodsList { get; set; }
    }

    public class CNERootObject
    {
        public string RequestName { get; set; }
        public string icID { get; set; }
        public string TimeStamp { get; set; }
        public string MD5 { get; set; }
        public List<CNERecList> RecList { get; set; }
        public string cEmsKind { get; set; }
    }

    public class ErrList
    {
        public string iIndex { get; set; }
        public string iID { get; set; }
        public string cNum { get; set; }
        public string cNo { get; set; }
        public string cMess { get; set; }
        public string cEmsKinda { get; set; }
        public string cReserve { get; set; }
        public string cBy1 { get; set; }
        public string cBy2 { get; set; }
        public string cBy3 { get; set; }
        public string cBy4 { get; set; }
        public string cBy5 { get; set; }
    }

    public class CNEResultRootObject
    {
        public string ReturnValue { get; set; }
        public string OK { get; set; }
        public List<ErrList> ErrList { get; set; }
    }
}