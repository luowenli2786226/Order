using DDX.OrderManagementSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using NHibernate;
using DDX.OrderManagementSystem.App.Controllers;
using DDX.NHibernateHelper;
using System.Web.Mvc;

namespace DDX.OrderManagementSystem.App.Service
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“pda”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 pda.svc 或 pda.svc.cs，然后开始调试。
    public class pda : BaseController
    {
        //protected ISession NSession = NhbHelper.GetCurrentSession();
        public void DoWork()
        {
        }

        /// <summary>
        /// PDA用户登录
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public JsonResult Login(string Uid, string Pwd)
        {
            List<UserType> users = NSession.CreateQuery("from UserType where Username=:p1 and Password=:p2").SetString("p1", Uid.Trim()).SetString("p2", Pwd.Trim()).List<UserType>().ToList();

            if (users.Count > 0)
            {
                // 成功
                return base.Json(new { IsSuccess = true, Result = users });
            }
            else
            {
                // 失败
                return base.Json(new { IsSuccess = false });
            }
        }
    }
}
