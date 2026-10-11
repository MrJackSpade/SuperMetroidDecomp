using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Unrelated WRAM words that enemy code reads through native table or operand quirks (#629).</summary>
[CpuBusBoundary("catalog: enemy over-read WRAM words")]
public static class EnemyOverreadWorkRam
{
    /// <summary>
    /// WRAM $003A, an unnamed direct-page word: <c>EnemyHeaders_dropChances</c> ($86:F120) indexed by
    /// header pointer zero reads $A0:003A, which LoROM mirrors here. A death explosion spawned from a
    /// cleared enemy slot takes its drop-chance pointer from it.
    /// </summary>
    public static readonly CpuBusCell ClearedHeaderDropChances = CpuBusCell.WramWord(nameof(ClearedHeaderDropChances),
        0x00003a, "drop-chance table pointer (u16)");

    /// <summary>
    /// $7E:0100, the stack-page word Spark's initializer ORs into its properties through the
    /// absolute-memory <c>ORA $0100</c> the disassembly notes as a bug (it means <c>ORA #$0100</c>).
    /// </summary>
    public static readonly CpuBusCell SparkPropertyOverlay = CpuBusCell.WramWord(nameof(SparkPropertyOverlay),
        0x7e0100, "EnemyProperties bits (u16)");
}

/// <summary>Typed view of the WRAM words enemy code over-reads (#629).</summary>
[CpuBusBoundary("domain view: enemy over-read WRAM words")]
public readonly struct EnemyOverreadMemory(ISnesMemoryBus bus)
{
    /// <summary>The drop-chance pointer a cleared enemy header's explosion inherits.</summary>
    [CpuBusViewOf(typeof(EnemyOverreadWorkRam), nameof(EnemyOverreadWorkRam.ClearedHeaderDropChances))]
    public ushort ClearedHeaderDropChances => new WramWord(bus, EnemyOverreadWorkRam.ClearedHeaderDropChances).Read();

    /// <summary>The live stack-page word Spark's initializer ORs into its properties.</summary>
    [CpuBusViewOf(typeof(EnemyOverreadWorkRam), nameof(EnemyOverreadWorkRam.SparkPropertyOverlay))]
    public ushort SparkPropertyOverlay => new WramWord(bus, EnemyOverreadWorkRam.SparkPropertyOverlay).Read();
}
