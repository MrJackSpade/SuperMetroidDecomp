using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Gameplay's background sets in the cartridge's colour order (#1464): the one place the import pipeline and the native
/// boundary turn the cartridge's 128 background colours into sets and back.
/// </summary>
public static class BackgroundColourSetImage
{
    /// <summary>The number of colours the background sets hold together in the cartridge's order.</summary>
    public const int ColourCount = ColourSets.SetCount * ColourSets.SetColours;

    /// <summary>The sets <paramref name="colours"/>, the cartridge's 128 background colours in its order, fill.</summary>
    /// <exception cref="ArgumentException"><paramref name="colours"/> does not hold 128 colours.</exception>
    public static BackgroundColourSets Decode(ReadOnlySpan<RgbColor> colours)
    {
        Ensure.LengthEqual(colours, ColourCount);
        var sets = new BackgroundColourSets();
        for (int position = 0; position < BackgroundColourSetPositionDefinitions.InCartridgeOrder.Length; position++)
        {
            sets.Set(BackgroundColourSetPositionDefinitions.InCartridgeOrder[position])
                .CopyFrom(colours.Slice(position * ColourSets.SetColours, ColourSets.SetColours));
        }
        return sets;
    }

    /// <summary>The cartridge's 128 background colours, in its order, that <paramref name="sets"/> hold.</summary>
    public static RgbColor[] Encode(ShownBackgroundColourSets sets)
    {
        Ensure.NotNull(sets);
        var colours = new RgbColor[ColourCount];
        for (int position = 0; position < BackgroundColourSetPositionDefinitions.InCartridgeOrder.Length; position++)
            sets.Set(BackgroundColourSetPositionDefinitions.InCartridgeOrder[position]).CopyTo(colours.AsSpan(position * ColourSets.SetColours));
        return colours;
    }
}
