using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The import's room-FX blend colours, selected by FX records (#1512): the import pre-colours the effect overlay pictures
/// in each selection's three inks. Only the import reads them (#1544).
/// </summary>
public sealed partial class RoomFxBlendInks
{
    /// <summary>Each authored selection's three inks.</summary>
    private readonly Dictionary<RoomEffectTint, RgbColor[]> inks;

    /// <summary>Keeps every authored selection's complete inks, as the import reads them.</summary>
    /// <param name="inks">The three inks of each of the eight authored selections.</param>
    /// <exception cref="InvalidDataException">A selection is missing, or holds other than three inks.</exception>
    internal RoomFxBlendInks(Dictionary<RoomEffectTint, RgbColor[]> inks)
    {
        foreach (RoomEffectTint selection in RoomFxPaletteBlendDocumentRules.Ids)
            if (!inks.TryGetValue(selection, out RgbColor[]? colours) || colours.Length != RoomFxPaletteBlendDefinitions.ColorCount)
                throw new InvalidDataException($"Room-FX blend {selection} requires {RoomFxPaletteBlendDefinitions.ColorCount} inks.");
        if (inks.Count != RoomFxPaletteBlendDocumentRules.Ids.Count())
            throw new InvalidDataException("Room-FX blends hold only the eight authored selections.");
        this.inks = inks.ToDictionary(pair => pair.Key, pair => (RgbColor[])pair.Value.Clone());
    }

    /// <summary>The inks <paramref name="selection"/> loads: its three colours, or the cleared black of no blend.</summary>
    internal IReadOnlyList<RgbColor> Inks(RoomEffectTint selection) => selection == RoomEffectTint.None
        ? [RgbColor.Black]
        : inks.TryGetValue(selection, out RgbColor[]? colours)
            ? colours
            : throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection.");
}
