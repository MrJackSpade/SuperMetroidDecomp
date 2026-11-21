namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// What a hit's knockback does to Samus (#2560): a closed choice of no knockback, a knockback moving her in its direction,
/// and a released knockback, which has returned her movement to her pose while its direction stays until the hit expires
/// (a damage boost, Crystal Flash's drain and her death release it). The hit's countdown, which its producer starts before
/// any knockback, is <see cref="SamusState.KnockbackHold"/>.
/// </summary>
public abstract partial record SamusKnockback
{
    /// <summary>Only the three cases below are knockbacks.</summary>
    private SamusKnockback()
    {
    }

    /// <summary>No knockback is pending or moving Samus.</summary>
    public static SamusKnockback None { get; } = new NoKnockback();

    /// <summary>Which way the knockback sends Samus; <see cref="SamusKnockbackDirection.None"/> for no knockback.</summary>
    public abstract SamusKnockbackDirection Direction { get; }

    /// <summary>Whether the knockback moves Samus (<see cref="SamusMovementOwner.Knockback"/>).</summary>
    public bool MovesSamus => this is MovingSamus;

    /// <summary>A knockback moving Samus <paramref name="direction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is no way a knockback moves her.</exception>
    public static SamusKnockback Moving(SamusKnockbackDirection direction) => new MovingSamus(direction is
        SamusKnockbackDirection.UpLeft or SamusKnockbackDirection.UpRight or SamusKnockbackDirection.DownLeft or SamusKnockbackDirection.DownRight
        ? direction
        : throw new ArgumentOutOfRangeException(nameof(direction), direction, "A knockback moves Samus one of four ways."));

    /// <summary>The knockback with Samus's movement returned to her pose; its direction stays until the hit expires.</summary>
    public SamusKnockback Released() => this is MovingSamus moving ? new ReleasedSamus(moving.Way) : this;

    /// <summary>No knockback.</summary>
    private sealed partial record NoKnockback : SamusKnockback
    {
        /// <inheritdoc/>
        public override SamusKnockbackDirection Direction => SamusKnockbackDirection.None;
    }

    /// <summary>A knockback moving Samus <paramref name="Way"/>.</summary>
    /// <param name="Way">Which way it moves her.</param>
    private sealed partial record MovingSamus(SamusKnockbackDirection Way) : SamusKnockback
    {
        /// <inheritdoc/>
        public override SamusKnockbackDirection Direction => Way;
    }

    /// <summary>A knockback that has returned Samus's movement to her pose, keeping <paramref name="Way"/>.</summary>
    /// <param name="Way">Which way it moved her.</param>
    private sealed partial record ReleasedSamus(SamusKnockbackDirection Way) : SamusKnockback
    {
        /// <inheritdoc/>
        public override SamusKnockbackDirection Direction => Way;
    }
}
