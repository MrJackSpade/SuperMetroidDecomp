using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Literal inventory writes in InitAI_Torizo.GTCode at $AA:C917-C95A.</summary>
internal static class GoldenTorizoCodeDefinitions
{
    /// <summary>$AA:C919 CMP operand: exactly held A+B+X+Y, with no other controller bits.</summary>
    internal const ControllerButton ControllerChord = ControllerButton.A | ControllerButton.B | ControllerButton.X | ControllerButton.Y;
    /// <summary>$AA:C91E immediate: current and maximum energy, exactly 700 rather than 799.</summary>
    internal const int Energy = 700;
    /// <summary>$AA:C927 immediate: current and maximum reserve energy; reserve mode is not written.</summary>
    internal const int ReserveEnergy = 300;
    /// <summary>$AA:C930 immediate: current and maximum missiles.</summary>
    internal const int Missiles = 100;
    /// <summary>$AA:C939 immediate: current and maximum Super Missiles.</summary>
    internal const int SuperMissiles = 20;
    /// <summary>$AA:C942 immediate: current and maximum Power Bombs.</summary>
    internal const int PowerBombs = 20;
    /// <summary>
    /// $AA:C94B immediate: every item but the Screw Attack, replacing the collected and equipped items without merging
    /// prior upgrades. The cartridge's word also sets bit 4, which names no item and no retail routine tests.
    /// </summary>
    internal const SamusEquipmentFlags Items = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall |
        SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster |
        SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.UnassignedItem;
    /// <summary>$AA:C954 immediate: every beam, collected and equipped, including simultaneous Spazer and Plasma.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma | SamusBeamFlags.Charge;
}
