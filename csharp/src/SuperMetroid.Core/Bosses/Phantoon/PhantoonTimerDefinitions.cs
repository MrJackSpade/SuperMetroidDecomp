namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon timer magnitudes and independently chosen random-bucket policy.</summary>
public static class PhantoonTimerDefinitions
{
    /// <summary>$A7:CE2B, InitAI_PhantoonBody: Japan/USA initial flame countdown is 120 frames. PAL uses 96.</summary>
    public const ushort InitialFlameDelayFrames = 120;

    /// <summary>Three selected exposure tiers used by the native RNG-bucket policies.</summary>
    internal enum DurationChoice
    {
        /// <summary>Shortest open-eye, closed-eye, or rain-hiding interval for its phase.</summary>
        Short,
        /// <summary>Middle interval, scaled from the phase's shortest exposure.</summary>
        Medium,
        /// <summary>Longest interval, twice the phase's middle exposure.</summary>
        Long
    }
    /// <summary>Phantoon phases with independent eight-bucket duration tables.</summary>
    internal enum TimerKind
    {
        /// <summary>Open-eye damage opportunity selected from RNG bits.</summary>
        Vulnerable,
        /// <summary>Closed-eye moving period selected from RNG or first-round NMI bits.</summary>
        EyeClosed,
        /// <summary>Hidden wait after flame-rain fading completes.</summary>
        RainHiding
    }

    /// <summary>$A7:CD41, RNG&amp;7 at $A7:D060-D06C selects the vulnerable window.</summary>
    /// <remarks>Narrow approved nonsense retention: RNG/frame buckets have no temporal or physical ordering. These
    /// exact permutations specify the random choice policy; inventing a function for them would only re-encode the same
    /// ordering. This does not exempt any other combat phase, motion, fade, placement or flame delay.</remarks>
    internal static readonly DurationChoice[] VulnerableChoices =
        [DurationChoice.Long, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Medium,
         DurationChoice.Long, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Long];
    /// <summary>$A7:CD53, RNG&amp;7 at D07C-D088; (NMI&gt;&gt;1)&amp;3 at D5A6-D5B2 for the first round.</summary>
    internal static readonly DurationChoice[] EyeClosedChoices =
        [DurationChoice.Long, DurationChoice.Short, DurationChoice.Medium, DurationChoice.Long,
         DurationChoice.Medium, DurationChoice.Short, DurationChoice.Medium, DurationChoice.Long];
    /// <summary>$A7:CD63, RNG&amp;7 at $A7:D7E7-D7F3 selects the rain hiding delay.</summary>
    internal static readonly DurationChoice[] RainHidingChoices =
        [DurationChoice.Medium, DurationChoice.Long, DurationChoice.Short, DurationChoice.Medium,
         DurationChoice.Short, DurationChoice.Medium, DurationChoice.Short, DurationChoice.Short];

    /// <summary>$A7:CD45 and D03F-D075/D60D-D65B: selected shortest fifteen-call opportunity to hit the open eye. The waiting state has no movement-derived duration; changing this choice changes the attack opportunity.</summary>
    internal const int ShortestVulnerableExposureFrames = 15;
    /// <summary>$A7:CD53 and D5E7-D60C: selected closed-eye moving phase lasts4 exposure units, then6 times that for medium, then doubles for long. Expiry opens the eye at its current position, independently of path completion; these ratios select attack pacing.</summary>
    internal const int EyeShortScale = 4;
    /// <summary>The medium closed-eye duration is six times the phase's short base.</summary>
    internal const int EyeMediumScale = 6;
    /// <summary>$A7:CD63 and D7D5-D829: selected hidden wait begins only after fading completes. Its shortest duration is two exposure units, with doubled medium/long tiers; the next location is chosen only on expiry, so neither fading nor travel determines this wait.</summary>
    internal const int RainShortScale = 2;
    /// <summary>$A7:CD41/CD53/CD63: exposure and hidden-wait tiers, plus the long closed-eye phase, double the preceding tier. These selected phase timings do not exempt separately timed swoop, fade, placement or projectile behavior.</summary>
    internal const int TierDoubling = 2;

    /// <summary>Gets the eight-entry RNG-bucket schedule for Phantoon's open-eye damage opportunity.</summary>
    public static PhantoonTimerSchedule VulnerableWindow => new(TimerKind.Vulnerable);

    /// <summary>Gets the eight-entry RNG/frame-bucket schedule for the closed-eye moving phase.</summary>
    public static PhantoonTimerSchedule EyeClosed => new(TimerKind.EyeClosed);

    /// <summary>Gets the eight-entry RNG-bucket schedule for the hidden delay after the flame rain.</summary>
    public static PhantoonTimerSchedule RainHiding => new(TimerKind.RainHiding);

    /// <summary>Each phase's short/medium/long bucket permutation.</summary>
    internal static readonly Dictionary<TimerKind, DurationChoice[]> Choices = new()
    {
        [TimerKind.Vulnerable] = VulnerableChoices,
        [TimerKind.EyeClosed] = EyeClosedChoices,
        [TimerKind.RainHiding] = RainHidingChoices,
    };
}
