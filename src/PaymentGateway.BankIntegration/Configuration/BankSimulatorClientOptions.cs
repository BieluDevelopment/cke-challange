namespace PaymentGateway.BankIntegration.Configuration;

public class BankSimulatorClientOptions
{
    public const string SectionName = "services__bankSimulator";
    public IEnumerable<string> Imposters { get; set; } = [];
}

