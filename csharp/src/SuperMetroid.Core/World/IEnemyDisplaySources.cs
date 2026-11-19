using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// The live enemy state the frame-boundary display latch, actor drawing and screen windows read at their own points. Reads are
/// live; the latch that captures them stays latched.
/// </summary>
internal interface IEnemyDisplaySources
{
    /// <summary>The loaded Mother Brain, if any.</summary>
    MotherBrainEnemyState? MotherBrain { get; }

    /// <summary>The loaded Phantoon, if any.</summary>
    PhantoonEnemyState? Phantoon { get; }

    /// <summary>The distortion Crocomire's melt shows on the room while its scroll runs (#1466), or null while it shows none.</summary>
    CrocomireMeltDistortion? CrocomireMelt { get; }

    /// <summary>
    /// How the turning floor Ceres Ridley's getaway flies over appears as the getaway last turned, zoomed and moved it, or
    /// null while the getaway does not show it (#2228).
    /// </summary>
    FloorView? CeresGetawayFloor { get; }

    /// <summary>The warning Ceres's self-destruct shows over the foreground (#1555), empty before it shows any.</summary>
    RoomWarningText CeresWarning { get; }

    /// <summary>The warning Zebes's escape shows over the background (#1555), empty before it shows any.</summary>
    RoomWarningText ZebesWarning { get; }

    /// <summary>The chamber's walls Ceres Ridley's getaway shows around its turning floor (#1545), in the room's loaded artwork.</summary>
    ImmutableArray<PerspectiveFloorScenery> CeresGetawayChamberWalls { get; }

    /// <summary>
    /// The body the room's boss shows (#1352, #1353, #1466, #1515, #2031), whole (#2513): its layer, where the boss places it
    /// and whether it shows nothing this frame; null where no boss attached one.
    /// </summary>
    IBossBodyDisplay? BossBody { get; }

    /// <summary>The morph-ball eye's beam.</summary>
    MorphBallEyeBeamState MorphBallEyeBeam { get; }

    /// <summary>The loaded bosses' owners of colour effects gameplay's pictures show, in the order they are laid over each other (#1512).</summary>
    IEnumerable<IGameplayShadingSource> ShadingSources { get; }
}
