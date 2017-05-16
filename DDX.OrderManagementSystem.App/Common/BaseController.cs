using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DDX.OrderManagementSystem.Domain;
using System.Web.Mvc;
using System.Text;
using System.EnterpriseServices;
using System.Configuration;
using DDX.NHibernateHelper;
using NHibernate;
//using DDX.OrderManagementSystem.Domain;
using System.Web.UI;
using NHibernate.Context;
using ITransaction = NHibernate.ITransaction;

namespace DDX.OrderManagementSystem.App.Controllers
{
    [SupportFilter]//此处如果去掉注释，则全部继承BaseController的Controller，都将执行SupportFilter过滤
    [ValidateInput(false)]
    public class BaseController : Controller
    {
        protected ISession NSession = NhbHelper.GetCurrentSession();
        protected bool permissionAdd = false;
        protected bool permissionEdit = false;
        protected bool permissionDelete = false;
        protected bool permissionExport = false;
        private UserType currentUser;

        public UserType CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    UserType account = GetCurrentAccount();
                    return account;
                }
                return currentUser;
            }
        }

        public virtual void GetPermission()
        {


        }

        public virtual string BuildToolBarButtons(int t)
        {
            return "";
        }

        /// <summary>
        /// 获取当前登陆人的账户信息
        /// </summary>
        /// <returns>账户信息</returns>
        public UserType GetCurrentAccount()
        {
            if (Session["account"] != null)
            {
                UserType account = (UserType)Session["account"];
                return account;
            }
            return null;

        }

        public bool IsAuthorized(string code)
        {
            return true;
        }



        //保存数据
        public virtual bool Save<T>(T entity)
        {
            try
            {
                NSession.Save(entity);
                if (!NSession.Transaction.IsActive)// 不是在事务里，就刷新到数据库
                    NSession.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("保存数据失败...", ex);

            }
            return true;
        }

        //更新数据
        public virtual bool Update<T>(T entity)
        {
            try
            {
                NSession.Update(entity);
                if (!NSession.Transaction.IsActive)// 不是在事务里，就刷新到数据库
                    NSession.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("更新数据失败...", ex);
            }
            return true;
        }

        //保存或更新数据
        public virtual bool SaveOrUpdate<T>(T entity)
        {
            try
            {
                NSession.SaveOrUpdate(entity);
                if (!NSession.Transaction.IsActive)// 不是在事务里，就刷新到数据库
                    NSession.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception("保存或更新数据失败...", ex);
            }
            return true;
        }

        //物理删除数据
        public virtual bool DeleteObj<T>(T entity)
        {
            try
            {
                NSession.Delete(entity);
                if (!NSession.Transaction.IsActive)// 不是在事务里，就刷新到数据库
                    NSession.Flush();
            }
            catch (System.Exception ex)
            {
                throw new Exception("删除数据失败...", ex);
            }
            return true;
        }

        //物理删除数据
        public virtual bool DeleteObj<T>(object id)
        {
            try
            {
                var entity = Get<T>(id);
                NSession.Delete(entity);
                if (!NSession.Transaction.IsActive)// 不是在事务里，就刷新到数据库
                    NSession.Flush();
            }
            catch (System.Exception ex)
            {
                throw new Exception("删除数据失败...", ex);
            }
            return true;
        }

        //获取数据（如果为空抛异常）
        public T Get<T>(object id)
        {
            try
            {
                T entity = NSession.Get<T>(id);
                if (entity == null)
                    throw new Exception("返回数据为空...");
                else
                    return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("获取数据失败...", ex);
            }

        }

        //获取数据 ，不会访问数据库（如果为空抛异常）
        public T Load<T>(string id)
        {
            try
            {
                T entity = NSession.Load<T>(id);
                if (entity == null)
                    throw new Exception("返回数据为空...");
                else
                    return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("获取数据失败...", ex);
            }
        }

        //判断字段的值是否存在 如果是插入id赋值-1或者new Guid,如果是修改id赋值 要修改项的值
        public bool IsFieldExist<T>(string fieldName, string fieldValue, string id)
        {
            return IsFieldExist<T>(fieldName, fieldValue, id, null);
        }

        //判断字段的值是否在一定范围内已存在 如果是插入id赋值-1或者new Guid,如果是修改id赋值 要修改项的值
        public bool IsFieldExist<T>(string fieldName, string fieldValue, string id, string where)
        {
            if (!string.IsNullOrEmpty(where))
                where = @" and " + where;
            var query = NSession.CreateQuery(
                string.Format(@"select count(*) from {0} as o where o.{1}='{2}' and o.Id<>'{3}'" + where,
                typeof(T).Name,
                fieldName,
                fieldValue, id));
            return query.UniqueResult<long>() > 0;
        }

        public int GetListCount<T>(string where)
        {
            if (!string.IsNullOrEmpty(where))
                where = @" where " + where;
            var query = NSession.CreateQuery(
               string.Format(@"select count(*) from {0} " + where,
               typeof(T).Name
               ));
            return query.UniqueResult<int>();
        }

        //简单查询
        public List<T> GetList<T>(string fieldName, string fieldValue, string where)
        {
            if (!string.IsNullOrEmpty(where))
                where = @" and " + where;
            var query = NSession.CreateQuery(
                string.Format(@"from {0} as o where o.{1}='{2}' " + where,typeof(T).Name,fieldName,fieldValue));
            return query.List<T>().ToList<T>();
        }

        //获取所有列表
        public virtual List<T> GetAll<T>()
        {
            return NSession.CreateQuery(string.Format(@" from {0}", typeof(T).Name))
                        .List<T>().ToList();
        }


        /// <summary>
        /// 根据Id获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public T GetById<T> (int Id)
        {
            T obj = NSession.Get<T>(Id);
            if (obj == null)
            {
                throw new Exception("返回实体为空");
            }
            else
            {
                return obj;
            }
        }


        private NHibernate.Criterion.ICriterion GetComparison(string comparison, string field, object value)
        {
            NHibernate.Criterion.ICriterion res;
            switch (comparison)
            {
                case "lt":
                    res = NHibernate.Criterion.Restrictions.Lt(field, value);
                    break;
                case "gt":
                    res = NHibernate.Criterion.Restrictions.Gt(field, value);
                    break;
                case "eq":
                    res = NHibernate.Criterion.Restrictions.Eq(field, value);
                    break;
                case "elt":
                    res = NHibernate.Criterion.Restrictions.Le(field, value);
                    break;
                case "egt":
                    res = NHibernate.Criterion.Restrictions.Ge(field, value);
                    break;
                default:
                    res = null;
                    break;
            }
            return res;
        }


    }
}
