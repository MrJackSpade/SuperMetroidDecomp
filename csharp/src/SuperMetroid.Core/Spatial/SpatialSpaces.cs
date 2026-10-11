namespace SuperMetroid.Core.Spatial;

/// <summary>A coordinate space that brands a spatial value; values in different spaces never mix implicitly.</summary>
public interface ISpatialSpace;

/// <summary>Room-relative pixels: Samus, enemies, PLM blocks and the camera's top-left corner (BG1 scroll).</summary>
public sealed class RoomPixels : ISpatialSpace;

/// <summary>Viewport-relative pixels: a room position minus the camera position.</summary>
public sealed class ScreenPixels : ISpatialSpace;

/// <summary>BG2 scroll-register pixels, which follow their own parallax rules rather than the camera.</summary>
public sealed class Bg2Pixels : ISpatialSpace;

/// <summary>BG3 scroll-register pixels (room FX layer, HUD and message boxes).</summary>
public sealed class Bg3Pixels : ISpatialSpace;

/// <summary>Room block grid: sixteen-pixel collision/level-data cells.</summary>
public sealed class RoomBlocks : ISpatialSpace;

/// <summary>Room screen grid: 256-pixel scroll-region cells.</summary>
public sealed class RoomScreens : ISpatialSpace;

/// <summary>Area-map tile grid used by the pause, file-select and minimap views.</summary>
public sealed class MapTiles : ISpatialSpace;

/// <summary>
/// Area-map plane pixels (eight per map tile): the pause and file-select map scroll offsets,
/// scroll bounds and the marker/arrow positions drawn over the map.
/// </summary>
public sealed class MapPixels : ISpatialSpace;

/// <summary>OAM sprite coordinates: nine-bit signed X, eight-bit Y, relative to the screen.</summary>
public sealed class OamPixels : ISpatialSpace;

/// <summary>Mode 7 plane coordinates and the matrix centre.</summary>
public sealed class Mode7Pixels : ISpatialSpace;

/// <summary>VRAM tilemap cells (eight-pixel tiles within a 32x32 or 64x32 tilemap).</summary>
public sealed class TilemapCells : ISpatialSpace;

/// <summary>
/// Host raster pixels: columns and rows of a decoded image, artwork atlas, tile bitmap or rendered
/// frame, measured from the image's own top-left corner rather than any screen or room.
/// </summary>
public sealed class ImagePixels : ISpatialSpace;

/// <summary>
/// Normalized analog-stick deflection of a host gamepad: signed sixteen-bit axes, X positive to the
/// right and Y positive downwards, centred on zero.
/// </summary>
public sealed class GamepadStick : ISpatialSpace;

/// <summary>
/// Marks a spatial value type, or a narrow storage/hardware/serialization adapter, that may declare
/// paired X/Y, width/height or horizontal/vertical scalar components (#628).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor,
    Inherited = false)]
public sealed class SpatialComponentsAttribute : Attribute
{
    /// <summary>Records why the components are stored or passed separately.</summary>
    public SpatialComponentsAttribute(string reason) => Reason = reason;

    /// <summary>Why this declaration is a component boundary rather than a spatial data clump.</summary>
    public string Reason { get; }
}
