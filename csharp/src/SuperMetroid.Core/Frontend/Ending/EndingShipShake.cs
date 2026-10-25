namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The gunship's horizontal shake while it emerges from the exploding planet: per-step signed 16.16 displacements whose
/// fast and slow cycles each move the ship exactly two pixels (#1416: moved from the deleted ending catalog).
/// </summary>
/// <remarks>Repeated samples, fixed-point sign assembly and amplitudes from the common cycle displacement calculate. The
/// exact directional schedules and shared displacement are selected cinematic path content: the sequential consumer
/// has no geometry/state rule choosing reversal phases. Generating a different schedule changes that composition.</remarks>
public static class EndingShipShake
{
    /// <summary>Positive fast steps in each complete fast run.</summary>
    private const int FastPositiveRun = 4;
    /// <summary>Negative fast steps ending each fast run.</summary>
    private const int FastNegativeRun = 2;
    /// <summary>The first positive slow steps.</summary>
    private const int SlowFirstPositiveRun = 3;
    /// <summary>The negative slow steps that follow them.</summary>
    private const int SlowFirstNegativeRun = 2;
    /// <summary>The second positive slow steps before the terminal negative step.</summary>
    private const int SlowSecondPositiveRun = 2;

    /// <summary>Length of a complete positive-then-negative fast run.</summary>
    private const int FastRunPeriod = FastPositiveRun + FastNegativeRun;
    /// <summary>Number of complete fast runs before the shortened final run.</summary>
    private const int FastCompleteRunCount = EndingMotionDefinitions.PlanetFastPatternLength / FastRunPeriod;
    /// <summary>Positive steps left in the final fast run after its terminal negative steps.</summary>
    private const int FastFinalPositiveRun = EndingMotionDefinitions.PlanetFastPatternLength % FastRunPeriod - FastNegativeRun;
    /// <summary>Total positive steps used to derive the fast amplitude from the net displacement.</summary>
    private const int FastPositiveSteps = FastCompleteRunCount * FastPositiveRun + FastFinalPositiveRun;
    /// <summary>Total positive steps in the two slow runs.</summary>
    private const int SlowPositiveSteps = SlowFirstPositiveRun + SlowSecondPositiveRun;

    /// <summary>The displacement of one whole cycle, fast or slow: two pixels.</summary>
    private static readonly int CycleDisplacement = SnesSignedSixteenSixteen.FromParts(2, 0).RawValue;
    /// <summary>The per-step fast delta that yields the cycle displacement.</summary>
    private static readonly int FastAmplitude =
        CycleDisplacement / (2 * FastPositiveSteps - EndingMotionDefinitions.PlanetFastPatternLength);
    /// <summary>The per-step slow delta that yields the cycle displacement.</summary>
    private static readonly int SlowAmplitude =
        CycleDisplacement / (2 * SlowPositiveSteps - EndingMotionDefinitions.PlanetSlowPatternLength);

    /// <summary>Fast step <paramref name="index"/>'s displacement: repeated positive/negative runs and a shorter final positive run.</summary>
    public static int PlanetFastDelta(int index)
    {
        if ((uint)index >= EndingMotionDefinitions.PlanetFastPatternLength) throw new ArgumentOutOfRangeException(nameof(index));
        int completeRuns = EndingMotionDefinitions.PlanetFastPatternLength / FastRunPeriod * FastRunPeriod;
        int positiveRun = index < completeRuns
            ? FastPositiveRun
            : EndingMotionDefinitions.PlanetFastPatternLength - completeRuns - FastNegativeRun;
        return index % FastRunPeriod < positiveRun ? FastAmplitude : -FastAmplitude;
    }

    /// <summary>Slow step <paramref name="index"/>'s displacement: two positive runs separated by negative steps.</summary>
    public static int PlanetSlowDelta(int index)
    {
        if ((uint)index >= EndingMotionDefinitions.PlanetSlowPatternLength) throw new ArgumentOutOfRangeException(nameof(index));
        int secondStart = SlowFirstPositiveRun + SlowFirstNegativeRun;
        bool positive = index < SlowFirstPositiveRun || index >= secondStart && index < secondStart + SlowSecondPositiveRun;
        return positive ? SlowAmplitude : -SlowAmplitude;
    }
}
