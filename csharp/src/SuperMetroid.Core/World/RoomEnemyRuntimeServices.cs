using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's cartridge state as the services one room's enemy AI calls (#633): the shared
/// RNG, event and boss bits, the room's scroll zones, Samus's control lock, Mother Brain's room
/// effects and the active PLM population. Boss bits address the room's area.
/// </summary>
[RoomLifetime]
internal sealed class RoomEnemyRuntimeServices(
    Bank80SystemState system,
    ActiveRoomState room,
    SamusFrameState player,
    RoomPlmSystem plms,
    GameplayDisplayState display,
    RoomAreaProgress progress) : IRoomEnemyServices,
    IEnemyRandomNumbers, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomPlmPresence
{
    /// <summary>Exposes the system-owned RNG to enemy AI without a raw system-state dependency.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes persistent global event bits to enemy AI.</summary>
    public IGlobalEventFlags Events => progress;
    /// <summary>Exposes boss bits for the active room's area.</summary>
    public IAreaBossFlags Bosses => progress;
    /// <summary>Exposes active PLM-header presence to enemy AI.</summary>
    public IRoomPlmPresence Plms => this;
    /// <summary>Exposes the room camera's scroll-zone storage.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Exposes Samus's script control lock to enemy commands.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes Mother Brain's glass, blending, and BG2 room effects.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances the shared system RNG once for an enemy request.</summary>
    public int Generate() => system.NextRandom();

    /// <summary>Observes the current RNG word without advancing it.</summary>
    public int Read() => system.RandomNumber;

    /// <summary>Installs an enemy-supplied seed into the shared system RNG.</summary>
    public void Seed(int value) => system.SetRandomNumber(value);

    /// <summary>Reads a camera scroll-zone cell without mutating its storage.</summary>
    RoomScrollState IRoomScrollZoneAccess.Read(int index) => room.LoadedCamera.Scrolls.ReadState(index);

    /// <summary>Writes one camera scroll-zone cell through the room's storage owner.</summary>
    public void Write(int index, RoomScrollState state) => room.LoadedCamera.Scrolls.SetStorage(index, state);

    /// <summary>Releases Samus's stationary script lock and reopens the player's grounded-movement gate.</summary>
    public void EnableControls()
    {
        ControlledSamus.Control.Unlock();
        player.GroundedSamusMovementEnabled = true;
    }

    /// <summary>Locks Samus stationary and closes the player's grounded-movement gate.</summary>
    public void DisableControls()
    {
        ControlledSamus.Control.LockStationary();
        player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>The Samus actor a Chozo control command acts on.</summary>
    private SamusState ControlledSamus => player.Samus ?? throw new InvalidOperationException(
        "Chozo control command requires an active Samus actor.");

    /// <summary>Advances the Mother Brain glass PLM argument after an enemy-driven phase event.</summary>
    public void IncrementGlassRoomArgument() => plms.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();

    /// <summary>Installs Mother Brain's requested default layer-blending configuration.</summary>
    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        display.LayerBlendingDefaultConfig = configuration;

    /// <summary>Writes tilemap words over the loaded room's BG1.</summary>
    public void DrawBg1Overlay(int firstWord, ReadOnlySpan<ushort> words) => room.LoadedScope.DrawBg1Overlay(firstWord, words);

    /// <inheritdoc />
    public void DrawBg2Overlay(int firstWord, ReadOnlySpan<ushort> words) => room.LoadedScope.DrawBg2Overlay(firstWord, words);

    /// <inheritdoc />
    public void DrawFloorMap(int firstCell, ReadOnlySpan<byte> tiles) =>
        (room.LoadedScope.FloorMap ?? throw new InvalidOperationException("Only a Ceres room has a Mode 7 map to write."))
            .Write(firstCell, tiles);

    /// <summary>Checks whether a requested Bomb Torizo hand PLM header remains active.</summary>
    public bool HasActiveHeader(PlmHeaderId header) => plms.BombTorizoHand.HasActiveHeader(header);
}
