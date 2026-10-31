using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseEquipmentLabelDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Names the equipment screen's item labels (<see cref="PauseEquipmentLabelDefinitions"/>), their widths and where each
/// starts; the import pictures their tiles (#1467).
/// </summary>
public static class PauseEquipmentLabelTiles
{
    /// <summary>Equipment label identities alias the corresponding selector controls. Native
    /// 82:C08C..C0A6 selects Charge/Ice/Wave/Spazer/Plasma, suit/misc and boot labels in
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

    /// <summary>The JSON key of an ordinary label, which is its selector control's anchor name.</summary>
    public static string Key(PauseEquipmentCategory category, int item) => Name(Label(category, item));

    /// <summary>The JSON key of a label: its selector control's anchor name, or <see cref="HyperKey"/>.</summary>
    public static string Name(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) return HyperKey;
        PauseEquipmentLabelKey entry = Labels().Single(candidate => candidate.Label == label);
        return PauseSelectorAnchors.Anchor(entry.Category, entry.Item);
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

    /// <summary>Returns the ordinary category strip width in sixteen-bit tile words, not bytes; Hyper's stored width and Plasma's button-patch overrun are separate contracts.</summary>
    /// <param name="category">Native equipment category: one denotes beams; every other integer returns the equipment width without category validation.</param>
    /// <returns>Five for the beam category, otherwise nine.</returns>
    public static int WordCount(PauseEquipmentCategory category) => category == PauseEquipmentCategory.Beams ? BeamWords : EquipmentWords;

    /// <summary>Uses five source words for ordinary beams and nine for other labels, including stored Hyper.</summary>
    internal static int StockWordCount(PauseEquipmentLabelId label)
    {
        _ = StockPageByte(label);
        return IsOrdinaryBeam(label) ? BeamWords : EquipmentWords;
    }

    /// <summary>$82:C06C-C086: label columns4/21 and category rows16/9/19, with the suit-to-misc gap.</summary>
    internal static int StockPageByte(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) label = Label(PauseEquipmentCategory.Beams, HyperBeamItem);
        return Ensure.Listed(destinations.Value, label);
    }

    /// <summary>Native label destinations derived once from category columns and rows for per-cell lookup.</summary>
    private static readonly Lazy<Dictionary<PauseEquipmentLabelId, int>> destinations = new(() =>
        Labels().ToDictionary(label => label.Label, label =>
        {
            Position16<TilemapCells> first = label.Category switch
            {
                PauseEquipmentCategory.Beams => new Position16<TilemapCells>(4, 16),
                PauseEquipmentCategory.Suits => new Position16<TilemapCells>(21, 9 + (label.Item >= 2 ? 2 : 0)),
                PauseEquipmentCategory.Boots => new Position16<TilemapCells>(21, 19),
                PauseEquipmentCategory.Reserves => throw new InvalidDataException("Unknown equipment label category."),
                _ => throw new InvalidDataException("Unknown equipment label category."),
            };
            return ((first.Y + label.Item) * TilemapColumns + first.X) * sizeof(ushort);
        }));
}
