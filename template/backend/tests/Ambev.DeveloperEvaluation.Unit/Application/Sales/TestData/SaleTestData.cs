using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

/// <summary>
/// Provides methods for generating test data for Sale-related commands and entities.
/// Uses Bogus library to generate realistic test data.
/// </summary>
public static class SaleTestData
{
    /// <summary>
    /// Faker for generating valid CreateSaleCommand instances.
    /// </summary>
    private static readonly Faker<CreateSaleCommand> createSaleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => f.Commerce.Ean13())
        .RuleFor(s => s.CustomerId, f => Guid.NewGuid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, f => Guid.NewGuid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Date, f => f.Date.Recent(30))
        .RuleFor(s => s.Items, f => GenerateValidSaleItems(f.Random.Int(1, 3)));

    /// <summary>
    /// Faker for generating valid UpdateSaleCommand instances.
    /// </summary>
    private static readonly Faker<UpdateSaleCommand> updateSaleCommandFaker = new Faker<UpdateSaleCommand>()
        .RuleFor(s => s.SaleId, f => Guid.NewGuid())
        .RuleFor(s => s.Items, f => GenerateValidUpdateSaleItems(f.Random.Int(1, 3)));

    /// <summary>
    /// Faker for generating valid Sale entities.
    /// </summary>
    private static readonly Faker<Sale> saleFaker = new Faker<Sale>()
        .CustomInstantiator(f => new Sale
        {
            SaleNumber = f.Commerce.Ean13(),
            CustomerId = Guid.NewGuid(),
            CustomerName = f.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = f.Company.CompanyName(),
            Date = f.Date.Recent(30)
        });

    /// <summary>
    /// Generates a valid CreateSaleCommand with random data.
    /// </summary>
    public static CreateSaleCommand GenerateValidCreateSaleCommand()
    {
        return createSaleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates a valid UpdateSaleCommand with random data.
    /// </summary>
    public static UpdateSaleCommand GenerateValidUpdateSaleCommand()
    {
        return updateSaleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates a valid Sale entity with random data and items.
    /// </summary>
    public static Sale GenerateValidSale(int itemCount = 2)
    {
        var sale = saleFaker.Generate();

        // Set a valid Id using reflection since Id has a private setter
        typeof(Sale).GetProperty("Id")!.SetValue(sale, Guid.NewGuid());

        for (int i = 0; i < itemCount; i++)
        {
            var item = SaleItem.Create(
                Guid.NewGuid(),
                new Faker().Commerce.ProductName(),
                new Faker().Random.Int(1, 20),
                new Faker().Random.Decimal(10, 1000)
            );

            // Set a valid Id using reflection since Id has a private setter
            typeof(SaleItem).GetProperty("Id")!.SetValue(item, Guid.NewGuid());

            sale.AddItem(item);
        }

        return sale;
    }

    /// <summary>
    /// Generates a list of valid CreateSaleItemCommand instances.
    /// </summary>
    private static List<CreateSaleItemCommand> GenerateValidSaleItems(int count)
    {
        var faker = new Faker();
        var items = new List<CreateSaleItemCommand>();

        for (int i = 0; i < count; i++)
        {
            items.Add(new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = faker.Commerce.ProductName(),
                Quantity = faker.Random.Int(1, 20),
                UnitPrice = faker.Random.Decimal(10, 1000),
                Discount = 0 // Will be calculated by domain
            });
        }

        return items;
    }

    /// <summary>
    /// Generates a list of valid UpdateSaleItemCommand instances.
    /// </summary>
    private static List<UpdateSaleItemCommand> GenerateValidUpdateSaleItems(int count)
    {
        var faker = new Faker();
        var items = new List<UpdateSaleItemCommand>();

        for (int i = 0; i < count; i++)
        {
            items.Add(new UpdateSaleItemCommand
            {
                ItemId = Guid.Empty, // New item
                ProductId = Guid.NewGuid(),
                ProductName = faker.Commerce.ProductName(),
                Quantity = faker.Random.Int(1, 20),
                UnitPrice = faker.Random.Decimal(10, 1000),
                Discount = 0
            });
        }

        return items;
    }

    /// <summary>
    /// Generates an invalid CreateSaleCommand (missing required fields).
    /// </summary>
    public static CreateSaleCommand GenerateInvalidCreateSaleCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = "", // Invalid: empty
            CustomerId = Guid.Empty, // Invalid: empty guid
            CustomerName = "",
            BranchId = Guid.Empty,
            BranchName = "",
            Items = new List<CreateSaleItemCommand>() // Invalid: no items
        };
    }
}
