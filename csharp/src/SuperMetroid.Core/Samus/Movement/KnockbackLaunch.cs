namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How a knockback starts: its horizontal direction and timer (#633).</summary>
public sealed partial record KnockbackLaunch
{
    /// <summary>The direction the knockback carries Samus horizontally.</summary>
    public required KnockbackHorizontalDirection KnockbackXDirection { get; init; }

    /// <summary>
    /// The hurt countdown in gameplay updates, which the producer of the hit owns: an enemy's touch supplies
    /// <see cref="Enemies.EnemyCollisionDefinitions.SamusTouchKnockbackUpdates"/>, the intro Rinka's strike its own.
    /// </summary>
    public required int KnockbackTimer { get; init; }

    /// <summary>
    /// The launch Samus's hurt state gives: the knockback direction the hit's contact set on her, and the hurt countdown
    /// still running. Both are her state from the hit until her knockback ends, which the knockback's movement reads as it
    /// runs; the start reads them as they stand.
    /// </summary>
    internal static KnockbackLaunch FromHurtState(SamusState samus)
    {
        Ensure.NotNull(samus);
        return new KnockbackLaunch
        {
            KnockbackXDirection = samus.KnockbackXDirection,
            KnockbackTimer = samus.KnockbackHold.UpdatesLeft,
        };
    }
}
