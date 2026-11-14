using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>A reserve tank's manual transfer: one energy per selected manual-transfer tick.</summary>
    public const int ReserveEnergyPerFrame = 1;

    /// <summary>Exact inverse of the four boot/suit equipment combinations.
    /// The game reduces the equipped items to Varia and Hi-Jump, then
    /// searches [neither, Hi-Jump, Varia, both]. Every combination of items
    /// is supported; every other item, including
    /// Gravity, is ignored.</summary>
    public static PauseWireframeKind WireframeKind(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
            ? equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? PauseWireframeKind.VariaSuitHiJump : PauseWireframeKind.VariaSuit
            : equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? PauseWireframeKind.PowerSuitHiJump : PauseWireframeKind.PowerSuit;
}
