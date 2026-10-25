using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Literal inventory writes in InitAI_Torizo.GTCode at $AA:C917-C95A.</summary>
internal static class GoldenTorizoCodeDefinitions
{
    /// <summary>$AA:C919 CMP operand: exactly held A+B+X+Y, with no other controller bits.</summary>
    internal const SnesButton ControllerChord = SnesButton.A | SnesButton.B | SnesButton.X | SnesButton.Y;
    /// <summary>$AA:C91E immediate: current and maximum energy, exactly 700 rather than 799.</summary>
    internal const ushort Energy = 700;
    /// <summary>$AA:C927 immediate: current and maximum reserve energy; reserve mode is not written.</summary>
    internal const ushort ReserveEnergy = 300;
    /// <summary>$AA:C930 immediate: current and maximum missiles.</summary>
    internal const ushort Missiles = 100;
    /// <summary>$AA:C939 immediate: current and maximum Super Missiles.</summary>
    internal const ushort SuperMissiles = 20;
    /// <summary>$AA:C942 immediate: current and maximum Power Bombs.</summary>
    internal const ushort PowerBombs = 20;
    /// <summary>
    /// $AA:C94B immediate: every item but the Screw Attack, replacing the collected and equipped items without merging
    /// prior upgrades. The cartridge's word also sets bit 4, which names no item and no retail routine tests.
    /// </summary>
    internal const SamusEquipmentFlags Items = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall |
        SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster |
        SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope | UnnamedItemBit;
    /// <summary>The bit the code's item word sets that names no item.</summary>
    private const SamusEquipmentFlags UnnamedItemBit = (SamusEquipmentFlags)0x0010;
    /// <summary>$AA:C954 immediate: every beam, collected and equipped, including simultaneous Spazer and Plasma.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma | SamusBeamFlags.Charge;
}
