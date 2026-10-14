namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitionsAccess
{
    /// <summary>Counts fake-death PLM programs and enumerates their native word addresses.</summary>
    extension(MotherBrainFakeDeathPlmProgramDefinitions)
    {
        /// <summary>Counts fixed-size fake-death PLM programs in the compiled address interval.</summary>
        internal static int ProgramCount => (MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive - MotherBrainFakeDeathPlmProgramDefinitions.Start) / MotherBrainFakeDeathPlmProgramDefinitions.ProgramByteLength;

        /// <summary>Visits three native words for every fake-death PLM program.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses() =>
            Enumerable.Range(0, MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount * 3)
                .Select(index => checked((ushort)(MotherBrainFakeDeathPlmProgramDefinitions.Start + index * 2)));
    }
}
