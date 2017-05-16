using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using DDX.NHibernateHelper;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class DisputeController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View();
            }
            OrderType order = NSession.Get<OrderType>(Utilities.ToInt(id));
            DisputeRecordType dispute = new DisputeRecordType();
            dispute.OrderNo = order.OrderExNo;
            dispute.OrderAmount2 = Utilities.ToDecimal(order.Amount);
            dispute.Account = order.Account;
            dispute.Platform = order.Platform;

            //List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            //CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == dispute.ExamineCurrencyCode);
            //dispute.ExamineAmountRmb = dispute.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);

            List<OrderProductType> list =
                NSession.CreateQuery("from OrderProductType where OId=" + order.Id).List<OrderProductType>().ToList();
            foreach (var orderProductType in list)
            {
                dispute.SKU += orderProductType.SKU + " ";
            }
            dispute.Remark = "发货时间：" + order.ScanningOn.ToString("yyyy-MM-dd HH:mm:ss") + "  " + order.TrackCode;
            return View(dispute);
        }

        public ActionResult Create1(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View();
            }
            OrderType order = NSession.Get<OrderType>(Utilities.ToInt(id));
            DisputeRecordType dispute = new DisputeRecordType();
            dispute.OrderNo = order.OrderExNo;
            dispute.OrderAmount2 = Utilities.ToDecimal(order.Amount);
            dispute.Account = order.Account;
            dispute.Platform = order.Platform;

            //List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
            //CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == dispute.ExamineCurrencyCode);
            //dispute.ExamineAmountRmb = dispute.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);

            List<OrderProductType> list =
                NSession.CreateQuery("from OrderProductType where OId=" + order.Id).List<OrderProductType>().ToList();
            foreach (var orderProductType in list)
            {
                dispute.SKU += orderProductType.SKU + " ";
            }
            dispute.Remark = "发货时间：" + order.ScanningOn.ToString("yyyy-MM-dd HH:mm:ss") + "  " + order.TrackCode;
            return View(dispute);
        }



        public ActionResult PrintPur(int id)
        {
            string sql = @"select *,Convert(nvarchar(10),CreateOn,120) as CreateOn2 from DisputeRecordType where Id=" + id;
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = sql;
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

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

        /// <summary>
        /// 变更审批状态
        /// </summary>
        /// <param name="k">纠纷编号</param>
        /// <param name="s">纠纷状态</param>
        /// <param name="a">实赔金额</param>
        /// <returns></returns>
        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoDisputeState(int k, string s, decimal a)
        {
            try
            {

                DisputeRecordType type;
                string errorMsg;
                type = base.Get<DisputeRecordType>(k);
                if (a != 0)
                {
                    type.ExamineAmount = a;
                    type.Rate = Math.Round(type.ExamineAmount / type.OrderAmount * 100, 2);
                }
                // 允许在任意状态下操作纠纷申请时折算人民币
                //// 非“纠纷中”状态下折换人民币
                //if (s != "纠纷中")
                //{
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == type.ExamineCurrencyCode);
                type.ExamineAmountRmb = type.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);

                //type.ExamineOn = DateTime.Now;
                //}

                type.DisputeState = s;
                NSession.Update(type);
                NSession.Flush();

            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足 " });
            }
            return base.Json(new { IsSuccess = "true" });
        }



        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoAudit(string k, int a, string m, string zr, decimal amount)
        {
            List<DisputeRecordType> list =
                NSession.CreateQuery("from DisputeRecordType where Id in(" + k + ")").List<DisputeRecordType>().ToList();
            int ss = a;
            try
            {
                foreach (DisputeRecordType type in list)
                {
                    if (ss == 0)
                    {
                        if (type.ExamineStatus == 1)
                        {
                            a = 2;
                        }
                        else if (type.ExamineStatus == 5)
                        {
                            a = 6;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == type.ExamineCurrencyCode);
                    // CNY人民币时指定为1
                    if (type.ExamineCurrencyCode == "CNY")
                    {
                        type.ExamineAmountRmb = type.ExamineAmount * Convert.ToDecimal(1);
                    }
                    else
                    {
                        type.ExamineAmountRmb = type.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                    }

                    if (type.Area == "义乌")
                    {
                        if (((a == 2) || (a == 6) || (a == 9)) && (base.CurrentUser.Realname == "雷刚"))
                        {
                            type.ExamineStatus = a;

                            if (!string.IsNullOrEmpty(m))
                                type.ExamineMemo += "雷刚:" + m;
                            if (!string.IsNullOrEmpty(type.ExamineBy))
                                type.ExamineBy += "--->";
                            type.ExamineBy += GetCurrentAccount().Realname;
                            type.ExamineOn = DateTime.Now;
                            type.ZeRenBy = zr;
                            type.ExamineBy += GetCurrentAccount().Realname;
                            //type.ExamineOn = DateTime.Now;
                            type.ExamineHandle = Math.Round(amount / list.Count, 2).ToString();
                            base.NSession.Update(type);
                            base.NSession.Flush();
                        }
                    }

                    if (type.Area == "宁波")
                    {
                        if (((a == 2) || (a == 6) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                        {

                            type.ExamineStatus = a;
                            if (!string.IsNullOrEmpty(m))
                                type.ExamineMemo += "邵纪银:" + m;
                            if (!string.IsNullOrEmpty(type.ExamineBy))
                                type.ExamineBy += "--->";
                            type.ExamineBy += GetCurrentAccount().Realname;
                            type.ExamineOn = DateTime.Now;
                            type.ZeRenBy = zr;

                            type.ExamineHandle = Math.Round(amount / list.Count, 2).ToString();
                            base.NSession.Update(type);
                            base.NSession.Flush();
                        }
                    }

                    if (((a == 3) || (a == 9)) && (base.CurrentUser.Realname == "胡亚儿"))
                    {

                        type.ExamineStatus = a;
                        type.ExamineMemo += "--->" + GetCurrentAccount().Realname + " 确认财务审核！";
                        if (!string.IsNullOrEmpty(type.ExamineBy))
                            type.ExamineBy += "--->";
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }

                    //if (a == 4 && CurrentUser.Realname == "刘慧儿")
                    if (a == 4 && CurrentUser.Realname == "夏午君")
                    {
                        type.ExamineStatus = a;
                        type.ExamineMemo += "--->" + GetCurrentAccount().Realname + " 确认付款！";
                        if (!string.IsNullOrEmpty(type.ExamineBy))
                            type.ExamineBy += "--->";
                        type.PayOn = DateTime.Now;
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }
                }
                return base.Json(new { IsSuccess = "true" });

            }
            catch (Exception exc)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足 " });
            }

            return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足" });

        }

        [HttpPost]
        public JsonResult Create(DisputeRecordType obj)
        {
            try
            {
                //根据orderno查找ExamineStatus，如果是①已处理，平台付款②未处理，平台付款 不能添加纠纷
                List<DisputeRecordType> listdispute = NSession.CreateQuery("from DisputeRecordType where OrderNo='" + obj.OrderNo + "'").List<DisputeRecordType>().ToList();
                if (listdispute.Count > 0)
                {
                    obj = listdispute[0];
                    if (obj.IsImport == 1 && obj.ExamineStatus == 5 || obj.ExamineStatus == 6)
                    {
                        return Json(new { Msg = "当前为导入订单且纠纷状态已经是平台付款，不能添加纠纷" });
                    }
                }
                obj.ExamineStatus = 1;
                if (obj.DisputeState != null && obj.DisputeState.IndexOf("财务") == -1)
                {
                    obj.ExamineStatus = 5;
                }
                // 非“纠纷中”状态下折换人民币
                //if (obj.DisputeState != "纠纷中")
                //{
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == obj.ExamineCurrencyCode);
                obj.ExamineAmountRmb = obj.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                //}

                List<OrderType> order = NSession.CreateQuery("from OrderType where OrderExNo='" + obj.OrderNo + "'").List<OrderType>().ToList();
                if (order.Count > 0)
                {
                    LoggerUtil.GetOrderRecord(order[0], "订单添加纠纷", "纠纷类别：" + obj.ExamineClass + " 备注：" + obj.Remark + " SKU：" + obj.SKU, CurrentUser, NSession);
                }
                obj.Rate = Math.Round(obj.ExamineAmount / obj.OrderAmount * 100, 2);
                obj.Area = GetCurrentAccount().FromArea;
                obj.CreateOn = obj.ExamineOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { Msg = "出错了,请填写正确" });
            }
            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        public JsonResult Create1(DisputeRecordType obj)
        {
            try
            {
                //根据orderno查找ExamineStatus，如果是①已处理，平台付款②未处理，平台付款 不能添加纠纷
                List<DisputeRecordType> listdispute = NSession.CreateQuery("from DisputeRecordType where OrderNo='" + obj.OrderNo + "'").List<DisputeRecordType>().ToList();
                if (listdispute.Count > 0)
                {
                    obj = listdispute[0];
                    if (obj.IsImport == 1 && obj.ExamineStatus == 5 || obj.ExamineStatus == 6)
                    {
                        return Json(new { Msg = "当前为导入订单且纠纷状态已经是平台付款，不能添加纠纷" });
                    }
                }
                obj.ExamineStatus = 1;
                if (obj.DisputeState != null && obj.DisputeState.IndexOf("财务") == -1)
                {
                    obj.ExamineStatus = 5;
                }
                // 非“纠纷中”状态下折换人民币
                //if (obj.DisputeState != "纠纷中")
                //{
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == obj.ExamineCurrencyCode);
                obj.ExamineAmountRmb = obj.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                //}

                List<OrderType> order = NSession.CreateQuery("from OrderType where OrderExNo='" + obj.OrderNo + "'").List<OrderType>().ToList();
                if (order.Count > 0)
                {
                    LoggerUtil.GetOrderRecord(order[0], "订单添加纠纷", "纠纷类别：" + obj.ExamineClass + " 备注：" + obj.Remark + " SKU：" + obj.SKU, CurrentUser, NSession);
                }
                obj.Rate = Math.Round(obj.ExamineAmount / obj.OrderAmount * 100, 2);
                obj.Area = GetCurrentAccount().FromArea;
                obj.CreateOn = obj.ExamineOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { Msg = "出错了,请填写正确" }, "text/html", JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsSuccess = true }, "text/html", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public DisputeRecordType GetById(int Id)
        {
            DisputeRecordType obj = NSession.Get<DisputeRecordType>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int id)
        {
            DisputeRecordType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(DisputeRecordType obj)
        {

            try
            {
                List<CurrencyType> currencies = NSession.CreateQuery("from CurrencyType").List<CurrencyType>().ToList();
                //当赔款币种为人民币时，另外计算比例
                if (obj.ExamineCurrencyCode == "CNY")
                {
                    List<OrderType> order = NSession.CreateQuery("from OrderType where OrderExNo='" + obj.OrderNo + "'").List<OrderType>().ToList();
                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == order[0].CurrencyCode);
                    decimal examinate = obj.ExamineAmount / Convert.ToDecimal(currencyType.CurrencyValue);//计算以订单金额为币种的实赔金额
                    if (obj.ExamineAmount == 0)
                    {
                        obj.Rate = 0;
                    }
                    else
                    {
                        obj.Rate = Math.Round(examinate / obj.OrderAmount * 100, 2);
                    }
                    obj.ExamineAmountRmb = obj.ExamineAmount;
                }
                else
                {
                    obj.Rate = Math.Round(obj.ExamineAmount / obj.OrderAmount * 100, 2);



                    CurrencyType currencyType = currencies.Find(x => x.CurrencyCode == obj.ExamineCurrencyCode);
                    obj.ExamineAmountRmb = obj.ExamineAmount * Convert.ToDecimal(currencyType.CurrencyValue);
                }
                //如果是系统导入或者API对接的数据就不能修改纠纷状态
                //List<OrderType> order = NSession.CreateQuery("from OrderType where OrderExNo='" + obj.OrderNo + "'").List<OrderType>().ToList();
                //if (order.Count > 0)
                //{
                //    LoggerUtil.GetOrderRecord(order[0], "订单修改纠纷", "纠纷类别：" + obj.ExamineClass + " 备注：" + obj.Remark + " SKU：" + obj.SKU, CurrentUser, NSession);
                //}
                //obj.CreateOn = obj.ExamineOn = DateTime.Now;
                obj.ExamineOn = DateTime.Now;
                obj.Area = GetCurrentAccount().FromArea;
                //obj.CreateBy = GetCurrentAccount().Realname;

                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" }, "text/html", JsonRequestBehavior.AllowGet);

        }
        public ActionResult EditReason(int id)
        {
            DisputeRecordType obj = GetById(id);
            return View(obj);
        }


        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult EditReason(DisputeRecordType obj)
        {
            try
            {
                if (obj.ExamineStatus == 6)
                {
                    obj.ExamineStatus = 5;
                }
                obj.ExamineOn = DateTime.Now;
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" }, "text/html", JsonRequestBehavior.AllowGet);

        }
        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                DisputeRecordType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" }, "text/html", JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetListByOrderExNo(string o)
        {
            IList<DisputeRecordType> objList = NSession.CreateQuery("from DisputeRecordType where  OrderNo='" + o + "'")

               .List<DisputeRecordType>();

            List<DisputeRecordType> listfooter = new List<DisputeRecordType>();

            listfooter.Add(new DisputeRecordType { ExamineAmount = objList.Sum(x => x.ExamineAmount) });


            return Json(new { total = objList.Count, rows = objList, footer = listfooter });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            // search= 
            string where = "";
            //string orderby = " order by Id desc ";
            string orderby = " order by ExamineOn desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where ExamineStatus<> 9 and " + where;
                }
            }
            IList<DisputeRecordType> objList = NSession.CreateQuery("from DisputeRecordType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<DisputeRecordType>();

            object count = NSession.CreateQuery("select count(Id) from DisputeRecordType " + where).UniqueResult();
            object bbb = NSession.CreateSQLQuery("select SUM(cast(ExamineHandle as float)) from DisputeRecordType " + where).UniqueResult();
            IList<object[]> objectses = NSession.CreateQuery("select ExamineCurrencyCode,sum(ExamineAmount),sum(OrderAmount),sum(OrderAmount2),sum(ExamineAmountRmb) from DisputeRecordType " + where + " Group by ExamineCurrencyCode ").List<object[]>();
            //IList<object[]> objectses = NSession.CreateSQLQuery("select CASE WHEN (GROUPING(ExamineCurrencyCode) = 1) THEN 'ALL' ELSE ISNULL(ExamineCurrencyCode, 'UNKNOWN')END from DisputeRecordType " + where + " Group by ExamineCurrencyCode WITH CUBE").List<object[]>();

            List<DisputeRecordType> listfooter = new List<DisputeRecordType>();
            decimal fff = 0;
            decimal ggg = 0;
            decimal totle = 0;
            foreach (object[] objectse in objectses)
            {
                if (objectse[1] == null)
                    objectse[1] = 0;
                fff += Convert.ToDecimal(objectse[1]);
                ggg += Convert.ToDecimal(objectse[2]);
                totle += Convert.ToDecimal(objectse[4]);
                listfooter.Add(new DisputeRecordType { ExamineCurrencyCode = Convert.ToString(objectse[0]), ExamineAmount = Convert.ToDecimal(objectse[1]), OrderAmount = Convert.ToDecimal(objectse[2]), OrderAmount2 = Convert.ToDecimal(objectse[3]), ExamineAmountRmb = Convert.ToDecimal(objectse[4]), ExamineHandle = Utilities.ToString(bbb), Rate = (fff == 0 || ggg == 0) ? 0 : Math.Round(fff / ggg, 4) * 100 });
            }
            listfooter.Add(new DisputeRecordType { ExamineCurrencyCode = "ALL", ExamineAmount = Convert.ToDecimal(0), ExamineAmountRmb = Convert.ToDecimal(totle), OrderAmount = Convert.ToDecimal(0), OrderAmount2 = Convert.ToDecimal(0), ExamineHandle = Utilities.ToString(0), Rate = 0 });

            //listfooter[0].Rate = Math.Round(fff / ggg, 4) * 100;
            return Json(new { total = count, rows = objList, footer = listfooter });
        }
        public JsonResult ToExcel(string search)
        {
            string where = "";
            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where ExamineStatus<> 9 and " + where;
                }
            }
            string sql = @"select case ExamineStatus
WHEN '1' THEN '未处理'
WHEN '2' THEN '已处理'
WHEN '3' THEN '已审核,未付款'
WHEN '4' THEN '已审核,已付款'
WHEN '5' THEN '未处理,平台付款'
WHEN '6' THEN '已处理,平台付款'
WHEN '9' THEN '审核失败'
else '未审核' end as '状态'
, Area as '区域',CreateOn as '创建时间',CreateBy as '创建人',Account as '账户',OrderNo as '订单号',OrderAmount2 as '订单金额',OrderAmount  as '预计金额',ExamineAmount as '实际金额',ExamineAmountRmb as'实际金额RMB',ExamineCurrencyCode as '货币',ExamineClass  as '纠纷类型',ExamineHandle as '纠纷处理',ExamineMemo as '纠纷备注',Remark  as '备注',Paypal as 'Paypal账号',SKU as 'SKU',DisputeState as '纠纷状态',ZeRenBy as '责任人',ExamineOn as '处理日期' from DisputeRecordType" + where;

            DataSet ds = Utilities.GetDataSet(sql, NSession);

            base.Session["ExportDown"] = ExcelHelper.GetExcelXml(ds);
            return base.Json(new { IsSuccess = true });
        }

        /// <summary>
        /// 纠纷表记录图片地址
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="pic"></param>
        /// <returns></returns>
        public JsonResult EditPic(int pid, string pic)
        {
            DisputeRecordType disputetype = Get<DisputeRecordType>(pid);
            disputetype.ImgPic = pic;
            NSession.Update(disputetype);
            NSession.Flush();
            return Json(new { IsSuccess = true });
        }



    }
}

