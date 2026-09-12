namespace SuperMetroid.Core.Game;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static class RidleyInertiaDefinitions
{
    /// <summary>$A6:C60E: MoveRidleyToDeathSpot loads Y=0, selecting the first inertia byte.</summary>
    public const int DeathDivisorIndex = 0;

    /// <summary>$A6:C611: MoveRidleyToDeathSpot loads A=$10, adding sixteen to reversal deceleration, independently of Y.</summary>
    public const ushort DeathReversalBoost = 16;

    /// <summary>$A6:D61F (RidleyInertiaTable) and $A6:D712 (CeresRidleyInertiaTable): identical bytes 16 down to 1.</summary>
    public static ushort Divisor(int index) => (uint)index < 16
        ? (ushort)(16 - index)
        : throw new ArgumentOutOfRangeException(nameof(index));
}
