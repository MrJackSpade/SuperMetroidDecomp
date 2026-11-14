using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Lays the escape animals' rescue walls (#1862).</summary>
internal static class EscapeAnimalWalls
{
    /// <summary>The block shape of <paramref name="request"/> when it is one of the four rescue-wall draws.</summary>
    /// <param name="request">The draw to describe; another family's draw is rejected.</param>
    /// <param name="shape">The wall's column; null when the draw is not a rescue wall's.</param>
    /// <returns>Whether the draw is a rescue wall's.</returns>
    /// <exception cref="InvalidOperationException">The draw is a rescue wall with no shape.</exception>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out RoomBlockShape shape)
    {
        if (request is not RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: var wall })
        {
            shape = null;
            return false;
        }
        shape = EscapeAnimalObjectDrawDefinitions.Shapes.TryGetValue(wall, out RoomBlockShape? described)
            ? described
            : throw new InvalidOperationException($"Undefined EscapeAnimalDraw {wall}.");
        return true;
    }
}
