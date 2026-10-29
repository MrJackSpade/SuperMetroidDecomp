using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The Hyper Beam's full-body colour cycle (#1464): for each of its frames, the colours Samus's suit shows. The import
/// pipeline decodes them from an installed document; the timer that steps through the frames is Samus's.
/// </summary>
public sealed class SamusHyperBeamColours
{
    /// <summary>Each cycle frame's suit colours, in cycle order.</summary>
    private readonly ImmutableArray<ImmutableArray<RgbColor>> frames;

    /// <summary>Keeps the decoded frames.</summary>
    /// <param name="frames">Every cycle frame's suit colours, in cycle order.</param>
    /// <exception cref="ArgumentException">The cycle does not hold every frame, or a frame not a whole suit's colours.</exception>
    internal SamusHyperBeamColours(ImmutableArray<ImmutableArray<RgbColor>> frames)
    {
        Ensure.LengthEqual(frames.AsSpan(), SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount);
        foreach (ImmutableArray<RgbColor> colours in frames)
            Ensure.LengthEqual(colours.AsSpan(), SamusColourDefinitions.RowColours);
        this.frames = frames;
    }

    /// <summary>One colour of Samus's suit in a cycle frame.</summary>
    /// <param name="frame">The cycle frame.</param>
    /// <param name="color">The colour within Samus's suit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the cycle.</exception>
    public RgbColor Resolve(int frame, int color)
    {
        ImmutableArray<RgbColor> colours = Frame(frame);
        if ((uint)color >= (uint)colours.Length) throw new ArgumentOutOfRangeException(nameof(color));
        return colours[color];
    }

    /// <summary>Shows a cycle frame's colours as Samus's suit colours.</summary>
    /// <param name="colours">Samus's colours, whose suit colours change.</param>
    /// <param name="frame">The cycle frame.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside the cycle.</exception>
    public void Apply(SamusColours colours, int frame)
    {
        Ensure.NotNull(colours);
        for (int colour = 0; colour < SamusColourDefinitions.RowColours; colour++)
            colours.Suit.Write(colour, Resolve(frame, colour));
    }

    /// <summary>The suit colours of cycle frame <paramref name="frame"/>.</summary>
    private ImmutableArray<RgbColor> Frame(int frame) =>
        (uint)frame < (uint)frames.Length ? frames[frame] : throw new ArgumentOutOfRangeException(nameof(frame));
}
