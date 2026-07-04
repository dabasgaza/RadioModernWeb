// ============================================================
// TestTelemetry — التليمتري
// ============================================================
// المسؤولية: تعريف التليمتري.
// ============================================================
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Radio.Tests.Helpers;

/// <summary>
/// صنف التليمتري.
/// </summary>
public static class TestTelemetry
{
    private static readonly Lazy<TelemetryClient> _instance = new(() =>
    {
        var config = new TelemetryConfiguration
        {
            ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost/"
        };
        return new TelemetryClient(config);
    });

    public static TelemetryClient Client => _instance.Value;
}
