using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where the bank-$90 trail programs and their steps are stored.</summary>
internal static class ProjectileTrailSources
{
    /// <summary>The bank-$90 address of <paramref name="program"/>'s first step.</summary>
    internal static ushort Start(ProjectileTrailSegment program) =>
        ProjectileTrailSourceDefinitions.Starts.ContainsKey(program)
            ? ProjectileTrailSourceDefinitions.Starts[program]
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined trail program.");

    /// <summary>The bank-$90 address of the step <paramref name="cursor"/> names: a pose takes four bytes, other steps two.</summary>
    internal static ushort Address(ProjectileTrailCursor cursor)
    {
        ProjectileTrailStep[] steps = ProjectileTrailAnimationDefinitions.Animations[cursor.Animation];
        int address = Start(cursor.Animation);
        for (int step = 0; step < cursor.Step; step++)
            address += steps[step] is ProjectileTrailStep.Pose ? 4 : 2;
        return checked((ushort)address);
    }

    /// <summary>The bank-$90 address of the pose that shows trail frame <paramref name="frame"/>.</summary>
    internal static ushort FrameAddress(ProjectileTrailFrame frame)
    {
        foreach ((ProjectileTrailSegment program, ProjectileTrailStep[] steps) in ProjectileTrailAnimationDefinitions.Animations)
            for (int step = 0; step < steps.Length; step++)
                if (steps[step] is ProjectileTrailStep.Pose pose && pose.Frame == frame)
                    return Address(new ProjectileTrailCursor(program, step));
        throw new ArgumentOutOfRangeException(nameof(frame), frame, "No trail pose shows this frame.");
    }
}
