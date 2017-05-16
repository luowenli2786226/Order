using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using DDX.OrderManagementSystem.App.Common.Utils;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using Newtonsoft.Json;

namespace DDX.OrderManagementSystem.App
{
    public static class Utilities
    {
        public const string OrderNo = "OrderNo";
        public const string UserNo = "UserNo";
        public const string PlanNo = "PlanNo";
        public const string Start_Time = "_st";
        public const string End_Time = "_et";
        public const string Start_Int = "_si";
        public const string End_Int = "_ei";
        public const string End_String = "_es";
        public const string DDL_String = "_ds";
        public const string DDL_UnString = "_un";
        public const string NotLike = "_uk";
        public const string ISNULL = "_nnl";//不为空
        public const string ISALL = "_all";//多选
        public const string CookieName = "DDX.OrderManagementSystem_User";
        public const string BPicPath = "/ProductImg/BPic/";
        public const string SPicPath = "/ProductImg/SPic/";
        public const string ImgPath = "/Product/Images/";
        public static object obj1 = new object();
        public static object obj2 = new object();
        public static object obj3 = new object();
        public static object obj4 = new object();
        public static object obj5 = new object();
        public static object obj6 = new object();
        public static object obj7 = new object();
        public static object obj8 = new object();

        #region MyRegion
        //public static string GetOrderNo(ISession NSession)
        //{
        //    lock (obj1)
        //    {
        //        string result = string.Empty;
        //        NSession.Clear();
        //        IList<SerialNumberType> list = NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", OrderNo).List<SerialNumberType>();
        //        if (list.Count > 0)
        //        {
        //            list[0].BeginNo = list[0].BeginNo + 1;
        //            NSession.Update(list[0]);
        //            NSession.Flush();
        //            return list[0].BeginNo.ToString();
        //        }
        //        return "";
        //    }
        //}
        //public static string GetUserNo(ISession NSession)
        //{
        //    lock (obj2)
        //    {
        //        string result = string.Empty;
        //        NSession.Clear();
        //        IList<SerialNumberType> list =
        //            NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", UserNo).List
        //                <SerialNumberType>();
        //        if (list.Count > 0)
        //        {
        //            list[0].BeginNo = list[0].BeginNo + 1;
        //            NSession.Update(list[0]);
        //            NSession.Flush();
        //            return list[0].BeginNo.ToString();
        //        }
        //        return "";
        //    }
        //}

        //public static string GetPlanNo(ISession NSession)
        //{
        //    lock (obj3)
        //    {
        //        string result = string.Empty;

        //        NSession.Clear();
        //        IList<SerialNumberType> list = NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", PlanNo).List<SerialNumberType>();
        //        if (list.Count > 0)
        //        {
        //            list[0].BeginNo = list[0].BeginNo + 1;
        //            NSession.Update(list[0]);
        //            NSession.Flush();
        //            return "SP" + list[0].BeginNo.ToString();
        //        }
        //        return "";
        //    }
        //}

        //public static int GetSKUCode(int count, ISession NSession)
        //{
        //    lock (obj4)
        //    {
        //        string result = string.Empty;
        //        int no = 0;
        //        NSession.Clear();
        //        IList<SerialNumberType> list = NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", "SKUNo").List<SerialNumberType>();
        //        if (list.Count > 0)
        //        {
        //            no = list[0].BeginNo + 1;
        //            list[0].BeginNo = list[0].BeginNo + count;
        //            NSession.Update(list[0]);
        //            NSession.Flush();
        //            return no;
        //        }
        //        return 0;
        //    }
        //} 
        #endregion

        public static String GetTrackCode(ISession NSession, string LogisticMode)
        {
            lock (obj5)
            {
                string result = string.Empty;
                NSession.Clear();
                IList<PostTrackCodeType> list =
                    NSession.CreateQuery("from PostTrackCodeType where IsUse=0 and LogisticMode ='" + LogisticMode + "' Order By Id").SetMaxResults(1).List<PostTrackCodeType>();
                if (list.Count > 0)
                {
                    list[0].IsUse = 1;
                    NSession.Update(list[0]);
                    NSession.Flush();
                    return list[0].Code;
                }
                return "已用完";
            }
        }

        public static string GetOrderNo(ISession NSession)
        {
            return GetNo(NSession, "OrderNo");
        }

        public static string GetAreaCode(ISession NSession)
        {
            lock (obj6)
            {
                string count = NSession.CreateSQLQuery("select COUNT(*) from WarehouseArea b  where not exists (select * from WarehouseArea a where cast(REPLACE(a.AreaCode,'A-','') as bigint)=cast(REPLACE(b.AreaCode,'A-','')  as bigint)+1)").UniqueResult().ToString();
                if (count == "0")
                {
                    return "A-0001";
                }
                else
                {
                    string code = NSession.CreateSQLQuery("select RIGHT(10000000001+min(REPLACE(AreaCode,'A-','')),4) from WarehouseArea b  where not exists (select * from WarehouseArea a where cast(REPLACE(a.AreaCode,'A-','')  as bigint)=cast(REPLACE(b.AreaCode,'A-','')  as bigint)+1)").UniqueResult().ToString();
                    return "A-" + code;
                }
            }
        }
        public static string GetLineCode(ISession NSession)
        {
            lock (obj7)
            {
                string count = NSession.CreateSQLQuery("select COUNT(*) from WarehouseLine b  where not exists (select * from WarehouseLine a where cast(REPLACE(a.LineCode,'L-','')  as bigint)=cast(REPLACE(b.LineCode,'L-','')  as bigint)+1)").UniqueResult().ToString();
                if (count == "0")
                {
                    return "L-0001";
                }
                else
                {
                    string code = NSession.CreateSQLQuery("select RIGHT(10000000001+min(REPLACE(LineCode,'L-','')),4) from WarehouseLine b  where not exists (select * from WarehouseLine a where cast(REPLACE(a.LineCode,'L-','')  as bigint)=cast(REPLACE(b.LineCode,'L-','')  as bigint)+1)").UniqueResult().ToString();
                    return "L-" + code;
                }
            }
        }
        public static string GetRackCode(ISession NSession)
        {
            lock (obj8)
            {
                string count = NSession.CreateSQLQuery("select COUNT(*) from WarehouseRack b  where not exists (select * from WarehouseRack a where cast(a.RackCode as bigint)=cast(b.RackCode as bigint)+1)").UniqueResult().ToString();
                if (count == "0")
                {
                    return "0001";
                }
                else
                {
                    string code = NSession.CreateSQLQuery("select RIGHT(10000000001+min(RackCode),4) from WarehouseRack b  where not exists (select * from WarehouseRack a where cast(a.RackCode as bigint)=cast(b.RackCode as bigint)+1)").UniqueResult().ToString();
                    return code;
                }
            }
        }

        public static string GetSKUNo(ISession NSession)
        {
            return GetNo(NSession, "SKUNo");
        }

        public static string GetNo(ISession NSession, string NoType)
        {
            lock (obj1)
            {
                string result = string.Empty;
                NSession.Clear();
                IList<SerialNumberType> list =
                    NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", NoType).List
                        <SerialNumberType>();

                if (list.Count > 0)
                {
                    list[0].BeginNo = list[0].BeginNo + 1;
                    NSession.Update(list[0]);
                    NSession.Flush();
                    return list[0].BeginNo.ToString();
                }
                else
                {
                    SerialNumberType snt = new SerialNumberType();
                    snt.BeginNo = 1;
                    snt.Code = NoType;
                    NSession.Save(snt);
                    NSession.Flush();
                    return snt.BeginNo.ToString();
                }
                return "";
            }
        }

        public static int GetSKUCode(int count, ISession NSession)
        {
            lock (obj4)
            {
                string result = string.Empty;
                int no = 0;
                NSession.Clear();
                IList<SerialNumberType> list = NSession.CreateQuery(" from SerialNumberType where Code=:p").SetString("p", "SKUNo").List<SerialNumberType>();
                if (list.Count > 0)
                {
                    no = list[0].BeginNo + 1;
                    list[0].BeginNo = list[0].BeginNo + count;
                    NSession.Update(list[0]);
                    NSession.Flush();
                    return no;
                }
                return 0;
            }
        }
        /// <summary>
        /// 创建SKU Code
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="count"></param>
        /// <param name="planNo"></param>
        /// <param name="sid"></param>
        /// <param name="NSession"></param>
        /// <returns></returns>
        public static int CreateSKUCode(string sku, int count, string planNo, string sid, ISession NSession)
        {
            //负数入库 Code会回退
            if (count > 0)
            {
                int code = GetSKUCode(count, NSession);
                string create = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                using (var tr = NSession.BeginTransaction())
                {
                    for (int i = code; i < code + count; i++)
                    {
                        SKUCodeType SKUCode = new SKUCodeType();
                        SKUCode.Code = i;
                        SKUCode.SKU = sku;
                        SKUCode.IsOut = 0;
                        SKUCode.IsNew = 1;
                        SKUCode.IsSend = 0;
                        SKUCode.IsScan = 0;
                        SKUCode.CreateOn = create;
                        SKUCode.PlanNo = planNo;
                        SKUCode.SId = sid;
                        SKUCode.SendOn = "";
                        SKUCode.PeiOn = "";
                        NSession.Save(SKUCode);
                    }
                    tr.Commit();
                }
                return code;
            }
            return 0;
        }

        public static void test()
        {
            ISession NSession = NhbHelper.OpenSession();

            //string orderno = Utilities.GetOrderNo(NSession);
            ////IList<OrderProductType> list = NSession.CreateQuery("from OrderProductType where OrderNo='1267157'").List<OrderProductType>();
            ////OrderHelper.CreateOrderPruduct(list[0], NSession);

            //IList<OrderType> list = NSession.CreateQuery("from OrderType where OrderNo='1334691'").List<OrderType>();
            //OrderHelper.ReckonFinance(list[0], NSession);
            ////var obj = Convert.ToDouble(OrderHelper.GetFreight((double)list[0].Weight, list[0].LogisticMode, list[0].Country, NSession, 0M));
            //NSession.Flush();
            //NSession.Close();

            //List<SKUCodeType> codelist2 = NSession.CreateQuery("from SKUCodeType where OrderNo='1461289'").List<SKUCodeType>().OrderBy(x => x.Code).ToList();
            //SKUCodeType code = codelist2.Find(p => p.OrderNo == "1461289"); // 返回多条还是单条有待测试


            // 计算财务数据
            //IList<OrderType> list = NSession.CreateQuery("from OrderType where OrderNo='1714006'").List<OrderType>();
            //OrderHelper.ReckonFinance(list[0], NSession);

            //// 缺货订单匹配
            //string SKU = "SF69900CS3";
            //IList<OrderType> list2 = NSession.CreateQuery(" from OrderType where Id in(select OId from OrderProductType where SKU ='" + SKU + "' ) and  Status in ('已处理') and Enabled=1 Order By CreateOn Asc ").List<OrderType>();
            ////NSession.CreateSQLQuery(" update OrderProducts set IsQue=0 where  SKU='" + SKU + "'").UniqueResult();
            //foreach (OrderType type in list2)
            //{
            //    OrderHelper.SetQueOrder(type, NSession);
            //}

            //// 批量重新计算已处理订单缺货状态
            //List<OrderType> objList = NSession.CreateSQLQuery("select * from Orders o join Account a on o.Account=a.AccountName join OrderProducts p on o.OrderNo=p.OrderNo where O.Status='已处理' and IsOutOfStock=0 and DATEPART(year,CreateOn)=2016 and Enabled=1 and a.FromArea='宁波'").AddEntity(typeof(OrderType)).List<OrderType>().ToList();
            ////List<OrderType> objList = NSession.CreateSQLQuery("select * from Orders o join Account a on o.Account=a.AccountName join OrderProducts p on o.OrderNo=p.OrderNo where O.Status='已处理' and DATEPART(year,CreateOn)=2016 and Enabled=1 and a.FromArea='宁波' and O.OrderNo='1841960'").AddEntity(typeof(OrderType)).List<OrderType>().ToList();
            //foreach (OrderType type in objList)
            //{
            //    OrderHelper.SetQueOrderAuto(type, NSession);
            //}

            int nI = 0;
        }

        public static void UpdateCurrency()
        {
            ISession NSession = NhbHelper.OpenSession();
            //cn.com.webxml.webservice.ForexRmbRateWebService server = new cn.com.webxml.webservice.ForexRmbRateWebService();
            //DataSet ds = server.getForexRmbRate();

            string hl = null;
            string tempurl = "http://www.boc.cn/sourcedb/whpj/index.html";
            Dictionary<string, string> div = new Dictionary<string, string>();
            div.Add("英镑", "GBP");
            div.Add("港币", "HKD");

            div.Add("美元", "USD");
            div.Add("瑞士法郎", "CHF");
            div.Add("新加坡元", "SGD");
            div.Add("瑞典克朗", "SEK");
            div.Add("丹麦克朗", "DKK");
            div.Add("挪威克朗", "NOK");
            div.Add("日元", "JPY");
            div.Add("加拿大元", "CAD");
            div.Add("澳大利亚元", "AUD");
            div.Add("欧元", "EUR");
            div.Add("澳门元", "MOP");
            div.Add("菲律宾比索", "PHP");
            div.Add("泰国铢", "THB");
            div.Add("新西兰元", "NZD");
            div.Add("韩国元", "KRW");
            div.Add("林吉特", "MYR");
            div.Add("印尼卢比", "IDR");

            HttpWebRequest webr = (HttpWebRequest)WebRequest.Create(tempurl);//创建请求
            HttpWebResponse wb = (HttpWebResponse)webr.GetResponse();
            Stream sr = wb.GetResponseStream();//得到返回数据流
            StreamReader sr1 = new StreamReader(sr, Encoding.GetEncoding("utf-8"));//用于读取数据流的内容
            string zz = sr1.ReadToEnd();//读取完成
            sr1.Close();
            wb.Close();//关闭

            Regex expr = new Regex(
                        @"<tr>\s*" +
                        @"<td>(?<name>.+)</td>\s*" +
                        @"<td>(?<fBuyPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<mBuyPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<SellPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<BasePrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<ZhongPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<date>.+)</td>\s*" +
                        @"<td>(?<time>.+)</td>\s*" +
                        @"</tr>", RegexOptions.Compiled);

            for (Match m = expr.Match(zz); m.Success; m = m.NextMatch())
            {
                string key;

                CurrencyType c = new CurrencyType();

                c.CurrencyName = m.Groups["name"].ToString();
                if (div.ContainsKey(c.CurrencyName))
                {
                    c.CurrencySign = div[c.CurrencyName];
                    c.CurrencyCode = div[c.CurrencyName];
                    //key = m.Groups["fBuyPrice"].ToString();
                    key = m.Groups["ZhongPrice"].ToString();
                    c.CurrencyValue = key.Length > 0 ? Convert.ToDecimal(key) / 100 : 0;
                    c.CreateOn = DateTime.Now;
                    NSession.Delete(" from CurrencyType where  CurrencySign='" + c.CurrencySign + "'");

                    NSession.Save(c);
                    NSession.Flush();
                }
            }

            NSession.CreateSQLQuery(
                "update AliActivity set Status='已结束',SortCode=3 where Status='活动中' and EndDate<'" + DateTime.Now.AddDays(-2) + "'").ExecuteUpdate();
            NSession.Flush();
            NSession.Close();
        }

        /// <summary>
        /// 库损计算
        /// </summary>
        public static void AutoGoodsDiscount()
        {
            ISession NSession = NhbHelper.OpenSession();
            //ISession NSession = NhbHelper.GetCurrentSession();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_AutoGoodsDiscount";

            //SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            //da.Fill(ds);
            command.ExecuteNonQuery();
            command.Dispose();
            NSession.Flush();
            NSession.Close();
        }

        /// <summary>
        /// 同步固定汇率
        /// </summary>
        public static void SyncFixedRate()
        {
            ISession NSession = NhbHelper.OpenSession();

            string hl = null;
            string tempurl = "http://www.boc.cn/sourcedb/whpj/index.html";
            Dictionary<string, string> div = new Dictionary<string, string>();
            div.Add("英镑", "GBP");
            div.Add("港币", "HKD");

            div.Add("美元", "USD");
            div.Add("瑞士法郎", "CHF");
            div.Add("新加坡元", "SGD");
            div.Add("瑞典克朗", "SEK");
            div.Add("丹麦克朗", "DKK");
            div.Add("挪威克朗", "NOK");
            div.Add("日元", "JPY");
            div.Add("加拿大元", "CAD");
            div.Add("澳大利亚元", "AUD");
            div.Add("欧元", "EUR");
            div.Add("澳门元", "MOP");
            div.Add("菲律宾比索", "PHP");
            div.Add("泰国铢", "THB");
            div.Add("新西兰元", "NZD");
            div.Add("韩国元", "KRW");
            div.Add("林吉特", "MYR");
            div.Add("印尼卢比", "IDR");

            HttpWebRequest webr = (HttpWebRequest)WebRequest.Create(tempurl);//创建请求
            HttpWebResponse wb = (HttpWebResponse)webr.GetResponse();
            Stream sr = wb.GetResponseStream();//得到返回数据流
            StreamReader sr1 = new StreamReader(sr, Encoding.GetEncoding("utf-8"));//用于读取数据流的内容
            string zz = sr1.ReadToEnd();//读取完成
            sr1.Close();
            wb.Close();//关闭

            Regex expr = new Regex(
                        @"<tr>\s*" +
                        @"<td>(?<name>.+)</td>\s*" +
                        @"<td>(?<fBuyPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<mBuyPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<SellPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<BasePrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<ZhongPrice>[0-9]*.?[0-9]*)</td>\s*" +
                        @"<td>(?<date>.+)</td>\s*" +
                        @"<td>(?<time>.+)</td>\s*" +
                        @"</tr>", RegexOptions.Compiled);

            for (Match m = expr.Match(zz); m.Success; m = m.NextMatch())
            {
                string key;

                FixedRateType c = new FixedRateType();

                c.CurrencyName = m.Groups["name"].ToString();
                if (div.ContainsKey(c.CurrencyName))
                {
                    c.CurrencyCode = div[c.CurrencyName];
                    c.Year = DateTime.Now.Year;
                    c.Month = DateTime.Now.Month;
                    List<FixedRateType> rateResult = NSession.CreateQuery("from FixedRateType where CurrencyCode='" + c.CurrencyCode + "' and Year='" + c.Year + "' and Month='" + c.Month + "'").List<FixedRateType>().ToList();
                    if (rateResult.Count == 0)
                    {
                        key = m.Groups["ZhongPrice"].ToString();
                        c.CurrencyValue = key.Length > 0 ? Convert.ToDecimal(key) / 100 : 0;
                        c.CreateOn = DateTime.Now;

                        NSession.Save(c);
                        NSession.Flush();
                    }
                }
            }

            NSession.Flush();
            NSession.Close();
        }

        public static DateTime GetAliDate(string DateStr)
        {
            return new DateTime(Convert.ToInt32(DateStr.Substring(0, 4)), Convert.ToInt32(DateStr.Substring(4, 2)), Convert.ToInt32(DateStr.Substring(6, 2)), Convert.ToInt32(DateStr.Substring(8, 2)), Convert.ToInt32(DateStr.Substring(10, 2)), Convert.ToInt32(DateStr.Substring(12, 2)));
        }

        public static object GetUnPeiQty(string sku, ISession NSession)
        {
            // object obj = NSession.CreateQuery("select COUNT(Id) from SKUCodeType where SKU ='" + sku + "' and IsOut=0 group by SKU ").UniqueResult();

            object obj = NSession.CreateQuery("select sum(Qty) from WarehouseStockType where SKU ='" + sku + "' ").UniqueResult();
            return obj == null ? "0" : obj;
        }

        public static string GetLastCai(string sku, ISession NSession)
        {
            //IList<PurchasePlanType> list = NSession.CreateQuery("from PurchasePlanType where SKU ='" + sku + "' and Status not in('异常','已收到')  Order By ExpectReceiveOn desc").SetMaxResults(1).List<PurchasePlanType>();
            // 累加多个采购数量
            IList<PurchasePlanType> list = NSession.CreateQuery("from PurchasePlanType where SKU ='" + sku + "' and Status not in('异常','已收到','失效','已退款')  Order By ExpectReceiveOn desc").List<PurchasePlanType>();
            if (list.Count > 0)
            {
                //return list[0].ExpectReceiveOn.ToString("yyyy-MM-dd") + " 预计到货 " + list[0].Qty;
                return list[0].ExpectReceiveOn.ToString("yyyy-MM-dd") + " 预计到货 " + list.Sum(x => x.Qty);
            }
            else
            {
                return "近期没有物品到货";
            }
        }

        public static DataSet GetDataSet(string sql, ISession NSession, string orderBy = " Order By Id Asc")
        {
            DataSet dataSet = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql + orderBy;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return dataSet;
        }

        public static DataSet GetDataSet1(string sql, ISession NSession)
        {
            DataSet dataSet = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandText = sql;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return dataSet;
        }


        #region 缩略图生成
        public static void DrawImageRectRect(Image imageFrom, string newImgPath, int width, int height)
        {

            // 源图宽度及高度 
            int imageFromWidth = imageFrom.Width;
            int imageFromHeight = imageFrom.Height;
            //在原画布中的位置
            int X, Y;
            //在原画布中取得的长宽
            int bitmapWidth, bitmapHeight;
            //// 根据源图及欲生成的缩略图尺寸,计算缩略图的实际尺寸及其在"画布"上的位置 
            if (imageFromWidth / width > imageFromHeight / height)
            {
                bitmapWidth = (width * imageFromHeight) / height;
                bitmapHeight = imageFromHeight;
                X = (imageFromWidth - bitmapWidth) / 2;
                Y = 0;
            }
            else
            {
                bitmapWidth = imageFromWidth;
                bitmapHeight = (height * imageFromWidth) / width;
                X = 0;
                Y = (imageFromHeight - bitmapHeight) / 2;

            }
            // 创建画布 
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            // 用白色清空 
            g.Clear(Color.White);
            // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // 指定高质量、低速度呈现。 
            g.SmoothingMode = SmoothingMode.HighQuality;
            // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
            g.DrawImage(imageFrom, new Rectangle(0, 0, width, height), new Rectangle(X, Y, bitmapWidth, bitmapHeight), GraphicsUnit.Pixel);
            try
            {
                //经测试 .jpg 格式缩略图大小与质量等最优 
                bmp.Save(newImgPath, ImageFormat.Jpeg);
            }
            catch
            {
            }
            finally
            {
                //显示释放资源 
                imageFrom.Dispose();
                bmp.Dispose();
                g.Dispose();
            }
        }

        public static void DrawImageRectRect(string rawImgPath, string newImgPath, int width, int height)
        {
            try
            {
                System.Drawing.Image imageFrom = System.Drawing.Image.FromFile(rawImgPath);
                DrawImageRectRect(imageFrom, newImgPath, width, height);
            }
            catch (Exception)
            {

            }

        }
        #endregion

        /// <summary>
        /// 将String转换为Dictionary类型，过滤掉为空的值，首先 6 分割，再 7 分割
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<KeyValue> StringToDictionary(string value)
        {

            List<KeyValue> queryDictionary = new List<KeyValue>();
            string[] s = value.Split('^');
            for (int i = 0; i < s.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(s[i]) && !s[i].Contains("undefined"))
                {
                    var ss = s[i].Split('&');
                    if (ss.Length == 2)
                    {
                        if ((!string.IsNullOrEmpty(ss[0])) && (!string.IsNullOrEmpty(ss[1])))
                        {
                            KeyValue kv = new KeyValue();
                            kv.Key = ss[0];
                            kv.Value = ss[1];
                            queryDictionary.Add(kv);
                        }
                    }
                }
            }
            return queryDictionary;
        }

        public static string XmlSerialize<T>(T obj)
        {
            string xmlString = string.Empty;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, obj);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }
        public static string Resolve(string search, bool iso = true)
        {
            string where = string.Empty;
            int flagWhere = 0;

            List<KeyValue> queryDic = StringToDictionary(search);
            if (queryDic != null && queryDic.Count > 0)
            {
                foreach (var item in queryDic)
                {
                    if (flagWhere != 0)
                    {
                        where += " and ";
                    }
                    flagWhere++;
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("SKU_OrderProduct")) //需要查询的列名
                    {
                        where += " Id in (select OId from OrderProductType where SKU like '%" + item.Value + "%')";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("Addressee_OrderAddress")) //需要查询的列名
                    {
                        where += " AddressId in (select Id from OrderAddressType where Addressee like '%" + item.Value + "%')";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("ProductAttribute_Product")) //需要查询的列名
                    {
                        where += " Id in (select OId from OrderProductType where SKU in (select SKU from ProductType where ProductAttribute='" + item.Value + "' ))";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(Start_Time)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(Start_Time)) + " >= '" + item.Value + "'";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(End_Time)) //需要查询的列名
                    {
                        DateTime date = Convert.ToDateTime(item.Value);
                        if (date.Hour == 0 && date.Minute == 0 && iso)
                            where += item.Key.Remove(item.Key.IndexOf(End_Time)) + " <=  '" + date.ToString("yyyy-MM-dd 23:59:59") + "'";
                        else
                            where += item.Key.Remove(item.Key.IndexOf(End_Time)) + " <=  '" + item.Value + "'";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(Start_Int)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(Start_Int)) + " >= " + item.Value;
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(End_Int)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(End_Int)) + " <= " + item.Value;
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(End_String)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(End_String)) + " = '" + item.Value + "'";
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(DDL_String)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(DDL_String)) + " = '" + item.Value + "'";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(DDL_UnString)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(DDL_UnString)) + " <> '" + item.Value + "'";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(NotLike)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(NotLike)) + " not like '%" + item.Value + "%'";
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(ISNULL)) //需要查询的列名
                    {
                        where += item.Key.Remove(item.Key.IndexOf(ISNULL)) + " is null";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains(ISALL)) //多选
                    {
                        where += item.Key.Remove(item.Key.IndexOf(ISALL)) + "  in ('" + item.Value.Replace(",", "','") + "')";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("OrderPdtType")) //多选
                    {
                        //where += item.Key.Remove(item.Key.IndexOf(ISALL)) + "  in ('" + item.Value.Replace(",", "','") + "')";
                        //continue;
                        if (item.Value == "单品单件")
                        {
                            where += " Id in (select OId from OrderProductType where qty=1 group by OId having count(OId)=1)";
                        }
                        if (item.Value == "单品多件")
                        {
                            where += " Id in (select OId from OrderProductType where qty>1 group by OId having count(OId)=1)";
                        }
                        if (item.Value == "多品多件")
                        {
                            where += " Id in (select OId from OrderProductType group by OId having count(OId)>1)";
                        }
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("warehouse"))
                    {
                        if (item.Value == "宁波仓库")
                        {
                            where += " Account in (select accountname from Account where FromArea='宁波')";
                        }
                        else if (item.Value == "义乌仓库")
                        {
                            where += " Account in (select accountname from Account where FromArea='义乌')";
                        }

                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("logistics"))
                    {
                        where += " [LogisticMode] in ('" + item.Value.Replace(",", "','") + "')";
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("_Users"))
                    {
                        where += " WS.SKU   in  (select SKU from Products where CreateBy in (select Realname from Users where HomeAddress like '%" + item.Value + "%'))"; ;
                        continue;
                    }
                    //海外仓出货明细清单页面，是否生成清单下拉框选择
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("IsList"))
                    {
                        if (item.Value == "1")
                        {
                            where += "ShipmentslistId in (select id from ShipmentslistType where IsExa in ('" + Shipmentapproval.审核通过 + "','" + Shipmentapproval.审核中 + "','" + Shipmentapproval.确认通过I + "','" + Shipmentapproval.确认通过II + "'))";
                            continue;
                        }
                        else if (item.Value == "否" || item.Value == "0")
                        {
                            where += "ShipmentslistId in (select id from ShipmentslistType where IsExa in ('" + Shipmentapproval.审核拒绝 + "','" + Shipmentapproval.确认拒绝I + "','" + Shipmentapproval.确认拒绝I + "')) or ShipmentslistId=0 ";
                            continue;
                        }
                        else
                        {
                            where += "1=1 ";
                            continue;
                        }

                    }
                    //海外仓审核页面，审核状态选择
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value) && item.Key.Contains("txtIsExa"))
                    {
                        if (item.Value == "ALL")
                        {
                            continue;
                        }
                        else
                        {
                            where += " IsExa='" + item.Value + "'";
                        }
                        continue;
                    }
                    where += item.Key + " like '%" + item.Value + "%'";
                }
            }
            return where;
        }

        public static string GetObjEditString(object o1, object o2)
        {
            StringBuilder sb = new StringBuilder();
            System.Reflection.PropertyInfo[] properties = o1.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                if (name.StartsWith("rows"))
                {
                    continue;
                }
                object value = item.GetValue(o1, null);
                object value2 = item.GetValue(o2, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    if (value == null)
                        value = "";
                    if (value2 == null)
                        value2 = "";
                    if (value.ToString() != value2.ToString())
                    {
                        sb.Append(" " + name + "从“" + value + "” 修改为 “" + value2 + "”<br>");
                    }

                }
            }
            return sb.ToString();
        }


        #region 登陆和Cookie

        /// <summary>
        /// Create or Set Cookies Values
        /// </summary>
        /// <param name="Obj">[0]:Name,[1]:Value</param>
        public static void CreateCookies(string u, string p, int t)
        {
            try
            {
                HttpCookie cookie = new HttpCookie(Utilities.CookieName)
                {
                    Expires = DateTime.Now.AddDays(t),
                };
                cookie.Value = u + "&" + p;
                System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Get Cookies Values
        /// </summary>
        /// <param name="name">Cookies的Name</param>
        /// <returns></returns>
        public static string GetCookies()
        {
            try
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[Utilities.CookieName];
                return cookie.Value;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// Clear Cookies Values
        /// </summary>
        /// <param name="name">Cookies的Name</param>
        public static void ClearCookies()
        {
            HttpCookie cookie = new HttpCookie(Utilities.CookieName)
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
            System.Web.HttpContext.Current.Session["account"] = null;
        }



        public static bool LoginByUser(string p, string u, ISession NSession)
        {

            IList<UserType> list = NSession.CreateQuery(" from  UserType where Username=:p1 and Password=:p2 and DeletionStateCode=0").SetString("p1", p).SetString("p2", u).List<UserType>();
            if (list.Count > 0)
            {   //登录成功
                UserType user = list[0];
                user.LastVisit = DateTime.Now;
                NSession.Update(user);
                NSession.Flush();
                GetPM(user, NSession);
                System.Web.HttpContext.Current.Session["account"] = user;
                return true;
            }
            return false;
        }

        public static void GetPM(UserType currentUser, ISession NSession)
        {
            List<PermissionScopeType> listByModules = new List<PermissionScopeType>();
            List<PermissionScopeType> listByPermissions = new List<PermissionScopeType>();
            List<PermissionScopeType> listByAccount = new List<PermissionScopeType>();

            foreach (var item in GetUserScope(currentUser.Id, NSession))
            {
                GetValue(item, listByModules, listByPermissions, listByAccount);

            }
            foreach (var item in GetRoleScope(currentUser.RoleId, NSession))
            {
                GetValue(item, listByModules, listByPermissions, listByAccount);
            }
            foreach (var item in GetDepartmentScope(currentUser.Id, NSession))
            {
                GetValue(item, listByModules, listByPermissions, listByAccount);
            }
            string mids = "";
            string pids = "";
            string aids = "";

            foreach (var item in listByModules)
            {
                mids += item.TargetId + ",";
            }
            foreach (var item in listByPermissions)
            {
                pids += item.TargetId + ",";
            }
            foreach (var item in listByAccount)
            {
                aids += item.TargetId + ",";
            }
            mids = mids.Trim(',');
            pids = pids.Trim(',');
            aids = aids.Trim(',');
            if (mids.Length == 0)
                mids = "''";
            if (pids.Length == 0)
                pids = "''";
            if (aids.Length == 0)
                aids = "''";
            List<ModuleType> Modules = NSession.CreateQuery("from ModuleType where Id in(" + mids + ")").List<ModuleType>().ToList<ModuleType>();
            List<PermissionItemType> Permissions = NSession.CreateQuery("from PermissionItemType where Id in(" + pids + ")").List<PermissionItemType>().ToList<PermissionItemType>();

            List<AccountType> Accounts = NSession.CreateQuery("from AccountType where Id in(" + aids + ")").List<AccountType>().ToList<AccountType>();
            currentUser.Modules = Modules;
            currentUser.Permissions = Permissions;
            currentUser.Accounts = Accounts;
            System.Web.HttpContext.Current.Session["account"] = currentUser;
        }

        /// <summary>
        /// 实体类转换成DataTable
        /// </summary>
        /// <param name="modelList">实体类列表</param>
        /// <returns></returns>
        public static DataTable FillDataTable<T>(IList<T> modelList)
        {
            if (modelList == null || modelList.Count == 0)
            {
                return new DataTable();
            }
            DataTable dt = CreateData<T>(modelList[0]);

            foreach (T model in modelList)
            {
                DataRow dataRow = dt.NewRow();
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null) == null ? DBNull.Value : propertyInfo.GetValue(model, null);
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 根据实体类得到表结构
        /// </summary>
        /// <param name="model">实体类</param>
        /// <returns></returns>
        private static DataTable CreateData<T>(T model)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                Type colType = propertyInfo.PropertyType;
                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    colType = colType.GetGenericArguments()[0];
                }
                dataTable.Columns.Add(new DataColumn(propertyInfo.Name, colType));
            }
            return dataTable;
        }

        #endregion

        private static void GetValue(PermissionScopeType item, List<PermissionScopeType> listByModules, List<PermissionScopeType> listByPermissions, List<PermissionScopeType> listByAccount)
        {
            if (item.TargetCategory == TargetCategoryEnum.Module.ToString())
            {
                listByModules.Add(item);
            }
            else if (item.TargetCategory == TargetCategoryEnum.PermissionItem.ToString())
            {
                listByPermissions.Add(item);
            }
            else
            {
                listByAccount.Add(item);
            }
        }

        public static List<PermissionScopeType> GetUserScope(int id, ISession NSession)
        {
            List<PermissionScopeType> list = NSession.CreateQuery("from PermissionScopeType where ResourceCategory=:p1 and ResourceId=:p2").SetString("p1", ResourceCategoryEnum.User.ToString()).SetInt32("p2", id).List<PermissionScopeType>().ToList<PermissionScopeType>();
            return list;
        }

        public static List<PermissionScopeType> GetRoleScope(int id, ISession NSession)
        {
            List<PermissionScopeType> list = NSession.CreateQuery("from PermissionScopeType where ResourceCategory=:p1 and ResourceId=:p2").SetString("p1", ResourceCategoryEnum.Role.ToString()).SetInt32("p2", id).List<PermissionScopeType>().ToList<PermissionScopeType>();
            return list;
        }

        public static List<PermissionScopeType> GetDepartmentScope(int id, ISession NSession)
        {
            List<PermissionScopeType> list = NSession.CreateQuery("from PermissionScopeType where ResourceCategory=:p1 and ResourceId=:p2").SetString("p1", ResourceCategoryEnum.User.ToString()).SetInt32("p2", id).List<PermissionScopeType>().ToList<PermissionScopeType>();
            return list;
        }

        /// <summary>
        /// 设置组合商品主SKU库存数量
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="wid"></param>
        /// <param name="UserName"></param>
        /// <param name="NSession"></param>
        public static void SetComposeStock(string sku, int wid, string UserName, ISession NSession)
        {
            //////这个产品有几个产品组成
            //// 查询SKU是否为被组合商品(bug：重复组合有多个主SKU情况)
            //List<ProductComposeType> products = NSession.CreateQuery(" from ProductComposeType where SKU in (select SKU from ProductComposeType where SrcSKU='" + sku + "')").List<ProductComposeType>().ToList();
            //string skulist = "";
            //// 非组合商品跳出
            //if (products.Count == 0)
            //{
            //    return;

            //}
            //// 获取组合商品全部被组合SKU(bug：重复组合产品主SKU因多个原因，会出现多个主SKU的被组合商品SKU被获取情况)
            //foreach (ProductComposeType productComposeType in products)
            //{
            //    skulist += productComposeType.SrcSKU + ",";
            //}

            //skulist = skulist.Trim(',').Replace(",", "','");
            //// 获取组合SKU内全部被组合SKU库存表主库存数量列表
            //IList<WarehouseStockType> stocklist = NSession.CreateQuery("from WarehouseStockType where WId=" + wid + " and SKU in ('" + skulist + "')").List<WarehouseStockType>();

            //int min = 0;
            //// 根据被组合商品库存数量比例计算主SKU数量
            //foreach (WarehouseStockType warehouseStockType in stocklist)
            //{
            //    // 获取有库存信息且被组合商品SKU匹配的商品
            //    ProductComposeType composeType = products.Find(p => p.SrcSKU.Trim().ToUpper() == warehouseStockType.SKU.ToUpper());

            //    // 通过组合商品表公式计算对应主SKU比例数量
            //    int j = warehouseStockType.Qty / composeType.SrcQty;
            //    if (min == 0 || j < min)
            //    {
            //        min = j;
            //    }
            //}

            //// 获取组合商品主SKU库存(bug：重复组合有多个主SKU情况，此时products[0].SKU只取其中一个)
            //IList<WarehouseStockType> list = NSession.CreateQuery("from WarehouseStockType where WId=" + wid + " and SKU ='" + products[0].SKU + "'").List<WarehouseStockType>();
            //if (list.Count > 0)
            //{
            //    // 设置组合商品主SKU按比例数量
            //    //list[0].Qty = min;
            //    list[0].Qty = 0; //主SKU的库存必须是零。 
            //    list[0].UpdateBy = UserName;
            //    list[0].UpdateOn = DateTime.Now;
            //    NSession.Update(list[0]);
            //    NSession.Flush();
            //}
            //else
            //{
            //    IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU ='" + products[0].SKU + "'").List<ProductType>();
            //    if (productTypes.Count > 0)
            //    {
            //        WarehouseType w = NSession.Get<WarehouseType>(wid);
            //        AddToWarehouse(productTypes[0], UserName, NSession, w.WName, min);
            //    }
            //}

        }
        // 增加库存
        public static void AddToWarehouse(ProductType obj, string UserName, ISession NSession, int WId, int Qty = 0)
        {

            //IList<WarehouseType> list = NSession.CreateQuery(" from WarehouseType " + (WName != null ? " where WName='" + WName + "'" : "")).List<WarehouseType>();
            IList<WarehouseType> list = NSession.CreateQuery(" from WarehouseType where Id=" + WId.ToString()).List<WarehouseType>();
            //
            //在仓库中添加产品库存
            //
            foreach (var item in list)
            {
                WarehouseStockType stock = new WarehouseStockType();
                stock.Pic = obj.SPicUrl;
                stock.WId = item.Id;
                stock.Warehouse = item.WName;
                stock.PId = obj.Id;
                stock.SKU = obj.SKU;
                stock.Title = obj.ProductName;
                stock.Qty = Qty;
                stock.UpdateBy = UserName;
                stock.UpdateOn = DateTime.Now;
                NSession.SaveOrUpdate(stock);
                NSession.Flush();

                //if (stock.Qty < 0)
                //{
                //    Comm.LogInfo.WriteLog(String.Format("AddToWarehouse835 用户:{0},时间:{1},IP:{2},SKU:{3},Qty:{4}", UserName, DateTime.Now, GetClientIP(), obj.SKU, obj.PicQty));
                //}
            }
        }

        public static string GetClientIP()
        {
            string result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (null == result || result == String.Empty)
            {
                result = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            return result;
        }

        public static bool StockOut(StockOutType stockOutType, ISession NSession)
        {

            IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", stockOutType.WId).SetString("p2", stockOutType.SKU).List<WarehouseStockType>();
            if (list.Count > 0)
            {
                WarehouseStockType ws = list[0];

                if (ws.Qty < stockOutType.Qty)
                {
                    return false;
                }
                ws.Qty = ws.Qty - stockOutType.Qty;
                ws.UpdateBy = stockOutType.CreateBy;
                ws.UpdateOn = DateTime.Now;
                NSession.SaveOrUpdate(ws);
                NSession.Flush();
                //if (ws.Qty < 0)
                //{
                //    Comm.LogInfo.WriteLog(String.Format("StockOut862 用户:{0},时间:{1},IP:{2},SKU:{3},Qty:{4}", stockOutType.CreateBy, DateTime.Now, GetClientIP(), ws.SKU, ws.Qty));
                //}
                stockOutType.IsAudit = 1;
                stockOutType.SourceQty = ws.Qty + stockOutType.Qty;
                NSession.Update(stockOutType);
                NSession.Flush();
                using (ITransaction tx = NSession.BeginTransaction())
                {
                    try
                    {
                        List<WarehouseStockDataType> stockInDataTypes = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockOutType.WId + " and SKU='" + stockOutType.SKU + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();
                        int fooQty = stockOutType.Qty;
                        foreach (var stockInDataType in stockInDataTypes)
                        {
                            if (stockInDataType.NowQty <= fooQty)//需要匹配多个现有批次
                            {
                                fooQty = fooQty - stockInDataType.NowQty;
                                //stockOutType.Qty = stockInDataType.NowQty;
                                //stockOutType.InNo = stockInDataType.InNo;
                                stockInDataType.NowQty = 0;
                                //stockOutType.Id = 0;
                                NSession.Clear();
                                NSession.Update(stockInDataType);
                                //NSession.Update(stockOutType);
                                NSession.Flush();
                            }
                            else
                            {//一个批次就能满足现有的需求
                                stockInDataType.NowQty = stockInDataType.NowQty - fooQty;
                                //stockOutType.InNo = stockInDataType.InNo;
                                //stockOutType.Qty = fooQty;
                                fooQty = 0;
                                //stockOutType.Id = 0;
                                NSession.Clear();
                                NSession.Update(stockInDataType);
                                //NSession.Update(stockOutType);
                                NSession.Flush();
                            }
                            if (fooQty == 0) break;

                        }
                        tx.Commit();
                    }
                    catch (HibernateException)
                    {
                        tx.Rollback();

                    }
                }
                return true;
            }
            else
            {
                IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + stockOutType.SKU + "'").List<ProductType>();
                if (productTypes.Count > 0)
                {
                    WarehouseType warehouse = NSession.Get<WarehouseType>(stockOutType.WId);
                    AddToWarehouse(productTypes[0], stockOutType.CreateBy, NSession, warehouse.Id, 0);
                    StockOut(stockOutType, NSession);
                }

            }
            return false;
        }


        public static int StockOut(int wid, string sku, int num, string outType, string user, string memo, string orderNo, ISession NSession)
        {

            IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", wid).SetString("p2", sku).List<WarehouseStockType>();
            if (list.Count > 0)
            {
                WarehouseStockType ws = list[0];

                // KS海外仓允许扣负数库存
                //if (ws.Qty < num && outType != "海外仓发货" && outType != "LAI出库")//LAI仓库验单时减库存
                //{
                //    return false;
                //}
                // LAI出库，FBA出库，FBA-NB01出库，FBA-NB02出库，FBA-NB03出库
                ws.Qty = ws.Qty - num;
                ws.UpdateBy = user;
                ws.UpdateOn = DateTime.Now;
                NSession.SaveOrUpdate(ws);
                NSession.Flush();

                //if (ws.Qty < 0)
                //{
                //    Comm.LogInfo.WriteLog(String.Format("StockOut947 用户:{0},时间:{1},IP:{2},SKU:{3},Qty:{4}", user, DateTime.Now, GetClientIP(), ws.SKU, ws.Qty));
                //}
                SetComposeStock(sku, wid, user, NSession);
                StockOutType stockOutType = new StockOutType();
                stockOutType.WName = ws.Warehouse;
                stockOutType.CreateBy = user;
                stockOutType.CreateOn = DateTime.Now;
                stockOutType.IsAudit = 1;
                stockOutType.OrderNo = orderNo;
                stockOutType.Qty = num;
                stockOutType.SKU = sku;
                stockOutType.OutType = outType;
                stockOutType.SourceQty = ws.Qty + num;
                stockOutType.WId = wid;
                stockOutType.Memo = memo;

                using (ITransaction tx = NSession.BeginTransaction())
                {
                    try
                    {
                        // 因skucode无法与入库单直接找到关联方式暂时取消
                        //// 根据出库商品对应入库编辑找到对应记录库存明细批次并扣数量
                        //List<WarehouseStockDataType> WarehouseStockDataList = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockOutType.WId + " and SKU='" + stockOutType.SKU + "' and InNo=" + stockOutType.InNo).List<WarehouseStockDataType>().ToList();

                        //if (WarehouseStockDataList.Count > 0)
                        //{
                        //    WarehouseStockDataList[0].NowQty = WarehouseStockDataList[0].NowQty - stockOutType.Qty;
                        //    stockOutType.InNo = WarehouseStockDataList[0].InNo;
                        //    stockOutType.Qty = stockOutType.Qty;
                        //    stockOutType.Price = WarehouseStockDataList[0].Amount;
                        //    stockOutType.Id = 0;
                        //    NSession.Clear();
                        //    NSession.Update(WarehouseStockDataList[0]);
                        //    NSession.Save(stockOutType);
                        //    NSession.Flush();
                        //}

                        // 先入先出扣数量
                        List<WarehouseStockDataType> stockInDataTypes = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockOutType.WId + " and SKU='" + stockOutType.SKU + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();
                        int fooQty = stockOutType.Qty;
                        foreach (var stockInDataType in stockInDataTypes)
                        {
                            if (stockInDataType.NowQty <= fooQty)//需要匹配多个现有批次
                            {
                                fooQty = fooQty - stockInDataType.NowQty;
                                stockOutType.Qty = stockInDataType.NowQty;
                                stockOutType.InNo = stockInDataType.InNo;
                                stockOutType.Price = stockInDataType.Amount;
                                stockInDataType.NowQty = 0;
                                stockOutType.Id = 0;
                                NSession.Clear();
                                NSession.Update(stockInDataType);
                                NSession.Save(stockOutType);
                                NSession.Flush();
                            }
                            else
                            {
                                //一个批次就能满足现有的需求
                                stockInDataType.NowQty = stockInDataType.NowQty - fooQty;
                                stockOutType.InNo = stockInDataType.InNo;
                                stockOutType.Qty = fooQty;
                                stockOutType.Price = stockInDataType.Amount;
                                fooQty = 0;
                                stockOutType.Id = 0;
                                NSession.Clear();
                                NSession.Update(stockInDataType);
                                NSession.Save(stockOutType);
                                NSession.Flush();
                            }
                            if (fooQty == 0) break;
                        }
                        // 特殊情况原库存为零同时为负数数量直接冲红
                        if (fooQty < 0 && stockInDataTypes.Count() == 0)
                        {
                            // 获取原订单出库记录
                            IList<StockOutType> stockout = NSession.CreateQuery("from StockOutType where OrderNo='" + orderNo + "'").List<StockOutType>().ToList<StockOutType>();

                            //一个批次就能满足现有的需求
                            if (stockout.Count() > 0)
                            {
                                WarehouseStockDataType stockInDataType = new WarehouseStockDataType();
                                stockInDataType.NowQty = Math.Abs(fooQty);
                                stockInDataType.Amount = stockout[0].Price;
                                stockInDataType.InNo = stockout[0].InNo;
                                stockInDataType.InId = Convert.ToInt32(stockout[0].InNo);
                                stockInDataType.PId = ws.PId;
                                stockInDataType.SKU = stockout[0].SKU;
                                stockInDataType.PName = ws.Title;
                                stockInDataType.WId = wid;
                                stockInDataType.WName = ws.Warehouse;
                                stockInDataType.CreateOn = DateTime.Now;
                                stockInDataType.Qty = Math.Abs(fooQty);
                                ws.Qty = Math.Abs(fooQty);

                                stockOutType.InNo = stockout[0].InNo;
                                stockOutType.Qty = fooQty;
                                stockOutType.Price = stockout[0].Price;
                                fooQty = 0;
                                stockOutType.Id = 0;
                                NSession.Clear();
                                NSession.Save(stockInDataType);
                                NSession.Save(stockOutType);
                                NSession.Flush();
                            }
                        }
                        tx.Commit();
                    }
                    catch (HibernateException exc)
                    {
                        tx.Rollback();

                    }
                }

                return stockOutType.Id;
            }
            else
            {
                IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + sku + "'").List<ProductType>();
                if (productTypes.Count > 0)
                {
                    WarehouseType warehouse = NSession.Get<WarehouseType>(wid);
                    AddToWarehouse(productTypes[0], user, NSession, warehouse.Id, 0);
                    StockOut(wid, sku, num, outType, user, memo, orderNo, NSession);
                }

            }
            return 0;
        }

        //内销存入出库记录表（不减库存，仅存入）
        public static bool StockOutDirect(string s_name, int wid, string sku, int num, string outType, string user, string memo, string orderNo, decimal price, ISession NSession)
        {
            StockOutType stockOutType = new StockOutType();
            stockOutType.WName = s_name;
            stockOutType.CreateBy = user;
            stockOutType.CreateOn = DateTime.Now;
            stockOutType.IsAudit = 1;
            stockOutType.OrderNo = orderNo;
            stockOutType.Qty = 1;
            stockOutType.SKU = sku;
            stockOutType.OutType = outType;
            //    stockOutType.SourceQty =1;
            stockOutType.Price = price;
            stockOutType.WId = wid;
            stockOutType.Memo = memo;
            NSession.Save(stockOutType);
            NSession.Flush();
            return true;
        }

        public static int StockIn(int wid, string sku, int num, double price, string InType, string user, string memo, ISession NSession, bool isAudit = false, bool isReStock = true, string Sort = "ASC")
        {
            IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", wid).SetString("p2", sku).List<WarehouseStockType>();
            IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + sku + "'").List<ProductType>();
            if (list.Count > 0)
            {
                WarehouseStockType ws = list[0];
                ws.Qty = ws.Qty + num;
                ws.UpdateOn = DateTime.Now;
                NSession.SaveOrUpdate(ws);
                NSession.Flush();
                SetComposeStock(sku, wid, user, NSession);



                StockInType stockInType = new StockInType();
                stockInType.IsAudit = 0;
                if (isAudit)
                    stockInType.IsAudit = 1;
                stockInType.Price = price;
                stockInType.Qty = num;
                stockInType.SKU = sku;
                stockInType.WId = wid;
                stockInType.InType = InType;
                stockInType.Memo = memo;
                stockInType.WName = ws.Warehouse;
                stockInType.SourceQty = ws.Qty - num;
                stockInType.CreateBy = user;
                stockInType.CreateOn = DateTime.Now;
                NSession.SaveOrUpdate(stockInType);
                NSession.Flush();

                if (num < 0)
                {
                    // 冲红减对应批次库存明细
                    // 先入先出扣数量
                    using (ITransaction tx = NSession.BeginTransaction())
                    {
                        try
                        {
                            //List<WarehouseStockDataType> stockInDataTypes = NSession.CreateQuery("from WarehouseStockDataType where NowQty=" + Math.Abs(num) + " and WId=" + stockInType.WId + " and SKU='" + stockInType.SKU + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();
                            //List<WarehouseStockDataType> WarehouseStockDataList = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockInType.WId + " and SKU='" + stockInType.SKU + "' Order By CreateOn ASC").List<WarehouseStockDataType>().ToList();
                            List<WarehouseStockDataType> WarehouseStockDataList = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockInType.WId + " and SKU='" + stockInType.SKU + "' Order By CreateOn " + Sort).List<WarehouseStockDataType>().ToList();
                            int fooQty = stockInType.Qty;
                            /*
                            foreach (var stockInDataType in stockInDataTypes)
                            {
                                if (stockInDataType.NowQty <= fooQty)//需要匹配多个现有批次
                                {
                                    fooQty = fooQty - stockInDataType.NowQty;
                                    //stockOutType.Qty = stockInDataType.NowQty;
                                    //stockOutType.InNo = stockInDataType.InNo;
                                    //stockOutType.Price = stockInDataType.Amount;
                                    stockInDataType.NowQty = 0;
                                    //stockOutType.Id = 0;
                                    NSession.Clear();
                                    NSession.Update(stockInDataType);
                                    //NSession.Save(stockOutType);
                                    NSession.Flush();
                                }
                                else
                                {//一个批次就能满足现有的需求
                                    stockInDataType.NowQty = stockInDataType.NowQty - Math.Abs(fooQty);
                                    //stockOutType.InNo = stockInDataType.InNo;
                                    //stockOutType.Qty = fooQty;
                                    //stockOutType.Price = stockInDataType.Amount;
                                    fooQty = 0;
                                    //stockOutType.Id = 0;
                                    NSession.Clear();
                                    NSession.Update(stockInDataType);
                                    //NSession.Save(stockOutType);
                                    NSession.Flush();
                                }
                                if (fooQty == 0) break;
                            }*/
                            foreach (var stockInDataType in WarehouseStockDataList)
                            {
                                if (stockInDataType.NowQty >= fooQty)
                                {
                                    // 批次匹配
                                    fooQty = (fooQty) + stockInDataType.NowQty;
                                    stockInDataType.NowQty = fooQty < 0 ? 0 : fooQty;
                                    NSession.Clear();
                                    NSession.Update(stockInDataType);
                                    NSession.Flush();
                                }
                                if (fooQty >= 0) break;
                            }
                            tx.Commit();
                        }
                        catch (HibernateException)
                        {
                            tx.Rollback();
                        }
                    }
                }
                else
                {
                    // 创建库存明细
                    WarehouseStockDataType stockData = new WarehouseStockDataType();

                    stockData.InId = stockInType.Id;
                    stockData.InNo = stockInType.Id.ToString();
                    stockData.WId = ws.WId;
                    stockData.Amount = Utilities.ToDecimal(price);
                    stockData.WName = ws.Warehouse;
                    stockData.SKU = ws.SKU;
                    stockData.PId = ws.PId;
                    stockData.PName = ws.Title;
                    stockData.Qty = stockData.NowQty = num;
                    stockData.CreateOn = DateTime.Now;
                    stockData.Id = 0;
                    NSession.Save(stockData);
                    NSession.Flush();
                }

                if (price > 0)
                {
                    foreach (var productType in productTypes)
                    {
                        productType.Price = price;
                        NSession.Update(productType);
                        NSession.Flush();
                    }
                }
                if (isReStock)
                {
                    IList<OrderType> list2 =
                        NSession.CreateQuery(
                            " from OrderType where Id in(select OId from OrderProductType where SKU ='" + sku +
                            "' ) and  Status in ('已处理') Order By CreateOn Asc ").List<OrderType>();
                    NSession.CreateSQLQuery(" update OrderProducts set IsQue=0 where  SKU='" + sku + "'").UniqueResult();
                    foreach (OrderType type in list2)
                    {
                        OrderHelper.SetQueOrder(type, NSession);
                    }
                }
                return stockInType.Id;
            }
            else
            {

                if (productTypes.Count > 0)
                {
                    AddToWarehouse(productTypes[0], user, NSession, wid, 0);

                    return StockIn(wid, sku, num, price, InType, user, memo, NSession, isAudit);
                }
            }
            return 0;
        }

        // 海外废品仓库用返回库存明细批次ID
        public static int StockIn(int wid, string sku, int num, double price, string InType, string user, string memo, ISession NSession, out int WarehouseStockDataId, bool isAudit = false, bool isReStock = true, string Sort = "ASC")
        {
            IList<WarehouseStockType> list = NSession.CreateQuery(" from WarehouseStockType where WId=:p1 and SKU=:p2").SetInt32("p1", wid).SetString("p2", sku).List<WarehouseStockType>();
            IList<ProductType> productTypes = NSession.CreateQuery("from ProductType where SKU='" + sku + "'").List<ProductType>();
            WarehouseStockDataId = 0; // 默认为0 
            if (list.Count > 0)
            {
                WarehouseStockType ws = list[0];
                ws.Qty = ws.Qty + num;
                ws.UpdateOn = DateTime.Now;
                NSession.SaveOrUpdate(ws);
                NSession.Flush();
                SetComposeStock(sku, wid, user, NSession);



                StockInType stockInType = new StockInType();
                stockInType.IsAudit = 0;
                if (isAudit)
                    stockInType.IsAudit = 1;
                stockInType.Price = price;
                stockInType.Qty = num;
                stockInType.SKU = sku;
                stockInType.WId = wid;
                stockInType.InType = InType;
                stockInType.Memo = memo;
                stockInType.WName = ws.Warehouse;
                stockInType.SourceQty = ws.Qty - num;
                stockInType.CreateBy = user;
                stockInType.CreateOn = DateTime.Now;
                NSession.SaveOrUpdate(stockInType);
                NSession.Flush();

                if (num < 0)
                {
                    // 冲红减对应批次库存明细
                    // 先入先出扣数量
                    using (ITransaction tx = NSession.BeginTransaction())
                    {
                        try
                        {
                            List<WarehouseStockDataType> WarehouseStockDataList = NSession.CreateQuery("from WarehouseStockDataType where NowQty>0 and WId=" + stockInType.WId + " and SKU='" + stockInType.SKU + "' Order By CreateOn " + Sort).List<WarehouseStockDataType>().ToList();
                            int fooQty = stockInType.Qty;
                            foreach (var stockInDataType in WarehouseStockDataList)
                            {
                                if (stockInDataType.NowQty >= fooQty)
                                {
                                    // 批次匹配
                                    fooQty = (fooQty) + stockInDataType.NowQty;
                                    stockInDataType.NowQty = fooQty < 0 ? 0 : fooQty;
                                    NSession.Clear();
                                    NSession.Update(stockInDataType);
                                    NSession.Flush();
                                }
                                if (fooQty >= 0) break;
                            }
                            tx.Commit();
                        }
                        catch (HibernateException)
                        {
                            tx.Rollback();
                        }
                    }
                }
                else
                {
                    // 创建库存明细
                    WarehouseStockDataType stockData = new WarehouseStockDataType();

                    stockData.InId = stockInType.Id;
                    stockData.InNo = stockInType.Id.ToString();
                    stockData.WId = ws.WId;
                    stockData.Amount = Utilities.ToDecimal(price);
                    stockData.WName = ws.Warehouse;
                    stockData.SKU = ws.SKU;
                    stockData.PId = ws.PId;
                    stockData.PName = ws.Title;
                    stockData.Qty = stockData.NowQty = num;
                    stockData.CreateOn = DateTime.Now;
                    stockData.Id = 0;
                    NSession.Save(stockData);
                    NSession.Flush();

                    WarehouseStockDataId = stockData.Id; // 记录库存明细批次ID
                }

                if (price > 0)
                {
                    foreach (var productType in productTypes)
                    {
                        productType.Price = price;
                        NSession.Update(productType);
                        NSession.Flush();
                    }
                }
                if (isReStock)
                {
                    IList<OrderType> list2 =
                        NSession.CreateQuery(
                            " from OrderType where Id in(select OId from OrderProductType where SKU ='" + sku +
                            "' ) and  Status in ('已处理') Order By CreateOn Asc ").List<OrderType>();
                    NSession.CreateSQLQuery(" update OrderProducts set IsQue=0 where  SKU='" + sku + "'").UniqueResult();
                    foreach (OrderType type in list2)
                    {
                        OrderHelper.SetQueOrder(type, NSession);
                    }
                }
                return stockInType.Id;
            }
            else
            {

                if (productTypes.Count > 0)
                {
                    AddToWarehouse(productTypes[0], user, NSession, wid, 0);

                    return StockIn(wid, sku, num, price, InType, user, memo, NSession, out WarehouseStockDataId, isAudit);
                }
            }
            return 0;
        }
        public static String ToDBC(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }


        public static int ToInt(string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static int ToInt(object obj)
        {
            try
            {
                if (obj == null || obj is DBNull)
                {
                    return 0;
                }
                return ToInt(obj.ToString());
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static double ToDouble(string str)
        {
            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static double ToDouble(object obj)
        {
            try
            {
                return Convert.ToDouble(obj);
            }
            catch (Exception)
            {
                return 0;
            }


        }
        public static decimal ToDecimal(string str)
        {
            try
            {
                return Convert.ToDecimal(str);
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static decimal ToDecimal(object str)
        {
            try
            {
                return Convert.ToDecimal(str);
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static string ToString(object obj)
        {
            try
            {
                if (obj == null)
                    return "";
                return obj.ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }


        public static string ToStr(this object obj)
        {
            try
            {
                if (obj is DBNull || obj == null)
                    return "";
                return obj.ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }
        public static string OrdeerBy(string sort, string order, string defaultSort)
        {
            string orderby = " order by " + defaultSort;
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }
            return orderby;
        }
        public static string OrdeerBy(string sort, string order)
        {
            return OrdeerBy(sort, order, "Id desc");

        }
        public static string SqlWhere(string search)
        {
            //search=HttpUtility.UrlDecode(search);
            string where = string.Empty;
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            return where;
        }


        internal static void GetSMTSSSs()
        {

            string str = @"
Dear Friend,
We appreciated your purchase from us. However, we noticed you that haven't made the payment yet. This is a friendly reminder to you to complete the payment transaction asap. Instant payments are very important, the earlier you pay, the sooner you will get the item.
Thanks again! Looking forward to hearing from you soon.
Best regards,
Bestore";

            //            string str = @"My dear friend ,
            //Thank you for your order. We will send you the product soon . Welcome to our shop next time!
            //Please buy more products in one time if you like,and we would like to give more discount to you,because the shipping fee is expensive.
            //So if you buy any other product together in our shop, you can get $1 discount for each other product, when you buy other product please leave a message to tell us your last order ID number ,we will help you modify the price and give you the discount.Your friend can also use this oeder ID number to get discount,we will send products to you together.";
            ISession NSession = NhbHelper.OpenSession();

            //IList<AccountType> list =
            //    NSession.CreateQuery("from AccountType where Platform='Aliexpress' and AccountName not like '%yw%'").List<AccountType>();

            IList<AccountType> list =
                NSession.CreateQuery("from AccountType where Platform='Aliexpress' and AccountName ='smt01'").List<AccountType>();
            foreach (AccountType accountType in list)
            {
                if (!string.IsNullOrEmpty(accountType.ApiToken) && !string.IsNullOrEmpty(accountType.ApiKey) && !string.IsNullOrEmpty(accountType.ApiSecret))
                {
                    string token = AliUtil.RefreshToken(accountType);
                    if (token == "")
                    {
                        continue;
                    }
                    List<CountryType> countryTypes = NSession.CreateQuery("from CountryType").List<CountryType>().ToList();
                    AliOrderListType aliOrderList = null;
                    int page = 1;
                    do
                    {
                        try
                        {
                            aliOrderList = AliUtil.findOrderListQuery(accountType.ApiKey, accountType.ApiSecret, token, page, null, null, "PLACE_ORDER_SUCCESS");
                            if (aliOrderList.totalItem != 0)
                            {
                                foreach (var o in aliOrderList.orderList)
                                {
                                    // continue;
                                    AliUtil.AddOrderMessage(accountType.ApiKey, accountType.ApiSecret, token,
                                                            o.orderId, str, o.buyerLoginId);
                                    System.Diagnostics.Debug.WriteLine(o.orderId);

                                }
                                page++;
                            }
                        }
                        catch (Exception ex)
                        {
                            token = AliUtil.RefreshToken(accountType);
                            continue;
                        }
                    } while (aliOrderList.totalItem > (page - 1) * 50);



                }
            }

            NSession.Close();
        }
        internal static void GetSMTShouHui()
        {
            ISession NSession = NhbHelper.OpenSession();
            IList<AccountType> list =
            NSession.CreateQuery("from AccountType where Platform='Aliexpress'").
                List<AccountType>();
            //IList<AccountType> list =
            //    NSession.CreateQuery("from AccountType where Platform='Aliexpress' and AccountName='smt01'").
            //        List<AccountType>(); // 测试
            //int ttt = 0;
            foreach (AccountType accountType in list)
            {
                //ttt++;
                //if (ttt < 7)
                //{
                //    continue;

                //}
                if (!string.IsNullOrEmpty(accountType.ApiToken) && !string.IsNullOrEmpty(accountType.ApiKey) &&
                    !string.IsNullOrEmpty(accountType.ApiSecret))
                {
                    try
                    {
                        accountType.ApiTokenInfo = AliUtil.RefreshToken(accountType);
                        int i = 0;
                        int page = 1;
                        do
                        {
                            try
                            {
                                LoanRootObject rootObject = AliUtil.FindLoanListQuery(accountType.ApiKey, accountType.ApiSecret, accountType.ApiTokenInfo, page);
                                string orderStr = "";
                                if (rootObject.orderList == null || rootObject.totalItem == 0)
                                {
                                    break;
                                }
                                foreach (POrderList pOrderList in rootObject.orderList)
                                {
                                    try
                                    {
                                        decimal amount =
                                    pOrderList.sonOrderList.Sum(x => x.realLoanAmount.amount);
                                        string orderId = pOrderList.orderId;
                                        List<OrderType> listorder = NSession.CreateQuery("from OrderType where OrderExNo='" + pOrderList.orderId + "' and Amount<>0").List<OrderType>().ToList();
                                        if (listorder.Count > 0)
                                        {
                                            if (listorder[0].FanState == 1)
                                            {
                                                continue;

                                            }

                                            listorder[0].FanAmount = amount;
                                            if (listorder[0].FanAmount > Convert.ToDecimal(listorder[0].Amount))
                                            {
                                                listorder[0].FanAmount = Convert.ToDecimal(listorder[0].Amount) * 0.9m;
                                            }
                                            foreach (var orderType in listorder)
                                            {
                                                orderType.FanDate = DateTime.Now;
                                                orderType.FanState = 1;
                                                NSession.Update(orderType);

                                                NSession.Flush();
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }


                                }
                                page++;
                                //i = i + rootObject.orderList.Count;
                                //if (rootObject.totalItem == i)
                                //{
                                //    break;
                                //}
                            }
                            catch (Exception ex)
                            {
                                accountType.ApiTokenInfo = AliUtil.RefreshToken(accountType);
                                continue;
                                ;
                            }
                        } while (1 == 1);
                        int nI = 0;
                    }
                    catch (Exception ee)
                    {
                        continue;
                    }

                }
            }
        }

        /// <summary>
        /// 速卖通自动评价
        /// </summary>
        internal static void GetSMTSSS()
        {
            string str = @"Excellently! Good buyer~";
            ISession NSession = NhbHelper.OpenSession();

            /////////////////////////////
            //// 测试计算财务数据
            //List<OrderType> type = NSession.CreateQuery("from OrderType where OrderNo='1249612'").List<OrderType>().ToList<OrderType>();
            //OrderHelper.ReckonFinance(type[0], NSession);
            /////////////////////////////

            IList<AccountType> list =
                NSession.CreateQuery("from AccountType where Platform='Aliexpress' and AccountName not like '%yw%'").List<AccountType>();

            foreach (AccountType accountType in list)
            {
                if (!string.IsNullOrEmpty(accountType.ApiToken) && !string.IsNullOrEmpty(accountType.ApiKey) && !string.IsNullOrEmpty(accountType.ApiSecret))
                {
                    try
                    {
                        EvaluationRoot root = new EvaluationRoot();
                        accountType.ApiTokenInfo = AliUtil.RefreshToken(accountType);

                        do
                        {
                            root = AliUtil.findSellerEvaluationOrderList(accountType.ApiKey, accountType.ApiSecret, accountType.ApiTokenInfo);
                            if (root.totalItem < 10)
                                break;
                            else
                            {
                                foreach (EvaluationListResult eee in root.listResult)
                                {
                                    AliUtil.SaveSellerFeedback(accountType.ApiKey, accountType.ApiSecret, accountType.ApiTokenInfo, eee.orderId.ToString(), str);
                                }
                            }
                        } while (1 == 1);


                        // OrderHelper.APIBySMT(accountType, DateTime.Now, DateTime.Now, NSession);
                    }
                    catch (Exception)
                    {

                        continue;
                        ;
                    }
                }

            }

            NSession.Close();


        }

        internal static void GetSMTOrderByAPI()
        {
            ISession NSession = NhbHelper.OpenSession();

            IList<AccountType> list =
                NSession.CreateQuery("from AccountType where Platform='Aliexpress'").List<AccountType>();

            foreach (AccountType accountType in list)
            {
                if (!string.IsNullOrEmpty(accountType.ApiToken) && !string.IsNullOrEmpty(accountType.ApiKey) && !string.IsNullOrEmpty(accountType.ApiSecret))
                {
                    OrderHelper.APIBySMT(accountType, DateTime.Now, DateTime.Now, NSession);

                }
            }

            NSession.Close();
        }
    }


    /// <summary>
    /// JsonResult格式化扩展
    /// </summary>
    public class FormatJsonResult : ActionResult
    {
        private bool iserror = false;
        /// <summary>
        /// 是否产生错误
        /// </summary>
        public bool IsError
        {
            get { return iserror; }
            set { this.iserror = value; }
        }

        private bool isSuccess = false;
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get { return isSuccess; }
            set { this.isSuccess = value; }
        }

        /// <summary>
        /// 错误信息，或者成功信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 成功可能时返回的数据
        /// </summary>
        public Object Data { get; set; }
        /// <summary>
        /// 正常序列化方式(为True则不进行UI友好的序列化)
        /// </summary>
        public bool NotLigerUIFriendlySerialize { get; set; }
        public override void ExecuteResult(ControllerContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json";

            StringWriter sw = new StringWriter();
            JsonSerializer serializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    // Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter() },
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore

                }
                );


            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                if (!NotLigerUIFriendlySerialize)
                    serializer.Serialize(jsonWriter, this);
                else
                    serializer.Serialize(jsonWriter, Data);
            }
            response.Write(sw.ToString());

        }
    }

    public class ResultInfo
    {
        public virtual string Key { get; set; }
        public virtual string Field1 { get; set; }
        public virtual string Field2 { get; set; }
        public virtual string Field3 { get; set; }
        public virtual string Field4 { get; set; }
        /// <summary>
        /// 原因
        /// </summary>
        public virtual string Info { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public virtual string Result { get; set; }


        public virtual DateTime CreateOn { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Memo { get; set; }
    }

    public class ModelData<T>
    {
        public virtual T m0 { get; set; }

        public virtual T m1 { get; set; }

        public virtual T m2 { get; set; }

        public virtual T m3 { get; set; }

        public virtual T m4 { get; set; }
    }

}