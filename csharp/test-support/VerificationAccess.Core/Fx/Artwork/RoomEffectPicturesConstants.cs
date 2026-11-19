using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Cartridge values and helpers of <see cref="RoomEffectPictures"/> that only verification reads.</summary>
internal static class RoomEffectPicturesConstants
{
    /// <summary>
    /// The artwork room effect <paramref name="type"/> shows, from <paramref name="sheet"/>, while the room's appearance
    /// inputs take the values <paramref name="appearance"/> reads, each picture as its part behind and its part in front (#1634).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no artwork of the effect, or none for those values.</exception>
    public static RoomEffectArtwork Artwork(WrittenPictureSheet sheet, RoomFxType type, RoomAppearanceView appearance) =>
        RoomFxTypes.ShowsLiquid(type) ? RoomEffectPictures.Liquid(sheet, type, appearance) : RoomEffectPictures.Atmosphere(sheet, type, appearance);
}
