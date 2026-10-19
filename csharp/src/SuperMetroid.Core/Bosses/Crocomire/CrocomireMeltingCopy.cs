namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One graphics-chunk-to-scratch copy authored in Crocomire's melt table.</summary>
/// <param name="Pass">The melt pass whose graphics the chunk belongs to.</param>
/// <param name="Chunk">Zero-based position of the chunk within its pass's graphics, in copy order.</param>
/// <param name="DestinationWord">Scratch address the chunk is copied to.</param>
internal readonly record struct CrocomireMeltingCopy(CrocomireMeltingHeader Pass, int Chunk, int DestinationWord);
