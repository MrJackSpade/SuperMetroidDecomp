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
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpecialAirReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [SpecialAirRevealReaction] = XrayRevealShape.CopyOne,
    };

    /// <summary>The command each spike-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpikeBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [SpikeRevealReaction] = XrayRevealShape.CopyOne,
    };

    /// <summary>The command each special-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpecialBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Common(0)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(8)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(9)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(10)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(11)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(12)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(13)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealShape.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealShape.CopySquare,
        [BlockReaction.Common(14)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(15)] = XrayRevealShape.CopyOne,
        [BlockReaction.AreaSpecific(2)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.AreaSpecific(3)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.AreaSpecific(4)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.AreaSpecific(5)] = XrayRevealShape.CopyBrinstar,
    };

    /// <summary>The command each shootable-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> ShootableBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Common(0)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealShape.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealShape.CopySquare,
        [BlockReaction.Common(8)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(9)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(10)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(11)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(12)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(13)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(14)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(15)] = XrayRevealShape.CopyOne,
    };

    /// <summary>The command each grapple-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> GrappleBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Common(0)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(2)] = XrayRevealShape.CopyOne,
    };

    /// <summary>The command each bombable-block reaction selects.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> BombableBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Common(0)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(4)] = XrayRevealShape.CopyOne,
        [BlockReaction.Common(1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(5)] = XrayRevealShape.CopyWide,
        [BlockReaction.Common(2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(6)] = XrayRevealShape.CopyTall,
        [BlockReaction.Common(3)] = XrayRevealShape.CopySquare,
        [BlockReaction.Common(7)] = XrayRevealShape.CopySquare,
    };
}
