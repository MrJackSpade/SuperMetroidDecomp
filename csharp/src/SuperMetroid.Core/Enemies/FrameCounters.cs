namespace SuperMetroid.Core.Enemies;

/// <summary>The accepted-frame counters enemy code reads for one pass.</summary>
/// <param name="Counter8">The 8-bit counter at WRAM <c>$05B5</c>.</param>
/// <param name="Counter">The 16-bit counter at WRAM <c>$05B6</c>.</param>
internal readonly record struct FrameCounters(byte Counter8, ushort Counter);
