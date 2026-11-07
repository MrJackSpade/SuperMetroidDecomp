namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values and helpers of <see cref="SamusEaterObjectDefinitions"/> that only verification reads.</summary>
internal static class SamusEaterObjectDefinitionsConstants
{
    /// <summary>$84:AC89, pin Samus to saved coordinates and OR immunity with $10.</summary>
    public const ushort HoldFrameBehavior = 0xac89;
}
