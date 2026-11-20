namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="SamusArmCannonArtworkFormat"/> that only verification reads.</summary>
internal static class SamusArmCannonArtworkFormatConstants
{
    /// <summary>Four tile-selector slots per native aim direction: closed sentinel 0 and three successive nonclosed cover-art frames.</summary>
    public const int FramesPerDirection = 4;
}
