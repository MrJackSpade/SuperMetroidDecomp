namespace SuperMetroid.Verification.Harness;

/// <summary>The name and data of one snapshot value.</summary>
/// <param name="Name">The name.</param>
/// <param name="Data">The data.</param>
internal readonly record struct SnapshotValue(string Name, byte[] Data);
