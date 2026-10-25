namespace SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to <see cref="DoorClosingPlms"/> members production does not use.</summary>
internal static class DoorClosingPlmsAccess
{
    /// <summary>Resolves a native door-closing header from its encoded orientation.</summary>
    extension(DoorClosingPlms)
    {
        /// <summary>Returns the bank-$84 header selected by a raw orientation byte, or the table's zero word.</summary>
        internal static ushort GetHeader(byte orientation) =>
            PlmNativeWords.Word(DoorClosingPlms.GetDefinition(CartridgeDoorOrientation.Decode(orientation))?.Header ?? PlmHeaderId.None);
    }
}
