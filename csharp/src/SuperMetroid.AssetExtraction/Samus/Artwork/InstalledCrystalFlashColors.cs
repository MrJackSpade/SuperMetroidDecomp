using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The installed Crystal Flash body and bubble colours, every frame of both kept as installed. Timers, frame cursors,
/// and completion still belong to <see cref="SamusCrystalFlashState"/>.
/// </summary>
public sealed class InstalledCrystalFlashColors
{
    /// <summary>Every installed body frame's colours, in playback order.</summary>
    private readonly RgbColor[][] body;
    /// <summary>Every installed bubble frame's colours, in playback order.</summary>
    private readonly RgbColor[][] bubble;

    /// <summary>Keeps the complete installed body and bubble frames.</summary>
    /// <param name="body">All <see cref="CrystalFlashColorFormat.BodyFrameCount"/> body frames of <see cref="CrystalFlashColorFormat.BodyColorCount"/> colours.</param>
    /// <param name="bubble">All <see cref="CrystalFlashColorFormat.BubbleFrameCount"/> bubble frames of <see cref="CrystalFlashColorFormat.BubbleColorCount"/> colours.</param>
    /// <exception cref="InvalidDataException">Either set is missing a frame or a colour.</exception>
    internal InstalledCrystalFlashColors(RgbColor[][] body, RgbColor[][] bubble)
    {
        this.body = Complete(body, CrystalFlashColorFormat.BodyFrameCount, CrystalFlashColorFormat.BodyColorCount, "body");
        this.bubble = Complete(bubble, CrystalFlashColorFormat.BubbleFrameCount, CrystalFlashColorFormat.BubbleColorCount, "bubble");
    }

    /// <summary>Returns one installed body colour.</summary>
    /// <param name="frame">Zero-based body record index, 0 through 9 in playback order.</param>
    /// <param name="color">Zero-based color within the ten-color body portion, including transparent color zero.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the body dimensions.</exception>
    public RgbColor ResolveBody(int frame, int color) => Resolve(body, frame, color);

    /// <summary>Returns one installed bubble colour.</summary>
    /// <param name="frame">Zero-based bubble palette index, 0 through 5.</param>
    /// <param name="color">Zero-based color within the six-color bubble portion, 0 through 5.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the bubble dimensions.</exception>
    public RgbColor ResolveBubble(int frame, int color) => Resolve(bubble, frame, color);

    /// <summary>The colour at <paramref name="frame"/> and <paramref name="color"/> of <paramref name="frames"/>.</summary>
    private static RgbColor Resolve(RgbColor[][] frames, int frame, int color)
    {
        if ((uint)frame >= frames.Length)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= frames[frame].Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames[frame][color];
    }

    /// <summary>Copies <paramref name="frames"/>, rejecting a set without every frame and colour.</summary>
    private static RgbColor[][] Complete(RgbColor[][] frames, int frameCount, int colorCount, string name)
    {
        if (frames.Length != frameCount)
            throw new InvalidDataException($"Crystal Flash {name} requires {frameCount} frames.");
        var copy = new RgbColor[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            if (frames[frame].Length != colorCount)
                throw new InvalidDataException($"Crystal Flash {name} frame {frame} requires {colorCount} colors.");
            copy[frame] = (RgbColor[])frames[frame].Clone();
        }
        return copy;
    }
}
