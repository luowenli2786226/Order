using DDX.OrderManagementSystem.App.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DDX.OrderManagementSystem.App
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Configuration.AppSettingsReader appReader = new System.Configuration.AppSettingsReader();//配置文件中取
            var isji = appReader.GetValue("IsJi", typeof(bool));
            Config.IsJi = Convert.ToBoolean(isji);
            AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Utilities.UpdateCurrency();
            //Utilities.GetSMTSSS();
            //Utilities.GetSMTShouHui();
            //var nI = 0;
            //Utilities.GetSMTOrderByAPI();// 开机启动 设置单元
            //Utilities.SyncFixedRate();
            //CdiscountUtil.SecuredService.TestOrderList();
            
            Utilities.test();

            System.Timers.Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            myTimer.Interval = 60000;

            myTimer.Enabled = true;
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            // 每月1号同步固定汇率
            if (DateTime.Now.Day == 1 && DateTime.Now.Hour == 1 && DateTime.Now.Minute == 1)
            {
                Utilities.SyncFixedRate();
                Utilities.AutoGoodsDiscount();
            }

            if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 1)
            {
                //Utilities.GetSMTOrderByAPI();
                Utilities.GetSMTSSS();
                Utilities.UpdateCurrency();
            }
            if (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 40)
            {
                //Utilities.GetSMTOrderByAPI();
                Utilities.GetSMTSSS();
                Utilities.UpdateCurrency();
            }
        }
    }
}