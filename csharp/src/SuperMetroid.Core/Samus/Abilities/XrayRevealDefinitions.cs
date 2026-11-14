using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Compiled form of the complete revealed-block check: the reveal shape each collision kind and reaction selects. The
/// shapes keep the game's copy dimensions, extension traversal and Brinstar-only behaviour; the blocks each copy shows
/// are installed pictures (#1596).
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>The reveal shape each special-air reaction selects: the scroll trigger alone (#1028).</summary>
    internal static readonly IReadOnlyDictionary<SpecialAirReaction, XrayRevealShape> SpecialAirReveals = new Dictionary<SpecialAirReaction, XrayRevealShape>
    {
        [SpecialAirReaction.ScrollTrigger] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each spike-block reaction selects: the revealed spike alone (#1030).</summary>
    internal static readonly IReadOnlyDictionary<SpikeReaction, XrayRevealShape> SpikeBlockReveals = new Dictionary<SpikeReaction, XrayRevealShape>
    {
        [SpikeReaction.XrayRevealed] = XrayRevealShape.CopyOne,
    };

    /// <summary>
    /// The reveal shape each special-block reaction selects (#1032): each crumble block by its size, the revealed solid
    /// block and the Speed Booster blocks every area has as one block, and Brinstar's Speed Booster blocks by the
    /// Brinstar-only shape.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialBlockReaction, XrayRevealShape> SpecialBlockReveals = new Dictionary<SpecialBlockReaction, XrayRevealShape>
    {
        [SpecialBlockReaction.RespawningCrumble1x1] = XrayRevealShape.CopyOne,
        [SpecialBlockReaction.PermanentCrumble1x1] = XrayRevealShape.CopyOne,
        [SpecialBlockReaction.XrayRevealedSolid] = XrayRevealShape.CopyOne,
        [SpecialBlockReaction.RespawningCrumble2x1] = XrayRevealShape.CopyWide,
        [SpecialBlockReaction.PermanentCrumble2x1] = XrayRevealShape.CopyWide,
        [SpecialBlockReaction.RespawningCrumble1x2] = XrayRevealShape.CopyTall,
        [SpecialBlockReaction.PermanentCrumble1x2] = XrayRevealShape.CopyTall,
        [SpecialBlockReaction.RespawningCrumble2x2] = XrayRevealShape.CopySquare,
        [SpecialBlockReaction.PermanentCrumble2x2] = XrayRevealShape.CopySquare,
        [SpecialBlockReaction.RespawningSpeedBlock] = XrayRevealShape.CopyOne,
        [SpecialBlockReaction.PermanentSpeedBlock] = XrayRevealShape.CopyOne,
        [SpecialBlockReaction.SlowRespawningSpeedBlock] = XrayRevealShape.CopyBrinstar,
        [SpecialBlockReaction.SlowPermanentSpeedBlock] = XrayRevealShape.CopyBrinstar,
        [SpecialBlockReaction.BrinstarRespawningSpeedBlock] = XrayRevealShape.CopyBrinstar,
        [SpecialBlockReaction.BrinstarPermanentSpeedBlock] = XrayRevealShape.CopyBrinstar,
    };

    /// <summary>
    /// The reveal shape each shootable-block reaction selects (#1033): each break by its size, and the Power Bomb and Super
    /// Missile blocks and the empty reaction as one block.
    /// </summary>
    internal static readonly IReadOnlyDictionary<ShootableReaction, XrayRevealShape> ShootableBlockReveals = new Dictionary<ShootableReaction, XrayRevealShape>
    {
        [ShootableReaction.RespawningBreak1x1] = XrayRevealShape.CopyOne,
        [ShootableReaction.PermanentBreak1x1] = XrayRevealShape.CopyOne,
        [ShootableReaction.RespawningBreak2x1] = XrayRevealShape.CopyWide,
        [ShootableReaction.PermanentBreak2x1] = XrayRevealShape.CopyWide,
        [ShootableReaction.RespawningBreak1x2] = XrayRevealShape.CopyTall,
        [ShootableReaction.PermanentBreak1x2] = XrayRevealShape.CopyTall,
        [ShootableReaction.RespawningBreak2x2] = XrayRevealShape.CopySquare,
        [ShootableReaction.PermanentBreak2x2] = XrayRevealShape.CopySquare,
        [ShootableReaction.RespawningPowerBombBlock] = XrayRevealShape.CopyOne,
        [ShootableReaction.PermanentPowerBombBlock] = XrayRevealShape.CopyOne,
        [ShootableReaction.RespawningSuperMissileBlock] = XrayRevealShape.CopyOne,
        [ShootableReaction.PermanentSuperMissileBlock] = XrayRevealShape.CopyOne,
        [ShootableReaction.Nothing] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each grapple-block reaction selects (#1034): the revealed anchor and the breakable ones.</summary>
    internal static readonly IReadOnlyDictionary<GrappleReaction, XrayRevealShape> GrappleBlockReveals = new Dictionary<GrappleReaction, XrayRevealShape>
    {
        [GrappleReaction.Anchor] = XrayRevealShape.CopyOne,
        [GrappleReaction.RespawningBreakableAnchor] = XrayRevealShape.CopyOne,
        [GrappleReaction.PermanentBreakableAnchor] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each bombable-block reaction selects (#1035): each break by its size.</summary>
    internal static readonly IReadOnlyDictionary<BombableReaction, XrayRevealShape> BombableBlockReveals = new Dictionary<BombableReaction, XrayRevealShape>
    {
        [BombableReaction.RespawningBreak1x1] = XrayRevealShape.CopyOne,
        [BombableReaction.PermanentBreak1x1] = XrayRevealShape.CopyOne,
        [BombableReaction.RespawningBreak2x1] = XrayRevealShape.CopyWide,
        [BombableReaction.PermanentBreak2x1] = XrayRevealShape.CopyWide,
        [BombableReaction.RespawningBreak1x2] = XrayRevealShape.CopyTall,
        [BombableReaction.PermanentBreak1x2] = XrayRevealShape.CopyTall,
        [BombableReaction.RespawningBreak2x2] = XrayRevealShape.CopySquare,
        [BombableReaction.PermanentBreak2x2] = XrayRevealShape.CopySquare,
    };
}
