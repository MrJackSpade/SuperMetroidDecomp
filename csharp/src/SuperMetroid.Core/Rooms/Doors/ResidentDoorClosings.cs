using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>How a resident door or gate that is not a typed cap closes behind Samus.</summary>
internal static class ResidentDoorClosings
{
    /// <summary>
    /// The closing start of <paramref name="header"/>, by identity. Every header without one in
    /// <see cref="ResidentDoorClosingDefinitions.Closings"/> is rejected, including blue collision PLMs.
    /// </summary>
    /// <exception cref="InvalidDataException">The header has no compiled closing definition.</exception>
    internal static RoomObjectProgramStart Resolve(RoomObjectHeaderId header) =>
        ResidentDoorClosingDefinitions.Closings.TryGetValue(header, out RoomObjectProgramStart start)
            ? start
            : throw new InvalidDataException($"Resident door header {header} has no compiled closing definition.");
}
