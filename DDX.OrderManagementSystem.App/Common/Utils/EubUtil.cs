using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Common
{
    public class EubUtil
    {


        public static string GenerationEubTrackCode(OrderType order, string pinfo, int t)
        {
            try
            {
                #region xml
                string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>VIKI</name>  
      <postcode>315000</postcode>  
      <phone>15988173792</phone>
      <mobile>0574-27903940</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330200</city>  
      <county>330204</county>  
      <company>Ningbo Bestore Co Ltd</company>  
      <street>3F,NO.4 Building,JUNSHENG Group,1266,Juxian Road</street>  
      <email>bestore01@hotmail.com</email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>吕晶晶</name>  
      <postcode>100067</postcode>  
      <phone>18505885815</phone>  
      <mobile>0574-27903940</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330200</city>  
      <county>330204</county>  
      <company/>  
      <street>聚贤路399号</street>  
      <email>bestore01@hotmail.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                //义乌寄件地址及退件地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>BaoJun</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company>YiWu YouLian Co Ltd</company>  
      <street>Zongze North Road NO.531</street>  
      <email>2045162821@qq.com</email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>鲍骏</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>  
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company/>  
      <street>宗泽北路531号</street>  
      <email>2045162821@qq.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                }
                #endregion
                if (pinfo.Length > 60)
                {
                    pinfo = pinfo.Substring(0, 58);
                }

                string sss = string.Format(postString, order.OrderNo, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                                           DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                                           order.AddressInfo.Addressee.Replace("'", "&apos;"),
                                           order.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(),
                                           order.AddressInfo.Phone, order.AddressInfo.Tel,
                                           order.AddressInfo.Province, order.AddressInfo.City,
                                           order.AddressInfo.County == null ? "" : order.AddressInfo.County,
                                           order.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), pinfo,
                                           order.AddressInfo.CountryCode.Replace(" ", "").Replace("'", "&apos;"), t);

                //这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
                byte[] postData = Encoding.UTF8.GetBytes(sss); //编码，尤其是汉字，事先要看下抓取网页的编码方式  


                //string url = "http://www.ems.com.cn/partner/api/public/p/order/"; //地址
                //string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    //url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                    url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                }

                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                if (order.Account.IndexOf("yw") != -1)
                {
                    // webClient.Headers.Add("authenticate", "unionsource_e6ac9ab417e23f84b03908f866c27dc0");
                    webClient.Headers.Add("authenticate", "youlian_89254693f18c3af8999675047757acc4");
                }
                else
                {
                    //webClient.Headers.Add("authenticate", "bestore_5f812ed2aa0b3880aefe1831f952a5c0");
                    webClient.Headers.Add("authenticate", "niyihang147_6aedcceee3853a059c288372b131d097"); // 2016-09-07更新(吕晶晶)
                }

                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Value;
            }
            catch (Exception ex)
            {

                return "";
            }


        }

        public static string GenerationEubTrackCode2(OrderType order, string pinfo, int t)
        {
            try
            {
                #region xml

                string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>XIAO LEI</name>  
      <postcode>200327</postcode>  
      <phone>18505885815</phone>
      <mobile>0000000</mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company>JI FENG</company>  
      <street>NO 169 LUOJING ROAD</street>  
      <email>bestore01@hotmail.com</email> 
    </sender>  
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
    <collect> 
       <name>吕晶晶</name>  
      <postcode>200327</postcode>  
      <phone>18505885815</phone>  
      <mobile>0000000</mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>  
      <street>罗锦路169号</street>  
      <email>bestore01@hotmail.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";

                //义乌寄件地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>BaoJun</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company>YiWu YouLian Co Ltd</company>  
      <street>Zongze North Road NO.531</street>  
      <email>2045162821@qq.com</email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>鲍骏</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>  
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company/>  
      <street>宗泽北路531号</street>  
      <email>2045162821@qq.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                }
                #endregion
                if (pinfo.Length > 60)
                {
                    pinfo = pinfo.Substring(0, 58);
                }

                string sss = string.Format(postString, order.OrderNo, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                                     DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"), order.AddressInfo.Addressee.Replace("'", "&apos;"),
                                     order.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(), order.AddressInfo.Phone, order.AddressInfo.Tel,
                                     order.AddressInfo.Province, order.AddressInfo.City,
                                     order.AddressInfo.County == null ? "" : order.AddressInfo.County, order.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), pinfo, order.AddressInfo.CountryCode.Replace(" ", "").Replace("'", "&apos;"), t);

                //这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
                byte[] postData = Encoding.UTF8.GetBytes(sss);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
                //string url = "http://www.ems.com.cn/partner/api/public/p/order/";//地址
                string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址

                if (order.Account.IndexOf("yw") != -1)
                {
                    url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                }

                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                if (order.Account.IndexOf("yw") != -1)
                {
                    webClient.Headers.Add("authenticate", "youlian_89254693f18c3af8999675047757acc4");
                }
                else
                {
                    //webClient.Headers.Add("authenticate", "bestore_5f812ed2aa0b3880aefe1831f952a5c0");
                    webClient.Headers.Add("authenticate", "niyihang147_6aedcceee3853a059c288372b131d097");
                }

                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Value;
            }
            catch (Exception ex)
            {

                return "";
            }


        }

        public static string GenerationEubTrackCode3(OrderType order, string pinfo, int t)
        {
            try
            {
                #region xml

                string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>Mr. ZHU</name>  
      <postcode>210058</postcode>  
      <phone>13162255335</phone>
      <mobile>0574-27903940</mobile>  
      <country>CN</country>  
      <province>320000</province>  
      <city>320100</city>  
      <county>320113</county>  
      <company></company>  
      <street>QIXIA DISTRICT NO.1PORT ROAD LONGTAN LOGISTICS BASE NO A-96</street>  
      <email>bestore01@hotmail.com</email> 
    </sender>  
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
    <collect> 
       <name>吕晶晶</name>  
      <postcode>100067</postcode>  
      <phone>18505885815</phone>  
      <mobile>0574-27903940</mobile>  
      <country>CN</country>  
      <province>320000</province>  
      <city>320100</city>  
      <county>320113</county>  
      <company/>  
      <street>南京经济技术开发区龙潭街道疏港路1号龙潭物流基地A一96号</street>  
      <email>bestore01@hotmail.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";

                //义乌寄件地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>BaoJun</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company>YiWu YouLian Co Ltd</company>  
      <street>Zongze North Road NO.531</street>  
      <email>2045162821@qq.com</email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>鲍骏</name>  
      <postcode>322000</postcode>  
      <phone>18329076923</phone>  
      <mobile>0579-89894946</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330700</city>  
      <county>331382</county>  
      <company/>  
      <street>宗泽北路531号</street>  
      <email>2045162821@qq.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                }
                #endregion
                if (pinfo.Length > 60)
                {
                    pinfo = pinfo.Substring(0, 58);
                }

                string sss = string.Format(postString, order.OrderNo, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                                     DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"), order.AddressInfo.Addressee.Replace("'", "&apos;"),
                                     order.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(), order.AddressInfo.Phone, order.AddressInfo.Tel,
                                     order.AddressInfo.Province, order.AddressInfo.City,
                                     order.AddressInfo.County == null ? "" : order.AddressInfo.County, order.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), pinfo, order.AddressInfo.CountryCode.Replace(" ", "").Replace("'", "&apos;"), t);

                //这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
                byte[] postData = Encoding.UTF8.GetBytes(sss);//编码，尤其是汉字，事先要看下抓取网页的编码方式  

                //string url = "http://www.ems.com.cn/partner/api/public/p/order/"; //地址
                string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                }

                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                if (order.Account.IndexOf("yw") != -1)
                {
                    webClient.Headers.Add("authenticate", "youlian_89254693f18c3af8999675047757acc4");
                }
                else
                {
                    //webClient.Headers.Add("authenticate", "bestore_5f812ed2aa0b3880aefe1831f952a5c0");
                    webClient.Headers.Add("authenticate", "niyihang147_6aedcceee3853a059c288372b131d097");
                }

                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Value;
            }
            catch (Exception ex)
            {

                return "";
            }


        }

        // 联捷E邮宝
        public static string GenerationEubTrackCode4(OrderType order, string pinfo, int t)
        {
            try
            {
                #region xml（宁波）
           /*    string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>james</name>  
      <postcode>315334</postcode>  
      <phone>15888059253</phone>
      <mobile>15888059253</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330200</city>  
      <county>330282</county>  
      <company/>
      <street>399# CHONGSHOU BLVD.CHONGSHOU TOWN CiXiShi</street>  
      <email></email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>james</name>  
      <postcode>315334</postcode>  
      <phone>15888059253</phone>  
      <mobile>15888059253</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330200</city>  
      <county>330282</county>  
      <company/>  
      <street>崇寿镇崇寿东路399</street>  
      <email></email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
*/
                #endregion

                #region xml （杭州）
//                string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
//<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
//  <order> 
//    <orderid>{0}</orderid>  
//    <!--客户订单号-->  
//    <operationtype>{13}</operationtype>  
//    <!--业务类型（默认0）-->  
//    <producttype>0</producttype>  
//    <!--产品种类（默认0）-->  
//    <customercode>33020310535000</customercode>  
//    <!--客户代码-->  
//    <vipcode>33020310535000</vipcode>  
//    <!--大客户代码-->  
//    <clcttype>1</clcttype>  
//    <!--揽收类型上门或者自送-->  
//    <pod>false</pod>  
//    <!--电子签收-->  
//    <untread>Returned</untread>  
//    <!--退回-->  
//    <volweight>10</volweight>  
//    <!--体积重量-->  
//    <startdate>{1}</startdate>  
//    <!--起始预约时间-->  
//    <enddate>{2}</enddate>  
//    <printcode>03</printcode>
//    <remark>{0}</remark>
//    <sku1>{0}</sku1>
//    <sku2>{0}</sku2>
//    <barcode>{0}</barcode>  
//    <sender> 
//      <name>yangjianjian</name>  
//      <postcode>310000</postcode>  
//      <phone>15381029008</phone>
//      <mobile>15381029008</mobile>  
//      <country>CN</country>  
//      <province>330000</province>  
//      <city>330100</city>  
//      <county>330186</county>  
//      <company/>
//      <street>economic development zone lin ping qita qu HANGZHOU CHIAN</street>  
//      <email></email> 
//    </sender>   
//    <receiver> 
//      <name>{3}</name>  
//      <postcode>{4}</postcode>  
//      <phone>{5}</phone>  
//      <mobile>{6}</mobile>  
//      <country>{12}</country>  
//      <province>{7}</province>  
//      <city>{8}</city>  
//      <county>{9}</county>  
//      <street>{10}</street> 
//    </receiver>  
//       <collect> 
//      <name>yangjianjian</name>  
//      <postcode>310000</postcode>  
//      <phone>15381029008</phone>  
//      <mobile>15381029008</mobile>  
//      <country>CN</country>  
//      <province>330000</province>  
//      <city>330100</city>  
//      <county>330186</county>  
//      <company/>  
//      <street>临平区经济开发区</street>  
//      <email></email> 
//    </collect>  
//    <items> 
//      <item> 
//        <cnname>衣服{11}</cnname>
//        <enname>Cloth</enname>  
//        <count>1</count>  
//        <unit>unit</unit>  
//        <weight>0.1</weight>  
//        <delcarevalue>5</delcarevalue>  
//        <origin>CN</origin>  
//        <description>sxd</description> 
//     </item>
//    </items>  
//  </order> 
//</orders>
//";

               #endregion
                string postString = "";
                if (t == 0)
                {
                    #region xml （联捷上海E邮宝）
                   postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>MR TIAN  TY1</name>  
      <postcode>201105</postcode>  
      <phone></phone>
      <mobile></mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>
      <street>NO.1888 HU QING PING ROAD  SHANGHAI</street>  
      <email></email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>MR TIAN  TY1</name>  
      <postcode>201105</postcode>  
      <phone></phone>  
      <mobile></mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>  
      <street>NO.1888 HU QING PING ROAD  SHANGHAI</street>  
      <email></email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                    #endregion
                }
                 
                else   if (t == 1)
                {
                    #region xml （联捷广州E邮宝）
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>MR.WONG</name>  
      <postcode>510000</postcode>  
      <phone>18966021616</phone>
      <mobile></mobile>  
      <country>CN</country>  
      <province>440000</province>  
      <city>440100</city>  
      <county>440111</county>  
      <company/>
      <street>baiyu huangshidonglu NO.70 BaiYunQu Guangzhoushi 510000 China</street>  
      <email></email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>MR.WONG</name>  
      <postcode>510000</postcode>  
      <phone>18966021616</phone>  
      <mobile></mobile>  
      <country>CN</country>  
      <province>440000</province>  
      <city>440100</city>  
      <county>440111</county>  
      <company/>  
      <street>广州市白云区黄石东路70号</street>  
      <email></email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                    #endregion
                }
                else if (t == 2)
                {
                    #region xml （汇鑫杭州E邮宝）
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>huixin</name>  
      <postcode>310000</postcode>  
      <phone>17757904520</phone>
      <mobile>17757904520</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330100</city>  
      <county>330106</county>  
      <company/>
      <street>Wensanlu Road No. 90 East Software Park</street>  
      <email></email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>huixin</name>  
      <postcode>310000</postcode>  
      <phone>17757904520</phone>  
      <mobile>17757904520</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>330100</city>  
      <county>330106</county>  
      <company/>  
      <street>Wensanlu Road No. 90 East Software Park</street>  
      <email></email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                    #endregion
                }
                else if (t == 3)
                {
                    #region xml （汇鑫上海E邮宝）
                    postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <remark>{0}</remark>
    <sku1>{0}</sku1>
    <sku2>{0}</sku2>
    <barcode>{0}</barcode>  
    <sender> 
      <name>Wanggang</name>  
      <postcode>201105</postcode>  
      <phone>85853555</phone>
      <mobile>85853555</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>
      <street>No16.253 Lane HangDong Road，MinHangQu Shanghai</street>  
      <email></email> 
    </sender>   
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
       <collect> 
      <name>王刚</name>  
      <postcode>201105</postcode>  
      <phone>85853555</phone>  
      <mobile>85853555</mobile>  
      <country>CN</country>  
      <province>330000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>  
      <street>上海市闵行区航东路253弄16号</street>  
      <email></email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>
    </items>  
  </order> 
</orders>
";
                    #endregion
                }

           
                if (pinfo.Length > 60)
                {
                    pinfo = pinfo.Substring(0, 58);
                }

                string sss = string.Format(postString, order.OrderNo, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                                           DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                                           order.AddressInfo.Addressee.Replace("'", "&apos;"),
                                           order.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(),
                                           order.AddressInfo.Phone, order.AddressInfo.Tel,
                                           order.AddressInfo.Province, order.AddressInfo.City,
                                           order.AddressInfo.County == null ? "" : order.AddressInfo.County,
                                           order.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), pinfo,
                                           order.AddressInfo.CountryCode.Replace(" ", "").Replace("'", "&apos;"), 0);

                //这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
                byte[] postData = Encoding.UTF8.GetBytes(sss); //编码，尤其是汉字，事先要看下抓取网页的编码方式  


                //string url = "http://www.ems.com.cn/partner/api/public/p/order/"; //地址
                string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                if (order.Account.IndexOf("yw") != -1)
                {
                    url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址
                }

                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                //webClient.Headers.Add("authenticate", "fishdrops_421074f4e7f838d3b19d84f2275e70f3");//宁波关闭
                //webClient.Headers.Add("authenticate", "yangjianjian_e85a797e98043a3d88160e6d38caf7aa");//杭州
                if (t == 0)
                {
                    //webClient.Headers.Add("authenticate", "master_80d93cde1919350b9408cd0244d5982f");//联捷上海E邮宝
                    webClient.Headers.Add("authenticate", "tianyi_1d50ad23a076302ab7e7c65c427cc74a");//联捷上海E邮宝
                }
                else   if (t == 1)
                {
                    webClient.Headers.Add("authenticate", "LIHAIBO_c9dc6d23473d34b6aa8682a988be73ab");//联捷广州E邮宝
                }
                else if (t == 2)
                {
                    webClient.Headers.Add("authenticate", "EB001_a3e6274b844f364f9db51e87ba3450e6");//汇鑫杭州E邮宝
                }
                else if (t == 3)
                {
                    webClient.Headers.Add("authenticate", "liuyong3_b7edfc0444633cf4a2e0e82b54e0f28e");//汇鑫杭州E邮宝
                }
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Value;
            }
            catch (Exception ex)
            {

                return "";
            }


        }

        // e邮宝（电子）
        public static string GenerationEubTrackCode5(OrderType order, string pinfo, int t)
        {
            try
            {
                #region xml

                string postString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
  <order> 
    <orderid>{0}</orderid>  
    <!--客户订单号-->  
    <ordernum>{14}</ordernum>
    <!--客户订单号-->
    <operationtype>{13}</operationtype>  
    <!--业务类型（默认0）-->  
    <producttype>0</producttype>  
    <!--产品种类（默认0）-->  
    <customercode>33020310535000</customercode>  
    <!--客户代码-->  
    <vipcode>33020310535000</vipcode>  
    <!--大客户代码-->  
    <clcttype>1</clcttype>  
    <!--揽收类型上门或者自送-->  
    <pod>false</pod>  
    <!--电子签收-->  
    <untread>Returned</untread>  
    <!--退回-->  
    <volweight>10</volweight>  
    <!--体积重量-->  
    <startdate>{1}</startdate>  
    <!--起始预约时间-->  
    <enddate>{2}</enddate>  
    <printcode>03</printcode>
    <sender> 
      <!--寄件人地址，英文-->
      <name>SUNWEN</name>  
      <postcode>200327</postcode>  
      <phone>13807961234</phone>
      <mobile>13807961234</mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company>JI FENG</company>  
      <street>LUZHOUDONGLU 11 HAO JiZhouQu JiAnShi JIANGXI 343000 CHINA</street>  
      <email>bestore01@hotmail.com</email> 
    </sender>  
    <receiver> 
      <name>{3}</name>  
      <postcode>{4}</postcode>  
      <phone>{5}</phone>  
      <mobile>{6}</mobile>  
      <country>{12}</country>  
      <province>{7}</province>  
      <city>{8}</city>  
      <county>{9}</county>  
      <street>{10}</street> 
    </receiver>  
    <collect> 
      <!--揽收地址，中文-->
      <name>吕晶晶</name>  
      <postcode>200327</postcode>  
      <phone>13807961234</phone>  
      <mobile>0000000</mobile>  
      <country>CN</country>  
      <province>310000</province>  
      <city>310100</city>  
      <county>310112</county>  
      <company/>  
      <street>LUZHOUDONGLU 11 HAO JiZhouQu JiAnShi JIANGXI 343000 CHINA</street>  
      <email>bestore01@hotmail.com</email> 
    </collect>  
    <items> 
      <item> 
        <cnname>衣服{11}</cnname>
        <enname>Cloth</enname>  
        <count>1</count>  
        <unit>unit</unit>  
        <weight>0.1</weight>  
        <delcarevalue>5</delcarevalue>  
        <origin>CN</origin>  
        <description>sxd</description> 
     </item>  
    </items>  
    <remark>{0}</remark>
  </order> 
</orders>
";
                #endregion
                if (pinfo.Length > 60)
                {
                    pinfo = pinfo.Substring(0, 58);
                }

                string sss = string.Format(postString, order.OrderNo, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                                     DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"), order.AddressInfo.Addressee.Replace("'", "&apos;"),
                                     order.AddressInfo.PostCode.ToString().Replace("'", "&apos;").Trim(), order.AddressInfo.Phone, order.AddressInfo.Tel,
                                     order.AddressInfo.Province, order.AddressInfo.City,
                                     order.AddressInfo.County == null ? "" : order.AddressInfo.County, order.AddressInfo.Street.Replace(" ", "").Replace("'", "&apos;"), pinfo, order.AddressInfo.CountryCode.Replace(" ", "").Replace("'", "&apos;"), t, order.OrderNo);

                //这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
                byte[] postData = Encoding.UTF8.GetBytes(sss);//编码，尤其是汉字，事先要看下抓取网页的编码方式  

                string url = "http://shipping.ems.com.cn/partner/api/public/p/order/"; //地址

                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");

                webClient.Headers.Add("authenticate", "ZJGJDS_fb6ab829fc8833a8b902bd950872b5f9");


                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Value;
            }
            catch (Exception ex)
            {

                return "";
            }


        }

        public static string getEubPdfUrl(string orderInfo, string printcode, bool isyi)
        {

            string str = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<printcode>{1}</printcode>
<filetype>0</filetype>
{0}
</orders>";

            //<order><mailnum>LN000000005CN</mailnum></order>
            //<order><mailnum>CX000000006CN</mailnum></order>
            //<order><mailnum>LN000000007CN</mailnum></order>
            str = string.Format(str, orderInfo, printcode);
            string url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels";
            //string url = "http://www.ems.com.cn/partner/api/public/p/print/downloadLabels";
            //string url = "http://www.ems.com.cn/partner/api/public/p/order/"; //地址
            if (isyi)
            {
                url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels"; //地址
            }
            byte[] postData = Encoding.UTF8.GetBytes(str);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                if (isyi)
                {
                    webClient.Headers.Add("authenticate", "youlian_89254693f18c3af8999675047757acc4");
                }
                else
                {
                    //webClient.Headers.Add("authenticate", "bestore_5f812ed2aa0b3880aefe1831f952a5c0");
                    webClient.Headers.Add("authenticate", "niyihang147_6aedcceee3853a059c288372b131d097");
                }
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Element("url").Value;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        //联捷E邮宝
        public static string getEubPdfUrl1(string orderInfo, string printcode, bool isyi,int t)
        {

            string str = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<printcode>{1}</printcode>
<filetype>0</filetype>
{0}
</orders>";

            //<order><mailnum>LN000000005CN</mailnum></order>
            //<order><mailnum>CX000000006CN</mailnum></order>
            //<order><mailnum>LN000000007CN</mailnum></order>
            str = string.Format(str, orderInfo, printcode);
            string url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels";
            if (isyi)
            {
                url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels"; //地址
            }
            byte[] postData = Encoding.UTF8.GetBytes(str);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                //webClient.Headers.Add("authenticate", "fishdrops_421074f4e7f838d3b19d84f2275e70f3");//宁波关闭
               // webClient.Headers.Add("authenticate", "yangjianjian_e85a797e98043a3d88160e6d38caf7aa");//杭州
                if (t == 0 || t == 1)//联捷上海E邮宝
                {
                    //webClient.Headers.Add("authenticate", "master_80d93cde1919350b9408cd0244d5982f");//联捷上海E邮宝
                    webClient.Headers.Add("authenticate", "tianyi_1d50ad23a076302ab7e7c65c427cc74a");//联捷上海E邮宝
                }
                else if (t == 2 || t == 3)//联捷广州E邮宝
                {
                    webClient.Headers.Add("authenticate", "LIHAIBO_c9dc6d23473d34b6aa8682a988be73ab");
                }
                else if (t == 4 || t == 5)//汇鑫杭州E邮宝
                {
                    webClient.Headers.Add("authenticate", "EB001_a3e6274b844f364f9db51e87ba3450e6");
                }
                else if (t == 6 || t == 7)//汇鑫上海E邮宝
                {
                    webClient.Headers.Add("authenticate", "liuyong3_b7edfc0444633cf4a2e0e82b54e0f28e");
                }
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Element("url").Value;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        //联捷E获得e邮宝（电子）单号
        public static string getEubPdfUrl3(string orderInfo, string printcode, bool isyi)
        {

            string str = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<orders xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<printcode>{1}</printcode>
<filetype>0</filetype>
{0}
</orders>";

            //<order><mailnum>LN000000005CN</mailnum></order>
            //<order><mailnum>CX000000006CN</mailnum></order>
            //<order><mailnum>LN000000007CN</mailnum></order>
            str = string.Format(str, orderInfo, printcode);
            string url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels";
            if (isyi)
            {
                url = "http://shipping.ems.com.cn/partner/api/public/p/print/downloadLabels"; //地址
            }
            byte[] postData = Encoding.UTF8.GetBytes(str);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可 

                webClient.Headers.Add("version", "international_eub_us_1.1");
                webClient.Headers.Add("authenticate", "ZJGJDS_fb6ab829fc8833a8b902bd950872b5f9");
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                string srcString = Encoding.UTF8.GetString(responseData);//解码 
                XElement root = XElement.Parse(srcString);
                return root.Element("url").Value;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        public string MD5Encrypt(string strPwd)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strPwd, "MD5").ToLower();
        }


    }
}