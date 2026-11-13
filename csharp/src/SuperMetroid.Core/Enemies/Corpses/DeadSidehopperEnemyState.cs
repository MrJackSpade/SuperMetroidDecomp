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

    /// <summary>Captures one corpse's silhouette and its own rot schedule, and starts its own rows.</summary>
    internal DeadSidehopperEnemyState(RoomEnemySlot slot, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
        Rows = new RottingCorpseRows(rot.Rows.Count);
    }

    /// <summary>The enemy slot owning this corpse's position, collision properties and animation timing.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The corpse's own rot schedule: the fall of each of its pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }
    /// <summary>The corpse's own pixel rows as its rot calls have left them, which its frames are drawn through (#1539).</summary>
    internal RottingCorpseRows Rows { get; }

    /// <summary>The current dead-sidehopper AI behaviour.</summary>
    public DeadSidehopperAction? Action { get; internal set; }

    /// <summary>Position 0..3 in the four-hop launch cycle, selecting the next launch velocity; each landing advances it modulo four.</summary>
    public int JumpPhase { get; internal set; }

    /// <summary>Draining AI calls since the drain last stepped, 0..7; the eighth call steps the drain and restarts the count.</summary>
    public int DrainStepCalls { get; internal set; }

    /// <summary>
    /// How far Shitroid's drain has gone: 0 before Shitroid feeds, 1..7 while draining and 8 once the victim is a corpse;
    /// absent for a corpse placed already dead, which has no drain and is drained from the start (<see cref="IsDrained"/>).
    /// Gameplay reads it: it decides whether the victim hops or drains after a landing and whether a shot, touch or Power
    /// Bomb starts it rotting.
    /// </summary>
    [IntegerQuantity("drain progress counted from zero to eight; comparisons test how far draining has gone")]
    public int? DrainStage { get; internal set; }

    /// <summary>The drain stage at which the victim is fully drained.</summary>
    internal const int FullyDrainedStage = 8;

    /// <summary>
    /// Whether the corpse is drained, placed already dead or turned into a corpse by Shitroid's feeding, so a shot, touch
    /// or Power Bomb starts it rotting.
    /// </summary>
    internal bool IsDrained => DrainStage is null or >= FullyDrainedStage;

    /// <summary>
    /// Whether draining has begun: Shitroid has fed on the corpse, or it was placed already dead. The hop's end and the
    /// post-landing pause turn to the drain once it has.
    /// </summary>
    internal bool HasBegunDraining => DrainStage is not 0;

    /// <summary>
    /// The colours the corpse shows, from how far its drain has gone (#1512): a corpse placed already dead lies in the
    /// drained corpses' colours; the victim shows the colours the Shitroid's room entry gives it until the drain's first
    /// step, then each step of the drain in turn.
    /// </summary>
    internal PictureShading Shading => DrainStage switch
    {
        null => TourianCorpseShades.InDrainedColours,
        < 2 => TourianCorpseShades.InShitroidTargetColours,
        { } stage => PictureShading.Shaded(TourianCorpseShades.SidehopperDrain(stage - 2)),
    };

    /// <summary>Signed 8.8 hop velocity, horizontal and vertical.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Pre-rot AI calls counted since Samus first stood on the corpse, from zero: the sixteenth call starts rotting, and
    /// the corpse never returns to the delay, so the count stops at sixteen.
    /// </summary>
    public int PreRotDelayCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// The countdown initialized to 64 for the post-landing pause; expiry requires decrementing through zero
    /// to -1, giving 65 delay-state calls.
    /// </summary>
    public int StateTimer { get; internal set; }

    /// <summary>Counts one pre-rot AI call.</summary>
    internal void CountPreRotDelayCall() => PreRotDelayCounter++;
}
