namespace SuperMetroid.Core.Game;

/// <summary>One Dachora instruction program: its entry, timed-frame count, and whether it loops.</summary>
internal readonly record struct DachoraInstructionProgram(ushort Entry, int FrameCount, bool Loops);
