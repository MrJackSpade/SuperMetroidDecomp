using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape each of the eight floor and ceiling plant draws draws (<see cref="SamusEaterObjectDrawDefinitions"/>,
/// #1862). What each frame shows is its installed picture (#1557).
/// </summary>
internal static class SamusEaterObjectDraws
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the plant's draws.</summary>
    /// <param name="request">The draw to describe.</param>
    /// <param name="shape">The plant's blocks; null when the draw is not the plant's.</param>
    /// <returns>Whether the draw is one of the plant's.</returns>
    /// <exception cref="InvalidOperationException">The draw is a plant draw with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<SamusEaterDraw> { Frame: var plant })
        {
            shape = null;
            return false;
        }
        shape = SamusEaterObjectDrawDefinitions.Shapes.TryGetValue(plant, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined SamusEaterDraw {plant}.");
        return true;
    }
}
