using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the simple retail room-FX animated-tile objects (<see cref="RoomFxAnimatedTileMechanicsDefinitions"/>): every
/// other defined animated-tile object has no header here, and an undefined one is rejected.
/// </summary>
public static class RoomFxAnimatedTiles
{
    /// <summary>The definition of an object this catalog supports.</summary>
    internal static RoomFxAnimatedTileLoop Require(AnimatedTileObject animatedObject) => SelectObject(animatedObject) ??
        throw new InvalidOperationException($"{animatedObject} has no definition in this catalog.");

    /// <summary>Enumerates the seven supported objects.</summary>
    public static IEnumerable<RoomFxAnimatedTileLoop> All
    {
        get
        {
            yield return Require(AnimatedTileObject.HorizontalSpikes);
            yield return Require(AnimatedTileObject.MaridiaSandCeiling);
            yield return Require(AnimatedTileObject.MaridiaSandFalling);
            yield return Require(AnimatedTileObject.Lava);
            yield return Require(AnimatedTileObject.Acid);
            yield return Require(AnimatedTileObject.Rain);
            yield return Require(AnimatedTileObject.Spores);
        }
    }

    /// <summary>Resolves a supported header; unknown identities preserve false and null output.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out RoomFxAnimatedTileLoop? definition)
    {
        definition = SelectObject(animatedObject);
        return definition is not null;
    }

    /// <summary>The header of a supported object, or null for any other defined object.</summary>
    private static RoomFxAnimatedTileLoop? SelectObject(AnimatedTileObject animatedObject) =>
        Enum.IsDefined(animatedObject)
            ? RoomFxAnimatedTileMechanicsDefinitions.Objects.GetValueOrDefault(animatedObject)
            : throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}.");
}
