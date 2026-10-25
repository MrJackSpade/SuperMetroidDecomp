namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of <see cref="DoorDefinition"/> that only verification reads.</summary>
internal static class DoorDefinitionConstants
{
    /// <summary>Encoded size of one retail bank-$83 door header.</summary>
    public const int SizeInBytes = DoorHeaderRomDataConstants.RecordByteCount;

    /// <summary>Provides the encoded cartridge door header size used when checking native door records.</summary>
    extension(DoorDefinition)
    {
        /// <inheritdoc cref="DoorDefinitionConstants.SizeInBytes"/>
        internal static int SizeInBytes => DoorDefinitionConstants.SizeInBytes;
    }
}
