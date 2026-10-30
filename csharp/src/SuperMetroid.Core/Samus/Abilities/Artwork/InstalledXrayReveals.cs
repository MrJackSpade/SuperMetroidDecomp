using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>
/// Installed visual choices for X-ray blocks. The cartridge's collision and behaviour lookup,
/// copy dimensions, extension traversal, and Brinstar-only condition remain compiled.
/// </summary>
public sealed class InstalledXrayReveals
{
    /// <summary>The number of behaviour values a collision kind's visuals span.</summary>
    private const int BehaviorValueCount = byte.MaxValue + 1;

    /// <summary>The collision kinds in the order the visual store lays out their behaviour spans.</summary>
    private static readonly RoomCollisionType[] TypeOrder =
    [
        RoomCollisionType.Air, RoomCollisionType.Slope, RoomCollisionType.SpikeAir, RoomCollisionType.SpecialAir,
        RoomCollisionType.ShootableAir, RoomCollisionType.HorizontalExtension, RoomCollisionType.UnusedAir,
        RoomCollisionType.BombableAir, RoomCollisionType.SolidBlock, RoomCollisionType.DoorBlock,
        RoomCollisionType.SpikeBlock, RoomCollisionType.SpecialBlock, RoomCollisionType.ShootableBlock,
        RoomCollisionType.VerticalExtension, RoomCollisionType.GrappleBlock, RoomCollisionType.BombableBlock,
    ];

    /// <summary>Each collision type's position in <see cref="TypeOrder"/>.</summary>
    private static readonly FrozenDictionary<RoomCollisionType, int> TypePosition =
        Enumerable.Range(0, TypeOrder.Length).ToFrozenDictionary(position => TypeOrder[position]);

    /// <summary>Installed reveal blocks indexed by collision kind and behaviour.</summary>
    /// <remarks>
    /// Issue #1037: the index is the kind's position times the 256 behaviour values, plus the behaviour, over the 16
    /// collision kinds. Of 4096 possible
    /// slots, the pinned NTSC J/U v1.0 ROM has 305 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates.
    /// Stock operands come from bank-$91 reveal command records and all
    /// 305 installed values match the independent cartridge oracle.
    /// The visual operands can also be replaced through reveals.json while
    /// the native command and copy dimensions remain fixed. Therefore no
    /// single ROM-derived formula can reproduce every valid installation:
    /// retain this bounded indexed store for the selected visual data.
    /// </remarks>
    private readonly XrayRevealVisualBlocks?[] blocks = new XrayRevealVisualBlocks?[16 * BehaviorValueCount];

    /// <summary>Where a collision kind and behaviour's visuals sit in the store.</summary>
    private static int Index(RoomCollisionType type, RoomBlockBehavior behavior) => TypePosition[type] * BehaviorValueCount + behavior.Value;

    /// <summary>Creates a complete visual replacement for every drawable native rule.</summary>
    /// <param name="entries">Exactly one set of blocks for each drawable collision kind and behaviour; command selection and copy geometry remain fixed.</param>
    /// <param name="overlays">Installed item/special-room visuals, or null for a noninstalled fixture.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, duplicates another pair, or leaves drawable coverage incomplete.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An entry names a block outside the tileset's block set.</exception>
    public InstalledXrayReveals(IEnumerable<XrayRevealVisualDefinition> entries, InstalledXrayOverlays? overlays = null)
    {
        Ensure.NotNull(entries);
        Overlays = overlays;
        foreach ((RoomCollisionType type, RoomBlockBehavior behavior, XrayRevealVisualBlocks visual) in entries)
        {
            XrayRevealDefinition? retail = XrayRevealTable.Find(type, behavior);
            if (retail is not { } definition || !IsDrawable(definition.Command))
                throw new InvalidDataException(
                    $"X-ray visual {type}/behaviour {behavior} has no drawable cartridge rule.");
            int index = Index(type, behavior);
            if (blocks[index] is not null)
                throw new InvalidDataException(
                    $"X-ray visual {type}/behaviour {behavior} appears more than once.");
            ValidateBlocks(visual);
            blocks[index] = visual;
        }

        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int value = 0; value < BehaviorValueCount; value++)
        {
            var behavior = new RoomBlockBehavior(value);
            bool drawable = XrayRevealTable.Find(type, behavior) is { } definition &&
                IsDrawable(definition.Command);
            if (drawable != (blocks[Index(type, behavior)] is not null))
                throw new InvalidDataException(
                    $"X-ray visual catalog does not cover cartridge rule {type}/behaviour {behavior}.");
        }
    }

    /// <summary>Installed item and special-room visuals; null only in noninstalled fixtures.</summary>
    public InstalledXrayOverlays? Overlays { get; }

    /// <summary>Identity of every selected drawable rule and its installed overlays, excluding command mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledXrayReveals), content =>
    {
        for (int index = 0; index < blocks.Length; index++)
            if (blocks[index] is { } visual)
            {
                content.Append("kind and behaviour", index);
                content.Append("top left", visual.TopLeft);
                content.Append("top right", visual.TopRight);
                content.Append("bottom left", visual.BottomLeft);
                content.Append("bottom right", visual.BottomRight);
            }
        content.Append("has overlays", Overlays is null ? 0 : 1);
        if (Overlays is not null)
            content.Append("overlays", Convert.FromHexString(Overlays.ContentIdentity));
    });

    /// <summary>
    /// Resolves the compiled command and its installed blocks from the same collision kind and behaviour. Callers
    /// cannot pair a drawable command with another block's absent artwork; unowned pairs retain the no-reveal result.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, RoomBlockBehavior behavior)
    {
        if (XrayRevealTable.Find(type, behavior) is not { } retail) return null;
        if (!IsDrawable(retail.Command)) return retail;
        XrayRevealVisualBlocks visual = blocks[Index(type, behavior)] ??
            throw new InvalidDataException($"Missing X-ray visual {type}/behaviour {behavior}.");
        return retail with
        {
            TopLeft = visual.TopLeft,
            TopRight = visual.TopRight,
            BottomLeft = visual.BottomLeft,
            BottomRight = visual.BottomRight,
        };
    }

    /// <summary>Rejects reveal blocks outside the tileset's block set, as an appearance of each would.</summary>
    private static void ValidateBlocks(XrayRevealVisualBlocks visual)
    {
        foreach (int block in (ReadOnlySpan<int>)[visual.TopLeft, visual.TopRight, visual.BottomLeft, visual.BottomRight])
            _ = RoomBlockAppearance.Of(block);
    }

    /// <summary>Whether this compiled command carries authored visual blocks.</summary>
    /// <remarks>
    /// Issue #1036: the pinned NTSC J/U v1.0 ROM has seven reachable
    /// revealed-block commands. $91:CF36, CF3E, CF4E, CF62, and CF6F
    /// copy one, Brinstar-only one, wide, tall, and square operand tiles;
    /// $91:CE79 and CEBB traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other ushort. The
    /// independent 16-by-256 native reveal oracle finds all seven commands;
    /// its drawable behaviour groups total 305, matching the installed visual
    /// catalog's exhaustive coverage assertion. Named membership is
    /// clearer than a pointer interval, since the two extension routines
    /// precede the copy commands but are not drawable.
    /// </remarks>
    public static bool IsDrawable(XrayRevealCommand command) => command is
        XrayRevealCommand.CopyOne or XrayRevealCommand.CopyWide or
        XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare or
        XrayRevealCommand.CopyBrinstar;
}
