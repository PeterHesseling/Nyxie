﻿using System;
using System.Linq;
using System.Net.Http;

using Nyxie.Plugin.Promotions.Actions;
using Nyxie.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Nyxie.Plugin.Promotions.Tests.Actions
{
    [Collection("Engine collection")]
    public class CartItemsMatchingInCategoryPriceDiscountActionTests
    {
        private readonly EngineFixture fixture;

        public CartItemsMatchingInCategoryPriceDiscountActionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_benefit_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartItemsMatchingInCategoryPriceDiscountActionBuilder()
                                                   .AmountOff(10)
                                                   .ForCategory("Laptops")
                                                   .Operator(Operator.Equal)
                                                   .NumberOfProducts(1)
                                                   .ApplyActionTo(ApplicationOrder.Ascending)
                                                   .ActionLimit(1))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment =
                line.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            // Subtotal = 30, Tax is 10% = 3, Fulfillment fee = 5
            Assert.Equal(38, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartItemsMatchingInCategoryPriceDiscountActionBuilder()
                                                   .AmountOff(10)
                                                   .ForCategory("Laptops")
                                                   .Operator(Operator.Equal)
                                                   .NumberOfProducts(1)
                                                   .ApplyActionTo(ApplicationOrder.Ascending)
                                                   .ActionLimit(1))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("34345454").Price(40))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "001");

            Assert.DoesNotContain(line.Adjustments,
                x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
        }

        [Fact]
        public async void Should_benefit_in_descending_order_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartItemsMatchingInCategoryPriceDiscountActionBuilder()
                                                   .AmountOff(10)
                                                   .ForCategory("Laptops")
                                                   .Operator(Operator.Equal)
                                                   .NumberOfProducts(2)
                                                   .ApplyActionTo(ApplicationOrder.Descending)
                                                   .ActionLimit(1))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "002");
            AwardedAdjustment adjustment =
                line.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            // Subtotal = 80, Tax is 10% = 8, Fulfillment fee = 5
            Assert.Equal(93, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_benefit_multiple_times_when_within_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartItemsMatchingInCategoryPriceDiscountActionBuilder()
                                                   .AmountOff(10)
                                                   .ForCategory("Laptops")
                                                   .Operator(Operator.Equal)
                                                   .NumberOfProducts(2)
                                                   .ApplyActionTo(ApplicationOrder.Ascending)
                                                   .ActionLimit(2))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment =
                firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            CartLineComponent secondLine = resultCart.Lines.Single(x => x.Id == "002");
            AwardedAdjustment secondAdjustment =
                secondLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
            Assert.Equal(-10, secondAdjustment.Adjustment.Amount);

            // Subtotal = 70, Tax is 10% = 7, Fulfillment fee = 5
            Assert.Equal(82, resultCart.Totals.GrandTotal.Amount);
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
        public async void Should_match_operator(Operator @operator, int numberOfProductsInPromotion, int numberOfProductsInCart,
            bool shouldQualify)
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartItemsMatchingInCategoryPriceDiscountActionBuilder()
                                                   .AmountOff(10)
                                                   .ForCategory("Laptops")
                                                   .Operator(@operator)
                                                   .NumberOfProducts(numberOfProductsInPromotion)
                                                   .ApplyActionTo(ApplicationOrder.Ascending)
                                                   .ActionLimit(numberOfProductsInPromotion))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder()
                                         .IdentifiedBy("001").Quantity(numberOfProductsInCart).InCategory("435345345").Price(40))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "001");

            if (shouldQualify)
                Assert.Contains(line.Adjustments, x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
            else
                Assert.DoesNotContain(line.Adjustments,
                    x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPriceDiscountAction));
        }
    }
}
