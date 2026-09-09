namespace SuperMetroid.Core.Game;

/// <summary>Constants used by the native $90:C157 Power Bomb fuse routine.</summary>
public static class SamusPowerBombFuseData
{
    /// <summary>$90:C157 switches to the fast animation when the decremented fuse reaches fifteen.</summary>
    public const ushort FastAnimationTimer = 15;
    /// <summary>The phase-equivalent fast animation instruction lies $1C bytes after the slow one.</summary>
    public const ushort FastAnimationOffset = 0x1c;
    /// <summary>Fuse-expiration marker consumed separately by the subsequent bank-$94 collision routine.</summary>
    public const ushort ExplosionStartedSentinel = 0xffff;
}
