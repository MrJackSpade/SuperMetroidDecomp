namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmScrollProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmScrollProgramDefinitionsAccess
{
    /// <summary>Finds authored scroll programs and projects their direct writes into legacy byte format.</summary>
    extension(RoomPlmScrollProgramDefinitions)
    {
        /// <summary>Scans bank-offset values for decoded scroll-state PLM programs.</summary>
        internal static IEnumerable<ushort> Pointers
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (RoomPlmScrollProgramDefinitions.TryApply((ushort)pointer, null)) yield return (ushort)pointer;
            }
        }

        /// <summary>Temporary native-format projection for verification/compatibility; gameplay executes writes directly.</summary>
        internal static ReadOnlyMemory<byte> Get(ushort pointer)
        {
            var bytes = new List<byte>();
            RoomPlmScrollProgramDefinitions.Apply(pointer, (index, state) => { bytes.Add((byte)index); bytes.Add((byte)state); });
            bytes.Add(0x80);
            return bytes.ToArray();
        }
    }
}
