namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The PLMs retail room populations place; every other PLM is rejected.</summary>
internal static class RoomObjectHeaders
{
    /// <summary>Selects a retail header or rejects a PLM absent from room populations.</summary>
    /// <exception cref="InvalidDataException">No retail room population places <paramref name="header"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="header"/> names no PLM.</exception>
    internal static RoomObjectHeaderDefinition Get(RoomObjectHeaderId header) =>
        !Enum.IsDefined(header) ? throw new InvalidOperationException($"Undefined RoomObjectHeaderId {header}.")
        : RoomObjectHeaderDefinitions.RetailHeaders.Contains(header)
            ? new(header)
            : throw new InvalidDataException($"Compiled room PLM headers lack retail header {header}.");

    /// <summary>The contact-crumble header that special-block reaction <paramref name="index"/> places.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the eight reactions.</exception>
    public static RoomObjectHeaderId ContactCrumbleByReactionIndex(int index) =>
        Ensure.Element(RoomObjectHeaderDefinitions.ContactCrumbleHeaders, index);
}
