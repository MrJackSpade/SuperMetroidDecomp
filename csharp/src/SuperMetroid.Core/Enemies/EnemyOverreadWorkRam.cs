namespace SuperMetroid.Core.Enemies;

/// <summary>Unrelated WRAM words that enemy code reads through native table or operand quirks (#629).</summary>
[CpuBusBoundary("catalog: enemy over-read WRAM words")]
[SnesEncodingOwner]
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
