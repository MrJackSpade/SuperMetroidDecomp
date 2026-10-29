namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Stable installed filenames for Kraid's private BG2 source maps.</summary>
public static class KraidBackgroundArtworkFormat
{
    /// <summary>Installation-relative JSON filename for the upper-body BG2 source, not the engine's composed live tilemap.</summary>
    public const string UpperFileName = "kraid-upper-bg2.json";
    /// <summary>Installation-relative JSON filename for the lower-body BG2 source, including the tail retained by the working-map composition.</summary>
    public const string LowerFileName = "kraid-lower-bg2.json";
    /// <summary>Installation-relative indexed PNG filename for $A7:A716, Tiles_KraidRoomBackground: sixteen 4-bpp characters uploaded to VRAM word $3F00 after growth or defeat.</summary>
    public const string RoomBackgroundFileName = "kraid-room-background.png";

    /// <summary>Installation-relative JSON filename of the 32-by-11 head tilemap shown at <paramref name="mouthStage"/>, as <see cref="Core.Bosses.Kraid.KraidHeadFrame.MouthStage"/> counts it.</summary>
    public static string HeadFileName(int mouthStage) => $"kraid-head-{mouthStage}.json";

}
