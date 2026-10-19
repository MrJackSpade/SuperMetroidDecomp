namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon body rectangle, relative to its component's origin, with its touch and shot reactions.</summary>
/// <param name="Left">Left edge offset in room pixels.</param>
/// <param name="Top">Top edge offset in room pixels.</param>
/// <param name="Right">Right edge offset in room pixels.</param>
/// <param name="Bottom">Bottom edge offset in room pixels.</param>
/// <param name="Touch">How the rectangle reacts when Samus touches it.</param>
/// <param name="Shot">How the rectangle reacts when a Samus projectile hits it.</param>
internal readonly record struct DraygonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, DraygonHitboxTouch Touch, DraygonHitboxShot Shot);
