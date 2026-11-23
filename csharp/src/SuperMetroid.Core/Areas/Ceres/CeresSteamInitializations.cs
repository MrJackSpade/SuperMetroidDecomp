namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Selects a Ceres steam vent's animation and action from its variant (<see cref="CeresSteamDefinitions"/>).</summary>
internal static partial class CeresSteamInitializations
{
    /// <summary>The animation and action <paramref name="variant"/> selects.</summary>
    internal static CeresSteamInitialization Initialization(CeresSteamVariant variant) =>
        CeresSteamDefinitions.Initializations[variant];
}
