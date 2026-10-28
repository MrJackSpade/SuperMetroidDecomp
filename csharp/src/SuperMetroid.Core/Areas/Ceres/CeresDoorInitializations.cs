namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Selects a Ceres door's behaviour from its population parameter (<see cref="CeresDoorInitializationDefinitions"/>).</summary>
internal static class CeresDoorInitializations
{
    /// <summary>The main function and instruction program population variant <paramref name="variant"/> selects.</summary>
    internal static CeresDoorInitializationDefinition For(int variant) =>
        Ensure.Listed(CeresDoorInitializationDefinitions.Initializations,
            CeresDoorInitializationDefinitions.Variants.Decode(variant), nameof(variant));
}
