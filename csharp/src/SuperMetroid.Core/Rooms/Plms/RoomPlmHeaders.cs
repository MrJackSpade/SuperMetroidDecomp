namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The PLMs retail room populations place; every other PLM is rejected.</summary>
internal static class RoomPlmHeaders
{
    /// <summary>Selects a retail header or rejects a PLM absent from room populations.</summary>
    /// <exception cref="InvalidDataException">No retail room population places <paramref name="header"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="header"/> names no PLM.</exception>
    internal static RoomPlmHeaderDefinition Get(PlmHeaderId header) =>
        !Enum.IsDefined(header) ? throw new InvalidOperationException($"Undefined PlmHeaderId {header}.")
        : RoomPlmHeaderDefinitions.RetailHeaders.Contains(header)
            ? new(header)
            : throw new InvalidDataException($"Compiled room PLM headers lack retail header {header}.");

    /// <summary>The contact-crumble header that special-block reaction <paramref name="index"/> places.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the eight reactions.</exception>
    public static PlmHeaderId ContactCrumbleByReactionIndex(int index) =>
        Ensure.Element(RoomPlmHeaderDefinitions.ContactCrumbleHeaders, index);
}
