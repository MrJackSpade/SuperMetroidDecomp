using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>The Zebes area a check's area is, for the checks that loop over areas and build a Zebes-only map.</summary>
internal static class VerificationZebesAreas
{
    /// <summary>The Zebes area <paramref name="area"/> is.</summary>
    /// <exception cref="ArgumentException"><paramref name="area"/> is Ceres.</exception>
    internal static ZebesArea Of(AreaId area) =>
        AreaIds.IsZebes(area, out ZebesArea zebes) ? zebes : throw new ArgumentException($"{area} is not a Zebes area.", nameof(area));
}
