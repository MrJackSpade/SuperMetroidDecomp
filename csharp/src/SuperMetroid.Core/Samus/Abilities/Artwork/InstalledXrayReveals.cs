using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>
/// The installed blocks every X-ray copy reveal shows, and the installed item and special-room overlays. The game's
/// collision and behaviour lookup, copy dimensions, extension traversal and Brinstar-only condition are the game's own; there
/// is no other copy of the blocks (#1596). An extension chain leaving the room shows no block of its own (#1792).
/// </summary>
public sealed class InstalledXrayReveals
{
    /// <summary>Installed reveal blocks keyed by collision kind and reaction.</summary>
    /// <remarks>
    /// Of every collision kind and each reaction it reads (#1722), the constructor requires exactly the pairs whose
    /// reveal draws blocks and rejects duplicates. The player edits the blocks in reveals.json while
    /// the shape and copy dimensions remain fixed, so no single formula can reproduce every valid installation: retain
    /// this bounded indexed store for the installed visual data.
    /// </remarks>
    internal readonly Dictionary<XrayRevealVisualKey, XrayRevealVisualBlocks> blocks = [];

    /// <summary>Creates the complete installed reveal blocks for every drawable reveal rule.</summary>
    /// <param name="entries">Exactly one set of blocks for each drawable collision kind and behaviour; shape selection and copy geometry remain fixed.</param>
    /// <param name="overlays">The installed item and special-room overlays.</param>
    /// <exception cref="ArgumentNullException">The entry sequence or the overlays are null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, shows other blocks than its rule's shape copies, duplicates another pair, or leaves drawable coverage incomplete.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An entry names a block outside the tileset's block set.</exception>
    public InstalledXrayReveals(IEnumerable<XrayRevealVisualDefinition> entries, InstalledXrayOverlays overlays)
    {
        Ensure.NotNull(entries);
        Overlays = Ensure.NotNull(overlays);
        foreach ((RoomCollisionType type, BlockReaction? reaction, XrayRevealVisualBlocks visual) in entries)
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
        {
            IReadOnlyList<BlockReaction> readings = BlockReactionFamilies.ReadingsOf(type);
            // A kind that reads no reaction has one rule of its own, under no reaction.
            if (readings.Count == 0)
                RequireCovered(type, null);
            foreach (BlockReaction reaction in readings)
                RequireCovered(type, reaction);
        }

        void RequireCovered(RoomCollisionType type, BlockReaction? reaction)
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

    /// <summary>
    /// Resolves the reveal shape and its installed blocks from the same collision kind and behaviour. Nothing
    /// can pair a drawable shape with another block's absent artwork; unowned pairs retain the no-reveal result, and
    /// an extension shape has no blocks.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, RoomBlockBehavior behavior)
    {
        if (XrayReveals.Find(type, behavior) is not { } shape) return null;
        if (!IsDrawable(shape)) return new XrayRevealDefinition(shape, null);
        if (!blocks.TryGetValue(new XrayRevealVisualKey(type, behavior.ReactionUnder(type)), out XrayRevealVisualBlocks visual))
            throw new InvalidDataException($"Missing X-ray visual {type}/behaviour {behavior}.");
        return new XrayRevealDefinition(shape, visual);
    }

    /// <summary>Whether <paramref name="reaction"/> is a reaction <paramref name="type"/> reads, or null for a kind that reads none.</summary>
    private static bool IsReadingOf(RoomCollisionType type, BlockReaction? reaction) => reaction is { } read
        ? BlockReactionFamilies.ReadingsOf(type).Contains(read)
        : BlockReactionFamilies.ReadingsOf(type).Count == 0;

    /// <summary>Rejects a negative reveal block, as an appearance of it would; the tileset's sheet bounds the rest (#2100).</summary>
    private static void ValidateBlocks(XrayRevealVisualBlocks visual)
    {
        foreach (int block in visual.InReadingOrder)
            _ = RoomBlockAppearance.Of(block);
    }

    /// <summary>Whether this reveal shape carries authored visual blocks.</summary>
    /// <remarks>
    /// The game has seven reachable revealed-block shapes. Five copy one, Brinstar-only one, wide, tall and square
    /// blocks; the other two traverse vertical and horizontal extensions instead. A shape is drawable exactly when it is
    /// one of those five copy shapes.
    /// </remarks>
    public static bool IsDrawable(XrayRevealShape shape) => shape is
        XrayRevealShape.CopyOne or XrayRevealShape.CopyWide or
        XrayRevealShape.CopyTall or XrayRevealShape.CopySquare or
        XrayRevealShape.CopyBrinstar;
}
