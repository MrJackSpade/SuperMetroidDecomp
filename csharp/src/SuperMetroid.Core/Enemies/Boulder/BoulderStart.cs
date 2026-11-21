namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How a Boulder starts once Samus comes close (#2023): it falls from a drop and bounces, or it rolls at once.</summary>
internal abstract record BoulderStart
{
    /// <summary>Closes the hierarchy to the starts declared here.</summary>
    private BoulderStart()
    {
    }

    /// <summary>How many pixels above its resting place the Boulder hangs before it starts.</summary>
    internal abstract int InitialLift { get; }

    /// <summary>Hangs above its resting place and falls, bouncing before it rolls.</summary>
    internal sealed record FallsFrom : BoulderStart
    {
        /// <summary>Starts a Boulder that hangs <paramref name="drop"/> pixels above its resting place.</summary>
        /// <param name="drop">The pixels it hangs above its resting place, one or more.</param>
        internal FallsFrom(int drop) => InitialLift = Ensure.BetweenInclusive(1, int.MaxValue, drop);

        /// <inheritdoc/>
        /// <remarks>The drop it falls before it bounces, one or more pixels.</remarks>
        internal override int InitialLift { get; }
    }

    /// <summary>Rolls at once, without falling or bouncing.</summary>
    internal sealed record RollsAtOnce : BoulderStart
    {
        /// <summary>The one start of its kind.</summary>
        internal static RollsAtOnce Instance { get; } = new();

        /// <inheritdoc/>
        /// <remarks>
        /// A Boulder that rolls at once still hangs a pixel above its resting place, and its trigger starts it rolling
        /// instead of falling.
        /// </remarks>
        internal override int InitialLift => BoulderMotionDefinitions.RollingStartLift;

        /// <summary>Creates the one start of its kind.</summary>
        private RollsAtOnce()
        {
        }
    }
}
