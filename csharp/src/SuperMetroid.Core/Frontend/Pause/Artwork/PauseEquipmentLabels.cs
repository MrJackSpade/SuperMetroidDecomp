namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Names the equipment screen's item labels (#1467): which label each category and item has, and which are the ordinary
/// beam labels Hyper replaces; the import pictures each label whole.
/// </summary>
public static class PauseEquipmentLabels
{
    /// <summary>Equipment label identities alias the corresponding selector controls. The game
    /// selects Charge/Ice/Wave/Spazer/Plasma, suit/misc and boot labels in
    /// the same category/item order. Reserve controls have no ordinary inventory label.</summary>
    public static PauseEquipmentLabelId Label(PauseEquipmentCategory category, int item)
    {
        if (!Enum.IsDefined(category) || (uint)item >= ItemCount(category))
            throw new ArgumentOutOfRangeException(nameof(item), $"No pause equipment label exists for category {category}, item {item}.");
        return category switch
        {
            PauseEquipmentCategory.Beams => item switch
            {
                0 => PauseEquipmentLabelId.Charge,
                1 => PauseEquipmentLabelId.Ice,
                2 => PauseEquipmentLabelId.Wave,
                3 => PauseEquipmentLabelId.Spazer,
                4 => PauseEquipmentLabelId.Plasma,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Suits => item switch
            {
                0 => PauseEquipmentLabelId.Varia,
                1 => PauseEquipmentLabelId.Gravity,
                2 => PauseEquipmentLabelId.MorphBall,
                3 => PauseEquipmentLabelId.Bombs,
                4 => PauseEquipmentLabelId.SpringBall,
                5 => PauseEquipmentLabelId.ScrewAttack,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Boots => item switch
            {
                0 => PauseEquipmentLabelId.HiJump,
                1 => PauseEquipmentLabelId.SpaceJump,
                2 => PauseEquipmentLabelId.SpeedBooster,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Reserves => throw new ArgumentOutOfRangeException(nameof(category)),
            _ => throw new ArgumentOutOfRangeException(nameof(category)),
        };
    }

    /// <summary>Whether a label is one of the five ordinary beam labels.</summary>
    internal static bool IsOrdinaryBeam(PauseEquipmentLabelId label) => label is PauseEquipmentLabelId.Charge or
        PauseEquipmentLabelId.Ice or PauseEquipmentLabelId.Wave or PauseEquipmentLabelId.Spazer or PauseEquipmentLabelId.Plasma;

    /// <summary>Uses the reviewed category contract, including zero labels for reserves.</summary>
    public static int ItemCount(PauseEquipmentCategory category) => PauseEquipmentCategories.Get(category).ItemCount;

    /// <summary>The ordinary label view of the selector anchors, derived once rather than per lookup.</summary>
    public static IReadOnlyList<PauseEquipmentLabelKey> Labels() => labels.Value;

    /// <summary>Non-reserve selector anchors projected once into the fourteen ordinary inventory label identities.</summary>
    private static readonly Lazy<PauseEquipmentLabelKey[]> labels = new(() =>
        PauseSelectorAnchors.Anchors()
            .Where(anchor => anchor.Category != PauseEquipmentCategory.Reserves)
            .Select(anchor => new PauseEquipmentLabelKey(anchor.Category, anchor.Item, Label(anchor.Category, anchor.Item))).ToArray());
}
