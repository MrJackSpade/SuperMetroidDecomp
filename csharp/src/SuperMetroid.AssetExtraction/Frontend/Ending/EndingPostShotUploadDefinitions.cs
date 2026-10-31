using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Compiled six-transfer schedule for Func142's post-credits Super Metroid logo.
/// Artwork bytes remain separate presentation assets; this table fixes only
/// their native ordering, lengths and sources.
/// </summary>
[EncodingOwner("ending post-shot transfer schedule: sources, lengths and order")]
internal static class EndingPostShotUploadDefinitions
{

    /// <summary>The bytes into the ending font's characters the subtitle's characters start at.</summary>
    public const int SubtitleFontOffset = 0x1000;

    /// <summary>Func142 uploads the subtitle, four logo-tile chunks, then the logo map.</summary>
    public const int Count = 6;

    /// <summary>Subtitle font transfer length at <c>$8B:E45A</c>.</summary>
    internal const ushort SubtitleLength = 0x0400;

    /// <summary>Logo-tile chunk length at <c>$8B:E462</c> through <c>$8B:E47A</c>.</summary>
    internal const ushort LogoTileChunkLength = 0x0800;

    /// <summary>Logo-map transfer length at <c>$8B:E482</c>.</summary>
    internal const ushort LogoMapLength = 0x0800;
}
