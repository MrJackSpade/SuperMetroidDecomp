using Engine.World;

namespace Game.Rooms;

/// <summary>
/// The rumble of a quake that goes on, heard in an uneven rhythm of its own: whatever keeps the room shaking steps
/// it once on each update of the shaking. Rising lava and acid have it, and the statues' hall as its way down opens.
/// </summary>
/// <remarks>
/// The wait is counted down an update at a time and the rumble comes on the update the count has already run out;
/// the next wait is the next of the rhythm's, with nought to three updates more by the last number chance gave,
/// taken as it stands and not drawn, and with one more besides (<see cref="ExtraWait"/>). Whoever steps it says
/// where in the number the two digits it goes by lie: the lowest, or the lowest of its high half for a quake
/// stepped once lava or acid has stirred the number on the same move, which changes its halves round.
/// </remarks>
public sealed class QuakeRumble
{
    /// <summary>The waits between rumbles, in updates, taken in turn and round again.</summary>
    private static readonly int[] Waits = [1, 3, 2, 1, 1, 2, 2, 1];

    /// <summary>
    /// The update added to every wait. The cartridge adds it for a save file whose history holds anything, a
    /// completed game or a start that found no saved game; and every save file that play can make holds one or the
    /// other from its first start on.
    /// </summary>
    private const int ExtraWait = 1;

    /// <summary>How many digits of the number lengthen a wait.</summary>
    private const int ChanceDigits = 2;

    private int _wait;
    private int _next;

    /// <summary>Starts the rhythm over: the next step is heard.</summary>
    public void StartOver() => (_wait, _next) = (0, 0);

    /// <summary>
    /// Passes one update of the quake in <paramref name="room"/>, asking for <paramref name="sound"/> when its wait
    /// has run out; the digits of the last number that lengthen the next wait lie at <paramref name="chanceAt"/>.
    /// </summary>
    public void Step(Room room, string sound, int chanceAt)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrEmpty(sound);
        if (_wait > 0)
        {
            _wait--;
            return;
        }

        room.Session.Sounds.Request(sound, channel: "room");
        _wait = Waits[_next] + room.Session.Random.Last.Part(chanceAt, ChanceDigits) + ExtraWait;
        _next = (_next + 1) % Waits.Length;
    }
}
