using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
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

    /// <summary>The loaded Draygon, if any.</summary>
    DraygonEnemyState? Draygon { get; }

    /// <summary>The loaded Phantoon, if any.</summary>
    PhantoonEnemyState? Phantoon { get; }

    /// <summary>Ceres Ridley, if loaded.</summary>
    RidleyEnemyState? CeresRidley { get; }

    /// <summary>The warning Ceres's self-destruct shows over the foreground (#1555), empty before it shows any.</summary>
    RoomWrittenOverlay CeresWarning { get; }

    /// <summary>The warning Zebes's escape shows over the background (#1555), empty before it shows any.</summary>
    RoomWrittenOverlay ZebesWarning { get; }

    /// <summary>The chamber's walls Ceres Ridley's getaway shows around its turning floor (#1545), in the room's loaded artwork.</summary>
    ImmutableArray<PerspectiveFloorScenery> CeresGetawayChamberWalls { get; }

    /// <summary>
    /// The body a boss shows (#1352, #1353, #1466, #1515): Kraid's while he owns it, otherwise the one a boss attached
    /// while its room shows it; null without either.
    /// </summary>
    BossBodyLayer? BossBody { get; }

    /// <summary>Where Kraid's body lies in the room while it is shown (#1352, #1617); otherwise null.</summary>
    Position16<RoomPixels>? KraidBodyOrigin { get; }

    /// <summary>
    /// Where Crocomire's body shows against the screen while Crocomire is loaded, the pixel of his body's area at the
    /// screen's top-left, and whether it shows (#1617); otherwise null.
    /// </summary>
    ScreenPlacedBossBody? CrocomireBody { get; }

    /// <summary>The morph-ball eye's beam.</summary>
    MorphBallEyeBeamState MorphBallEyeBeam { get; }

    /// <summary>The loaded bosses' owners of colour effects gameplay's pictures show, in the order they are laid over each other (#1512).</summary>
    IEnumerable<IGameplayShadingSource> ShadingSources { get; }
}
