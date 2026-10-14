using SuperMetroid.Core.Assets;

/// <summary>Cartridge values of <see cref="IntroMotherBrainExplosionSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitionsConstants
{
    /// <summary>$8C:98D2, exclusive end of the twelve consecutive composition records.</summary>
    public const ushort End = 0x98d2;

    /// <summary>Provides the intro Mother Brain explosion sprite boundary for frame extraction.</summary>
    extension(IntroMotherBrainExplosionSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainExplosionSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroMotherBrainExplosionSpriteDefinitionsConstants.End;
    }
}
