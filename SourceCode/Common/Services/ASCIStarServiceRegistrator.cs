using ASCISTARCustom.Common.Services.Interfaces;
using Autofac;

namespace ASCISTARCustom.Common.Services
{
    public class ASCIStarServiceRegistrator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ASCIStarRESTService>().As<IASCIStarRESTService>();
            builder.RegisterType<ASCIStarMetalsAPILatestRates>().As<IASCIStarMetalsAPILatestRates>();
        }
    }
}
