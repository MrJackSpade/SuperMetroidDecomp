namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a probe meets among the hitboxes of the frame an enemy shows (#2558): the first hitbox it overlaps, whose reaction
/// runs, or no hitbox at all. A contact that meets no hitbox carries no reaction.
/// </summary>
internal abstract partial record EnemyHitboxContact
{
    /// <summary>Only the two contacts below exist.</summary>
    private EnemyHitboxContact()
    {
    }

    /// <summary>The probe overlaps none of the frame's hitboxes.</summary>
    internal static EnemyHitboxContact Missed { get; } = new MissedHitboxes();

    /// <summary>The probe overlaps a hitbox whose <paramref name="reaction"/> runs.</summary>
    /// <param name="reaction">The overlapped hitbox's touch or shot reaction, as the probe selects.</param>
    internal static EnemyHitboxContact Struck(EnemyReaction reaction) => new StruckHitbox(Ensure.NotNull(reaction));

    /// <summary>The probe overlaps none of the frame's hitboxes.</summary>
    internal sealed partial record MissedHitboxes : EnemyHitboxContact;

    /// <summary>The probe overlaps a hitbox; <paramref name="Reaction"/> is the reaction that runs.</summary>
    /// <param name="Reaction">The overlapped hitbox's touch or shot reaction, as the probe selects.</param>
    internal sealed partial record StruckHitbox(EnemyReaction Reaction) : EnemyHitboxContact;
}
