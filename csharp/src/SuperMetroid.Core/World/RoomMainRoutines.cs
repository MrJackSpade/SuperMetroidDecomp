using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room mains' work that reads the session (#2287): Ceres falling debris and the Crocomire comeback, which those rooms'
/// mains run, the block animations every room runs, and elevator drawing rules.
/// </summary>
internal sealed class RoomMainRoutines
{
    /// <summary>The layer scrolls adjusted during Crocomire's hidden-wall rumble.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Defeated bosses and the random number sampled by room-main behaviours.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the layer scrolls and the session state the room-main behaviours read.</summary>
    internal RoomMainRoutines(RoomMainRoutinesDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _system = dependencies.System;
    }

    /// <summary>Crocomire's room shaking while his comeback hides the wall.</summary>
    /// <param name="crocomire">The room's Crocomire, whom the room's main is composed with (#2486).</param>
    internal void RunCrocomireComebackRoomMain(CrocomireEnemyState crocomire)
    {
        Ensure.NotNull(crocomire);
        if (crocomire.Body.Presence.HasAny(EnemyPresence.Hidden) ||
            crocomire.DeathPhase != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        // The rumble moves the foreground's view by a plain offset from the camera's row: in Crocomire's one-screen-high
        // room the camera's row is the room's top, so the view rises above it, a negative row (#2384).
        _backgroundScroll.ShiftForegroundViewFromCamera(crocomire.Death.RumbleYOffset);
    }

    /// <summary>
    /// Reconstructs the one side effect the door transition still owns.
    /// Gameplay resumes immediately after loading the destination, so the arriving
    /// downward actor can cross its pseudo-door while the arrival statuses are visible to
    /// main gameplay. Retain its moving movement mode, but permit its door block's reaction to
    /// publish a real transition or elevator flag only while departing; otherwise a
    /// collapsed transition can immediately depart again.
    /// </summary>
    internal static bool ShouldPublishFacingForwardElevatorDoorSideEffects(
        ElevatorActorStatus status) => status == ElevatorActorStatus.Departing;

    /// <summary>
    /// The block animations' step: gameplay, the door
    /// transition's scroll and every fade step take it. The X-Ray Scope's setup
    /// disables the block animations, freezing every animation until the scope ends.
    /// </summary>
    /// <param name="room">The loaded room, whose area's boss the treadmills wait for.</param>
    /// <param name="samus">Samus, whose X-Ray Scope holds the animations still.</param>
    internal void RunBlockAnimations(RoomScope room, SamusState samus)
    {
        if (samus.Xray.AreBlockAnimationsSuspended)
            return;
        // The treadmills, the effect's and the one a Wrecked Ship entrance door starts, show a frame a pass only after
        // Phantoon is defeated.
        room.BlockAnimations.Step(_system.HasDefeatedAnyOf(room.Room.Area, DefeatedBosses.AreaBoss), room.ArtworkFrames);
        room.TourianStatues?.StepBlockAnimations();
    }

    /// <summary>
    /// Whether Samus is drawn this update while she rides an elevator: every second update, so she flickers as the
    /// elevator carries her. The flicker is the game's own, counted in updates, whatever the display's refresh.
    /// </summary>
    internal static bool ShouldDrawSamusOnElevator(int frameCounter) =>
        SamusCadenceDefinitions.ElevatorRideDrawnEverySecondUpdate.IsDue(frameCounter);

    /// <summary>
    /// Executes the Falling Tile room main. It samples the existing RNG
    /// value rather than generating a new one, preserving subsequent random consumers.
    /// </summary>
    /// <param name="room">The pre-elevator hall.</param>
    /// <param name="debris">The hall's countdown to the next falling tile.</param>
    internal void RunCeresFallingDebrisRoomMain(RoomScope room, CeresFallingDebrisRoomMainState debris)
    {
        if (room.Enemies.Ceres.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            return;

        // The countdown starts from what the previous room's main left, normally the shaft's turn step.
        if (!debris.Step())
            return;

        int random = _system.RandomNumber;
        CeresFallingDebrisSegment look = CeresFallingDebrisDefinitions.PieceIsDark.IsTakenBy(random)
            ? CeresFallingDebrisSegment.Dark
            : CeresFallingDebrisSegment.Light;
        room.Enemies.CeresFallingTiles.Spawn(
            CeresFallingDebrisDefinitions.ColumnX.Choose(random),
            new CeresFallingDebrisAnimation(look),
            CeresFallingDebrisVisualDefinitions.Graphics);
    }
}
