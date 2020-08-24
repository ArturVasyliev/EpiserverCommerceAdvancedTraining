using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.BaseClasses;
using Mediachase.Web.Console.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GiftCardPaymentProvider
{
    public partial class ConfigurePayment : OrderBaseUserControl, IGatewayControl
    {
        private PaymentMethodDto paymentMethodDto = null;
        private const string metaClassName = "GiftCardMetaClassName";
        public string ValidationGroup { get; set; } = string.Empty;

        public void LoadObject(object dto)
        {
            paymentMethodDto = dto as PaymentMethodDto;
        }

        public void SaveChanges(object dto)
        {
            if (Visible)
            {
                paymentMethodDto = dto as PaymentMethodDto;
                if (paymentMethodDto != null && paymentMethodDto.PaymentMethodParameter != null)
                {
                    var paymentMethodId = Guid.Empty;
                    if (paymentMethodDto.PaymentMethod.Count > 0)
                    {
                        paymentMethodId = paymentMethodDto.PaymentMethod[0].PaymentMethodId;
                    }
                    var param = GetParameterByName(metaClassName);
                    if (param != null)
                    {
                        param.Value = txtMetaclass.Text;
                    }
                    else
                    {
                        var newRow = paymentMethodDto.PaymentMethodParameter.NewPaymentMethodParameterRow();
                        newRow.PaymentMethodId = paymentMethodId;
                        newRow.Parameter = metaClassName;
                        newRow.Value = txtMetaclass.Text;
                        paymentMethodDto.PaymentMethodParameter.Rows.Add(newRow);
                    }
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (paymentMethodDto != null && paymentMethodDto.PaymentMethodParameter != null)
            {
                var param = GetParameterByName(metaClassName);
                if (param != null)
                {
                    txtMetaclass.Text = param.Value;
                }
            }
        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            var rows = paymentMethodDto.PaymentMethodParameter.Select($"Parameter='{name}'");
            if (rows != null && rows.Length > 0)
            {
                return rows[0] as PaymentMethodDto.PaymentMethodParameterRow;
            }
            return null;
        }
    }
}