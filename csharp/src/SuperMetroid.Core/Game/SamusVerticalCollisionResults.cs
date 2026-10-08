namespace SuperMetroid.Core.Game;

/// <summary>
/// Values of WRAM <c>$0DC6</c> <c>SamusSolidVerticalCollisionResult</c> written by bank $90.
/// </summary>
public static class SamusVerticalCollisionResults
{
    /// <summary>Upward movement without a collision clears the word (<c>$90:E616</c>).</summary>
    public const ushort None = 0;

    /// <summary>Downward movement hit a solid surface (<c>$90:E623</c>).</summary>
    public const ushort Landed = 1;

    /// <summary>Downward movement without a collision (<c>$90:E644</c>).</summary>
    public const ushort Falling = 2;

    /// <summary>Upward movement hit a solid surface (<c>$90:E60E</c>).</summary>
    public const ushort HitCeiling = 4;

    /// <summary>
    /// A wall jump was triggered (<c>$90:9E5E</c>/<c>$90:9E7F</c>). Downward movement
    /// without a collision leaves this value in place (<c>$90:E63F</c>).
    /// </summary>
    public const ushort WallJump = 5;
}
