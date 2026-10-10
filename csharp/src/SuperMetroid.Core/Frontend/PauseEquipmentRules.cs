using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>$82:BF04 ReserveTank_TransferEnergyPerFrame: one energy per selected manual-transfer tick.</summary>
    public const ushort ReserveEnergyPerFrame = 1;

    /// <summary>Selects the upgrade flag belonging to a named category/item control.
    /// Native82:C04C..C067 has five beam, six suit/misc and three boot words in menu
    /// order. These are independent bit identities, not a numeric sequence; callers
    /// use the selected bit to test inventory or toggle exactly that upgrade.
    /// Reserve/unknown categories reject before item selection; category-local holes reject.</summary>
    public static ushort Mask(PauseEquipmentCategory category, int item)
    {
        if (category is not (PauseEquipmentCategory.Beams or PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots))
            throw new ArgumentOutOfRangeException(nameof(category), "Reserves have their own controls, not equipment masks.");
        return (category, item) switch
        {
            (PauseEquipmentCategory.Beams, 0) => (ushort)SamusBeamFlags.Charge,
            (PauseEquipmentCategory.Beams, 1) => (ushort)SamusBeamFlags.Ice,
            (PauseEquipmentCategory.Beams, 2) => (ushort)SamusBeamFlags.Wave,
            (PauseEquipmentCategory.Beams, 3) => (ushort)SamusBeamFlags.Spazer,
            (PauseEquipmentCategory.Beams, 4) => (ushort)SamusBeamFlags.Plasma,
            (PauseEquipmentCategory.Suits, 0) => (ushort)SamusEquipmentFlags.VariaSuit,
            (PauseEquipmentCategory.Suits, 1) => (ushort)SamusEquipmentFlags.GravitySuit,
            (PauseEquipmentCategory.Suits, 2) => (ushort)SamusEquipmentFlags.MorphBall,
            (PauseEquipmentCategory.Suits, 3) => (ushort)SamusEquipmentFlags.Bombs,
            (PauseEquipmentCategory.Suits, 4) => (ushort)SamusEquipmentFlags.SpringBall,
            (PauseEquipmentCategory.Suits, 5) => (ushort)SamusEquipmentFlags.ScrewAttack,
            (PauseEquipmentCategory.Boots, 0) => (ushort)SamusEquipmentFlags.HiJumpBoots,
            (PauseEquipmentCategory.Boots, 1) => (ushort)SamusEquipmentFlags.SpaceJump,
            (PauseEquipmentCategory.Boots, 2) => (ushort)SamusEquipmentFlags.SpeedBooster,
            _ => throw new ArgumentOutOfRangeException(nameof(item)),
        };
    }
    /// <summary>Exact inverse of the four equipment words at $82:B257..B25E.
    /// Native $82:B212 masks the entire equipped-items word to Varia/Hi-Jump, then
    /// searches [neither, Hi-Jump, Varia, both]. Pack Hi-Jump into result bit0 and
    /// Varia into bit1. All65536 input words are supported; every other bit, including
    /// Gravity, is ignored. The resulting0..3 ordinal selects the matching wireframe kind.</summary>
    public static int WireframeIndex(ushort equippedItems) =>
        (equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? 1 : 0) |
        (equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 2 : 0);
}
