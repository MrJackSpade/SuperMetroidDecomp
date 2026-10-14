namespace SuperMetroid.Core.Game;

/// <summary>Typed view of the persistent-progress SRAM mirror in WRAM (#629).</summary>
[CpuBusBoundary("domain view: persistent-progress SRAM mirror in WRAM")]
public readonly struct SaveMirrorMemory(ISnesMemoryBus bus)
{
    /// <summary>The persistent event bitset.</summary>
    public WorkRamByteTable Events => new(bus, SaveMirrorWorkRam.Events);

    /// <summary>The per-area boss bitset.</summary>
    public WorkRamByteTable BossBits => new(bus, SaveMirrorWorkRam.BossBits);

    /// <summary>The room-indexed opened-Chozo-orb bitset.</summary>
    public WorkRamByteTable RoomChozoBits => new(bus, SaveMirrorWorkRam.RoomChozoBits);

    /// <summary>The room-indexed collected-item bitset.</summary>
    public WorkRamByteTable CollectedItemBits => new(bus, SaveMirrorWorkRam.CollectedItemBits);

    /// <summary>The room-indexed opened-door bitset.</summary>
    public WorkRamByteTable OpenedDoorBits => new(bus, SaveMirrorWorkRam.OpenedDoorBits);

    /// <summary>The per-area used save-station and elevator bitset.</summary>
    public WorkRamByteTable UsedSaveStations => new(bus, SaveMirrorWorkRam.UsedSaveStations);

    /// <summary>The per-area acquired map-station flags.</summary>
    public WorkRamByteTable MapStations => new(bus, SaveMirrorWorkRam.MapStations);

    /// <summary>The raw saved frontend dispatcher word.</summary>
    public ushort LoadingGameState
    {
        get => new WramWord(bus, SaveMirrorWorkRam.LoadingGameState).Read();
        set => new WramWord(bus, SaveMirrorWorkRam.LoadingGameState).Write(value);
    }

    /// <summary>The cumulative item-PLM setup count.</summary>
    public ushort LoadedItemCount
    {
        get => new WramWord(bus, SaveMirrorWorkRam.LoadedItemCount).Read();
        set => new WramWord(bus, SaveMirrorWorkRam.LoadedItemCount).Write(value);
    }

    /// <summary>
    /// Whether a write to the CPU bytes <paramref name="first"/> through <paramref name="last"/>
    /// reaches any byte from the event bitset through the saved dispatcher word.
    /// </summary>
    internal static bool Overlaps(int first, int last) =>
        first <= SaveMirrorWorkRam.LoadingGameState.Address + SaveMirrorWorkRam.LoadingGameState.ByteCount - 1 &&
        last >= SaveMirrorWorkRam.Events.Address;
}
