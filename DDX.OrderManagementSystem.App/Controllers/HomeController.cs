using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDX.NHibernateHelper;
using NHibernate;
using DDX.OrderManagementSystem.Domain;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using System.Web.UI;
using System.IO;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Management;
using System.Net;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult AccountList(string id)
        {
            IList<AccountType> list = base.NSession.CreateQuery(" from AccountType where Status=0 and Platform=:p").SetString("p", id).List<AccountType>();
            List<object> data = new List<object> {
                new { id = "ALL", text = "ALL" }
            };
            foreach (AccountType type in list)
            {
                data.Add(new { id = type.Id, text = type.AccountName });
            }
            return base.Json(data);
        }

        public ActionResult Create()
        {
            return base.View();
        }

        public ActionResult GetDPF()
        {
            var mimeType = "application/pdf";
            var fileDownloadName = DateTime.Now.Ticks.ToString() + ".pdf";
            //LoggerUtil.GetOrderRecord(orderType, "订单打印", "国际E邮宝打印！", base.CurrentUser, base.NSession);

            return File((byte[])Session["pdf"], mimeType, fileDownloadName);

        }

        public ActionResult Default()
        {
            return base.View();
        }


        public ActionResult GetAliexpressAuthCode(string code)
        {
            base.ViewData["code"] = code;
            return base.View();
        }

        private string GetClientIP()
        {
            string result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (null == result || result == String.Empty)
            {
                result = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            return result;
        }

        public ActionResult GetCompetence()
        {
            return base.View();
        }

        public ActionResult GetEbayLogistics()
        {
            List<string> list = new List<string> { "China Post", "Chunghwa Post", "DHL", "FedEx", "Hong Kong Post", "TNT", "UPS", "USPS" };
            List<object> data = new List<object>();
            foreach (string str in list)
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        public static string GetMACByIP(string ip)
        {
            string str;
            try
            {
                byte[] mac = new byte[6];
                int dest = inet_addr(ip);
                int length = 6;
                int num3 = SendARP(dest, 0, mac, ref length);
                str = BitConverter.ToString(mac, 0, 6);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public ActionResult GetProductAttributeEnum()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(ProductAttributeEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        public JsonResult GetResult()
        {
            List<ResultInfo> list = new List<ResultInfo>();
            if (base.Session["Results"] != null)
            {
                list = base.Session["Results"] as List<ResultInfo>;
            }
            return base.Json(new { total = list.Count, rows = list });
        }

        public ActionResult GetRoleEnum()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(RoleEnum)))
            {
                RoleEnum enum2 = (RoleEnum)Enum.Parse(typeof(RoleEnum), str);
                data.Add(new { id = enum2, text = str });
            }
            return base.Json(data);
        }

        public ActionResult GetSMTLogistics()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("DHL Global Mail", "EMS_SH_ZX_US");
            dictionary.Add("EMS", "EMS");
            dictionary.Add("Seller's Shipping Method", "Other");
            dictionary.Add("Sweden Post", "SEP");
            dictionary.Add("Fedex IP", "FEDEX");
            dictionary.Add("UPS Expedited", "UPSE");
            dictionary.Add("ePacket", "EMS_ZX_ZX_US");
            dictionary.Add("FEDEX_IE", "FEDEX_IE");
            dictionary.Add("Ruston Package", "RUSTON");
            dictionary.Add("HongKong Post Air Parcel", "HKPAP");
            dictionary.Add("China Post Air Mail", "CPAM");
            dictionary.Add("SF Express", "SF");
            dictionary.Add("HongKong Post Air Mail", "HKPAM");
            dictionary.Add("Swiss Post", "CHP");
            dictionary.Add("ZTO Express to Russia", "ZTORU");
            dictionary.Add("ARAMEX", "ARAMEX");
            dictionary.Add("China Post Air Parcel", "CPAP");
            dictionary.Add("TNT", "TNT");
            dictionary.Add("139 ECONOMIC Package", "ECONOMIC139");
            dictionary.Add("DHL", "DHL");
            dictionary.Add("UPS", "UPS");
            dictionary.Add("Singapore Post", "SGP");
            dictionary.Add("China Post Ordinary Small Packet Plus", "YANWEN_JYT");
            List<object> data = new List<object>();
            foreach (string str in dictionary.Keys)
            {
                data.Add(new { id = dictionary[str], text = str });
            }
            return base.Json(data);
        }

        public ActionResult GetWishLogistics()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("中国邮政", "ChinaAirPost");
            dictionary.Add("E邮宝", "EPacket");
            dictionary.Add("荷兰邮政", "NewZealandPost");

            dictionary.Add("新加坡小包", "SingaporePost");
            dictionary.Add("燕文平邮", "Yanwen");

            List<object> data = new List<object>();
            foreach (string str in dictionary.Keys)
            {
                data.Add(new { id = dictionary[str], text = str });
            }
            return base.Json(data);

        }

        public ViewResult Index()
        {
            base.ViewData["Username"] = base.CurrentUser.Realname;
            this.UserLogin();
            return base.View();
        }

        [DllImport("Ws2_32.dll")]
        private static extern int inet_addr(string ip);
        public ActionResult OrderStatus(string Id)
        {
            List<object> data = new List<object>();
            if (!string.IsNullOrEmpty(Id))
            {
                data.Add(new { id = "ALL", text = "ALL" });
            }
            foreach (string str in Enum.GetNames(typeof(OrderStatusEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        public ActionResult Platform(string Id)
        {
            List<object> data = new List<object>();
            if (Id == "1")
            {
                data.Add(new { id = "ALL", text = "ALL" });
            }
            if (Id == "2")
            {
                data.Add(new { id = "不限", text = "不限" });
                data.Add(new { id = "无侵权", text = "无侵权" });
                data.Add(new { id = "全侵权", text = "全侵权" });
            }
            foreach (string str in Enum.GetNames(typeof(PlatformEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }
        public ActionResult OrderPdtType(string Id)
        {
            List<object> data = new List<object>();
            if (!string.IsNullOrEmpty(Id))
            {
                data.Add(new { id = "ALL", text = "ALL" });
            }
            foreach (string str in Enum.GetNames(typeof(COnfigTempleCategory)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PostData(string ids)
        {
            base.Session["ids"] = ids;
            return base.Json(new { IsSuccess = true });
        }

        public ActionResult PrintCategory()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(PrintCategoryEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        public ContentResult PrintData(int Id)
        {
            PrintDataType type = base.NSession.Get<PrintDataType>(Id);
            return this.Content(type.Content, "text/xml", Encoding.UTF8);
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PrintDesign(string Id)
        {
            base.ViewData["id"] = Id;
            return base.View();
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PrintDetail(string Id)
        {
            base.ViewData["grf"] = Id;
            return base.View();
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ContentResult PrintGrf(string Id)
        {
            base.NSession.Clear();
            object obj2 = base.NSession.CreateQuery("select Content from PrintTemplateType where Id=" + Id).UniqueResult();
            return this.Content(obj2.ToString(), "text/xml", Encoding.UTF8);
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ContentResult PrintOrder(int Id)
        {
            string str = "select * from Orders O left join OrderAddress OA ON O.AddressId=OA.Id  where O.id=" + Id;
            DataSet dataSet = new DataSet();
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = str;
            new SqlDataAdapter(command as SqlCommand).Fill(dataSet);
            return base.Content(dataSet.GetXml(), "text/xml");
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public JsonResult PrintSave(string Id)
        {
            base.NSession.Clear();
            PrintTemplateType type = base.NSession.Get<PrintTemplateType>(Convert.ToInt32(Id));
            byte[] bytes = base.Request.BinaryRead(base.Request.TotalBytes);
            type.Content = Encoding.UTF8.GetString(bytes);
            base.NSession.Update(type);
            base.NSession.Flush();
            return base.Json(new { IsSuccess = 1 });
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PrintSetup(string ids, string type)
        {
            base.ViewData["ids"] = base.Session["ids"];
            return base.View();
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PrintSingleSetup(string ids, string type)
        {
            base.ViewData["ids"] = base.Session["ids"];
            return base.View();
        }

        public ActionResult ProductStatus()
        {
            List<object> data = new List<object>();
            foreach (string str in Enum.GetNames(typeof(ProductStatusEnum)))
            {
                data.Add(new { id = str, text = str });
            }
            return base.Json(data);
        }

        public ActionResult Result()
        {
            return base.View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveFile(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                try
                {
                    string str;
                    string str2;
                    string str3;
                    this.SaveFile(fileData, out str, out str2, out str3);
                    return base.Json(new { Success = true, FileName = str2, SaveName = str3, FilePath = base.Server.MapPath("~" + str3) });
                }
                catch (Exception exception)
                {
                    return base.Json(new { Success = false, Message = exception.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return base.Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);
        }

        private void SaveFile(HttpPostedFileBase fileData, out string filePath, out string fileName, out string saveName)
        {
            filePath = "/Uploads/";
            filePath = filePath + DateTime.Now.ToString("yyyyMMdd") + "/";
            if (!Directory.Exists(base.Server.MapPath("~" + filePath)))
            {
                Directory.CreateDirectory(base.Server.MapPath("~" + filePath));
            }
            fileName = Path.GetFileName(fileData.FileName);
            string extension = Path.GetExtension(fileName);
            saveName = filePath + Guid.NewGuid().ToString() + extension;
            fileData.SaveAs(base.Server.MapPath("~" + saveName));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SavePic(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                try
                {
                    string str;
                    string fileNameWithoutExtension;
                    string str3;
                    this.SaveFile(fileData, out str, out fileNameWithoutExtension, out str3);
                    str = base.Server.MapPath("~");
                    string img = str3;
                    fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
                    str3 = base.Server.MapPath("~" + str3);

                    IList<ProductType> list = base.NSession.CreateQuery(" from ProductType where SKU='" + fileNameWithoutExtension + "' or OldSKU='" + fileNameWithoutExtension + "' ").List<ProductType>();
                    foreach (ProductType type in list)
                    {
                        type.PicUrl = img;
                        type.SPicUrl = img;
                        //Utilities.DrawImageRectRect(str3, str + type.PicUrl, 0x80, 0x80);
                        base.NSession.SaveOrUpdate(type);
                        base.NSession.Flush();
                    }
                    return base.Json(new { Success = true, FileName = fileNameWithoutExtension, SaveName = str + str3, FilePath = str });
                }
                catch (Exception exception)
                {
                    return base.Json(new { Success = false, Message = exception.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return base.Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);
        }

        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(int dest, int host, byte[] mac, ref int length);

        public JsonResult SetPrintData(string ids)
        {

            //string format = "select O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',P.Location as '库位',P.Category as 分类中文 from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU   left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo  ";

            // 1、12小时内打印仓库入库人名 2、12小时外打印库位
           // string format = "select   [dbo].[F_GetProducts]('{0}') as '产品名',[dbo].[F_GetSKU]('{0}') as '物品SKU1',[dbo].[F_GetSKU1]('{0}') as '物品SKU2',[dbo].[F_GetQty]('{0}') as '物品总Qty','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=20 then (case when Amount<=5 then Amount else 5 end) else 10 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo ";
            string format = "select [dbo].[F_GetSKU2](O.OrderNo) as '物品SKU2',[dbo].[F_GetProducts](O.OrderNo) as '产品名','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=40 then (case when Amount<=5 and Amount>0 then Amount else 5 end) else 8 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo  "; //O.OrderNo 订单按SKU升序进行排序 OP.SKU 【不允许按SKU排序原因：多条订单批量打印时原订单商品SKU打印排序被打乱，造成1个订单打印出多个面单情况】

            format = string.Format(format, ids.Replace(",", "','"));
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = format;
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            List<string> list2 = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                object obj = NSession.CreateSQLQuery("select top 1 AreaName from [LogisticsArea] where LId = (select top 1 ParentID from LogisticsMode where LogisticsCode='" + row["订单发货方式"] + "')  and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode  and CountryCode in (select ID from Country where ECountry=N'" + row["收件人国家"].ToString().Replace("'", "''") + "') )").UniqueResult();
                row["分区"] = obj;

                if (!list2.Contains(row["订单编号"].ToString()))
                {
                    LoggerUtil.GetOrderRecord(Convert.ToInt32(row["Id"]), row["订单编号"].ToString(), "订单打印", CurrentUser.Realname + "订单打印！", CurrentUser, NSession);
                    list2.Add(row["订单编号"].ToString());
                    base.NSession.CreateQuery("update OrderType set IsPrint=IsPrint+1 where  IsAudit=1 and  OrderNo IN('" + ids.Replace(",", "','") + "') ").ExecuteUpdate();
                }
                string country = dataSet.Tables[0].Rows[0]["收件人国家"].ToString();
                string postCode = dataSet.Tables[0].Rows[0]["收件人邮编"].ToString();
                string Fenjianma = GetFjm(country, postCode);
                row["分拣码"] = Fenjianma;

            }
            dataSet.Tables[0].DefaultView.Sort = " 订单编号 Asc";
            string xml = dataSet.GetXml();
            PrintDataType type = new PrintDataType
            {
                Content = xml,
                CreateOn = DateTime.Now
            };
            base.NSession.Save(type);
            base.NSession.Flush();
            return base.Json(new { IsSuccess = true, Result = type.Id });

        }
        public JsonResult SetPrintSingleData(string ids)
        {

            //string format = "select O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',P.Location as '库位',P.Category as 分类中文 from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU   left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo  ";

            // 1、12小时内打印仓库入库人名 2、12小时外打印库位
            // string format = "select   [dbo].[F_GetProducts]('{0}') as '产品名',[dbo].[F_GetSKU]('{0}') as '物品SKU1',[dbo].[F_GetSKU1]('{0}') as '物品SKU2',[dbo].[F_GetQty]('{0}') as '物品总Qty','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=20 then (case when Amount<=5 then Amount else 5 end) else 10 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By O.OrderNo ";
            string format = "select [dbo].[F_GetSKU2](O.OrderNo) as '物品SKU2',[dbo].[F_GetProducts](O.OrderNo) as '产品名','' as '分拣码',L.ParentID as '渠道ID',cast(O.Weight/1000.000 as decimal(10,2)) as Weight,O.Id,O.OrderNo as '订单编号',O.OrderExNo as '平台订单号',O.Amount as '订单金额',O.Platform as '订单平台',O.Account as '订单账户', O.BuyerName as '买家名称',O.BuyerEmail as '买家邮箱',O.Country  as '收件人国家',O.LogisticMode as '订单发货方式',O.BuyerMemo as '订单留言',O.SellerMemo as '卖家留言',O.TrackCode as '追踪码',O.TrackCode2 as '追踪码2', OP.ExSKU as '物品平台编号',OP.SKU as '物品SKU',OP.ImgUrl  as '物品图片网址',OP.Standard  as '物品规格',OP.Qty as '物品Qty', OP.Remark as '物品描述',OP.Title as '物品英文标题',P.ProductName as '物品中文标题', OA.Street as '收件人街道',OA.Addressee  as '收件人名称',OA.City as '收件人城市',OA.Phone  as '收件人手机',OA.Tel as '收件人电话', OA.PostCode as '收件人邮编',OA.Province as '收件人省',C.CountryCode as '国家代码',C.CCountry as '收件人国家中文',R.Street as '发件人地址',R.RetuanName as '发件人名称' ,R.PostCode as '发件人邮编',R.Tel as '发件人电话','' as '分区',(select top 1 PC.EName  from ProductCategory PC where PC.Name=P.Category) as '分类英文',(select top 1 case when DATEDIFF(HOUR,CreateOn,getdate())>12 then isnull(P.Location,'') else isnull(P.Location,'')+' '+CreateBy end from StockIn where SKU=P.SKU and InType='采购到货' order by CreateOn desc) as '库位',P.Category as 分类中文,case when Amount<=40 then (case when Amount<=5 and Amount>0 then Amount else 5 end) else 8 end as Value from Orders O left join OrderProducts OP on O.Id=OP.OId left join OrderAddress OA on O.AddressId=OA.Id left join Products P on OP.SKU=P.SKU  inner join LogisticsMode L on L.LogisticsName=O.LogisticMode  join Logistics K on K.Id=L.ParentID left join Country C on O.Country=C.ECountry left join ReturnAddress R on R.Id=1  where  O.IsOutOfStock=0 and O.OrderNo in('{0}') Order By OP.SKU  "; //O.OrderNo 订单按SKU升序进行排序 OP.SKU 【不允许按SKU排序原因：多条订单批量打印时原订单商品SKU打印排序被打乱，造成1个订单打印出多个面单情况】

            format = string.Format(format, ids.Replace(",", "','"));
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = format;
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            List<string> list2 = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                object obj = NSession.CreateSQLQuery("select top 1 AreaName from [LogisticsArea] where LId = (select top 1 ParentID from LogisticsMode where LogisticsCode='" + row["订单发货方式"] + "')  and Id =(select top 1 AreaCode from LogisticsAreaCountry where [LogisticsArea].Id=AreaCode  and CountryCode in (select ID from Country where ECountry=N'" + row["收件人国家"].ToString().Replace("'", "''") + "') )").UniqueResult();
                row["分区"] = obj;

                if (!list2.Contains(row["订单编号"].ToString()))
                {
                    LoggerUtil.GetOrderRecord(Convert.ToInt32(row["Id"]), row["订单编号"].ToString(), "订单打印", CurrentUser.Realname + "订单打印！", CurrentUser, NSession);
                    list2.Add(row["订单编号"].ToString());
                    base.NSession.CreateQuery("update OrderType set IsPrint=IsPrint+1 where  IsAudit=1 and  OrderNo IN('" + ids.Replace(",", "','") + "') ").ExecuteUpdate();
                }
                string country = dataSet.Tables[0].Rows[0]["收件人国家"].ToString();
                string postCode = dataSet.Tables[0].Rows[0]["收件人邮编"].ToString();
                string Fenjianma = GetFjm(country, postCode);
                row["分拣码"] = Fenjianma;

            }
            dataSet.Tables[0].DefaultView.Sort = " 订单编号 Asc";
            string xml = dataSet.GetXml();
            PrintDataType type = new PrintDataType
            {
                Content = xml,
                CreateOn = DateTime.Now
            };
            base.NSession.Save(type);
            base.NSession.Flush();
            return base.Json(new { IsSuccess = true, Result = type.Id });

        }
        public string GetFjm(string country, string PostCode)
        {
            string Fenjianma = string.Empty;
            int code = 0;

            if (country == "United States")
            {
                code = Convert.ToInt32(PostCode.Substring(0, 3));
                if ((code >= 000 && code <= 069) || (code >= 074 && code <= 078) || (code >= 080 && code <= 087) || (code >= 090 && code <= 099) || (code >= 105 && code <= 109) || (code >= 117 && code <= 299) || (code == 115))
                {
                    Fenjianma = "1F";
                }
                else if ((code == 103) || (code >= 110 && code <= 114) || (code == 116))
                {
                    Fenjianma = "1P";
                }
                else if ((code >= 070 && code <= 073) || code == 079 || code == 088 || code == 089)
                {
                    Fenjianma = "1Q";
                }
                else if ((code >= 100 && code <= 102) || code == 104)
                {
                    Fenjianma = "1R";
                }
                else if ((code >= 400 && code <= 433) || (code >= 437 && code <= 439) || (code >= 450 && code <= 459) || (code >= 470 && code <= 471) || (code >= 475 && code <= 477) || (code == 480) || (code >= 483 && code <= 485) || (code >= 490 && code <= 491) || (code >= 493 && code <= 497) || (code >= 500 && code <= 529) || (code == 533) || (code == 536) || (code == 540) || (code >= 546 && code <= 548) || (code >= 550 && code <= 609) || (code == 612) || (code >= 617 && code <= 619) || (code == 621) || (code == 624) || (code == 632) || (code == 635) || (code >= 640 && code <= 699) || (code >= 740 && code <= 758) || (code >= 760 && code <= 772) || (code >= 785 && code <= 787) || (code >= 789 && code <= 799))
                {
                    Fenjianma = "3F";
                }
                else if ((code >= 460 && code <= 469) || (code >= 472 && code <= 474) || (code >= 478 && code <= 479))
                {
                    Fenjianma = "3P";
                }
                else if ((code >= 498 && code <= 499) || (code >= 530 && code <= 532) || (code >= 534 && code <= 535) || (code >= 537 && code <= 539) || (code >= 541 && code <= 545) || (code == 549) || (code >= 610 && code <= 611))
                {
                    Fenjianma = "3Q";
                }
                else if ((code == 759) || (code >= 773 && code <= 778))
                {
                    Fenjianma = "3R";
                }
                else if ((code >= 613 && code <= 616) || (code == 620) || (code >= 622 && code <= 623) || (code >= 625 && code <= 631) || (code >= 633 && code <= 634) || (code >= 636 && code <= 639))
                {
                    Fenjianma = "3U";
                }
                else if ((code >= 434 && code <= 436) || (code >= 481 && code <= 482) || (code >= 486 && code <= 489) || (code == 492))
                {
                    Fenjianma = "3C";
                }
                else if ((code >= 779 && code <= 784) || (code == 788))
                {
                    Fenjianma = "3D";
                }
                else if (code >= 440 && code <= 449)
                {
                    Fenjianma = "3H";
                }
                else if ((code >= 813 && code <= 849) || (code == 854) || (code >= 856 && code <= 858) || (code >= 861 && code <= 862) || (code >= 864 && code <= 899) || (code == 906) || (code >= 909 && code <= 918) || (code >= 926 && code <= 939))
                {
                    Fenjianma = "4F";
                }
                else if ((code >= 900 && code <= 905) || (code >= 907 && code <= 908))
                {
                    Fenjianma = "4P";
                }
                else if ((code >= 850 && code <= 853) || (code == 855) || (code >= 859 && code <= 860) || (code == 863))
                {
                    Fenjianma = "4Q";
                }
                else if (code >= 919 && code <= 921)
                {
                    Fenjianma = "4R";
                }
                else if (code >= 922 && code <= 925)
                {
                    Fenjianma = "4U";
                }
                else if ((code == 942) || (code >= 950 && code <= 953) || (code >= 956 && code <= 979) || (code >= 986 && code <= 999))
                {
                    Fenjianma = "2F";
                }
                else if (code >= 980 && code <= 985)
                {
                    Fenjianma = "2P";
                }
                else if (code >= 800 && code <= 812)
                {
                    Fenjianma = "2Q";
                }
                else if (code >= 945 && code <= 948)
                {
                    Fenjianma = "2R";
                }
                else if ((code >= 940 && code <= 941) || (code >= 943 && code <= 944) || (code == 949) || (code >= 954 && code <= 955))
                {
                    Fenjianma = "2U";
                }
                else if ((code >= 300 && code <= 320) || (code >= 322 && code <= 326) || (code >= 334 && code <= 339) || (code >= 341 && code <= 346) || (code >= 348 && code <= 399) || (code >= 700 && code <= 739))
                {
                    Fenjianma = "5F";
                }
                else if ((code >= 330 && code <= 333) || (code == 340))
                {
                    Fenjianma = "5P";
                }
                else if ((code == 321) || (code >= 327 && code <= 329) || (code == 347))
                {
                    Fenjianma = "5Q";
                }




            }
            if (country == "Russian Federation")
            {
                code = Convert.ToInt32(PostCode.Substring(0, 3));
                int code2 = Convert.ToInt32(PostCode.Substring(0, 2));
                int code3 = Convert.ToInt32(PostCode.Substring(0, 1));
                if ((code >= 101 && code <= 157) || (code2 >= 21 && code2 <= 30))
                {
                    Fenjianma = "1";
                }
                else if (code2 >= 63 && code2 <= 69)
                {
                    Fenjianma = "2";
                }
                else if (code2 >= 16 && code2 <= 19)
                {
                    Fenjianma = "3";
                }
                else if ((code2 >= 60 && code2 <= 62) || (code3 >= 3 && code3 <= 4))
                {
                    Fenjianma = "4";
                }
            }
            if (country == "Canada")
            {
                string strcode = PostCode.Substring(0, 1).ToUpper();
                if (strcode == "A" || strcode == "B" || strcode == "C" || strcode == "D" || strcode == "E" || strcode == "F" || strcode == "G" || strcode == "H" || strcode == "I" || strcode == "J" || strcode == "K" || strcode == "L" || strcode == "M" || strcode == "N" || strcode == "O" || strcode == "P" || strcode == "Q" || strcode == "R")
                {
                    Fenjianma = "1";
                }
                else
                {
                    Fenjianma = "2";
                }
            }
            if (country == "Australia")
            {
                code = Convert.ToInt32(PostCode.Substring(0, 1));
                if (code == 1 || code == 2 || code == 4 || code == 9 || code==0)
                {
                    Fenjianma = "1";
                }
                else
                {
                    Fenjianma = "2";
                }
            }
            return Fenjianma;
        }

        private void UserLogin()
        {
            string clientIP = this.GetClientIP();
            string mACByIP = GetMACByIP(clientIP);
            UserLoginType type = new UserLoginType
            {
                UserCode = base.CurrentUser.Code,
                UserName = base.CurrentUser.Realname,
                IP = clientIP,
                MAC = mACByIP,
                CreateOn = DateTime.Now
            };
            base.NSession.Save(type);
            base.NSession.Flush();
        }

        //用户表
        public ActionResult UserList()
        {
            IList<UserType> list = base.NSession.CreateQuery(" from UserType where FromArea='义乌' ").List<UserType>();
            List<object> data = new List<object> {
                new { id = "ALL", text = "ALL" }
            };
            foreach (UserType type in list)
            {
                data.Add(new { id = type.Id, text = type.Realname });
            }
            return base.Json(data);
        }
    }
}
