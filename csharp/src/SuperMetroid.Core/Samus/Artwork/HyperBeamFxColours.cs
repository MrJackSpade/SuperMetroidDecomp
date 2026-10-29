using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

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
        Ensure.LengthEqual(frames.AsSpan(), SamusPaletteDefinitions.HyperBeamFx.CycleFrameCount);
        foreach (ImmutableArray<RgbColor> colours in frames)
            Ensure.LengthEqual(colours.AsSpan(), SamusColourDefinitions.HyperBeamColours);
        this.frames = frames;
    }

    /// <summary>Shows a cycle frame's colours in the beam colours the cycle recolours, without advancing the cycle.</summary>
    /// <param name="destination">The colours the cycle shows, eight of them.</param>
    /// <param name="frame">The cycle frame.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside the cycle.</exception>
    public void Apply(ColourRun destination, int frame)
    {
        if ((uint)frame >= (uint)frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        Ensure.LengthEqual(destination.Colors, SamusColourDefinitions.HyperBeamColours);
        ImmutableArray<RgbColor> colours = frames[frame];
        for (int color = 0; color < colours.Length; color++)
            destination.Write(color, colours[color]);
    }
}
