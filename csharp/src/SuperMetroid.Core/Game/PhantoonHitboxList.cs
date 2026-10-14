namespace SuperMetroid.Core.Game;

/// <summary>Phantoon's three bank-$A7 hitbox lists.</summary>
internal enum PhantoonHitboxList : ushort
{
    /// <summary>$A7:E020, the one-point no-op hitbox used by hidden parts.</summary>
    Point = 0xe020,
    /// <summary>$A7:E02E, Phantoon's five-rectangle full body.</summary>
    FullBody = 0xe02e,
    /// <summary>$A7:E06C, the single vulnerable eye rectangle.</summary>
    EyeOnly = 0xe06c,
}
