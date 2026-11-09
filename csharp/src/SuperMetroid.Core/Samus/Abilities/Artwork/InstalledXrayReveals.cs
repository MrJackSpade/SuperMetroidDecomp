using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>
/// Installed visual choices for X-ray blocks. The game's collision and behaviour lookup,
/// copy dimensions, extension traversal, and Brinstar-only condition remain compiled.
/// </summary>
public sealed class InstalledXrayReveals
{
    /// <summary>Installed reveal blocks keyed by collision kind and reaction.</summary>
    /// <remarks>
    /// Issue #1037: of every collision kind's reading of every reaction, the original data has 305 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates.
    /// Stock operands come from reveal command records and all
    /// 305 installed values match the independent oracle.
    /// The visual operands can also be replaced through reveals.json while
    /// the command and copy dimensions remain fixed. Therefore no
    /// single formula derived from the original data can reproduce every valid installation:
    /// retain this bounded indexed store for the selected visual data.
    /// </remarks>
    internal readonly Dictionary<XrayRevealVisualKey, XrayRevealVisualBlocks> blocks = [];

    /// <summary>Creates a complete visual replacement for every drawable reveal rule.</summary>
    /// <param name="entries">Exactly one set of blocks for each drawable collision kind and behaviour; command selection and copy geometry remain fixed.</param>
    /// <param name="overlays">Installed item/special-room visuals, or null for a noninstalled fixture.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, duplicates another pair, or leaves drawable coverage incomplete.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An entry names a block outside the tileset's block set.</exception>
    public InstalledXrayReveals(IEnumerable<XrayRevealVisualDefinition> entries, InstalledXrayOverlays? overlays = null)
    {
        Ensure.NotNull(entries);
        Overlays = overlays;
        foreach ((RoomCollisionType type, BlockReaction reaction, XrayRevealVisualBlocks visual) in entries)
        {
            XrayRevealDefinition? retail = XrayRevealTable.Find(type, RoomBlockBehavior.Reacting(reaction));
            if (retail is not { } definition || !IsDrawable(definition.Command))
                throw new InvalidDataException(
                    $"X-ray visual {type}/reaction {reaction} has no drawable reveal rule.");
            ValidateBlocks(visual);
            if (!blocks.TryAdd(new XrayRevealVisualKey(type, reaction), visual))
                throw new InvalidDataException(
                    $"X-ray visual {type}/reaction {reaction} appears more than once.");
        }

        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        foreach (BlockReaction reaction in BlockReaction.All)
        {
            bool drawable = XrayRevealTable.Find(type, RoomBlockBehavior.Reacting(reaction)) is { } definition &&
                IsDrawable(definition.Command);
            if (drawable != blocks.ContainsKey(new XrayRevealVisualKey(type, reaction)))
                throw new InvalidDataException(
                    $"X-ray visual catalog does not cover reveal rule {type}/reaction {reaction}.");
        }
    }

    /// <summary>Installed item and special-room visuals; null only in noninstalled fixtures.</summary>
    public InstalledXrayOverlays? Overlays { get; }

    /// <summary>
    /// Resolves the compiled command and its installed blocks from the same collision kind and behaviour. Callers
    /// cannot pair a drawable command with another block's absent artwork; unowned pairs retain the no-reveal result.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, RoomBlockBehavior behavior)
    {
        if (XrayRevealTable.Find(type, behavior) is not { } retail) return null;
        if (!IsDrawable(retail.Command)) return retail;
        if (!blocks.TryGetValue(new XrayRevealVisualKey(type, behavior.Reaction), out XrayRevealVisualBlocks visual))
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
    /// Issue #1036: the original data has seven reachable
    /// revealed-block commands. Five
    /// copy one, Brinstar-only one, wide, tall, and square operand tiles;
    /// and two traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other ushort. The
    /// independent 16-by-256 reveal oracle finds all seven commands;
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
