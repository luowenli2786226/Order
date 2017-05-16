using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DDX.OrderManagementSystem.App.com.bdt.post;
using DDX.OrderManagementSystem.Domain;

namespace DDX.OrderManagementSystem.App.Common.Utils
{
    public class BatUtil
    {
        public static returnObject[] GetBdtTrackCode(OrderType orderType, string productname, int t, string pinfo)
        {
            productname = productname.Replace("礼物", "");
            productname = productname.Replace("Gift", "");
            productname = productname.Replace("礼品", "");
            productname = productname.Replace("present", "");
            productname = productname.Replace("日用品", "");
            productname = productname.Replace("饰品", "");
            productname = productname.Replace("生活用品", "");
            com.bdt.post.ParcelOprWebService sp = new ParcelOprWebService();
            string companyID = "5737034557ef5b8c02c0e46513b98f90";
            string pwd = "80275fad5b81900e4cf8e5fd81d03d26";
            parcel[] parcelList = new parcel[1];
            parcelList[0] = new parcel();
            parcelList[0].apdestination = orderType.Country;
            if (t == 0)//广州E邮宝
            {
                parcelList[0].apmethod = "GEUB";
            }
            else if (t == 1)//西班牙专线
            {
                parcelList[0].apmethod = "ESZX";
            }
            else if (t == 2)//美国专线
            {
                parcelList[0].apmethod = "BEUS";
            }
            else if (t == 3)//欧邮宝PG
            {
                parcelList[0].apmethod = "BEPG";  
            }
            else if (t == 4)//DHL挂号美国小包DGM US
            {
                parcelList[0].apmethod = "DGUS";
            }
            else if (t == 5)//欧邮宝PP
            {
                parcelList[0].apmethod = "BEPP";
                if (orderType.Country == "Russian Federation")
                {
                    parcelList[0].apdestination = "RUSSIA";
                }
            }
            else if (t == 6)//通达宝PG
            {
                parcelList[0].apmethod = "TRPG";
            }
            parcelList[0].apname = orderType.AddressInfo.Addressee;
            parcelList[0].apaddress = orderType.AddressInfo.Street;
            
            parcelList[0].apTel = ((orderType.AddressInfo.Tel != null || orderType.AddressInfo.Tel != "") ? orderType.AddressInfo.Tel : orderType.AddressInfo.Phone);
            if (orderType.AddressInfo.PostCode == null)
            {
                parcelList[0].zipCode = "";
            }
            else
            {
                parcelList[0].zipCode = orderType.AddressInfo.PostCode;
            }
            parcelList[0].refNo = orderType.OrderNo;
            foreach (var orderProductType in orderType.Products)
            {
                parcelList[0].apquantitys += orderProductType.Qty.ToString() + ";";
          //      parcelList[0].apweights += ((orderType.Weight != 0) ? (orderType.Weight / 1000.0 / orderProductType.Qty).ToString() : "1") + ";";
                parcelList[0].apweights += "0.1" + ";";
                parcelList[0].apvalues += "5" + ";";
                
            }
            parcelList[0].apdescriptions ="衣服";
            parcelList[0].sku = pinfo.TrimEnd(';');
            parcelList[0].apquantitys = parcelList[0].apquantitys.TrimEnd(';');
            parcelList[0].apweights = parcelList[0].apweights.TrimEnd(';');
            parcelList[0].apvalues = parcelList[0].apvalues.TrimEnd(';');
            parcelList[0].city = orderType.AddressInfo.City;
            parcelList[0].province = orderType.AddressInfo.Province;
            parcelList[0].address2 = orderType.AddressInfo.Street;
            parcelList[0].customsArticleName = parcelList[0].sku.Replace(";","/").TrimEnd('/');
            parcelList[0].customsArticleNames = parcelList[0].sku.Replace(";", "/").TrimEnd('/');
            parcelList[0].actualWeight = 0;

            returnObject[] re = sp.addParcelAndForecastService(companyID, pwd, parcelList);
            return re;
        }

        public static string GetBatpdfUrl(string orderInfo,int t)
        {
            string url = "";
            if (t == 0)//广州E邮宝
            {
                url = string.Format("http://post.8dt.com/lineUnderEub/fastPrintEub?shipCode=GEUB&printcode=01&trackingNos={0}", orderInfo);
            }
            else //西班牙专线、美国专线
            {
                url = string.Format("http://post.8dt.com/apiLabelPrint/freemarkerPrint?&apUserId=5737034557ef5b8c02c0e46513b98f90&apVsnumber=&abOrder=&apTrackingNo={0}&apRefNo=&&abColset=Y&pageType=Label_100_100&itemTitle=0&printType=pdf&peihuo=0", orderInfo);
            }
            return url;

        }

        public static parcel[] queryParcelByRefNoService(OrderType orderType, int t)
        {
            com.bdt.post.ParcelOprWebService sp = new ParcelOprWebService();
            string companyID = "5737034557ef5b8c02c0e46513b98f90";
            string pwd = "80275fad5b81900e4cf8e5fd81d03d26";
            string[] refNos = new string[1];
            refNos[0] = orderType.OrderNo;
            parcel[] re = sp.queryParcelByRefNoService(companyID, pwd, refNos);
            return re;
        }
    }
}