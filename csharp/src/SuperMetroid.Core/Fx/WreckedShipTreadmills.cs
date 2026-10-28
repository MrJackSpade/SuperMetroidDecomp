using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the two Wrecked Ship treadmill animated-tile objects (<see cref="WreckedShipTreadmillMechanicsDefinitions"/>) by
/// conveyor direction or by animated-tile object.
/// </summary>
public static class WreckedShipTreadmills
{
    /// <summary>Dispatches the two physical conveyor directions to their native headers.</summary>
    public static WreckedShipTreadmillLoop ForDirection(WreckedShipTreadmillDirection direction) =>
        WreckedShipTreadmillMechanicsDefinitions.Directions.TryGetValue(direction, out WreckedShipTreadmillLoop? loop)
            ? loop : throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}.");

    /// <summary>Resolves either named header; all other bank87 identities return false/null.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out WreckedShipTreadmillLoop? definition)
    {
        if (!Enum.IsDefined(animatedObject))
            throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}.");
        definition = WreckedShipTreadmillMechanicsDefinitions.ObjectDirections.TryGetValue(animatedObject,
            out WreckedShipTreadmillDirection direction) ? ForDirection(direction) : null;
        return definition is not null;
    }
}
