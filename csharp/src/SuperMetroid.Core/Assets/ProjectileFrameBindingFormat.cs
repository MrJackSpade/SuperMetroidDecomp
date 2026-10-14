namespace SuperMetroid.Core.Assets;

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    /// <summary>JSON resource filename for timed projectile record-to-spritemap assignments, separate from the composition artwork file.</summary>
    public const string FileName = "projectile-frame-bindings.json";
    /// <summary>Supported schema revision, one, requiring an explicit visual assignment for every authored timed record.</summary>
    public const int Version = 1;
    /// <summary>805 distinct timed bank-$93 record starts requiring bindings; not the number of sprite compositions or updates in one animation.</summary>
    public const int FrameCount = 805;
    /// <summary>Formats a bank-relative timed-record identity as the canonical JSON key, such as frame_86DB for the upward Power beam record.</summary>
    /// <param name="pointer">Sixteen-bit record-start offset; formatting does not validate that it names a required timed record.</param>
    /// <returns>The frame_ prefix followed by exactly four uppercase hexadecimal digits.</returns>
    public static string FrameName(ushort pointer) => $"frame_{pointer:X4}";
}
