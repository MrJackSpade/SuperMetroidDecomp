namespace SuperMetroid.Core.Game;

/// <summary>Phantoon timer magnitudes and independently chosen random-bucket policy.</summary>
public static class PhantoonTimerDefinitions
{
    /// <summary>$A7:CE2B, InitAI_PhantoonBody: Japan/USA initial flame countdown is 120 frames. PAL uses 96.</summary>
    public const ushort InitialFlameDelayFrames = 120;

    private enum DurationChoice { Short, Medium, Long }
    internal enum TimerKind { Vulnerable, EyeClosed, RainHiding }

    // Narrow approved nonsense retention: RNG/frame buckets have no temporal or
    // physical ordering. These exact permutations specify the random choice policy;
    // inventing a function for them would only re-encode the same ordering. This does
    // not exempt the independently required rain-hiding timing below.
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
    // REQUIRED: rain-hiding duration scale and selected tier relationship.
    private const int RainShortScale = 2;
    /// <summary>$A7:CD41: medium/long vulnerable opportunities and the long closed-eye phase double the preceding tier. Those reviewed timing choices are independent of the separately timed swoop handoff; the rain-hiding profile remains required.</summary>
    private const int TierDoubling = 2;

    public static Schedule VulnerableWindow => new(TimerKind.Vulnerable);
    public static Schedule EyeClosed => new(TimerKind.EyeClosed);
    public static Schedule RainHiding => new(TimerKind.RainHiding);

    public readonly struct Schedule : IReadOnlyList<ushort>
    {
        private readonly TimerKind kind;
        internal Schedule(TimerKind kind) => this.kind = kind;
        private DurationChoice[] Choices => kind switch
        {
            TimerKind.Vulnerable => VulnerableChoices,
            TimerKind.EyeClosed => EyeClosedChoices,
            TimerKind.RainHiding => RainHidingChoices,
            _ => throw new InvalidOperationException(),
        };
        public int Count => Choices.Length;
        public int Length => Count;
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
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
