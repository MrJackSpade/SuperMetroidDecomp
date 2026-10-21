namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The projectile slot and sound of a fired beam, if one fired.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Sound">The sound.</param>
internal readonly record struct BeamShot(int? Slot, SoundEffectId? Sound);
