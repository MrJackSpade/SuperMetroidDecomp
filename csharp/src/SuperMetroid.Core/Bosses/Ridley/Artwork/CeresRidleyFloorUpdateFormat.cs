namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The installed file holding the descriptors the getaway's floor runs queue (#1415, #1467).</summary>
public static class CeresRidleyFloorUpdateFormat
{
    /// <summary>JSON filename loaded by the presentation catalog for the getaway's floor update sources.</summary>
    public const string FileName = "ceres-ridley-floor-updates.json";
    /// <summary>Supported revision of the capsule-and-wing descriptor schema: revision 2 records each run's count and floor cell (#1467).</summary>
    public const int Version = 2;
}
