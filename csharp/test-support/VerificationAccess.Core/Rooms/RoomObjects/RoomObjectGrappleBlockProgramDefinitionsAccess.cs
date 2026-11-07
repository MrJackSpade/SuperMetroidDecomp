namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectGrappleBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomObjectGrappleBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates grapple-block program words and packed operands at retail addresses.</summary>
    extension(RoomObjectGrappleBlockProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for grapple blocks.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomObjectGrappleBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomObjectGrappleBlockProgramDefinitions.ProgramAt(index);
                yield return program.Start;
                yield return checked((ushort)(program.Start + 4));
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 7 + 4 * frame));
                yield return program.TerminalAddress;
                if (program.Respawns)
                {
                    yield return checked((ushort)(program.TerminalAddress + 2));
                    yield return checked((ushort)(program.TerminalAddress + 4));
                }
            }
        }

        /// <summary>Locates the one-byte operand after each grapple-block setup pair.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomObjectGrappleBlockProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomObjectGrappleBlockProgramDefinitions.ProgramAt(index)).Start + 6));
        }
    }
}
