namespace SuperMetroid.Core.Game;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static class RidleyInertiaDefinitions
{
    /// <summary>$A6:C60E: MoveRidleyToDeathSpot loads Y=0, selecting the first inertia byte.</summary>
    public const int DeathDivisorIndex = 0;

    /// <summary>$A6:C611: MoveRidleyToDeathSpot loads A=$10, adding sixteen to reversal deceleration, independently of Y.</summary>
    public const ushort DeathReversalBoost = 16;

    /// <summary>$A6:D569/D5DF: positive 8.8 velocity limit in the Norfair acceleration routines.</summary>
    public const ushort MaximumVelocity = 0x0500;

    /// <summary>$A6:D59D/D613: negative 8.8 velocity limit, compared with the native N flag.</summary>
    public const ushort MinimumVelocity = 0xfb00;

    /// <summary>$A6:D562/D596/D5D8/D60C: base acceleration added when reversing direction.</summary>
    public const ushort ReversalAcceleration = 8;

    /// <summary>$A6:D61F (RidleyInertiaTable) and $A6:D712 (CeresRidleyInertiaTable): identical bytes 16 down to 1.</summary>
    public static ushort Divisor(int index) => (uint)index < 16
        ? (ushort)(16 - index)
        : throw new ArgumentOutOfRangeException(nameof(index));
}
