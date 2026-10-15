namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleProgramDefinitionsAccess
{
    /// <summary>Enumerates contact-crumble mechanics words and packed byte operands.</summary>
    extension(RoomPlmContactCrumbleProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for each contact-crumble list.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmContactCrumbleProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmContactCrumbleProgramDefinitions.ProgramAt((int)(index));
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 3 + 4 * frame));
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal +
                        (program.Dimension == 0 ? 2 : 4)));
            }
        }

        /// <summary>Locates each contact-crumble program's byte operand after its start word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomPlmContactCrumbleProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomPlmContactCrumbleProgramDefinitions.ProgramAt((int)(index))).Start + 2));
        }
    }
}
