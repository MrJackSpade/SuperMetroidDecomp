using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
internal static class SamusSourceDefinitionsConstants
{
    /// <summary>$90:CC21, CostOfSBAsInPowerBombs: twelve word costs by beam index, the arm cannon drawing window's last bytes.</summary>
    internal static readonly SnesAddress SpecialBeamAttackCosts = new(0x90, 0xcc21);
    /// <summary>$90:C204, where the firing tables <see cref="SuperMetroid.Core.Samus.Projectiles.SamusFiringTableDefinitions"/> lays out start.</summary>
    internal static readonly SnesAddress FiringTables = new(0x90, 0xc204);
}
