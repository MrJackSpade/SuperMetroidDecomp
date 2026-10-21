namespace SuperMetroid.Verification.Bosses;

/// <summary>The hit and callback of one hitbox result.</summary>
/// <param name="Hit">The hit.</param>
/// <param name="Callback">The callback.</param>
internal readonly record struct HitboxResult(bool Hit, ushort Callback);
