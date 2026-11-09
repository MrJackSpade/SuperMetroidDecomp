namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The two groups of four shards a breaking stage of Mother Brain's glass throws, each with the shatter sound.</summary>
/// <param name="First">The shards thrown as the stage breaks, in spawn order.</param>
/// <param name="Second">The shards thrown once the next image has shown, in spawn order.</param>
internal sealed record MotherBrainGlassBurst(IReadOnlyList<MotherBrainGlassShard> First, IReadOnlyList<MotherBrainGlassShard> Second);
