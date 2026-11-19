using SuperMetroid.Core.Bosses.Crocomire.Artwork;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's death sequence state outside his enemy places: the bridge collapse, the acid, the melt's passes, the
/// hidden wall's rumble and the baring of his skeleton.
/// </summary>
public sealed class CrocomireDeathState
{
    /// <summary>
    /// Bridge fragments the collapse has launched so far, zero through
    /// <see cref="CrocomireBridgeFragmentDefinitions.FragmentCount"/>; it advances once per collapse frame and stops
    /// at the last fragment.
    /// </summary>
    public int BridgeFragmentsSpawned { get; internal set; }

    /// <summary>
    /// The wait for the acid's next smoke about his body (#2463): the bridge's collapse starts it to run out on his death's
    /// next update, and each smoke starts it again to last <see cref="CrocomireBridgeCollapseDefinitions.AcidSmokeInterval"/>.
    /// </summary>
    public BossCountdown AcidSmokeWait { get; } = new();

    /// <summary>
    /// The wait for the acid's next burning sound (#2463): the bridge's collapse starts it to run out on his death's next
    /// update, and each sound starts it again to last <see cref="CrocomireBridgeCollapseDefinitions.AcidSoundInterval"/>.
    /// </summary>
    public BossCountdown AcidSoundWait { get; } = new();

    /// <summary>
    /// How many stages of his approach onto the bridge (<see cref="CrocomireBridgeCollapseDefinitions.ApproachStages"/>)
    /// have crumbled their blocks and raised their dust since he last stood off it (#2182, #2540), zero through their count.
    /// </summary>
    public int ApproachStagesRun
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, CrocomireBridgeCollapseDefinitions.ApproachStages.Count, value);
    }

    /// <summary>The melt pass in progress: which of the two melt images Crocomire's body shows and dissolves.</summary>
    public CrocomireMeltingIllustration MeltPass { get; internal set; }

    /// <summary>
    /// Dissolve steps the pass has taken, zero through <see cref="CrocomireMeltingDefinitions.ColumnCount"/>: one
    /// column of the melt image erased per step.
    /// </summary>
    public int MeltColumnsErased { get; internal set; }

    /// <summary>
    /// The band the pass shows next, zero through its band count; at the band count the pass's bands have all been shown
    /// once and it starts over at the first.
    /// </summary>
    public int NextMeltBand { get; internal set; }

    /// <summary>
    /// The dissolve step each band of the melt image last showed, by band (#1652): the pass's image hides, in each band, the
    /// columns the band's step had erased when the band was last shown. A pass starts every band at no step.
    /// </summary>
    internal int[] MeltBandSteps { get; } = new int[CrocomireMeltingDefinitions.SecondPassBandCount];

    /// <summary>
    /// The updates his death has spent baring the skeleton so far, zero through
    /// <see cref="CrocomireSkeletonLookDefinitions.BaringDuration"/>: one more each update the skeleton waits behind the wall
    /// breaking through, the skeleton look showing from the last on; the final sink starts it over.
    /// </summary>
    public int SkeletonBaringElapsed { get; internal set; }

    /// <summary>Lowest background source row the melt distortion stretches back to, from the active pass.</summary>
    public int LowestDistortionSourceRow { get; internal set; }

    /// <summary>Background source row the distortion starts at; it climbs three rows per frame and stops at sixteen.</summary>
    public int DistortionSourceRow { get; internal set; }

    /// <summary>
    /// The rows the melt distortion falls behind per line (#2016): it starts at
    /// <see cref="CrocomireMeltingDefinitions.FirstDistortionSlope"/>, grows by
    /// <see cref="CrocomireMeltingDefinitions.DistortionSlopeGrowth"/> an update and stops at
    /// <see cref="CrocomireMeltingDefinitions.SteepestDistortionSlope"/>.
    /// </summary>
    public UnsignedSixteenSixteen DistortionStep { get; internal set; }

    /// <summary>Body X captured immediately before each melting distortion starts; the melt shake jitters around it.</summary>
    public int BodyXBeforeMelting { get; internal set; }

    /// <summary>
    /// Melt shake counter: zero before the first melt, decremented once per dissolve frame and wrapping below zero; the
    /// shake alternates between its two looks every <see cref="CrocomireMeltingDefinitions.ShakeHalfPeriod"/> counts, the
    /// second of them jittered four pixels on X.
    /// The wrap is load-bearing: the first dissolve frame steps the counter down from zero to its largest count, which
    /// lies in a jittered half, so the shake starts on the jittered side.
    /// </summary>
    public WrappingCountdown MeltShakeCounter { get; internal set; }

    /// <summary>Signed vertical offset in pixels the hidden-wall rumble adds to the foreground scroll.</summary>
    public int RumbleYOffset { get; internal set; }

    /// <summary>The swings left in the current rumble oscillation after the one under way; it stops at zero.</summary>
    public int RumbleRepeatsLeft { get; internal set; }

    /// <summary>
    /// The skeleton's wait before it falls once it has broken through the wall (#2463): the break-through starts it to last
    /// <see cref="CrocomireSkeletonDefinitions.FallDelay"/>, and the skeleton starts falling as it runs out.
    /// </summary>
    public BossCountdown SkeletonFallWait { get; } = new();

    /// <summary>
    /// The stretch the melt sags his body layer through (#1468): his body layer's vertical scroll, levelled at each pass's
    /// start and stretched through a band below the tongue as the dissolve runs. The display shows it while
    /// <see cref="MeltingScrollActive"/>.
    /// </summary>
    public CrocomireMeltDistortion MeltDistortion { get; internal set; } = CrocomireMeltDistortion.Level(0);

    /// <summary>
    /// Whether the melting's per-line scroll distortion is running: the melting starts it and ends it.
    /// Levelling the stretch alone does not start it.
    /// </summary>
    public bool MeltingScrollActive { get; internal set; }
}
