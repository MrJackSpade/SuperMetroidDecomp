namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A block of a Samus eater run.</summary>
/// <param name="Run">The run.</param>
/// <param name="Block">The block.</param>
internal readonly record struct SamusEaterBlock(int Run, int Block);
