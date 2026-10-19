using SuperMetroid.Core.Rooms;

namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>Retail room identities and liquid-surface data used by frame publication checks.</summary>
internal static class RoomPublicationFixtureDefinitions
{
    /// <summary>Retail Brinstar water room $01/$27, header $8F:A3DD.</summary>
    internal static RoomHeaderDefinition BrinstarWater => RoomHeaderDefinitions.SkreeBoost;
    /// <summary>Retail Norfair Business Center $8F:A75D, used by the existing heat audit.</summary>
    internal static RoomHeaderDefinition NorfairBusinessCenter => RoomHeaderDefinitions.IceBeamAcid;
    /// <summary>Authored liquid surface Y from Business Center's default FX record.</summary>
    internal const ushort BusinessCenterSurface = 0x00b8;
}
