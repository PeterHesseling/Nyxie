﻿using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartProductTotalInCategoryConditionBuilder : IQualificationBuilder
    {
        private string category = "";
        private string includeSubCategories = "false";
        private Operator @operator = Builders.Operator.Equal;
        private decimal total;

        public ConditionModel Build()
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

            return new ConditionModel
            {
                Name = "Ny_CartProductTotalInCategoryCondition",
                LibraryId = "Ny_CartProductTotalInCategoryCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Ny_Compares",
                        Value = comparer,
                        IsOperator = true
                    },
                    new PropertyModel
                    {
                        Name = "Ny_SpecificValue",
                        Value = total.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_IncludeSubCategories",
                        Value = includeSubCategories
                    }
                }
            };
        }

        public CartProductTotalInCategoryConditionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public CartProductTotalInCategoryConditionBuilder Total(decimal total)
        {
            this.total = total;
            return this;
        }

        public CartProductTotalInCategoryConditionBuilder Operator(Operator @operator)
        {
            this.@operator = @operator;
            return this;
        }

        public CartProductTotalInCategoryConditionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartProductTotalInCategoryConditionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }
    }
}
