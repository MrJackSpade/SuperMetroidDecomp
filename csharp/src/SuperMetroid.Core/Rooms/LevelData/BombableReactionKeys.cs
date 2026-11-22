namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="BombableReaction"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class BombableReactionKeys
{
    /// <summary>Each BombableReaction member's installed key.</summary>
    internal static InstalledKeys<BombableReaction> Installed { get; } = new(new Dictionary<BombableReaction, string>
    {
        [BombableReaction.RespawningBreak1x1] = "RespawningBreak1x1",
        [BombableReaction.RespawningBreak2x1] = "RespawningBreak2x1",
        [BombableReaction.RespawningBreak1x2] = "RespawningBreak1x2",
        [BombableReaction.RespawningBreak2x2] = "RespawningBreak2x2",
        [BombableReaction.PermanentBreak1x1] = "PermanentBreak1x1",
        [BombableReaction.PermanentBreak2x1] = "PermanentBreak2x1",
        [BombableReaction.PermanentBreak1x2] = "PermanentBreak1x2",
        [BombableReaction.PermanentBreak2x2] = "PermanentBreak2x2",
        [BombableReaction.Nothing] = "Nothing",
        [BombableReaction.Inert] = "Inert",
    });

    /// <summary>The BombableReaction members in the order the installation and a kind's reaction list name them (#2567), written out beside the keys so reordering the members never reorders them.</summary>
    internal static IReadOnlyList<BombableReaction> ListedOrder { get; } =
    [
        BombableReaction.RespawningBreak1x1,
        BombableReaction.RespawningBreak2x1,
        BombableReaction.RespawningBreak1x2,
        BombableReaction.RespawningBreak2x2,
        BombableReaction.PermanentBreak1x1,
        BombableReaction.PermanentBreak2x1,
        BombableReaction.PermanentBreak1x2,
        BombableReaction.PermanentBreak2x2,
        BombableReaction.Nothing,
        BombableReaction.Inert,
    ];
}
