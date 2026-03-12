using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

public static class OtelMetrics
{
    /*
     * Provider will subscribe to all Meters. We just need to
     * keep a reference so that it does not get garbage collected.
     */
    public static MeterProvider Provider;
    public static readonly Meter Meter = new("custom-metrics");

    public static void Init()
    {
        if (Provider != null)
        {
            return;
        }

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(Environment.GetEnvironmentVariable("ELASTIC_APM_SERVICE_NAME") ?? "UNSET")
            .AddAttributes(new Dictionary<string, object>
            {
                // ["service.version"] = "",
                ["host.name"] = Environment.MachineName,
                ["process.pid"] = Environment.ProcessId

            });

        Provider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("custom-metrics")
            .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                exporterOptions.Endpoint = new Uri(Environment.GetEnvironmentVariable("ELASTIC_APM_SERVER_URL") + "/api/metrics/v1/save/otlp");
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;

                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 60_000;
            })
            /*
            * ConsoleExporter can be used in place of OtlpExporter
            * for testing. It will put the metrics data on console.
            */
            // .AddConsoleExporter()
            .Build();
    }
}