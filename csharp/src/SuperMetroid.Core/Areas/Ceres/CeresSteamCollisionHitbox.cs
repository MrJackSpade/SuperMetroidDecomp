namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// One steam plume rectangle. Every steam rectangle reacts the same way to Samus's touch and to her projectiles, so the
/// reactions belong to <see cref="CeresSteamHitboxCallbackCodec"/> rather than to each rectangle.
/// </summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
internal readonly record struct CeresSteamCollisionHitbox(short Left, short Top, short Right, short Bottom);
