
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The sample a loop re-enters and the cursor it resumes at.</summary>
/// <param name="Sample">The sample.</param>
/// <param name="Cursor">The cursor.</param>
public readonly record struct PcmLoopEntry(ManagedPcmSample Sample, int Cursor);
