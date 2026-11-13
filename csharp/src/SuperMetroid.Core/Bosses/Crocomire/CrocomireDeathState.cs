using SuperMetroid.Core.Bosses.Crocomire.Artwork;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's death sequence state outside his enemy slots: the bridge collapse, the acid, the melt's passes, the
/// hidden wall's rumble and the skeleton's characters.
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

    /// <summary>One-shot dust latch for the 1536 bridge approach threshold.</summary>
    public bool BridgeDustAt1536Spawned { get; internal set; }

    /// <summary>One-shot crumble latch for the bridge block at X 1568.</summary>
    public bool BridgeBlockAt1568Crumbling { get; internal set; }

    /// <summary>One-shot crumble latch for the two bridge blocks from X 1584.</summary>
    public bool BridgeBlocksAt1584Crumbling { get; internal set; }

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
    /// Per-row slope of the melt distortion in 1/65536 of a row; it grows from 256 by 384 a frame and stops
    /// at the dissolve's maximum.
    /// </summary>
    public int DistortionStep { get; internal set; }

    /// <summary>Body X captured immediately before each melting distortion starts; the melt shake jitters around it.</summary>
    public int BodyXBeforeMelting { get; internal set; }

    /// <summary>
    /// Melt shake counter: zero before the first melt, decremented once per dissolve frame and wrapping below zero, and
    /// its second bit selects the four-pixel X jitter. The game's hidden-wall rumble later leaves a marker in
    /// the same value (#1476); every melt is over by then and nothing reads the counter again, so the marker is not kept.
    /// The wrap is load-bearing: the first dissolve frame steps the counter from zero, and the jitter's bit is read from
    /// the wrapped count, so the shake starts on the jittered side.
    /// </summary>
    public WrappingCountdown MeltShakeCounter { get; internal set; }

    /// <summary>Signed vertical offset in pixels the hidden-wall rumble adds to the foreground scroll.</summary>
    public int RumbleYOffset { get; internal set; }

    /// <summary>Oscillations left before the rumble moves on, consumed by negative rumble-table entries; it stops at zero.</summary>
    public int RumbleCooldown { get; internal set; }

    /// <summary>Pixels per frame the rumble offset moves toward its target, loaded from each completed negative target.</summary>
    public int RumbleDelta { get; internal set; }

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
