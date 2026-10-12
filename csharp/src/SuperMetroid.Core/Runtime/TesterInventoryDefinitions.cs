using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>Inventory granted by the explicit host testing option, not a cartridge pickup.</summary>
internal static class TesterInventoryDefinitions
{
    /// <summary>Stores energy.</summary>
    internal const ushort Energy = 1499;
    /// <summary>Stores reserves.</summary>
    internal const ushort Reserves = 400;
    /// <summary>Stores missiles.</summary>
    internal const ushort Missiles = 230;
    /// <summary>Stores super missiles.</summary>
    internal const ushort SuperMissiles = 50;
    /// <summary>Stores power bombs.</summary>
    internal const ushort PowerBombs = 50;
    /// <summary>Stores equipment.</summary>
    internal const SamusEquipmentFlags Equipment = SamusEquipmentFlags.VariaSuit |
        SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack |
        SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump |
        SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam |
        SamusEquipmentFlags.XrayScope;
    /// <summary>Stores beams.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Charge | SamusBeamFlags.Ice |
        SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;
}
