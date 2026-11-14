namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The behaviour Nuclear Waffle's main AI dispatches. “Nuclear Waffle” is the enemy's traditional
/// name; the game's own name table calls it Puromi.
/// Its values are arbitrary.
/// </summary>
public enum NuclearWaffleAction
{
    /// <summary>Decrements the signed-expiry wait timer, then resets the sweep and joint-orientation latches and enables offscreen processing.</summary>
    Waiting,
    /// <summary>Positions the head and seven interleaved links, advances angular speed, and returns to waiting when the final projectile link reaches the endpoint.</summary>
    Sweeping,
}
