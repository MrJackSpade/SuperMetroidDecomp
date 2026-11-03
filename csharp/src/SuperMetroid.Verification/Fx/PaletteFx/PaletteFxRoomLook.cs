using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;

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
    internal static void LayRoomShades(RoomPaletteFxSystem fx, IPaletteColors cgram, IPaletteFxColorSource colours)
    {
        ArgumentNullException.ThrowIfNull(fx);
        ArgumentNullException.ThrowIfNull(cgram);
        List<SheetShade> made = [];
        foreach (PaletteFxColours group in PaletteFxRoomColourDefinitions.RoomColours)
            if (fx.RecordOf(group) is { } record)
                made.Add(PaletteFxRoomShades.Shade(group, record, colours));
        PictureShading shading = GameplayShadingProbe.ShadingOf(fx, GameplayPictures.Room, made.Select(shade => shade.Shade));
        foreach (PictureShade shade in shading.Shades)
        {
            SheetShade laid = made.Single(candidate => candidate.Shade == shade);
            for (int index = 0; index < laid.Inks.Length; index++)
                cgram.SetColor(laid.Inks[index], laid.Colours[index]);
        }
    }
}
