namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One step of a trail program. Each kind of step is its own sealed record holding only what that step needs.</summary>
public abstract record ProjectileTrailStep
{
    /// <summary>Closes the hierarchy to the steps declared here.</summary>
    private ProjectileTrailStep()
    {
    }

    /// <summary>Ends the side: its timer stays zero and it draws nothing more.</summary>
    public sealed record End : ProjectileTrailStep;

    /// <summary>Moves a sprite one pixel and continues with the next step at once.</summary>
    /// <param name="Movement">The sprite and direction moved.</param>
    public sealed record Move(ProjectileTrailInstruction Movement) : ProjectileTrailStep;

    /// <summary>Shows <see cref="Frame"/> for <see cref="Duration"/> updates.</summary>
    /// <param name="Duration">The updates the frame lasts.</param>
    /// <param name="Frame">The trail frame, which selects its appearance.</param>
    public sealed record Pose(int Duration, ProjectileTrailFrame Frame) : ProjectileTrailStep;
}
