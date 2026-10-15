namespace SuperMetroid.Core.Saves;

/// <summary>Typed view of the persistent-progress SRAM mirror in WRAM (#629).</summary>
[CpuBusBoundary("domain view: persistent-progress SRAM mirror in WRAM")]
public readonly struct SaveMirrorMemory(ISnesMemoryBus bus)
{
    /// <summary>The persistent event bitset.</summary>
    public WorkRamByteTable Events => new(bus, SaveMirrorWorkRamAddresses.Events);

    /// <summary>The per-area boss bitset.</summary>
    public WorkRamByteTable BossBits => new(bus, SaveMirrorWorkRamAddresses.BossBits);

    /// <summary>The room-indexed opened-Chozo-orb bitset.</summary>
    public WorkRamByteTable RoomChozoBits => new(bus, SaveMirrorWorkRamAddresses.RoomChozoBits);

    /// <summary>The room-indexed collected-item bitset.</summary>
    public WorkRamByteTable CollectedItemBits => new(bus, SaveMirrorWorkRamAddresses.CollectedItemBits);

    /// <summary>The room-indexed opened-door bitset.</summary>
    public WorkRamByteTable OpenedDoorBits => new(bus, SaveMirrorWorkRamAddresses.OpenedDoorBits);

    /// <summary>The per-area used save-station and elevator bitset.</summary>
    public WorkRamByteTable UsedSaveStations => new(bus, SaveMirrorWorkRamAddresses.UsedSaveStations);

    /// <summary>The per-area acquired map-station flags.</summary>
    public WorkRamByteTable MapStations => new(bus, SaveMirrorWorkRamAddresses.MapStations);

    /// <summary>The raw saved frontend dispatcher word.</summary>
    public ushort LoadingGameState
    {
        get => new WramWord(bus, SaveMirrorWorkRamAddresses.LoadingGameState).Read();
        set => new WramWord(bus, SaveMirrorWorkRamAddresses.LoadingGameState).Write(value);
    }

    /// <summary>The cumulative item-PLM setup count.</summary>
    public ushort LoadedItemCount
    {
        get => new WramWord(bus, SaveMirrorWorkRamAddresses.LoadedItemCount).Read();
        set => new WramWord(bus, SaveMirrorWorkRamAddresses.LoadedItemCount).Write(value);
    }

    /// <summary>
    /// Whether a write to the CPU bytes <paramref name="first"/> through <paramref name="last"/>
    /// reaches any byte from the event bitset through the saved dispatcher word.
    /// </summary>
    internal static bool Overlaps(int first, int last) =>
        first <= SaveMirrorWorkRamAddresses.LoadingGameState.Address + SaveMirrorWorkRamAddresses.LoadingGameState.ByteCount - 1 &&
        last >= SaveMirrorWorkRamAddresses.Events.Address;
}
