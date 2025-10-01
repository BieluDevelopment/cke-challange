using NSubstitute;

using PaymentGateway.Core.Feature.Merchants.Services;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.UnitTests.Services;

public class MerchantServiceUnitTests  : BaseTest
{
      private (IMerchantService service, IMerchantRepository repository) PrepareForTest()
    {
        var repository = Substitute.For<IMerchantRepository>();
        var service = new MerchantService(repository);

        return (service, repository);
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldCreateNewMerchant()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.UpsertMerchantAsync(Arg.Any<Merchant>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.NotNull(result.Value.ApiKey);
        Assert.NotEmpty(result.Value.ApiKey);

        await context.repository.Received(1).UpsertMerchantAsync(Arg.Is<Merchant>(m =>
            m.Id != Guid.Empty &&
            !string.IsNullOrEmpty(m.ApiKey)
        ));
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldGenerateUniqueId()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.UpsertMerchantAsync(Arg.Any<Merchant>())
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await context.service.CreateMerchantAsync();
        var result2 = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldGenerateUniqueApiKey()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.UpsertMerchantAsync(Arg.Any<Merchant>())
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await context.service.CreateMerchantAsync();
        var result2 = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value.ApiKey, result2.Value.ApiKey);
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldCallRepositoryUpsert()
    {
        var context = PrepareForTest();

        // Arrange
        Merchant? capturedMerchant = null;
        await context.repository.UpsertMerchantAsync(Arg.Do<Merchant>(m => capturedMerchant = m));

        // Act
        var result = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMerchant);
        Assert.Equal(result.Value.Id, capturedMerchant.Id);
        Assert.Equal(result.Value.ApiKey, capturedMerchant.ApiKey);

        await context.repository.Received(1).UpsertMerchantAsync(Arg.Any<Merchant>());
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldGenerateApiKeyWithoutDashes()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.UpsertMerchantAsync(Arg.Any<Merchant>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("-", result.Value.ApiKey);
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldReturnTrue_WhenApiKeyExists()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "valid-api-key-12345678901234567890";
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey
        };

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns(merchant);

        // Act
        var result = await context.service.IsValidApiKeyAsync(apiKey);

        // Assert
        Assert.True(result);
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldReturnFalse_WhenApiKeyDoesNotExist()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "invalid-api-key-12345678901234567890";

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.IsValidApiKeyAsync(apiKey);

        // Assert
        Assert.False(result);
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldReturnFalse_WhenApiKeyIsNull()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.GetMerchantByApiKey(null)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.IsValidApiKeyAsync(null);

        // Assert
        Assert.False(result);
        await context.repository.Received(1).GetMerchantByApiKey(null);
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldReturnFalse_WhenApiKeyIsEmpty()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.GetMerchantByApiKey(string.Empty)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.IsValidApiKeyAsync(string.Empty);

        // Assert
        Assert.False(result);
        await context.repository.Received(1).GetMerchantByApiKey(string.Empty);
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldCallRepository_WithCorrectApiKey()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "test-api-key-12345678901234567890";
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey
        };

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns(merchant);

        // Act
        await context.service.IsValidApiKeyAsync(apiKey);

        // Assert
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
        await context.repository.DidNotReceive().GetMerchantByApiKey(Arg.Is<string>(k => k != apiKey));
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnMerchant_WhenApiKeyExists()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "valid-api-key-12345678901234567890";
        var expectedMerchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey
        };

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns(expectedMerchant);

        // Act
        var result = await context.service.GetMerchantByApiKey(apiKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMerchant.Id, result.Id);
        Assert.Equal(expectedMerchant.ApiKey, result.ApiKey);

        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenApiKeyDoesNotExist()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "invalid-api-key-12345678901234567890";

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.GetMerchantByApiKey(apiKey);

        // Assert
        Assert.Null(result);
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenApiKeyIsNull()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.GetMerchantByApiKey(null)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.GetMerchantByApiKey(null);

        // Assert
        Assert.Null(result);
        await context.repository.Received(1).GetMerchantByApiKey(null);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnNull_WhenApiKeyIsEmpty()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.GetMerchantByApiKey(string.Empty)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.GetMerchantByApiKey(string.Empty);

        // Assert
        Assert.Null(result);
        await context.repository.Received(1).GetMerchantByApiKey(string.Empty);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldReturnCorrectMerchant_WhenMultipleMerchantsExist()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey1 = "api-key-1-12345678901234567890";
        var apiKey2 = "api-key-2-12345678901234567890";
        
        var merchant1 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey1
        };

        var merchant2 = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey2
        };

        context.repository.GetMerchantByApiKey(apiKey1)
            .Returns(merchant1);
        
        context.repository.GetMerchantByApiKey(apiKey2)
            .Returns(merchant2);

        // Act
        var result1 = await context.service.GetMerchantByApiKey(apiKey1);
        var result2 = await context.service.GetMerchantByApiKey(apiKey2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(merchant1.Id, result1.Id);
        Assert.Equal(merchant2.Id, result2.Id);
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public async Task GetMerchantByApiKey_ShouldPassCorrectApiKeyToRepository()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "specific-api-key-12345678901234567890";
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            ApiKey = apiKey
        };

        context.repository.GetMerchantByApiKey(apiKey)
            .Returns(merchant);

        // Act
        await context.service.GetMerchantByApiKey(apiKey);

        // Assert
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }

    [Fact]
    public async Task CreateMerchantAsync_ShouldReturnMerchantWithValidProperties()
    {
        var context = PrepareForTest();

        // Arrange
        context.repository.UpsertMerchantAsync(Arg.Any<Merchant>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await context.service.CreateMerchantAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        // Validate Id
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        
        // Validate ApiKey
        Assert.NotNull(result.Value.ApiKey);
        Assert.NotEmpty(result.Value.ApiKey);
        Assert.True(result.Value.ApiKey.Length >= 32); // Typical GUID without dashes is 32 chars
    }

    [Fact]
    public async Task IsValidApiKeyAsync_ShouldHandleWhitespaceApiKey()
    {
        var context = PrepareForTest();

        // Arrange
        var apiKey = "   ";
        context.repository.GetMerchantByApiKey(apiKey)
            .Returns((Merchant?)null);

        // Act
        var result = await context.service.IsValidApiKeyAsync(apiKey);

        // Assert
        Assert.False(result);
        await context.repository.Received(1).GetMerchantByApiKey(apiKey);
    }
}