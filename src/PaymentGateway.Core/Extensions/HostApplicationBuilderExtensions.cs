using Microsoft.Extensions.Hosting;

using PaymentGateway.Core.Feature.Merchants.Extensions;
using PaymentGateway.Core.Feature.Payments.Extensions;
using PaymentGateway.Core.Feature.Security.Extensions;

namespace PaymentGateway.Core.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBusinessLogic(this IHostApplicationBuilder builder)
    {
        builder.AddPayments();
        builder.AddMerchants();
        builder.AddSecurity();
        return builder;
    }

}