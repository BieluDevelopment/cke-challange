using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.Core.Feature.Payments.Services;

namespace PaymentGateway.Core.Feature.Payments.Extensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddPayments(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<PaymentValidator>();
        return builder;
    }

}