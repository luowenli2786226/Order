using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DDX.OrderManagementSystem.Domain;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ReportsIIController : BaseController
    {

        public ActionResult DayOrder()
        {
            return base.View();
        }

        public ActionResult ReportExpectTotle()
        {
            return base.View();
        }

        public ActionResult ReportExpectTotleI()
        {
            return base.View();
        }

        public ActionResult ReportInvoiceTotleI()
        {
            return base.View();
        }

        public ActionResult ReportInvoiceTotleII()
        {
            return base.View();
        }

        public ActionResult ReportFactTotleI()
        {
            return base.View();
        }

        public ActionResult ReportFactTotleII()
        {
            return base.View();
        }

        public ActionResult ReportFactTotleIII()
        {
            return base.View();
        }

        public ActionResult ReportProfitTotle()
        {
            return base.View();
        }
        public ActionResult ReportReturngoodsTotle()
        {
            return base.View();
        }
        public ActionResult ReportWishTotle()
        {
            return base.View();
        }
        /// <summary>
        /// 获取日订单报表
        /// </summary>
        public string GetDayOrder(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            //int Year = 2016;
            //int Month = 12;
            //string Sku = "0";

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportOrderDayII";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }

        /// <summary>
        /// 预计核算I
        /// </summary>
        public string ReportExpectI(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportExpectI";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 预计核算II
        /// </summary>
        public string ReportExpectII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportExpectII";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 运单管理I
        /// </summary>
        public string ReportInvoiceI(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportInvoiceI";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 运单管理II
        /// </summary>
        public string ReportInvoiceII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportInvoiceII";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 实际核算I
        /// </summary>
        public string ReportFactI(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportFactI";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 实际核算II
        /// </summary>
        public string ReportFactII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportFactII";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 实际核算II
        /// </summary>
        public string ReportFactIII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportFactIII";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }

        /// <summary>
        /// 退件订单
        /// </summary>
        public string ReportStatusI(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string sstatus = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportStatusI";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@sstatus";
            parameter6.Value = sstatus;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 退件订单-明细
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportStatusIDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string sstatus = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportStatusIDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;
                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@sstatus";
                parameter6.Value = sstatus;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 预计核算-明细
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportExpectDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportExpectDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 运单管理-明细
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportInvoiceDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportInvoiceDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 实际核算-明细-下载(导入收汇)
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportFactDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportFactDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 实际核算-明细-下载(自动收汇)
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportFactDetailExcelII(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportFactDetailII";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 利润-明细-下载
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportProfitDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportProfitDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 赔款-明细-下载
        /// </summary>
        /// <returns></returns>
        public JsonResult PaymentDetailsToExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_PaymentDetail";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 其它费用-下载
        /// </summary>
        /// <returns></returns>
        public JsonResult OtherExpensesToExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_OtherExpenses";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 退货-明细-下载
        /// </summary>
        /// <returns></returns>
        public JsonResult ReturnDetailsToExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string platform = search.Split('&')[3];
                string account = search.Split('&')[4];
                string isfreight = search.Split('&')[5];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportOrderReturn";

                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@platform";
                parameter1.Value = platform;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;

                var parameter6 = command.CreateParameter();
                parameter6.ParameterName = "@IsFreight";
                parameter6.Value = isfreight;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);
                command.Parameters.Add(parameter6);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }
        /// <summary>
        /// 利润
        /// </summary>
        public string ReportProfit(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string platform = search.Split('&')[3];
            string account = search.Split('&')[4];
            string isfreight = search.Split('&')[5];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportProfitI";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@platform";
            parameter1.Value = platform;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;

            var parameter6 = command.CreateParameter();
            parameter6.ParameterName = "@IsFreight";
            parameter6.Value = isfreight;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);
            command.Parameters.Add(parameter6);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }


        /// <summary>
        /// 通途数据列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public string ReportWishII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string account = search.Split('&')[3];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportWish";

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@account";
            parameter2.Value = account;

            var parameter3 = command.CreateParameter();
            parameter3.ParameterName = "@area";
            parameter3.Value = sArea;

            var parameter4 = command.CreateParameter();
            parameter4.ParameterName = "@year";
            parameter4.Value = Year;

            var parameter5 = command.CreateParameter();
            parameter5.ParameterName = "@month";
            parameter5.Value = Month;


            command.Parameters.Add(parameter2);
            command.Parameters.Add(parameter3);
            command.Parameters.Add(parameter4);
            command.Parameters.Add(parameter5);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
        /// <summary>
        /// 通途数据报表明细下载
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public JsonResult ReportwishDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string account = search.Split('&')[3];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportWishDetail";


                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@account";
                parameter2.Value = account;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@area";
                parameter3.Value = sArea;

                var parameter4 = command.CreateParameter();
                parameter4.ParameterName = "@year";
                parameter4.Value = Year;

                var parameter5 = command.CreateParameter();
                parameter5.ParameterName = "@month";
                parameter5.Value = Month;



                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);
                command.Parameters.Add(parameter4);
                command.Parameters.Add(parameter5);


                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        /// <summary>
        /// 实际核算-月库损明细下载(导入收汇)
        /// </summary>
        /// <returns></returns>
        public JsonResult ReportGoodsDiscountDetailExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportGoodsDiscountDetail";
                var parameter1 = command.CreateParameter();
                parameter1.ParameterName = "@area";
                parameter1.Value = sArea;

                var parameter2 = command.CreateParameter();
                parameter2.ParameterName = "@year";
                parameter2.Value = Year;

                var parameter3 = command.CreateParameter();
                parameter3.ParameterName = "@month";
                parameter3.Value = Month;

                command.Parameters.Add(parameter1);
                command.Parameters.Add(parameter2);
                command.Parameters.Add(parameter3);

                SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
                da.Fill(ds);

                Session["ExportDown"] = ExcelHelper.GetExcelXml(ds.Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

    }
}
