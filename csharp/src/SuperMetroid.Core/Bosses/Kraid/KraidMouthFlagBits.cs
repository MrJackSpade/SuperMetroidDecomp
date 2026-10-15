namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The reopen count that shares Kraid's mouth word.</summary>
public static class KraidMouthFlagBits
{
    /// <summary>One reopen in the high-byte count.</summary>
    private const ushort OneReopen = 0x0100;

    /// <summary>A reopen count placed in its byte so it can be combined with the latches.</summary>
    public static KraidMouthFlags ReopenCountField(byte count) => (KraidMouthFlags)(count * OneReopen);

    /// <summary>Whether reopens remain in the high-byte count.</summary>
    public static bool HasPendingReopens(this KraidMouthFlags flags) => (flags & KraidMouthFlags.ReopenCount) != 0;

    /// <summary>The word with one fewer pending reopen; the count wraps as the cartridge's word subtraction does.</summary>
    public static KraidMouthFlags WithOneFewerReopen(this KraidMouthFlags flags) =>
        (KraidMouthFlags)unchecked((ushort)((ushort)flags - OneReopen));
}
