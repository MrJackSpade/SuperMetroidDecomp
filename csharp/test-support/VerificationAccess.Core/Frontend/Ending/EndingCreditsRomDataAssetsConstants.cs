namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values of <see cref="EndingCreditsRomData.Assets"/> that only verification reads.</summary>
internal static class EndingCreditsRomDataAssetsConstants
{
    /// <summary>$95:A82F, decompressed to $7F:0000 for the flyaway's high-byte Mode-7 character lane.</summary>
    public const int FlyawayCharacters = 0x95a82f;

    /// <summary>Provides the ending flyaway character source used to verify credits asset extraction.</summary>
    extension(EndingCreditsRomData.Assets)
    {
        /// <inheritdoc cref="EndingCreditsRomDataAssetsConstants.FlyawayCharacters"/>
        internal static int FlyawayCharacters => EndingCreditsRomDataAssetsConstants.FlyawayCharacters;
    }
}
