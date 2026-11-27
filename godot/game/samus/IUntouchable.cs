namespace Game;

/// <summary>Marks a state of Samus's in which no hit can land on her.</summary>
public interface IUntouchable
{
    /// <summary>Whether no hit can land on her now: all through the state, unless the state says when.</summary>
    bool NoHitLands => true;
}
