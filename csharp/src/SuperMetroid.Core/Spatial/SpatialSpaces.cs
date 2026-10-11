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

/// <summary>OAM sprite coordinates: nine-bit signed X, eight-bit Y, relative to the screen.</summary>
public sealed class OamPixels : ISpatialSpace;

/// <summary>Mode 7 plane coordinates and the matrix centre.</summary>
public sealed class Mode7Pixels : ISpatialSpace;

/// <summary>VRAM tilemap cells (eight-pixel tiles within a 32x32 or 64x32 tilemap).</summary>
public sealed class TilemapCells : ISpatialSpace;

/// <summary>
/// Sixteen-pixel blocks of the circular BG1/BG2 VRAM tilemap: a PPU scroll register shifted
/// right four times, as the level and background streamers address it.
/// </summary>
public sealed class TilemapBlocks : ISpatialSpace;

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
