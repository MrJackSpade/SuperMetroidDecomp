using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values and helpers of <see cref="CrocomireReactions"/> that only verification reads.</summary>
internal static class CrocomireReactionsConstants
{
    /// <summary>
    /// The shot reaction his definition names (#2558): his parts collide by their frames' hitboxes, whose reactions
    /// run, so a shot or bomb that meets him by his radius has no response of his.
    /// </summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.CrocomireShot;
}
