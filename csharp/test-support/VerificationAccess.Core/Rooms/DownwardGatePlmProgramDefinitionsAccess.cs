using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="DownwardGatePlmProgramDefinitions"/> members production does not use.</summary>
internal static class DownwardGatePlmProgramDefinitionsAccess
{
    /// <summary>$84:BC61: adjacent upward-gate program, outside the downward-gate decoder.</summary>
    private const ushort ResidentEnd = 0xbc61;

    /// <summary>Enumerates downward-gate instruction words and packed sound bytes at their native addresses.</summary>
    extension(DownwardGatePlmProgramDefinitions)
    {
        /// <summary>Visits downward-gate opening and trigger words while excluding adjacent upward-gate code.</summary>
        internal static IEnumerable<(ushort Address, ushort Value)> MechanicsWords
        {
            get
            {
                for (int address = DownwardGatePlmProgramDefinitions.OpenStart; address < ResidentEnd; address++)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
                for (int address = RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft; address < DownwardGatePlmProgramDefinitions.TriggerEnd; address += 2)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
            }
        }

        /// <summary>Exposes the opening and closing sound operand bytes in their native positions.</summary>
        internal static IEnumerable<(ushort Address, byte Value)> MechanicsBytes
        {
            get
            {
                yield return (DownwardGatePlmProgramDefinitions.ClosingSoundAddress, DownwardGatePlmRomData.MovementSound);
                yield return (DownwardGatePlmProgramDefinitions.OpeningSoundAddress, DownwardGatePlmRomData.MovementSound);
            }
        }
    }
}
