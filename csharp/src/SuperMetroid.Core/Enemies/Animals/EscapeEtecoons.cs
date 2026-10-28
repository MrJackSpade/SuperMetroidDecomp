using static SuperMetroid.Core.Enemies.Animals.EscapeEtecoonDefinitions;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Starts an escape Etecoon in the role its population parameter selects.</summary>
internal static class EscapeEtecoons
{
    /// <summary>The initialization of the role <paramref name="parameter1"/> selects, ignoring its bit zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameter1"/> selects none of the three roles.</exception>
    internal static EscapeEtecoonInitialization Initialization(int parameter1) =>
        Roles.TryDecode(parameter1 & RoleParameterMask, out EscapeEtecoonRole role)
        && Initializations.TryGetValue(role, out EscapeEtecoonInitialization initialization)
            ? initialization
            : throw new ArgumentOutOfRangeException(nameof(parameter1), parameter1,
                "Escape Etecoon parameter must select one of the three authored roles.");
}
