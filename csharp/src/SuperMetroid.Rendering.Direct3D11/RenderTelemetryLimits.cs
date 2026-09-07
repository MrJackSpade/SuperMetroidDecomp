namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Bounded renderer telemetry storage; allocation occurs before the worker starts.</summary>
public static class RenderTelemetryLimits
{
    /// <summary>Normal desktop rolling history, keeping tooltip sorting small.</summary>
    public const int DefaultHistoryCapacity = 2048;
    /// <summary>Hard limit on samples in each independent timing stream.</summary>
    public const int MaximumHistoryCapacity = 65536;
}
