using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>A reserve tank's manual transfer: one energy per selected manual-transfer tick.</summary>
    public const int ReserveEnergyPerFrame = 1;

    /// <summary>
    /// The wireframe the equipment page shows for <paramref name="equippedItems"/>: Samus in the Varia Suit or the Power
    /// Suit, with Hi-Jump Boots or without. Every other item, the Gravity Suit included, leaves it unchanged.
    /// </summary>
    public static PauseWireframeKind WireframeKind(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
            ? equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? PauseWireframeKind.VariaSuitHiJump : PauseWireframeKind.VariaSuit
            : equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? PauseWireframeKind.PowerSuitHiJump : PauseWireframeKind.PowerSuit;
}
