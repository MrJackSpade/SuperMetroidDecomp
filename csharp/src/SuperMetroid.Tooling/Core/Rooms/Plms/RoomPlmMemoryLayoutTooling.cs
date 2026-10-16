namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Native PLM program-word layout that only development tools read; never linked by player hosts.</summary>
internal static class RoomPlmMemoryLayoutTooling
{
    /// <summary>Cartridge-resident program identities begin at bank offset $8000.</summary>
    internal const int CompiledProgramStart = 0x8000;
    /// <summary>Bit 15 distinguishes an instruction-routine word from a positive timer.</summary>
    internal const ushort RoutineWordMask = 0x8000;
}
