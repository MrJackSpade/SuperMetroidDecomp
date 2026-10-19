using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Cartridge values and helpers of <see cref="ElevatorPlatformPlmDefinitions"/> that only verification reads.</summary>
internal static class ElevatorPlatformPlmDefinitionsConstants
{
    /// <summary>First $B70B instruction word, $84:AFB6.</summary>
    internal const ushort InstructionLoop = 0xafb6;
    /// <summary>
    /// $84:AFB6..AFC8, ten aligned words: four holds of four ticks with a
    /// first/second/third/second ping-pong selection, then Goto the loop start.
    /// Only original word starts are owned; odd and adjacent reads are rejected.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - InstructionLoop;
        if (offset < 0 || offset > 18 || (offset & 1) != 0)
        {
            value = 0;
            return false;
        }
        if (offset == 16) value = (ushort)RoomPlmInstruction.Goto;
        else if (offset == 18) value = InstructionLoop;
        else if ((offset & 3) == 0) value = 4;
        else
        {
            int phase = offset / 4;
            int frame = 2 - Math.Abs(phase - 2);
            value = (ushort)((ushort)ElevatorPlatformDraw.First + frame * ElevatorPlatformPlmDefinitions.DrawStride);
        }
        return true;
    }
}
