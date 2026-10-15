namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's mouth word: shot latches in the low bits and, in the high byte, a count of pending mouth reopens. The
/// cartridge sets, clears and tests each latch on its own; the count is decremented as a byte.
/// </summary>
[Flags]
public enum KraidMouthFlags : ushort
{
    /// <summary>No latch is set and no reopen is pending.</summary>
    None = 0,
    /// <summary>Bit 0: a charged shot hit the mouth or body this pass.</summary>
    Hit = 0x0001,
    /// <summary>Bit 1: a body hit started the eye glow with a charged shot, arming the reopen.</summary>
    ReopenArmed = 0x0002,
    /// <summary>Bit 2: a mouth hit landed while the reopen was armed.</summary>
    ReopenPending = 0x0004,
    /// <summary>The high-byte reopen count. It is a counter, not a flag.</summary>
    ReopenCount = 0xff00,
}
