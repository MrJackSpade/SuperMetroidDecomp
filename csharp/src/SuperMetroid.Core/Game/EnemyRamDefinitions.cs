namespace SuperMetroid.Core.Game;

/// <summary>Layout of the native enemy RAM block (<c>EnemyData</c>, $0F78..$177F).</summary>
public static class EnemyRamDefinitions
{
    /// <summary><c>Enemy.ID</c> of slot zero: WRAM $0F78.</summary>
    public const int FirstSlotAddress = 0x0f78;

    /// <summary>Bytes per enemy record.</summary>
    public const int SlotStride = 0x40;
}
