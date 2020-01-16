﻿using Nyxie.Plugin.Promotions.Components;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class LineBuilder
    {
        private string categorySitecoreId;
        private EntityReference fullfilmentMethod;
        private string itemId = "001";
        private string lineId = "001";
        private decimal price = 33;
        private decimal quantity = 1;

        public LineBuilder IdentifiedBy(string lineId)
        {
            this.lineId = lineId;
            return this;
        }

        public LineBuilder WithProductId(string productId)
        {
            itemId = productId;
            return this;
        }

        public LineBuilder Quantity(decimal quantity)
        {
            this.quantity = quantity;
            return this;
        }

        public LineBuilder Price(decimal price)
        {
            this.price = price;
            return this;
        }

        public LineBuilder InCategory(string categorySitecoreId)
        {
            this.categorySitecoreId = categorySitecoreId;
            return this;
        }

        public LineBuilder WithStandardFulfillment()
        {
            fullfilmentMethod = new EntityReference("001", "Standard");
            return this;
        }

        public CartLineComponent Build()
        {
            var line = new CartLineComponent
            {
                Id = lineId,
                Quantity = quantity,
                ItemId = itemId,
                UnitListPrice = new Money(price),
                Policies =
                {
                    new PurchaseOptionMoneyPolicy
                    {
                        SellPrice = new Money(price)
                    }
                },
                Totals = new Totals
                {
                    GrandTotal = new Money(quantity * price)
                }
            };

            if (fullfilmentMethod != null)
                line.SetComponent(new FulfillmentComponent
                {
                    FulfillmentMethod = fullfilmentMethod
                });

            if (categorySitecoreId != null)
            {
                var categoryComponent = line.GetComponent<CategoryComponent>();
                categoryComponent.ParentCategoryList.Add(categorySitecoreId);
            }

            return line;
        }
    }
}
