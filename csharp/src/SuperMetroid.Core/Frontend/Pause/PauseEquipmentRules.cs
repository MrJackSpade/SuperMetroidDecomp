using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>A reserve tank's manual transfer: one energy per selected manual-transfer tick.</summary>
    public const int ReserveEnergyPerFrame = 1;

    /// <summary>
    /// The wireframe the equipment page shows for <paramref name="equippedItems"/>: Samus in the Varia Suit or the Power
    /// Suit, with Hi-Jump Boots or without. Every other item, the Gravity Suit included, leaves it unchanged.
    /// </summary>
    public static PauseWireframe Wireframe(SamusEquipmentFlags equippedItems) => new(
        equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? PauseWireframeSuit.VariaSuit : PauseWireframeSuit.PowerSuit,
        equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? PauseWireframeBoots.HiJumpBoots : PauseWireframeBoots.RegularBoots);
}
