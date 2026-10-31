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
/// Room-main routines: Ceres falling debris, the Crocomire comeback, animated tiles, and elevator drawing rules.
/// </summary>
internal sealed class RoomMainRoutines
{
    /// <summary>BG1 scroll registers adjusted during Crocomire's hidden-wall rumble.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Samus state consulted for X-ray animation suspension.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Active room callback and area identity used to dispatch room-main work.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Boss bits and RNG word sampled by room-main callbacks.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the room, enemy, tile, and memory owners used by room-main callbacks.</summary>
    internal RoomMainRoutines(RoomMainRoutinesDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }
    /// <summary>The hidden-wall branch of MainASM_CrocomiresRoomShaking at $8F:E8CD/$E942.</summary>
    internal void RunCrocomireComebackRoomMain(RoomScope room)
    {
        if (_room.ActiveRoom?.State.MainCallback != RoomMainCallback.CrocomireRoomShaking ||
            room.Enemies.RoomMainHooks.Crocomire is not { } boss ||
            boss.Body.Properties.HasAny(EnemyProperties.Invisible) ||
            boss.DeathSequenceIndex != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        var death = room.Enemies.RoomMainHooks.CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire's comeback room callback has no death state.");
        _backgroundScroll.SetBg1VerticalScroll(CoordinateAxis.Wrap(
            _backgroundScroll.Layer1Position.Y + _backgroundScroll.Bg1Offset.Y + death.RumbleYOffset));
    }

    /// <summary>
    /// Reconstructs the one side effect still owned by the native door-transition caller.
    /// This host resumes gameplay immediately after loading the destination, so the arriving
    /// downward actor can cross its pseudo-door while statuses two/three are visible to
    /// state eight. Retain its moving beta handler, but permit its type-$9 handler to
    /// publish a real transition or elevator flag only for status one; otherwise a
    /// collapsed transition can immediately depart again.
    /// </summary>
    internal static bool ShouldPublishFacingForwardElevatorDoorSideEffects(
        ElevatorActorStatus status) => status == ElevatorActorStatus.Departing;

    /// <summary>Most recent <c>$89:ACC3</c> room-main call.</summary>
    public CeresElevatorShaftRoomMainResult LastCeresElevatorShaftRoomMain { get; internal set; }

    /// <summary>
    /// <c>AnimatedTilesObject_Handler</c> ($87:8064): gameplay ($82:8B75) and the door
    /// transition's $82:E659 and every $82:E737 fade step call it. XraySetup's
    /// Disable_AnimatedTilesObjects ($91:E239) freezes every object until the scope ends.
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

        // Door ASM $B971/$E1D8 creates an ordinary bank-$87 animated-tile object. Its
        // handler shows one treadmill frame a pass only after Phantoon's area-boss bit is set.
        if (room.WreckedShipTreadmill.IsActive)
        {
            AreaId areaIndex = _room.ActiveRoom?.AreaIndex ?? throw new InvalidOperationException(
                "A live Wrecked Ship treadmill animation has no active cartridge room.");
            room.WreckedShipTreadmill.Step(
                _system.HasAnyBossBits(areaIndex, BossBits.AreaBoss),
                room.ArtworkFrames);
        }
    }

    /// <summary>
    /// Implements the complete visibility test in <c>SamusDisplayHandler_UsingElevator</c>
    /// at <c>$90:EC14</c>. The retail handler returns on odd frames and calls the fatal/no-
    /// animation body renderer on even frames, producing the deliberate 30 Hz elevator
    /// flicker. Keeping the predicate named prevents host display cadence (including RDP)
    /// from being mistaken for the cartridge-authored effect.
    /// </summary>
    internal static bool ShouldDrawSamusOnElevator(int frameCounter) =>
        (frameCounter & 1) == 0;

    /// <summary>
    /// Executes Falling Tile room main <c>$8F:E525-$E550</c>. It samples the existing RNG
    /// word rather than generating a new one, preserving subsequent random consumers.
    /// </summary>
    internal void RunCeresFallingDebrisRoomMain(RoomScope room)
    {
        if (_room.ActiveRoom?.State.MainCallback !=
                RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris ||
            room.Enemies.Ceres.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            return;

        // The countdown starts from what the previous room's main left, normally the shaft's turn step.
        if (!room.CeresFallingDebris.Step())
            return;

        int random = _system.RandomNumber;
        bool dark = WordWrap.Signed(random) < 0;
        room.Enemies.Projectiles.SpawnCeresFallingDebris(
            CeresFallingDebrisX(random & 15),
            dark,
            new CeresFallingDebrisAnimation(dark ? CeresFallingDebrisProgram.Dark : CeresFallingDebrisProgram.Light),
            CeresFallingDebrisVisualDefinitions.Graphics);
    }

    /// <summary>
    /// $8F:E551: sixteen debris columns, sixteen pixels apart from X $50 through $F0, then
    /// thirty-two pixels apart through $190.
    /// </summary>
    internal static int CeresFallingDebrisX(int column) => (uint)column < 16
        ? CoordinateAxis.Wrap(column <= 10 ? 80 + 16 * column : 240 + 32 * (column - 10))
        : throw new ArgumentOutOfRangeException(nameof(column));
}
