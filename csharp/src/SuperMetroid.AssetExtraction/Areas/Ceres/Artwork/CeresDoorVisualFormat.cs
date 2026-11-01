using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>Stable host filenames for Ceres-door graphics and palettes, and the size of the door's installed character sheet.</summary>
public static class CeresDoorVisualFormat
{
    /// <summary>Installed indexed PNG filename for the 32 4bpp door characters used by variant two's OBJ tile upload.</summary>
    public const string TilesFileName = "ceres-door-tiles.png";
    /// <summary>Installed editable JSON filename for normal/escape setup colors, animation inks, and the two Mode 7 platform strips.</summary>
    public const string ColorsFileName = "ceres-door-colors.json";

    /// <summary>The door's characters, which its second variant shows over the sprite characters.</summary>
    public const int CharacterCount = 32;

    /// <summary>The bytes of the door's four-bit characters.</summary>
    public const int CharacterBytes = CharacterCount * RoomCharacterAtlasFormat.BytesPerTile;
}
