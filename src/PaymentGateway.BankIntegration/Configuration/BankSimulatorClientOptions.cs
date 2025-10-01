namespace PaymentGateway.BankIntegration.Configuration;

public class BankSimulatorClientOptions
{
    public const string SectionName = "services:bankSimulator";
    public List<string> Imposters { get; set; } = [];
}

