namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Stable installed filenames for Kraid's private BG2 source maps.</summary>
public static class KraidBackgroundArtworkFormat
{
    /// <summary>Installation-relative JSON filename for the upper-body BG2 source, not the engine's composed live tilemap.</summary>
    public const string UpperFileName = "kraid-upper-bg2.json";
    /// <summary>Installation-relative JSON filename for the lower-body BG2 source, including the tail retained by the working-map composition.</summary>
    public const string LowerFileName = "kraid-lower-bg2.json";
    /// <summary>Installation-relative indexed PNG filename for $A7:A716, Tiles_KraidRoomBackground: sixteen 4-bpp characters uploaded to VRAM word $3F00 after growth or defeat.</summary>
    public const string RoomBackgroundFileName = "kraid-room-background.png";

    /// <summary>Formats the installation-relative JSON filename for one 32-by-11 head tilemap, using its bank-$A7 source pointer as an uppercase four-digit hexadecimal identity.</summary>
    /// <param name="sourcePointer">Native head-art pointer selected by Kraid's instruction lists; the formatter does not validate that a corresponding installed frame exists.</param>
    /// <returns>The filename kraid-head-XXXX.json; instruction timing and hitboxes remain outside this visual file.</returns>
    public static string HeadFileName(ushort sourcePointer) =>
        $"kraid-head-{sourcePointer:X4}.json";
}
