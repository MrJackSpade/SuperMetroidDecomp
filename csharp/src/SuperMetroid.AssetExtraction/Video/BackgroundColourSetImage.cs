using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Gameplay's background sets in the cartridge's colour order (#1464): the one place the import pipeline and the native
/// boundary turn the cartridge's 128 background colours into sets and back, and name the set each colour group of an
/// installed gameplay background image draws in.
/// </summary>
public static class BackgroundColourSetImage
{
    /// <summary>The number of colours the background sets hold together in the cartridge's order.</summary>
    public const int ColourCount = ColourSets.SetCount * ColourSets.SetColours;

    /// <summary>
    /// The set each colour group of a gameplay background image draws in, group after group: an image installed from
    /// the cartridge's background colours keeps their order, so its group of each order draws in the set of that order.
    /// </summary>
    public static ImmutableArray<BackgroundColourSet> ImageGroups { get; } = [.. BackgroundColourSetPositionDefinitions.InCartridgeOrder];

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

    /// <summary>The position among the cartridge's background colours of <paramref name="set"/>'s first colour.</summary>
    public static int First(BackgroundColourSet set)
    {
        int position = Array.IndexOf(BackgroundColourSetPositionDefinitions.InCartridgeOrder, set);
        return position >= 0
            ? position * ColourSets.SetColours
            : throw new ArgumentOutOfRangeException(nameof(set), set, "Undefined background colour set.");
    }

    /// <summary>Gives a frontend screen's background sets gameplay's colours, each screen set the colours of the gameplay set of its order.</summary>
    public static void CopyTo(ShownBackgroundColourSets sets, ScreenPalette screen)
    {
        Ensure.NotNull(screen);
        screen.BackgroundSets.All.CopyFrom(Encode(sets));
    }

    /// <summary>Loads gameplay's starting colours into a frontend screen's background and sprite sets, as an import composes over them.</summary>
    public static void LoadStarting(GameplayBasePalette palettes, ScreenPalette screen)
    {
        Ensure.NotNull(palettes);
        Ensure.NotNull(screen);
        CopyTo(palettes.starting.Backgrounds, screen);
        palettes.starting.Sprites.CopyTo(screen.SpriteSets);
    }
}
