using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>A reserve tank's manual transfer: one energy per selected manual-transfer tick.</summary>
    public const int ReserveEnergyPerFrame = 1;

    /// <summary>Selects the upgrade flag belonging to a named category/item control.
    /// The menu lists five beams, six suit and miscellaneous items and three boots in menu
    /// order. These are independent bit identities, not a numeric sequence; callers
    /// use the selected bit to test inventory or toggle exactly that upgrade.
    /// Reserve/unknown categories reject before item selection; category-local holes reject.</summary>
    public static PauseEquipmentBit Mask(PauseEquipmentCategory category, int item)
    {
        if (category is not (PauseEquipmentCategory.Beams or PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots))
            throw new ArgumentOutOfRangeException(nameof(category), "Reserves have their own controls, not equipment masks.");
        return new PauseEquipmentSlot(category, item) switch
        {
            (PauseEquipmentCategory.Beams, 0) => PauseEquipmentBit.Of(SamusBeamFlags.Charge),
            (PauseEquipmentCategory.Beams, 1) => PauseEquipmentBit.Of(SamusBeamFlags.Ice),
            (PauseEquipmentCategory.Beams, 2) => PauseEquipmentBit.Of(SamusBeamFlags.Wave),
            (PauseEquipmentCategory.Beams, 3) => PauseEquipmentBit.Of(SamusBeamFlags.Spazer),
            (PauseEquipmentCategory.Beams, 4) => PauseEquipmentBit.Of(SamusBeamFlags.Plasma),
            (PauseEquipmentCategory.Suits, 0) => PauseEquipmentBit.Of(SamusEquipmentFlags.VariaSuit),
            (PauseEquipmentCategory.Suits, 1) => PauseEquipmentBit.Of(SamusEquipmentFlags.GravitySuit),
            (PauseEquipmentCategory.Suits, 2) => PauseEquipmentBit.Of(SamusEquipmentFlags.MorphBall),
            (PauseEquipmentCategory.Suits, 3) => PauseEquipmentBit.Of(SamusEquipmentFlags.Bombs),
            (PauseEquipmentCategory.Suits, 4) => PauseEquipmentBit.Of(SamusEquipmentFlags.SpringBall),
            (PauseEquipmentCategory.Suits, 5) => PauseEquipmentBit.Of(SamusEquipmentFlags.ScrewAttack),
            (PauseEquipmentCategory.Boots, 0) => PauseEquipmentBit.Of(SamusEquipmentFlags.HiJumpBoots),
            (PauseEquipmentCategory.Boots, 1) => PauseEquipmentBit.Of(SamusEquipmentFlags.SpaceJump),
            (PauseEquipmentCategory.Boots, 2) => PauseEquipmentBit.Of(SamusEquipmentFlags.SpeedBooster),
            _ => throw new ArgumentOutOfRangeException(nameof(item)),
        };
    }
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
