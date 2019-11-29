﻿using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Factory;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Order history contains [compares] [specific value] products in the [specific] category"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(OrderHistoryAmountInCategoryCondition))]
    public class OrderHistoryAmountInCategoryCondition : ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly GetCategoryCommand _getCategoryCommand;

        public OrderHistoryAmountInCategoryCondition(FindEntitiesInListCommand findEntitiesInListCommand, GetCategoryCommand getCategoryCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _getCategoryCommand = getCategoryCommand;
        }

        public IBinaryOperator<decimal, decimal> Pm_Compares { get; set; }

        public IRuleValue<decimal> Pm_SpecificValue { get; set; }

        public IRuleValue<string> Pm_SpecificCategory { get; set; }

        public IRuleValue<bool> Pm_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificCategory = Pm_SpecificCategory.Yield(context);
            var specificValue = Pm_SpecificValue.Yield(context);
            var includeSubCategories = Pm_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Pm_Compares == null)
            {
                return false;
            }

            //Get Data
            var commerceContext = context.Fact<CommerceContext>();
            var categoryFactory = new CategoryFactory(commerceContext, null, _getCategoryCommand);
            var categorySitecoreId = AsyncHelper.RunSync(() => categoryFactory.GetSitecoreIdFromCommerceId(specificCategory));

            var orderHistoryFactory = new OrderHistoryFactory(commerceContext, _findEntitiesInListCommand);
            var categoryLines = AsyncHelper.RunSync(() => orderHistoryFactory.GetOrderHistory(categorySitecoreId, includeSubCategories));
            if (categoryLines == null)
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Sum(x => x.Quantity);
            return Pm_Compares.Evaluate(productAmount, specificValue);
        }
    }
}