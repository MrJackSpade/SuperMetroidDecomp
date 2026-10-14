using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="SpeedBoosterBlockPlmProgramDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates speed-block reveal, timed frames, terminal commands, and packed setup bytes.</summary>
    extension(SpeedBoosterBlockPlmProgramDefinitions)
    {
        /// <summary>Visits bomb reveal, timed frame, terminal, and optional respawn words.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            yield return SpeedBoosterBlockPlmProgramDefinitions.BombReveal;
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 2));
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 4));
            for (int index = 0; index < SpeedBoosterBlockPlmProgramDefinitions.ProgramCount; index++)
            {
                var program = SpeedBoosterBlockPlmProgramDefinitions.ProgramAt((int)(index));
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                {
                    yield return checked((ushort)(program.Start + 3 + frame * 4));
                    yield return checked((ushort)(program.Start + 5 + frame * 4));
                }
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal + 2));
            }
        }

        /// <summary>Locates each speed-block program's byte operand after its start word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < SpeedBoosterBlockPlmProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((SpeedBoosterBlockPlmProgramDefinitions.ProgramAt((int)(index))).Start + 2));
        }
    }
}
