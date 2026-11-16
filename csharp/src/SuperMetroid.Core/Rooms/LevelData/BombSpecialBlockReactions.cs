using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly SpecialBlockBombRunOn<SpecialBlockReaction, BlockReactionChange> RunOn =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the no-op reaction that only holds its place for an update,
    /// for a special block with <paramref name="reaction"/>. Brinstar's own Speed Booster blocks are revealed as Speed
    /// Booster blocks; the import resolves the same entries of the other areas' tables to plain solid blocks (#1805).
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no special-block reaction.</exception>
    internal static BlockReactionChange ChangeFor(SpecialBlockReaction? reaction)
    {
        SpecialBlockReaction read = reaction ??
            throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The block has no special-block reaction for a bomb.");
        if (RunOn.TryShowAsSpeedBlock(read, out BlockReactionChange? shown))
            return shown;
        return read switch
        {
            SpecialBlockReaction.RespawningCrumble1x1 or SpecialBlockReaction.RespawningCrumble2x1 or
                SpecialBlockReaction.RespawningCrumble1x2 or SpecialBlockReaction.RespawningCrumble2x2 or
                SpecialBlockReaction.PermanentCrumble1x1 or SpecialBlockReaction.PermanentCrumble2x1 or
                SpecialBlockReaction.PermanentCrumble1x2 or SpecialBlockReaction.PermanentCrumble2x2 =>
                BlockReactionChanges.CrumbleReveal(read),
            SpecialBlockReaction.RespawningSpeedBlock or SpecialBlockReaction.PermanentSpeedBlock or
                SpecialBlockReaction.SlowRespawningSpeedBlock or SpecialBlockReaction.SlowPermanentSpeedBlock or
                SpecialBlockReaction.BrinstarRespawningSpeedBlock or SpecialBlockReaction.BrinstarPermanentSpeedBlock =>
                SpeedBoosterBlockObjectDefinitions.BombReveal,
            SpecialBlockReaction.XrayRevealedSolid or SpecialBlockReaction.CollectibleTrigger or
                SpecialBlockReaction.MapStationRight or SpecialBlockReaction.MapStationLeft or
                SpecialBlockReaction.EnergyStationRight or SpecialBlockReaction.EnergyStationLeft or
                SpecialBlockReaction.MissileStationRight or SpecialBlockReaction.MissileStationLeft or
                SpecialBlockReaction.SaveStationFloor or SpecialBlockReaction.Solid or
                SpecialBlockReaction.WreckedShipChozoHand or SpecialBlockReaction.LowerNorfairChozoHand =>
                BlockReactionChange.MadeAtSpawn,
            SpecialBlockReaction.BombRevealsSpeedBlock => throw new InvalidOperationException(
                $"{nameof(SpecialBlockReaction.BombRevealsSpeedBlock)} is shown as a Speed Booster block by the run-on."),
            _ => throw new InvalidOperationException($"Undefined {nameof(SpecialBlockReaction)} {read}."),
        };
    }
}
