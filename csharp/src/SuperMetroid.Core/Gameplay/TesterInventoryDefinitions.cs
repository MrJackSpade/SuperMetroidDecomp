using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Inventory granted by the explicit host testing option, not a cartridge pickup.</summary>
internal static class TesterInventoryDefinitions
{
    /// <summary>Sets both current and maximum health when the host test inventory is granted.</summary>
    internal const int Energy = 1499;
    /// <summary>Sets both current and maximum reserve energy in the host test inventory.</summary>
    internal const int Reserves = 400;
    /// <summary>Sets both current and maximum missile ammunition in the host test inventory.</summary>
    internal const int Missiles = 230;
    /// <summary>Sets both current and maximum super-missile ammunition in the host test inventory.</summary>
    internal const int SuperMissiles = 50;
    /// <summary>Sets both current and maximum power-bomb ammunition in the host test inventory.</summary>
    internal const int PowerBombs = 50;
    /// <summary>Marks the test option's item set as both collected and equipped.</summary>
    internal const SamusEquipmentFlags Equipment = SamusEquipmentFlags.VariaSuit |
        SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack |
        SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump |
        SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam |
        SamusEquipmentFlags.XrayScope;
    /// <summary>Marks all beam upgrades collected; the host equips this set without Spazer to respect Plasma exclusion.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Charge | SamusBeamFlags.Ice |
        SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;
}
