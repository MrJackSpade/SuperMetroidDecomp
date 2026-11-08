namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species states one occupant of an enemy place owns, keyed by state type (#1543). A place starts a new set for
/// every occupant it binds and drops the set when the occupant is deleted or killed, so a state lives exactly as long
/// as the occupant its species installed it for. A reaction that runs on after the hit that killed its enemy reaches
/// the struck occupant's set through the hit's context (<see cref="EnemyShotContext.StruckOccupant"/>,
/// <see cref="EnemyBombContext.StruckOccupant"/>).
/// </summary>
internal sealed class EnemyOccupantStates
{
    /// <summary>Each installed state, keyed by its type.</summary>
    private readonly Dictionary<Type, object> _states = [];

    /// <summary>The occupant's <typeparamref name="T"/>, or null when its species installed none.</summary>
    internal T? Find<T>() where T : class =>
        _states.TryGetValue(typeof(T), out object? state) ? (T)state : null;

    /// <summary>The occupant's <typeparamref name="T"/>, which its species installs before reading it.</summary>
    /// <exception cref="InvalidOperationException">The occupant has no <typeparamref name="T"/>.</exception>
    internal T Require<T>() where T : class =>
        Find<T>() ?? throw new InvalidOperationException($"The enemy has no {typeof(T).Name} for this occupant.");

    /// <summary>Installs or replaces the occupant's <typeparamref name="T"/>.</summary>
    internal void Install<T>(T state) where T : class => _states[typeof(T)] = Ensure.NotNull(state);
}
