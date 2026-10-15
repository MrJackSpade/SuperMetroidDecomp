namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>The four room holes selected by native eight-byte offsets.</summary>
internal enum BotwoonHoleLocation : ushort
{
    /// <summary><c>BotwoonHoleHitboxes</c> left hole at $B3:949B; center (64,112).</summary>
    Left = 0,
    /// <summary><c>BotwoonHoleHitboxes</c> bottom hole at $B3:94A3; center (128,176).</summary>
    Bottom = 8,
    /// <summary><c>BotwoonHoleHitboxes</c> top hole at $B3:94AB; center (160,96).</summary>
    Top = 16,
    /// <summary><c>BotwoonHoleHitboxes</c> right hole at $B3:94B3; center (224,144).</summary>
    Right = 24,
}
