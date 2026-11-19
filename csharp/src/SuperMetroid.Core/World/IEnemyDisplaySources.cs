using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// What the room's enemies contribute to the display, by what each contribution is (#2571): the displayed states the
/// frame-boundary latch latches, the display effects that step with the room's effects, the boss body, the overlays, the
/// flown turning floor, the distortion, beam and clip the frame shows, and the owners of colour effects. Reads are live;
/// the latch that captures them stays latched. No member names a species.
/// </summary>
internal interface IEnemyDisplaySources
{
    /// <summary>The enemies' displayed states, which the accepted frame's boundary latches with the rest of the frame.</summary>
    IEnumerable<IDisplayLatch> DisplayLatches { get; }

    /// <summary>The beam cone the last frame boundary latched, or none while no beam showed.</summary>
    MorphBallEyeBeamCone? LatchedBeamCone { get; }

    /// <summary>The encounters' display effects, which step with the room's own effects.</summary>
    IEnumerable<IStepWithRoomEffects> RoomEffectSteps { get; }

    /// <summary>The waving body the room's boss shows as its displayed wave and visibility stand, or none while no boss waves.</summary>
    PhantoonDisplaySnapshot? WavingBody { get; }

    /// <summary>Whether a boss's rising floor clips the room's view this frame.</summary>
    bool ClipsRisingFloor { get; }

    /// <summary>The rainbow beam a boss fires, as the frame shows it, or none while none fires.</summary>
    MotherBrainRainbowBeamRenderSnapshot? RainbowBeam { get; }

    /// <summary>The distortion a boss's melt shows on the room's background while its scroll runs (#1466), or null while it shows none.</summary>
    CrocomireMeltDistortion? BackgroundDistortion { get; }

    /// <summary>
    /// How the turning floor an encounter flies over appears as the flight last turned, zoomed and moved it, or null while
    /// no flight shows it (#2228).
    /// </summary>
    FloorView? FlownFloor { get; }

    /// <summary>The warning written over the foreground (#1555), empty before it shows any.</summary>
    RoomWrittenOverlay ForegroundWrittenOverlay { get; }

    /// <summary>The warning written over the background (#1555), empty before it shows any.</summary>
    RoomWrittenOverlay BackgroundWrittenOverlay { get; }

    /// <summary>The walls the flight shows around its turning floor (#1545), in the room's loaded artwork.</summary>
    ImmutableArray<PerspectiveFloorScenery> FlownFloorWalls { get; }

    /// <summary>
    /// The body the room's boss shows (#1352, #1353, #1466, #1515, #2031), whole (#2513): its layer, where the boss places it,
    /// whether it shows nothing this frame and whether it hides the background layer; null where no boss attached one.
    /// </summary>
    IBossBodyDisplay? BossBody { get; }

    /// <summary>The loaded bosses' owners of colour effects gameplay's pictures show, in the order they are laid over each other (#1512).</summary>
    IEnumerable<IGameplayShadingSource> ShadingSources { get; }
}
