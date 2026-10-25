namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail programs one projectile selection runs on its left and right sides.</summary>
/// <param name="Left">The program the left trail runs.</param>
/// <param name="Right">The program the right trail runs.</param>
internal readonly record struct ProjectileTrailPrograms(ProjectileTrailProgram Left, ProjectileTrailProgram Right);
