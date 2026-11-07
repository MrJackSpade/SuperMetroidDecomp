using SuperMetroid.Core;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The Hyper Beam's own colour cycle (#1464): for each of its frames, the eight beam colours it shows. The import pipeline
/// decodes them from an installed document; the palette effect that steps through the frames is the beam's.
/// </summary>
public sealed class HyperBeamFxColours
{
    /// <summary>Each cycle frame's beam colours, in cycle order.</summary>
    private readonly ImmutableArray<ImmutableArray<RgbColor>> frames;

    /// <summary>Keeps the decoded frames.</summary>
    /// <param name="frames">Every cycle frame's beam colours, in cycle order.</param>
    /// <exception cref="ArgumentException">The cycle does not hold every frame, or a frame not every beam colour it shows.</exception>
    internal HyperBeamFxColours(ImmutableArray<ImmutableArray<RgbColor>> frames)
    {
        Ensure.LengthEqual(frames.AsSpan(), SamusColourSets.HyperBeamCycleFrames);
        foreach (ImmutableArray<RgbColor> colours in frames)
            Ensure.LengthEqual(colours.AsSpan(), SamusColourSets.HyperBeamColours);
        this.frames = frames;
    }

    /// <summary>The beam colours a cycle frame shows, in the order the cycle recolours them (#1512).</summary>
    /// <param name="frame">The cycle frame.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside the cycle.</exception>
    public ImmutableArray<RgbColor> Frame(int frame) =>
        (uint)frame < (uint)frames.Length ? frames[frame] : throw new ArgumentOutOfRangeException(nameof(frame));
}
