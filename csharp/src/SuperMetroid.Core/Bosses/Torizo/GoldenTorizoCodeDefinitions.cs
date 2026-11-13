using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Literal inventory writes of Golden Torizo's initialization code.</summary>
internal static class GoldenTorizoCodeDefinitions
{
    /// <summary>The chord the code compares: exactly held A+B+X+Y, with no other controller bits.</summary>
    internal const ControllerButton ControllerChord = ControllerButton.A | ControllerButton.B | ControllerButton.X | ControllerButton.Y;
    /// <summary>Current and maximum energy, exactly 700 rather than 799.</summary>
    internal const int Energy = 700;
    /// <summary>Current and maximum reserve energy; reserve mode is not written.</summary>
    internal const int ReserveEnergy = 300;
    /// <summary>Current and maximum missiles.</summary>
    internal const int Missiles = 100;
    /// <summary>Current and maximum Super Missiles.</summary>
    internal const int SuperMissiles = 20;
    /// <summary>Current and maximum Power Bombs.</summary>
    internal const int PowerBombs = 20;
    /// <summary>
    /// Every item but the Screw Attack, replacing the collected and equipped items without merging
    /// prior upgrades. The game's item set also holds one more flag, which names no item and nothing tests.
    /// </summary>
    internal const SamusEquipmentFlags Items = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall |
        SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster |
        SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.UnassignedItem;
    /// <summary>Every beam, collected and equipped, including simultaneous Spazer and Plasma.</summary>
    internal const SamusBeamFlags Beams = SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer |
        SamusBeamFlags.Plasma | SamusBeamFlags.Charge;
}
