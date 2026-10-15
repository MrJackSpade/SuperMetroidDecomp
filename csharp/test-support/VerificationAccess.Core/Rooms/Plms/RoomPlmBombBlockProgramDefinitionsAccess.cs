namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmBombBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates respawning bomb-block instruction words and packed operands.</summary>
    extension(RoomPlmBombBlockProgramDefinitions)
    {
        /// <summary>Visits collision, reaction, frame, terminal, and optional respawn words for each bomb block.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmBombBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmBombBlockProgramDefinitions.ProgramAt((int)(index));
                yield return program.CollisionHead;
                yield return checked((ushort)(program.CollisionHead + 3));
                yield return checked((ushort)(program.CollisionHead + 5));
                yield return program.ReactionHead;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Tail + 4 * frame));
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal +
                        (program.SingleBlock ? 2 : 4)));
            }
        }

        /// <summary>Locates bomb-block collision and reaction operands stored as bytes.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomPlmBombBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmBombBlockProgramDefinitions.ProgramAt((int)(index));
                yield return checked((ushort)(program.CollisionHead + 2));
                yield return checked((ushort)(program.ReactionHead + 2));
            }
        }
    }
}
