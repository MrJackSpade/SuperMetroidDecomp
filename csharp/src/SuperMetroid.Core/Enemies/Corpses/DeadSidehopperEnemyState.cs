using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One dead sidehopper's corpse state: its silhouette, rot schedule, animation, hop, drain and contact progress.
/// </summary>
public sealed class DeadSidehopperEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianSegment> Playback { get; set; }

    /// <summary>Captures one corpse's silhouette and its own rot schedule, and starts its own rows and its first action.</summary>
    internal DeadSidehopperEnemyState(CorpseRotSchedule rot, DeadSidehopperAction action)
    {
        Action = action;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
    }

    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot has left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }

    /// <summary>The current dead-sidehopper AI behaviour.</summary>
    public DeadSidehopperAction Action { get; internal set; }

    /// <summary>
    /// The launch of its next hop in its four-hop cycle (<see cref="DeadSidehopperLaunchDefinitions.Launches"/>): each
    /// landing steps it on, the first following the fourth (#2432).
    /// </summary>
    internal CyclingSequence<EightEightVector<RoomPixels>> NextHop { get; set; } = CyclingSequence<EightEightVector<RoomPixels>>.StartOf(DeadSidehopperLaunchDefinitions.Launches);

    /// <summary>Draining AI updates since the drain last stepped, 0..7; the eighth steps the drain and restarts the count.</summary>
    public int DrainStepUpdates { get; internal set; }

    /// <summary>
    /// How far Shitroid's drain has gone (#2369); absent for a corpse placed already dead, which has no drain and is
    /// drained from the start (<see cref="IsDrained"/>).
    /// </summary>
    public SidehopperDrainStage? DrainStage { get; internal set; }

    /// <summary>
    /// Whether the corpse is drained, placed already dead or turned into a corpse by Shitroid's feeding, so a shot, touch
    /// or Power Bomb starts it rotting.
    /// </summary>
    internal bool IsDrained => DrainStage is not { } stage || stage.IsDrained;

    /// <summary>
    /// Whether draining has begun: Shitroid has fed on the corpse, or it was placed already dead. The hop's end and the
    /// post-landing pause turn to the drain once it has.
    /// </summary>
    internal bool HasBegunDraining => DrainStage is not { } stage || stage.HasBegun;

    /// <summary>
    /// The colours the corpse shows, from how far its drain has gone (#1512): a corpse placed already dead lies in the
    /// drained corpses' colours; the victim shows the colours the Shitroid's room entry gives it until the drain's first
    /// step, then each step of the drain in turn.
    /// </summary>
    internal PictureShading Shading => DrainStage switch
    {
        null => TourianCorpseShades.InDrainedColours,
        { IsShaded: false } => TourianCorpseShades.InShitroidTargetColours,
        { } stage => PictureShading.Shaded(TourianCorpseShades.SidehopperDrain(stage)),
    };

    /// <summary>Signed 1/256-pixel hop velocity, horizontal and vertical.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Pre-rot AI updates counted since Samus first stood on the corpse, from zero: the update that counts
    /// <see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/> starts rotting, and the corpse never returns to the
    /// delay, so the count stops there.
    /// </summary>
    public int PreRotDelayCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// The updates left in the post-landing pause, the one it ends on included: it starts at
    /// <see cref="DeadSidehopperVictimDefinitions.PostLandingPause"/> and the step that takes it to zero ends the pause (#2281).
    /// </summary>
    public int StateTimer { get; internal set; }

    /// <summary>Counts one pre-rot AI update.</summary>
    internal void CountPreRotDelayUpdate() => PreRotDelayCounter++;
}
