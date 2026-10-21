namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>The edges of a window.</summary>
/// <param name="Left">The left.</param>
/// <param name="Top">The top.</param>
/// <param name="Right">The right.</param>
/// <param name="Bottom">The bottom.</param>
internal readonly record struct WindowEdges(int Left, int Top, int Right, int Bottom);
