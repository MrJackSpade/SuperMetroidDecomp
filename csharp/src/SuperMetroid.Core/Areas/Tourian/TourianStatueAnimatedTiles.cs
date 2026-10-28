using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Resolves the four Tourian entrance statues' animated-tile definitions
/// (<see cref="TourianStatueAnimatedTileMechanicsDefinitions"/>) from their animated-tile objects.
/// </summary>
public static class TourianStatueAnimatedTiles
{
    /// <summary>The four boss-statue programs in cartridge header order.</summary>
    public static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return Require(AnimatedTileObject.TourianStatuePhantoon);
            yield return Require(AnimatedTileObject.TourianStatueRidley);
            yield return Require(AnimatedTileObject.TourianStatueKraid);
            yield return Require(AnimatedTileObject.TourianStatueDraygon);
        }
    }

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

    /// <summary>The definition of a statue's animated-tile object.</summary>
    private static TourianStatueAnimatedTileProgramDefinition Require(AnimatedTileObject animatedObject) => SelectObject(animatedObject) ??
        throw new InvalidOperationException($"{animatedObject} has no definition in this catalog.");
}
