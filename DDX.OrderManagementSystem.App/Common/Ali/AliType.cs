using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{


    public class AmountTotal
    {
        public string amount { get; set; }
        public string cent { get; set; }
        public string currencyCode { get; set; }
        public string centFactor { get; set; }
        public Currency currency { get; set; }
    }



    public class AffiliateCommission
    {
        public string amount { get; set; }
        public string cent { get; set; }
        public string currencyCode { get; set; }
        public string centFactor { get; set; }
        public Currency currency { get; set; }
    }



    public class EscrowFee
    {
        public string amount { get; set; }
        public string cent { get; set; }
        public string currencyCode { get; set; }
        public string centFactor { get; set; }
        public Currency currency { get; set; }
    }


    public class RealLoanAmount
    {
        public decimal amount { get; set; }
        public string cent { get; set; }
        public string currencyCode { get; set; }
        public string centFactor { get; set; }
        public Currency currency { get; set; }
    }

    public class SonOrderList
    {
        public string loanStatus { get; set; }
        public AffiliateCommission affiliateCommission { get; set; }
        public EscrowFee escrowFee { get; set; }
        public string childOrderId { get; set; }
        public RealLoanAmount realLoanAmount { get; set; }
    }

    public class POrderList
    {
        public AmountTotal amountTotal { get; set; }
        public string orderId { get; set; }
        public List<SonOrderList> sonOrderList { get; set; }
    }

    public class LoanRootObject
    {
        public int totalItem { get; set; }
        public List<POrderList> orderList { get; set; }
    }
}