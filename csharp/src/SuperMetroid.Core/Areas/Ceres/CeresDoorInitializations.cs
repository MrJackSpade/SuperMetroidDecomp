namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Selects a Ceres door's behaviour from its variant (<see cref="CeresDoorInitializationDefinitions"/>).</summary>
internal static class CeresDoorInitializations
{
    /// <summary>The main function and instruction program <paramref name="variant"/> starts with.</summary>
    internal static CeresDoorInitializationDefinition For(CeresDoorVariant variant) =>
        Ensure.Listed(CeresDoorInitializationDefinitions.Initializations, variant);
}
