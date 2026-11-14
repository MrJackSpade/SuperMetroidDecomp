using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The backdrop each colour-effect frame of the space behind the exploding planet shows (#1512, #1583, #1598), as the
/// import supplies it: every frame of those colours writes its first colour, the space, before any colour it skips, so each
/// frame shows that colour behind every layer. The ending's other colour-effect colours are pre-coloured into its sheets.
/// </summary>
public sealed class EndingSpaceBackdrops
{
    /// <summary>The backdrop each frame of the space's colours shows.</summary>
    private readonly FrozenDictionary<ColourEffectFrame, RgbColor> backdrops;

    /// <summary>Keeps the backdrop each frame of the space's colours shows.</summary>
    /// <param name="backdrops">The backdrop of each frame, by the frame.</param>
    public EndingSpaceBackdrops(IReadOnlyDictionary<ColourEffectFrame, RgbColor> backdrops)
    {
        this.backdrops = Ensure.NotNull(backdrops).ToFrozenDictionary();
    }

    /// <summary>The backdrop <paramref name="frame"/> of the space's colours shows.</summary>
    /// <exception cref="InvalidDataException">The frame's backdrop is not installed.</exception>
    public RgbColor Of(ColourEffectFrame frame) => backdrops.TryGetValue(frame, out RgbColor backdrop)
        ? backdrop
        : throw new InvalidDataException($"Colour-effect frame {frame} of the space behind the exploding planet has no installed backdrop.");
}
