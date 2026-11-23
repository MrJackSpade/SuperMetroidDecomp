using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One dead sidehopper's corpse state: its rot schedule, pixel rows, animation, hop, drain and contact progress.
/// </summary>
public sealed partial class DeadSidehopperEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DeadTourianSegment> Playback { get; set; }

    /// <summary>Captures one corpse's own rot schedule and layout, and starts its own rows and its first action.</summary>
    internal DeadSidehopperEnemyState(CorpseRotSchedule rot, DeadSidehopperAction action, SidehopperCorpseLayout layout)
    {
        Action = action;
        Layout = Ensure.NotNull(layout);
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
    internal CyclingSequence<PixelVelocity<RoomPixels>> NextHop { get; set; } = CyclingSequence<PixelVelocity<RoomPixels>>.StartOf(DeadSidehopperLaunchDefinitions.Launches);

    /// <summary>The drain's current stage, counted once each draining AI update: the update it ends on steps the drain.</summary>
    public UpdateCountdown DrainStageWait { get; internal set; } = UpdateCountdown.Lasting(DeadSidehopperVictimDefinitions.DrainUpdatesPerStage);

    /// <summary>
    /// How the corpse lies in its room (#2558): placed already dead, or Baby Metroid's victim, whose drain has gone as far as
    /// its stage (#2369).
    /// </summary>
    public SidehopperCorpseLayout Layout
    {
        get;
        internal set => field = Ensure.NotNull(value);
    }

    /// <summary>
    /// Whether the corpse is drained, placed already dead or turned into a corpse by Baby Metroid's feeding, so a shot, touch
    /// or Power Bomb starts it rotting.
    /// </summary>
    internal bool IsDrained => Layout.IsDrained;

    /// <summary>
    /// Whether draining has begun: Baby Metroid has fed on the corpse, or it was placed already dead. The hop's end and the
    /// post-landing pause turn to the drain once it has.
    /// </summary>
    internal bool HasBegunDraining => Layout.HasBegunDraining;

    /// <summary>The colours the corpse shows, from its layout and how far its drain has gone (#1512).</summary>
    internal PictureShading Shading => Layout.Shading;

    /// <summary>The hop velocity, horizontal and vertical, in pixels per update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The wait before rotting, counted once each pre-rot AI update from Samus first standing on the corpse: the update it
    /// ends on, the <see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/>th, starts rotting, and the corpse never
    /// returns to the delay.
    /// </summary>
    public UpdateCountdown PreRotDelay { get; } = UpdateCountdown.Lasting(CorpseRotScheduleDefinitions.PreRotDelayUpdates);

    /// <summary>
    /// The post-landing pause, lasting <see cref="DeadSidehopperVictimDefinitions.PostLandingPause"/>: the update it ends on
    /// ends the pause (#2281).
    /// </summary>
    public UpdateCountdown PostLandingPause { get; internal set; } = UpdateCountdown.Ended();
}
