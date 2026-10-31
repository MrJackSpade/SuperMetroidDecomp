using static SuperMetroid.Core.Enemies.Animals.EscapeEtecoonDefinitions;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Starts an escape Etecoon in the role its population entry names.</summary>
internal static class EscapeEtecoons
{
    /// <summary>The initialization of <paramref name="role"/>.</summary>
    internal static EscapeEtecoonInitialization Initialization(EscapeEtecoonRole role) =>
        Ensure.Listed(Initializations, role);
}
