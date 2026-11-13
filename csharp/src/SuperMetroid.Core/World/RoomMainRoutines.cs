using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room-main behaviours: Ceres falling debris, the Crocomire comeback, animated tiles, and elevator drawing rules.
/// </summary>
internal sealed class RoomMainRoutines
{
    /// <summary>The layer scrolls adjusted during Crocomire's hidden-wall rumble.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Samus state consulted for X-ray animation suspension.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Active room behaviour and area identity used to dispatch room-main work.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Boss bits and the random number sampled by room-main behaviours.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room, enemy, tile, and memory owners used by room-main behaviours.</summary>
    internal RoomMainRoutines(RoomMainRoutinesDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }
    /// <summary>The hidden-wall branch of Crocomire's room shaking.</summary>
    internal void RunCrocomireComebackRoomMain(RoomScope room)
    {
        if (_room.ActiveRoom?.State.MainBehavior != RoomMainBehavior.CrocomireRoomShaking ||
            room.Enemies.RoomMainHooks.Crocomire is not { } boss ||
            boss.Body.Presence.HasAny(EnemyPresence.Hidden) ||
            boss.DeathPhase != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        var death = room.Enemies.RoomMainHooks.CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire's room shaking found no Crocomire death in progress.");
        _backgroundScroll.SetForegroundVerticalScroll(CoordinateAxis.Wrap(
            _backgroundScroll.ForegroundPosition.Y + death.RumbleYOffset));
    }

    /// <summary>
    /// Reconstructs the one side effect still owned by the door-transition caller.
    /// Gameplay resumes immediately after loading the destination, so the arriving
    /// downward actor can cross its pseudo-door while statuses two/three are visible to
    /// main gameplay. Retain its moving movement mode, but permit its door-block handler to
    /// publish a real transition or elevator flag only for status one; otherwise a
    /// collapsed transition can immediately depart again.
    /// </summary>
    internal static bool ShouldPublishFacingForwardElevatorDoorSideEffects(
        ElevatorActorStatus status) => status == ElevatorActorStatus.Departing;

    /// <summary>Most recent Ceres elevator shaft room-main call.</summary>
    public CeresElevatorShaftRoomMainResult LastCeresElevatorShaftRoomMain { get; internal set; }

    /// <summary>
    /// The animated tiles objects' step: gameplay and the door
    /// transition's scroll and every fade step call it. The X-Ray Scope's setup
    /// disables the animated tiles objects, freezing every object until the scope ends.
    /// </summary>
    internal void RunAnimatedTilesObjectHandler(RoomScope room)
    {
        if (_player.Samus?.Xray.AreAnimatedTilesSuspended == true)
            return;
        room.SandTiles.Step(room.ArtworkFrames);
        room.SpikeTiles.Step(room.ArtworkFrames);
        if (_room.ActiveRoom is not null)
            room.TreadmillTiles.Step(_system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss), room.ArtworkFrames);
        room.TourianStatues.StepTiles();

        // The Wrecked Ship treadmill door code creates an ordinary animated-tile object. Its
        // handler shows one treadmill frame a pass only after Phantoon's area-boss bit is set.
        if (room.WreckedShipTreadmill.IsActive)
        {
            AreaId areaIndex = _room.ActiveRoom?.AreaIndex ?? throw new InvalidOperationException(
                "A live Wrecked Ship treadmill animation has no active room.");
            room.WreckedShipTreadmill.Step(
                _system.HasAnyBossBits(areaIndex, BossBits.AreaBoss),
                room.ArtworkFrames);
        }
    }

    /// <summary>
    /// Implements the complete visibility test of Samus's display while she rides an elevator. The retail handler returns on odd frames and calls the fatal/no-
    /// animation body renderer on even frames, producing the deliberate 30 Hz elevator
    /// flicker. Keeping the predicate named prevents host display cadence (including RDP)
    /// from being mistaken for the game-authored effect.
    /// </summary>
    internal static bool ShouldDrawSamusOnElevator(int frameCounter) =>
        SamusCadenceDefinitions.ElevatorRideDrawnEverySecondUpdate.IsDue(frameCounter);

    /// <summary>
    /// Executes the Falling Tile room main. It samples the existing RNG
    /// value rather than generating a new one, preserving subsequent random consumers.
    /// </summary>
    internal void RunCeresFallingDebrisRoomMain(RoomScope room)
    {
        if (_room.ActiveRoom?.State.MainBehavior !=
                RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris ||
            room.Enemies.Ceres.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            return;

        // The countdown starts from what the previous room's main left, normally the shaft's turn step.
        if (!room.CeresFallingDebris.Step())
            return;

        int random = _system.RandomNumber;
        bool dark = CeresFallingDebrisDefinitions.Dark.IsSetIn(random);
        room.Enemies.Projectiles.SpawnCeresFallingDebris(
            CeresFallingDebrisX(CeresFallingDebrisDefinitions.Column.Of(random)),
            dark,
            new CeresFallingDebrisAnimation(dark ? CeresFallingDebrisSegment.Dark : CeresFallingDebrisSegment.Light),
            CeresFallingDebrisVisualDefinitions.Graphics);
    }

    /// <summary>
    /// Sixteen debris columns, sixteen pixels apart from X 80 through 240, then
    /// thirty-two pixels apart through 400.
    /// </summary>
    internal static int CeresFallingDebrisX(int column) => (uint)column < 16
        ? CoordinateAxis.Wrap(column <= 10 ? 80 + 16 * column : 240 + 32 * (column - 10))
        : throw new ArgumentOutOfRangeException(nameof(column));
}
