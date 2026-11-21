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

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this fireflea flight state, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(FirefleaFlightStateReactions<TContext> reactions, TContext context);
}

/// <summary>What the owner of a <see cref="FirefleaFlightState"/> does on each of its cases (#2553).</summary>
/// <param name="BobbingFirefleaState">Reacts to a <see cref="BobbingFirefleaState"/>.</param>
/// <param name="CirclingFirefleaState">Reacts to a <see cref="CirclingFirefleaState"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record FirefleaFlightStateReactions<TContext>(Action<BobbingFirefleaState, TContext> BobbingFirefleaState, Action<CirclingFirefleaState, TContext> CirclingFirefleaState);
