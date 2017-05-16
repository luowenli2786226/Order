using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DDX.OrderManagementSystem.App;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.OrderManagementSystem.Domain;
using NHibernate;

namespace DDX.OrderManagementSystem.Controllers
{
    public class ServiceExaminationController : BaseController
    {
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }



        public ActionResult PrintPur(int id)
        {
            string sql = @"select *,Convert(nvarchar(10),CreateOn,120) as CreateOn2 from ServiceExamination where Id=" + id;
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


        [OutputCache(Location = OutputCacheLocation.None), HttpPost]
        public ActionResult DoAudit(int k, int a, string m, string h, string c)
        {
            try
            {
                ServiceExaminationType type;
                string errorMsg;
                type = base.Get<ServiceExaminationType>(k);


                if (type.Area == "义乌")
                {
                    if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "雷刚"))
                    {

                        type.ExamineStatus = a;

                        if (!string.IsNullOrEmpty(m))
                            type.ExamineMemo += "雷刚:" + m;
                        if (!string.IsNullOrEmpty(type.ExamineBy))
                            type.ExamineBy += "--->";
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        type.ExamineHandle = h;
                        base.NSession.Update(type);
                        base.NSession.Flush();
                        return base.Json(new { IsSuccess = "true" });
                    }
                }

                if (type.Area == "宁波")
                {
                    if (((a == 2) || (a == 9)) && (base.CurrentUser.Realname == "邵纪银"))
                    {

                        type.ExamineStatus = a;
                        if (!string.IsNullOrEmpty(m))
                            type.ExamineMemo += "邵纪银:" + m;
                        if (!string.IsNullOrEmpty(type.ExamineBy))
                            type.ExamineBy += "--->";
                        type.ExamineBy += GetCurrentAccount().Realname;
                        type.ExamineOn = DateTime.Now;
                        base.NSession.Update(type);
                        type.ExamineHandle = h;
                        base.NSession.Flush();
                        return base.Json(new { IsSuccess = "true" });

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
                //else if (a == 4 && CurrentUser.Realname == "刘慧儿")
                else if (a == 4 && CurrentUser.Realname == "夏午君")
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
                else
                {
                    return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足" });
                }

            }
            catch (Exception)
            {
                return base.Json(new { IsSuccess = false, ErrorMsg = "权限不足 " });
            }
            return base.Json(new { IsSuccess = "true" });
        }

        [HttpPost]
        public JsonResult Create(ServiceExaminationType obj)
        {
            try
            {
                obj.ExamineStatus = 0;
                obj.Area = GetCurrentAccount().FromArea;
                obj.CreateOn = obj.ExamineOn = DateTime.Now;
                obj.CreateBy = GetCurrentAccount().Realname;
                NSession.SaveOrUpdate(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }

        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ServiceExaminationType GetById(int Id)
        {
            ServiceExaminationType obj = NSession.Get<ServiceExaminationType>(Id);
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
            ServiceExaminationType obj = GetById(id);
            return View(obj);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(ServiceExaminationType obj)
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
                ServiceExaminationType obj = GetById(id);
                NSession.Delete(obj);
                NSession.Flush();
            }
            catch (Exception ee)
            {
                return Json(new { errorMsg = "出错了" });
            }
            return Json(new { IsSuccess = "true" });
        }


        public JsonResult GetListByOrderExNo(string o)
        {
            IList<ServiceExaminationType> objList = NSession.CreateQuery("from ServiceExaminationType where  OrderNo='" + o + "'")

               .List<ServiceExaminationType>();

            List<ServiceExaminationType> listfooter = new List<ServiceExaminationType>();

            listfooter.Add(new ServiceExaminationType { ExamineAmount = objList.Sum(x => x.ExamineAmount) });


            return Json(new { total = objList.Count, rows = objList, footer = listfooter });
        }

        public JsonResult List(int page, int rows, string sort, string order, string search)
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
                    where = " where ExamineStatus<> 9 and " + where;
                }
            }
            IList<ServiceExaminationType> objList = NSession.CreateQuery("from ServiceExaminationType " + where + orderby)
                .SetFirstResult(rows * (page - 1))
                .SetMaxResults(rows)
                .List<ServiceExaminationType>();

            object count = NSession.CreateQuery("select count(Id) from ServiceExaminationType " + where).UniqueResult();
            IList<object[]> objectses = NSession.CreateQuery("select ExamineCurrencyCode,sum(ExamineAmount) from ServiceExaminationType " + where + " Group by ExamineCurrencyCode ").List<object[]>();
            List<ServiceExaminationType> listfooter = new List<ServiceExaminationType>();
            foreach (object[] objectse in objectses)
            {
                if (objectse[1] == null)
                    objectse[1] = 0;
                listfooter.Add(new ServiceExaminationType { ExamineCurrencyCode = objectse[0].ToString(), ExamineAmount = Convert.ToDecimal(objectse[1]) });
            }



            return Json(new { total = count, rows = objList, footer = listfooter });
        }

        public JsonResult ToExcel(string search)
        {
            try
            {
                string sql = "select ExamineTitle as '标题',case  when ExamineStatus='1' then '审核中'  when ExamineStatus='2' then '财审中' when ExamineStatus='3' then '已审核,未付款' when ExamineStatus='4' then '已审核,已付款' when ExamineStatus='9' then '审核失败' else '未审核' end +char(10)+isnull(ExamineType,'null')  as '状态',OrderNo  as '订单号',Account as '店铺',ExamineCurrencyCode  as '货币',ExamineAmount as '金额',Remark as '备注',Content as '内容',ExamineMemo as '审批评语',ExamineBy as'审批人',PayOn as '付款时间',ExamineClass as '原因',ExamineOn as '审批时间',CreateBy as '创建人' ,CreateOn as '创建时间' from ServiceExamination ";
          //      string sql = "select ExamineTitle as '标题',case  when ExamineStatus='1' then '审核中'+char(10)+ExamineType  when ExamineStatus='2' then '财审中'+char(10)+ExamineType when ExamineStatus='3' then '已审核,未付款'+char(10)+ExamineType when ExamineStatus='4' then '已审核,已付款'+char(10)+ExamineType when ExamineStatus='9' then '审核失败'+char(10)+ExamineType else '未审核'+char(10)+ExamineType end  as '状态',OrderNo  as '订单号',Account as '店铺',ExamineCurrencyCode  as '货币',ExamineAmount as '金额',Remark as '备注',Content as '内容',ExamineMemo as '审批评语',ExamineBy as'审批人',PayOn as '付款时间',ExamineClass as '原因',ExamineOn as '审批时间',CreateBy as '创建人' ,CreateOn as '创建时间' from ServiceExamination";
                sql += Utilities.SqlWhere(search);
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

