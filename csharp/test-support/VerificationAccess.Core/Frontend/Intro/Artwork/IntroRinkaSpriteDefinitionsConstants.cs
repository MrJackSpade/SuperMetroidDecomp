namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Cartridge values of <see cref="IntroRinkaSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroRinkaSpriteDefinitionsConstants
{
    /// <summary>$8C:8CCF, exclusive end after three consecutive 22-byte records.</summary>
    public const ushort End = 0x8ccf;

    /// <summary>Provides the intro Rinka sprite boundary for frame extraction.</summary>
    extension(IntroRinkaSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroRinkaSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroRinkaSpriteDefinitionsConstants.End;
    }
}
