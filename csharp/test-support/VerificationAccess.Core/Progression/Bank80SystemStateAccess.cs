namespace SuperMetroid.Core.Progression;
/// <summary>Verification access to <see cref="Bank80SystemState"/> members production does not use.</summary>
internal static class Bank80SystemStateAccess
{
    /// <summary>Reproduces bank-$80's unsigned 16-by-16 multiply with a full 32-bit product.</summary>
    extension(Bank80SystemState)
    {
        /// <summary>
        /// Unsigned <c>16 x 16 -> 32</c> multiplication from <c>$80:82D6</c>.
        /// The widening casts must happen before multiplication or C# would discard the high
        /// word before returning it.
        /// </summary>
        internal static uint Multiply16By16(ushort left, ushort right) => (uint)left * right;
    }

    /// <summary>Resolves a Chozo-room bit into the native 64-byte persistent table.</summary>
    private static (int ByteIndex, byte BitMask) RoomChozoBit(int bitIndex) =>
        Bank80SystemState.ResolvePersistentRoomBit(bitIndex, Bank80SystemState.RoomChozoBitByteCount, "Chozo-room bit index must fit the native 64-byte table.");

    /// <summary>Reads and sets the room Chozo bit table and addresses explored-map bytes by retail area.</summary>
    extension(Bank80SystemState self)
    {
        /// <summary>Whether the room's Chozo-statue bit is set in the native 64-byte table.</summary>
        internal bool HasRoomChozoBit(int bitIndex)
        {
            (int byteIndex, byte bitMask) = RoomChozoBit(bitIndex);
            return (self.RoomChozoBits[byteIndex] & bitMask) != 0;
        }

        /// <summary>Sets the room's Chozo-statue bit in the native 64-byte table.</summary>
        internal void SetRoomChozoBit(int bitIndex)
        {
            (int byteIndex, byte bitMask) = RoomChozoBit(bitIndex);
            self.RoomChozoBits[byteIndex] |= bitMask;
        }

        /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
        internal byte GetExploredMapByteRaw(AreaId areaIndex, int byteIndex) =>
            self.GetExploredMapByteRaw(AreaIds.ToIndex(areaIndex), byteIndex);
    }
}
