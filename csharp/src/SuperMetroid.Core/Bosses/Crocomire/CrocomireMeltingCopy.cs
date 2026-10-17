namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One source-to-scratch copy authored in Crocomire's melt table.</summary>
/// <param name="SourceWord">Offset of the copied graphics chunk within the pass's cartridge bank.</param>
/// <param name="DestinationWord">Scratch address the chunk is copied to.</param>
internal readonly record struct CrocomireMeltingCopy(int SourceWord, int DestinationWord);
