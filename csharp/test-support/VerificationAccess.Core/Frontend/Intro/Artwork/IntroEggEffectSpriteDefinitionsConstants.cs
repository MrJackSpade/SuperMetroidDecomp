namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Cartridge values of <see cref="IntroEggEffectSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroEggEffectSpriteDefinitionsConstants
{
    /// <summary>$8C:8FCB, exclusive end after eleven consecutive seven-byte records.</summary>
    public const ushort End = 0x8fcb;

    /// <summary>Provides the intro egg effect sprite boundary for frame decoding.</summary>
    extension(IntroEggEffectSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroEggEffectSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroEggEffectSpriteDefinitionsConstants.End;
    }
}
