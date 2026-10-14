
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Verification access to <see cref="GrowingShutterInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GrowingShutterInstructionProgramDefinitionsAccess
{
    /// <summary>Indexes four successive shutter-growth programs and their frame entry points.</summary>
    extension(GrowingShutterInstructionProgramDefinitions)
    {
        /// <summary>The shutter's four successive growth stages.</summary>
        internal static int ProgramCount => 4;

        /// <summary>Each growth stage owns a six-byte duration/visual/sleep program.</summary>
        internal static ushort ProgramEntryPoint(int index)
        {
            if ((uint)index >= GrowingShutterInstructionProgramDefinitions.ProgramCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * index);
        }
    }
}
