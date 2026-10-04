using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>$82:BF04 ReserveTank_TransferEnergyPerFrame: one energy per selected manual-transfer tick.</summary>
    public const ushort ReserveEnergyPerFrame = 1;

    /// <summary>Selects the upgrade flag belonging to a named category/item control.
    /// Native82:C04C..C065 has five beam, six suit/misc and three boot words in menu
    /// order. These are independent bit identities, not a numeric sequence; callers
    /// use the selected bit to test inventory or toggle exactly that upgrade.
    /// Reserve/unknown categories reject before item selection; category-local holes reject.</summary>
    public static ushort Mask(int category, int item)
    {
        if (category is not (PauseEquipmentCategories.Beams or PauseEquipmentCategories.Suits or PauseEquipmentCategories.Boots))
            throw new ArgumentOutOfRangeException(nameof(category), "Reserves have their own controls, not equipment masks.");
        return (category, item) switch
        {
            (PauseEquipmentCategories.Beams, 0) => (ushort)SamusBeamFlags.Charge,
            (PauseEquipmentCategories.Beams, 1) => (ushort)SamusBeamFlags.Ice,
            (PauseEquipmentCategories.Beams, 2) => (ushort)SamusBeamFlags.Wave,
            (PauseEquipmentCategories.Beams, 3) => (ushort)SamusBeamFlags.Spazer,
            (PauseEquipmentCategories.Beams, 4) => (ushort)SamusBeamFlags.Plasma,
            (PauseEquipmentCategories.Suits, 0) => (ushort)SamusEquipmentFlags.VariaSuit,
            (PauseEquipmentCategories.Suits, 1) => (ushort)SamusEquipmentFlags.GravitySuit,
            (PauseEquipmentCategories.Suits, 2) => (ushort)SamusEquipmentFlags.MorphBall,
            (PauseEquipmentCategories.Suits, 3) => (ushort)SamusEquipmentFlags.Bombs,
            (PauseEquipmentCategories.Suits, 4) => (ushort)SamusEquipmentFlags.SpringBall,
            (PauseEquipmentCategories.Suits, 5) => (ushort)SamusEquipmentFlags.ScrewAttack,
            (PauseEquipmentCategories.Boots, 0) => (ushort)SamusEquipmentFlags.HiJumpBoots,
            (PauseEquipmentCategories.Boots, 1) => (ushort)SamusEquipmentFlags.SpaceJump,
            (PauseEquipmentCategories.Boots, 2) => (ushort)SamusEquipmentFlags.SpeedBooster,
            _ => throw new ArgumentOutOfRangeException(nameof(item)),
        };
    }
    /// <summary>$82:B20C/$82:B257 selects [neither, Hi-Jump, Varia, both]. Gravity and other bits do not participate.</summary>
    public static int WireframeIndex(ushort equippedItems) =>
        (equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? 1 : 0) |
        (equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 2 : 0);
}
