namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How a knockback starts: its horizontal direction and timer (#633).</summary>
public sealed record KnockbackLaunch
{
    /// <summary>The direction the knockback carries Samus horizontally.</summary>
    public required KnockbackHorizontalDirection KnockbackXDirection { get; init; }

    /// <summary>Nonzero producer-owned hurt countdown in gameplay updates; normal contact supplies five, while scripted intro Rinka contact supplies eleven.</summary>
    public int KnockbackTimer { get; init; } = 5;

    /// <summary>
    /// The launch a damage producer left pending on Samus: the direction its contact published and the hurt
    /// countdown still running. Samus still stores the direction as zero for left and one for right; any other value is
    /// corrupt state.
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
