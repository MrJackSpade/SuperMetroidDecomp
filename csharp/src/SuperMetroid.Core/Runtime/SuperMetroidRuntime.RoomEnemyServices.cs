using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// The runtime's cartridge state as the services one room's enemy AI calls (#633): the shared
    /// RNG, event and boss bits, the room's scroll zones, Samus's control lock, Mother Brain's room
    /// effects and the active PLM population. Boss bits address the room's area.
    /// </summary>
    private sealed class RoomEnemyRuntimeServices(SuperMetroidRuntime runtime, AreaId area) : IRoomEnemyServices,
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

        public ushort Generate() => runtime.System.NextRandom();

        public ushort Read() => runtime.System.RandomNumber;

        public void Seed(ushort value) => runtime.System.SetRandomNumber(value);

        public bool Has(EventNumber eventNumber) => runtime.System.HasEvent(eventNumber);

        public void Set(EventNumber eventNumber) => runtime.System.SetEvent(eventNumber);

        public void Clear(EventNumber eventNumber) => runtime.System.ClearEvent(eventNumber);

        public bool IsDefeated(BossBits bits) => runtime.System.HasAnyBossBits(area, bits);

        public void SetDefeated(BossBits bits) => runtime.System.SetBossBits(area, bits);

        RoomScrollState IRoomScrollZoneAccess.Read(int index) => runtime.Camera.Scrolls.ReadState(index);

        public void Write(int index, RoomScrollState state) => runtime.Camera.Scrolls.SetStorage(index, state);

        public void SetControlsEnabled(bool enabled)
        {
            SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
                "Chozo control command requires an active Samus actor.");
            samus.SetStationaryScriptControlLock(!enabled);
            runtime.GroundedSamusMovementEnabled = enabled;
        }

        public void IncrementGlassRoomArgument() => runtime.Plms.IncrementMotherBrainGlassRoomArgument();

        public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
            runtime.LayerBlendingDefaultConfig = configuration;

        public void SetBg2Scroll(Position16<Bg2Pixels> scroll) => runtime.BackgroundScroll.SetBg2ScrollRegisters(scroll);

        public bool HasActiveHeader(PlmHeaderId header) => runtime.Plms.HasActiveHeader(header);
    }
}
