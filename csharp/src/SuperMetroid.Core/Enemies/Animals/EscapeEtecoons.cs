using static SuperMetroid.Core.Enemies.Animals.EscapeEtecoonDefinitions;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Starts an escape Etecoon in the role its placement names.</summary>
internal static partial class EscapeEtecoons
{
    /// <summary>The initialization of <paramref name="role"/>.</summary>
    internal static EscapeEtecoonInitialization Initialization(EscapeEtecoonRole role) =>
        Initializations[role];
}
