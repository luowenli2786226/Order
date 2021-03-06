﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DDX.OrderManagementSystem.App
{
    public class AeopAEProductDisplayDTOList
    {
        public string gmtModified { get; set; }
        public string gmtCreate { get; set; }
        public string freightTemplateId { get; set; }
        public string ownerMemberSeq { get; set; }
        public string subject { get; set; }
        public string imageURLs { get; set; }
        public string ownerMemberId { get; set; }
        public string wsOfflineDate { get; set; }
        public string productId { get; set; }
        public string productMinPrice { get; set; }
        public string wsDisplay { get; set; }
        public string productMaxPrice { get; set; }
    }

    public class AliProductListRoot
    {
        public List<AeopAEProductDisplayDTOList> aeopAEProductDisplayDTOList { get; set; }
        public string productCount { get; set; }
        public string currentPage { get; set; }
        public string success { get; set; }
        public int totalPage { get; set; }
    }

    public class AeopSKUProperty
    {
        public string propertyValueId { get; set; }
        public string skuImage { get; set; }
        public string skuPropertyId { get; set; }
    }

    public class AeopAeProductSKUs
    {
        public string id { get; set; }
        public string ipmSkuStock { get; set; }
        public string skuPrice { get; set; }
        public string skuStock { get; set; }
        public List<AeopSKUProperty> aeopSKUProperty { get; set; }
        public string skuCode { get; set; }
    }

    public class AeopAeProductPropertys
    {
        public string attrValueId { get; set; }
        public string attrNameId { get; set; }
    }

    public class ALiProductRootObject
    {
        public string bulkOrder { get; set; }
        public string lotNum { get; set; }
        public string summary { get; set; }
        public List<AeopAeProductSKUs> aeopAeProductSKUs { get; set; }
        public string detail { get; set; }
        public string packageType { get; set; }
        public string freightTemplateId { get; set; }
        public string subject { get; set; }
        public string productMoreKeywords1 { get; set; }
        public string reduceStrategy { get; set; }
        public string productMoreKeywords2 { get; set; }
        public string productUnit { get; set; }
        public string wsOfflineDate { get; set; }
        public string sizechartId { get; set; }
        public string packageLength { get; set; }
        public string wsDisplay { get; set; }
        public string isImageDynamic { get; set; }
        public string packageHeight { get; set; }
        public string packageWidth { get; set; }
        public string isPackSell { get; set; }
        public string ownerMemberSeq { get; set; }
        public string categoryId { get; set; }
        public string keyword { get; set; }
        public string imageURLs { get; set; }
        public string ownerMemberId { get; set; }
        public string productStatusType { get; set; }
        public List<AeopAeProductPropertys> aeopAeProductPropertys { get; set; }
        public string grossWeight { get; set; }
        public string productId { get; set; }
        public string groupId { get; set; }

        public string deliveryTime { get; set; }
        public string wsValidNum { get; set; }
        public string bulkDiscount { get; set; }
        public string promiseTemplateId { get; set; }
        public string success { get; set; }
        public string productPrice { get; set; }
    }

}
