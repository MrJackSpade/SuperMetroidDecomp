namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Cartridge values of <see cref="IntroScientistSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroScientistSpriteDefinitionsConstants
{
    /// <summary>$8C:8D6F, exclusive end of ten consecutive compositions.</summary>
    public const ushort End = 0x8d6f;

    /// <summary>Provides the intro scientist sprite boundary for frame extraction.</summary>
    extension(IntroScientistSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroScientistSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroScientistSpriteDefinitionsConstants.End;
    }
}
