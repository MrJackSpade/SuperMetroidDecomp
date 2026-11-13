namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Stable filenames of Kraid's body and head layouts and room background. All are import data (#1636, #1638): only the
/// import composes Kraid's body sheet from them, and the installation holds the pictures it composes.
/// </summary>
public static class KraidBackgroundArtworkFormat
{
    /// <summary>Import-data JSON filename for the upper-body BG2 source, not the engine's composed live tilemap.</summary>
    public const string UpperFileName = "kraid-upper-bg2.json";
    /// <summary>Import-data JSON filename for the lower-body BG2 source, including the tail retained by the working-map composition.</summary>
    public const string LowerFileName = "kraid-lower-bg2.json";
    /// <summary>Import-data indexed PNG filename for $A7:A716, Tiles_KraidRoomBackground: sixteen 4-bpp characters uploaded to VRAM word $3F00 after growth or defeat.</summary>
    public const string RoomBackgroundFileName = "kraid-room-background.png";

    /// <summary>Import-data JSON filename of the 32-by-11 head tilemap shown at <paramref name="mouthStage"/>, as <see cref="Core.Bosses.Kraid.KraidHeadFrame.MouthStage"/> counts it.</summary>
    public static string HeadFileName(int mouthStage) => $"kraid-head-{mouthStage}.json";

}
