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
    public class PackagingScanLogController : BaseController
    {
        //
        // GET: /PackagingScanLog/

        public ActionResult Index()
        {
            return base.View();
        }

        public ActionResult Log()
        {
            return base.View();
        }

        public string GetLog(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };
            int Year = int.Parse(search.Split('&')[0]);
            int Month = int.Parse(search.Split('&')[1]);


            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportPackagingScan";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = Year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }

        public string GetLogII(int page, int rows, string sort, string order, string search)
        {
            if (search == null) { return ""; };
            int Year = int.Parse(search.Split('&')[0]);
            int Month = int.Parse(search.Split('&')[1]);


            DataSet ds = new DataSet();
            IDbCommand command = NSession.Connection.CreateCommand();
            command.CommandTimeout = 500;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "U_ReportPackagingScanP";

            var parameter1 = command.CreateParameter();
            parameter1.ParameterName = "@Year";
            parameter1.Value = Year;

            var parameter2 = command.CreateParameter();
            parameter2.ParameterName = "@Month";
            parameter2.Value = Month;

            command.Parameters.Add(parameter1);
            command.Parameters.Add(parameter2);

            SqlDataAdapter da = new SqlDataAdapter(command as SqlCommand);
            da.Fill(ds);

            string result = JsonConvert.SerializeObject(ds.Tables[0]);
            return result;
        }
    }
}
