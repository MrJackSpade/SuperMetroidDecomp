using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Assets"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataAssetsConstants
{
    /// <summary>Bank-$95 source at $D713 for the second Japanese intro font tile set.</summary>
    public const int JapaneseFontTwo = 0x95d713;
    /// <summary>Bank-$8C source at $BEC3 for Mother Brain flashback room level data.</summary>
    public const int MotherBrainLevelData = 0x8cbec3;

    /// <summary>Provides the Japanese font and Mother Brain level asset locations used by intro cinematic checks.</summary>
    extension(IntroCinematicRomData.Assets)
    {
        /// <inheritdoc cref="IntroCinematicRomDataAssetsConstants.JapaneseFontTwo"/>
        internal static int JapaneseFontTwo => IntroCinematicRomDataAssetsConstants.JapaneseFontTwo;
        /// <inheritdoc cref="IntroCinematicRomDataAssetsConstants.MotherBrainLevelData"/>
        internal static int MotherBrainLevelData => IntroCinematicRomDataAssetsConstants.MotherBrainLevelData;
    }
}
