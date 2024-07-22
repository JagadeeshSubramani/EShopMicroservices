using Catalog.API.Exceptions;

namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;

    public record DeleteProductResult(bool IsSuccess);

    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("Product ID is required.");
        }
    }

    internal class DeleteProductCommandHandler(IDocumentSession session, ILogger<DeleteProductCommandHandler> logger)
        : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("DeleteProductCommandHandler.Handle called with {@command}", command);

            var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            session.Delete<Product>(command.Id);

            await session.SaveChangesAsync();

            return new DeleteProductResult(true);
        }
    }
}
