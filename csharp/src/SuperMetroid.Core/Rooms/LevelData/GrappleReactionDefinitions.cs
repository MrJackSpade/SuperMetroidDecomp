namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What the grapple blocks' reactions do (<see cref="GrappleReaction"/>).</summary>
internal static class GrappleReactionDefinitions
{
    /// <summary>The breakable grapple blocks' reactions, each with whether the block grows back once the beam breaks it.</summary>
    internal static readonly IReadOnlyDictionary<GrappleReaction, bool> BreakableAnchorsGrowBack = new Dictionary<GrappleReaction, bool>
    {
        [GrappleReaction.RespawningBreakableAnchor] = true,
        [GrappleReaction.PermanentBreakableAnchor] = false,
    };
}
