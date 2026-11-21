using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The artwork each room effect shows on the effect overlay, drawn from a room's effect overlay picture sheet by the effect
/// (#1466, #1514, #1536, #1706). The import pictures, in every tileset's sheet, the artwork of each effect a room state of
/// the tileset runs that shows any (<see cref="RoomFxOverlayState.ShowsOnOverlay"/>): a liquid's surface strip and body under
/// <see cref="SurfaceName"/> and <see cref="BodyName"/>, an atmosphere's picture under <see cref="AtmosphereName"/>.
/// </summary>
public static partial class RoomEffectPictures
{
    /// <summary>The name, in a room's effect overlay picture sheet, of the surface strip of liquid effect <paramref name="type"/>.</summary>
    [InstalledKey]
    public static string SurfaceName(RoomFxType type) => $"Fx{RoomFxTypeKeys.Installed.Of(type)}.Surface";

    /// <summary>The name, in a room's effect overlay picture sheet, of the body of liquid effect <paramref name="type"/>.</summary>
    [InstalledKey]
    public static string BodyName(RoomFxType type) => $"Fx{RoomFxTypeKeys.Installed.Of(type)}.Body";

    /// <summary>The name, in a room's effect overlay picture sheet, of the picture of atmosphere effect <paramref name="type"/>.</summary>
    [InstalledKey]
    public static string AtmosphereName(RoomFxType type) => $"Fx{RoomFxTypeKeys.Installed.Of(type)}";

    /// <summary>The names of the pictures effect <paramref name="type"/> shows: a liquid's strip and body, or an atmosphere's picture.</summary>
    public static IEnumerable<string> NamesOf(RoomFxType type) =>
        RoomFxTypes.ShowsLiquid(type) ? [SurfaceName(type), BodyName(type)] : [AtmosphereName(type)];

    /// <summary>
    /// The surface strip and body liquid effect <paramref name="type"/> shows, from <paramref name="sheet"/>, while the
    /// room's appearance inputs take the values <paramref name="appearance"/> reads (#2505).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no surface strip or body of the effect, or none for those values.</exception>
    public static LiquidArtwork Liquid(WrittenPictureSheet sheet, RoomFxType type, RoomAppearanceView appearance)
    {
        Ensure.NotNull(sheet);
        return new LiquidArtwork(sheet.Picture(SurfaceName(type), appearance), sheet.Picture(BodyName(type), appearance));
    }

    /// <summary>
    /// The repeating picture atmosphere effect <paramref name="type"/> shows, from <paramref name="sheet"/>, while the
    /// room's appearance inputs take the values <paramref name="appearance"/> reads (#2505).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no picture of the effect, or none for those values.</exception>
    public static AtmosphereArtwork Atmosphere(WrittenPictureSheet sheet, RoomFxType type, RoomAppearanceView appearance)
    {
        Ensure.NotNull(sheet);
        return new AtmosphereArtwork(sheet.Picture(AtmosphereName(type), appearance));
    }
}
