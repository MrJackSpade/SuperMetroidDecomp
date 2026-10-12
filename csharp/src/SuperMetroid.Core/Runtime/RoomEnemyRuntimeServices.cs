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
    /// <summary>Gets random.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Gets events.</summary>
    public IGlobalEventFlags Events => this;
    /// <summary>Gets bosses.</summary>
    public IAreaBossFlags Bosses => this;
    /// <summary>Gets plms.</summary>
    public IRoomPlmPresence Plms => this;
    /// <summary>Gets room scroll.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Gets samus controls.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Gets mother brain.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Generate.</summary>
    public ushort Generate() => system.NextRandom();

    /// <summary>Read.</summary>
    public ushort Read() => system.RandomNumber;

    /// <summary>Seed.</summary>
    public void Seed(ushort value) => system.SetRandomNumber(value);

    /// <summary>Has.</summary>
    public bool Has(EventNumber eventNumber) => system.HasEvent(eventNumber);

    /// <summary>Set.</summary>
    public void Set(EventNumber eventNumber) => system.SetEvent(eventNumber);

    /// <summary>Clear.</summary>
    public void Clear(EventNumber eventNumber) => system.ClearEvent(eventNumber);

    /// <summary>Checks whether defeated.</summary>
    public bool IsDefeated(BossBits bits) => system.HasAnyBossBits(area, bits);

    /// <summary>Sets defeated.</summary>
    public void SetDefeated(BossBits bits) => system.SetBossBits(area, bits);

    /// <summary>Read.</summary>
    RoomScrollState IRoomScrollZoneAccess.Read(int index) => room.Camera.Scrolls.ReadState(index);

    /// <summary>Write.</summary>
    public void Write(int index, RoomScrollState state) => room.Camera.Scrolls.SetStorage(index, state);

    /// <summary>Sets controls enabled.</summary>
    public void SetControlsEnabled(bool enabled)
    {
        SamusState samus = player.Samus ?? throw new InvalidOperationException(
            "Chozo control command requires an active Samus actor.");
        samus.ScriptLocks.SetStationaryScriptControlLock(!enabled);
        player.GroundedSamusMovementEnabled = enabled;
    }

    /// <summary>Increments glass room argument.</summary>
    public void IncrementGlassRoomArgument() => plms.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();

    /// <summary>Sets layer blending default.</summary>
    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        display.LayerBlendingDefaultConfig = configuration;

    /// <summary>Sets bg2 scroll.</summary>
    public void SetBg2Scroll(Position16<Bg2Pixels> scroll) => backgroundScroll.SetBg2ScrollRegisters(scroll);

    /// <summary>Checks whether active header.</summary>
    public bool HasActiveHeader(PlmHeaderId header) => plms.BombTorizoHand.HasActiveHeader(header);
}
