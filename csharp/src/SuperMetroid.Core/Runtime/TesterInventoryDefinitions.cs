using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>Inventory granted by the explicit host testing option, not a cartridge pickup.</summary>
internal static class TesterInventoryDefinitions
{
    /// <summary>Starting energy assigned by the explicit tester inventory option.</summary>
    internal const ushort Energy = 1499;
    /// <summary>Reserve energy assigned by the explicit tester inventory option.</summary>
    internal const ushort Reserves = 400;
    /// <summary>Missile ammunition assigned by the explicit tester inventory option.</summary>
    internal const ushort Missiles = 230;
    /// <summary>Super-missile ammunition assigned by the explicit tester inventory option.</summary>
    internal const ushort SuperMissiles = 50;
    /// <summary>Power-bomb ammunition assigned by the explicit tester inventory option.</summary>
    internal const ushort PowerBombs = 50;
    /// <summary>Equipment flags granted by the explicit tester inventory option.</summary>
    internal const SamusEquipmentFlags Equipment = SamusEquipmentFlags.VariaSuit |
        SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack |
        SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump |
        SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam |
        SamusEquipmentFlags.XrayScope;
    /// <summary>Beam flags granted by the explicit tester inventory option.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Charge | SamusBeamFlags.Ice |
        SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;
}
