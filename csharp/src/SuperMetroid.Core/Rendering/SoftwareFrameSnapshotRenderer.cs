using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Explicit CPU pixel access for packet tests, screenshots and software presentation.</summary>
public static class SoftwareFrameSnapshotRenderer
{
    public static Rgba32[] Render(RenderFrameSnapshot frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        Rgba32[] pixels;
        if (frame.Layers is { } layers) pixels = SoftwareLayeredSnapshotRenderer.Render(layers);
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
