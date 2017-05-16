using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using NHibernate;
using DDX.OrderManagementSystem.App.Controllers;
using System.Security;
using DDX.OrderManagementSystem.Domain;
using Comm;

namespace DDX.OrderManagementSystem.App.Synchronous
{
    /// <summary>
    /// Warehouse 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Warehouse : BaseController
    {
        // 允许访问IP
        private string _IP = "192.168.8.98,192.168.0.145,218.0.1.171,47.89.192.70,61.130.107.155,61.130.107.157";
        private string _Token = "A7151321-2C94-4059-90C9-E7C9754DF4A4";
        private string UserHostAddress;
        private string UserHostName;

        public Warehouse()
        {
            // 限定IP
            UserHostAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
            UserHostName = System.Web.HttpContext.Current.Request.UserHostName;

            if (_IP.Contains(UserHostAddress) == false)
            {
                LogInfo.WriteSynchronousLog(UserHostAddress + " HostName: " + UserHostName + " [access barred!]");
                throw new SecurityException("access barred!");
            }

        }

        //[WebMethod]
        //public string HelloWorld()
        //{
        //    return "Hello World";
        //}

        /// <summary>
        /// 获取订单(宁波)
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public DataTable GetOrders(string Token, DateTime Begin, DateTime End)
        {
            if (_Token != Token)
            {
                throw new SecurityException("illegally accessed!");
            }
            LogInfo.WriteSynchronousLog(UserHostName + "[" + UserHostAddress + "][获取订单]");

            string sql = @"SELECT 
(case when O.Platform='Ebay' then  O.TId else O.OrderExNo end) as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,(STUFF((SELECT ','+SKU FROM OrderProducts t WHERE OrderNo=O.OrderNo order by id  for xml path('')), 1, 1, '')) AS SKU,(STUFF((SELECT ','+rtrim(Qty) FROM OrderProducts t WHERE OrderNo=O.OrderNo order by id  for xml path('')), 1, 1, '')) AS 数量,0 AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID',ltrim(0)+'*'+ltrim(0)+'*'+ltrim(0) as  '长宽高'
 from Orders O 
 left join Account A on A.AccountName=O.Account
 left join OrderAddress OA on O.AddressId=OA.Id  where A.FromArea='宁波' and O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn >= '" + Begin.ToString("yyyy-MM-dd HH:mm:ss") + "'and ScanningOn <= '" + End.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.IsFBA=1 and O.FBABy in('US-East','KS') and O.Enabled=1 and O.IsAudit=1";

            // 暂时移除义乌'KS-YW'，'CA'仓库
            //            string sql = @"SELECT 
            //(case when O.Platform='Ebay' then  O.TId else O.OrderExNo end) as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,P.sku AS SKU,op.Qty AS 数量,P.Weight*0.001/1000*op.Qty AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID',ltrim(Long)+'*'+ltrim(P.Wide)+'*'+ltrim(P.High) as  '长宽高'
            // from Orders O 
            // left join OrderProducts OP on O.Id=OP.OId  
            // left join Products P on OP.SKU=P.SKU 
            // left join OrderAddress OA on O.AddressId=OA.Id  where  O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn >= '" + Begin.ToString("yyyy-MM-dd HH:mm:ss") + "'and ScanningOn <= '" + End.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.IsFBA=1 and O.FBABy in('US-East','KS-YW','KS','CA') and O.Enabled=1 and O.IsAudit=1";

            // KS-YW 出货少

            DataSet set2 = this.GetOrderExport(sql);
            //DataTable table = set2.Tables[0].Clone();

            //set2.Tables[0].TableName = "Sheet1";
            return set2.Tables[0];
        }

        /// <summary>
        /// 获取订单(义乌)
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public DataTable GetOrdersYW(string Token, DateTime Begin, DateTime End)
        {
            if (_Token != Token)
            {
                throw new SecurityException("illegally accessed!");
            }
            LogInfo.WriteSynchronousLog(UserHostName + "[" + UserHostAddress + "][获取订单]");

            string sql = @"SELECT 
(case when O.Platform='Ebay' then  O.TId else O.OrderExNo end) as '店铺单号',O.Account as 交易ID,CONVERT(VARCHAR(10),O.GenerateOn,120) as 交易时间,OA.CountryCode as 收货国家代码,OA.Addressee as 收货人,OA.Street AS 收货人地址,'' AS 收货人地址2,OA.City AS 收货城市,OA.Province AS 州或省,OA.PostCode AS 收货邮编,OA.Phone AS 收货人电话,(STUFF((SELECT ','+SKU FROM OrderProducts t WHERE OrderNo=O.OrderNo order by id  for xml path('')), 1, 1, '')) AS SKU,(STUFF((SELECT ','+rtrim(Qty) FROM OrderProducts t WHERE OrderNo=O.OrderNo order by id  for xml path('')), 1, 1, '')) AS 数量,0 AS 重量KG,'' AS 报关价格$,'' AS 原产国代码,'' AS '买家ID','' AS '店铺运输方式','' AS '运费总额','' AS '商品itemID','' AS 'ebay交易ID',ltrim(0)+'*'+ltrim(0)+'*'+ltrim(0) as  '长宽高'
 from Orders O 
 left join Account A on A.AccountName=O.Account
 left join OrderAddress OA on O.AddressId=OA.Id  where A.FromArea='义乌' and O.Status='已发货' and (O.TrackCode is null or O.TrackCode ='已用完') and  ScanningOn >= '" + Begin.ToString("yyyy-MM-dd HH:mm:ss") + "'and ScanningOn <= '" + End.ToString("yyyy-MM-dd HH:mm:ss") + "' and O.IsFBA=1 and O.FBABy in('YWCA-WEST(DONG)','CA') and O.Enabled=1 and O.IsAudit=1";

            DataSet set2 = this.GetOrderExport(sql);
            return set2.Tables[0];
        }
        //[WebMethod]
        //public void test2()
        //{
        //    GetOrders("A7151321-2C94-4059-90C9-E7C9754DF4A4", Convert.ToDateTime("2016-10-31"), Convert.ToDateTime("2016-10-31"));
        //}

        //[WebMethod]
        //public void test()
        //{
        //    DataTable dt2 = new DataTable("sheet1");
        //    DataRow row;
        //    row = dt2.NewRow();

        //    // Add three column objects to the table.
        //    DataColumn idColumn = new DataColumn();
        //    idColumn.DataType = System.Type.GetType("System.String");
        //    idColumn.ColumnName = "OrderExNo";
        //    //idColumn.AutoIncrement = true;
        //    dt2.Columns.Add(idColumn);

        //    // Add three column objects to the table.
        //    DataColumn idColumn2 = new DataColumn();
        //    idColumn2.DataType = System.Type.GetType("System.String");
        //    idColumn2.ColumnName = "TrackCode";
        //    //idColumn2.AutoIncrement = true;
        //    dt2.Columns.Add(idColumn2);


        //    // Then add the new row to the collection.
        //    //row["OrderExNo"] = "A7151321-2C94-4059-90C9-E7C9754DF4A4";
        //    row["OrderExNo"] = "107-3417026-9880241";
        //    row["TrackCode"] = "测试跟踪码1";
        //    dt2.Rows.Add(row);

        //    //List<ResultInfo> resultInfos = new List<ResultInfo>();
        //    var o = SetTrackNumber("A7151321-2C94-4059-90C9-E7C9754DF4A4", dt2);
        //}

        /// <summary>
        /// 上传跟踪码
        /// </summary>
        /// <param name="TrackList">两列：A列 订单号||| B列 产品运单号</param>
        /// <returns></returns>
        [WebMethod]
        public List<ResultInfo> SetTrackNumber(string Token, DataTable TrackNumber)
        {
            if (_Token != Token)
            {
                throw new SecurityException("illegally accessed!");
            }
            LogInfo.WriteSynchronousLog(UserHostName + "[" + UserHostAddress + "][上传跟踪码]");

            DataTable dt = TrackNumber;
            List<ResultInfo> resultInfos = new List<ResultInfo>();
            if (dt.Columns.Count >= 2)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] == null)
                    {
                        continue;
                    }
                    List<OrderType> orderTypes =
                        NSession.CreateQuery("from OrderType where OrderExNo='" + row[0].ToString().Trim() + "' Or TId='" + row[0].ToString().Trim() + "'").List
                            <OrderType>().ToList();
                    if (orderTypes.Count == 0)
                    {
                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "该单号不在系统中", "失败"));
                        continue;

                    }

                    foreach (var orderType in orderTypes)
                    {
                        UserType CurrentUser = new UserType { Username = "sys_auto", Realname = "sys_auto" };
                        LoggerUtil.GetOrderRecord(orderType, "海个仓订单跟踪码导入", "设置发货单号为：" + orderType.TrackCode + " 替换为" + row[1].ToString(), CurrentUser, NSession);
                        orderType.TrackCode = row[1].ToString();
                        //orderType.Status = "已发货";
                        //orderType.IsFreight = 1;
                        //orderType.Freight = 0.01;
                        NSession.Update(orderType);
                        NSession.Flush();

                        // 上传跟踪码(未确认暂时不上传到服务器)
                        OrderHelper.UploadTrackCode(orderType, NSession);

                        resultInfos.Add(OrderHelper.GetResult(row[0].ToString(), "订单标记发货", "成功"));
                    }
                }

            }

            return resultInfos;
        }

        private DataSet GetOrderExport(string sql)
        {
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = sql + " order by O.OrderExNo,O.OrderNo asc";
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return dataSet;
        }
    }
}
