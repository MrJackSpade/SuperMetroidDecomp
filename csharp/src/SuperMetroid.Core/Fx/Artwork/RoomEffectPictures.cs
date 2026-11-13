using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The artwork each room effect shows on the effect overlay, drawn from a room's effect overlay picture sheet by the effect
/// (#1466, #1514, #1536, #1706). The import pictures, in every tileset's sheet, the artwork of each effect a room state of
/// the tileset runs that shows any (<see cref="RoomFxOverlayState.ShowsPage"/>): a liquid's surface strip and body under
/// <see cref="SurfaceName"/> and <see cref="BodyName"/>, an atmosphere's picture under <see cref="AtmosphereName"/>.
/// </summary>
public static class RoomEffectPictures
{
    /// <summary>The name, in a room's effect overlay picture sheet, of the surface strip of liquid effect <paramref name="type"/>.</summary>
    public static string SurfaceName(RoomFxType type) => $"Fx{type}.Surface";

    /// <summary>The name, in a room's effect overlay picture sheet, of the body of liquid effect <paramref name="type"/>.</summary>
    public static string BodyName(RoomFxType type) => $"Fx{type}.Body";

    /// <summary>The name, in a room's effect overlay picture sheet, of the picture of atmosphere effect <paramref name="type"/>.</summary>
    public static string AtmosphereName(RoomFxType type) => $"Fx{type}";

    /// <summary>The names of the pictures effect <paramref name="type"/> shows: a liquid's strip and body, or an atmosphere's picture.</summary>
    public static IEnumerable<string> NamesOf(RoomFxType type) =>
        RoomFxTypes.ShowsLiquid(type) ? [SurfaceName(type), BodyName(type)] : [AtmosphereName(type)];

    /// <summary>
    /// The artwork room effect <paramref name="type"/> shows, from <paramref name="sheet"/>, while the room's appearance
    /// inputs take the values <paramref name="appearance"/> reads, each picture as its part behind and its part in front (#1634).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no artwork of the effect, or none for those values.</exception>
    public static RoomEffectArtwork Artwork(WrittenPictureSheet sheet, RoomFxType type, RoomAppearanceView appearance)
    {
        Ensure.NotNull(sheet);
        return RoomFxTypes.ShowsLiquid(type)
            ? new LiquidArtwork(sheet.Picture(SurfaceName(type), appearance), sheet.Picture(BodyName(type), appearance))
            : new AtmosphereArtwork(sheet.Picture(AtmosphereName(type), appearance));
    }
}
