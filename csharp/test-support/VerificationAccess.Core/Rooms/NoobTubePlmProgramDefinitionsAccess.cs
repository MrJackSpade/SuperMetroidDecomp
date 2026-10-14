using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="NoobTubePlmProgramDefinitions"/> members production does not use.</summary>
internal static class NoobTubePlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Noob Tube program words and byte operands for cartridge comparison.</summary>
    extension(NoobTubePlmProgramDefinitions)
    {
        /// <summary>Visits main, post-sound, and already-broken Noob Tube control words.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int offset = 0; offset <= 0x30; offset += 2)
                yield return checked((ushort)(NoobTubePlmProgramDefinitions.MainStart + offset));
            for (int offset = 0; offset <= 0x10; offset += 2)
                yield return checked((ushort)(NoobTubePlmProgramDefinitions.MainAfterSound + offset));
            yield return NoobTubePlmProgramDefinitions.AlreadyBrokenStart;
            yield return checked((ushort)(NoobTubePlmProgramDefinitions.AlreadyBrokenStart + 2));
        }

        /// <summary>Locates the standalone tube-breaking sound operand byte.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            yield return NoobTubePlmProgramDefinitions.BreakSoundAddress;
        }
    }
}
