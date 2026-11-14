using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Rooms.RoomObjects.OldTourianEscapeShaftWallObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-wall draw of the old Tourian escape shaft and its block shape (#1862).</summary>
internal static class OldTourianEscapeShaftWallDraws
{
    /// <summary>The wall's opened blocks when <paramref name="request"/> is the wall's draw.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The opened blocks; null when the draw is not the wall's.</param>
    /// <returns>Whether the draw is the wall's.</returns>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        shape = request == OpenWallDraw ? OpenedWall : null;
        return shape is not null;
    }
}
