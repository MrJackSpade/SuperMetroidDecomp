namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>Retail room identities and liquid-surface data used by frame publication checks.</summary>
internal static class RoomPublicationFixtureDefinitions
{
    /// <summary>Retail Brinstar water room $01/$27, header $8F:A3DD.</summary>
    internal const ushort BrinstarWater = 0xa3dd;
    /// <summary>Retail Norfair Business Center $8F:A75D, used by the existing heat audit.</summary>
    internal const ushort NorfairBusinessCenter = 0xa75d;
    /// <summary>Authored liquid surface Y from Business Center's default FX record.</summary>
    internal const ushort BusinessCenterSurface = 0x00b8;
}
