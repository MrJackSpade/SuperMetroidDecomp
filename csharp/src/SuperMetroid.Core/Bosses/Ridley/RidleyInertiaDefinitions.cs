namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static class RidleyInertiaDefinitions
{
    /// <summary>$A6:C60E: MoveRidleyToDeathSpot loads Y=0, selecting the first inertia byte.</summary>
    public const int DeathDivisorIndex = 0;

    /// <summary>$A6:C611: MoveRidleyToDeathSpot loads A=$10, adding sixteen to reversal deceleration, independently of Y.</summary>
    public const ushort DeathReversalBoost = 16;

    /// <summary>$A6:D569/D5DF: the fastest positive speed the Norfair acceleration routines allow, five pixels per frame.</summary>
    public static SignedEightEight MaximumVelocity { get; } = SignedEightEight.FromSignedRaw(5 * 256);

    /// <summary>$A6:D59D/D613: the fastest negative speed, five pixels per frame, compared with the native N flag.</summary>
    public static SignedEightEight MinimumVelocity { get; } = SignedEightEight.FromSignedRaw(-5 * 256);

    /// <summary>$A6:D562/D596/D5D8/D60C: base acceleration added when reversing direction.</summary>
    public const ushort ReversalAcceleration = 8;

    /// <summary>$A6:D61F (RidleyInertiaTable) and $A6:D712 (CeresRidleyInertiaTable): identical bytes 16 down to 1.</summary>
    public static readonly ushort[] Divisors = [16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
}
