namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where one trail side is in its program: the step it runs next.</summary>
/// <param name="Program">The program the side runs.</param>
/// <param name="Step">The index of the next step.</param>
public readonly record struct ProjectileTrailCursor(ProjectileTrailProgram Program, int Step)
{
    /// <summary>The step the cursor names.</summary>
    internal ProjectileTrailStep Current => ProjectileTrailProgramDefinitions.Programs[Program][Step];

    /// <summary>The cursor on the following step.</summary>
    internal ProjectileTrailCursor Next() => this with { Step = Step + 1 };

    /// <summary>The pose the side showed last: the step before the cursor; null before the side has shown a pose.</summary>
    internal ProjectileTrailStep? PreviousPose => Step > 0 ? ProjectileTrailProgramDefinitions.Programs[Program][Step - 1] : null;
}
