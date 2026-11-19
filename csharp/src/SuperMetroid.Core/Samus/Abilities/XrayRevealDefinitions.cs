using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The complete revealed-block check: the reveal shape each collision kind and reaction selects. The
/// shapes keep the game's copy dimensions, extension traversal and Brinstar-only behaviour; the blocks each copy shows
/// are installed pictures (#1596).
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>The reveal shape each special-air reaction selects: the scroll trigger alone (#1028).</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpecialAirReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(SpecialAirReaction.ScrollTrigger)] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each spike-block reaction selects: the revealed spike alone (#1030).</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpikeBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(SpikeReaction.XrayRevealed)] = XrayRevealShape.CopyOne,
    };

    /// <summary>
    /// The reveal shape each special-block reaction selects (#1032): each crumble block by its size, the revealed solid
    /// block and the Speed Booster blocks every area has as one block, and Brinstar's Speed Booster blocks by the
    /// Brinstar-only shape.
    /// </summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> SpecialBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(SpecialBlockReaction.RespawningCrumble1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(SpecialBlockReaction.PermanentCrumble1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(SpecialBlockReaction.XrayRevealedSolid)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(SpecialBlockReaction.RespawningCrumble2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(SpecialBlockReaction.PermanentCrumble2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(SpecialBlockReaction.RespawningCrumble1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(SpecialBlockReaction.PermanentCrumble1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(SpecialBlockReaction.RespawningCrumble2x2)] = XrayRevealShape.CopySquare,
        [BlockReaction.Of(SpecialBlockReaction.PermanentCrumble2x2)] = XrayRevealShape.CopySquare,
        [BlockReaction.Of(SpecialBlockReaction.RespawningSpeedBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(SpecialBlockReaction.PermanentSpeedBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(SpecialBlockReaction.SlowRespawningSpeedBlock)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.Of(SpecialBlockReaction.SlowPermanentSpeedBlock)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.Of(SpecialBlockReaction.BrinstarRespawningSpeedBlock)] = XrayRevealShape.CopyBrinstar,
        [BlockReaction.Of(SpecialBlockReaction.BrinstarPermanentSpeedBlock)] = XrayRevealShape.CopyBrinstar,
    };

    /// <summary>
    /// The reveal shape each shootable-block reaction selects (#1033): each break by its size, and the Power Bomb and Super
    /// Missile blocks and the empty reaction as one block.
    /// </summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> ShootableBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(ShootableReaction.RespawningBreak1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.PermanentBreak1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.RespawningBreak2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(ShootableReaction.PermanentBreak2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(ShootableReaction.RespawningBreak1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(ShootableReaction.PermanentBreak1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(ShootableReaction.RespawningBreak2x2)] = XrayRevealShape.CopySquare,
        [BlockReaction.Of(ShootableReaction.PermanentBreak2x2)] = XrayRevealShape.CopySquare,
        [BlockReaction.Of(ShootableReaction.RespawningPowerBombBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.PermanentPowerBombBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.RespawningSuperMissileBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.PermanentSuperMissileBlock)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(ShootableReaction.Nothing)] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each grapple-block reaction selects (#1034): the revealed anchor and the breakable ones.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> GrappleBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(GrappleReaction.Anchor)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(GrappleReaction.RespawningBreakableAnchor)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(GrappleReaction.PermanentBreakableAnchor)] = XrayRevealShape.CopyOne,
    };

    /// <summary>The reveal shape each bombable-block reaction selects (#1035): each break by its size.</summary>
    internal static readonly IReadOnlyDictionary<BlockReaction, XrayRevealShape> BombableBlockReveals = new Dictionary<BlockReaction, XrayRevealShape>
    {
        [BlockReaction.Of(BombableReaction.RespawningBreak1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(BombableReaction.PermanentBreak1x1)] = XrayRevealShape.CopyOne,
        [BlockReaction.Of(BombableReaction.RespawningBreak2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(BombableReaction.PermanentBreak2x1)] = XrayRevealShape.CopyWide,
        [BlockReaction.Of(BombableReaction.RespawningBreak1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(BombableReaction.PermanentBreak1x2)] = XrayRevealShape.CopyTall,
        [BlockReaction.Of(BombableReaction.RespawningBreak2x2)] = XrayRevealShape.CopySquare,
        [BlockReaction.Of(BombableReaction.PermanentBreak2x2)] = XrayRevealShape.CopySquare,
    };
}
