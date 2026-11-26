using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// How the player's speed is read against its limits, and against what asks how fast they move up or down. The engine reads it plainly
/// (<see cref="PlainSpeedReadings"/>); a game whose speeds must be read some other way supplies its own through its
/// <see cref="Core.GameDefinition"/>.
/// </summary>
public interface ISpeedReadings
{
    /// <summary>Whether a speed that has just gained is past the top speed, so that the top speed replaces it.</summary>
    bool IsPastTop(Pixels speed, Pixels top);

    /// <summary>Whether a dash has reached its cap, so that the cap holds it and it gains no more.</summary>
    bool IsAtCap(Pixels dash, Pixels cap);

    /// <summary>
    /// Whether a speed up or down reads as at least <paramref name="whole"/> whole pixels an update.
    /// <paramref name="size"/> is the speed's size the way the player is held to be going
    /// (<see cref="Player.HeldSpeedUpOrDown"/>), which is less than nothing once gravity has worn the speed of a
    /// rise, or of a player adrift, past rest and they come down.
    /// </summary>
    bool ReachesWhole(Pixels size, int whole);
}
