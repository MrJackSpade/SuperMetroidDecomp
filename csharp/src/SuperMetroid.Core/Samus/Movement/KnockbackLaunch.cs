namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How a knockback starts: its horizontal direction and timer (#633).</summary>
public sealed record KnockbackLaunch
{
    /// <summary>The direction the knockback carries Samus horizontally.</summary>
    public required KnockbackHorizontalDirection KnockbackXDirection { get; init; }

    /// <summary>
    /// The hurt countdown in gameplay updates, which the producer of the hit owns: an enemy's touch supplies
    /// <see cref="Enemies.EnemyCollisionDefinitions.SamusTouchKnockbackUpdates"/>, the intro Rinka's strike its own.
    /// </summary>
    public required int KnockbackTimer { get; init; }

    /// <summary>
    /// The launch a damage producer left pending on Samus: the direction its contact published, left or right, and the
    /// hurt countdown still running.
    /// </summary>
    internal static KnockbackLaunch FromPendingHit(SamusState samus)
    {
        Ensure.NotNull(samus);
        return new KnockbackLaunch
        {
            KnockbackXDirection = samus.KnockbackXDirection switch
            {
                KnockbackHorizontalDirection.Left => KnockbackHorizontalDirection.Left,
                KnockbackHorizontalDirection.Right => KnockbackHorizontalDirection.Right,
                _ => throw new InvalidDataException(
                    $"Pending knockback horizontal direction {samus.KnockbackXDirection} is neither left nor right."),
            },
            KnockbackTimer = samus.KnockbackTimer,
        };
    }
}
