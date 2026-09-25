namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed control words for the n00b-tube PLM's two reachable instruction
/// branches. Draw operands select separately compiled physical layouts; the
/// break sound remains an authored audio operand at its one byte position.
/// </summary>
internal static class NoobTubePlmProgramDefinitions
{
    /// <summary>Main n00b-tube instruction list at $84:D4D4.</summary>
    private const ushort MainStart = 0xd4d4;
    /// <summary>One-byte library-two break sound at $84:D506.</summary>
    private const ushort BreakSoundAddress = 0xd506;
    /// <summary>Continuation after the break-sound byte at $84:D507.</summary>
    private const ushort MainAfterSound = 0xd507;
    /// <summary>Already-broken room-state branch at $84:D521.</summary>
    private const ushort AlreadyBrokenStart = 0xd521;

    private static readonly byte[] Main = Convert.FromHexString(
        "2D880B0021D5248AE8D4C18626BD0100D198B486248AF2D4C186BFD4B486" +
        "CA86E6D52CD53000D798010091990100E599108C1A43D536D56000DD983E88" +
        "0B0025D5EED5BC86");
    private static readonly byte[] AlreadyBroken = Convert.FromHexString("25D5BC86");

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        for (int offset = 0; offset <= 0x30; offset += 2)
            yield return checked((ushort)(MainStart + offset));
        for (int offset = 0; offset <= 0x10; offset += 2)
            yield return checked((ushort)(MainAfterSound + offset));
        yield return AlreadyBrokenStart;
        yield return checked((ushort)(AlreadyBrokenStart + 2));
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        yield return BreakSoundAddress;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (IsEvenStep(address, MainStart, 0xd504))
        {
            int offset = address - MainStart;
            value = (ushort)(Main[offset] | Main[offset + 1] << 8);
            return true;
        }
        if (IsEvenStep(address, MainAfterSound, 0xd517))
        {
            int offset = address - MainStart;
            value = (ushort)(Main[offset] | Main[offset + 1] << 8);
            return true;
        }
        if (IsEvenStep(address, AlreadyBrokenStart, 0xd523))
        {
            int offset = address - AlreadyBrokenStart;
            value = (ushort)(AlreadyBroken[offset] | AlreadyBroken[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == BreakSoundAddress)
        {
            value = Main[address - MainStart];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool IsEvenStep(ushort address, ushort first, ushort last) =>
        address >= first && address <= last && ((address - first) & 1) == 0;
}
