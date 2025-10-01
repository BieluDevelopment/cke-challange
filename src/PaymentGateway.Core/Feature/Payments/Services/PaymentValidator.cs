using FluentValidation;

using PaymentGateway.Models.Entities.Requests;

namespace PaymentGateway.Core.Feature.Payments.Services;

public class PaymentValidator: AbstractValidator<MerchantPaymentProcessRequest> {
    public PaymentValidator()
    {
        SetRulesForCardNumber();
    }

    private void SetRulesForCardNumber()
    {
        RuleFor(x => x.CardNumber).NotEmpty().WithMessage("Card number is required");
    }
}