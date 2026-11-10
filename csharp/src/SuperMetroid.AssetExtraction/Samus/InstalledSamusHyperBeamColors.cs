using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed ten-frame full-body Hyper Beam colour cycle, every frame's colours kept as installed.</summary>
public sealed class InstalledSamusHyperBeamColors
{
    /// <summary>Every installed cycle frame's sixteen colours, in cycle order.</summary>
    private readonly RgbColor[][] frames;

    /// <summary>Keeps copies of the complete installed cycle.</summary>
    /// <param name="frames">All <see cref="SamusHyperBeamColorFormat.FrameCount"/> frames of <see cref="SamusHyperBeamColorFormat.ColorsPerFrame"/> colours, in cycle order.</param>
    /// <exception cref="InvalidDataException">The cycle is missing a frame or a colour.</exception>
    internal InstalledSamusHyperBeamColors(RgbColor[][] frames)
    {
        if (!SamusHyperBeamColorFormat.Shape.Fits(frames))
            throw new InvalidDataException("Samus Hyper Beam colors require ten frames of sixteen colors.");
        this.frames = [.. frames.Select(frame => (RgbColor[])frame.Clone())];
    }

    /// <summary>
    /// The installed colours keyed by what they show (#1464): every cycle frame's suit colours, in cycle order. The
    /// document's frame-by-colour layout ends here.
    /// </summary>
    public SamusHyperBeamColours ToColours() =>
        new([.. frames.Select(frame => frame.ToImmutableArray())]);

    /// <summary>Returns one installed colour; the frame clock remains gameplay's.</summary>
    /// <param name="frame">The cycle frame, 0 through 9.</param>
    /// <param name="color">The colour within the frame, 0 through 15.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the cycle.</exception>
    public RgbColor Resolve(int frame, int color)
    {
        if ((uint)frame >= frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= frames[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
        return frames[frame][color];
    }
}
