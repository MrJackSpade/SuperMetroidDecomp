using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification's native view of the retail scroll programs: where each is stored in bank $8F and its native encoding.</summary>
internal static class RoomPlmScrollProgramDefinitionsAccess
{
    /// <summary>The retail scroll programs by their bank-$8F addresses.</summary>
    extension(RoomPlmScrollProgramDefinitions)
    {
        /// <summary>Every retail scroll program's bank-$8F address, ascending.</summary>
        internal static IEnumerable<ushort> Pointers => RoomScrollProgramNative.Addresses.Values.Order();

        /// <summary>The program stored at bank-$8F address <paramref name="pointer"/>.</summary>
        /// <exception cref="InvalidDataException">No retail scroll program is stored there.</exception>
        internal static RoomScrollProgram ProgramAt(ushort pointer) =>
            RoomScrollProgramNative.Addresses.Where(entry => entry.Value == pointer).Select(entry => (RoomScrollProgram?)entry.Key).SingleOrDefault()
                ?? throw new InvalidDataException($"No retail scroll program is stored at $8F:{pointer:X4}.");

        /// <summary>Applies the ordered writes of the program stored at <paramref name="pointer"/>.</summary>
        internal static void ApplyAt(ushort pointer, Action<int, RoomScrollState> write) =>
            RoomPlmScrollPrograms.Apply(RoomPlmScrollProgramDefinitions.ProgramAt(pointer), write);

        /// <summary>The native encoding of the program stored at <paramref name="pointer"/>: index/state pairs, then the terminator.</summary>
        internal static ReadOnlyMemory<byte> Get(ushort pointer)
        {
            var bytes = new List<byte>();
            RoomPlmScrollProgramDefinitions.ApplyAt(pointer, (index, state) => { bytes.Add((byte)index); bytes.Add((byte)state); });
            bytes.Add(0x80);
            return bytes.ToArray();
        }
    }
}
