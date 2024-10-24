using ASCJSMCustom.Common.Services.DataProvider;
using ASCJSMCustom.Common.Services.DataProvider.Interfaces;
using ASCJSMCustom.Common.Services.REST;
using ASCJSMCustom.Common.Services.REST.Interfaces;
using Autofac;

namespace ASCJSMCustom.Common.Services
{
    public class ASCJSMServiceRegistrator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ASCJSMRESTService>().As<IASCJSMRESTService>();
            builder.RegisterType<ASCJSMMetalsAPILatestRateService>().As<IASCJSMMetalsAPILatestRateService>();

            builder.RegisterType<ASCJSMVendorDataProvider>().As<IASCJSMrVendorDataProvider>();
            builder.RegisterType<ASCJSMInventoryItemDataProvider>().As<IASCJSMInventoryItemDataProvider>();
        }
    }
}
