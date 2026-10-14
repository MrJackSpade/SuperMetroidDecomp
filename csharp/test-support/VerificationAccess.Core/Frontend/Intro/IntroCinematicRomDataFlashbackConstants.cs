namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Flashback"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataFlashbackConstants
{
    /// <summary>Byte length of the Mother Brain flashback room's imported level data.</summary>
    public const int MotherBrainLevelByteCount = 448;

    /// <summary>Provides the Mother Brain flashback level data length for intro import checks.</summary>
    extension(IntroCinematicRomData.Flashback)
    {
        /// <inheritdoc cref="IntroCinematicRomDataFlashbackConstants.MotherBrainLevelByteCount"/>
        internal static int MotherBrainLevelByteCount => IntroCinematicRomDataFlashbackConstants.MotherBrainLevelByteCount;
    }
}
