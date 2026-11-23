namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What each spike reaction is to the grapple beam and to enemies (<see cref="SpikeReactions"/>, #1722).</summary>
internal static partial class SpikeReactionDefinitions
{
    /// <summary>The spike reactions the grapple beam passes through rather than stopping on.</summary>
    internal static readonly SpikeReaction[] LettingGrappleThrough = [SpikeReaction.HarmlessLetsGrappleThrough, SpikeReaction.EnemyBreakableLetsGrappleThrough];

    /// <summary>The spike reactions whose blocks an enemy breaks rather than meets as solid.</summary>
    internal static readonly SpikeReaction[] EnemyBreakable = [SpikeReaction.EnemyBreakable, SpikeReaction.EnemyBreakableLetsGrappleThrough];
}
