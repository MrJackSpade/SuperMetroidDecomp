using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Finds the definition of a PLM a Chozo statue publishes.</summary>
internal static class ChozoStatueObjectHeaders
{
    /// <summary>Resolves a supported header without interpreting adjacent data.</summary>
    /// <exception cref="InvalidDataException"><paramref name="header"/> is not one of the five Chozo PLMs.</exception>
    internal static ChozoStatueObjectDefinition Resolve(RoomObjectHeaderId header) =>
        ChozoStatueObjectDefinitions.ByHeader.TryGetValue(header, out ChozoStatueObjectDefinition definition)
            ? definition
            : throw new InvalidDataException($"Unsupported Chozo PLM {header}.");
}
