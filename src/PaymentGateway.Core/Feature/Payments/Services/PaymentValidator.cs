using FluentValidation;

using PaymentGateway.Models.Entities.Requests;

namespace PaymentGateway.Core.Feature.Payments.Services;

public class PaymentValidator: AbstractValidator<MerchantPaymentProcessRequest> {
    public PaymentValidator()
    {
        SetRulesForCardNumber();
        SetRulesForMonth();
        SetRulesForYear();
        SetRulesForMonthAndYear();
        SetRulesForCurrency();
        SetRulesForAmount();
        SetRulesForCvv();
        
    }

    private void SetRulesForCvv()
    {
        RuleFor(x => x.Cvv).NotEmpty().WithMessage("Cvv is required.");
        RuleFor(x=>x.Cvv).Length(3,4).WithMessage("Cvv length must be between 3 and 4 digits.");
        RuleFor(x => x.Cvv).Matches("^[0-9]{3,4}$").WithMessage("Cvv must contain only numeric characters.");

    }

    private void SetRulesForAmount()
    {
        RuleFor(x => x.Amount).NotEmpty().WithMessage("Amount is required.");

    }

    private void SetRulesForMonthAndYear()
    {
        RuleFor(x => new {x.ExpiryMonth, x.ExpiryYear})
            .Must(x => x.ExpiryYear>2000 && x.ExpiryMonth is > 0 and < 13 && new DateTime(x.ExpiryYear, x.ExpiryMonth.Value, 1) > DateTime.Now).WithMessage("Expiry month and year must be in the future.");
    }

    private void SetRulesForCurrency()
    {
        RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required.");
        RuleFor(x => x.Currency).Length(3,3).WithMessage("Currency must be a 3 character ISO code.");
        string[] validCurrencies = ["USD", "EUR", "GBP"];
        RuleFor(x => x.Currency).Must(x=>validCurrencies.Contains(x)).WithMessage("Currency is not supported.");
        

    }

    private void SetRulesForYear()
    {
        RuleFor(x => x.ExpiryYear).NotEmpty().WithMessage("Expiry year is required.");
        RuleFor(x => x.ExpiryYear).GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Year must be same or higher than current year.");
        
    }

    private void SetRulesForCardNumber()
    {
        RuleFor(x => x.CardNumber).NotEmpty().WithMessage("Card number is required.");
        RuleFor(x=>x.CardNumber).Length(14,19).WithMessage(
            "The length of 'Card Number' must be at least 14 characters and lower than 19 characters.");
        RuleFor(x => x.CardNumber).Matches("^[0-9]{14,19}$").WithMessage("Card number must contain only numeric characters.");
    }
    private void SetRulesForMonth()
    {
        RuleFor(x => x.ExpiryMonth).NotNull().WithMessage("Expiry month is required.");
        RuleFor(x => x.ExpiryMonth).GreaterThan(0).WithMessage("Expiry month must be between 1 and 12.")
            .LessThanOrEqualTo(12).WithMessage("Expiry month must be between 1 and 12.");
    }
}