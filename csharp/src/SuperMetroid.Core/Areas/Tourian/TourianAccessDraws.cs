using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Describes the Tourian access floor's draws (<see cref="TourianAccessObjectDefinitions"/>, <see cref="TourianAccessObjectDrawDefinitions"/>):
/// the block shape each draw opens (#1862).
/// </summary>
internal static class TourianAccessDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the floor's draws.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The rows the draw opens; null when the draw is not the floor's.</param>
    /// <returns>Whether the draw is one of the floor's.</returns>
    /// <exception cref="InvalidOperationException">The draw is a floor draw with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<TourianAccessDraw> { Frame: var floor })
        {
            shape = null;
            return false;
        }
        shape = TourianAccessObjectDrawDefinitions.Shapes.TryGetValue(floor, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined TourianAccessDraw {floor}.");
        return true;
    }
}
