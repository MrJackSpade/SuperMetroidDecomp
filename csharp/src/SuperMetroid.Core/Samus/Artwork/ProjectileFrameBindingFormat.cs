namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    /// <summary>JSON resource filename for timed projectile record-to-spritemap assignments, separate from the composition artwork file.</summary>
    public const string FileName = "projectile-frame-bindings.json";
    /// <summary>Supported schema revision, two, requiring an explicit visual assignment for every authored timed record by its index.</summary>
    public const int Version = 2;
    /// <summary>805 timed records requiring bindings; not the number of sprite compositions or updates in one animation.</summary>
    public const int FrameCount = 805;
    /// <summary>Formats a timed record's index as the canonical JSON key, such as frame_0 for the first Power beam record.</summary>
    /// <param name="record">The record's index among the timed records; formatting does not validate that it names one.</param>
    /// <returns>The frame_ prefix followed by the record's index.</returns>
    public static string FrameName(int record) => $"frame_{record}";
}
