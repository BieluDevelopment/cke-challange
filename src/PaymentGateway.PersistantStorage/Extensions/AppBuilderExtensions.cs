
using Microsoft.Extensions.Hosting;

namespace PaymentGateway.PersistantStorage.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddPersistentStorage(this IHostApplicationBuilder builder)
    {
      
        return builder;
    }
}