﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

using Nyxie.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Nyxie.Plugin.Promotions.Tests.Conditions
{
    [Collection("Engine collection")]
    public class OrderHistoryTotalInCategoryConditionTests
    {
        private readonly EngineFixture fixture;

        public OrderHistoryTotalInCategoryConditionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_qualify_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(33)
                                     .InCategory("435345345"))
                          .Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .Total(33)
                                                     .ForCategory("Laptops"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_qualify_when_category_matches_multiple_orders()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(33)
                                     .InCategory("435345345"))
                          .Build();

            Order secondOrder = new OrderBuilder()
                                .WithLines(new LineBuilder()
                                           .Quantity(1)
                                           .Price(33)
                                           .InCategory("435345345"))
                                .Build();

            fixture.Factory.AddEntitiesToList(string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId),
                order, secondOrder);

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .Total(66) // Both orders have 1 product and total should be 2
                                                     .ForCategory("Laptops"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(33)
                                     .InCategory("435345345"))
                          .Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .Total(33)
                                                     .ForCategory("Tablets"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_qualify_when_includes_sub_category()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(33)
                                     .InCategory("/435345345/subcategory"))
                          .Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .Total(33)
                                                     .ForCategory("Laptops")
                                                     .IncludeSubCategories())
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_does_not_includes_sub_category()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(33)
                                     .InCategory("/435345345/subcategory"))
                          .Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .Total(33)
                                                     .ForCategory("Laptops")
                                                     .DoesNotIncludeSubCategories())
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Theory]
        [InlineData(Operator.Equal, 10, 10, true)]
        [InlineData(Operator.Equal, 10, 9, false)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 10, true)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 11, true)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 9, false)]
        [InlineData(Operator.GreaterThan, 10, 11, true)]
        [InlineData(Operator.GreaterThan, 10, 10, false)]
        [InlineData(Operator.LessThanOrEqual, 10, 10, true)]
        [InlineData(Operator.LessThanOrEqual, 10, 9, true)]
        [InlineData(Operator.LessThanOrEqual, 10, 11, false)]
        [InlineData(Operator.LessThan, 10, 9, true)]
        [InlineData(Operator.LessThan, 10, 10, false)]
        [InlineData(Operator.NotEqual, 9, 10, true)]
        [InlineData(Operator.NotEqual, 10, 10, false)]
        public async void Should_match_operator(Operator @operator, int totalRequired, int totalInOrder, bool shouldQualify)
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Guid customerId = Guid.NewGuid();

            Order order = new OrderBuilder()
                          .WithLines(new LineBuilder()
                                     .Quantity(1)
                                     .Price(totalInOrder)
                                     .InCategory("435345345"))
                          .Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new OrderHistoryTotalInCategoryConditionBuilder()
                                                     .Operator(@operator)
                                                     .Total(totalRequired)
                                                     .ForCategory("Laptops"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            if (shouldQualify)
                Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
            else
                Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }
    }
}
