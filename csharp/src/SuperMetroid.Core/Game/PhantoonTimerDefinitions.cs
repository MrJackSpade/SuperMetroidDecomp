namespace SuperMetroid.Core.Game;

/// <summary>Phantoon timer magnitudes and independently chosen random-bucket policy.</summary>
public static class PhantoonTimerDefinitions
{
    /// <summary>$A7:CE2B, InitAI_PhantoonBody: Japan/USA initial flame countdown is 120 frames. PAL uses 96.</summary>
    public const ushort InitialFlameDelayFrames = 120;

    /// <summary>Relative tier converted to a phase-specific gameplay duration.</summary>
    private enum DurationChoice
    {
        /// <summary>Shortest duration defined by the selected phase's scale.</summary>
        Short,
        /// <summary>Middle duration, scaled from the phase's shortest tier.</summary>
        Medium,
        /// <summary>Longest duration, twice the phase's medium tier.</summary>
        Long
    }

    /// <summary>Phantoon phase whose native eight-bucket timer schedule is being viewed.</summary>
    internal enum TimerKind
    {
        /// <summary>Open-eye period during which Phantoon can be damaged.</summary>
        Vulnerable,
        /// <summary>Closed-eye movement period that precedes the next open-eye opportunity.</summary>
        EyeClosed,
        /// <summary>Hidden waiting period selected after the flame-rain sequence.</summary>
        RainHiding
    }

    // Narrow approved nonsense retention: RNG/frame buckets have no temporal or
    // physical ordering. These exact permutations specify the random choice policy;
    // inventing a function for them would only re-encode the same ordering. This does
    // not exempt any other combat phase, motion, fade, placement or flame delay.
    /// <summary>$A7:CD41, RNG&amp;7 at $A7:D060-D06C selects the vulnerable window.</summary>
    private static readonly DurationChoice[] VulnerableChoices =
        [DurationChoice.Long, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Medium,
         DurationChoice.Long, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Long];
    /// <summary>$A7:CD53, RNG&amp;7 at D07C-D088; (NMI&gt;&gt;1)&amp;3 at D5A6-D5B2 for the first round.</summary>
    private static readonly DurationChoice[] EyeClosedChoices =
        [DurationChoice.Long, DurationChoice.Short, DurationChoice.Medium, DurationChoice.Long,
         DurationChoice.Medium, DurationChoice.Short, DurationChoice.Medium, DurationChoice.Long];
    /// <summary>$A7:CD63, RNG&amp;7 at $A7:D7E7-D7F3 selects the rain hiding delay.</summary>
    private static readonly DurationChoice[] RainHidingChoices =
        [DurationChoice.Medium, DurationChoice.Long, DurationChoice.Short, DurationChoice.Medium,
         DurationChoice.Short, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Short];

    /// <summary>$A7:CD45 and D03F-D075/D60D-D65B: selected shortest fifteen-call opportunity to hit the open eye. The waiting state has no movement-derived duration; changing this choice changes the attack opportunity.</summary>
    private const int ShortestVulnerableExposureFrames = 15;
    /// <summary>$A7:CD53 and D5E7-D60C: selected closed-eye moving phase lasts4 exposure units, then6 times that for medium, then doubles for long. Expiry opens the eye at its current position, independently of path completion; these ratios select attack pacing.</summary>
    private const int EyeShortScale = 4, EyeMediumScale = 6;
    /// <summary>$A7:CD63 and D7D5-D829: selected hidden wait begins only after fading completes. Its shortest duration is two exposure units, with doubled medium/long tiers; the next location is chosen only on expiry, so neither fading nor travel determines this wait.</summary>
    private const int RainShortScale = 2;
    /// <summary>$A7:CD41/CD53/CD63: exposure and hidden-wait tiers, plus the long closed-eye phase, double the preceding tier. These selected phase timings do not exempt separately timed swoop, fade, placement or projectile behavior.</summary>
    private const int TierDoubling = 2;

    /// <summary>Gets the eight-entry RNG-bucket schedule for Phantoon's open-eye damage opportunity.</summary>
    public static Schedule VulnerableWindow => new(TimerKind.Vulnerable);

    /// <summary>Gets the eight-entry RNG/frame-bucket schedule for the closed-eye moving phase.</summary>
    public static Schedule EyeClosed => new(TimerKind.EyeClosed);

    /// <summary>Gets the eight-entry RNG-bucket schedule for the hidden delay after the flame rain.</summary>
    public static Schedule RainHiding => new(TimerKind.RainHiding);

    /// <summary>Allocation-free calculated view of one native eight-bucket Phantoon phase-timer table.</summary>
    public readonly struct Schedule : IReadOnlyList<ushort>
    {
        /// <summary>Identifies which phase's bucket schedule this view calculates.</summary>
        private readonly TimerKind kind;

        /// <summary>Creates a calculated view over one phase's native timer buckets.</summary>
        /// <param name="kind">Phase schedule to expose through this value.</param>
        internal Schedule(TimerKind kind) => this.kind = kind;

        /// <summary>Gets the authored short, medium, or long choice for each native bucket.</summary>
        private DurationChoice[] Choices => kind switch
        {
            TimerKind.Vulnerable => VulnerableChoices,
            TimerKind.EyeClosed => EyeClosedChoices,
            TimerKind.RainHiding => RainHidingChoices,
            _ => throw new InvalidOperationException(),
        };
        /// <summary>Gets the eight random-selection buckets in the schedule.</summary>
        public int Count => Choices.Length;

        /// <summary>Calculates the selected phase duration in gameplay updates.</summary>
        /// <param name="index">Bucket index from zero through seven, normally selected by masked RNG or frame bits.</param>
        /// <returns>The exact short, medium, or long duration for this schedule.</returns>
        public ushort this[int index]
        {
            get
            {
                DurationChoice choice = Choices[index];
                int shortest = ShortestVulnerableExposureFrames * (kind == TimerKind.EyeClosed ? EyeShortScale : kind == TimerKind.RainHiding ? RainShortScale : 1);
                int medium = shortest * (kind == TimerKind.EyeClosed ? EyeMediumScale : TierDoubling);
                return (ushort)(choice switch
                {
                    DurationChoice.Short => shortest,
                    DurationChoice.Medium => medium,
                    DurationChoice.Long => medium * TierDoubling,
                    _ => throw new InvalidOperationException(),
                });
            }
        }
        /// <summary>Enumerates all eight calculated durations in native bucket order.</summary>
        /// <returns>An enumerator over the phase durations.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
