﻿using Catalog.API.Exceptions;

namespace Catalog.API.Products.UpdateProduct
{
    public record UpdateProductCommand(Guid Id, string Name, List<string> Categories, string Description, string ImageFile, decimal Price) : ICommand<UpdateProductResult>;

    public record UpdateProductResult(bool IsSuccess);

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("Product ID is required.");
            RuleFor(c => c.Name).NotEmpty()
                .WithMessage("Name is required.")
                .Length(2, 150).WithMessage("Name length must be between 2 and 150 characters.");
            RuleFor(c => c.Categories).NotEmpty().WithMessage("Categories is required.");
            RuleFor(c => c.ImageFile).NotEmpty().WithMessage("ImageFile is required.");
            RuleFor(c => c.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }

    internal class UpdateProductCommandHandler(IDocumentSession session)
        : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            //product.Name = command.Name;
            //product.Description = command.Description;
            //product.ImageFile = command.ImageFile;
            //product.Price = command.Price;
            //product.Categories = command.Categories;

            product = command.Adapt<Product>();

            session.Update(product);
            await session.SaveChangesAsync(cancellationToken);

            return new UpdateProductResult(true);
        }
    }
}
