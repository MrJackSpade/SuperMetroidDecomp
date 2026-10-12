namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Cumulative bytes submitted and CPU call time; not asynchronous GPU transfer duration.</summary>
public readonly record struct RenderUploadStatistics(long Bytes, long Calls, double CpuMilliseconds);
