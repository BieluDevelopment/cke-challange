
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PaymentGateway.PersistantStorage;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Extensions;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.UnitTests;

public abstract class BaseTest :IDisposable
{
    public IDbContextFactory<PaymentGatewayDbContext> PrepareSubDbContextFcatory()
    {
        var dbContextFactorySub = Substitute.For<IDbContextFactory<PaymentGatewayDbContext>>();
        EncryptionExtension.SetEncryptionKey("22ca492b4e814cab8d1b1dc4f0f560d4"); //unused key, only for testing
        dbContextFactorySub.CreateDbContextAsync().Returns(e=>
        {
            var options = new DbContextOptionsBuilder<PaymentGatewayDbContext>()
                .UseInMemoryDatabase(databaseName:"test")
                .Options;
            return new PaymentGatewayDbContext(options);
        });
        return dbContextFactorySub;
    }

    public void Dispose()
    {
        
    }
}
public class MerchantRepositoryUnitTests : BaseTest
{
    private IMerchantRepository PrepareForTest()
    {
        return new MerchantRepository(PrepareSubDbContextFcatory());
    }

    [Fact]
    public async Task UpsertMerchantAsync_ShouldAddNewMerchant()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "test-api-key-12345678901234567890"
        };

        // Act
        await repository.UpsertMerchantAsync(merchant);

        // Assert
        var result = await repository.GetMerchantAsync(merchant.Id);
        Assert.NotNull(result);
        Assert.Equal(merchant.Id, result.Id);
        Assert.Equal(merchant.ApiKey, result.ApiKey);
    }

    [Fact]
    public async Task UpsertMerchantAsync_ShouldUpdateExistingMerchant()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchantId = Guid.NewGuid();
        var originalMerchant = new Merchant
        {
            Id = merchantId,
            ApiKey = "original-api-key-123456789012"
        };

        await repository.UpsertMerchantAsync(originalMerchant);

        var updatedMerchant = new Merchant
        {
            Id = merchantId,
            ApiKey = "updated-api-key-123456789012"
        };

        // Act
        await repository.UpsertMerchantAsync(updatedMerchant);

        // Assert
        var result = await repository.GetMerchantAsync(merchantId);
        Assert.NotNull(result);
        Assert.Equal("updated-api-key-123456789012", result.ApiKey);
    }

    [Fact]
    public async Task GetMerchantAsync_ShouldReturnMerchant_WhenExists()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "test-api-key-12345678901234567890"
        };

        await repository.UpsertMerchantAsync(merchant);

        // Act
        var result = await repository.GetMerchantAsync(merchant.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(merchant.Id, result.Id);
        Assert.Equal(merchant.ApiKey, result.ApiKey);
    }

    [Fact]
    public async Task GetMerchantAsync_ShouldReturnNull_WhenNotExists()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetMerchantAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMerchants_ShouldReturnAllMerchants()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant1 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-merchant-1-1234567890123"
        };

        var merchant2 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-merchant-2-1234567890123"
        };

        var merchant3 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-merchant-3-1234567890123"
        };

        await repository.UpsertMerchantAsync(merchant1);
        await repository.UpsertMerchantAsync(merchant2);
        await repository.UpsertMerchantAsync(merchant3);

        // Act
        var results = await repository.GetMerchants();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count());
        Assert.Contains(results, m => m.Id == merchant1.Id);
        Assert.Contains(results, m => m.Id == merchant2.Id);
        Assert.Contains(results, m => m.Id == merchant3.Id);
    }

    [Fact]
    public async Task GetMerchants_ShouldReturnEmpty_WhenNoMerchantsExist()
    {
        var repository = PrepareForTest();
        
        // Act
        var results = await repository.GetMerchants();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnMerchant_WhenExists()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var apiKey = "unique-api-key-12345678901234567";
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey
        };

        await repository.UpsertMerchantAsync(merchant);

        // Act
        var result = await repository.GetMerchantByApiKey(apiKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(merchant.Id, result.Id);
        Assert.Equal(apiKey, result.ApiKey);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenNotExists()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var nonExistentApiKey = "non-existent-api-key-12345678";

        // Act
        var result = await repository.GetMerchantByApiKey(nonExistentApiKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenApiKeyIsNull()
    {
        var repository = PrepareForTest();
        
        // Act
        var result = await repository.GetMerchantByApiKey(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenApiKeyIsEmpty()
    {
        var repository = PrepareForTest();
        
        // Act
        var result = await repository.GetMerchantByApiKey(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnCorrectMerchant_WhenMultipleMerchantsExist()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant1 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-1-12345678901234567890"
        };

        var merchant2 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-2-12345678901234567890"
        };

        var merchant3 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-3-12345678901234567890"
        };

        await repository.UpsertMerchantAsync(merchant1);
        await repository.UpsertMerchantAsync(merchant2);
        await repository.UpsertMerchantAsync(merchant3);

        // Act
        var result = await repository.GetMerchantByApiKey(merchant2.ApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(merchant2.Id, result.Id);
        Assert.Equal(merchant2.ApiKey, result.ApiKey);
    }

    [Fact]
    public async Task DeleteMerchantAsync_ShouldRemoveMerchant()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "test-api-key-12345678901234567890"
        };

        await repository.UpsertMerchantAsync(merchant);

        // Act
        await repository.DeleteMerchantAsync(merchant.Id);

        // Assert
        var result = await repository.GetMerchantAsync(merchant.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteMerchantAsync_ShouldNotThrow_WhenMerchantDoesNotExist()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await repository.DeleteMerchantAsync(nonExistentId));

        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteMerchantAsync_ShouldOnlyDeleteSpecifiedMerchant()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant1 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-1-12345678901234567890"
        };

        var merchant2 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "api-key-2-12345678901234567890"
        };

        await repository.UpsertMerchantAsync(merchant1);
        await repository.UpsertMerchantAsync(merchant2);

        // Act
        await repository.DeleteMerchantAsync(merchant1.Id);

        // Assert
        var deletedMerchant = await repository.GetMerchantAsync(merchant1.Id);
        var remainingMerchant = await repository.GetMerchantAsync(merchant2.Id);

        Assert.Null(deletedMerchant);
        Assert.NotNull(remainingMerchant);
        Assert.Equal(merchant2.Id, remainingMerchant.Id);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldBeCaseSensitive()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = "ApiKey-12345678901234567890"
        };

        await repository.UpsertMerchantAsync(merchant);

        // Act
        var resultUpperCase = await repository.GetMerchantByApiKey("APIKEY-12345678901234567890");
        var resultLowerCase = await repository.GetMerchantByApiKey("apikey-12345678901234567890");
        var resultCorrectCase = await repository.GetMerchantByApiKey("ApiKey-12345678901234567890");

        // Assert
        Assert.Null(resultUpperCase);
        Assert.Null(resultLowerCase);
        Assert.NotNull(resultCorrectCase);
        Assert.Equal(merchant.Id, resultCorrectCase.Id);
    }

    [Fact]
    public async Task UpsertMerchantAsync_ShouldHandleMultipleOperations()
    {
        var repository = PrepareForTest();
        
        // Arrange
        var merchantId = Guid.NewGuid();
        var merchant1 = new Merchant
        {
            Id = merchantId,
            ApiKey = "first-key-12345678901234567890"
        };

        var merchant2 = new Merchant
        {
            Id = merchantId,
            ApiKey = "second-key-12345678901234567890"
        };

        var merchant3 = new Merchant
        {
            Id = merchantId,
            ApiKey = "third-key-12345678901234567890"
        };

        // Act
        await repository.UpsertMerchantAsync(merchant1);
        await repository.UpsertMerchantAsync(merchant2);
        await repository.UpsertMerchantAsync(merchant3);

        // Assert
        var result = await repository.GetMerchantAsync(merchantId);
        Assert.NotNull(result);
        Assert.Equal("third-key-12345678901234567890", result.ApiKey);

        var allMerchants = await repository.GetMerchants();
        Assert.Single(allMerchants);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}