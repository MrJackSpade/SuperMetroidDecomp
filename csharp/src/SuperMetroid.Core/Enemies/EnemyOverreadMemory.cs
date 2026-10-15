namespace SuperMetroid.Core.Enemies;

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
