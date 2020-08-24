using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.Shipping
{
    public class ShippingProcessor
    {
        private ILogger Logger;
        public StringDictionary Warnings { get; set; } // ...for the "Warnings-dict" (the old WF-stuff)

        public string  ProcessShipments(OrderGroup orderGroup)
        {
            Logger = LogManager.GetLogger(GetType());

            ShippingMethodDto methods = ShippingManager.GetShippingMethods
                (/*Thread.CurrentThread.CurrentUICulture.Name*/String.Empty);

            OrderGroup order = orderGroup;
            var billingCurrency = order.BillingCurrency;

            // request rates, make sure we request rates not bound to selected delivery method
            foreach (OrderForm form in order.OrderForms)
            {
                foreach (Shipment shipment in form.Shipments)
                {
                    bool processThisShipment = true;

                    // is this in use?
                    string discountName = "@ShipmentSkipRateCalc"; // ...this is the "old" promos
                    
                    // If you find the shipment discount which represents ... OLD?
                    if (shipment.Discounts.Cast<ShipmentDiscount>().Any(
                        x => x.ShipmentId == shipment.ShipmentId && x.DiscountName.Equals(discountName)))
                    {
                        processThisShipment = false;
                    }

                    if (!processThisShipment)
                    {
                        continue;
                    }

                    ShippingMethodDto.ShippingMethodRow row = 
                        methods.ShippingMethod.FindByShippingMethodId(shipment.ShippingMethodId);

                    // If shipping method is not found, set it to 0 and continue
                    if (row == null)
                    {
                        Logger.Information(String.Format("Total shipment is 0 so skip shipment calculations."));
                        shipment.ShippingSubTotal = 0;
                        continue;
                    }

                    // Check if package contains shippable items, if it does not use the default shipping method instead of the one specified
                    Logger.Debug(String.Format("Getting the type \"{0}\".", row.ShippingOptionRow.ClassName));
                    Type type = Type.GetType(row.ShippingOptionRow.ClassName); // ...the gateway 
                    if (type == null)
                    {
                        throw new TypeInitializationException(row.ShippingOptionRow.ClassName, null);
                    }

                    Logger.Debug(String.Format("Creating instance of \"{0}\".", type.Name));

                    // where it starts happening things
                    IShippingGateway provider = null;
                    var orderMarket = ServiceLocator.Current.GetInstance<IMarketService>()
                        .GetMarket(order.MarketId);

                    if (orderMarket != null)
                    {
                        provider = (IShippingGateway)Activator.CreateInstance(type, orderMarket);
                    }
                    else
                    {
                        provider = (IShippingGateway)Activator.CreateInstance(type);
                    }

                    Logger.Debug(String.Format("Calculating the rates."));
                    string message = String.Empty;
                    ShippingRate rate = provider.GetRate(orderMarket, row.ShippingMethodId, shipment, ref message);

                    if (rate != null)
                    {
                        Logger.Debug(String.Format("Rates calculated."));
                        // check if shipment currency is convertable to Billing currency, and then convert it
                        // Added the namespace below
                        if (!Mediachase.Commerce.Shared.CurrencyFormatter.CanBeConverted
                            (rate.Money, billingCurrency))

                        {
                            Logger.Debug(String.Format("Cannot convert selected shipping's currency({0}) to current currency({1}).", rate.Money.Currency.CurrencyCode, billingCurrency));
                            throw new Exception(String.Format("Cannot convert selected shipping's currency({0}) to current currency({1}).", rate.Money.Currency.CurrencyCode, billingCurrency));
                        }
                        else
                        {
                            Money convertedRate = Mediachase.Commerce.Shared.CurrencyFormatter.ConvertCurrency(rate.Money, billingCurrency);
                            shipment.ShippingSubTotal = convertedRate.Amount;
                        }
                        //return "In Rate";
                    }
                    else
                    {
                        Warnings[String.Concat("NoShipmentRateFound-", shipment.ShippingMethodName)] =
                            String.Concat("No rates have been found for ", shipment.ShippingMethodName);
                        Logger.Debug(String.Format("No rates have been found."));

                        //return "else";
                    }
                }

                //return "Near end";
            }

            return "Hello";
        } // end method



    }
}