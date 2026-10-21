namespace SuperMetroid.Core.Programs;

/// <summary>The frames an instruction program runs and the word that ends it.</summary>
/// <param name="Frames">The frames.</param>
/// <param name="Terminator">The terminator.</param>
internal readonly record struct ProgramFrameSpan(int Frames, ushort Terminator);
