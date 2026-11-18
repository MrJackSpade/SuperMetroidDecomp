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

    /// <summary>Frames until the next acid smoke, on a six-frame cadence.</summary>
    public int AcidSmokeTimer { get; internal set; }

    /// <summary>Frames until the next acid damage sound, on a thirty-two-frame cadence; zero once the sounds stop.</summary>
    public int AcidSoundTimer { get; internal set; }

    /// <summary>Whether his first stage onto the bridge has raised its dust (#2182).</summary>
    public bool ApproachDustRaised { get; internal set; }

    /// <summary>Whether his second stage onto the bridge has started its first block crumbling (#2182).</summary>
    public bool FirstBridgeBlockCrumbling { get; internal set; }

    /// <summary>Whether his third stage onto the bridge has started its next two blocks crumbling (#2182).</summary>
    public bool NextBridgeBlocksCrumbling { get; internal set; }

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
    /// Melt shake counter: zero before the first melt, decremented once per dissolve frame and wrapping below zero, and
    /// its second bit selects the four-pixel X jitter.
    /// The wrap is load-bearing: the first dissolve frame steps the counter down from zero to its largest count, whose
    /// jitter bit is set, so the shake starts on the jittered side.
    /// </summary>
    public WrappingCountdown MeltShakeCounter { get; internal set; }

    /// <summary>Signed vertical offset in pixels the hidden-wall rumble adds to the foreground scroll.</summary>
    public int RumbleYOffset { get; internal set; }

    /// <summary>The swings left in the current rumble oscillation after the one under way; it stops at zero.</summary>
    public int RumbleRepeatsLeft { get; internal set; }

    /// <summary>
    /// The updates left before the skeleton starts falling once it has broken through the wall, counted down while
    /// above zero.
    /// </summary>
    public int SkeletonFallDelay { get; internal set; }

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
