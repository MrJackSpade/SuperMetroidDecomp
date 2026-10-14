using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="DoorClosingPlmRomData"/> members production does not use.</summary>
internal static class DoorClosingPlmRomDataAccess
{
    /// <summary>Resolves a native door-closing header from its encoded orientation.</summary>
    extension(DoorClosingPlmRomData)
    {
        /// <summary>Returns the bank-$84 header selected by a raw orientation byte, or the table's zero word.</summary>
        internal static ushort GetHeader(byte orientation) =>
            (ushort)(DoorClosingPlmRomData.GetDefinition(CartridgeDoorOrientation.Decode(orientation))?.Header ?? PlmHeaderId.None);
    }
}
