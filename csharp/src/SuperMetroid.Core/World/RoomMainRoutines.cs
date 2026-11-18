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
    /// <summary>Samus state consulted for X-ray animation suspension.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Active room behaviour and area identity used to select room-main work.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Defeated bosses and the random number sampled by room-main behaviours.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room, enemy, picture, and Samus owners used by room-main behaviours.</summary>
    internal RoomMainRoutines(RoomMainRoutinesDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }
    /// <summary>Crocomire's room shaking while his comeback hides the wall.</summary>
    internal void RunCrocomireComebackRoomMain(RoomScope room)
    {
        if (room.Enemies.RoomMainHooks.Crocomire is not { } boss ||
            boss.Body.Presence.HasAny(EnemyPresence.Hidden) ||
            boss.DeathPhase != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        var death = room.Enemies.RoomMainHooks.CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire's room shaking found no Crocomire death in progress.");
        // The rumble moves the foreground's view by a plain offset from the camera's row: in Crocomire's one-screen-high
        // room the camera's row is the room's top, so the view rises above it, a negative row (#2384).
        _backgroundScroll.ShiftForegroundViewFromCamera(death.RumbleYOffset);
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
    internal void RunBlockAnimations(RoomScope room)
    {
        if (_player.Samus?.Xray.AreBlockAnimationsSuspended == true)
            return;
        // The treadmills, the effect's and the one a Wrecked Ship entrance door starts, show a frame a pass only after
        // Phantoon is defeated.
        room.BlockAnimations.Step(_system.HasDefeatedAnyOf(_room.LoadedRoom.Area, DefeatedBosses.AreaBoss), room.ArtworkFrames);
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
        bool dark = CeresFallingDebrisDefinitions.Dark.IsSetIn(random);
        room.Enemies.CeresFallingTiles.Spawn(
            CeresFallingDebrisDefinitions.ColumnX[CeresFallingDebrisDefinitions.Column.Of(random)],
            dark,
            new CeresFallingDebrisAnimation(dark ? CeresFallingDebrisSegment.Dark : CeresFallingDebrisSegment.Light),
            CeresFallingDebrisVisualDefinitions.Graphics);
    }
}
