using System.Reflection;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Verification access to the horizontal distances a <see cref="ProjectileLaunchInheritance"/> records. Gameplay reads
/// them only as inherited velocities; snapshot captures write them back out as the native distance records.
/// </summary>
internal static class ProjectileLaunchInheritanceAccess
{
    /// <summary>Exposes the recorded horizontal distances.</summary>
    extension(ProjectileLaunchInheritance self)
    {
        /// <summary>The last distance recorded in <paramref name="direction"/>; zero once cleared.</summary>
        internal SignedSixteenSixteen Distance(HorizontalDirection direction) => Distances(self)[direction switch
        {
            HorizontalDirection.Left => 0,
            HorizontalDirection.Right => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined horizontal direction."),
        }];
    }

    /// <summary>The inheritance's four distance records, leftward, rightward, upward and downward.</summary>
    private static SignedSixteenSixteen[] Distances(ProjectileLaunchInheritance inheritance) =>
        typeof(ProjectileLaunchInheritance).GetField("_distances", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(inheritance) as SignedSixteenSixteen[]
            ?? throw new InvalidOperationException("ProjectileLaunchInheritance no longer keeps its distances in _distances.");
}
