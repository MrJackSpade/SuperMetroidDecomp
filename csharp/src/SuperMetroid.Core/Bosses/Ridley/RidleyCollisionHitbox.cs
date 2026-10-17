namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One Ridley body rectangle, engine-owned and never derived from editable OAM. Every Ridley rectangle reacts the
/// same way to Samus's touch and to her projectiles, so the reactions belong to <see cref="RidleyHitboxCallbackCodec"/>
/// rather than to each rectangle.
/// </summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
internal readonly record struct RidleyCollisionHitbox(short Left, short Top, short Right, short Bottom);
