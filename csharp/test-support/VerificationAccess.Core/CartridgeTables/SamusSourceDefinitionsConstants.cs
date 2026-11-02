using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
internal static class SamusSourceDefinitionsConstants
{
    /// <summary>$90:CC21, CostOfSBAsInPowerBombs: twelve word costs by beam index, the arm cannon drawing window's last bytes.</summary>
    internal static readonly SnesAddress SpecialBeamAttackCosts = new(0x90, 0xcc21);
    /// <summary>$90:C204, where the firing tables <see cref="SuperMetroid.Core.Samus.Projectiles.SamusFiringTableDefinitions"/> lays out start.</summary>
    internal static readonly SnesAddress FiringTables = new(0x90, 0xc204);

    /// <summary>$90:C204: the ten direction-indexed X muzzle origins for poses that are not running.</summary>
    internal const int DefaultXOrigins = 0x90c204;

    /// <summary>$90:C218: the ten direction-indexed Y muzzle origins for poses that are not running.</summary>
    internal const int DefaultYOrigins = 0x90c218;

    /// <summary>$90:C22C: the ten direction-indexed X muzzle origins for running and moonwalking poses.</summary>
    internal const int RunningXOrigins = 0x90c22c;

    /// <summary>$90:C240: the ten direction-indexed Y muzzle origins for running and moonwalking poses.</summary>
    internal const int RunningYOrigins = 0x90c240;
}
