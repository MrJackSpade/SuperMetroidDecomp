namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The shot place and sound of a fired beam, if one fired.</summary>
/// <param name="Place">The place the shot took among Samus's shots.</param>
/// <param name="Sound">The sound the shot fires with, or null for none.</param>
internal readonly record struct BeamShot(int? Place, SoundEffectId? Sound);
