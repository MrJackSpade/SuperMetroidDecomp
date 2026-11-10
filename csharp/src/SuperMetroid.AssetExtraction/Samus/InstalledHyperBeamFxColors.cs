using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The Hyper Beam projectile's colour cycle, as installed (#1596): each of its ten frames' eight colours, which the
/// handler copies into the beams' colours 1..8.
/// </summary>
public sealed class InstalledHyperBeamFxColors
{
    /// <summary>Each cycle frame's eight installed colours, in cycle order.</summary>
    private readonly ImmutableArray<ImmutableArray<RgbColor>> frames;

    /// <summary>Copies every cycle frame's installed colours.</summary>
    /// <param name="frames">Ten frames of eight colours each, in cycle order.</param>
    /// <exception cref="ArgumentException">A frame is missing or does not hold eight colours.</exception>
    internal InstalledHyperBeamFxColors(RgbColor[][] frames)
    {
        Ensure.NotNull(frames);
        if (frames.Length != HyperBeamFxColorFormat.FrameCount ||
            frames.Any(colours => colours is null || colours.Length != HyperBeamFxColorFormat.ColorsPerFrame))
            throw new ArgumentException($"The Hyper Beam cycle needs {HyperBeamFxColorFormat.ColorsPerFrame} colours in each of {HyperBeamFxColorFormat.FrameCount} frames.", nameof(frames));
        this.frames = [.. frames.Select(colours => colours.ToImmutableArray())];
    }

    /// <summary>
    /// The decoded colours keyed by what they show (#1464): each cycle frame's eight beam colours, in cycle order. The
    /// document's frame-by-colour layout ends here.
    /// </summary>
    public HyperBeamFxColours ToColours() => new(frames);
}
