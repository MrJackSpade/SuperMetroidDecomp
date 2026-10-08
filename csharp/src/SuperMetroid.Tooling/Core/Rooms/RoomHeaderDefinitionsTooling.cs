using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="RoomHeaderDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomHeaderDefinitionsTooling
{
    /// <summary>Whether this is exactly one of the selected retail room identities.</summary>
    public static bool Contains(ushort roomPointer) => RoomHeaderDefinitions.Select(roomPointer) is not null;
}
