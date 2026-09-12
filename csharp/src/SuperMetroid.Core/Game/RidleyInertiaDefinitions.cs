namespace SuperMetroid.Core.Game;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static class RidleyInertiaDefinitions
{
    /// <summary>$A6:D62F: the LDA absolute,Y opcode following RidleyInertiaTable. The existing Norfair death caller selects byte 16; preserve its prior ROM read.</summary>
    public const ushort NorfairDeathAdjacentByte = 0xb9;

    /// <summary>Norfair's authored table plus the adjacent byte selected by the existing death movement path.</summary>
    public static ushort NorfairDivisor(int index) => index == 16 ? NorfairDeathAdjacentByte : Divisor(index);

    /// <summary>$A6:D61F (RidleyInertiaTable) and $A6:D712 (CeresRidleyInertiaTable): identical bytes 16 down to 1.</summary>
    public static ushort Divisor(int index) => (uint)index < 16
        ? (ushort)(16 - index)
        : throw new ArgumentOutOfRangeException(nameof(index));
}
