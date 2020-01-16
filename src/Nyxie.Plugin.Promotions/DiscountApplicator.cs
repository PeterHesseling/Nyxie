﻿using System;
using System.Collections.Generic;

using Nyxie.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;

namespace Nyxie.Plugin.Promotions
{
    public class DiscountApplicator
    {
        private readonly CommerceContext commerceContext;

        public DiscountApplicator(CommerceContext commerceContext)
        {
            this.commerceContext = commerceContext;
        }

        public void ApplyPercentageDiscount(IEnumerable<CartLineComponent> cartLines, decimal percentage, DiscountOptions options)
        {
            ApplyDiscount(cartLines, line => new MoneyEx(commerceContext, line.UnitListPrice)
                                             .CalculatePercentageDiscount(percentage).Round().Value, options);
        }

        public void ApplyPriceDiscount(IEnumerable<CartLineComponent> cartLines, decimal price, DiscountOptions options)
        {
            ApplyDiscount(cartLines, line => new MoneyEx(commerceContext, line.UnitListPrice)
                                             .CalculatePriceDiscount(price).Round().Value, options);
        }

        private void ApplyDiscount(IEnumerable<CartLineComponent> cartLines, Func<CartLineComponent, Money> calculateDiscount,
            DiscountOptions options)
        {
            IEnumerable<CartLineComponent> cartLinesToApply = options.ApplicationOrder.Order(cartLines);

            var counter = 0;
            foreach (CartLineComponent line in cartLinesToApply)
            {
                Money discount = calculateDiscount(line);

                for (var i = 0; i < line.Quantity; i++)
                {
                    // Stop applying the discount when the number of items has exceeded the limit.
                    if (counter == options.ActionLimit)
                        return;

                    line.Adjustments.Add(AwardedAdjustmentFactory.CreateLineLevelAwardedAdjustment(discount.Amount * -1,
                        options.AwardingBlock, line.ItemId, commerceContext));

                    line.GetComponent<MessagesComponent>().AddPromotionApplied(commerceContext, options.AwardingBlock);

                    counter++;
                }
            }
        }
    }
}
