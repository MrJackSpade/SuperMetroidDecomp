using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="CartridgeDoorHeader"/> that only verification reads.</summary>
internal static class CartridgeDoorHeaderConstants
{
    /// <summary>Encoded size of one retail bank-$83 door header.</summary>
    public const int SizeInBytes = DoorHeaderRomDataConstants.RecordByteCount;

    /// <summary>Provides the encoded cartridge door header size used when checking native door records.</summary>
    extension(CartridgeDoorHeader)
    {
        /// <inheritdoc cref="CartridgeDoorHeaderConstants.SizeInBytes"/>
        internal static int SizeInBytes => CartridgeDoorHeaderConstants.SizeInBytes;
    }
}
