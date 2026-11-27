using Game.Compatibility;

namespace Game.Tests.Movies;

/// <summary>
/// The pace of a room's load a movie's host gives the game: what the console the recording was made on took. The
/// recording's list of updates shows it: after the update a door's passage loads its room on, the scroll goes on by
/// itself until the cartridge's main loop turns again, and the updates say how many of the room's enemies the
/// loader had set up by each.
/// </summary>
internal sealed class CapturedRoomLoadPace : IRoomLoadPace
{
    private IReadOnlyList<MovieUpdate> _updates = [];
    private int _playing;

    /// <summary>Takes the movie's updates and which of them is about to be played.</summary>
    public void Expect(IReadOnlyList<MovieUpdate> updates, int playing) => (_updates, _playing) = (updates, playing);

    /// <summary>The first update, from the one being played on, that the cartridge's main loop turned on.</summary>
    private int Turn
    {
        get
        {
            for (int index = _playing; index < _updates.Count; index++)
            {
                if (_updates[index].Kind != MovieUpdate.Continuation)
                    return index;
            }

            throw new InvalidDataException($"The movie ends inside the door's scroll that update {_updates[_playing].Number} loads a room on.");
        }
    }

    /// <inheritdoc/>
    public int UpdatesBeforeTurn => Turn - _playing;

    /// <inheritdoc/>
    public int UpdatesBeforeEnemies
    {
        get
        {
            // The loader's count is given from the turn on while the scroll lasts. A room with no enemies never has
            // one above nought: nothing waits to be set up.
            int turn = Turn;
            for (int index = turn; index < _updates.Count && (index == turn || _updates[index].Kind == MovieUpdate.Continuation); index++)
            {
                if (_updates[index].EnemiesSetUp is > 0)
                    return index - turn;
            }

            return 0;
        }
    }
}
