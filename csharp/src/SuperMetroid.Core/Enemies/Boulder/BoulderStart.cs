namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How a Boulder starts once Samus comes close (#2023): it falls from a drop and bounces, or it rolls at once.</summary>
internal abstract record BoulderStart
{
    /// <summary>Closes the hierarchy to the starts declared here.</summary>
    private BoulderStart()
    {
    }

    /// <summary>Hangs above its resting place and falls, bouncing before it rolls.</summary>
    internal sealed record FallsFrom : BoulderStart
    {
        /// <summary>Starts a Boulder that hangs <paramref name="drop"/> pixels above its resting place.</summary>
        /// <param name="drop">The pixels it hangs above its resting place, 1 through 255.</param>
        internal FallsFrom(int drop) => Drop = Ensure.BetweenInclusive(1, byte.MaxValue, drop);

        /// <summary>The pixels it hangs above its resting place, 1 through 255.</summary>
        internal int Drop { get; }
    }

    /// <summary>Rolls at once, without falling or bouncing.</summary>
    internal sealed record RollsAtOnce : BoulderStart
    {
        /// <summary>The one start of its kind.</summary>
        internal static RollsAtOnce Instance { get; } = new();

        /// <summary>Creates the one start of its kind.</summary>
        private RollsAtOnce()
        {
        }
    }
}
