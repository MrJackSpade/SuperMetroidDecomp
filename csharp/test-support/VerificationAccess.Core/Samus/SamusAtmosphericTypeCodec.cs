namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The cartridge's numbering of Samus's atmospheric effect types, decoded to the pictures each draws (#2556): type 1 the
/// footstep splash, types 4, 6 and 7 the shared lava spray and dust. Type 2's mirrored words and the spritemap-based effects
/// draw no single-piece pictures.
/// </summary>
internal static class SamusAtmosphericTypeCodec
{
    /// <summary>The pictures the cartridge's atmospheric type <paramref name="type"/> draws.</summary>
    /// <returns>False for a type that draws no single-piece pictures.</returns>
    internal static bool TryDecode(int type, out SamusAtmosphericPictures pictures)
    {
        pictures = type == 1 ? SamusAtmosphericPictures.FootstepSplash : SamusAtmosphericPictures.LavaAndDust;
        return type is 1 or 4 or 6 or 7;
    }
}
