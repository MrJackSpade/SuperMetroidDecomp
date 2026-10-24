namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where one Samus projectile program's list stands in the cartridge (#1411).</summary>
/// <param name="Entry">The list's first instruction pointer, 0 through 65535.</param>
/// <param name="PrecedingWord">The word just before the list, which a trail spawned at the list's start reads as its animation frame, 0 through 65535.</param>
public readonly record struct SamusProjectileListPosition(int Entry, int PrecedingWord);
