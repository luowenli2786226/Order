using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.App.Controllers
{
    public class PurchasePlanExamineRecordController : BaseController
    {
        public ViewResult Index()
        {
            string sql = @"
delete from PurchasePlan where Qty=0
update PurchasePlan set Profit=
((select MIN(p2.Price+p2.Freight/p2.Qty) from PurchasePlan p2 where p2.SKU= PurchasePlan.SKU and CreateOn < PurchasePlan.CreateOn )-(Price+Freight/Qty))*Qty
update PurchasePlan set Profit=0 where Profit is null";
            NSession.CreateSQLQuery(sql).ExecuteUpdate();
            return View();

        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(PurchasePlanExamineRecordType obj, string c)
        {
            try
            {
                IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType Where Id in(" + c + ")")
              .List<PurchasePlanType>();
                object amount = NSession.CreateQuery("select SUM(Price*Qty+Freight) from PurchasePlanType Where Id in(" + c + ")").UniqueResult();
                object count = NSession.CreateQuery("select count(Id) from PurchasePlanType Where Id in(" + c + ")").UniqueResult();
                obj.ExamineAmount = Math.Round(Utilities.ToDouble(amount.ToString()), 2);
                obj.ExamineCount = Utilities.ToInt(count);
                obj.ExamineStatus = 0;
                objList.GroupBy(x => x.FromTo);
                obj.ExamineType = "日常";
                foreach (IGrouping<string, PurchasePlanType> purchasePlanTypes in objList.GroupBy(x => x.FromTo))
                {
                    if (purchasePlanTypes.Key != null && purchasePlanTypes.Key != "阿里巴巴" && purchasePlanTypes.Key != "淘宝" && purchasePlanTypes.Key != "售后")
                    {
                        obj.ExamineType = "其他";
                        break;
                    }
                }
                obj.Area = GetCurrentAccount().FromArea;
                obj.CreateBy = GetCurrentAccount().Realname;
                obj.ExamineOn = obj.CreateOn = DateTime.Now;
                //当采购购买金额<=2000时自动2级审批通过,审核状态为已审核已付款
                /*bool flag = true;
                foreach(PurchasePlanType pp in objList)
                {
                    if (pp.Price*pp.Qty+pp.Freight > 2000)
                    {
                        flag = false;
                        break;
                    }                                      
                }
                if (flag)
                {
                    obj.ExamineStatus = 3;
                }*/
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
                NSession.CreateQuery("Update PurchasePlanType set IsExamine=1,ExamineId=" + obj.Id + " Where Id in(" + c + ")").ExecuteUpdate();
            }
            catch (Exception ee)
            {
                return Json(new { ErrorMsg = "出错了", IsSuccess = false });
            }
            return Json(new { IsSuccess = true });
        }



        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoAudit(int k, int a, string m)
        {
            try
            {
                PurchasePlanExamineRecordType type;
                string errorMsg;
                type = base.Get<PurchasePlanExamineRecordType>(k);
                IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType Where ExamineId =" + k)
             .List<PurchasePlanType>();
                if (type.Area == "义乌")
                {

                    if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "雷刚"))
                    {

                        //if (type.ExamineType == "日常")
                        //{
                        //    a = 2;
                        //}
                        type.ExamineStatus = a;

                        type.ExamineContent += "雷刚:" + m;
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        //当2000>采购金额<=10000时1级审批一次性通过,仅需一审审核状态即改为已审核已付款                        
                        /*if (a == 2)
                        {
                            bool flag = true;
                            foreach (PurchasePlanType pp in objList)
                            {
                                if (pp.Price * pp.Qty + pp.Freight > 10000)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                type.ExamineStatus = 3;
                            }
                        }*/
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }
                }
                if (type.Area == "宁波")
                {
                    if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                    {

                        //if (type.ExamineType == "日常")
                        //{
                        //    a = 2;
                        //}
                        type.ExamineStatus = a;

                        type.ExamineContent += "邵纪银:" + m;
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        //当2000>采购金额<=10000时1级审批一次性通过,仅需一审审核状态即改为已审核已付款                        
                        /*if (a == 2)
                        {
                            bool flag = true;
                            foreach (PurchasePlanType pp in objList)
                            {
                                if (pp.Price * pp.Qty + pp.Freight > 10000)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                type.ExamineStatus = 3;
                            }
                        }*/
                        base.NSession.Update(type);
                        base.NSession.Flush();
                    }
                }

                //if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                //{
                //    type.ExamineStatus = a;
                //    type.ExamineContent += "邵纪银:" + m;
                //    type.ExamineBy += GetCurrentAccount().Realname;
                //    type.ExamineOn = DateTime.Now;
                //    base.NSession.Update(type);
                //    base.NSession.Flush();
                //}
                //else 
                //if (((a == 3) || (a == 9)) && (base.CurrentUser.Realname == "刘慧儿"))
                if (((a == 3) || (a == 9)) && (base.CurrentUser.Realname == "夏午君"))
                {

                    type.ExamineStatus = a;
                    type.ExamineContent += "胡亚儿:" + m;
                    type.ExamineBy += GetCurrentAccount().Realname;
                    type.ExamineOn = DateTime.Now;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                }
                else
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足！" });
                }
            }
            catch (Exception)
            {
                return base.Json(new { ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoTui(int k, decimal a, string m)
        {
            try
            {
                PurchasePlanExamineRecordType type;
                string errorMsg;
                type = base.Get<PurchasePlanExamineRecordType>(k);

                if (base.CurrentUser.Realname == "夏午君")
                {

                    //if (type.ExamineType == "日常")
                    //{
                    //    a = 2;
                    //}
                    type.TuiAmount = a;
                    type.Remark += m;
                    base.NSession.Update(type);
                    base.NSession.Flush();
                }


            }
            catch (Exception)
            {
                return base.Json(new { ErrorMsg = "出错了" });
            }
            return base.Json(new { IsSuccess = "true" });
        }


        public ActionResult PrintPur(int id)
        {
            string sql = @"select P.ExamineTitle,P.CreateBy,P.EndDate as '结束日期',P.BeginDate as '开始日期',PP.ProductName as '产品',pp.Suppliers as '供应商',pp.Qty as '数量',pp.Freight as '运费',pp.Price as '单价',(pp.Price*pp.Qty+pp.Freight) as '总价',pp.FromTo as '来源',0 as '淘宝',0 as '阿里巴巴' from PurchasePlanExamineRecord P 
left join PurchasePlan PP on p.Id=pp.ExamineId
where (Status<>'异常' and Status<>'失效') and p.Id=" + id + "Order By FromTo Asc";
            IDbCommand command = base.NSession.Connection.CreateCommand();
            command.CommandText = sql;
            SqlDataAdapter adapter = new SqlDataAdapter(command as SqlCommand);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);
            //object o1 = dataSet.Tables[0].Compute("SUM(总价)", "来源='淘宝'");
            //object o2 = dataSet.Tables[0].Compute("SUM(总价)", "来源='阿里巴巴'");
            //foreach (DataRow row in dataSet.Tables[0].Rows)
            //{
            //    row["淘宝"]=
            //}
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
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PurchasePlanExamineRecordType GetById(int Id)
        {
            PurchasePlanExamineRecordType obj = NSession.Get<PurchasePlanExamineRecordType>(Id);
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
            PurchasePlanExamineRecordType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(PurchasePlanExamineRecordType obj)
        {

            try
            {
                NSession.Update(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });

        }

        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(int id)
        {

            try
            {
                NSession.CreateQuery("Update PurchasePlanType set IsExamine=0,ExamineId=0 Where ExamineId =" + id).ExecuteUpdate();
                PurchasePlanExamineRecordType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }



        public JsonResult GetPlan(string sort, string order, string search)
        {
            string where = "";
            string orderby = " order by Id desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            if (!string.IsNullOrEmpty(search))
            {
                where = Utilities.Resolve(search);
                if (where.Length > 0)
                {
                    where = " where " + where;
                }
            }
            IList<PurchasePlanType> objList = NSession.CreateQuery("from PurchasePlanType " + where + orderby)
                .List<PurchasePlanType>();

            object count = NSession.CreateQuery("select count(Id) from PurchasePlanType " + where).UniqueResult();
            return Json(new { total = count, rows = objList });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
        {
            //   string where = "";
            string where = Utilities.SqlWhere(search);

            string orderby = " order by Id desc ";
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
            {
                orderby = " order by " + sort + " " + order;
            }

            /*   if (!string.IsNullOrEmpty(search))
               {
                   where = Utilities.Resolve(search);
                   if (where.Length > 0)
                   {
                       where = " where " + where;
                   }
             }*/
            IList<PurchasePlanExamineRecordType> objList = NSession.CreateQuery("from PurchasePlanExamineRecordType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<PurchasePlanExamineRecordType>();

            foreach (PurchasePlanExamineRecordType obj in objList)
            {
                obj.ExamineAmount = Convert.ToDouble(base.NSession.CreateSQLQuery("select sum(price*Qty+Freight) from PurchasePlan where (Status<>'异常' and Status<>'失效') and ExamineId=" + obj.Id).UniqueResult());

            }

            object count = NSession.CreateQuery("select count(Id) from PurchasePlanExamineRecordType " + where).UniqueResult();
            object sum = NSession.CreateQuery("select SUM(ExamineAmount) from PurchasePlanExamineRecordType " + where).UniqueResult();
            object sum2 = NSession.CreateQuery("select SUM(ExamineCount) from PurchasePlanExamineRecordType " + where).UniqueResult();
            object sum3 = NSession.CreateQuery("select SUM(TuiAmount) from PurchasePlanExamineRecordType " + where).UniqueResult();
            List<object> footers = new List<object>();

            footers.Add(new PurchasePlanExamineRecordType { Id = 0, ExamineAmount = Math.Round(Utilities.ToDouble(sum)), ExamineCount = Utilities.ToInt(sum2), TuiAmount = Math.Round(Utilities.ToDecimal(sum3)) });
            return Json(new { total = count, rows = objList, footer = footers });
        }

        public JsonResult ToExcel(string id)
        {
            try
            {
                string sql = "select OrderNo,Suppliers,sum(Price*Qty+Freight) as Amount from PurchasePlan where (Status<>'异常' and Status<>'失效') and ExamineId=" + id +
                             " group by OrderNo,Suppliers";

                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession, ""));

            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });
        }

        public JsonResult ToExcelTotal(string search)
        {
            try
            {
                string sql = "select ExamineTitle as '审批名称',ExamineType as '审批类型',Area as '区域',ExamineOn as '审批时间',ExamineBy as '审批人',ExamineContent as '审批评语',case  when ExamineStatus='1' then '审核中'  when ExamineStatus='2' then '已审核,等待付款' when ExamineStatus='3' then '已审核,已付款' when ExamineStatus='4' then '已付款' when ExamineStatus='9' then '审核失败' else '未审核' end  as '审批状态',ExamineAmount  as '审批总金额',TuiAmount as '退款金额',Remark  as '备注',ExamineCount as '审批总计划数',CreateOn as '创建时间',CreateBy as '创建人' from PurchasePlanExamineRecord";
                sql += Utilities.SqlWhere(search);
                //      string where = Utilities.SqlWhere(search);
                //      sql += where;
                Session["ExportDown"] = ExcelHelper.GetExcelXml(Utilities.GetDataSet(sql, NSession).Tables[0]);
            }
            catch (Exception ee)
            {
                return Json(new { IsSuccess = false, ErrorMsg = "出错了" });
            }
            return Json(new { IsSuccess = true, ErrorMsg = "导出成功" });

        }


    }
}

