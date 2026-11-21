namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The knockback a hit put Samus in (#2553): none, one moving her in its direction, or one whose movement a damage boost,
/// Crystal Flash or her death handed back to her pose, which keeps its direction until her knockback hold's expiry
/// finishes it. A knockback that moves her always has a direction: the game's knockback movement has no movement for
/// the direction a hit never sets.
/// </summary>
public abstract record SamusKnockback
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private SamusKnockback()
    {
    }

    /// <summary>No knockback is pending or moving her.</summary>
    public static SamusKnockback None { get; } = new NoKnockback();

    /// <summary>
    /// The knockback once its movement is handed back to her pose: a moving knockback keeps its direction as a handed-back
    /// one; any other stays as it is.
    /// </summary>
    internal virtual SamusKnockback HandBack() => this;

    /// <summary>No knockback is pending or moving her.</summary>
    public sealed record NoKnockback : SamusKnockback;

    /// <summary>A knockback that moves Samus (<see cref="SamusMovementOwner.Knockback"/>).</summary>
    /// <param name="Direction">Which way it moves her.</param>
    public sealed record Moving(SamusKnockbackDirection Direction) : SamusKnockback
    {
        /// <inheritdoc/>
        internal override SamusKnockback HandBack() => new HandedBack(Direction);
    }

    /// <summary>A knockback whose movement went back to her pose before her knockback hold ran out.</summary>
    /// <param name="Direction">Which way it moved her.</param>
    public sealed record HandedBack(SamusKnockbackDirection Direction) : SamusKnockback;
}
