namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea's flight (#2063): a <see cref="CirclingFirefleaState"/> or a
/// <see cref="BobbingFirefleaState"/>, the two flights its spawn settings choose between.
/// </summary>
public abstract partial class FirefleaFlightState
{
    /// <summary>Closes the hierarchy to the flights declared here.</summary>
    private protected FirefleaFlightState()
    {
    }
}
