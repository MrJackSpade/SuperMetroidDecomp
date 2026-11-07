namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectShotBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomObjectShotBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates shot-block setup, frame, and terminal mechanics addresses separately from draw operands.</summary>
    extension(RoomObjectShotBlockProgramDefinitions)
    {
        /// <summary>All authored control-word addresses, excluding draw-list operands.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            foreach (var program in RoomObjectShotBlockProgramDefinitions.Programs)
            {
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 3 + 4 * frame));
                yield return program.TerminalAddress;
                if (program.RestoresLevelWord)
                    yield return checked((ushort)(program.TerminalAddress + 2));
            }
        }

        /// <summary>Locates the byte operand after each shot-block program's setup word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            foreach (var program in RoomObjectShotBlockProgramDefinitions.Programs)
                yield return checked((ushort)(program.Start + 2));
        }
    }
}
