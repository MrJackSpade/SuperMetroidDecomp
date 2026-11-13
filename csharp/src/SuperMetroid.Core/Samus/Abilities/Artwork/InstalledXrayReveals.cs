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
    /// Issue #1037: of every collision kind's reading of every reaction, the game's data has 305 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates. The player edits the blocks in reveals.json while
    /// the command and copy dimensions remain fixed, so no single formula can reproduce every valid installation: retain
    /// this bounded indexed store for the installed visual data.
    /// </remarks>
    internal readonly Dictionary<XrayRevealVisualKey, XrayRevealVisualBlocks> blocks = [];

    /// <summary>Creates the complete installed reveal blocks for every drawable reveal rule.</summary>
    /// <param name="entries">Exactly one set of blocks for each drawable collision kind and behaviour; command selection and copy geometry remain fixed.</param>
    /// <param name="overlays">The installed item and special-room overlays.</param>
    /// <param name="extensionExitBlock">The block an extension chain shows when it leaves the room backward.</param>
    /// <exception cref="ArgumentNullException">The entry sequence or the overlays are null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, duplicates another pair, or leaves drawable coverage incomplete.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An entry or the extension exit names a block outside the tileset's block set.</exception>
    public InstalledXrayReveals(IEnumerable<XrayRevealVisualDefinition> entries, InstalledXrayOverlays overlays, int extensionExitBlock)
    {
        Ensure.NotNull(entries);
        Overlays = Ensure.NotNull(overlays);
        _ = RoomBlockAppearance.Of(extensionExitBlock);
        ExtensionExitBlock = extensionExitBlock;
        foreach ((RoomCollisionType type, BlockReaction reaction, XrayRevealVisualBlocks visual) in entries)
        {
            if (XrayRevealTable.Find(type, RoomBlockBehavior.Reacting(reaction)) is not { } command || !IsDrawable(command))
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
            bool drawable = XrayRevealTable.Find(type, RoomBlockBehavior.Reacting(reaction)) is { } command &&
                IsDrawable(command);
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
    /// Resolves the compiled command and its installed blocks from the same collision kind and behaviour. Callers
    /// cannot pair a drawable command with another block's absent artwork; unowned pairs retain the no-reveal result, and
    /// an extension command has no blocks.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, RoomBlockBehavior behavior)
    {
        if (XrayRevealTable.Find(type, behavior) is not { } command) return null;
        if (!IsDrawable(command)) return new XrayRevealDefinition(command, 0, 0, 0, 0);
        if (!blocks.TryGetValue(new XrayRevealVisualKey(type, behavior.Reaction), out XrayRevealVisualBlocks visual))
            throw new InvalidDataException($"Missing X-ray visual {type}/behaviour {behavior}.");
        return new XrayRevealDefinition(command, visual.TopLeft, visual.TopRight, visual.BottomLeft, visual.BottomRight);
    }

    /// <summary>Rejects reveal blocks outside the tileset's block set, as an appearance of each would.</summary>
    private static void ValidateBlocks(XrayRevealVisualBlocks visual)
    {
        foreach (int block in (ReadOnlySpan<int>)[visual.TopLeft, visual.TopRight, visual.BottomLeft, visual.BottomRight])
            _ = RoomBlockAppearance.Of(block);
    }

    /// <summary>Whether this compiled command carries authored visual blocks.</summary>
    /// <remarks>
    /// Issue #1036: the game's data has seven reachable
    /// revealed-block commands. Five
    /// copy one, Brinstar-only one, wide, tall, and square operand tiles;
    /// and two traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other command. The
    /// independent 16-by-256 reveal oracle finds all seven commands;
    /// its drawable behaviour groups total 305, matching the installed visual
    /// catalog's exhaustive coverage assertion. Named membership is
    /// clearer than a range of the commands' layout, since the two extension commands
    /// precede the copy commands but are not drawable.
    /// </remarks>
    public static bool IsDrawable(XrayRevealCommand command) => command is
        XrayRevealCommand.CopyOne or XrayRevealCommand.CopyWide or
        XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare or
        XrayRevealCommand.CopyBrinstar;
}
