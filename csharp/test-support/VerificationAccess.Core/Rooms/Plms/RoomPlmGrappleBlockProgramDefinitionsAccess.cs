namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates grapple-block program words and packed operands at retail addresses.</summary>
    extension(RoomPlmGrappleBlockProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for grapple blocks.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmGrappleBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmGrappleBlockProgramDefinitions.ProgramAt(index);
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
            for (int index = 0; index < RoomPlmGrappleBlockProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomPlmGrappleBlockProgramDefinitions.ProgramAt(index)).Start + 6));
        }
    }
}
