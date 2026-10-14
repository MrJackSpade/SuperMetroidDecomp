namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>GPU duration for one interval and its composition portion, or invalid values after a disjoint clock.</summary>
internal readonly record struct GpuTimingSample(bool Valid, double Milliseconds, double CompositionMilliseconds);
