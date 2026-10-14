namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Cartridge values of <see cref="IntroMotherBrainSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainSpriteDefinitionsConstants
{
    /// <summary>$8C:8C2F, second nine-part Mother Brain frame.</summary>
    public const ushort FrameOne = IntroMotherBrainSpriteDefinitions.FrameZero + 2 + 5 * IntroMotherBrainSpriteDefinitions.StockPartCount;
    /// <summary>$8C:8C5E, third nine-part Mother Brain frame.</summary>
    public const ushort FrameTwo = FrameOne + 2 + 5 * IntroMotherBrainSpriteDefinitions.StockPartCount;

    /// <summary>Provides the first two intro Mother Brain sprite frames for cinematic comparisons.</summary>
    extension(IntroMotherBrainSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainSpriteDefinitionsConstants.FrameOne"/>
        internal static ushort FrameOne => IntroMotherBrainSpriteDefinitionsConstants.FrameOne;
        /// <inheritdoc cref="IntroMotherBrainSpriteDefinitionsConstants.FrameTwo"/>
        internal static ushort FrameTwo => IntroMotherBrainSpriteDefinitionsConstants.FrameTwo;
    }
}
