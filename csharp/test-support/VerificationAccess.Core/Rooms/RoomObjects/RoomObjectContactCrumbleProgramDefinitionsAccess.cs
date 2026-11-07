namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectContactCrumbleProgramDefinitions"/> members production does not use.</summary>
internal static class RoomObjectContactCrumbleProgramDefinitionsAccess
{
    /// <summary>Enumerates contact-crumble mechanics words and packed byte operands.</summary>
    extension(RoomObjectContactCrumbleProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for each contact-crumble list.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomObjectContactCrumbleProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomObjectContactCrumbleProgramDefinitions.ProgramAt(index);
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
            for (int index = 0; index < RoomObjectContactCrumbleProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomObjectContactCrumbleProgramDefinitions.ProgramAt(index)).Start + 2));
        }
    }
}
