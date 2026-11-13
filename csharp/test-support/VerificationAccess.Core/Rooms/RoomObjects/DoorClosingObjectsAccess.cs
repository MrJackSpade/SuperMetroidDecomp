namespace SuperMetroid.Core.Rooms.RoomObjects;

using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="DoorClosingObjects"/> members production does not use.</summary>
internal static class DoorClosingObjectsAccess
{
    /// <summary>Resolves a native door-closing header from its encoded orientation.</summary>
    extension(DoorClosingObjects)
    {
        /// <summary>Returns the bank-$84 header selected by a raw orientation byte, or the table's zero word.</summary>
        internal static ushort GetHeader(byte orientation) =>
            RoomObjectNativeWords.Word(DoorClosingObjects.GetDefinition(DoorRecordCodec.DecodeOrientation(orientation))?.Kind ?? RoomObjectKind.None);
    }
}
