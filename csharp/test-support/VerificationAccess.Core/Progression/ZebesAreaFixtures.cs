namespace SuperMetroid.Core.Progression;

/// <summary>Verification access: the Zebes area a check's area identity names, for checks that walk every area by number.</summary>
internal static class ZebesAreaFixtures
{
    /// <summary>The Zebes area <paramref name="area"/> is.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The area is Ceres or the debug area, which lie off Zebes.</exception>
    internal static ZebesArea Of(AreaId area) => AreaIds.IsZebes(area, out ZebesArea zebes)
        ? zebes
        : throw new ArgumentOutOfRangeException(nameof(area), area, "The check names an area off Zebes.");
}
