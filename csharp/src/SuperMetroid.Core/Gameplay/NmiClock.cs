namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The accepted-NMI frame counters: the 8-bit <c>$05B5</c> and the 16-bit <c>$05B6</c>, separate words.
/// </summary>
internal sealed class NmiClock
{

    /// <summary>Low-byte NMI frame counter at WRAM <c>$05B5</c>.</summary>
    public byte NmiFrameCounter8 { get; internal set; }

    /// <summary>16-bit accepted-NMI frame counter at WRAM <c>$05B6</c>.</summary>
    public ushort NmiFrameCounter { get; internal set; }
}
