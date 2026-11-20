namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a room enemy place has held since its room loaded (#2558): no enemy yet (<see cref="NeverPopulatedEnemyPlace"/>),
/// or, from its first spawn on, the history of a populated place (<see cref="PopulatedEnemyPlace"/>), which always keeps
/// the population entry its latest occupant was spawned from. A place never returns to holding no history.
/// </summary>
public abstract class EnemyPlaceHistory
{
    /// <summary>Limits the histories to the never-populated and the populated place's.</summary>
    private protected EnemyPlaceHistory()
    {
    }
}
