
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainBg2Definitions"/> that only verification reads.</summary>
internal static class MotherBrainBg2DefinitionsConstants
{
    /// <summary>EnemyBG2Tilemap at $7E:2000.</summary>
    public const int WorkAddress = 0x7e2000;

    /// <summary>Exposes the native Mother Brain BG2 work buffer address to verification callers.</summary>
    extension(MotherBrainBg2Definitions)
    {
        /// <inheritdoc cref="MotherBrainBg2DefinitionsConstants.WorkAddress"/>
        internal static int WorkAddress => MotherBrainBg2DefinitionsConstants.WorkAddress;
    }
}
