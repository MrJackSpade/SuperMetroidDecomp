namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The four bounded bank-$84 instruction-list ranges used by the translated
/// Chozo statue terrain PLMs. Adjacent native setup and callback machine code
/// is deliberately excluded; draw-list payloads are compiled separately.
/// </summary>
internal static class ChozoStatuePlmProgramDefinitions
{
    /// <summary>Crumbling plug animation/control list, $84:D0F6-D107.</summary>
    internal const ushort CrumblePlugStart = 0xd0f6;
    /// <summary>Last byte before the crumbling plug's $84:D108 setup routine.</summary>
    internal const ushort CrumblePlugEnd = 0xd107;
    /// <summary>Lower Norfair hand event/acid list, $84:D13F-D154.</summary>
    internal const ushort LowerNorfairHandStart = 0xd13f;
    /// <summary>Last byte before the hand's $84:D155 native callback.</summary>
    internal const ushort LowerNorfairHandEnd = 0xd154;
    /// <summary>Wrecked Ship clear-slope list, $84:D3CF-D3D6.</summary>
    internal const ushort ClearSlopeStart = 0xd3cf;
    /// <summary>Last byte before native slope transform $84:D3D7.</summary>
    internal const ushort ClearSlopeEnd = 0xd3d6;
    /// <summary>Wrecked Ship block-slope list, $84:D3EC-D3F3.</summary>
    internal const ushort BlockSlopeStart = 0xd3ec;
    /// <summary>Last byte before native spike restore $84:D3F4.</summary>
    internal const ushort BlockSlopeEnd = 0xd3f3;

    private static readonly byte[] CrumblePlug = Convert.FromHexString(
        "040045A304004BA3040051A3010057A3BC86");
    private static readonly byte[] LowerNorfairHand = Convert.FromHexString(
        "2D880C004DD1C1865CD1B486BC8655D10100B5A2BC86");
    private static readonly byte[] ClearSlope = Convert.FromHexString(
        "0100C59CD7D3BC86");
    private static readonly byte[] BlockSlope = Convert.FromHexString(
        "01000F9DF4D3BC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset) &&
            offset + 1 < bytes.Length)
        {
            value = (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset))
        {
            value = bytes[offset];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool TryGetBytes(ushort address, out byte[] bytes, out int offset)
    {
        if (address >= CrumblePlugStart && address <= CrumblePlugEnd)
        {
            bytes = CrumblePlug;
            offset = address - CrumblePlugStart;
            return true;
        }
        if (address >= LowerNorfairHandStart && address <= LowerNorfairHandEnd)
        {
            bytes = LowerNorfairHand;
            offset = address - LowerNorfairHandStart;
            return true;
        }
        if (address >= ClearSlopeStart && address <= ClearSlopeEnd)
        {
            bytes = ClearSlope;
            offset = address - ClearSlopeStart;
            return true;
        }
        if (address >= BlockSlopeStart && address <= BlockSlopeEnd)
        {
            bytes = BlockSlope;
            offset = address - BlockSlopeStart;
            return true;
        }
        bytes = Array.Empty<byte>();
        offset = 0;
        return false;
    }
}
