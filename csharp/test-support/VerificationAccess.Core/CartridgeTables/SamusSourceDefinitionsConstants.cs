using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
internal static class SamusSourceDefinitionsConstants
{
    /// <summary>$90:CC21, CostOfSBAsInPowerBombs: twelve word costs by beam index, the arm cannon drawing window's last bytes.</summary>
    internal static readonly SnesAddress SpecialBeamAttackCosts = new(0x90, 0xcc21);
    /// <summary>The ten firing directions each direction-indexed firing table holds.</summary>
    internal const int FiringDirectionCount = 10;

    /// <summary>The twelve retail beam combinations each combination-indexed firing table holds.</summary>
    internal const int RetailBeamCombinationCount = 12;

    /// <summary>$90:C254: the uncharged cooldown row, a byte per beam combination; the charged row follows at $90:C264.</summary>
    internal const int UnchargedCooldowns = 0x90c254;

    /// <summary>$90:C264: the charged cooldown row, a byte per beam combination.</summary>
    internal const int ChargedCooldowns = 0x90c264;

    /// <summary>$90:C283: the held-fire cooldown row, a byte per beam combination.</summary>
    internal const int HeldFireCooldowns = 0x90c283;

    /// <summary>$90:C303: the missile acceleration records, an X and a Y word per firing direction.</summary>
    internal const int MissileAccelerations = 0x90c303;

    /// <summary>$90:C32B: the Super Missile acceleration records, an X and a Y word per firing direction.</summary>
    internal const int SuperMissileAccelerations = 0x90c32b;

    /// <summary>$90:C353: the beam X accelerations, a word per firing direction.</summary>
    internal const int BeamXAccelerations = 0x90c353;

    /// <summary>$90:C367: the beam Y accelerations, a word per firing direction.</summary>
    internal const int BeamYAccelerations = 0x90c367;

    /// <summary>$90:C204: the ten direction-indexed X muzzle origins for poses that are not running.</summary>
    internal const int DefaultXOrigins = 0x90c204;

    /// <summary>$90:C218: the ten direction-indexed Y muzzle origins for poses that are not running.</summary>
    internal const int DefaultYOrigins = 0x90c218;

    /// <summary>$90:C22C: the ten direction-indexed X muzzle origins for running and moonwalking poses.</summary>
    internal const int RunningXOrigins = 0x90c22c;

    /// <summary>$90:C240: the ten direction-indexed Y muzzle origins for running and moonwalking poses.</summary>
    internal const int RunningYOrigins = 0x90c240;
}
