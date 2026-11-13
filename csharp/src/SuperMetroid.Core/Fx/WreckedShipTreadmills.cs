using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the two Wrecked Ship treadmill block animations (<see cref="WreckedShipTreadmillMechanicsDefinitions"/>) by
/// conveyor direction or by block animation.
/// </summary>
public static class WreckedShipTreadmills
{
    /// <summary>Dispatches the two physical conveyor directions to their definitions.</summary>
    public static WreckedShipTreadmillLoop ForDirection(WreckedShipTreadmillDirection direction) =>
        WreckedShipTreadmillMechanicsDefinitions.Directions.TryGetValue(direction, out WreckedShipTreadmillLoop? loop)
            ? loop : throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}.");

    /// <summary>Resolves either named definition; every other block animation returns false/null.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out WreckedShipTreadmillLoop? definition)
    {
        if (!Enum.IsDefined(animatedObject))
            throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}.");
        definition = WreckedShipTreadmillMechanicsDefinitions.ObjectDirections.TryGetValue(animatedObject,
            out WreckedShipTreadmillDirection direction) ? ForDirection(direction) : null;
        return definition is not null;
    }
}
