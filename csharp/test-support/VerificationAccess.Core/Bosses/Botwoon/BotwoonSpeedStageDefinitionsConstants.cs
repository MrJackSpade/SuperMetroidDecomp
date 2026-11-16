
namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values of <see cref="BotwoonSpeedStageDefinitions"/> that only verification reads.</summary>
internal static class BotwoonSpeedStageDefinitionsConstants
{
    /// <summary>$B3:94BB, BotwoonSpeedTable: three speed/body-travel-time pairs.</summary>
    public const int MovementReferenceAddress = 0xb394bb;

    /// <summary>The bytes of one head-history entry; the cartridge's segment spacing is a byte distance.</summary>
    public const int HistoryEntryBytes = 4;

    /// <summary>Provides Botwoon's native movement reference address for speed data comparison.</summary>
    extension(BotwoonSpeedStageDefinitions)
    {
        /// <inheritdoc cref="BotwoonSpeedStageDefinitionsConstants.MovementReferenceAddress"/>
        internal static int MovementReferenceAddress => BotwoonSpeedStageDefinitionsConstants.MovementReferenceAddress;
        /// <inheritdoc cref="BotwoonSpeedStageDefinitionsConstants.HistoryEntryBytes"/>
        internal static int HistoryEntryBytes => BotwoonSpeedStageDefinitionsConstants.HistoryEntryBytes;
    }
}
