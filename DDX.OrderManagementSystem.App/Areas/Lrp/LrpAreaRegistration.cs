using System.Web.Mvc;

namespace DDX.OrderManagementSystem.App.Areas.Lrp
{
    public class LrpAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Lrp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Lrp_default",
                "Lrp/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
