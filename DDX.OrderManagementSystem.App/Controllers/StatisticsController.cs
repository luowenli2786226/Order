using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DDX.OrderManagementSystem.Domain;
using System.Collections;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class StatisticsController : BaseController
    {
        public ActionResult AmountCount(DateTime st, DateTime et, string p, string a)
        {
            IList<DDX.OrderManagementSystem.App.AmountCount> list = new List<DDX.OrderManagementSystem.App.AmountCount>();
            string str = this.Where(st, et, p, a);
            IList<object[]> list2 = base.NSession.CreateQuery("select Account,Count(Account),sum(Amount) from RefundAmountType " + str + " group by Account").List<object[]>();
            foreach (object[] objArray in list2)
            {
                string str2 = objArray[0].ToString();
                int num = Convert.ToInt32(objArray[1]);
                decimal num2 = Convert.ToDecimal(objArray[2]);
                DDX.OrderManagementSystem.App.AmountCount item = new DDX.OrderManagementSystem.App.AmountCount
                {
                    Account = str2,
                    Count = num,
                    Qcount = num2
                };
                list.Add(item);
            }
            return base.Json(new
            {
                rows = from x in list
                       orderby x.Count descending
                       select x
            });
        }

        private static string ConvertJson(IList<object[]> objectses, List<string> strData)
        {
            int num2;
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            int num = 0;
            while (num < objectses.Count)
            {
                builder.Append("{");
                num2 = 0;
                while (num2 < strData.Count)
                {
                    builder.Append("\"");
                    builder.Append(strData[num2]);
                    builder.Append("\":\"");
                    builder.Append(objectses[num][num2]);
                    builder.Append("\",");
                    num2++;
                }
                builder.Remove(builder.Length - 1, 1);
                builder.Append("},");
                num++;
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]");
            StringBuilder builder2 = new StringBuilder();
            builder2.Append("[{");
            for (num2 = 1; num2 < strData.Count; num2++)
            {
                int num3 = 0;
                builder2.Append("\"");
                builder2.Append(strData[num2]);
                builder2.Append("\":\"");
                for (num = 0; num < objectses.Count; num++)
                {
                    num3 += Convert.ToInt32(objectses[num][num2]);
                }
                builder2.Append(num3.ToString());
                builder2.Append("\",");
            }
            builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("}]");
            return string.Concat(new object[] { "{\"total\":", objectses.Count, ",\"rows\":", builder.ToString(), ",\"footer\":", builder2.ToString(), "}" });
        }

        public ActionResult DisputeCount()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult DisputeCount(DateTime st, DateTime et, string p, string a)
        {
            IList<DDX.OrderManagementSystem.App.DisputeCount> list = new List<DDX.OrderManagementSystem.App.DisputeCount>();
            string str = this.Where(st, et, p, a);
            IList<object[]> list2 = base.NSession.CreateQuery("select Count(Id),DisputeCategory from DisputeType " + str + " group by DisputeCategory").List<object[]>();
            foreach (object[] objArray in list2)
            {
                string str2 = objArray[1].ToString();
                decimal num = Convert.ToDecimal(objArray[0]);
                DDX.OrderManagementSystem.App.DisputeCount item = new DDX.OrderManagementSystem.App.DisputeCount
                {
                    DType = str2,
                    Count = num
                };
                list.Add(item);
            }
            return base.Json(new
            {
                rows = from x in list
                       orderby x.Count descending
                       select x
            });
        }

        [HttpPost]
        public ActionResult DisputeTypeCount(DateTime st, DateTime et, string p, string a)
        {
            IList<DDX.OrderManagementSystem.App.DisputeCount> list = new List<DDX.OrderManagementSystem.App.DisputeCount>();
            string str = this.Where(st, et, p, a);
            IList<object[]> list2 = base.NSession.CreateQuery("select count(Id),Solution  from DisputeType " + str + " group by Solution").List<object[]>();
            foreach (object[] objArray in list2)
            {
                string str2 = "未开始处理";
                if (objArray[1] != null)
                {
                    str2 = objArray[1].ToString();
                }
                decimal num = Convert.ToDecimal(objArray[0]);
                DDX.OrderManagementSystem.App.DisputeCount item = new DDX.OrderManagementSystem.App.DisputeCount
                {
                    DType = str2,
                    Count = num
                };
                list.Add(item);
            }
            return base.Json(new
            {
                rows = from x in list
                       orderby x.Count descending
                       select x
            });
        }

        public JsonResult ExportOut(DateTime st, DateTime et, string a, string p)
        {
            List<ProductData> modelList = this.GetOutCount(st, et, a, p);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable<ProductData>(modelList));
            return base.Json(new { IsSuccess = true });
        }

        public JsonResult ExportQue(string area, string s, string p, string a, int iscon = 2, int isneed = 1)
        {
            s = SqlWhere(a, p, s, "");
            s = s.Replace("SKU", "OP.SKU");
            if (s.Length > 0)
            {
                s = " and " + s;
            }
            List<QuestionSKUType> listsku = NSession.CreateQuery("from QuestionSKUType").List<QuestionSKUType>().ToList();
            string str2 = "";
            if (area != "0")
            {
                if (area == "1")
                    s += "and O.Account not like '%yw%'";
                if (area == "2")
                    s += "and O.Account  like '%yw%'";

            }
            if (isneed != 2)
            {
                if (isneed != 0)
                    str2 += "and NeedQty > 0";
            }
            if (iscon != 2)
            {
                if (iscon == 1)
                    str2 += " and SKU in (Select SKU from QuestionSKU)";
                else
                    str2 += " and SKU not in (Select SKU from QuestionSKU)";
            }
            string str3 =
                string.Format(
           //         "select * from (\r\nselect *,(Qty-BuyQty-UnPeiQty) as NeedQty,(SQty-BuyQty-UnPeiQty) as SNeedQty from( select * ,isnull((select SUM(Qty) from WarehouseStock where WarehouseStock.SKU=tbl1.SKU),0) as UnPeiQty,\r\nisnull((select SUM(Qty) from   Orders O left join OrderProducts OP On O.Id=OP.OId where O.IsOutOfStock=1 and O.Enabled=1 and O.Status in ('已处理','待拣货') and OP.IsQue=1 and OP.SKU=tbl1.SKU),0) as 'SQty'\r\nfrom (\r\nselect OP.SKU,SUM(OP.Qty) as Qty,MIN(O.CreateOn) as MinDate,P.Standard,\r\n(select isnull(SUM(Qty-DaoQty),0) from PurchasePlan \r\n where Status in('已采购','已发货','部分到货') and  SKU=OP.SKU ) as BuyQty\r\nfrom Orders O left join OrderProducts OP On O.Id=OP.OId \r\nleft join Products P on OP.SKU=P.SKU \r\nwhere  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null {0} group by OP.SKU,P.Standard)\r\n as tbl1 ) as tbl2 ) as tbl3 where  (Qty-UnPeiQty)>0 {1} ",
           "select * from (\r\nselect *,(Qty-BuyQty-UnPeiQty) as NeedQty,(SQty-BuyQty-UnPeiQty) as SNeedQty from( select * ,isnull((select SUM(Qty) from WarehouseStock where WarehouseStock.SKU=tbl1.SKU),0) as UnPeiQty,\r\nisnull((select SUM(Qty) from   Orders O left join OrderProducts OP On O.Id=OP.OId where O.IsOutOfStock=1 and O.Enabled=1 and O.Status in ('已处理','待拣货') and OP.IsQue=1 and OP.SKU=tbl1.SKU),0) as 'SQty'\r\nfrom (\r\nselect OP.SKU,SUM(OP.Qty) as Qty,MIN(O.CreateOn) as MinDate,P.Standard,\r\n(select isnull(SUM(Qty-DaoQty),0) from PurchasePlan \r\n where Status in('已采购','已发货','部分到货') and  SKU=OP.SKU ) as BuyQty,MAX(QS.Memo)\r\n as Memo from Orders O left join OrderProducts OP On O.Id=OP.OId \r\nleft join Products P on OP.SKU=P.SKU  \r\nleft join QuestionSKU QS on OP.SKU=QS.SKU \r\nwhere  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null {0} group by OP.SKU,P.Standard)\r\n as tbl1 ) as tbl2 ) as tbl3 where  (Qty-UnPeiQty)>0 {1} ",
                    s, str2);


            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str3;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(dataSet);
            return base.Json(new { IsSuccess = true });
        }

        public JsonResult ExportSellCount(DateTime st, DateTime et, string a, string p, string ss)
        {
            List<ProductData> modelList = this.GetSellCount(st, et, a, p, ss, 0, 0);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable<ProductData>(modelList));
            return base.Json(new { IsSuccess = true });
        }

        public JsonResult ExportPackagingScanLog(int year, int month)
        {
            List<PackagingScanLog> modelList = this.PackagingScanLogList(year, month, 0, 0);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable<PackagingScanLog>(modelList));
            return base.Json(new { IsSuccess = true });
        }

        public JsonResult ExportPackagingScanLogII(int year, int month)
        {
            List<PackagingScanLogII> modelList = this.PackagingScanLogListII(year, month, 0, 0);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable<PackagingScanLogII>(modelList));
            return base.Json(new { IsSuccess = true });
        }

        public JsonResult ExportSore(DateTime st, DateTime et)
        {
            IList<Sores> modelList = this.SoreList(st, et);
            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.FillDataTable<Sores>(modelList));
            return base.Json(new { IsSuccess = true });
        }

        [HttpPost]
        public ActionResult GetColumns(DateTime st, DateTime et)
        {
            List<object> list = new List<object>();
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                string introduced5 = time.ToString("MMdd");
                list.Add(new { field = introduced5, title = time.ToString("MM.dd") + "(" + week + ")" });
            }
            list.Add(new { field = "合计", title = "合计", width = "80" });
            list.Add(new { field = "近3个月", title = "近3个月", width = "80" });
            list.Add(new { field = "近6个月", title = "近6个月", width = "80" });
            list.Add(new { field = "一年统计", title = "一年统计", width = "80" });
            return base.Json(new { columns = list });
        }

        public JsonResult GetOrder(string id)
        {
            string str = this.sub(id);
            id = this.subid(id);
            DateTime time = Convert.ToDateTime(this.sub(id));
            id = this.subid(id);
            DateTime time2 = Convert.ToDateTime(this.sub(id));
            id = this.subid(id);
            string p = this.sub(id);
            id = this.subid(id);
            string a = id;
            IList<OrderType> list = new List<OrderType>();
            IList<OrderType> data = base.NSession.CreateQuery(string.Concat(new object[] { "from OrderType where Enabled=1 and Id in(select OId  from OrderProductType where SKU='", str, "') and CreateOn >='", time, "' and CreateOn <'", time2.AddDays(1.0), "'", this.pa(p, a) })).List<OrderType>();
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        private List<ProductData> GetOutCount(DateTime st, DateTime et, string a, string p)
        {
            string str = this.SqlWhere(st, et, a, p, "");
            IList<object[]> list = base.NSession.CreateQuery(string.Format("select SKU,SUM(Qty) as Qty from OrderProductType where OId in(select Id from OrderType {0} and Status='已发货') group by SKU ", str)).List<object[]>();
            string str2 = string.Empty;
            List<ProductData> list2 = new List<ProductData>();
            foreach (object[] objArray in list)
            {
                str2 = str2 + objArray[0] + ",";
                ProductData item = new ProductData
                {
                    SKU = objArray[0].ToString(),
                    Qty = Convert.ToInt32(objArray[1])
                };
                list2.Add(item);
            }
            List<ProductType> list3 = base.NSession.CreateQuery("from ProductType where SKU in('" + str2.Trim(new char[] { ',' }).Replace(",", "','") + "')").List<ProductType>().ToList<ProductType>();
            using (List<ProductData>.Enumerator enumerator2 = list2.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Predicate<ProductType> match = null;
                    ProductData pp = enumerator2.Current;
                    if (match == null)
                    {
                        match = x => pp.SKU.Trim().ToUpper() == x.SKU.Trim().ToUpper();
                    }
                    ProductType type = list3.Find(match);
                    if (type != null)
                    {
                        pp.Price = type.Price.ToString();
                        pp.PicUrl = type.SPicUrl;
                        pp.Title = type.ProductName;
                        pp.TotalPrice = Utilities.ToDouble(pp.Price) * pp.Qty;
                    }
                }
            }
            return list2;
        }

        [HttpPost]
        public ContentResult GetSaleChart(string s, DateTime st, string p)
        {
            int num;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<chart caption='{0} 的销售记录({1}-{2})' subCaption='销量'  showLabels='1' showColumnShadow='1' animation='1' showAlternateHGridColor='1' AlternateHGridColor='ff5904' divLineColor='ff5904' divLineAlpha='20' alternateHGridAlpha='5' canvasBorderColor='666666' baseFontColor='666666'  lineAlpha='85' showValues='1' rotateValues='0' valuePosition='auto' canvaspadding='8' lineThickness='3'>");
            DateTime now = st.AddDays(15.0);
            List<string> list = new List<string>();
            StringBuilder builder2 = new StringBuilder();
            DateTime date = st;
            builder2.Append("select {0} as 'Y',");
            if (now > DateTime.Now)
            {
                now = DateTime.Now;
            }
            while (date <= now)
            {
                string week = this.GetWeek("zh", date);
                list.Add(date.ToString("MM.dd") + "(" + week + ")");
                builder2.Append(" SUM(case  when convert(varchar(10),[CreateOn],120)='" + date.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + date.ToString("MM.dd") + "(" + week + ")' ,");
                date = date.AddDays(1.0);
            }
            builder2 = builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("  from  ( select {0} ,convert(varchar(10),[CreateOn],120) [CreateOn] ,sum(op.Qty) as 'rcount'  from Orders O left join OrderProducts OP on O.Id=OP.OId where [CreateOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + now.ToString("yyyy-MM-dd") + " 23:59:59' and SKU='{1}' {2}  group by {0} ,convert(varchar(10),[CreateOn],120)) as tbl1  group by {0} ");
            string queryString = "";
            if (p != "ALL")
            {
                queryString = string.Format(builder2.ToString(), "Account", s, " and Platform='" + p + "'");
            }
            else
            {
                queryString = string.Format(builder2.ToString(), "Platform", s, "");
            }
            IList<object[]> list2 = base.NSession.CreateSQLQuery(queryString).List<object[]>();
            builder.AppendLine("<categories>");
            foreach (string str3 in list)
            {
                builder.Append("<category label='" + str3 + "'/>");
            }
            builder.AppendLine("</categories>");
            foreach (object[] objArray in list2)
            {
                builder.Append("<dataset lineThickness='3' seriesName=\"" + objArray[0] + "\">");
                num = 1;
                while (num < objArray.Length)
                {
                    builder.Append("<set value='" + objArray[num] + "' />");
                    num++;
                }
                builder.AppendLine("</dataset>");
            }
            foreach (object[] objArray in list2)
            {
                builder.Append("<dataset lineThickness='3' seriesName='ALL'>");
                for (num = 1; num < objArray.Length; num++)
                {
                    int num2 = 0;
                    for (int i = 0; i < list2.Count; i++)
                    {
                        num2 += Convert.ToInt32(list2[i][num]);
                    }
                    builder.Append("<set value='" + num2 + "' />");
                }
                builder.AppendLine("</dataset>");
                break;
            }
            builder.AppendLine("</chart>");
            return base.Content(string.Format(builder.ToString(), s, st.ToShortDateString(), now.ToShortDateString()));
        }

        public JsonResult GetScore(DateTime st, DateTime et)
        {
            IList<Sores> list = this.SoreList(st, et);
            return base.Json(new
            {
                rows = from p in list
                       orderby p.PackSores descending
                       select p
            });
        }

        [HttpPost]
        private List<ProductData> GetSellCount(DateTime st, DateTime et, string a, string p, string s, int page = 0, int rows = 0)
        {
            string str = this.SqlWhere(st, et, a, p, s);
            if (str.Length > 3)
            {
                str = str + " and SKU is not null ";
            }
            else
            {
                str = " where SKU is not null ";
            }
            IList<object[]> list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(Orders.Id) as Qty from OrderProducts right join Orders on OId=Orders.Id   {0} group by SKU Order By sQty desc", str)).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<object[]>();
            if (page == 0)
            {
                list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(Orders.Id) as Qty from OrderProducts right join Orders on OId=Orders.Id   {0} group by SKU Order By sQty desc", str)).List<object[]>();
            }
            else
            {
                list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(Orders.Id) as Qty from OrderProducts right join Orders on OId=Orders.Id   {0} group by SKU Order By sQty desc", str)).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<object[]>();
            }
            string str2 = string.Empty;
            List<ProductData> list2 = new List<ProductData>();
            foreach (object[] objArray in list)
            {
                str2 = str2 + objArray[0] + ",";
                ProductData item = new ProductData
                {
                    SKU = objArray[0].ToString(),
                    Qty = Convert.ToInt32(objArray[1]),
                    OQty = Convert.ToInt32(objArray[2])
                };
                list2.Add(item);
            }
            List<ProductType> list3 = base.NSession.CreateQuery("from ProductType where SKU in('" + str2.Trim(new char[] { ',' }).Replace(",", "','") + "')").List<ProductType>().ToList<ProductType>();
            using (List<ProductData>.Enumerator enumerator2 = list2.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Predicate<ProductType> match = null;
                    ProductData pp = enumerator2.Current;
                    if (match == null)
                    {
                        match = x => pp.SKU.Trim().ToUpper() == x.SKU.Trim().ToUpper();
                    }
                    ProductType type = list3.Find(match);
                    if (type != null)
                    {
                        pp.Category = type.Category;
                        pp.Status = type.Status;
                        pp.Price = type.Price.ToString();
                        pp.PicUrl = type.SPicUrl;
                        pp.Title = type.ProductName;
                        pp.TotalPrice = Utilities.ToDouble(pp.Price) * pp.Qty;
                    }
                }
            }
            return list2;
        }

        [HttpPost]
        public List<PackagingScanLog> PackagingScanLogList(int year, int month, int page = 0, int rows = 0)
        {
            string str = this.SqlLog(year, month);

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportPackagingScan";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            IList<object[]> list = new List<object[]>();
            DataTable dt = ds.Tables[0];
            foreach (DataRow r in dt.Rows)
            {
                int colCount = r.ItemArray.Count();
                string[] items = new string[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    items[i] = Convert.ToString(r.ItemArray[i]);
                }
                list.Add(items);
            }

            string str2 = string.Empty;
            List<PackagingScanLog> list2 = new List<PackagingScanLog>();
            foreach (object[] objArray in list)
            {
                str2 = str2 + objArray[0] + ",";
                PackagingScanLog item = new PackagingScanLog
                {
                    PackagingType = objArray[0].ToString(),
                    Operator = objArray[1].ToString(),
                    d1 = objArray[2].ToString(),
                    d2 = objArray[3].ToString(),
                    d3 = objArray[4].ToString(),
                    d4 = objArray[5].ToString(),
                    d5 = objArray[6].ToString(),
                    d6 = objArray[7].ToString(),
                    d7 = objArray[8].ToString(),
                    d8 = objArray[9].ToString(),
                    d9 = objArray[10].ToString(),
                    d10 = objArray[11].ToString(),
                    d11 = objArray[12].ToString(),
                    d12 = objArray[13].ToString(),
                    d13 = objArray[14].ToString(),
                    d14 = objArray[15].ToString(),
                    d15 = objArray[16].ToString(),
                    d16 = objArray[17].ToString(),
                    d17 = objArray[18].ToString(),
                    d18 = objArray[19].ToString(),
                    d19 = objArray[20].ToString(),
                    d20 = objArray[21].ToString(),
                    d21 = objArray[22].ToString(),
                    d22 = objArray[23].ToString(),
                    d23 = objArray[24].ToString(),
                    d24 = objArray[25].ToString(),
                    d25 = objArray[26].ToString(),
                    d26 = objArray[27].ToString(),
                    d27 = objArray[28].ToString(),
                    d28 = objArray[29].ToString(),
                    d29 = objArray[30].ToString(),
                    d30 = objArray[31].ToString(),
                    d31 = objArray[32].ToString()

                };
                list2.Add(item);
            }
            return list2;
        }

        [HttpPost]
        public List<PackagingScanLogII> PackagingScanLogListII(int year, int month, int page = 0, int rows = 0)
        {
            string str = this.SqlLog(year, month);

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportPackagingScanP";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            IList<object[]> list = new List<object[]>();
            DataTable dt = ds.Tables[0];
            foreach (DataRow r in dt.Rows)
            {
                int colCount = r.ItemArray.Count();
                string[] items = new string[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    items[i] = Convert.ToString(r.ItemArray[i]);
                }
                list.Add(items);
            }

            string str2 = string.Empty;
            List<PackagingScanLogII> list2 = new List<PackagingScanLogII>();
            foreach (object[] objArray in list)
            {
                str2 = str2 + objArray[0] + ",";
                PackagingScanLogII item = new PackagingScanLogII
                {
                    Operator = objArray[0].ToString(),
                    d1 = objArray[1].ToString(),
                    d2 = objArray[2].ToString(),
                    d3 = objArray[3].ToString(),
                    d4 = objArray[4].ToString(),
                    d5 = objArray[5].ToString(),
                    d6 = objArray[6].ToString(),
                    d7 = objArray[7].ToString(),
                    d8 = objArray[8].ToString(),
                    d9 = objArray[9].ToString(),
                    d10 = objArray[10].ToString(),
                    d11 = objArray[11].ToString(),
                    d12 = objArray[12].ToString(),
                    d13 = objArray[13].ToString(),
                    d14 = objArray[14].ToString(),
                    d15 = objArray[15].ToString(),
                    d16 = objArray[16].ToString(),
                    d17 = objArray[17].ToString(),
                    d18 = objArray[18].ToString(),
                    d19 = objArray[19].ToString(),
                    d20 = objArray[20].ToString(),
                    d21 = objArray[21].ToString(),
                    d22 = objArray[22].ToString(),
                    d23 = objArray[23].ToString(),
                    d24 = objArray[24].ToString(),
                    d25 = objArray[25].ToString(),
                    d26 = objArray[26].ToString(),
                    d27 = objArray[27].ToString(),
                    d28 = objArray[28].ToString(),
                    d29 = objArray[29].ToString(),
                    d30 = objArray[30].ToString(),
                    d31 = objArray[31].ToString()

                };
                list2.Add(item);
            }
            return list2;
        }

        public string GetWeek(string lan, DateTime date)
        {
            string[] strArray = null;
            if (lan == "zh")
            {
                strArray = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            }
            else if (lan == "en")
            {
                strArray = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            }
            else
            {
                strArray = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            }
            int index = Convert.ToInt32(date.DayOfWeek);
            return strArray[index];
        }

        [HttpPost]
        public string JiCount(DateTime st, DateTime et)
        {
            List<string> strData = new List<string>();
            StringBuilder builder = new StringBuilder();
            strData.Add("人员");
            builder.Append("select [PackBy] as '扫描人',");
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                strData.Add(time.ToString("MMdd"));
                builder.Append("SUM(case  when convert(varchar(10),[PackOn],120)='" + time.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + time.ToString("MM.dd") + "(" + week + ")' ,");
            }
            builder = builder.Remove(builder.Length - 1, 1);
            builder.Append("from  (select [PackBy] ,convert(varchar(10),[PackOn],120) [PackOn] ,COUNT(1) as 'rcount'  from OrderPackRecord where [PackOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by [PackBy] ,convert(varchar(10),[PackOn],120)) as tbl1  group by [PackBy]");
            return ConvertJson(base.NSession.CreateSQLQuery(builder.ToString()).List<object[]>(), strData);
        }

        public ActionResult OrderCount()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult OrderCount(DateTime st, DateTime et, string a, string p, string i)
        {
            string str = this.SqlWhere(st, et, a, p, "");
            string queryString = string.Format("select Account,Count(Id),Platform,Sum(Amount),Min(CurrencyCode) from OrderType {0} group by Account,Platform", str);
            if (!string.IsNullOrEmpty(i))
            {
                queryString = queryString + " ,CurrencyCode";
            }
            IList<object[]> list = base.NSession.CreateQuery(queryString).List<object[]>();
            List<DDX.OrderManagementSystem.App.OrderCount> list2 = new List<DDX.OrderManagementSystem.App.OrderCount>();
            int num = 0;
            foreach (object[] objArray in list)
            {
                DDX.OrderManagementSystem.App.OrderCount item = new DDX.OrderManagementSystem.App.OrderCount
                {
                    Account = objArray[0].ToString(),
                    OCount = Convert.ToInt32(objArray[1]),
                    Platform = objArray[2].ToString(),
                    TotalAmount = Math.Round(Convert.ToDouble(objArray[3].ToString()), 2),
                    CurrencyCode = objArray[4].ToString()
                };
                num += Convert.ToInt32(objArray[1]);
                list2.Add(item);
            }
            List<object> list3 = new List<object> {
                new { OCount = num }
            };
            return base.Json(new
            {
                rows = from f in list2
                       orderby f.OCount descending
                       select f,
                footer = list3,
                total = list2.Count
            });
        }

        [HttpPost]
        public ActionResult OrderCountryData(DateTime st, DateTime et, string a, string p)
        {
            string str = this.SqlWhere(st, et, a, p, "");
            IList<object[]> list = base.NSession.CreateQuery(string.Format("select Country,COUNT(Id) from OrderType {0} group by Country", str)).List<object[]>();
            decimal num = Convert.ToDecimal(base.NSession.CreateQuery(string.Format("select COUNT(Id) from OrderType {0} ", str)).UniqueResult());
            List<ProportionData> list2 = new List<ProportionData>();
            foreach (object[] objArray in list)
            {
                ProportionData data = new ProportionData();
                data = new ProportionData
                {
                    Count = Convert.ToInt32(objArray[1]),
                    Key = objArray[0].ToString(),
                    Proportion = Math.Round(((Convert.ToDecimal(data.Count) / num) * 100), 2)
                };
                list2.Add(data);
            }
            List<object> list3 = new List<object> {
                new { Count = num }
            };
            return base.Json(new
            {
                rows = from f in list2
                       orderby f.Proportion descending
                       select f,
                footer = list3,
                total = list2.Count
            });
        }

        [HttpPost]
        public ActionResult OrderLeveData(DateTime st, DateTime et, string a, string p)
        {
            int count = 0;
            string str = this.SqlWhere(st, et, a, p, "");
            List<LeveData> list = new List<LeveData>();
            IList<OrderType> list2 = base.NSession.CreateQuery(string.Format("from OrderType " + str, new object[0])).List<OrderType>();
            double[,] numArray = new double[,] { { 0.0, 5.0 }, { 5.0, 10.0 }, { 10.0, 20.0 }, { 20.0, 50.0 }, { 50.0, 100.0 }, { 100.0, 0.0 } };
            for (int i = 0; i < (numArray.Length / 2); i++)
            {
                int num3 = 0;
                LeveData item = new LeveData();
                foreach (OrderType type in list2)
                {
                    if (numArray[i, 1] != Convert.ToDouble(0))
                    {
                        if ((type.Amount >= numArray[i, 0]) && (type.Amount < numArray[i, 1]))
                        {
                            num3++;
                        }
                    }
                    else if (type.Amount >= numArray[i, 0])
                    {
                        num3++;
                    }
                }
                count = list2.Count;
                if (count != 0)
                {
                    if (!(numArray[i, 1] == Convert.ToDouble(0)))
                    {
                        item.Platform = numArray[i, 0] + "-" + numArray[i, 1];
                    }
                    else
                    {
                        item.Platform = numArray[i, 0] + " 以上";
                    }
                    item.Account = num3;
                    item.OCount = Math.Round((decimal)((Convert.ToDecimal(num3) / count) * 100M), 2);
                    list.Add(item);
                }
            }
            List<object> list3 = new List<object> {
                new { Account = count }
            };
            return base.Json(new { rows = list, footer = list3, total = list.Count });
        }

        public ActionResult OrderSendInfo()
        {
            return base.View();
        }

        public ActionResult OrderSendStatistcs(int Id)
        {
            IList<OrderType> list = new List<OrderType>();
            DateTime now = DateTime.Now;
            switch (Id)
            {
                case 1:
                    list = base.NSession.CreateQuery("from OrderType where Status='已处理' and IsOutOfStock=0 and CreateOn<'" + now.AddDays(-1.0).ToString("yyyy/MM/dd HH:mm:ss") + "'").List<OrderType>();
                    break;

                case 2:
                    list = base.NSession.CreateQuery("from OrderType where Status='待包装' and IsOutOfStock=0 and CreateOn<'" + now.AddHours(-12.0).ToString("yyyy/MM/dd HH:mm:ss") + "'").List<OrderType>();
                    break;

                case 3:
                    list = base.NSession.CreateQuery("from OrderType where Status='待发货' and IsOutOfStock=0 and CreateOn<'" + now.AddHours(-12.0).ToString("yyyy/MM/dd HH:mm:ss") + "'").List<OrderType>();
                    break;
            }
            return base.Json(new
            {
                rows = from f in list
                       orderby f.CreateOn
                       select f,
                total = list.Count
            });
        }

        public ActionResult OutCount()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult OutCount(DateTime st, DateTime et, string a, string p)
        {
            List<ProductData> list = this.GetOutCount(st, et, a, p);
            List<object> list2 = new List<object>();
            return base.Json(new
            {
                rows = from f in list
                       orderby f.Qty descending
                       select f,
                total = list.Count
            });
        }

        private string pa(string p, string a)
        {
            string str = "";
            if (p != "ALL")
            {
                str = str + " and Platform='" + p + "'";
            }
            if (a != "ALL")
            {
                str = str + " and Account='" + a + "'";
            }
            return str;
        }

        public ActionResult PackScore()
        {
            return base.View();
        }

        [HttpPost]
        public string PeiCount(DateTime st, DateTime et)
        {
            int num2;
            List<string> list = new List<string>();
            StringBuilder builder = new StringBuilder();
            list.Add("人员");
            builder.Append("select [PeiBy] as '扫描人',");
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                list.Add(time.ToString("MMdd"));
                builder.Append("SUM(case  when convert(varchar(10),[CreateOn],120)='" + time.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + time.ToString("MM.dd") + "(" + week + ")' ,");
            }
            builder = builder.Remove(builder.Length - 1, 1);
            builder.Append("from  (select [PeiBy] ,convert(varchar(10),[CreateOn],120) [CreateOn] ,COUNT(1) as 'rcount'  from OrderPeiRecord where [CreateOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by [PeiBy] ,convert(varchar(10),[CreateOn],120)) as tbl1  group by [PeiBy]");
            IList<object[]> list2 = base.NSession.CreateSQLQuery(builder.ToString()).List<object[]>();
            StringBuilder builder2 = new StringBuilder();
            builder2.Append("[");
            int num = 0;
            while (num < list2.Count)
            {
                builder2.Append("{");
                num2 = 0;
                while (num2 < list.Count)
                {
                    builder2.Append("\"");
                    builder2.Append(list[num2]);
                    builder2.Append("\":\"");
                    builder2.Append(list2[num][num2]);
                    builder2.Append("\",");
                    num2++;
                }
                builder2.Remove(builder2.Length - 1, 1);
                builder2.Append("},");
                num++;
            }
            builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("]");
            StringBuilder builder3 = new StringBuilder();
            builder3.Append("[{");
            for (num2 = 1; num2 < list.Count; num2++)
            {
                int num3 = 0;
                builder3.Append("\"");
                builder3.Append(list[num2]);
                builder3.Append("\":\"");
                for (num = 0; num < list2.Count; num++)
                {
                    num3 += Convert.ToInt32(list2[num][num2]);
                }
                builder3.Append(num3.ToString());
                builder3.Append("\",");
            }
            builder3.Remove(builder3.Length - 1, 1);
            builder3.Append("}]");
            return string.Concat(new object[] { "{\"total\":", list2.Count, ",\"rows\":", builder2.ToString(), ",\"footer\":", builder3.ToString(), "}" });
        }

        public ActionResult PurchaseInfo()
        {
            return base.View();
        }

        public ActionResult PurchaseStatistcs(int Id)
        {
            IList<PurchasePlanType> list = new List<PurchasePlanType>();
            if (Id == 1)
            {
                list = base.NSession.CreateQuery("from PurchasePlanType where Status in ('已采购') and  BuyOn <=dateadd(day,-3,GETDATE())").List<PurchasePlanType>();
            }
            else
            {
                list = base.NSession.CreateQuery("from PurchasePlanType where Status in ('已发货','部分发货') and  BuyOn <=dateadd(day,-5,GETDATE())").List<PurchasePlanType>();
            }
            List<object> list2 = new List<object>();
            return base.Json(new
            {
                rows = from f in list
                       orderby f.BuyOn
                       select f,
                total = list.Count
            });
        }

        public ActionResult QueCount()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult SetSKU(string sku, string memo)
        {
            if (!string.IsNullOrEmpty(memo))
            {
                QuestionSKUType questionSku = new QuestionSKUType();
                List<QuestionSKUType> list =
                    NSession.CreateQuery("from QuestionSKUType where SKU='" + sku + "'").List<QuestionSKUType>().ToList();
                if (list.Count > 0)
                {
                    questionSku = list[0];
                    questionSku.Memo += " --分割-- " + memo;
                    NSession.Update(questionSku);
                    NSession.Flush();
                }
                else
                {
                    questionSku.SKU = sku;
                    questionSku.Memo = memo;
                    questionSku.CreateBy = GetCurrentAccount().Realname;
                    questionSku.CreateOn = DateTime.Now;
                    NSession.Save(questionSku);
                    NSession.Flush();
                }
            }
            else
            {
                NSession.Delete("from QuestionSKUType where SKU='" + sku + "'");
                NSession.Flush();
            }
            return Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 缺货库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="sort"></param>
        /// <param name="area"></param>
        /// <param name="s"></param>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="ss"></param>
        /// <param name="c"></param>
        /// <param name="iscon"></param>
        /// <param name="isneed"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QueData(string order, string sort, string area, string s, string p, string a, string ss, string c, int page, int rows,int iscon = 2, int isneed = 1)
        {
            string str = "";
            if (!(string.IsNullOrEmpty(order) || string.IsNullOrEmpty(sort)))
            {
                str = " order by " + sort + " " + order;
            }
            s = SqlWhere(a, p, s, "");
            s = s.Replace("SKU", "OP.SKU");
            if (s.Length > 0)
            {
                s = " and " + s;
            }
            List<QuestionSKUType> listsku = NSession.CreateQuery("from QuestionSKUType").List<QuestionSKUType>().ToList();
            string str2 = "";
            string str3 = "";
            string wsql = "";
            if (area != "0")
            {
                if (area == "1")
                {
                    s += "and O.Account not like '%yw%'";
                    wsql = " Wid=1 and ";
                }
                else if (area == "2")
                {
                    s += "and O.Account  like '%yw%'";
                    wsql = " Wid=3 and ";
                }
                else
                {
                    wsql = " (Wid=1 or Wid=3)  and ";
                }

            }
            if (isneed != 2)
            {
                if (isneed != 0)
                    str2 += "and NeedQty > 0";
                else
                    str2 += "and NeedQty <= 0";
            }
            if (iscon != 2)
            {
                if (iscon == 1)
                    str2 += " and SKU in (Select SKU from QuestionSKU)";
                else
                    str2 += " and SKU not in (Select SKU from QuestionSKU)";
            }
            if (!string.IsNullOrEmpty(ss))
            {
                str2 += " and SKU in (Select SKU from SuppliersProduct Where SuppliersName like '%" + ss + "%')";
            }

            if (!string.IsNullOrEmpty(c) && c != "0")
            {

                IList<object> list = base.NSession.CreateSQLQuery("with a as(\r\nselect * from ProductCategory where ID=" + c + "\r\nunion all\r\nselect x.* from ProductCategory x,a\r\nwhere x.ParentId=a.Id)\r\nselect Name from a").List<object>();
                foreach (object obj2 in list)
                {
                    object obj3 = str3;
                    str3 = string.Concat(new object[] { obj3, "'", obj2, "'," });
                }
                if (str3.Length > 0)
                {
                    str3 = str3.Trim(new char[] { ',' });
                }

                str2 += " And SKU in (select SKU from Products where Category in (" + str3 + "))";
            }
            IList<object[]> list2 = base.NSession.CreateSQLQuery(string.Format("select * from (\r\nselect *,(Qty-BuyQty-UnPeiQty) as NeedQty,(SQty-BuyQty-UnPeiQty) as SNeedQty from( select * ,isnull((select sum(NowQty) from WarehouseStockData Where " + wsql + "  NowQty>0 and SKU=tbl1.SKU ),0) as UnPeiQty,\r\nisnull((select SUM(Qty) from   Orders O left join OrderProducts OP On O.Id=OP.OId where O.IsOutOfStock=1 and O.Enabled=1 and O.Status in ('已处理','待拣货') and O.IsFBA=0 and OP.IsQue=1 and OP.SKU=tbl1.SKU),0) as 'SQty'\r\n from (\r\nselect OP.SKU,SUM(OP.Qty) as Qty,MIN(O.CreateOn) as MinDate,MAX(O.CreateOn) as LastDate,P.Standard,max(P.Caigou1) as Caigou1,\r\n(select isnull(SUM(Qty-DaoQty),0) from PurchasePlan \r\nwhere Status in('已采购','已发货','部分到货') and (ProcurementModel<>'海外仓采购' or ProcurementModel is null) and  SKU=OP.SKU ) as BuyQty \r\n,isnull(max(P.PicUrl),'')  as 'ttt' from Orders O left join OrderProducts OP On O.Id=OP.OId \r\nleft join Products P on OP.SKU=P.SKU \r\nwhere  O.Enabled=1 and O.IsStop=0 and O.Status in ('已处理','待拣货') and OP.SKU is not null {0} group by OP.SKU,P.Standard)\r\n as tbl1 ) as tbl2 ) as tbl3 where  (Qty-UnPeiQty)>0 {1} {2}", s, str2, str)).List<object[]>();
            /*
             * select SUM(Qty) from WarehouseStock where WarehouseStock.SKU=tbl1.SKU),0) as UnPeiQty
             * 由固定库存数量调整为实时获取仓库明细累计
             *  (select sum(NowQty) from WarehouseStockData  Where NowQty>0 and SKU=tbl1.SKU ),0) as UnPeiQty
            说明：
            SQty:已处理、待拣货订单内缺货的订单商品数量
            BuyQty：已采购、已发货、部分到货的采购商品数量 - 指定小包采购 -> ProcurementModel<>'海外仓采购'
            Qty: 已处理、待拣货的订单商品数量[缺货(预)]
            UnPeiQty：商品库存批次总和-[需判断仓库****待确认待处理问题]->指定判断宁波2或义乌3仓库
            ---
            NeedQty：订单商品数量-采购商品数量-商品库存数量->采购(预)
            SNeedQty：订单缺货商品数量-采购商品数量-商品库存数量->采购(扫)?模块页面内未使用
             */
            List<DDX.OrderManagementSystem.App.QueCount> data = new List<DDX.OrderManagementSystem.App.QueCount>();
            foreach (object[] objArray in list2)
            {
                DDX.OrderManagementSystem.App.QueCount item = new DDX.OrderManagementSystem.App.QueCount
                {
                    SKU = objArray[0].ToString(),
                    Standard = Utilities.ToString(objArray[4]),
                    Caigou = Utilities.ToString(objArray[5]),
                    Qty = Utilities.ToInt(objArray[1]),
                    MinDate = Convert.ToDateTime(objArray[2]),
                    LastDate = Convert.ToDateTime(objArray[3]),
                    BuyQty = Utilities.ToInt(objArray[6]),
                    NeedQty = Utilities.ToInt(objArray[10]),
                    SNeedQty = Utilities.ToInt(objArray[11]),
                    UnPeiQty = Utilities.ToInt(objArray[8]),
                    SQty = Utilities.ToInt(objArray[9]),
                    Pic = Utilities.ToString(objArray[7])
                };
                //if (!(objArray[3] is DBNull) && (objArray[3] != null))
                //{
                //    item.Standard = objArray[3].ToString();
                //}
                if (!((objArray[2] is DBNull) || (objArray[2] == null)))
                {
                    item.Field1 = objArray[2].ToString();
                }
                if (item.NeedQty <= 0)
                {
                    item.NeedQty = 0;
                }
                if (item.SNeedQty <= 0)
                {
                    item.SNeedQty = 0;
                }
                QuestionSKUType sku = listsku.Find(x => x.SKU == item.SKU);
                if (sku != null)
                    item.Memo = sku.Memo;

                data.Add(item);
            }
            var query = from t in data
                        select t;
            query = query.Skip<DDX.OrderManagementSystem.App.QueCount>((page - 1) * rows).Take<DDX.OrderManagementSystem.App.QueCount>(rows).ToList();
            //     return base.Json(data);
            return Json(new { total = data.Count, rows = query });
        }

        public ActionResult RefundAmountCount()
        {
            return base.View();
        }

        public ActionResult ScanCount()
        {
            return base.View();
        }

        [HttpPost]
        public string ScanCount(DateTime st, DateTime et)
        {
            int num2;
            List<string> list = new List<string>();
            StringBuilder builder = new StringBuilder();
            list.Add("人员");
            builder.Append("select [ScanningBy] as '扫描人',");
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                list.Add(time.ToString("MMdd"));
                builder.Append("SUM(case  when convert(varchar(10),[ScanningOn],120)='" + time.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + time.ToString("MM.dd") + "(" + week + ")' ,");
            }
            builder = builder.Remove(builder.Length - 1, 1);
            builder.Append("from  (select [ScanningBy] ,convert(varchar(10),[ScanningOn],120) [ScanningOn] ,COUNT(1) as 'rcount'  from Orders where Status='已发货' and [ScanningOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by [ScanningBy] ,convert(varchar(10),[ScanningOn],120)) as tbl1  group by [ScanningBy]");
            IList<object[]> list2 = base.NSession.CreateSQLQuery(builder.ToString()).List<object[]>();
            StringBuilder builder2 = new StringBuilder();
            builder2.Append("[");
            int num = 0;
            while (num < list2.Count)
            {
                builder2.Append("{");
                num2 = 0;
                while (num2 < list.Count)
                {
                    builder2.Append("\"");
                    builder2.Append(list[num2]);
                    builder2.Append("\":\"");
                    builder2.Append(list2[num][num2]);
                    builder2.Append("\",");
                    num2++;
                }
                builder2.Remove(builder2.Length - 1, 1);
                builder2.Append("},");
                num++;
            }
            builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("]");
            StringBuilder builder3 = new StringBuilder();
            builder3.Append("[{");
            for (num2 = 1; num2 < list.Count; num2++)
            {
                int num3 = 0;
                builder3.Append("\"");
                builder3.Append(list[num2]);
                builder3.Append("\":\"");
                for (num = 0; num < list2.Count; num++)
                {
                    num3 += Convert.ToInt32(list2[num][num2]);
                }
                builder3.Append(num3.ToString());
                builder3.Append("\",");
            }
            builder3.Remove(builder3.Length - 1, 1);
            builder3.Append("}]");
            return string.Concat(new object[] { "{\"total\":", list2.Count, ",\"rows\":", builder2.ToString(), ",\"footer\":", builder3.ToString(), "}" });
        }

        public ActionResult SellCount()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult SellCount(DateTime st, DateTime et, string a, string p, string s, int page, int rows)
        {
            List<ProductData> list = this.GetSellCount(st, et, a, p, s, page, rows);
            List<object> list2 = new List<object>();
            string str = this.SqlWhere(st, et, a, p, s);
            if (str.Length > 3)
            {
                str = str + " and SKU is not null ";
            }
            else
            {
                str = " where SKU is not null ";
            }
            object obj2 = base.NSession.CreateSQLQuery(string.Format("select COUNT(1) from ( select SKU from OrderProducts right join Orders on OId=Orders.Id   {0} group by SKU ) as tbl", str)).UniqueResult();
            return base.Json(new
            {
                rows = from f in list
                       orderby f.Qty descending
                       select f,
                total = obj2
            });
        }

        public ActionResult SendDays()
        {
            return base.View();
        }

        [HttpPost]
        public ActionResult SendDays(DateTime st, DateTime et, string a, string p)
        {
            int count = 0;
            string str = this.SqlWhere(st, et, a, p, "");
            List<LeveDays> list = new List<LeveDays>();
            int nb = 0;
            int yw = 0;
            IList<OrderType> list2 = base.NSession.CreateQuery(string.Format("from OrderType " + str + " and Account <> 'su-smt' and Status='已发货'", new object[0])).List<OrderType>();
            int[,] numArray = new int[,] { { 0, 1 }, { 1, 3 }, { 3, 5 }, { 5, 7 }, { 7, 9 }, { 9, 11 }, { 11, 0 } };
            for (int i = 0; i < (numArray.Length / 2); i++)
            {
                int num3 = 0;
                int num4 = 0;
                LeveDays item = new LeveDays();
                foreach (OrderType type in list2)
                {
                    TimeSpan span;
                    item.Account = type.Account;
                    if (item.Account.IndexOf("yw") != -1)
                    {
                        item.Area = "义乌";
                    }
                    else
                    {
                        item.Area = "宁波";
                    }
                    if (numArray[i, 1] != 0)
                    {
                        span = (TimeSpan)(type.ScanningOn - type.CreateOn);

                        if ((span.Days >= numArray[i, 0]) && ((span = (TimeSpan)(type.ScanningOn - type.CreateOn)).Days < numArray[i, 1]))
                        {
                            if (item.Area == "宁波")
                            {
                                nb += (span.Days + 1);
                                num3++;
                            }
                            else
                            {
                                yw += (span.Days + 1);
                                num4++;
                            }

                        }
                    }
                    else
                    {
                        span = (TimeSpan)(type.ScanningOn - type.CreateOn);
                        if (span.Days >= numArray[i, 0])
                        {
                            if (item.Area == "宁波")
                            {
                                nb += (span.Days + 1);
                                num3++;
                            }
                            else
                            {
                                yw += (span.Days + 1);
                                num4++;
                            }
                        }
                    }
                }
                count = list2.Where(x => x.Account.IndexOf("yw") == -1).Count();
                if (numArray[i, 1] != Convert.ToDouble(0))
                {
                    item.Platform = numArray[i, 0] + "-" + numArray[i, 1];
                }
                else
                {
                    item.Platform = numArray[i, 0] + " 以上";
                }
                item.OCount = num3;
                item.Rate = Math.Round((decimal)((Convert.ToDecimal(num3) / count) * 100M), 2);
                item.Area = "宁波";
                list.Add(item);
                item = new LeveDays();
                count = list2.Where(x => x.Account.IndexOf("yw") != -1).Count();
                if (numArray[i, 1] != Convert.ToDouble(0))
                {
                    item.Platform = numArray[i, 0] + "-" + numArray[i, 1];
                }
                else
                {
                    item.Platform = numArray[i, 0] + " 以上";
                }
                item.OCount = num4;
                item.Rate = Math.Round((decimal)((Convert.ToDecimal(num4) / count) * 100M), 2);
                item.Area = "义乌";
                list.Add(item);
            }
            LeveDays item3 = new LeveDays();
            item3.Platform = "平均发货天数";
            item3.OCount = list2.Where(x => x.Account.IndexOf("yw") == -1).Count();
            item3.Rate = Math.Round(nb / item3.OCount, 2);
            item3.Area = "宁波";
            list.Add(item3);

            LeveDays item4 = new LeveDays();
            item4.Platform = "平均发货天数";
            item4.OCount = list2.Where(x => x.Account.IndexOf("yw") != -1).Count();
            item4.Rate = Math.Round(yw / item4.OCount, 2);
            item4.Area = "义乌";
            list.Add(item4);


            List<object> list3 = new List<object> {
                new { Account = count }
            };
            return base.Json(new { rows = list, footer = list3, total = list.Count });
        }

        public IList<Sores> SoreList(DateTime st, DateTime et)
        {
            IList<Sores> list = new List<Sores>();
            IList<object[]> list2 = base.NSession.CreateSQLQuery("select COUNT(Id) as Qcount, PackBy as PackBy,SUM(PackCoefficient) as PackCoefficient from OrderPackRecord where [PackOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by [PackBy]").List<object[]>();
            foreach (object[] objArray in list2)
            {
                object obj2 = base.NSession.CreateQuery("select SUM(Sore) from SoresAddType where Worker='" + objArray[1].ToString() + "' and WorkDate between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59'").UniqueResult();
                decimal num = Convert.ToDecimal(Convert.ToDouble(objArray[2]).ToString("f1")) / Convert.ToDecimal(objArray[0]);
                decimal num2 = Convert.ToDecimal(Convert.ToDouble(objArray[2]).ToString("f1"));
                decimal num3 = Convert.ToDecimal(obj2);
                decimal num4 = num3 + num2;
                Sores item = new Sores
                {
                    PackBy = objArray[1].ToString(),
                    PackSores = num2,
                    Qcount = Convert.ToDecimal(objArray[0]),
                    Avg = Convert.ToDecimal(num.ToString("f1")),
                    Sore = num3,
                    TotalSores = num4
                };
                list.Add(item);
            }
            return list;
        }



        private static string SqlWhere(string a, string p, string ss, string sqlWhere)
        {
            if (!(string.IsNullOrEmpty(p) || !(p != "ALL")))
            {
                sqlWhere = sqlWhere + " Platform='" + p + "' and";
            }
            if (!(string.IsNullOrEmpty(a) || !(a != "ALL")))
            {
                sqlWhere = sqlWhere + " Account='" + a + "' and";
            }
            if (!(string.IsNullOrEmpty(ss) || !(ss != "ALL")))
            {
                sqlWhere = sqlWhere + " SKU like '%" + ss + "%' and";
            }
            if (sqlWhere.Length > 4)
            {
                sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 3);
            }
            return sqlWhere;
        }

        private string SqlWhere(DateTime st, DateTime et, string a, string p, string ss = "")
        {
            string sqlWhere = " where Status<>'待处理' and MId=0 and Enabled=1 and CreateOn between '" + st.ToString("yyyy/MM/dd 00:00:00") + "' and '" + et.AddDays(1.0).ToString("yyyy/MM/dd 00:00:00") + "' and";
            return SqlWhere(a, p, ss, sqlWhere);
        }

        private string SqlLog(int year, int month)
        {
            string sqlLog = " where DATEPART(MONTH,p.OperationOn)=month and DATEPART(YEAR,p.OperationOn)=year  and";
            return sqlLog;
        }

        private string sub(string id)
        {
            return id.Substring(0, id.IndexOf("$"));
        }

        private string subid(string id)
        {
            return id.Substring(id.IndexOf("$") + 1);
        }

        public string Where(DateTime st, DateTime et, string p, string a)
        {
            string str = "where CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59'";
            if (p != "ALL")
            {
                str = str + " and Platform='" + p + "'";
            }
            if (a != "ALL")
            {
                str = str + " and Account='" + a + "'";
            }
            return str;
        }

        public JsonResult ExportUnprocessedOrderNB()
        {
            try
            {
                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandText = "select AccountName from Account where AccountName not like '%yw%'";

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                string[] str = new string[ds.Tables[0].Rows.Count];//把DataSet中的数据转换为一维数组

                for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                {
                    str[row] = "'" + ds.Tables[0].Rows[row]["AccountName"].ToString() + "'";
                }


                /* string sql = "select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  O.Status='已处理' and O.Enabled=1 and Account=" + str[0].ToString() + " group by OP.SKU";*/
                string sql = "";
                for (int r = 0; r < 3; r++)
                {
                    if (r == 0)
                    {
                        sql = "select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '0' and DATEDIFF(day,GenerateOn,GETDATE()) <= '6' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '0' and DATEDIFF(day,GenerateOn,GETDATE()) <= '6' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                    else if (r == 1)
                    {
                        sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '7' and DATEDIFF(day,GenerateOn,GETDATE()) <= '12' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '7' and DATEDIFF(day,GenerateOn,GETDATE()) <= '12' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                    else if (r == 2)
                    {
                        sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '13' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '13' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet1(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });

        }

        public JsonResult ExportUnprocessedOrderYW()
        {
            try
            {
                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandText = "select AccountName from Account where AccountName like '%yw%'";

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                string[] str = new string[ds.Tables[0].Rows.Count];//把DataSet中的数据转换为一维数组

                for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                {
                    str[row] = "'" + ds.Tables[0].Rows[row]["AccountName"].ToString() + "'";
                }


                string sql = "";
                for (int r = 0; r < 3; r++)
                {
                    if (r == 0)
                    {
                        sql = "select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '0' and DATEDIFF(day,GenerateOn,GETDATE()) <= '6' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '0' and DATEDIFF(day,GenerateOn,GETDATE()) <= '6' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                    else if (r == 1)
                    {
                        sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '7' and DATEDIFF(day,GenerateOn,GETDATE()) <= '12' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '7' and DATEDIFF(day,GenerateOn,GETDATE()) <= '12' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                    else if (r == 2)
                    {
                        sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end  as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[0].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '13' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sql = sql + " Union select MIN(Account) as'账号',OP.SKU as '物品SKU',SUM(OP.Qty) as '数量',MIN(O.GenerateOn) as '最小缺货时间',case when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=0 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=6 then '0-6' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=7 and abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))<=12 then '7-12' when abs(datediff(day,GETDATE(),MIN(O.GenerateOn)))>=13  then '13 以上' end as'时间区间（天）' from Orders O left join OrderProducts OP on O.Id=OP.OId  Where  Account=" + str[i].ToString() + "and DATEDIFF(day,GenerateOn,GETDATE())>= '13' and O.Status='已处理' and O.Enabled=1  group by OP.SKU";
                        }
                    }
                }
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet1(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });

        }
    }
}
