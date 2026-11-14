using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// The room's palette-FX colours as the room's pictures show them (#1512): the system lays the shade of each room group's
/// record over the room's pictures, and the import pre-colours each record's shade. The checks that compare a room
/// group's records with the cartridge's colour writes lay those shades into their colour table in the order the system
/// lays them.
/// </summary>
internal static class PaletteFxRoomLook
{
    /// <summary>
    /// Lays the shades of the room records <paramref name="fx"/> shows over <paramref name="cgram"/>'s room colours, each
    /// coloured from <paramref name="colours"/> as the import colours it.
    /// </summary>
    internal static void LayRoomShades(ColourEffectSystem fx, IPaletteColors cgram, IPaletteFxColorSource colours)
    {
        ArgumentNullException.ThrowIfNull(fx);
        ArgumentNullException.ThrowIfNull(cgram);
        List<SheetShade> made = [];
        foreach (ColourEffectColours group in ColourEffectRoomColourDefinitions.RoomColours)
            if (fx.FrameOf(group) is { } record)
                made.Add(PaletteFxRoomShades.Shade(group, record, colours));
        PictureShading shading = GameplayShadingProbe.ShadingOf(fx, GameplayPictures.Room, made.Select(shade => shade.Shade));
        foreach (PictureShade shade in shading.Shades)
        {
            SheetShade laid = made.Single(candidate => candidate.Shade == shade);
            for (int index = 0; index < laid.Inks.Length; index++)
                cgram.SetColor(laid.Inks[index], laid.Colours[index]);
        }
    }
    /// <summary>
    /// Lays every record <paramref name="fx"/> shows over <paramref name="cgram"/> as the pictures that show it are
    /// pre-coloured (#1512): the room groups' shades over the room's colours, as <see cref="LayRoomShades"/> lays them, then
    /// each other group's last record from the native colour its records start at, which Samus, an enemy or a frontend
    /// screen shows. Both a native and an installed system are laid this way, so their colour tables differ exactly
    /// where the records shown or the colours they are coloured from differ.
    /// </summary>
    internal static void LayShownShades(ColourEffectSystem fx, IPaletteColors cgram, IPaletteFxColorSource colours)
    {
        LayRoomShades(fx, cgram, colours);
        foreach (ColourEffectColours group in Enum.GetValues<ColourEffectColours>())
        {
            if (ColourEffectRoomColourDefinitions.RoomColours.Contains(group) || fx.FrameOf(group) is not { } record)
                continue;
            LayShade(cgram, group, record, colours);
        }
    }

    /// <summary>
    /// Lays <paramref name="record"/>, shown in <paramref name="group"/>, over <paramref name="cgram"/> as the pictures that
    /// show the group are pre-coloured from <paramref name="colours"/>: a room group's shade over the room's colours, any
    /// other group's from the native colour its records start at.
    /// </summary>
    internal static void LayRecord(IPaletteColors cgram, ColourEffectColours group, ColourEffectFrame record, IPaletteFxColorSource colours)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (!ColourEffectRoomColourDefinitions.RoomColours.Contains(group))
        {
            LayShade(cgram, group, record, colours);
            return;
        }
        SheetShade shade = PaletteFxRoomShades.Shade(group, record, colours);
        for (int index = 0; index < shade.Inks.Length; index++)
            cgram.SetColor(shade.Inks[index], shade.Colours[index]);
    }

    /// <summary>
    /// Lays <paramref name="record"/>, shown in <paramref name="group"/>, over <paramref name="cgram"/> from the native colour
    /// its records start at, coloured from <paramref name="colours"/> as the import colours the pictures that show it.
    /// </summary>
    internal static void LayShade(IPaletteColors cgram, ColourEffectColours group, ColourEffectFrame record, IPaletteFxColorSource colours)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        int first = group.CartridgeColour;
        SheetShade shade = PaletteFxRecordShades.Shade(group, record, new InkRange(first, (first / RowColours + 1) * RowColours), colours);
        for (int index = 0; index < shade.Inks.Length; index++)
            cgram.SetColor(shade.Inks[index], shade.Colours[index]);
    }

    /// <summary>Colours in one colour row.</summary>
    private const int RowColours = 16;
}
