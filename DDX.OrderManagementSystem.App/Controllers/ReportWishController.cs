using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App.Common;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class ReportWishController:BaseController
    {
        public ViewResult ReportWishTotleII()
        {
            return base.View();
        }
        public string  ReportFactWish(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };

            string sArea = search.Split('&')[0];
            int Year = int.Parse(search.Split('&')[1]);
            int Month = int.Parse(search.Split('&')[2]);
            string account = search.Split('&')[3];
            string isfreight = search.Split('&')[4];

            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportWishII";


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
        public JsonResult ReportWishFactDetailExcel(string search)
        { 
         try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string account = search.Split('&')[3];
                string IsFreight = search.Split('&')[4];
                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_ReportWishDetailII";


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
        public JsonResult PaymentWishDetailsToExcel(string search)
        {
            try
            {
                string sArea = search.Split('&')[0];
                int Year = int.Parse(search.Split('&')[1]);
                int Month = int.Parse(search.Split('&')[2]);
                string account = search.Split('&')[3];
                string isfreight = search.Split('&')[4];

                DataSet ds = new DataSet();
                IDbCommand command = NSession.Connection.CreateCommand();
                command.CommandTimeout = 500;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "U_WishPaymentDetail";


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
    }
}