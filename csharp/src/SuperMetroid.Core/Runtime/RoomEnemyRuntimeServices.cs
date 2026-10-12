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
    public IEnemyRandomNumbers Random => this;
    public IGlobalEventFlags Events => this;
    public IAreaBossFlags Bosses => this;
    public IRoomPlmPresence Plms => this;
    public IRoomScrollZoneAccess RoomScroll => this;
    public ISamusControlLock SamusControls => this;
    public IMotherBrainRoomEffects MotherBrain => this;

    public ushort Generate() => system.NextRandom();

    public ushort Read() => system.RandomNumber;

    public void Seed(ushort value) => system.SetRandomNumber(value);

    public bool Has(EventNumber eventNumber) => system.HasEvent(eventNumber);

    public void Set(EventNumber eventNumber) => system.SetEvent(eventNumber);

    public void Clear(EventNumber eventNumber) => system.ClearEvent(eventNumber);

    public bool IsDefeated(BossBits bits) => system.HasAnyBossBits(area, bits);

    public void SetDefeated(BossBits bits) => system.SetBossBits(area, bits);

    RoomScrollState IRoomScrollZoneAccess.Read(int index) => room.Camera.Scrolls.ReadState(index);

    public void Write(int index, RoomScrollState state) => room.Camera.Scrolls.SetStorage(index, state);

    public void SetControlsEnabled(bool enabled)
    {
        SamusState samus = player.Samus ?? throw new InvalidOperationException(
            "Chozo control command requires an active Samus actor.");
        samus.ScriptLocks.SetStationaryScriptControlLock(!enabled);
        player.GroundedSamusMovementEnabled = enabled;
    }

    public void IncrementGlassRoomArgument() => plms.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();

    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        display.LayerBlendingDefaultConfig = configuration;

    public void SetBg2Scroll(Position16<Bg2Pixels> scroll) => backgroundScroll.SetBg2ScrollRegisters(scroll);

    public bool HasActiveHeader(PlmHeaderId header) => plms.BombTorizoHand.HasActiveHeader(header);
}
