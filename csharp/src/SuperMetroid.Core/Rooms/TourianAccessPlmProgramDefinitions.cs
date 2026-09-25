using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded bank-$84 instruction data for the two access-floor PLMs spawned when
/// the Tourian entrance statue changes state. The adjacent $AB00 routine is
/// executable code, not part of either instruction stream.
/// </summary>
internal static class TourianAccessPlmProgramDefinitions
{
    /// <summary><c>$84:AAE5</c>: six-row crumble loop.</summary>
    internal const ushort Crumble = TourianAccessPlmDefinitions.CrumbleInstructionList;
    /// <summary><c>$84:AB0C</c>: one-pass six-row clear.</summary>
    internal const ushort Clear = TourianAccessPlmDefinitions.ClearInstructionList;
    /// <summary>Native eight-bit timer value for the six crumble rows.</summary>
    internal const byte CrumbleRows = 6;
    /// <summary>Each of the four crumble frames holds for four PLM passes.</summary>
    internal const ushort CrumbleFrameDuration = 4;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == Crumble)
        {
            value = RoomPlmInstructionCodes.SetEightBitTimer;
            return true;
        }
        if (address >= Crumble + 3 && address < Crumble + 19)
        {
            int offset = address - Crumble - 3;
            if (offset % 4 == 0)
            {
                value = CrumbleFrameDuration;
                return true;
            }
            if (offset % 4 == 2)
            {
                value = TourianAccessPlmDrawDefinitions.CrumbleFramePointer(offset / 4);
                return true;
            }
        }
        value = address switch
        {
            Crumble + 19 => TourianStatueRomData.MoveAccessDown,
            Crumble + 21 => RoomPlmInstructionCodes.DecrementTimerAndGoto,
            Crumble + 23 => checked((ushort)(Crumble + 3)),
            Crumble + 25 => RoomPlmInstructionCodes.Delete,
            Clear => 1,
            Clear + 2 => TourianAccessPlmDrawDefinitions.ClearPointer,
            Clear + 4 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return address is Crumble + 19 or Crumble + 21 or Crumble + 23 or
            Crumble + 25 or Clear or Clear + 2 or Clear + 4;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == Crumble + 2)
        {
            value = CrumbleRows;
            return true;
        }
        value = 0;
        return false;
    }

    internal static IEnumerable<ushort> NativeWordAddresses()
    {
        yield return Crumble;
        for (int frame = 0; frame < 4; frame++)
        {
            yield return checked((ushort)(Crumble + 3 + frame * 4));
            yield return checked((ushort)(Crumble + 5 + frame * 4));
        }
        yield return checked((ushort)(Crumble + 19));
        yield return checked((ushort)(Crumble + 21));
        yield return checked((ushort)(Crumble + 23));
        yield return checked((ushort)(Crumble + 25));
        yield return Clear;
        yield return checked((ushort)(Clear + 2));
        yield return checked((ushort)(Clear + 4));
    }
}
