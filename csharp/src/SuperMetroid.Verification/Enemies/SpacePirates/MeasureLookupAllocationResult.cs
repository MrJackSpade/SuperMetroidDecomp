namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The allocated and checksum of one measure lookup allocation result.</summary>
/// <param name="Allocated">The allocated.</param>
/// <param name="Checksum">The checksum.</param>
internal readonly record struct MeasureLookupAllocationResult(long Allocated, int Checksum);
