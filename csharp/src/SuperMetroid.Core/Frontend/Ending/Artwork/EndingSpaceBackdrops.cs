using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The backdrop each palette-effect record of the space behind the exploding planet shows (#1512, #1583), as the import
/// supplies it: every record of those colours writes its first colour, the space, before any colour it skips, so each
/// record shows that colour behind every layer. The ending's other palette-effect colours are pre-coloured into its sheets.
/// </summary>
public sealed class EndingSpaceBackdrops
{
    /// <summary>The backdrop each record of the space's colours shows.</summary>
    private readonly FrozenDictionary<PaletteFxFrame, RgbColor> backdrops;

    /// <summary>Keeps the backdrop each record of the space's colours shows.</summary>
    /// <param name="backdrops">The backdrop of each record, by the record.</param>
    public EndingSpaceBackdrops(IReadOnlyDictionary<PaletteFxFrame, RgbColor> backdrops)
    {
        this.backdrops = Ensure.NotNull(backdrops).ToFrozenDictionary();
    }

    /// <summary>The backdrop <paramref name="record"/> of the space's colours shows.</summary>
    /// <exception cref="InvalidDataException">The record's backdrop is not installed.</exception>
    public RgbColor Of(PaletteFxFrame record) => backdrops.TryGetValue(record, out RgbColor backdrop)
        ? backdrop
        : throw new InvalidDataException($"Palette-FX record {record.Record} of the space behind the exploding planet has no installed backdrop.");
}
