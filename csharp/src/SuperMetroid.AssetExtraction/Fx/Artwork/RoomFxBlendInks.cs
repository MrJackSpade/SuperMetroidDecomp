using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The installed room-FX blends selected by FX records (#1512): the import pre-colours the effect overlay pictures in
/// each selection's three inks, kept as installed. Only the import reads them (#1544).
/// </summary>
public sealed class RoomFxBlendInks
{
    /// <summary>Each authored selection's three installed inks.</summary>
    private readonly Dictionary<RoomFxPaletteBlend, RgbColor[]> inks;

    /// <summary>Keeps every authored selection's complete installed inks.</summary>
    /// <param name="inks">The three inks of each of the eight authored selections.</param>
    /// <exception cref="InvalidDataException">A selection is missing, or holds other than three inks.</exception>
    internal RoomFxBlendInks(Dictionary<RoomFxPaletteBlend, RgbColor[]> inks)
    {
        foreach (RoomFxPaletteBlend selection in RoomFxPaletteBlendDocumentRules.Ids)
            if (!inks.TryGetValue(selection, out RgbColor[]? colours) || colours.Length != RoomFxPaletteBlendDefinitions.ColorCount)
                throw new InvalidDataException($"Room-FX blend {selection} requires {RoomFxPaletteBlendDefinitions.ColorCount} installed colors.");
        if (inks.Count != RoomFxPaletteBlendDocumentRules.Ids.Count())
            throw new InvalidDataException("Room-FX blends hold only the eight authored selections.");
        this.inks = inks.ToDictionary(pair => pair.Key, pair => (RgbColor[])pair.Value.Clone());
    }

    /// <summary>The inks <paramref name="selection"/> loads: its three installed colours, or the cleared black of no blend.</summary>
    internal IReadOnlyList<RgbColor> Inks(RoomFxPaletteBlend selection) => selection == RoomFxPaletteBlend.None
        ? [RgbColor.Black]
        : inks.TryGetValue(selection, out RgbColor[]? colours)
            ? colours
            : throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection.");
}
