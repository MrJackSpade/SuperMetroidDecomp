using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Explicit CPU pixel access for packet tests, screenshots and software presentation.</summary>
public static class SoftwareFrameSnapshotRenderer
{
    /// <summary>
    /// Renders a packet. An optional caller-owned buffer is reused for ordinary gameplay;
    /// other composition paths may return a different array. Always use the returned
    /// array and finish consuming it before reusing the supplied buffer for another frame.
    /// </summary>
    public static Rgba32[] Render(RenderFrameSnapshot frame, Rgba32[]? gameplayOutputBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        Rgba32[] pixels;
        if (frame.Layers is { } layers) pixels = SoftwareLayeredSnapshotRenderer.Render(layers, gameplayOutputBuffer);
        else if (frame.Mode7 is { } mode7) pixels = SoftwareMode7ObjSnapshotRenderer.Render(mode7);
        else if (frame.SolidColor is { } color)
        {
            pixels = new Rgba32[frame.Width * frame.Height];
            Array.Fill(pixels, color);
        }
        else throw new InvalidDataException("Render packet has no composition payload.");
        foreach (byte level in frame.BrightnessPasses)
            MasterBrightnessFilter.Apply(pixels, level);
        return pixels;
    }
}
