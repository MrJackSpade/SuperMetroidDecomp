namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Files and exact native transfer sizes of the Zebes reveal's tilemap and characters (#1467).</summary>
public static class ZebesRevealArtworkFormat
{
    /// <summary>Installed JSON filename for the ordinary 32-by-32 BG tilemap page shown during the Zebes reveal.</summary>
    public const string MapFileName = "zebes-reveal-tilemap.json";
    /// <summary>Installed indexed PNG filename for the Zebes-reveal 4bpp characters, separate from its tilemap references.</summary>
    public const string CharacterFileName = "zebes-reveal-characters.png";
    /// <summary>$0800 bytes for one ordinary BG tilemap page, transferred from native ZebesTilemap to VRAM word $5C00 by $8B:C6F1–$C70E.</summary>
    public const int MapByteCount = 1024 * sizeof(ushort);
    /// <summary>$4000 bytes of 4bpp Zebes characters, matching the native ZebesTiles transfer to VRAM word $6000 at $8B:C70C–$C72E.</summary>
    public const int CharacterByteCount = 512 * 32;
}
