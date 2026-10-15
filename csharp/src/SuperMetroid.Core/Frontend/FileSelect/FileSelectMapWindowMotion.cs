namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Application-owned signed 16.16 window motion, in left/right/top/bottom order.</summary>
internal readonly record struct FileSelectMapWindowMotion(ushort Timer, uint Left, uint Right, uint Top, uint Bottom);
