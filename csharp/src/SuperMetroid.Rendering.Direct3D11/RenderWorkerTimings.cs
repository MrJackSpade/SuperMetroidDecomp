namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Independent windows: GPU results arrive asynchronously; Present includes display submission and host waiting.</summary>
public readonly record struct RenderWorkerTimings(RenderTimingDistribution CpuComposition,
    RenderTimingDistribution CpuDisplayAndPresent, RenderTimingDistribution GpuComposition,
    RenderTimingDistribution GpuCompositionAndDisplay);
