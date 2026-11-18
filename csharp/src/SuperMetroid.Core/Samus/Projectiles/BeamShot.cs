namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A shot that fired, and the sound it fires with; a firing that fires nothing has none.</summary>
/// <param name="Sound">The sound the shot fires with, or null for none.</param>
internal readonly record struct BeamShot(SoundEffectId? Sound);
