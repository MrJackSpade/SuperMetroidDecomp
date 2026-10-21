namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Where a sound library's allocation state starts in SPC RAM, and how many channels it has.</summary>
/// <param name="BaseAddress">The SPC RAM address of the library's allocation state.</param>
/// <param name="ChannelCount">The number of channels the library allocates.</param>
internal readonly record struct SoundLibraryAllocation(int BaseAddress, int ChannelCount);
