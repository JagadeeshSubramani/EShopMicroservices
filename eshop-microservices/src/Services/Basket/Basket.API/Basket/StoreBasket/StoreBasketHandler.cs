namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);

    public class StoreBasketValidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketValidator()
        {
            RuleFor(s => s.Cart).NotNull().WithMessage("Cart can not be null.");
            RuleFor(s => s.Cart.UserName).NotEmpty().WithMessage("UserName is required.");
        }

        internal class StoreBasketCommandHandler(IBasketRepository repository) : ICommandHandler<StoreBasketCommand, StoreBasketResult>
        {
            public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
            {
                ShoppingCart cart = await repository.StoreAsync(command.Cart, cancellationToken);

                return new StoreBasketResult(cart.UserName);
            }
        }
    }
}
