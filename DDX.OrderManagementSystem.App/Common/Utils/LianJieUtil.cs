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
    /// 义乌联捷线下挂号小包
    /// </summary>
    public class LianJieUtil
    {
        public static LianJieResultRootObject GetTracoCode(OrderType orderType, string t, string category, string productname, string ccountry, string pinfo, ISession session)
        {
            //            IP：114.55.43.183
            //ID：1808
            //密钥：vVuZx1oS2ZhiBXd

            string url = "http://114.55.43.183/cgi-bin/EmsData.dll?DoApp";
            string key = "vVuZx1oS2ZhiBXd";
            string cid = "1808";
            // TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            LianJieRootObject rootObject = new LianJieRootObject();
            rootObject.TimeStamp = GetTimeStamp();
            rootObject.RequestName = "PreInputSet";
            rootObject.cEmsKind = t;
            rootObject.icID = cid;
            rootObject.RecList = new List<LianJieRecList>();
            LianJieRecList lianjieRec = new LianJieRecList();
            if (orderType.AddressInfo.Country == "Germany")
            {
                lianjieRec.cDes = "德国";
            }
            else if (orderType.AddressInfo.Country == "France")
            {
                lianjieRec.cDes = "法国";
            }
            else if (orderType.AddressInfo.Country == "Great Britain")
            {
                lianjieRec.cDes = "英国";
            }
            else
            {
                lianjieRec.cDes = ccountry;
            }
            // lianjieRec .cDes = orderType.AddressInfo.Country == "Germany" ? "德国" : "英国";

            lianjieRec.cRCity = orderType.AddressInfo.City;
            lianjieRec.cRAddr = orderType.AddressInfo.Street;
            lianjieRec.cRCountry = orderType.AddressInfo.Country;
            // lianjieRec .cREMail = orderType.BuyerEmail;
            lianjieRec.cREMail = "";
            lianjieRec.cRNo = "";
            lianjieRec.cNum = orderType.OrderNo+ GetTimeStamp();
            lianjieRec.cRPhone = orderType.AddressInfo.Phone + "(" + orderType.AddressInfo.Tel + ")";
            lianjieRec.cRPostcode = orderType.AddressInfo.PostCode;
            lianjieRec.cRProvince = orderType.AddressInfo.Province;
            lianjieRec.cReceiver = orderType.AddressInfo.Addressee;
            lianjieRec.fWeight = "0.1";
            lianjieRec.iID = "0";
            //新增一个国家 奥地利Austria
            lianjieRec.cMoney = ((orderType.AddressInfo.Country == "Germany" || orderType.AddressInfo.Country == "France" || orderType.AddressInfo.Country == "Austria") ? "EUR" : "GBP");
            lianjieRec.fDValue = ((orderType.AddressInfo.Country == "Germany" || orderType.AddressInfo.Country == "France" || orderType.AddressInfo.Country == "Austria") ? (orderType.Amount < 21 ? orderType.Amount.ToString() : "21") : (orderType.Amount < 14 ? orderType.Amount.ToString() : "14"));

            lianjieRec.nItemType = "1";
            lianjieRec.nLanguage = "0";
            lianjieRec.cTransNote = productname;//托运备注
            lianjieRec.GoodsList = new List<LianJieGoodsList>();
            object obj = orderType.OrderNo;
            //内容对接
            foreach (var orderProductType in orderType.Products)
            {
                LianJieGoodsList foo = new LianJieGoodsList();
                //foo.cxGCodeA += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                foo.cxGCodeA = pinfo;
                foo.cxGoods = category;
                foo.cxGoodsA = category;
                foo.fxPrice = "5";
                foo.ixQuantity = orderProductType.Qty.ToString();
                lianjieRec.GoodsList.Add(foo);
                //cneRec.cGCodeA += "[" + orderProductType.SKU + "," + orderProductType.Qty + "]";
                lianjieRec.cGCodeA = pinfo;
                if (obj != null)
                    lianjieRec.cGCodeA += obj + " ";
            }
            // lianjieRec .cMemo = cneRec.cGoods;
            lianjieRec.cMemo = productname;//托运备注
            rootObject.RecList.Add(lianjieRec);

            rootObject.MD5 = MD5Encrypt(cid + rootObject.TimeStamp + key);



            //string xml = string.Format(postXML, cid, orderType.OrderNo + orderType.OrderNo, orderType.AddressInfo.Addressee,
            //                           orderType.AddressInfo.County, orderType.AddressInfo.Street,
            //                           orderType.AddressInfo.Phone, orderType.AddressInfo.Tel);

            //xml += "<MD5>" + MD5Encrypt(xml) + "</MD5>";

            string str = PostWebRequest(url, Newtonsoft.Json.JsonConvert.SerializeObject(rootObject));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LianJieResultRootObject>(str);

        }

        public static string GetTimeStamp()
        {
            // string url = "http://www.cnexps.com/cgi-bin/EmsData.dll?DoApp";//这个地址本地运行出错
            string url = "http://114.55.43.183/cgi-bin/EmsData.dll?DoApi";

            string poststr = "{\"RequestName\":\"TimeStamp\"}";

            string str = PostWebRequest(url, poststr);
            JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(str);
            return token["ReturnValue"].ToString();


        }
        public static string GetPDF(string ids)
        {
            string key = "vVuZx1oS2ZhiBXd";
            string cid = "1808";


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

        //public static LianJieResultRootObject DoGetNo(OrderType orderType, string t, string category, string productname, string ccountry, string pinfo, ISession session)
        //{
        //    string url = "http://114.55.43.183/cgi-bin/EmsData.dll?DoApp";
        //    string key = "vVuZx1oS2ZhiBXd";
        //    string cid = "1808";
        //    // TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

        //    LianJieRootObject1 rootObject = new LianJieRootObject1();
        //    rootObject.TimeStamp = GetTimeStamp();
        //    rootObject.RequestName = "DoGetNo";
        //    rootObject.cEmsKind = "新杭州小包";
        //    rootObject.icID = cid;
        //    rootObject.iType = "1";
        //    rootObject.MD5 = MD5Encrypt(cid + rootObject.TimeStamp + key);

        //    string str = PostWebRequest(url, Newtonsoft.Json.JsonConvert.SerializeObject(rootObject));
        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<LianJieResultRootObject>(str); 
        //}
    }



    public class LianJieGoodsList
    {
        public string cxGoods { get; set; }
        public string ixQuantity { get; set; }
        public string fxPrice { get; set; }
        public string cxGCodeA { get; set; }
        public string cxGoodsA { get; set; }
    }

    public class LianJieRecList
    {
        public string iID { get; set; }
        public string nItemType { get; set; }
        public string nLanguage { get; set; }
        public string cRNo { get; set; }
        public string cDes { get; set; }
        public string fWeight { get; set; }
        public string fDValue { get; set; }
        public string cMoney { get; set; }
        public string cGoods { get; set; }
        public string cMemo { get; set; }
        public string cNum { get; set; }


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
        public List<LianJieGoodsList> GoodsList { get; set; }
    }

    public class LianJieRootObject
    {
        public string RequestName { get; set; }
        public string icID { get; set; }
        public string TimeStamp { get; set; }
        public string MD5 { get; set; }
        public List<LianJieRecList> RecList { get; set; }
        public string cEmsKind { get; set; }
    }
    //public class LianJieRootObject1
    //{
    //    public string RequestName { get; set; }
    //    public string icID { get; set; }
    //    public string TimeStamp { get; set; }
    //    public string MD5 { get; set; }
    //    public string iType { get; set; }
    //    public string cEmsKind { get; set; }
    //}

    public class LianJieErrList
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

    public class LianJieResultRootObject
    {
        public string ReturnValue { get; set; }
        public string OK { get; set; }
        public List<LianJieErrList> ErrList { get; set; }
    }
}