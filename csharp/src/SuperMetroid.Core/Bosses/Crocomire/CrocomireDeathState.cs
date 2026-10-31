using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's death sequence state outside his enemy slots: the bridge collapse, the acid, the melt's passes, the
/// hidden wall's rumble and the skeleton's characters.
/// </summary>
public sealed class CrocomireDeathState
{
    /// <summary>
    /// The canvas Crocomire's death assembles his body on before showing it (#1466): his melt images over a blank, and the
    /// body frames he draws meanwhile.
    /// </summary>
    internal BossBodyCanvas? WorkingBody { get; init; }

    /// <summary>
    /// Bridge fragments the collapse has launched so far, zero through
    /// <see cref="CrocomireBridgeFragmentDefinitions.FragmentCount"/>; it advances once per collapse frame and stops
    /// at the last fragment.
    /// </summary>
    public int BridgeFragmentsSpawned { get; internal set; }

    /// <summary>Six-frame acid smoke cadence at $7E:9018.</summary>
    public WrappingCountdown AcidSmokeTimer { get; internal set; }

    /// <summary>Thirty-two-frame acid damage sound cadence (Crocomire var $20).</summary>
    public WrappingCountdown AcidSoundTimer { get; internal set; }

    /// <summary>One-shot dust latch for the $0600 bridge approach threshold.</summary>
    public bool BridgeDustAt1536Spawned { get; internal set; }

    /// <summary>One-shot crumble latch for bridge block $61.</summary>
    public bool BridgeBlockAt1568Crumbling { get; internal set; }

    /// <summary>One-shot crumble latch for bridge blocks $62/$63.</summary>
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
    /// The skeleton's character chunks shown so far, in their fixed order, zero through
    /// <see cref="CrocomireSkeletonTransferDefinitions.FrameCount"/>: one more each update the skeleton waits behind the wall
    /// breaking through; the final sink starts it over. His skeleton frames draw the pictures of the chunks shown.
    /// </summary>
    public int SkeletonChunksShown { get; internal set; }

    /// <summary>Lowest BG2 source row the melt distortion stretches back to, from the active pass.</summary>
    public int LowestDistortionSourceRow { get; internal set; }

    /// <summary>BG2 source row the distortion starts at; it climbs three rows per frame and stops at sixteen.</summary>
    public int DistortionSourceRow { get; internal set; }

    /// <summary>
    /// Per-scanline slope of the melt distortion in 1/65536 of a row; it grows from 256 by 384 a frame and stops
    /// at the dissolve's maximum.
    /// </summary>
    public int DistortionStep { get; internal set; }

    /// <summary>Height at which the current distortion band stops, from the active pass.</summary>
    public int DistortionEndY { get; internal set; }

    /// <summary>Body X captured immediately before each melting distortion starts; the melt shake jitters around it.</summary>
    public int BodyXBeforeMelting { get; internal set; }

    /// <summary>
    /// Melt shake counter: zero before the first melt, decremented once per dissolve frame and wrapping below zero, and
    /// its second bit selects the four-pixel X jitter. The original game's hidden-wall rumble later leaves a marker in
    /// the same value (#1476); every melt is over by then and nothing reads the counter again, so the marker is not kept.
    /// </summary>
    public WrappingCountdown MeltShakeCounter { get; internal set; }

    /// <summary>Signed vertical offset in pixels the hidden-wall rumble adds to the foreground scroll.</summary>
    public int RumbleYOffset { get; internal set; }

    /// <summary>Countdown consumed by negative rumble-table entries.</summary>
    public WrappingCountdown RumbleCooldown { get; internal set; }

    /// <summary>Pixels per frame the rumble offset moves toward its target, loaded from each completed negative target.</summary>
    public int RumbleDelta { get; internal set; }

    /// <summary>True once $A4:9B86 publishes area-miniboss bit $0002.</summary>
    public bool BossBitSet { get; internal set; }

    /// <summary>True once the skeleton-collapse state invokes the boss item-drop routine.</summary>
    public bool ItemDropRequested { get; internal set; }

    /// <summary>
    /// The stretch the melt sags his body layer through (#1468): his body layer's vertical scroll, levelled at each pass's
    /// start and stretched through a band below the tongue as the dissolve runs. The display shows it while
    /// <see cref="MeltingScrollActive"/>.
    /// </summary>
    public CrocomireMeltDistortion MeltDistortion { get; internal set; } = CrocomireMeltDistortion.Level(0);

    /// <summary>
    /// Whether the melting's per-line scroll distortion is running: $A4:9555 starts it and $A4:95CE ends it.
    /// Levelling the stretch alone does not start it.
    /// </summary>
    public bool MeltingScrollActive { get; internal set; }
}
