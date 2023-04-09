using ASCISTARCustom.Common.Services.DataProvider;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.Common.Services.REST;
using ASCISTARCustom.Common.Services.REST.Interfaces;
using Autofac;

namespace ASCISTARCustom.Common.Services
{
    public class ASCIStarServiceRegistrator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ASCIStarRESTService>().As<IASCIStarRESTService>();
            builder.RegisterType<ASCIStarMetalsAPILatestRateService>().As<IASCIStarMetalsAPILatestRateService>();

            builder.RegisterType<ASCIStarVendorDataProvider>().As<IASCIStarVendorDataProvider>();
        }
    }
}
