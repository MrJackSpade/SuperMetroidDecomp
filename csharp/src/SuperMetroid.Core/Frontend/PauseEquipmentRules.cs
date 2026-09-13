using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned pause inventory and transfer rules, independent of authored menu visuals.</summary>
internal static class PauseEquipmentRules
{
    /// <summary>$82:C04C EquipmentBitmasks.weapons: native menu order is Charge, Ice, Wave, Spazer, Plasma.</summary>
    private static readonly SamusBeamFlags[] beams =
        [SamusBeamFlags.Charge, SamusBeamFlags.Ice, SamusBeamFlags.Wave, SamusBeamFlags.Spazer, SamusBeamFlags.Plasma];
    /// <summary>$82:C056 EquipmentBitmasks.suits: the native category includes morph/bomb/misc equipment.</summary>
    private static readonly SamusEquipmentFlags[] suits =
        [SamusEquipmentFlags.VariaSuit, SamusEquipmentFlags.GravitySuit, SamusEquipmentFlags.MorphBall,
         SamusEquipmentFlags.Bombs, SamusEquipmentFlags.SpringBall, SamusEquipmentFlags.ScrewAttack];
    /// <summary>$82:C062 EquipmentBitmasks.boots: Hi-Jump, Space Jump, Speed Booster.</summary>
    private static readonly SamusEquipmentFlags[] boots =
        [SamusEquipmentFlags.HiJumpBoots, SamusEquipmentFlags.SpaceJump, SamusEquipmentFlags.SpeedBooster];
    /// <summary>$82:BF04 ReserveTank_TransferEnergyPerFrame: one energy per selected manual-transfer tick.</summary>
    public const ushort ReserveEnergyPerFrame = 1;

    public static ushort Mask(int category, int item)
    {
        int count = category switch
        {
            PauseEquipmentCategories.Beams => beams.Length,
            PauseEquipmentCategories.Suits => suits.Length,
            PauseEquipmentCategories.Boots => boots.Length,
            _ => throw new ArgumentOutOfRangeException(nameof(category), "Reserves have their own controls, not equipment masks.")
        };
        if ((uint)item >= count) throw new ArgumentOutOfRangeException(nameof(item));
        return category switch
        {
            PauseEquipmentCategories.Beams => (ushort)beams[item],
            PauseEquipmentCategories.Suits => (ushort)suits[item],
            _ => (ushort)boots[item]
        };
    }

    /// <summary>$82:B20C/$82:B257 selects [neither, Hi-Jump, Varia, both]. Gravity and other bits do not participate.</summary>
    public static int WireframeIndex(ushort equippedItems) =>
        (equippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots) ? 1 : 0) |
        (equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 2 : 0);
}
