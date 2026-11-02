using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Resolves the four Tourian entrance statues' animated-tile definitions
/// (<see cref="TourianStatueAnimatedTileMechanicsDefinitions"/>) from their animated-tile objects.
/// </summary>
public static class TourianStatueAnimatedTiles
{
    /// <summary>Resolves one statue's animated-tile definition.</summary>
    public static bool TryResolveObjectHeader(
        AnimatedTileObject animatedObject,
        [NotNullWhen(true)] out TourianStatueAnimatedTileProgramDefinition? definition)
    {
        definition = SelectObject(animatedObject);
        return definition is not null;
    }

    /// <summary>The definition of a statue's animated-tile object, or null for any other defined object.</summary>
    private static TourianStatueAnimatedTileProgramDefinition? SelectObject(AnimatedTileObject animatedObject) =>
        Enum.IsDefined(animatedObject)
            ? TourianStatueAnimatedTileMechanicsDefinitions.Statues.GetValueOrDefault(animatedObject)
            : throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}.");
}
