using System.Net.Http;

using Nyxie.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Nyxie.Plugin.Promotions.Tests.Conditions
{
    [Collection("Engine collection")]
    public class CartPaymentConditionTests
    {
        private readonly EngineFixture fixture;

        public CartPaymentConditionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_qualify_when_operator_is_equal_and_payment_method_is_same()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartPaymentConditionBuilder()
                                                     .Equal()
                                                     .WithValue("Federated"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithPaymentMethod(new EntityReference("001", "Federated"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_operator_is_equal_and_payment_method_is_different()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartPaymentConditionBuilder()
                                                     .Equal()
                                                     .WithValue("Federated"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithPaymentMethod(new EntityReference("001", "Express"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_qualify_when_operator_is_not_equal_and_payment_method_is_different()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartPaymentConditionBuilder()
                                                     .NotEqual()
                                                     .WithValue("Federated"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithPaymentMethod(new EntityReference("001", "Express"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_operator_is_not_equal_and_payment_method_is_same()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartPaymentConditionBuilder()
                                                     .NotEqual()
                                                     .WithValue("Federated"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithPaymentMethod(new EntityReference("001", "Federated"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }
    }
}
