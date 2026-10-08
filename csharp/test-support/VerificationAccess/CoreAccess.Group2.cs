using SuperMetroid.Core.Rooms;

internal static partial class CoreAccess
{
    extension(RoomPlmSystem plms)
    {
        /// <summary>
        /// Native PLM_Vars bit fifteen of the live resident scroll trigger at
        /// <paramref name="blockIndex"/>: set by `$84:B393` contact, cleared when the
        /// bank-$8F program returns the object to sleep. Throws unless exactly one live
        /// scroll PLM owns that block.
        /// </summary>
        internal bool ScrollPlmTriggered(int blockIndex)
        {
            object[] owners = PrivateState.Field<Array>(plms, "_slots").Cast<object>()
                .Where(slot => PrivateState.Property<bool>(slot, "Active") &&
                    PrivateState.Property<int>(slot, "BlockIndex") == blockIndex &&
                    PrivateState.Property<object?>(slot, "Scroll") is not null)
                .ToArray();
            if (owners.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one live scroll PLM at block {blockIndex}; found {owners.Length}.");
            return PrivateState.Property<bool>(PrivateState.Property<object>(owners[0], "Scroll"), "Triggered");
        }
    }
}
