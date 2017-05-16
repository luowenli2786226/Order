using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class FreightStatusCount
    {
        public string Account { get; set; }

        public string Platform { get; set; }
        /// <summary>
        /// 总订单数
        /// </summary>
        public double OCount { get; set; }

        //利润RMB
        public double Profit { get; set; }

        //利润RMB
        public double SUMProfit { get; set; }

        //产品费用RMB
        public double ProductFee { get; set; }

        //运费费用RMB
        public double FreightFee { get; set; }
  
        //销售金额
        public double Amount { get; set; }

        //销售金额RMB
        public double Amountrmb { get; set; }

        //平台费用RMB
        public double AccountFee { get; set; }

        //毛利润
        public double ProfitRate { get; set; }

        //赔款额（纠纷登记）
        public double Loss { get; set; }


        public string Area { get; set; }

        public string CurrencyCode { get; set; }
    }
}