﻿using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartAmountOffFulfillmentActionBuilder : IBenefitBuilder
    {
        private decimal amountOff = 10;

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Hc_CartAmountOffFulfillmentAction",
                LibraryId = "Hc_CartAmountOffFulfillmentAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Hc_SpecificAmount",
                        Value = amountOff.ToString()
                    }
                }
            };
        }

        public CartAmountOffFulfillmentActionBuilder AmountOff(decimal value)
        {
            amountOff = value;
            return this;
        }
    }
}
