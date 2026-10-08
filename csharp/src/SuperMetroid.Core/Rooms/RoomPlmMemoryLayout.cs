namespace SuperMetroid.Core.Rooms;

/// <summary>Native PLM pointer interpretation; no cartridge-read capability.</summary>
internal static class RoomPlmMemoryLayout
{
    /// <summary>Bank $84 owns instruction/draw identities and its low WRAM mirror.</summary>
    internal const byte ProgramBank = 0x84;
    /// <summary>$84:0000-1FFF is live WRAM; $2000 is the exclusive mirror end.</summary>
    internal const ushort WorkRamMirrorEnd = 0x2000;
}
