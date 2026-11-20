namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where one of Mother Brain's bouncing bombs stands in its flight (#2553): its first fall, the gravity of the bounce it
/// last made, or landed from its last bounce, so it expires. Each bounce leads to the next bounce's gravity or, after the
/// last, to the landing, so no bounce past the authored ones exists.
/// </summary>
internal abstract record MotherBrainBombBounces
{
    /// <summary>Allows only the stands below.</summary>
    private MotherBrainBombBounces()
    {
    }

    /// <summary>The bomb's first fall, before any bounce, at the ordinary gravity.</summary>
    internal static Flying FirstFall { get; } = new Flying.Falling();

    /// <summary>The bomb is in the air: falling for the first time, or after a bounce.</summary>
    internal abstract record Flying : MotherBrainBombBounces
    {
        /// <summary>Allows only the flights below.</summary>
        private Flying()
        {
        }

        /// <summary>Where the bomb stands once it bounces off the floor from this flight.</summary>
        internal abstract MotherBrainBombBounces Bounced { get; }

        /// <summary>The bomb falls for the first time, before any bounce.</summary>
        internal sealed record Falling : Flying
        {
            /// <inheritdoc/>
            internal override MotherBrainBombBounces Bounced { get; } =
                new Bouncing(ListStep<SignedEightEight>.FirstOf(MotherBrainBombBounceDefinitions.BounceAccelerations));
        }

        /// <summary>The bomb has bounced, and falls at the gravity of the bounce it last made.</summary>
        /// <param name="Gravity">The bounce's gravity, among each bounce's in turn.</param>
        internal sealed record Bouncing(ListStep<SignedEightEight> Gravity) : Flying
        {
            /// <inheritdoc/>
            internal override MotherBrainBombBounces Bounced => Gravity.Following is { } next ? new Bouncing(next) : new Landed();
        }
    }

    /// <summary>The bomb has landed from its last bounce: it expires on its next update.</summary>
    internal sealed record Landed : MotherBrainBombBounces;
}
