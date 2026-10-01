namespace SuperMetroid.Core.Rooms;

/// <summary>Native PLM pointer interpretation; no cartridge-read capability.</summary>
internal static class RoomPlmMemoryLayout
{
    /// <summary>Bank $84 owns instruction/draw identities and its low WRAM mirror.</summary>
    internal const byte ProgramBank = 0x84;
    /// <summary>$84:0000-1FFF is live WRAM; $2000 is the exclusive mirror end.</summary>
    internal const ushort WorkRamMirrorEnd = 0x2000;
    /// <summary>Cartridge-resident program identities begin at bank offset $8000.</summary>
    internal const int CompiledProgramStart = 0x8000;
    /// <summary>Bit 15 distinguishes an instruction-routine word from a positive timer.</summary>
    internal const ushort RoutineWordMask = 0x8000;
}
