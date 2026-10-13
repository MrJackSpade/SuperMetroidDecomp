using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Room-main routines: Ceres falling debris, the Crocomire comeback, animated tiles, and elevator drawing rules.
/// </summary>
internal sealed class RoomMainRoutines
{
    /// <summary>BG1 scroll registers adjusted during Crocomire's hidden-wall rumble.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Enemy owners used by the Crocomire callback and Ceres debris spawn.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Samus state consulted for X-ray animation suspension.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Active room callback and area identity used to dispatch room-main work.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Room spike tile animation advanced by the bank-$87 handler.</summary>
    private readonly RoomSpikeAnimatedTilesState _roomSpikes;
    /// <summary>Room treadmill tile animation advanced when a room is active.</summary>
    private readonly RoomTreadmillAnimatedTilesState _roomTreadmills;
    /// <summary>Sand tile animation advanced before other room tile objects.</summary>
    private readonly RoomSandAnimatedTilesState _sandAnimatedTiles;
    /// <summary>Boss bits and RNG word sampled by room-main callbacks.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Tourian statue tile changes advanced alongside room animations.</summary>
    private readonly TourianStatueSequence _tourianStatues;
    /// <summary>VRAM target for animated sand and spike tiles.</summary>
    private readonly SnesVram _vram;
    /// <summary>Queued tile transfers consumed at the next accepted NMI.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Wrecked Ship treadmill animation gated by Phantoon's boss bit.</summary>
    private readonly WreckedShipTreadmillAnimatedTilesState _wreckedShipTreadmill;
    /// <summary>Memory context supplied to the room animated-tile handlers.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Binds the room, enemy, tile, and memory owners used by room-main callbacks.</summary>
    internal RoomMainRoutines(RoomMainRoutinesDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _enemies = dependencies.Enemies;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomSpikes = dependencies.RoomSpikes;
        _roomTreadmills = dependencies.RoomTreadmills;
        _sandAnimatedTiles = dependencies.SandAnimatedTiles;
        _system = dependencies.System;
        _tourianStatues = dependencies.TourianStatues;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _wreckedShipTreadmill = dependencies.WreckedShipTreadmill;
        _addressSpace = dependencies.AddressSpace;
    }
    /// <summary>The hidden-wall branch of MainASM_CrocomiresRoomShaking at $8F:E8CD/$E942.</summary>
    internal void RunCrocomireComebackRoomMain()
    {
        if (_room.ActiveRoom?.State.MainCallback != RoomMainCallback.CrocomireRoomShaking ||
            _enemies.CrocomireBehavior.Crocomire is not { } boss ||
            boss.Body.Properties.HasAny(EnemyProperties.Invisible) ||
            boss.DeathSequenceIndex != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        var death = _enemies.CrocomireBehavior.CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire's comeback room callback has no death state.");
        _backgroundScroll.SetBg1VerticalScrollRegister(unchecked((ushort)(
            _backgroundScroll.Layer1Position.Y + _backgroundScroll.Bg1Offset.Y + death.RumbleYOffset)));
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
    internal void RunAnimatedTilesObjectHandler()
    {
        if (_player.Samus?.Xray.AreAnimatedTilesSuspended == true)
            return;
        _sandAnimatedTiles.Step(_addressSpace, _vram, _vramWrites);
        _roomSpikes.Step(_addressSpace, _vram, _vramWrites);
        if (_room.ActiveRoom is not null)
            _roomTreadmills.Step(_addressSpace,
                _system.HasAnyBossBits(_room.ActiveRoom.AreaIndex, BossBits.AreaBoss), _vramWrites);
        _tourianStatues.StepTiles();

        // Door ASM $B971/$E1D8 creates an ordinary bank-$87 animated-tile object. Its
        // handler publishes one 32-byte source per frame only after Phantoon's area-boss
        // bit is set; NMI consumes the queued transfer on the following accepted frame.
        if (_wreckedShipTreadmill.IsActive)
        {
            AreaId areaIndex = _room.ActiveRoom?.AreaIndex ?? throw new InvalidOperationException(
                "A live Wrecked Ship treadmill animation has no active cartridge room.");
            _wreckedShipTreadmill.Step(
                _addressSpace,
                _system.HasAnyBossBits(areaIndex, BossBits.AreaBoss),
                _vramWrites);
        }
    }

    /// <summary>
    /// Implements the complete visibility test in <c>SamusDisplayHandler_UsingElevator</c>
    /// at <c>$90:EC14</c>. The retail handler returns on odd NMIs and calls the fatal/no-
    /// animation body renderer on even NMIs, producing the deliberate 30 Hz elevator
    /// flicker. Keeping the predicate named prevents host display cadence (including RDP)
    /// from being mistaken for the cartridge-authored effect.
    /// </summary>
    internal static bool ShouldDrawSamusOnElevator(ushort nmiFrameCounter) =>
        (nmiFrameCounter & 1) == 0;

    /// <summary>
    /// Executes Falling Tile room main <c>$8F:E525-$E550</c>. It samples the existing RNG
    /// word rather than generating a new one, preserving subsequent random consumers.
    /// </summary>
    internal void RunCeresFallingDebrisRoomMain()
    {
        if (_room.ActiveRoom?.State.MainCallback !=
                RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris ||
            _enemies.RidleyEncounter.CeresStatus == 0)
            return;

        // The timer is RoomMainASMVar1, inherited from the shaft's rotation index.
        NativeWordCounterStep timer = _room.RoomMainScratch.Countdown.Decrement();
        _room.RoomMainScratch.Countdown = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        _room.RoomMainScratch.Countdown = NativeCountdown.Start(8);
        ushort random = _system.RandomNumber;
        _enemies.Projectiles.SpawnCeresFallingDebris(
            CeresFallingDebrisX(random & 0x000f),
            dark: (random & 0x8000) != 0);
    }

    /// <summary>
    /// $8F:E551: sixteen debris columns, sixteen pixels apart from X $50 through $F0, then
    /// thirty-two pixels apart through $190.
    /// </summary>
    internal static ushort CeresFallingDebrisX(int column) => (uint)column < 16
        ? (ushort)(column <= 10 ? 0x0050 + 0x10 * column : 0x00f0 + 0x20 * (column - 10))
        : throw new ArgumentOutOfRangeException(nameof(column));
}
