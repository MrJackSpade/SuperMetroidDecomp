namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>A sound library's priority outside and inside low-health mode.</summary>
/// <param name="Normal">The normal.</param>
/// <param name="LowHealth">The low health.</param>
internal readonly record struct LowHealthPriorityPair(int Normal, int LowHealth);
