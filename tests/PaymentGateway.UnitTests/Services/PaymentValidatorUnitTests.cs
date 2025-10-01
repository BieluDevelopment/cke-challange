using AwesomeAssertions;
using PaymentGateway.Core.Feature.Payments.Services;
using PaymentGateway.Models.Entities.Requests;

namespace PaymentGateway.UnitTests.Services;

public class PaymentValidatorTests
{
    private readonly PaymentValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CardNumber_Should_Be_Required(string cardNumber)
    {
        var model = ValidModel();
        model.CardNumber = cardNumber;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.CardNumber));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.CardNumber));
        error.ErrorMessage.Should().Be("Card number is required.");
    }

    [Theory]
    [InlineData("1234567890123")]
    [InlineData("12345678901234567890")]
    public void CardNumber_Should_Be_Between_14_And_19_Characters(string cardNumber)
    {
        var model = ValidModel();
        model.CardNumber = cardNumber;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.CardNumber));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.CardNumber));
        error.ErrorMessage.Should().Be("The length of 'Card Number' must be at least 14 characters and lower than 19 characters.");
    }

    [Theory]
    [InlineData("1234567890123a")]
    [InlineData("1234-5678-9012-34")]
    public void CardNumber_Should_Contain_Only_Numeric_Characters(string cardNumber)
    {
        var model = ValidModel();
        model.CardNumber = cardNumber;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.CardNumber));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.CardNumber));
        error.ErrorMessage.Should().Be("Card number must contain only numeric characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void ExpiryMonth_Should_Be_Between_1_And_12(int month)
    {
        var model = ValidModel();
        model.ExpiryMonth = month;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.ExpiryMonth));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.ExpiryMonth));
        error.ErrorMessage.Should().Be("Expiry month must be between 1 and 12.");
    }

    [Fact]
    public void ExpiryMonth_Should_Be_Required()
    {
        var model = ValidModel();
        model.ExpiryMonth = null;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.ExpiryMonth));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.ExpiryMonth));
        error.ErrorMessage.Should().Be("Expiry month is required.");
    }

    [Fact]
    public void ExpiryYear_Should_Be_Required()
    {
        var model = ValidModel();
        model.ExpiryYear = 0;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.ExpiryYear));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.ExpiryYear));
        error.ErrorMessage.Should().Be("Expiry year is required.");
    }

    [Fact]
    public void ExpiryYear_Should_Be_In_The_Future()
    {
        var model = ValidModel();
        model.ExpiryYear = DateTime.Now.Year - 1;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.ExpiryYear));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.ExpiryYear));
        error.ErrorMessage.Should().Be("Year must be same or higher than current year.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(13)]
    public void ExpiryMonth_And_Year_Should_Be_Equal_Or_Later_Than_Current_Date(int month)
    {
        var model = ValidModel();
        model.ExpiryYear = DateTime.Now.Year;
        model.ExpiryMonth = month;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x =>
            x.ErrorMessage == "The expiry month and year must be in the future.");
        error.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Currency_Should_Be_Required(string currency)
    {
        var model = ValidModel();
        model.Currency = currency;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Currency));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Currency));
        error.ErrorMessage.Should().Be("Currency is required.");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Currency_Should_Be_Three_Characters(string currency)
    {
        var model = ValidModel();
        model.Currency = currency;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Currency));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Currency));
        error.ErrorMessage.Should().Be("Currency must be a 3 character ISO code.");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Currency_Should_Be_Valid(string currency)
    {
        var model = ValidModel();
        model.Currency = currency;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Currency));
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("PLN")]
    public void Currency_Should_Be_One_Of_Allowed(string currency)
    {
        var model = ValidModel();
        model.Currency = currency;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Currency));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Currency));
        error.ErrorMessage.Should().Be("Currency is not supported.");
    }

    [Fact]
    public void Amount_Should_Be_Required()
    {
        var model = ValidModel();
        model.Amount = null;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Amount));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Amount));
        error.ErrorMessage.Should().Be("Amount is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Cvv_Should_Be_Required(string cvv)
    {
        var model = ValidModel();
        model.Cvv = cvv;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Cvv));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Cvv));
        error.ErrorMessage.Should().Be("Cvv is required.");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    public void Cvv_Should_Be_Three_Or_Four_Characters(string cvv)
    {
        var model = ValidModel();
        model.Cvv = cvv;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Cvv));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Cvv));
        error.ErrorMessage.Should().Be("Cvv length must be between 3 and 4 digits.");
    }

    [Theory]
    [InlineData("12a")]
    [InlineData("1 23")]
    public void Cvv_Should_Contain_Only_Numeric_Characters(string cvv)
    {
        var model = ValidModel();
        model.Cvv = cvv;
        var result = _validator.Validate(model);
        var error = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(model.Cvv));
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(nameof(model.Cvv));
        error.ErrorMessage.Should().Be("Cvv must contain only numeric characters.");
    }

    private MerchantPaymentProcessRequest ValidModel() => new()
    {
        CardNumber = "12345678901234",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.Now.Year + 1,
        Currency = "USD",
        Amount = 100,
        Cvv = "123"
    };
}