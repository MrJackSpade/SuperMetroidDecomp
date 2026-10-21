
namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values of <see cref="EnemyDropChanceDefinitions"/> that only verification reads.</summary>
internal static class EnemyDropChanceDefinitionsConstants
{
    /// <summary>Pointer of the final six-byte item-drop probability record.</summary>
    public const ushort LastPointer = 0xf4b2;
    /// <summary>Bank-$B4 address used by native enemy drop probability pointers.</summary>
    public const int NativeBank = 0xb40000;

    /// <summary>Pointer of the first six-byte item-drop probability record.</summary>
    public const ushort FirstPointer = 0xf1f4;

    /// <summary>The bank-$B4 address of <paramref name="table"/>.</summary>
    internal static ushort Pointer(EnemyDropTable table) =>
        checked((ushort)(FirstPointer + table.Record * EnemyDropChanceDefinitions.RecordSize));

    /// <summary>Provides the enemy drop chance bank and last pointer used to bound native drop tables.</summary>
    extension(EnemyDropChanceDefinitions)
    {
        /// <inheritdoc cref="EnemyDropChanceDefinitionsConstants.LastPointer"/>
        internal static ushort LastPointer => EnemyDropChanceDefinitionsConstants.LastPointer;
        /// <inheritdoc cref="EnemyDropChanceDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => EnemyDropChanceDefinitionsConstants.NativeBank;
    }
}
