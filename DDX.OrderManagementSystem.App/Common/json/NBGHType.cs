using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App.Common.json
{
    public class RequestParamType
    {
       
        /// <summary>
        /// 大客户编号
        /// </summary>
        public string ecCompanyId { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string eventTime { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string whCode { get; set; }
        /// <summary>
        /// 电商平台物流订单号
        /// </summary>
        public string logisticsOrderId { get; set; }
        /// <summary>
        /// 业务交易号
        /// </summary>
        public string tradeId { get; set; }
        /// <summary>
        /// 物流公司代码
        /// </summary>
        public string LogisticsCompany { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string LogisticsBiz { get; set; }
        /// <summary>
        /// 电商标识
        /// </summary>
        public string mailType { get; set; }
        /// <summary>
        /// 接口类型
        /// </summary>
        public int faceType { get; set; }
    }
    public class NBGHReturnType
    {
        public string return_success { get; set; }
        public List<barCodeList> barCodeList { get; set; }
    }
    public class OrderResponse
    {
 
    }
    public class barCodeList
    {
        public string bar_code { get; set; }
        public string logisticsOrderId { get; set; }
    }
    public class LogisticsCompany
    {
        public List<DataLogisticsCompany> data { get; set; }
       
    }
    public class DataLogisticsCompany
    {
        public string businessCode { get; set; }
        public string businessName { get; set; }
        public string companyCode { get; set; }
        public string companyName { get; set; }
    }
    public class LogisticsBiz
    {
        public List<DataLogisticsBiz> data { get; set; }
    }
    public class DataLogisticsBiz
    {
        public string businessCode { get; set; }
        public string businessName { get; set; }
        public string companyCode { get; set; }
        public string companyName { get; set; }
    }
}