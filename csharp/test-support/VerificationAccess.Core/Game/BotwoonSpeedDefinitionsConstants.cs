using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="BotwoonSpeedDefinitions"/> that only verification reads.</summary>
internal static class BotwoonSpeedDefinitionsConstants
{
    /// <summary>$B3:94BB, BotwoonSpeedTable: three speed/body-travel-time pairs.</summary>
    public const int MovementReferenceAddress = 0xb394bb;

    /// <summary>Provides Botwoon's native movement reference address for speed data comparison.</summary>
    extension(BotwoonSpeedDefinitions)
    {
        /// <inheritdoc cref="BotwoonSpeedDefinitionsConstants.MovementReferenceAddress"/>
        internal static int MovementReferenceAddress => BotwoonSpeedDefinitionsConstants.MovementReferenceAddress;
    }
}
