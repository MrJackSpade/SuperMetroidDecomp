using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>Inventory granted by the explicit host testing option, not a cartridge pickup.</summary>
internal static class TesterInventoryDefinitions
{
    internal const ushort Energy = 1499;
    internal const ushort Reserves = 400;
    internal const ushort Missiles = 230;
    internal const ushort SuperMissiles = 50;
    internal const ushort PowerBombs = 50;
    internal const SamusEquipmentFlags Equipment = SamusEquipmentFlags.VariaSuit |
        SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack |
        SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump |
        SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam |
        SamusEquipmentFlags.XrayScope;
    internal const SamusBeamFlags Beams = SamusBeamFlags.Charge | SamusBeamFlags.Ice |
        SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;
}
