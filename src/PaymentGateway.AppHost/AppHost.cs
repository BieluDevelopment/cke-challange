var builder = DistributedApplication.CreateBuilder(args);

var bankContainer = builder.AddContainer("bankSimulator", "bbyars/mountebank:2.8.1")
    .WithBindMount("../../imposters", "/imposters")
    .WithArgs("--configfile", "/imposters/bank_simulator.ejs", "--allowInjection")
    .WithHttpEndpoint(port: 2525, targetPort: 2525, name: "management", isProxied: false)
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "imposters", isProxied: false);
var apiService = bankContainer.GetEndpoint("imposters");
var postgres = builder.AddPostgres("postgressDb", port: 59674)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin(x =>
    {
        x.WithHostPort(5050);
    });
builder.AddProject<Projects.PaymentGateway_Web>("payment-gateway")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(postgres)
    .WaitFor(bankContainer)
    .WaitFor(postgres);
;

builder.Build().Run();