using SuperMetroid.Core.Assets;

/// <summary>Cartridge values of <see cref="IntroDiscoveryActorSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroDiscoveryActorSpriteDefinitionsConstants
{
    /// <summary>$8C:90FE, exclusive end of the reused nineteen-part asteroid composition.</summary>
    public const ushort BabyEnd = 0x90fe;
    /// <summary>$8C:8FE0, exclusive end of the three small confused-baby compositions.</summary>
    public const ushort BabySmallEnd = 0x8fe0;
    /// <summary>$8C:8F7E, exclusive end of the sixteen consecutive egg compositions.</summary>
    public const ushort EggEnd = 0x8f7e;

    /// <summary>Provides end boundaries of the intro discovery Baby and egg sprite groups.</summary>
    extension(IntroDiscoveryActorSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.BabyEnd"/>
        internal static ushort BabyEnd => IntroDiscoveryActorSpriteDefinitionsConstants.BabyEnd;
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.BabySmallEnd"/>
        internal static ushort BabySmallEnd => IntroDiscoveryActorSpriteDefinitionsConstants.BabySmallEnd;
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.EggEnd"/>
        internal static ushort EggEnd => IntroDiscoveryActorSpriteDefinitionsConstants.EggEnd;
    }
}
