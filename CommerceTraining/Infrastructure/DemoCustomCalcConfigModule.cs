using System;
using System.Linq;
using CommerceTraining.Infrastructure.CartAndCheckout;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DemoCustomCalcConfigModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //context.Services.AddSingleton<ITaxCalculator, DemoCustomTaxCalc>();
            context.Services.Intercept<ITaxCalculator>(
                (locator, defaultCalculator) => new DemoCustomTaxCalc(defaultCalculator));
        }

        public void Initialize(InitializationEngine context)
        {
            //Add initialization logic, this method is called once after CMS has been initialized
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}