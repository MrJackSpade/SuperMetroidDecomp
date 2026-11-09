using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the looping room-FX animations (<see cref="RoomFxAnimatedTileMechanicsDefinitions"/>): every other defined
/// animation has no loop here, and an undefined one is rejected.
/// </summary>
public static class RoomFxAnimatedTiles
{
    /// <summary>Resolves a looping animation; any other defined animation resolves to false and null.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out RoomFxAnimatedTileLoop? definition)
    {
        definition = SelectObject(animatedObject);
        return definition is not null;
    }

    /// <summary>The loop of a looping animation, or null for any other defined animation.</summary>
    private static RoomFxAnimatedTileLoop? SelectObject(AnimatedTileObject animatedObject) =>
        Enum.IsDefined(animatedObject)
            ? RoomFxAnimatedTileMechanicsDefinitions.Objects.GetValueOrDefault(animatedObject)
            : throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}.");
}
