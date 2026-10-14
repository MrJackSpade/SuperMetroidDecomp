namespace SuperMetroid.Core.Game;

/// <summary>
/// The persistent-progress fields of the live SRAM mirror at <c>$7E:D7C0-$7E:DE1B</c> that
/// bank <c>$81</c> copies to and from a save slot (#629).
/// </summary>
[CpuBusBoundary("catalog: persistent-progress SRAM mirror in WRAM")]
public static class SaveMirrorWorkRam
{
    /// <summary><c>$7E:D820-$7E:D827</c> <c>EventBits</c>: the 64 persistent event flags.</summary>
    public static readonly CpuBusCell Events = CpuBusCell.WramBytes(nameof(Events), 0x7ed820,
        Bank80SystemState.EventByteCount, "event bitset (u8[])");

    /// <summary><c>$7E:D828-$7E:D82F</c> <c>BossBits</c>: one boss-state byte per area.</summary>
    public static readonly CpuBusCell BossBits = CpuBusCell.WramBytes(nameof(BossBits), 0x7ed828,
        Bank80SystemState.AreaCount, "per-area boss bitset (u8[])");

    /// <summary><c>$7E:D830-$7E:D86F</c>: room-indexed opened-Chozo-orb bits.</summary>
    public static readonly CpuBusCell RoomChozoBits = CpuBusCell.WramBytes(nameof(RoomChozoBits), 0x7ed830,
        Bank80SystemState.RoomChozoBitByteCount, "room bitset (u8[])");

    /// <summary><c>$7E:D870-$7E:D8AF</c> <c>ItemBits</c>: room-indexed collected-item bits.</summary>
    public static readonly CpuBusCell CollectedItemBits = CpuBusCell.WramBytes(nameof(CollectedItemBits), 0x7ed870,
        Bank80SystemState.ItemBitByteCount, "room bitset (u8[])");

    /// <summary><c>$7E:D8B0-$7E:D8EF</c> <c>DoorBits</c>: room-indexed opened-door bits.</summary>
    public static readonly CpuBusCell OpenedDoorBits = CpuBusCell.WramBytes(nameof(OpenedDoorBits), 0x7ed8b0,
        Bank80SystemState.DoorBitByteCount, "room bitset (u8[])");

    /// <summary><c>$7E:D8F8-$7E:D907</c>: per-area used save-station and elevator bits.</summary>
    public static readonly CpuBusCell UsedSaveStations = CpuBusCell.WramBytes(nameof(UsedSaveStations), 0x7ed8f8,
        Bank80SystemState.UsedSaveStationByteCount, "per-area bitset (u8[])");

    /// <summary><c>$7E:D908-$7E:D913</c> <c>MapStationByteArray</c>: per-area acquired map stations.</summary>
    public static readonly CpuBusCell MapStations = CpuBusCell.WramBytes(nameof(MapStations), 0x7ed908,
        Bank80SystemState.MapStationByteCount, "per-area flags (u8[])");

    /// <summary><c>$7E:D914</c> <c>loading_game_state</c>: the saved frontend dispatcher word consumed by $82:EEB4.</summary>
    public static readonly CpuBusCell LoadingGameState = CpuBusCell.WramWord(nameof(LoadingGameState), 0x7ed914,
        "SaveLoadingGameState (u16)");

    /// <summary><c>$7E:D91A</c>: cumulative item-PLM setup count, including repeat visits.</summary>
    public static readonly CpuBusCell LoadedItemCount = CpuBusCell.WramWord(nameof(LoadedItemCount), 0x7ed91a);
}
