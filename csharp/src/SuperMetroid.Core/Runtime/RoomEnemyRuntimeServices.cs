using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The runtime's cartridge state as the services one room's enemy AI calls (#633): the shared
/// RNG, event and boss bits, the room's scroll zones, Samus's control lock, Mother Brain's room
/// effects and the active PLM population. Boss bits address the room's area.
/// </summary>
internal sealed class RoomEnemyRuntimeServices(
    Bank80SystemState system,
    ActiveRoomState room,
    SamusFrameState player,
    RoomPlmSystem plms,
    GameplayDisplayState display,
    BackgroundScrollState backgroundScroll,
    AreaId area) : IRoomEnemyServices,
    IEnemyRandomNumbers, IGlobalEventFlags, IAreaBossFlags, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomPlmPresence
{
    /// <summary>Exposes the system-owned RNG to enemy AI without a raw system-state dependency.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes persistent global event bits to enemy AI.</summary>
    public IGlobalEventFlags Events => this;
    /// <summary>Exposes boss bits for the active room's area.</summary>
    public IAreaBossFlags Bosses => this;
    /// <summary>Exposes active PLM-header presence to enemy AI.</summary>
    public IRoomPlmPresence Plms => this;
    /// <summary>Exposes the room camera's scroll-zone storage.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Exposes Samus's script control lock to enemy commands.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes Mother Brain's glass, blending, and BG2 room effects.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances the shared system RNG once for an enemy request.</summary>
    public ushort Generate() => system.NextRandom();

    /// <summary>Observes the current RNG word without advancing it.</summary>
    public ushort Read() => system.RandomNumber;

    /// <summary>Installs an enemy-supplied seed into the shared system RNG.</summary>
    public void Seed(ushort value) => system.SetRandomNumber(value);

    /// <summary>Checks a persistent event bit from the system owner.</summary>
    public bool Has(EventNumber eventNumber) => system.HasEvent(eventNumber);

    /// <summary>Publishes an event bit through the system owner.</summary>
    public void Set(EventNumber eventNumber) => system.SetEvent(eventNumber);

    /// <summary>Clears an event bit through the system owner.</summary>
    public void Clear(EventNumber eventNumber) => system.ClearEvent(eventNumber);

    /// <summary>Checks whether any requested boss bit is set for this room's area.</summary>
    public bool IsDefeated(BossBits bits) => system.HasAnyBossBits(area, bits);

    /// <summary>Sets boss bits in the active room's area record.</summary>
    public void SetDefeated(BossBits bits) => system.SetBossBits(area, bits);

    /// <summary>Reads a camera scroll-zone cell without mutating its storage.</summary>
    RoomScrollState IRoomScrollZoneAccess.Read(int index) => room.LoadedCamera.Scrolls.ReadState(index);

    /// <summary>Writes one camera scroll-zone cell through the room's storage owner.</summary>
    public void Write(int index, RoomScrollState state) => room.LoadedCamera.Scrolls.SetStorage(index, state);

    /// <summary>Pairs Samus's stationary script lock with the player's grounded-movement gate.</summary>
    public void SetControlsEnabled(bool enabled)
    {
        SamusState samus = player.Samus ?? throw new InvalidOperationException(
            "Chozo control command requires an active Samus actor.");
        samus.ScriptLocks.SetStationaryScriptControlLock(!enabled);
        player.GroundedSamusMovementEnabled = enabled;
    }

    /// <summary>Advances the Mother Brain glass PLM argument after an enemy-driven phase event.</summary>
    public void IncrementGlassRoomArgument() => plms.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();

    /// <summary>Installs Mother Brain's requested default layer-blending configuration.</summary>
    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        display.LayerBlendingDefaultConfig = configuration;

    /// <summary>Writes the room's BG2 scroll registers through the background owner.</summary>
    public void SetBg2Scroll(Position16<Bg2Pixels> scroll) => backgroundScroll.SetBg2ScrollRegisters(scroll);

    /// <summary>Checks whether a requested Bomb Torizo hand PLM header remains active.</summary>
    public bool HasActiveHeader(PlmHeaderId header) => plms.BombTorizoHand.HasActiveHeader(header);
}
