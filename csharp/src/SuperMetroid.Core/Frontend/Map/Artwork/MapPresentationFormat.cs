using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Presentation schema geometry and its compiled SNES output encoding.</summary>
public static class MapPresentationFormat
{
    /// <summary>Supported per-area map JSON schema revision, checked before compiling the cells.</summary>
    public const int Version = 1;
    /// <summary>Number of 8-pixel character columns in the shared map atlas; atlas row times 32 plus column yields the native character index.</summary>
    public const int AtlasColumns = 32;
    /// <summary>Number of 8-pixel character rows in the shared 64-pixel-high map atlas, bounding editable tile-row values.</summary>
    public const int AtlasRows = MapTileAtlasFormat.Height / 8;
    /// <summary>Eight selectable SNES BG palettes representable by the tilemap word's three palette bits.</summary>
    public const int PaletteCount = 8;
    /// <summary>Bit position 10 of the three-bit palette field, immediately above the ten-bit SNES BG character index.</summary>
    public const int PaletteShift = 10;
    /// <summary>$2000, SNES BG tile priority mask at bit 13, used when compiling an edited cell's priority attribute.</summary>
    public const int PriorityBit = 0x2000;
    /// <summary>$4000, SNES BG character horizontal-mirror mask at bit 14, preserved when exporting or compiling map cells.</summary>
    public const int FlipXBit = 0x4000;
    /// <summary>$8000, SNES BG character vertical-mirror mask at bit 15, preserved when exporting or compiling map cells.</summary>
    public const int FlipYBit = 0x8000;
    /// <summary>Emits stable camel-case map resources and rejects unknown authoring fields.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
}
