namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid head cursor/timer pair selected when the first phase ends.</summary>
internal readonly record struct KraidHeadResumeDefinition(ushort Pointer, ushort Timer);
