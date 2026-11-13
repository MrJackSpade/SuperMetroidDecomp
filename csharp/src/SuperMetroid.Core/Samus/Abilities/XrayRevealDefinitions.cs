using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Compiled form of the complete revealed-block dispatcher: the command each collision kind and reaction selects. The
/// commands keep the game's copy dimensions, extension traversal and Brinstar-only behaviour; the blocks each copy shows
/// are installed pictures (#1596).
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>The only special-air reaction the table admits.</summary>
    /// <remarks>
    /// Issue #1028: the special-air table holds one entry for this behaviour and a terminator, then a copy-one command.
    /// The lookup compares the behaviour exactly, so equality with it is the complete bounded algorithm over
    /// 0..255; every other behaviour returns no reveal. The authored data and all 256 special-air cases in the
    /// independent X-ray reveal oracle agree.
    /// </remarks>
    internal static readonly BlockReaction SpecialAirRevealReaction = BlockReaction.Common(70);

    /// <summary>The only spike-block reaction the table admits.</summary>
    /// <remarks>
    /// Issue #1030: the spike-block table holds one entry for this behaviour and a terminator, then a copy-one command.
    /// The lookup compares the behaviour exactly, making equality with it the exact bounded rule over 0..255; other
    /// behaviour values return no reveal. The authored data and all 256 spike-block cases in the independent X-ray
    /// reveal oracle agree.
    /// </remarks>
    internal static readonly BlockReaction SpikeRevealReaction = BlockReaction.Common(14);

    /// <summary>The command each special-air reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> SpecialAirReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [SpecialAirRevealReaction] = XrayRevealCommand.CopyOne,
    };

    /// <summary>The command each spike-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> SpikeBlockReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [SpikeRevealReaction] = XrayRevealCommand.CopyOne,
    };

    /// <summary>The command each special-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> SpecialBlockReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [BlockReaction.Common(0)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(8)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(9)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(10)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(11)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(12)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(13)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealCommand.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealCommand.CopySquare,
        [BlockReaction.Common(14)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(15)] = XrayRevealCommand.CopyOne,
        [BlockReaction.AreaSpecific(2)] = XrayRevealCommand.CopyBrinstar,
        [BlockReaction.AreaSpecific(3)] = XrayRevealCommand.CopyBrinstar,
        [BlockReaction.AreaSpecific(4)] = XrayRevealCommand.CopyBrinstar,
        [BlockReaction.AreaSpecific(5)] = XrayRevealCommand.CopyBrinstar,
    };

    /// <summary>The command each shootable-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> ShootableBlockReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [BlockReaction.Common(0)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealCommand.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealCommand.CopySquare,
        [BlockReaction.Common(8)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(9)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(10)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(11)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(12)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(13)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(14)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(15)] = XrayRevealCommand.CopyOne,
    };

    /// <summary>The command each grapple-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> GrappleBlockReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [BlockReaction.Common(0)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(2)] = XrayRevealCommand.CopyOne,
    };

    /// <summary>The command each bombable-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealCommand> BombableBlockReveals = new Dictionary<BlockReaction, XrayRevealCommand>
    {
        [BlockReaction.Common(0)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealCommand.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealCommand.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealCommand.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealCommand.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealCommand.CopySquare,
    };
}
