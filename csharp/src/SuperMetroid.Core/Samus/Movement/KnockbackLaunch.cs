namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How a knockback starts: its horizontal direction and timer (#633).</summary>
public sealed record KnockbackLaunch
{
    /// <summary>The direction the knockback carries Samus horizontally.</summary>
    public required KnockbackHorizontalDirection KnockbackXDirection { get; init; }

    /// <summary>Nonzero producer-owned hurt countdown in gameplay updates; normal contact supplies five, while scripted intro Rinka contact supplies eleven.</summary>
    public NativeCountdown KnockbackTimer { get; init; } = NativeCountdown.Start(5);

    /// <summary>
    /// The launch a damage producer left pending on Samus: the direction its contact published and the hurt
    /// countdown still running. Samus still stores the direction as the cartridge's left/right word, zero
    /// for left and one for right; any other word is corrupt state.
    /// </summary>
    internal static KnockbackLaunch FromPendingHit(SamusState samus)
    {
        Ensure.NotNull(samus);
        return new KnockbackLaunch
        {
            KnockbackXDirection = samus.KnockbackXDirection switch
            {
                0 => KnockbackHorizontalDirection.Left,
                1 => KnockbackHorizontalDirection.Right,
                _ => throw new InvalidDataException(
                    $"Pending knockback horizontal direction {samus.KnockbackXDirection} is neither left (0) nor right (1)."),
            },
            KnockbackTimer = samus.KnockbackTimer,
        };
    }
}
