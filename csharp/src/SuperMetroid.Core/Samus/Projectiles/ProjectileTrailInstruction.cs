namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The bank-$90 beam-trail instructions; either stream may execute any of them.</summary>
public enum ProjectileTrailInstruction
{
    /// <summary>Left-stream instruction that moves its sprite one pixel down, $90:B525.</summary>
    MoveLeftDown,
    /// <summary>Right-stream instruction that moves its sprite one pixel down, $90:B587.</summary>
    MoveRightDown,
    /// <summary>Left-stream instruction that moves its sprite one pixel up, $90:B5B3.</summary>
    MoveLeftUp,
}
