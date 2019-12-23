﻿using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartItemsMatchingInCategoryPercentageDiscountActionBuilder : IBenefitBuilder
    {
        private string percentageOff = "10";
        private Operator @operator;
        private int numberOfProducts;
        private int actionLimit;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder PercentageOff(string value)
        {
            percentageOff = value;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder Operator(Operator @operator)
        {
            this.@operator = @operator;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder NumberOfProducts(int numberOfProducts)
        {
            this.numberOfProducts = numberOfProducts;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder ActionLimit(int limit)
        {
            actionLimit = limit;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder ApplyActionTo(ApplicationOrder applicationOrder)
        {
            this.applicationOrder = applicationOrder;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartItemsMatchingInCategoryPercentageDiscountActionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }

        public ActionModel Build()
        {
            string comparer;
            switch (@operator)
            {
                case Builders.Operator.NotEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalNotEqualityOperator";
                    break;
                case Builders.Operator.GreaterThan:
                    comparer = "Sitecore.Framework.Rules.DecimalGreaterThanOperator";
                    break;
                case Builders.Operator.GreaterThanOrEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator";
                    break;
                case Builders.Operator.LessThanOrEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalLessThanEqualToOperator";
                    break;
                case Builders.Operator.LessThan:
                    comparer = "Sitecore.Framework.Rules.DecimalLessThanOperator";
                    break;
                default:
                    comparer = "Sitecore.Framework.Rules.DecimalEqualityOperator";
                    break;
            }

            return new ActionModel
            {
                Name = "Pm_CartItemsMatchingInCategoryPercentageDiscountAction",
                LibraryId = "Pm_CartItemsMatchingInCategoryPercentageDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_PercentageOff",
                        Value = percentageOff
                    },
                    new PropertyModel
                    {
                        Name = "Pm_SpecificValue",
                        Value = numberOfProducts.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Pm_Operator",
                        Value = comparer,
                        IsOperator = true
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ApplyActionTo",
                        Value = applicationOrder.Name
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ActionLimit",
                        Value = actionLimit.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Pm_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Pm_IncludeSubCategories",
                        Value = includeSubCategories
                    }
                }
            };
        }
    }
}
