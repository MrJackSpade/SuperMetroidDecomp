namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at each special block's reaction (#1742).</summary>
internal static partial class BombSpecialBlockReactionDefinitions
{
    /// <summary>
    /// Each special-block reaction's response to a normal bomb: the crumble blocks show their reveal, every Speed Booster
    /// block the Speed Booster block's, and the other blocks hold their place for an update. The plain solid block a bomb
    /// shows as a Speed Booster block is the compatibility service's (<see cref="BombSpecialBlockReactions"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialBlockReaction, SpecialBlockBombResponse> Responses =
        new Dictionary<SpecialBlockReaction, SpecialBlockBombResponse>
        {
            [SpecialBlockReaction.RespawningCrumble1x1] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.RespawningCrumble2x1] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.RespawningCrumble1x2] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.RespawningCrumble2x2] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.PermanentCrumble1x1] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.PermanentCrumble2x1] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.PermanentCrumble1x2] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.PermanentCrumble2x2] = SpecialBlockBombResponse.CrumbleReveal,
            [SpecialBlockReaction.RespawningSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.PermanentSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.SlowRespawningSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.SlowPermanentSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.BrinstarRespawningSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.BrinstarPermanentSpeedBlock] = SpecialBlockBombResponse.SpeedBlockReveal,
            [SpecialBlockReaction.XrayRevealedSolid] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.CollectibleTrigger] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.MapStationRight] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.MapStationLeft] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.EnergyStationRight] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.EnergyStationLeft] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.MissileStationRight] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.MissileStationLeft] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.SaveStationFloor] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.Solid] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.WreckedShipChozoHand] = SpecialBlockBombResponse.Nothing,
            [SpecialBlockReaction.LowerNorfairChozoHand] = SpecialBlockBombResponse.Nothing,
        };
}
