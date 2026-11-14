using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>
/// The installed blocks every X-ray copy reveal shows, and the installed item and special-room overlays. The game's
/// collision and behaviour lookup, copy dimensions, extension traversal and Brinstar-only condition remain compiled; there
/// is no compiled copy of the blocks (#1596), the block an extension chain leaving the room backward shows included (#1615).
/// </summary>
public sealed class InstalledXrayReveals
{
    /// <summary>Installed reveal blocks keyed by collision kind and reaction.</summary>
    /// <remarks>
    /// Issue #1037: of every collision kind and each reaction it reads (#1722), the game's data has 42 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates. The player edits the blocks in reveals.json while
    /// the shape and copy dimensions remain fixed, so no single formula can reproduce every valid installation: retain
    /// this bounded indexed store for the installed visual data.
    /// </remarks>
    internal readonly Dictionary<XrayRevealVisualKey, XrayRevealVisualBlocks> blocks = [];

    /// <summary>Creates the complete installed reveal blocks for every drawable reveal rule.</summary>
    /// <param name="entries">Exactly one set of blocks for each drawable collision kind and behaviour; shape selection and copy geometry remain fixed.</param>
    /// <param name="overlays">The installed item and special-room overlays.</param>
    /// <param name="extensionExitBlock">The block an extension chain shows when it leaves the room backward.</param>
    /// <exception cref="ArgumentNullException">The entry sequence or the overlays are null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, shows other blocks than its rule's shape copies, duplicates another pair, or leaves drawable coverage incomplete.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An entry or the extension exit names a block outside the tileset's block set.</exception>
    public InstalledXrayReveals(IEnumerable<XrayRevealVisualDefinition> entries, InstalledXrayOverlays overlays, int extensionExitBlock)
    {
        Ensure.NotNull(entries);
        Overlays = Ensure.NotNull(overlays);
        _ = RoomBlockAppearance.Of(extensionExitBlock);
        ExtensionExitBlock = extensionExitBlock;
        foreach ((RoomCollisionType type, Enum? reaction, XrayRevealVisualBlocks visual) in entries)
        {
            if (!IsReadingOf(type, reaction) || XrayReveals.Find(type, reaction) is not { } shape || !IsDrawable(shape))
                throw new InvalidDataException(
                    $"X-ray visual {type}/reaction {reaction} has no drawable reveal rule.");
            if (visual != XrayRevealVisualBlocks.ForShape(shape, visual.InReadingOrder))
                throw new InvalidDataException(
                    $"X-ray visual {type}/reaction {reaction} shows other blocks than its {shape} reveal copies.");
            ValidateBlocks(visual);
            if (!blocks.TryAdd(new XrayRevealVisualKey(type, reaction), visual))
                throw new InvalidDataException(
                    $"X-ray visual {type}/reaction {reaction} appears more than once.");
        }

        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        foreach (Enum? reaction in BlockReactionFamilies.ReadingsOf(type))
        {
            bool drawable = XrayReveals.Find(type, reaction) is { } shape &&
                IsDrawable(shape);
            if (drawable != blocks.ContainsKey(new XrayRevealVisualKey(type, reaction)))
                throw new InvalidDataException(
                    $"X-ray visual catalog does not cover reveal rule {type}/reaction {reaction}.");
        }
    }

    /// <summary>The installed item and special-room overlays.</summary>
    public InstalledXrayOverlays Overlays { get; }

    /// <summary>The installed block an extension chain shows when its next step leaves the room backward.</summary>
    public int ExtensionExitBlock { get; }

    /// <summary>
    /// Resolves the compiled shape and its installed blocks from the same collision kind and behaviour. Callers
    /// cannot pair a drawable shape with another block's absent artwork; unowned pairs retain the no-reveal result, and
    /// an extension shape has no blocks.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, RoomBlockBehavior behavior)
    {
        if (XrayRevealTable.Find(type, behavior) is not { } shape) return null;
        if (!IsDrawable(shape)) return new XrayRevealDefinition(shape, null);
        if (!blocks.TryGetValue(new XrayRevealVisualKey(type, behavior.ReactionUnder(type)), out XrayRevealVisualBlocks visual))
            throw new InvalidDataException($"Missing X-ray visual {type}/behaviour {behavior}.");
        return new XrayRevealDefinition(shape, visual);
    }

    /// <summary>Whether <paramref name="reaction"/> is a reaction <paramref name="type"/> reads, or null for a kind that reads none.</summary>
    private static bool IsReadingOf(RoomCollisionType type, Enum? reaction) => BlockReactionFamilies.ReadingsOf(type).Contains(reaction);

    /// <summary>Rejects reveal blocks outside the tileset's block set, as an appearance of each would.</summary>
    private static void ValidateBlocks(XrayRevealVisualBlocks visual)
    {
        foreach (int block in visual.InReadingOrder)
            _ = RoomBlockAppearance.Of(block);
    }

    /// <summary>Whether this compiled shape carries authored visual blocks.</summary>
    /// <remarks>
    /// Issue #1036: the game's data has seven reachable
    /// revealed-block shapes. Five
    /// copy one, Brinstar-only one, wide, tall, and square blocks;
    /// and two traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other shape. The
    /// independent 16-by-256 reveal oracle finds all seven shapes;
    /// its drawable behaviour groups total 305, matching the installed visual
    /// catalog's exhaustive coverage assertion. Named membership is
    /// clearer than a range of the shapes' layout, since the two extension shapes
    /// precede the copy shapes but are not drawable.
    /// </remarks>
    public static bool IsDrawable(XrayRevealShape shape) => shape is
        XrayRevealShape.CopyOne or XrayRevealShape.CopyWide or
        XrayRevealShape.CopyTall or XrayRevealShape.CopySquare or
        XrayRevealShape.CopyBrinstar;
}
