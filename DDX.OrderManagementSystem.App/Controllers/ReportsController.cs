using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DDX.OrderManagementSystem.Domain;
using System.Data;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ReportsController : BaseController
    {
        private static string ConvertJson(IList<object[]> objectses, List<string> strData)
        {
            int num;
            int num2;
            int num3;
            int l = 0;
            IList<object[]> list = new List<object[]>();
            for (num = 0; num < objectses.Count; num++)
            {
                object[] item = new object[strData.Count<string>()];
                num2 = 0;
                num3 = 0;
                while (num3 < objectses[num].Length)
                {
                    num2 += Utilities.ToInt(objectses[num][num3]);
                    item[num3] = objectses[num][num3];
                    num3++;
                }
                //item[item.Length - 1] = num2;
                l = item.Length;
                list.Add(item);
            }
            List<object[]> list2 = (from a in list
                                    orderby a[l - 1] descending
                                    select a).ToList<object[]>();
            for (num = 0; num < list2.Count; num++)
            {
                if (list2[num][0] == null)
                {
                    list2.Insert(list2.Count, list2[num].ToArray<object>());
                    list2.RemoveAt(num);
                    break;
                }
            }
            list = list2;
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            num = 0;
            while (num < list.Count)
            {
                builder.Append("{");
                num3 = 0;
                while (num3 < strData.Count)
                {
                    builder.Append("\"");
                    builder.Append(strData[num3]);
                    builder.Append("\":\"");
                    builder.Append(list[num][num3]);
                    builder.Append("\",");
                    num3++;
                }
                builder.Remove(builder.Length - 1, 1);
                builder.Append("},");
                num++;
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]");
            StringBuilder builder2 = new StringBuilder();
            builder2.Append("[{");
            for (num3 = 1; num3 < strData.Count; num3++)
            {
                num2 = 0;
                builder2.Append("\"");
                builder2.Append(strData[num3]);
                builder2.Append("\":\"");
                for (num = 0; num < list.Count; num++)
                {
                    num2 += Convert.ToInt32(list[num][num3]);
                }
                builder2.Append(num2.ToString());
                builder2.Append("\",");
            }
            builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("}]");
            return string.Concat(new object[] { "{\"total\":", list.Count, ",\"rows\":", builder.ToString(), ",\"footer\":", builder2.ToString(), "}" });
        }

        public ActionResult DayOrder()
        {
            return base.View();
        }

        public ActionResult DaySale()
        {
            return base.View();
        }

        public ActionResult DealReport()
        {
            return base.View();
        }


        public ActionResult MonthlySalesGrowthByYW()
        {
            return base.View();
        }
        public ActionResult MonthlySalesGrowthByNB()
        {
            return base.View();
        }


        public ActionResult GetDealReport(string a, string p, DateTime d)
        {
            if (a == null)
                return null;
            string sqlWhere = " where  O.Enabled=1 And O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt'";

            if (((!string.IsNullOrEmpty(p) && (p != "ALL")) && (p != "0")) && (p != "===请选择==="))
            {
                sqlWhere += " and O.Platform='" + p + "' ";
            }
            if (((!string.IsNullOrEmpty(a) && (a != "ALL")) && (a != "0")) && (a != "===请选择==="))
            {
                sqlWhere += " and O.Account in ('" + a.Replace(",", "','") + "')";
            }
            string sqlWhere2 = sqlWhere + " and O.GenerateOn  between '" + d.AddMonths(-1).ToString("yyyy-MM") + "-01' and '" + d.ToString("yyyy-MM") + "-01'";
            sqlWhere += " and O.GenerateOn  between '" + d.ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01'";


            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();

            string sql = "select Account,O.CurrencyCode,SUM(Amount) as '销售额',COUNT(O.Id) as '订单数',SUM(Amount)/SUM(OP.TQty) as '客单价' from Orders O left join (select OId,SUM(Qty) as TQty from OrderProducts  group by OId  ) OP on OP.OId=O.Id " + sqlWhere + " group by Account,CurrencyCode ";

            string sql2 = "select Account,O.CurrencyCode,SUM(Amount) as '销售额',COUNT(O.Id) as '订单数',SUM(Amount)/SUM(OP.TQty) as '客单价' from Orders O left join (select OId,SUM(Qty) as TQty from OrderProducts  group by OId  ) OP on OP.OId=O.Id " + sqlWhere2 + " group by Account,CurrencyCode ";

            List<MonthlySalesModel> list = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list2 = new List<MonthlySalesModel>();
            List<MonthlySalesCountModel> result = new List<MonthlySalesCountModel>();
            List<MonthlySalesCountModel> resultfooter = new List<MonthlySalesCountModel>();

            List<object[]> objectses = NSession.CreateSQLQuery(sql).List<object[]>().ToList();
            List<object[]> objectses2 = NSession.CreateSQLQuery(sql2).List<object[]>().ToList();
            ///本月的数据
            foreach (object[] objectse in objectses)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.CurrencyCode = objectse[1].ToString();
                model.Amount = objectse[2] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[2]), currencies);
                model.OrderCount = objectse[3] == null ? 0 : Convert.ToInt32(objectse[3]);
                model.UnitPrice = objectse[4] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[4]), currencies);
                list.Add(model);
            }
            ///上月的数据
            foreach (object[] objectse in objectses2)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.CurrencyCode = objectse[1].ToString();
                model.Amount = objectse[2] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[2]), currencies);
                model.OrderCount = objectse[3] == null ? 0 : Convert.ToInt32(objectse[3]);
                model.UnitPrice = objectse[4] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[4]), currencies);
                list2.Add(model);
            }
            foreach (MonthlySalesModel monthlySalesModel in list)
            {
                MonthlySalesCountModel monthlySalesCountModel = new MonthlySalesCountModel();
                MonthlySalesModel monthlySalesModel2 = list2.Find(x => x.Account == monthlySalesModel.Account);
                monthlySalesCountModel.Account = monthlySalesModel.Account;
                monthlySalesCountModel.A = monthlySalesModel.CurrencyCode;
                monthlySalesCountModel.B = monthlySalesModel2 == null ? "0" : monthlySalesModel2.OrderCount.ToString();
                monthlySalesCountModel.C = monthlySalesModel.OrderCount.ToString();
                monthlySalesCountModel.D = Math.Round((Convert.ToDouble(monthlySalesCountModel.C) - Convert.ToDouble(monthlySalesCountModel.B)) / Convert.ToDouble(monthlySalesCountModel.B) * 100, 2).ToString() + "%";

                monthlySalesCountModel.E = monthlySalesModel2 == null ? "0" : monthlySalesModel2.Amount.ToString();
                monthlySalesCountModel.F = monthlySalesModel.Amount.ToString();
                monthlySalesCountModel.G = Math.Round((Convert.ToDouble(monthlySalesCountModel.F) - Convert.ToDouble(monthlySalesCountModel.E)) / Convert.ToDouble(monthlySalesCountModel.E) * 100, 2).ToString() + "%";

                monthlySalesCountModel.H = monthlySalesModel2 == null ? "0" : monthlySalesModel2.UnitPrice.ToString();
                monthlySalesCountModel.I = monthlySalesModel.UnitPrice.ToString();
                monthlySalesCountModel.J = Math.Round((Convert.ToDouble(monthlySalesCountModel.I) - Convert.ToDouble(monthlySalesCountModel.H)) / Convert.ToDouble(monthlySalesCountModel.H) * 100, 2).ToString() + "%";

                result.Add(monthlySalesCountModel);
            }

            MonthlySalesCountModel footmon = new MonthlySalesCountModel
                                                 {
                                                     Account = "本组",
                                                     B = result.Sum(x => Utilities.ToDouble(x.B)).ToString(),
                                                     C = result.Sum(x => Utilities.ToDouble(x.C)).ToString(),

                                                     E = result.Sum(x => Utilities.ToDouble(x.E)).ToString(),
                                                     F = result.Sum(x => Utilities.ToDouble(x.F)).ToString(),

                                                     H = result.Sum(x => Utilities.ToDouble(x.H)).ToString(),
                                                     I = result.Sum(x => Utilities.ToDouble(x.I)).ToString()

                                                 };
            footmon.D = Math.Round((Convert.ToDouble(footmon.C) - Convert.ToDouble(footmon.B)) / Convert.ToDouble(footmon.B) * 100, 2).ToString() + "%";
            footmon.G = Math.Round((Convert.ToDouble(footmon.F) - Convert.ToDouble(footmon.E)) / Convert.ToDouble(footmon.E) * 100, 2).ToString() + "%";
            footmon.J = Math.Round((Convert.ToDouble(footmon.I) - Convert.ToDouble(footmon.H)) / Convert.ToDouble(footmon.H) * 100, 2).ToString() + "%";
            resultfooter.Add(footmon);


            return Json(new { rows = result, total = result.Count, footer = resultfooter });
        }

        public ActionResult GetDealReport2(string a, string p, DateTime d, string u)
        {
            if (a == null)
                return null;
            string[] cels = u.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string sqlWhere = " where  O.Enabled=1 And O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt'";

            if (((!string.IsNullOrEmpty(p) && (p != "ALL")) && (p != "0")) && (p != "===请选择==="))
            {
                sqlWhere += " and O.Platform='" + p + "' ";
            }
            if (((!string.IsNullOrEmpty(a) && (a != "ALL")) && (a != "0")) && (a != "===请选择==="))
            {
                sqlWhere += " and O.Account in ('" + a.Replace(",", "','") + "')";
            }

            string sqlWhere2 = sqlWhere + " and O.GenerateOn  between '" + d.AddMonths(-1).ToString("yyyy-MM") + "-01' and '" + d.ToString("yyyy-MM") + "-01'";
            sqlWhere += " and O.GenerateOn  between '" + d.ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01'";


            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();

            string sql = "select Max(Account),O.CurrencyCode,SUM(Amount) as '销售额',COUNT(O.Id) as '订单数',SUM(Amount)/SUM(OP.TQty) as '客单价' from Orders O left join (select OId,SUM(Qty) as TQty from OrderProducts  group by OId  ) OP on OP.OId=O.Id  " + sqlWhere + " group by CurrencyCode ";

            string sql2 = "select Max(Account),O.CurrencyCode,SUM(Amount) as '销售额',COUNT(O.Id) as '订单数',SUM(Amount)/SUM(OP.TQty) as '客单价' from Orders O left join (select OId,SUM(Qty) as TQty from OrderProducts  group by OId  ) OP on OP.OId=O.Id  " + sqlWhere2 + " group by CurrencyCode ";

            List<MonthlySalesModel> list = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list2 = new List<MonthlySalesModel>();
            List<MonthlySalesCountModel> result = new List<MonthlySalesCountModel>();
            List<MonthlySalesCountModel> resultfooter = new List<MonthlySalesCountModel>();

            List<object[]> objectses = NSession.CreateSQLQuery(sql).List<object[]>().ToList();
            List<object[]> objectses2 = NSession.CreateSQLQuery(sql2).List<object[]>().ToList();
            ///本月的数据
            foreach (object[] objectse in objectses)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.CurrencyCode = objectse[1].ToString();
                model.Amount = objectse[2] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[2]), currencies);
                model.OrderCount = objectse[3] == null ? 0 : Convert.ToInt32(objectse[3]);
                model.UnitPrice = objectse[4] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[4]), currencies);
                list.Add(model);
            }
            ///上月的数据
            foreach (object[] objectse in objectses2)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.CurrencyCode = objectse[1].ToString();
                model.Amount = objectse[2] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[2]), currencies);
                model.OrderCount = objectse[3] == null ? 0 : Convert.ToInt32(objectse[3]);
                model.UnitPrice = objectse[4] == null ? 0 : GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[4]), currencies);
                list2.Add(model);
            }
            foreach (MonthlySalesModel monthlySalesModel in list)
            {
                MonthlySalesCountModel monthlySalesCountModel = new MonthlySalesCountModel();
                MonthlySalesCountModel monthlySalesCountModel2 = new MonthlySalesCountModel();
                MonthlySalesModel monthlySalesModel2 = list2.Find(x => x.Account == monthlySalesModel.Account);
                //monthlySalesCountModel.Account = monthlySalesCountModel2.Account = monthlySalesModel.Account;
                monthlySalesCountModel.A = monthlySalesCountModel2.A = monthlySalesModel.CurrencyCode;
                monthlySalesCountModel.Account = "人均销售额";


                monthlySalesCountModel.B = monthlySalesModel2 == null ? "0" : monthlySalesModel2.Amount.ToString();
                monthlySalesCountModel.C = monthlySalesModel.Amount.ToString();
                monthlySalesCountModel.B = Math.Round((Convert.ToDouble(monthlySalesCountModel.B) / cels.Length), 2).ToString();
                monthlySalesCountModel.C = Math.Round((Convert.ToDouble(monthlySalesCountModel.C) / cels.Length), 2).ToString();
                monthlySalesCountModel.D = Math.Round((Convert.ToDouble(monthlySalesCountModel.C) - Convert.ToDouble(monthlySalesCountModel.B)) / Convert.ToDouble(monthlySalesCountModel.B) * 100, 2).ToString() + "%";

                monthlySalesCountModel2.Account = "人均出单";
                monthlySalesCountModel2.B = monthlySalesModel2 == null ? "0" : monthlySalesModel2.OrderCount.ToString();
                monthlySalesCountModel2.B = (Convert.ToInt32(monthlySalesCountModel2.B) / cels.Length).ToString();

                monthlySalesCountModel2.C = monthlySalesModel.OrderCount.ToString();
                monthlySalesCountModel2.C = (Convert.ToInt32(monthlySalesCountModel2.C) / cels.Length).ToString();
                monthlySalesCountModel2.D = Math.Round((Convert.ToDouble(monthlySalesCountModel2.C) - Convert.ToDouble(monthlySalesCountModel2.B)) / Convert.ToDouble(monthlySalesCountModel2.B) * 100, 2).ToString() + "%";
                result.Add(monthlySalesCountModel);
                result.Add(monthlySalesCountModel2);
            }

            return Json(new { rows = result, total = result.Count });
        }

        public ActionResult GetDealReport3(string a, string p, DateTime d, string u)
        {
            if (a == null)
                return null;
            string[] cels = u.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string sqlWhere = " where  O.Enabled=1 And O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt'";

            if (((!string.IsNullOrEmpty(p) && (p != "ALL")) && (p != "0")) && (p != "===请选择==="))
            {
                sqlWhere += " and O.Platform='" + p + "' ";
            }
            if (((!string.IsNullOrEmpty(a) && (a != "ALL")) && (a != "0")) && (a != "===请选择==="))
            {
                sqlWhere += " and O.Account in ('" + a.Replace(",", "','") + "')";
            }
            if (((!string.IsNullOrEmpty(u) && (u != "ALL")) && (u != "0")) && (a != "===请选择==="))
            {
                sqlWhere += " and P.CreateBy in ('" + u.Replace(",", "','") + "')";
            }
            string sqlWhere2 = sqlWhere + " and O.GenerateOn  between '" + d.AddYears(-1).AddMonths(-1).ToString("yyyy-MM") + "-01' and '" + d.ToString("yyyy-MM") + "-01'";
            string sqlWhere3 = sqlWhere + " and O.GenerateOn  between '" + d.AddYears(-1).ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01'";
            sqlWhere += " and O.GenerateOn  between '" + d.ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01'";



            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();

            string sql = "select CreateBy,COUNT(distinct O.Id) from Orders O left join (select OId,SKU  from OrderProducts  group by OId, SKU ) OP on OP.OId=O.Id   left join Products P on OP.SKU=p.SKU  " + sqlWhere + "  group by CreateBy";
            string sql2 = "select CreateBy,COUNT(distinct O.Id) from Orders O left join (select OId,SKU  from OrderProducts  group by OId, SKU ) OP on OP.OId=O.Id   left join Products P on OP.SKU=p.SKU  " + sqlWhere2 + "group by CreateBy";
            string sql5 = "select CreateBy,COUNT(distinct O.Id) from Orders O left join (select OId,SKU  from OrderProducts  group by OId, SKU ) OP on OP.OId=O.Id   left join Products P on OP.SKU=p.SKU  " + sqlWhere3 + "group by CreateBy";
            string sql3 =
                "select CreateBy,COUNT(distinct OldSKU) from Products where CreateBy<>'system' and CreateOn  between '" + d.ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01' and CreateBy in('" + u.Replace(",", "','") + "') Group by CreateBy ";
            string sql4 =
              "select CreateBy,COUNT(distinct OldSKU) from Products where CreateBy<>'system' and CreateOn  between '" + d.AddYears(-1).AddMonths(-1).ToString("yyyy-MM") + "-01' and '" + d.ToString("yyyy-MM") + "-01' and CreateBy in('" + u.Replace(",", "','") + "') Group by CreateBy ";
            string sql6 =
                 "select CreateBy,COUNT(distinct OldSKU) from Products where CreateBy<>'system' and CreateOn  between '" + d.AddYears(-1).ToString("yyyy-MM") + "-01' and '" + d.AddMonths(1).ToString("yyyy-MM") + "-01' and CreateBy in('" + u.Replace(",", "','") + "') Group by CreateBy ";

            List<MonthlySalesModel> list = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list2 = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list3 = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list4 = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list5 = new List<MonthlySalesModel>();
            List<MonthlySalesModel> list6 = new List<MonthlySalesModel>();
            List<MonthlySalesCountModel> result = new List<MonthlySalesCountModel>();
            List<MonthlySalesCountModel> resultfooter = new List<MonthlySalesCountModel>();

            List<object[]> objectses = NSession.CreateSQLQuery(sql).List<object[]>().ToList();
            List<object[]> objectses2 = NSession.CreateSQLQuery(sql2).List<object[]>().ToList();
            List<object[]> objectses3 = NSession.CreateSQLQuery(sql3).List<object[]>().ToList();
            List<object[]> objectses4 = NSession.CreateSQLQuery(sql4).List<object[]>().ToList();
            List<object[]> objectses5 = NSession.CreateSQLQuery(sql5).List<object[]>().ToList();
            List<object[]> objectses6 = NSession.CreateSQLQuery(sql6).List<object[]>().ToList();
            ///本月的数据
            foreach (object[] objectse in objectses)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list.Add(model);
            }
            ///上月的数据
            foreach (object[] objectse in objectses2)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list2.Add(model);
            }
            foreach (object[] objectse in objectses3)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list3.Add(model);
            }
            foreach (object[] objectse in objectses4)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list4.Add(model);
            }
            foreach (object[] objectse in objectses5)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list5.Add(model);
            }
            foreach (object[] objectse in objectses6)
            {
                MonthlySalesModel model = new MonthlySalesModel();
                model.Account = objectse[0].ToString();
                model.OrderCount = objectse[1] == null ? 0 : Convert.ToInt32(objectse[1]);
                list6.Add(model);
            }
            foreach (string cel in cels)
            {
                MonthlySalesCountModel monthlySalesCountModel = new MonthlySalesCountModel();
                monthlySalesCountModel.Account = cel;
                MonthlySalesModel model1 = list.Find(x => x.Account == cel);
                MonthlySalesModel model3 = list3.Find(x => x.Account == cel);
                MonthlySalesModel model2 = list2.Find(x => x.Account == cel);
                MonthlySalesModel model4 = list4.Find(x => x.Account == cel);
                MonthlySalesModel model5 = list5.Find(x => x.Account == cel);
                MonthlySalesModel model6 = list6.Find(x => x.Account == cel);
                monthlySalesCountModel.A = model3.OrderCount.ToString();
                monthlySalesCountModel.B = model1.OrderCount.ToString();
                monthlySalesCountModel.C = Math.Round((Convert.ToDouble(model2.OrderCount) / (Convert.ToDouble(model4.OrderCount))), 2).ToString();
                monthlySalesCountModel.D = Math.Round((Convert.ToDouble(model5.OrderCount) / (Convert.ToDouble(model6.OrderCount))), 2).ToString();
                result.Add(monthlySalesCountModel);
            }


            return Json(new { rows = result, total = result.Count });
        }

        public ActionResult GetMonthlySalesGrowth(int year, int yw)
        {
            string str = "";

            if (yw == 0)
                str = " and Account not like '%yw%'";
            else
                str = " and Account  like '%yw%'";
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            string sql =
                "select datepart(yy,GenerateOn) as 'y',datepart(mm,GenerateOn) as 'm',round(SUM(Amount),2) as 'a',Account,CurrencyCode from Orders O where O.Enabled=1 and O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt' and  GenerateOn between '" + (year - 1) + "-12-01' and '" + (year + 1) + "-01-01' " + str + " group by datepart(yy,GenerateOn),datepart(mm,GenerateOn),Account,CurrencyCode order by Account,y,m ";
            List<MonthlySalesModel> list = new List<MonthlySalesModel>();
            List<MonthlySalesModel> result = new List<MonthlySalesModel>();
            List<MonthlySalesCountModel> resultfooter = new List<MonthlySalesCountModel>();
            MonthlySalesCountModel monthlySalesCountModel1 = new MonthlySalesCountModel();
            MonthlySalesCountModel monthlySalesCountModel2 = new MonthlySalesCountModel();
            List<object[]> objectses = NSession.CreateSQLQuery(sql).List<object[]>().ToList();
            foreach (object[] objectse in objectses)
            {
                MonthlySalesModel model = new MonthlySalesModel();

                model.Year = objectse[0].ToString();
                model.Monthly = Convert.ToInt32(objectse[1].ToString());
                model.CurrencyCode = objectse[4].ToString();
                model.Account = objectse[3].ToString() + "(" + model.CurrencyCode + ")";
                model.Amount = GetAmount(model.CurrencyCode, Convert.ToDouble(objectse[2].ToString()), currencies);
                list.Add(model);
            }
            monthlySalesCountModel2.Account = monthlySalesCountModel1.Account = "合计：";
            double o = list.Where(x => x.Monthly == 12 && x.Year == (year - 1).ToString()).Sum(x => x.Amount);

            monthlySalesCountModel1.A = list.Where(x => x.Monthly == 1).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.B = list.Where(x => x.Monthly == 2).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.C = list.Where(x => x.Monthly == 3).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.D = list.Where(x => x.Monthly == 4).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.E = list.Where(x => x.Monthly == 5).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.F = list.Where(x => x.Monthly == 6).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.G = list.Where(x => x.Monthly == 7).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.H = list.Where(x => x.Monthly == 8).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.I = list.Where(x => x.Monthly == 9).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.J = list.Where(x => x.Monthly == 10).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.K = list.Where(x => x.Monthly == 11).Sum(x => x.Amount).ToString();
            monthlySalesCountModel1.L = list.Where(x => x.Monthly == 12 && x.Year == year.ToString()).Sum(x => x.Amount).ToString();

            if (monthlySalesCountModel1.A != "0")
                monthlySalesCountModel2.A = Math.Round((Convert.ToDouble(monthlySalesCountModel1.A) - o) / o * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.B != "0")
                monthlySalesCountModel2.B = Math.Round((Convert.ToDouble(monthlySalesCountModel1.B) - Convert.ToDouble(monthlySalesCountModel1.A)) / Convert.ToDouble(monthlySalesCountModel1.A) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.C != "0")
                monthlySalesCountModel2.C = Math.Round((Convert.ToDouble(monthlySalesCountModel1.C) - Convert.ToDouble(monthlySalesCountModel1.B)) / Convert.ToDouble(monthlySalesCountModel1.B) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.D != "0")
                monthlySalesCountModel2.D = Math.Round((Convert.ToDouble(monthlySalesCountModel1.D) - Convert.ToDouble(monthlySalesCountModel1.C)) / Convert.ToDouble(monthlySalesCountModel1.C) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.E != "0")
                monthlySalesCountModel2.E = Math.Round((Convert.ToDouble(monthlySalesCountModel1.E) - Convert.ToDouble(monthlySalesCountModel1.D)) / Convert.ToDouble(monthlySalesCountModel1.D) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.F != "0")
                monthlySalesCountModel2.F = Math.Round((Convert.ToDouble(monthlySalesCountModel1.F) - Convert.ToDouble(monthlySalesCountModel1.E)) / Convert.ToDouble(monthlySalesCountModel1.E) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.G != "0")
                monthlySalesCountModel2.G = Math.Round((Convert.ToDouble(monthlySalesCountModel1.G) - Convert.ToDouble(monthlySalesCountModel1.F)) / Convert.ToDouble(monthlySalesCountModel1.F) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.H != "0")
                monthlySalesCountModel2.H = Math.Round((Convert.ToDouble(monthlySalesCountModel1.H) - Convert.ToDouble(monthlySalesCountModel1.G)) / Convert.ToDouble(monthlySalesCountModel1.G) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.I != "0")
                monthlySalesCountModel2.I = Math.Round((Convert.ToDouble(monthlySalesCountModel1.I) - Convert.ToDouble(monthlySalesCountModel1.H)) / Convert.ToDouble(monthlySalesCountModel1.H) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.J != "0")
                monthlySalesCountModel2.J = Math.Round((Convert.ToDouble(monthlySalesCountModel1.J) - Convert.ToDouble(monthlySalesCountModel1.I)) / Convert.ToDouble(monthlySalesCountModel1.I) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.K != "0")
                monthlySalesCountModel2.K = Math.Round((Convert.ToDouble(monthlySalesCountModel1.K) - Convert.ToDouble(monthlySalesCountModel1.J)) / Convert.ToDouble(monthlySalesCountModel1.J) * 100, 2).ToString() + "%";
            if (monthlySalesCountModel1.L != "0")
                monthlySalesCountModel2.L = Math.Round((Convert.ToDouble(monthlySalesCountModel1.L) - Convert.ToDouble(monthlySalesCountModel1.K)) / Convert.ToDouble(monthlySalesCountModel1.K) * 100, 2).ToString() + "%";
            resultfooter.Add(monthlySalesCountModel1);
            resultfooter.Add(monthlySalesCountModel2);
            foreach (IGrouping<string, MonthlySalesModel> foo in list.GroupBy(x => x.Account))
            {
                foo.Sum(x => x.Amount);
                for (int i = 1; i <= 12; i++)
                {
                    //计算本月和上月的比例
                    IEnumerable<MonthlySalesModel> ww = foo.Where(x => x.Monthly == i && x.Year == year.ToString());
                    IEnumerable<MonthlySalesModel> ww2 = foo.Where(x => x.Monthly == ((i - 1) == 0 ? 12 : (i - 1)) && x.Year == ((i - 1) == 0 ? (year - 1).ToString() : year.ToString()));
                    if (ww.Count() > 0)
                    {
                        if (ww2.Count() > 0)
                        {
                            ww.First().Rate =
                                ((ww.First().Amount - ww2.First().Amount) / ww2.First().Amount * 100).ToString();
                        }
                        if (ww.First().Rate == null)
                        {
                            ww.First().Rate = "0";
                        }
                        result.Add(ww.First());
                    }
                    else
                    {
                        MonthlySalesModel model = new MonthlySalesModel();
                        model.Account = foo.Key;
                        model.Year = year.ToString();
                        model.Monthly = i;
                        model.Amount = 0;
                        model.Rate = "0";
                        result.Add(model);

                    }
                }
            }

            List<MonthlySalesCountModel> resultmodel = new List<MonthlySalesCountModel>();
            foreach (IGrouping<string, MonthlySalesModel> foo in result.GroupBy(x => x.Account))
            {
                MonthlySalesCountModel model1 = new MonthlySalesCountModel();
                MonthlySalesCountModel model2 = new MonthlySalesCountModel();
                model1.Account = model2.Account = foo.Key;
                foreach (MonthlySalesModel monthlySalesModel in foo)
                {

                    if (monthlySalesModel == null)
                        continue;
                    switch (monthlySalesModel.Monthly)
                    {
                        case 1:
                            model1.A = monthlySalesModel.Amount.ToString();
                            model2.A = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 2:
                            model1.B = monthlySalesModel.Amount.ToString();
                            model2.B = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 3:
                            model1.C = monthlySalesModel.Amount.ToString();
                            model2.C = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 4:
                            model1.D = monthlySalesModel.Amount.ToString();
                            model2.D = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 5:
                            model1.E = monthlySalesModel.Amount.ToString();
                            model2.E = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 6:
                            model1.F = monthlySalesModel.Amount.ToString();
                            model2.F = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 7:
                            model1.G = monthlySalesModel.Amount.ToString();
                            model2.G = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 8:
                            model1.H = monthlySalesModel.Amount.ToString();
                            model2.H = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 9:
                            model1.I = monthlySalesModel.Amount.ToString();
                            model2.I = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 10:
                            model1.J = monthlySalesModel.Amount.ToString();
                            model2.J = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 11:
                            model1.K = monthlySalesModel.Amount.ToString();
                            model2.K = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                        case 12:
                            model1.L = monthlySalesModel.Amount.ToString();
                            model2.L = Math.Round(Convert.ToDouble(monthlySalesModel.Rate), 2) + "%";
                            break;
                    }
                }
                resultmodel.Add(model1);
                resultmodel.Add(model2);
            }


            return Json(new { rows = resultmodel, total = resultmodel.Count, footer = resultfooter });
        }

        public double GetAmount(string CurrencyCode, double amount, List<CurrencyType> currencies)
        {
            if (CurrencyCode.ToUpper() != "USD")
            {
                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == CurrencyCode);
                CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
                if (currencyType != null && currencyType2 != null)
                {
                    return Math.Round(amount * Convert.ToDouble(currencyType.CurrencyValue) / Convert.ToDouble(currencyType2.CurrencyValue),
                                    2);
                }
            }
            return Math.Round(amount, 2);
        }
        public JsonResult ExportSellCount(DateTime st, DateTime et, string a, string p, string ss)
        {
            List<ProductData> list = this.GetSellCount(st, et, a, p, ss, 0, 0);
            Session["ExportDown"] = ExcelHelper.GetExcelXml(ConvertToDataSet<ProductData>(list));
            return base.Json(new { IsSuccess = true });
        }
        //IList转DataSet
        public static DataSet ConvertToDataSet<T>(IList<T> list)
        {
            if (list == null || list.Count <= 0)
            {
                return null;
            }

            DataSet ds = new DataSet();
            DataTable dt = new DataTable(typeof(T).Name);
            DataColumn column;
            DataRow row;

            System.Reflection.PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (T t in list)
            {
                if (t == null)
                {
                    continue;
                }

                row = dt.NewRow();

                for (int i = 0, j = myPropertyInfo.Length; i < j; i++)
                {
                    System.Reflection.PropertyInfo pi = myPropertyInfo[i];

                    string name = pi.Name;

                    if (dt.Columns[name] == null)
                    {
                        column = new DataColumn(name, pi.PropertyType);
                        dt.Columns.Add(column);
                    }

                    row[name] = pi.GetValue(t, null);
                }

                dt.Rows.Add(row);
            }

            ds.Tables.Add(dt);

            return ds;
        }
    
        public JsonResult ExportSore(DateTime st, DateTime et)
        {
            return base.Json(new { IsSuccess = true });
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

        [HttpPost]
        public ContentResult GetSaleChart(string s, DateTime st, string p)
        {
            int num;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<chart caption='{0} 的销售记录({1}-{2}) 总销量：{3} 日平均销量：{4}' subCaption='销量'  showLabels='1' showColumnShadow='1' animation='1' showAlternateHGridColor='1' AlternateHGridColor='ff5904' divLineColor='ff5904' divLineAlpha='20' alternateHGridAlpha='5' canvasBorderColor='666666' baseFontColor='666666'  lineAlpha='85' showValues='1' rotateValues='0' valuePosition='auto' canvaspadding='8' lineThickness='3'>");
            DateTime now = st.AddDays(30);
            List<string> list = new List<string>();
            StringBuilder builder2 = new StringBuilder();
            DateTime date = st;
            builder2.Append("select {0} as 'Y',");
            if (now > DateTime.Now)
            {
                now = DateTime.Now;
            }
            int d = (now - st).Days;
            while (date <= now)
            {
                string week = this.GetWeek("zh", date);
                list.Add(date.ToString("MM.dd") + "(" + week + ")");
                builder2.Append(" SUM(case  when convert(varchar(10),[CreateOn],120)='" + date.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + date.ToString("MM.dd") + "(" + week + ")' ,");
                date = date.AddDays(1.0);
            }
            builder2 = builder2.Remove(builder2.Length - 1, 1);
            builder2.Append("  from  ( select {0} ,convert(varchar(10),O.CreateOn,120) [CreateOn] ,sum(op.Qty) as 'rcount'  from Orders O left join OrderProducts OP on O.Id=OP.OId where O.CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + now.ToString("yyyy-MM-dd") + " 23:59:59' and SKU='{1}' {2}  group by {0} ,convert(varchar(10),O.CreateOn,120)) as tbl1  group by {0} ");
            string queryString = "";
            object totalAmount = null;
            if (p != "ALL")
            {
                queryString = string.Format(builder2.ToString(), "Account", s, " and Platform='" + p + "'");
                totalAmount = base.NSession.CreateSQLQuery("select sum(op.Qty) as 'rcount' from Orders O left join OrderProducts OP on O.Id=OP.OId where  O.CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + now.ToString("yyyy-MM-dd") + " 23:59:59' and SKU='" + s + "' " + " and Platform='" + p + "'").UniqueResult();//总销售额
            }
            else
            {
                queryString = string.Format(builder2.ToString(), "Platform", s, "");
                totalAmount = base.NSession.CreateSQLQuery("select sum(op.Qty) as 'rcount' from Orders O left join OrderProducts OP on O.Id=OP.OId where  O.CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + now.ToString("yyyy-MM-dd") + " 23:59:59' and SKU='" + s + "' ").UniqueResult();//总销售额
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
            return base.Content(string.Format(builder.ToString(), s, st.ToShortDateString(), now.ToShortDateString(), totalAmount, Utilities.ToInt(totalAmount) / d));
        }

        [HttpPost]
        private List<ProductData> GetSellCount(DateTime st, DateTime et, string a, string p, string s, int page = 0, int rows = 0, int area = 0)
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
            if (area == 1)
                str += " and O.Account not like '%yw%'";
            if (area == 2)
                str += " and O.Account like '%yw%'";
            IList<object[]> list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(O.Id) as Qty,Min(Price) as price from OrderProducts right join Orders  O on OId=O.Id   {0} group by SKU Order By sQty desc", str)).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<object[]>();
            if (page == 0)
            {
                list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(O.Id) as Qty,SUM(O.Amount) as price from OrderProducts right join Orders O on OId=O.Id   {0} group by SKU Order By sQty desc", str)).List<object[]>();
            }
            else
            {
                list = base.NSession.CreateSQLQuery(string.Format("select SKU,SUM(Qty) as sQty,count(O.Id) as Qty,SUM(O.Amount) as price from OrderProducts right join Orders O on OId=O.Id   {0} group by SKU Order By sQty desc", str)).SetFirstResult(rows * (page - 1)).SetMaxResults(rows).List<object[]>();
            }
            // 区分宁波义乌
            string wsql = "";
            if (area == 1)
            {
                // 宁波
                wsql = " WID=1 and ";
            }
            else if (area == 2)
            {
                // 义乌
                wsql = " WID=3 and ";
            }
            else
            {
                // 全部
                wsql = " 1=1 and ";
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
                    //SKU对应库存
                    //SourceQty = Convert.ToInt32(base.NSession.CreateQuery("select sum(Qty) from WarehouseStockType where SKU ='" + objArray[0].ToString() + "' ").UniqueResult()),
                    // 库存库存明细总和 -- 未区分宁波义乌
                    SourceQty = (Convert.ToInt32(base.NSession.CreateQuery("select sum(NowQty) from WarehouseStockDataType where " + wsql + " NowQty>0 and WName='义乌仓库' and SKU ='" + objArray[0].ToString() + "' ").UniqueResult())).ToString() + "/" + (Convert.ToInt32(base.NSession.CreateQuery("select sum(NowQty) from WarehouseStockDataType where " + wsql + " NowQty>0 and SKU ='" + objArray[0].ToString() + "' ").UniqueResult()) - Convert.ToInt32(base.NSession.CreateQuery("select sum(NowQty) from WarehouseStockDataType where " + wsql + " NowQty>0 and WName='义乌仓库' and  SKU ='" + objArray[0].ToString() + "' ").UniqueResult())).ToString(),
                    //SKU对应库存总金额
                    SourceAmount = Convert.ToDouble(base.NSession.CreateSQLQuery(" select sum(NowQty*Amount) as TotalPrice from WarehouseStockData  Where  " + wsql + " NowQty>0 and SKU ='" + objArray[0].ToString() + "' ").UniqueResult()),
                    //采购（预）【采购未到数-缺货数】
                    PurchaseplanQty = (Convert.ToInt32(base.NSession.CreateQuery("select sum(Qty-DaoQty) from PurchasePlanType where Status in( '已采购','已发货','部分到货') and SKU ='" + objArray[0].ToString() + "' ").UniqueResult())).ToString() + "-" + (Convert.ToInt32(base.NSession.CreateSQLQuery("select sum(OP.Qty) from  Orders O left join OrderProducts OP on O.Id=OP.OId where O.Status in ('已处理','待拣货') and OP.IsQue=1 and O.Enabled=1  and OP.SKU ='" + objArray[0].ToString() + "' ").UniqueResult())).ToString() + "=" + (Convert.ToInt32(base.NSession.CreateQuery("select sum(Qty-DaoQty) from PurchasePlanType where Status in( '已采购','已发货','部分到货') and SKU ='" + objArray[0].ToString() + "' ").UniqueResult()) - Convert.ToInt32(base.NSession.CreateSQLQuery("select sum(OP.Qty) from  Orders O left join OrderProducts OP on O.Id=OP.OId where O.Status in ('已处理','待拣货') and OP.IsQue=1 and O.Enabled=1  and OP.SKU ='" + objArray[0].ToString() + "' ").UniqueResult())).ToString(),
                    //已处理  缺货否
                    HandingQty= Convert.ToInt32(base.NSession.CreateSQLQuery("select sum(OP.Qty) from  Orders O left join OrderProducts OP on O.Id=OP.OId where O.Status in ('已处理') and OP.IsQue=3 and O.Enabled=1  and OP.SKU ='" + objArray[0].ToString() + "' ").UniqueResult()),
                    OQty = Convert.ToInt32(objArray[2]),
                    OAmount = "#"
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
                        // pp.Price = type.Price;
                        pp.PicUrl = type.PicUrl;
                        pp.Title = type.ProductName;
                        pp.Rate = Math.Round(Convert.ToDouble(pp.Qty) / Convert.ToDouble(pp.OQty), 2);
                        pp.Price = "#";
                        pp.TotalPrice = Math.Round(Utilities.ToDouble(pp.Price) * pp.Qty, 2);
                    }
                }
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
        public ActionResult OrderCount(string timeType, DateTime st, DateTime et, string a, string p, string i)
        {
            string str = "";
            if (timeType.ToString() == "CreateOn")
            {
                str = this.SqlWhereCreateOn(st, et, a, p, "");
            }
            else
            {
                str = this.SqlWhere(st, et, a, p, "");
            }
            //string queryString = string.Format("select Account,Count(Id),Platform,Sum(Amount),Min(CurrencyCode) from OrderType O {0} group by Account,Platform", str);
            string queryString = string.Format("select Account,Count(1) ,o.Platform,Sum(Amount) as Amount,CurrencyCode from Orders O join Account a on o.Account=a.AccountName {0} group by Account,o.Platform ,CurrencyCode,Manager order by o.platform,Account,Manager asc", str);

            if (!string.IsNullOrEmpty(i))
            {
                queryString = queryString + " ,CurrencyCode";
            }
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            //IList<object[]> list = base.NSession.CreateQuery(queryString).List<object[]>();
            IList<object[]> list = base.NSession.CreateSQLQuery(queryString).List<object[]>();
            List<DDX.OrderManagementSystem.App.OrderCount> list2 = new List<DDX.OrderManagementSystem.App.OrderCount>();
            int num = 0;
            double num2 = 0.0;
            foreach (object[] objArray in list)
            {
                OrderCount item = new OrderCount
                {
                    Account = objArray[0].ToString(),
                    OCount = Convert.ToInt32(objArray[1]),

                    Platform = objArray[2].ToString(),
                    TotalAmount = Math.Round(Convert.ToDouble(objArray[3].ToString()), 2),
                    CurrencyCode = objArray[4].ToString(),
                    AmountUSD = Math.Round(Convert.ToDouble(objArray[3].ToString()), 2)
                };
                if (item.CurrencyCode != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == item.CurrencyCode);
                    CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
                    if (currencyType != null || currencyType2 != null)
                    {
                        item.AmountUSD =
                            Math.Round(item.TotalAmount * Convert.ToDouble(currencyType.CurrencyValue) / Convert.ToDouble(currencyType2.CurrencyValue),
                                       2);
                    }
                }

                if (item.Account.IndexOf("yw") != -1)
                {
                    item.Area = "义乌";
                }
                else
                {
                    item.Area = "宁波";
                }
                item.AmountUSD = Math.Round(item.AmountUSD, 2);
                num += Convert.ToInt32(objArray[1]);
                num2 += item.AmountUSD;
                list2.Add(item);
            }

            IList<AccountType> list3 = base.NSession.CreateQuery("from AccountType where Status=0 " + (p != "0" ? " and Platform='" + p + "'" : "")).List<AccountType>();
            using (IEnumerator<AccountType> enumerator2 = list3.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Predicate<DDX.OrderManagementSystem.App.OrderCount> match = null;
                    AccountType accountType = enumerator2.Current;
                    if (accountType.AccountName != "su-smt")
                    {
                        if (match == null)
                        {
                            match = x => x.Account == accountType.AccountName;
                        }
                        DDX.OrderManagementSystem.App.OrderCount count3 = list2.Find(match);
                        if (count3 != null)
                        {
                            count3.AccountUrl = accountType.AccountUrl;
                            count3.Icon = accountType.Icon;
                            count3.ManageBy = accountType.Manager;
                        }
                        else
                        {
                            count3 = new DDX.OrderManagementSystem.App.OrderCount
                            {
                                Account = accountType.AccountName,
                                Platform = accountType.Platform,
                                AccountUrl = accountType.AccountUrl,
                                Icon = accountType.Icon,
                                ManageBy = accountType.Manager,
                                QCount = 0,
                                OCount = 0
                            };
                            list2.Add(count3);
                        }
                    }
                }
            }
            foreach (DDX.OrderManagementSystem.App.OrderCount count4 in list2)
            {
                if (count4.Account.IndexOf("yw") != -1)
                {
                    count4.Area = "义乌";
                }
                else
                {
                    count4.Area = "宁波";
                }
            }

            // 停用、帐号订单数量为0、帐号金额为0移除不显示
            for (int nK = list2.Count - 1; nK >= 0; nK--)
            {
                if (list2[nK].ManageBy == "停用" && list2[nK].AmountUSD == 0 && list2[nK].OCount == 0)
                {
                    list2.Remove(list2[nK]);
                }
            }

            List<object> list4 = new List<object> {
                new { OCount = num, AmountUSD = Math.Round(num2, 2) }
            };
            return base.Json(new
            {
                rows = from x in list2
                       orderby x.Area, x.Platform, x.ManageBy, x.Account
                       select x,
                footer = list4,
                total = list2.Count
            });
        }

        [HttpPost]
        public ActionResult OrderCountryData(string timeType, DateTime st, DateTime et, string a, string p, int t = 0)
        {
            string str = "";
            if (timeType.ToString() == "CreateOn")
            {
                str = this.SqlWhereCreateOn(st, et, a, p, "");
            }
            else
            {
                str = this.SqlWhere(st, et, a, p, "");
            }
            List<ProportionData> proportionDatas = new List<ProportionData>();

            if (t == 0)
                str += " and Account not like '%yw%'";
            else
                str += " and Account like '%yw%'";

            IList<object[]> list = base.NSession.CreateQuery(string.Format("select Country,COUNT(Id) from OrderType O {0} group by Country", str)).List<object[]>();
            decimal num = Convert.ToDecimal(base.NSession.CreateQuery(string.Format("select COUNT(Id) from OrderType O {0}", str)).UniqueResult());

            foreach (object[] objArray in list)
            {
                ProportionData data = new ProportionData();
                data = new ProportionData
                {
                    Count = Convert.ToInt32(objArray[1]),
                    Key = objArray[0].ToString(),

                    Area = "宁波",
                    Proportion = Math.Round((decimal)((Convert.ToDecimal(objArray[1]) / num) * 100M), 2)
                };
                proportionDatas.Add(data);
            }


            List<object> list3 = new List<object> {
                new { Count = num }
            };
            return base.Json(new
            {
                rows = from f in proportionDatas
                       orderby f.Count descending
                       select f,
                footer = list3,
                total = proportionDatas.Count
            });
        }

        [HttpPost]
        public ActionResult OrderLeveData(string timeType, DateTime st, DateTime et, string a, string p, int t = 0)
        {
            int count = 0;
            string str = "";
            if (timeType.ToString() == "CreateOn")
            {
                str = this.SqlWhereCreateOn(st, et, a, p, "");
            }
            else
            {
                str = this.SqlWhere(st, et, a, p, "");
            }
            if (t == 0)
                str += " and Account not like '%yw%'";
            else
                str += " and Account like '%yw%'";
            List<LeveData> list = new List<LeveData>();
            IList<OrderType> list2 = base.NSession.CreateQuery(string.Format("from OrderType O " + str, new object[0])).List<OrderType>();
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
            return base.Json(new { rows = list.OrderByDescending(x => x.Account), footer = list3, total = list.Count });
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

        /// <summary>
        /// 日销售报表-订单数量统计
        /// </summary>
        /// <param name="st"></param>
        /// <param name="et"></param>
        /// <param name="a"></param>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <param name="area"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SellCount(DateTime st, DateTime et, string a, string p, string s, string area, int page, int rows)
        {
            List<ProductData> list = this.GetSellCount(st, et, a, p, s, page, rows, Convert.ToInt32(area));
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
            //switch (area)
            //{
            //    case 1:
            //        str+=""
            //        break;
            //    case 2:
            //        break;

            //}
            object obj2 = base.NSession.CreateSQLQuery(string.Format("select COUNT(1) from ( select SKU from OrderProducts right join Orders O on OId=O.Id   {0} group by SKU ) as tbl", str)).UniqueResult();

            List<object[]> obj3 = base.NSession.CreateSQLQuery(string.Format("select SUM(Qty) as sQty,count(O.Id) as Qty,SUM(O.Amount) as price from OrderProducts right join Orders O on OId=O.Id   {0}  Order By sQty desc", str)).List<object[]>().ToList();

            string sqlwhere = this.SqlWhere(st, et, a, p);
            if (!string.IsNullOrEmpty(s))
            {
                if (s.IndexOf(",") != -1)
                {
                    sqlwhere += " and Id in (select OId from OrderProducts where SKU in ('" + s.Replace(",", "','") + "'))";
                }
                else
                {
                    sqlwhere += " and Id in (select OId from OrderProducts where SKU like '%" + s + "%')";
                }

            }

            List<object[]> obj4 =
                base.NSession.CreateSQLQuery(string.Format("select O.CurrencyCode,SUM(O.Amount) from Orders O  {0}  group by CurrencyCode", sqlwhere)).List<object[]>().ToList();
            object obj5 =
                base.NSession.CreateSQLQuery(string.Format("select count(O.Id) as Qty from Orders O  {0}", sqlwhere)).UniqueResult();
            List<ProductData> ff = new List<ProductData>();
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            CurrencyType currencyType2 = currencies.Find(x => x.CurrencyCode == "USD");
            decimal oamount = 0;
            foreach (object[] objectse in obj4)
            {
                if (objectse[0].ToString() != "USD")
                {
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == objectse[0].ToString());

                    if (currencyType != null || currencyType2 != null)
                    {
                        oamount +=
                             Math.Round(Convert.ToDecimal(objectse[1]) * Convert.ToDecimal(currencyType.CurrencyValue) / Convert.ToDecimal(currencyType2.CurrencyValue),
                                        2);
                    }

                }
                else
                {
                    oamount += Math.Round(Convert.ToDecimal(objectse[1]), 2);
                }
            }
            ProductData pp = new ProductData
                                 {
                                     Qty = Convert.ToInt32(obj3[0][0]),
                                     OQty = Utilities.ToInt(obj5),
                                     OAmount = Utilities.ToString(oamount)

                                 };
            pp.Rate = Math.Round(Convert.ToDouble(pp.Qty) / Convert.ToDouble(pp.OQty), 2);
            pp.Price = Math.Round(Utilities.ToDouble(pp.OAmount) / pp.Qty, 2).ToString();
            //  pp.Price = Math.Round(pp.OAmount / pp.Qty, 2);

            ff.Add(pp);
            return base.Json(new
            {
                rows = list,
                total = obj2,
                footer = ff
            });
        }
        [HttpPost]
        public ActionResult SellCountTotal(DateTime st, DateTime et, string a, string p, string s)
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
            object obj2 = base.NSession.CreateSQLQuery(string.Format("select Sum(O.Amount) from OrderProducts right join Orders O on OId=O.Id   {0}  ", str)).UniqueResult();
            return base.Json(obj2);
        }

        public ActionResult SKUCreate()
        {
            return base.View();
        }

        [HttpPost]
        public string SKUCreateData(DateTime st, DateTime et)
        {
            List<string> strData = new List<string>();
            StringBuilder builder = new StringBuilder();
            strData.Add("人员");

            builder.AppendLine(
                "select *,(select COUNT( distinct OldSKU) from Products where CreateBy<>'system' and [CreateOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' and CreateBy=人员 ) as '合',(select COUNT( distinct OldSKU) from Products where CreateBy<>'system' and CreateOn>'" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "' and CreateBy=人员 ) as '3',(select COUNT( distinct OldSKU) from Products where CreateBy<>'system' and CreateOn>'" + DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") + "' and CreateBy=人员 ) as '6',(select COUNT( distinct OldSKU) from Products where CreateBy<>'system' and CreateOn>'" + DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd") + "' and CreateBy=人员 ) as '12'from (");
            builder.Append("select [CreateBy] as '人员',");
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                strData.Add(time.ToString("MMdd"));
                builder.Append("SUM(case  when convert(varchar(10),CreateOn,120)='" + time.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + time.ToString("MM.dd") + "(" + week + ")' ,");
            }
            strData.Add("合计");
            strData.Add("近3个月");
            strData.Add("近6个月");
            strData.Add("一年统计");
            builder = builder.Remove(builder.Length - 1, 1);
            builder.Append("from  (select [CreateBy],convert(varchar(10),[CreateOn],120) [CreateOn] ,1 as 'rcount'  from Products where CreateBy<>'system' and [CreateOn] between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by [CreateBy] ,OldSKU,convert(varchar(10),[CreateOn],120)) as tbl1  group by CreateBy ) as  tbl2");
            return ConvertJson(base.NSession.CreateSQLQuery(builder.ToString()).List<object[]>(), strData);
        }

        [HttpPost]
        public string SKUToOrderData(DateTime st, DateTime et)
        {
            List<string> strData = new List<string>();
            StringBuilder builder = new StringBuilder();
            strData.Add("人员");
            builder.AppendLine(
              "select *,(select COUNT( distinct O.Id) from Orders O left join OrderProducts OP ON O.Id=OP.OId    left join Products P on OP.SKU=p.SKU  where  O.CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' and P.CreateBy=人员 ) as '合',(select COUNT(O.Id) from Orders O left join OrderProducts OP ON O.Id=OP.OId    left join Products P on OP.SKU=p.SKU where  O.CreateOn>'" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "' and P.CreateBy=人员 ) as '3',(select COUNT(O.Id) from Orders O left join OrderProducts OP ON O.Id=OP.OId    left join Products P on OP.SKU=p.SKU where O.CreateOn>'" + DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") + "' and P.CreateBy=人员 ) as '6',(select COUNT(O.Id) from Orders O left join OrderProducts OP ON O.Id=OP.OId    left join Products P on OP.SKU=p.SKU where  O.CreateOn>'" + DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd") + "' and P.CreateBy=人员 ) as '12'from (");
            builder.Append("select [CreateBy] as '人员',");
            for (DateTime time = st; time <= et; time = time.AddDays(1.0))
            {
                string week = this.GetWeek("zh", time);
                strData.Add(time.ToString("MMdd"));
                builder.Append("SUM(case  when convert(varchar(10),CreateOn,120)='" + time.ToString("yyyy-MM-dd") + "' then  rcount else 0 end  ) as '" + time.ToString("MM.dd") + "(" + week + ")' ,");
            }
            strData.Add("合计");
            strData.Add("近3个月");
            strData.Add("近6个月");
            strData.Add("一年统计");
            builder = builder.Remove(builder.Length - 1, 1);
            builder.Append("from  (\r\n    select P.CreateBy,convert(varchar(10),O.CreateOn,120) CreateOn,COUNT(1) as 'rcount'  from Orders O\r\n    left join OrderProducts OP ON O.Id=OP.OId\r\n    left join Products P on OP.SKU=p.SKU\r\nwhere P.CreateBy<>'system' and O.CreateOn between '" + st.ToString("yyyy-MM-dd") + "' and '" + et.ToString("yyyy-MM-dd") + " 23:59:59' group by P.CreateBy,convert(varchar(10),O.CreateOn,120)) as tbl1  group by CreateBy ) as  tbl2");
            return ConvertJson(base.NSession.CreateSQLQuery(builder.ToString()).List<object[]>(), strData);
        }

        private static string SqlWhere(string a, string p, string ss, string sqlWhere)
        {
            if (((!string.IsNullOrEmpty(p) && (p != "ALL")) && (p != "0")) && (p != "===请选择==="))
            {
                sqlWhere = sqlWhere + " O.Platform='" + p + "' and";
            }
            if (((!string.IsNullOrEmpty(a) && (a != "ALL")) && (a != "0")) && (a != "===请选择==="))
            {
                //sqlWhere = sqlWhere + " Account='" + a + "' and";
                //平台店铺多选
                sqlWhere = sqlWhere + " Account In('" + a.Replace(",", "','") + "') and";
            }
            if (((!string.IsNullOrEmpty(ss) && (ss != "ALL")) && (ss != "0")) && (ss != "===请选择==="))
            {
                ss = ss.Trim();
                if (ss.IndexOf(",") != -1)
                {

                    sqlWhere = sqlWhere + " SKU In('" + ss.Replace(",", "','") + "')and";
                }
                else
                {
                    sqlWhere = sqlWhere + " SKU like '%" + ss + "%' and";
                }

            }
            if (sqlWhere.Length > 4)
            {
                sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 3);
            }
            return sqlWhere;
        }

        private string SqlWhere(DateTime st, DateTime et, string a, string p, string ss = "")
        {

            string sqlWhere = " where O.Enabled=1 and  O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt' and  O.GenerateOn between '" + st.ToString("yyyy/MM/dd HH:mm:ss") + "' and '" + et.ToString("yyyy/MM/dd HH:mm:ss") + "' and";
            return SqlWhere(a, p, ss, sqlWhere);
        }

        private string SqlWhereCreateOn(DateTime st, DateTime et, string a, string p, string ss = "")
        {

            string sqlWhere = " where O.Enabled=1 and  O.Status not in( '待处理','作废订单') and O.Account <> 'su-smt' and  O.CreateOn between '" + st.ToString("yyyy/MM/dd HH:mm:ss") + "' and '" + et.ToString("yyyy/MM/dd HH:mm:ss") + "' and";
            return SqlWhere(a, p, ss, sqlWhere);
        }

        private string sub(string id)
        {
            return id.Substring(0, id.IndexOf("$"));
        }

        private string subid(string id)
        {
            return id.Substring(id.IndexOf("$") + 1);
        }

        public JsonResult GetUnSendSKU(string a, string b)
        {
            string sql =
                "select OP.SKU,SUM(op.Qty) as QQty,MIN(O.GenerateOn) as MinDate, MAX(IsQue) as QQQ from Orders O left join OrderProducts OP on O.Id=OP.OId {0} group by OP.SKU Order BY QQty Desc";
            string where = " where ";
            if (a.IndexOf("-") != -1)
            {
                string[] cels = a.Split('-');
                where +=
                    string.Format(
                        " DATEDIFF(day,GenerateOn,'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "')>= '{0}' and DATEDIFF(day,GenerateOn,'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "') <= '{1}'",
                        cels[0], cels[1]);
            }
            else
            {
                where +=
                   string.Format(
                       " DATEDIFF(day,GenerateOn,'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "')>= '{0}'",
                      13);
            }
            where += " and Account='" + b + "' and O.Status='已处理' and O.Enabled=1";
            IList<object[]> list2 = base.NSession.CreateSQLQuery(string.Format(sql, where)).List<object[]>();
            List<UnSendSKUModel> list = new List<UnSendSKUModel>();
            foreach (object[] objectse in list2)
            {
                UnSendSKUModel foo = new UnSendSKUModel();
                foo.SKU = Utilities.ToString(objectse[0]);
                foo.Qty = Utilities.ToInt(objectse[1]);
                foo.MinDate = Convert.ToDateTime(objectse[2]);
                foo.QQQ = Utilities.ToInt(objectse[3]);
                list.Add(foo);
            }
            return Json(new { rows = list, total = list.Count });
        }

        [HttpPost]
        public ActionResult UnSendDays(DateTime st, DateTime et, string a, string p, string Area)
        {
            int count = 0;
            string str = this.SqlWhere(st, et, a, p, "");
            List<LeveDays> list = new List<LeveDays>();
            string where = "";
            if (Area == "宁波")
            {
                where = " and Account not like '%yw%'";
            }
            else
            {
                where = " and Account like '%yw%'";
            }
            IList<OrderType> list2 = base.NSession.CreateQuery(string.Format("from OrderType Where  Account <> 'su-smt' and Enabled=1 and Status ='已处理' " + where, new object[0])).List<OrderType>();
            int[,] arry = new int[,] { { 0, 6 }, { 7, 12 }, { 13, 0 } };
            count = list2.Count;
            using (IEnumerator<OrderType> enumerator = list2.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    OrderType item = enumerator.Current;
                    int num2 = 0;
                    LeveDays days = new LeveDays();
                    Predicate<LeveDays> match = null;
                    Predicate<LeveDays> predicate2 = null;
                    for (int i = 0; i < (arry.Length / 2); i++)
                    {
                        TimeSpan span;
                        if (arry[i, 1] != 0)
                        {

                            span = (TimeSpan)(DateTime.Now.Date - item.GenerateOn.Date);
                            if (span.Days >= arry[i, 0] && (span.Days <= arry[i, 1]))
                            {
                                if (match == null)
                                {
                                    match = x => ((x.Platform == (arry[i, 0] + "-" + arry[i, 1])) || (x.Platform == (arry[i, 0] + " 以上"))) && (x.Account == item.Account);
                                }
                                days = list.Find(match);
                                if (days == null)
                                {
                                    days = new LeveDays();
                                    if (arry[i, 1] != 0)
                                    {
                                        days.Platform = arry[i, 0] + "-" + arry[i, 1];
                                    }
                                    else
                                    {
                                        days.Platform = arry[i, 0] + " 以上";
                                    }
                                    days.Account = item.Account;

                                    days.OCount = 1M;
                                    days.Rate = Math.Round((decimal)((Convert.ToDecimal(days.OCount) / list2.Where(x => x.Account == days.Account).Count()) * 100M), 2);
                                    list.Add(days);
                                }
                                else
                                {
                                    days.OCount++;
                                    days.Rate = Math.Round((decimal)((Convert.ToDecimal(days.OCount) / list2.Where(x => x.Account == days.Account).Count()) * 100M), 2);
                                }
                            }
                        }
                        else
                        {
                            span = (TimeSpan)(DateTime.Now.Date - item.GenerateOn.Date);
                            if (span.Days >= arry[i, 0])
                            {
                                if (predicate2 == null)
                                {
                                    predicate2 = x => ((x.Platform == (arry[i, 0] + "-" + arry[i, 1])) || (x.Platform == (arry[i, 0] + " 以上"))) && (x.Account == item.Account);
                                }
                                days = list.Find(predicate2);
                                if (days == null)
                                {
                                    days = new LeveDays();
                                    if (arry[i, 1] != Convert.ToDouble(0))
                                    {
                                        days.Platform = arry[i, 0] + "-" + arry[i, 1];
                                    }
                                    else
                                    {
                                        days.Platform = arry[i, 0] + " 以上";
                                    }
                                    days.Account = item.Account;
                                    days.OCount = 1M;
                                    days.Rate = Math.Round((decimal)((Convert.ToDecimal(days.OCount) / list2.Where(x => x.Account == days.Account).Count()) * 100M), 2);

                                    list.Add(days);
                                }
                                else
                                {
                                    days.OCount++;
                                    days.Rate = Math.Round((decimal)((Convert.ToDecimal(days.OCount) / list2.Where(x => x.Account == days.Account).Count()) * 100M), 2);
                                }
                            }
                        }
                    }
                }
            }
            List<object> list3 = new List<object> {
                new { Account = count }
            };
            return base.Json(new { rows = list.OrderByDescending(x => x.Platform).ThenBy(x => x.Account), footer = list3, total = list.Count });
        }

        public ActionResult GetOrderAmountTotal(string search)
        {
            string str = "";
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where Enabled=1 and " + str;
                }
            }
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            object o1 = base.NSession.CreateQuery("select SUM(Profit) from OrderType " + str + " and Profit<0").UniqueResult();

            object o2 = base.NSession.CreateQuery("select SUM(Profit) from OrderType " + str + " and Profit>0").UniqueResult();
            object o3 = base.NSession.CreateQuery("select SUM(Profit) from OrderType " + str + "").UniqueResult();
            List<object[]> o5 = base.NSession.CreateQuery("select CurrencyCode,SUM(Amount) from OrderType " + str + " Group By CurrencyCode").List<object[]>().ToList();
            o1 = (o1 == null ? "0" : o1);
            o2 = (o2 == null ? "0" : o2);
            o3 = (o3 == null ? "0" : o3);
            //o5 = (o5 == null ? "0" : o5);
            double total = 0;
            foreach (object[] objectse in o5)
            {
                total += GetAmount(objectse[0].ToString(), Convert.ToDouble(objectse[1].ToString()), currencies);
            }
            OrderAmountModel foo = new DDX.OrderManagementSystem.App.OrderAmountModel();
            foo.Loss = Math.Round(Utilities.ToDouble(o1.ToString().ToString()), 5);
            foo.Profit = Math.Round(Utilities.ToDouble(o2.ToString().ToString()), 5);
            foo.SUMProfit = Math.Round(Utilities.ToDouble(o3.ToString().ToString()), 5);
            foo.SumAmount = Math.Round(total, 2);
            foo.ProfitRate = Math.Round(foo.SUMProfit / (foo.SumAmount * 6.2) * 100, 2);
            return Json(foo);
        }

        /// <summary>
        /// 日盈亏报表
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetOrderAmount(string sort, string order, string search)
        {
            List<OrderAmountModel> list = new List<OrderAmountModel>();
            string str = "";
            string str2 = " order by Id desc";


            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                    str = " where Enabled=1 and " + str;
                }
            }

            //IList<object[]> list1 = base.NSession.CreateQuery("select Account,SUM(Profit) from OrderType " + str + " and Profit<0 Group BY Account").List<object[]>();
            //IList<object[]> list2 = base.NSession.CreateQuery("select Account,SUM(Profit) from OrderType " + str + " and Profit>0 Group BY Account").List<object[]>();
            //IList<object[]> list3 = base.NSession.CreateQuery("select Account,SUM(Profit) from OrderType " + str + " Group BY Account").List<object[]>();
            //IList<object[]> list5 = base.NSession.CreateQuery("select Account,SUM(Amount),max(CurrencyCode) from OrderType " + str + " Group BY Account").List<object[]>();
            // lazada平台帐号多货币情况统一转换为美元货币
            IList<object[]> list1 = base.NSession.CreateSQLQuery("select Account,SUM(Profit*b.CurrencyValue/c.CurrencyValue),'USD' from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) " + str + " and Profit<0 Group BY Account").List<object[]>();
            IList<object[]> list2 = base.NSession.CreateSQLQuery("select Account,SUM(Profit*b.CurrencyValue/c.CurrencyValue),'USD' from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) " + str + " and Profit>0 Group BY Account").List<object[]>();
            IList<object[]> list3 = base.NSession.CreateSQLQuery("select Account,SUM(Profit*b.CurrencyValue/c.CurrencyValue),'USD' from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) " + str + " Group BY Account").List<object[]>();
            IList<object[]> list5 = base.NSession.CreateSQLQuery("select Account,SUM(Amount*b.CurrencyValue/c.CurrencyValue),'USD' from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) " + str + " Group BY Account").List<object[]>();

            IList<AccountType> list4 = base.NSession.CreateQuery("from AccountType where AccountName " + (search.IndexOf("Account_uk&yw") != -1 ? " not " : "") + " like '%yw%'").List<AccountType>();
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            using (IEnumerator<AccountType> enumerator2 = list4.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {

                    AccountType accountType = enumerator2.Current;
                    if (accountType.AccountName != "su-smt")
                    {

                        OrderAmountModel foo = new DDX.OrderManagementSystem.App.OrderAmountModel
                            {
                                Account = accountType.AccountName,
                                Platform = accountType.Platform,

                                Profit = 0,
                                SUMProfit = 0,
                                SumAmount = 0,
                                Loss = 0,
                                OCount = 0
                            };
                        if (foo.Account.IndexOf("yw") != -1)
                        {
                            foo.Area = "义乌";
                        }
                        else
                        {
                            foo.Area = "宁波";
                        }
                        foreach (object[] fooo in list1)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.Loss = Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);
                                break;
                            }
                        }
                        foreach (object[] fooo in list2)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.Profit = Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);
                                break;
                            }
                        }
                        foreach (object[] fooo in list3)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.SUMProfit = Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);
                                break;
                            }
                        }
                        foreach (object[] fooo in list5)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.CurrencyCode = fooo[2].ToString();

                                foo.SumAmount = GetAmount(foo.CurrencyCode, Convert.ToDouble(fooo[1].ToString()), currencies);
                                break;
                            }
                        }
                        list.Add(foo);
                    }

                }
            }
            CurrencyType CurrencyUSD = currencies.Find(x => x.CurrencyCode == "USD");
            foreach (OrderAmountModel amount in list)
            {
                if (amount.SumAmount >= 1)
                    //amount.ProfitRate = Math.Round(amount.SUMProfit / (amount.SumAmount * 6.2) * 100, 2);
                    amount.ProfitRate = Math.Round(amount.SUMProfit / (amount.SumAmount * Convert.ToDouble(CurrencyUSD.CurrencyValue)) * 100, 2);
                else
                {
                    amount.ProfitRate = 0;
                }
            }
            //double dd = Math.Round(list.Sum(x => x.SUMProfit) / (list.Sum(x => x.SumAmount) * 6.2) * 100, 2);
            double dd = Math.Round(list.Sum(x => x.SUMProfit) / (list.Sum(x => x.SumAmount) * Convert.ToDouble(CurrencyUSD.CurrencyValue)) * 100, 2);
            if (double.IsNaN(dd))
            {
                dd = 0;
            }
            List<OrderAmountModel> footerlist = new List<OrderAmountModel> {
                new OrderAmountModel{ SumAmount =Math.Round(list.Sum(x=>x.SumAmount),2),SUMProfit = Math.Round(list.Sum(x=>x.SUMProfit),2),Profit = Math.Round(list.Sum(x=>x.Profit),2),Loss = Math.Round(list.Sum(x=>x.Loss),2), ProfitRate = dd}
            };
            return base.Json(new
            {
                total = list.Count,
                footer = footerlist,
                rows = from x in list
                       orderby x.Account
                       select x,
            });
        }

        //发货报表
        [HttpPost]
        public ActionResult GetFreightStatus(string sort, string order, string search, string search1)
        {
            List<FreightStatusCount> list = new List<FreightStatusCount>();
            string str = "";
            string str1 = "";
            string str2 = " order by Id desc";


            if (!(string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order)))
            {
                str2 = " order by " + sort + " " + order;
            }
            if (!string.IsNullOrEmpty(search))
            {
                str = Utilities.Resolve(search, true);
                if (str.Length > 0)
                {
                 //str = " where Enabled=1 and  IsFreight=1 and " + str;
                    str = " where Enabled=1 and " + str;
                }
            }
            if (!string.IsNullOrEmpty(search1))
            {
                str1 = Utilities.Resolve(search1, true);
                if (str1.Length > 0)
                {
                    str1 = " where  a.Account=d.Account and " + str1;
                }
            }
            //List<object[]> list1 = base.NSession.CreateSQLQuery("select a.Account,count(a.Id),round(sum(a.Amount),2),round(sum(a.Amount)*b.CurrencyValue/c.CurrencyValue*6.5,2),round(sum(a.ProductFees),2),round(sum(a.Freight),2) , (select sum(d.ExamineAmountRmb) from DisputeRecordType d  where d.OrderNo in (select a.OrderExNo from Orders a  " + str + " )) from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE())" + str + "  Group BY a.Account,c.CurrencyValue,b.CurrencyValue").List<object[]>().ToList();
            // lazada平台帐号多货币情况统一转换为美元货币
            #region list1~list5查询语句
            List<object[]> list1 = base.NSession.CreateSQLQuery("select a.Account,count(a.Id) from Orders a " + str + "  Group BY a.Account").List<object[]>().ToList();
            List<object[]> list2 = base.NSession.CreateSQLQuery("select a.Account,round(sum(a.Amount*b.CurrencyValue/c.CurrencyValue),2)  from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE()) " + str + "  Group BY a.Account,c.CurrencyValue,b.CurrencyValue").List<object[]>().ToList();
            List<object[]> list3 = base.NSession.CreateSQLQuery("select a.Account,round(sum(a.Amount)*b.CurrencyValue/c.CurrencyValue*6.5,2) from Orders a join FixedRate b on a.CurrencyCode=b.CurrencyCode AND b.Year=DATEPART(YEAR,GETDATE()) AND b.Month=DATEPART(MONTH,GETDATE()) join FixedRate c on 'USD'=c.CurrencyCode AND c.Year=DATEPART(YEAR,GETDATE()) AND c.Month=DATEPART(MONTH,GETDATE())" + str + "  Group BY a.Account,c.CurrencyValue,b.CurrencyValue").List<object[]>().ToList();
            List<object[]> list4 = base.NSession.CreateSQLQuery("select a.Account,round(sum(a.ProductFees),2) from Orders a " + str + "  Group BY a.Account ").List<object[]>().ToList();
            List<object[]> list5 = base.NSession.CreateSQLQuery("select a.Account,round(sum(a.Freight),2)  from Orders a " + str + "  Group BY a.Account ").List<object[]>().ToList();
            //List<object[]> list6 = base.NSession.CreateSQLQuery("select a.Account, (select sum(d.ExamineAmountRmb) from DisputeRecordType d  where d.OrderNo in (select a.OrderExNo from Orders a  " + str + " )) from Orders a " + str + "  Group BY a.Account").List<object[]>().ToList();
            List<object[]> list6 = base.NSession.CreateSQLQuery("select a.Account, (select isnull(sum(d.ExamineAmountRmb),0) from DisputeRecordType d  " + str1 + "  ) from Orders a " + str + "  Group BY a.Account ").List<object[]>().ToList();
            #endregion
            IList<AccountType> list44 = base.NSession.CreateQuery("from AccountType where AccountName " + (search.IndexOf("Account_uk&yw") != -1 ? " not " : "") + " like '%yw%'").List<AccountType>();
            List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            using (IEnumerator<AccountType> enumerator2 = list44.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {

                    AccountType accountType = enumerator2.Current;
                    if (accountType.AccountName != "su-smt")
                    {

                        FreightStatusCount foo = new DDX.OrderManagementSystem.App.FreightStatusCount
                        {
                            Account = accountType.AccountName,
                            Platform = accountType.Platform,
                            Amount = 0, //销售金额
                            AccountFee = 0,//平台费用RMB
                            Amountrmb = 0, //销售金额RMB
                            Profit = 0, //利润RMB
                            ProductFee = 0,   //产品费用RMB
                            FreightFee = 0,  //运费费用RMB
                            Loss = 0, //赔款额（纠纷登记）
                            ProfitRate = 0,//毛利润
                            OCount = 0 //订单数
                        };
                        //FreightStatusCount foo = new FreightStatusCount();
                        if (foo.Account.IndexOf("yw") != -1)
                        {
                            foo.Area = "义乌";
                        }
                        else
                        {
                            foo.Area = "宁波";
                        }
                        foreach (object[] fooo in list1)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.OCount += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 0);//订单数 
                                break;
                            }
                        }
                        foreach (object[] fooo in list2)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.Amount += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);//销售金额
                             //   break;
                            }
                        }
                        foreach (object[] fooo in list3)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.Amountrmb += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);//销售金额RMB
                                if (accountType.AccountName.Contains("smt"))
                                {
                                    foo.AccountFee += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5) * 0.05;
                                }
                                else
                                {
                                    foo.AccountFee += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5) * 0.15;
                                }
                              //  break;
                            }
                        }
                        foreach (object[] fooo in list4)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.ProductFee += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);//产品费用RMB
                             //   break;
                            }
                        }
                        foreach (object[] fooo in list5)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.FreightFee += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);//运费费用RMB
                              //  break;
                            }
                        }
                        foreach (object[] fooo in list6)
                        {
                            if (fooo[0].ToString().Trim() == accountType.AccountName)
                            {
                                foo.Loss += Math.Round(Utilities.ToDouble(fooo[1].ToString().ToString()), 5);//赔款额（纠纷登记）
                                foo.Profit += Math.Round(foo.Amountrmb - foo.ProductFee - foo.FreightFee - foo.Loss - foo.AccountFee, 5);//利润RMB
                              //  break;
                            }
                        }
                        list.Add(foo);
                    }

                }
            }
            CurrencyType CurrencyUSD = currencies.Find(x => x.CurrencyCode == "USD");
            foreach (FreightStatusCount amount in list)
            {
                if (amount.Amount >= 1)
                    //amount.ProfitRate = Math.Round(amount.SUMProfit / (amount.SumAmount * 6.2) * 100, 2);
                    amount.ProfitRate = Math.Round(amount.Profit / amount.Amountrmb * 100, 2);
                else
                {
                    amount.ProfitRate = 0;
                }
            }
            //double dd = Math.Round(list.Sum(x => x.SUMProfit) / (list.Sum(x => x.SumAmount) * 6.2) * 100, 2);
            double dd = Math.Round(list.Sum(x => x.Profit) / (list.Sum(x => x.Amountrmb)) * 100, 2);
            if (double.IsNaN(dd))
            {
                dd = 0;
            }
            List<FreightStatusCount> footerlist = new List<FreightStatusCount> {
                new FreightStatusCount{OCount=Math.Round(list.Sum(x=>x.OCount),0),Amount =Math.Round(list.Sum(x=>x.Amount),2), Amountrmb =Math.Round(list.Sum(x=>x.Amountrmb),2),ProductFee = Math.Round(list.Sum(x=>x.ProductFee),2),FreightFee = Math.Round(list.Sum(x=>x.FreightFee),2),Loss = Math.Round(list.Sum(x=>x.Loss),2),Profit =Math.Round(list.Sum(x=>x.Profit),2), ProfitRate = dd}

            };
            return base.Json(new
            {
                total = list.Count,
                footer = footerlist,
                rows = from x in list
                       orderby x.Account
                       select x,
            });
        }
    }
}
