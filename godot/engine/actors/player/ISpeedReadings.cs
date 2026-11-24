using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// How the player's speed across is read against its limits. The engine reads it plainly
/// (<see cref="PlainSpeedReadings"/>); a game whose speeds must be read some other way supplies its own through its
/// <see cref="Core.GameDefinition"/>.
/// </summary>
public interface ISpeedReadings
{
    /// <summary>Whether a speed that has just gained is past the top speed, so that the top speed replaces it.</summary>
    bool IsPastTop(Pixels speed, Pixels top);

    /// <summary>Whether a dash has reached its cap, so that the cap holds it and it gains no more.</summary>
    bool IsAtCap(Pixels dash, Pixels cap);
}
