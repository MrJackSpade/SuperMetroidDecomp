namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="GrappleReaction"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class GrappleReactionKeys
{
    /// <summary>Each GrappleReaction member's installed key.</summary>
    internal static InstalledKeys<GrappleReaction> Installed { get; } = new(new Dictionary<GrappleReaction, string>
    {
        [GrappleReaction.Anchor] = "Anchor",
        [GrappleReaction.RespawningBreakableAnchor] = "RespawningBreakableAnchor",
        [GrappleReaction.PermanentBreakableAnchor] = "PermanentBreakableAnchor",
        [GrappleReaction.UnrevealedAnchor] = "UnrevealedAnchor",
        [GrappleReaction.Rejected] = "Rejected",
    });

    /// <summary>The GrappleReaction members in the order the installation and a kind's reaction list name them (#2567), written out beside the keys so reordering the members never reorders them.</summary>
    internal static IReadOnlyList<GrappleReaction> ListedOrder { get; } =
    [
        GrappleReaction.Anchor,
        GrappleReaction.RespawningBreakableAnchor,
        GrappleReaction.PermanentBreakableAnchor,
        GrappleReaction.UnrevealedAnchor,
        GrappleReaction.Rejected,
    ];
}
