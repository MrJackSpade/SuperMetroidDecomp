namespace Engine.Actors;

/// <summary>One projectile of a shot: how it behaves, and how far to the side of the shot's line it is launched.</summary>
/// <param name="Side">How far from the shot's line it starts, in pixels: below or to the right of it when positive.</param>
/// <param name="Projectile">How it behaves.</param>
public sealed record WeaponShot(int Side, ProjectileSettings Projectile);
