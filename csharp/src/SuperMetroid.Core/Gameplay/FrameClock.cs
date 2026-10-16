namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The accepted-frame counters: the 8-bit <c>$05B5</c> and the 16-bit <c>$05B6</c>. They are separate words, not one
/// counter and its low byte: the door IRQ clears only the byte.
/// </summary>
internal sealed class FrameClock
{
    /// <summary>8-bit accepted-frame counter at WRAM <c>$05B5</c>.</summary>
    public byte Counter8 { get; private set; }

    /// <summary>16-bit accepted-frame counter at WRAM <c>$05B6</c>.</summary>
    public ushort Counter { get; private set; }

    /// <summary>Advances both counters for one accepted frame, wrapping at their widths.</summary>
    internal void AdvanceAcceptedFrame() => AdvanceHardwareWaitFrames(1);

    /// <summary>
    /// Counts frames the cartridge accepts while its main loop is stalled in a hardware wait ($80:95E7-$95F4): each
    /// advances <c>$05B5/$05B6</c> with no publication, latch or gameplay.
    /// </summary>
    internal void AdvanceHardwareWaitFrames(int count)
    {
        Ensure.AtLeastZero(count);
        Counter8 = unchecked((byte)(Counter8 + count));
        Counter = unchecked((ushort)(Counter + count));
    }

    /// <summary>
    /// $80:97BA/$9823 ends a door scanout with a sixteen-bit STZ/INC at $05B4, whose high byte is <c>$05B5</c>: the next
    /// accepted frame observes zero there while <c>$05B6</c> keeps advancing.
    /// </summary>
    internal void ClearCounter8ForDoorIrq() => Counter8 = 0;

    /// <summary>Clears both counters, as the reset vector's bank-$7E wipe does.</summary>
    internal void Reset() => Import(0, 0);

    /// <summary>
    /// Takes over the counters another owner kept, such as the frontend that counted before this runtime existed or a
    /// native snapshot import. The cartridge has one pair of words; allocation never resets it.
    /// </summary>
    internal void Import(byte counter8, ushort counter)
    {
        Counter8 = counter8;
        Counter = counter;
    }
}
