namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Selects a Ceres steam vent's program and function from its variant (<see cref="CeresSteamDefinitions"/>).</summary>
internal static class CeresSteamInitializations
{
    /// <summary>The instruction program and function <paramref name="variant"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="variant"/> is not one of the six authored variants.</exception>
    internal static CeresSteamInitialization Initialization(CeresSteamVariant variant) =>
        CeresSteamDefinitions.Initializations.TryGetValue(variant, out CeresSteamInitialization initialization)
            ? initialization
            : throw new InvalidDataException($"Ceres steam variant {variant} exceeds its six-entry tables.");
}
