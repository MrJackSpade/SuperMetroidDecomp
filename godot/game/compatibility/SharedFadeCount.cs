using Engine.Core;

namespace Game.Compatibility;

/// <summary>
/// The count of steps that every gradual fade of the cartridge shares: a door's fade out and in, the black-out of
/// Samus's death, the time up's white-out, the fades of Kraid's backdrop and the fades of a Torizo's body. Each
/// takes its step by this count, moves it on, and puts it back to nothing as it completes; none puts it back as it
/// starts, but Kraid, whose room is drawn anew before his fades. So a fade begun after one that stopped part way
/// completes that much sooner: a Torizo's waking takes sixteen steps of a fade that completes on its fifteenth, and
/// leaves the count at one, and a death during his fight is over an update sooner for it.
/// </summary>
/// <remarks>
/// When a fade completes is this count's to say; what a fade shows on the way is its own count of the steps it has
/// taken. The count is the game's, and starts over with each game begun. What only reads it holds still while it is
/// past nothing: the giant Metroid's membrane, while a door fades.
/// </remarks>
public sealed class SharedFadeCount : ISessionService
{
    /// <summary>How many steps have been taken since a fade last completed.</summary>
    public int Count { get; private set; }

    /// <summary>Whether a fade is under way, or one that stopped part way has left the count where it stopped.</summary>
    public bool IsPastStart => Count != 0;

    /// <summary>
    /// Takes one step of a fade whose colours last move with the count at <paramref name="lastStep"/>: while the
    /// count stands at nothing through that, the fade takes a step and the count moves on; once it has passed it,
    /// the fade completes, taking no step, and the count is put back to nothing.
    /// </summary>
    /// <returns>Whether the fade completed on this update.</returns>
    public bool TakeStep(int lastStep)
    {
        if (Count > lastStep)
        {
            Count = 0;
            return true;
        }

        Count++;
        return false;
    }

    /// <summary>Puts the count back to nothing, as a room drawn anew before its fades does.</summary>
    public void Restart() => Count = 0;

    /// <inheritdoc/>
    public void BeginAgain() => Count = 0;

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
