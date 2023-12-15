using ASCJewelryLibrary.Common.Services.DataProvider;
using ASCJewelryLibrary.Common.Services.DataProvider.Interfaces;
using ASCJewelryLibrary.Common.Services.REST;
using ASCJewelryLibrary.Common.Services.REST.Interfaces;
using Autofac;

namespace ASCJewelryLibrary.Common.Services
{
    public class ASCJServiceRegistrator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ASCJRESTService>().As<IASCJRESTService>();
            builder.RegisterType<ASCJMetalsAPILatestRateService>().As<IASCJMetalsAPILatestRateService>();

            builder.RegisterType<ASCJVendorDataProvider>().As<IASCJVendorDataProvider>();
            builder.RegisterType<ASCJInventoryItemDataProvider>().As<IASCJInventoryItemDataProvider>();
        }
    }
}
