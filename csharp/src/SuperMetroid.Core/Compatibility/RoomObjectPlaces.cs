namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The room's forty room-object places (#1622), which set the order its room objects update in and lose a spawn into a
/// full room. Every pass over the room's objects runs through the places in one order, the update order: the
/// room-object update, every search for the object that owns a block, and every spawn's search for a free place. A
/// spawn takes the first free place in that order, so in a room where nothing has left the objects update in the order
/// they were spawned, and a spawn after an object has left takes that object's place and updates where it did. A spawn
/// while all forty places are held takes none, and the object it would have made never exists: a gate trigger's shot,
/// a Speed Booster block's contact or a boss's terrain change is lost then.
/// </summary>
/// <remarks>
/// <para>
/// Taken by every room-object spawn (a room population's records in authored order, a block reaction, a door, a boss's
/// terrain change) before the object's setup runs; freed when the object leaves the room, which may be its own setup
/// (every scroll extension), leaving the place to the next spawn. Read in order by each room-object update and by a door's
/// or a gate trigger's search for the object at a block; read from the last place back by the searches for a touched or
/// shot collectible, station or scroll trigger. Lifetime: the room's; the room loader creates it with each room's
/// objects, so a new room starts with every place free.
/// </para>
/// <para>
/// The order is kept because the game observes it: a spawn made during the room-object update into a place the update
/// has still to reach runs on that same update, and into one it has passed, on the next; two requests made in one
/// update, such as the pair of mural rows Mother Brain's ascent asks for, update in the order of the places they took;
/// a search for the object at a block takes the first match in its order (a gate's trigger finding its gate in update
/// order, a touch finding its station from the last place back). The full room is kept because a room can fill: every
/// block reaction Samus starts holds a place
/// until its own updates end it, and while room-object updates are suspended (X-Ray, and G-Mode, which keeps them
/// suspended across a door into the next room while enemies and bosses keep running) none of those places is released,
/// so repeated reactions fill the room and a boss that changes it meanwhile finds no free place.
/// </para>
/// <para>
/// Evidence: the original room-object spawns search their forty places from the last down for a free one and report
/// none when all are held, and the room-object update runs through the places in the same order, so a population whose
/// setups free their places at once leaves them to its next records; the original touch searches run from the first
/// of the forty up, the reverse of that order.
/// </para>
/// </remarks>
[CompatibilityService("room objects update in the order of the forty places they hold; a spawn takes the first free place in that order, and a spawn into a full room is lost")]
public sealed class RoomObjectPlaces
{
    /// <summary>How many room objects the room holds at once; a spawn while every place is held takes none.</summary>
    public const int PlaceCount = 40;

    /// <summary>Whether each place, numbered in update order, holds a room object.</summary>
    private readonly bool[] _held = new bool[PlaceCount];

    /// <summary>How many places hold a room object.</summary>
    internal int HeldCount { get; private set; }

    /// <summary>Whether every place holds a room object, so a spawn would take none.</summary>
    internal bool IsFull => HeldCount == PlaceCount;

    /// <summary>Whether <paramref name="place"/> holds a room object.</summary>
    /// <param name="place">A place, numbered in update order from zero, the place the update reaches first.</param>
    internal bool IsHeld(int place) => _held[Ensure.BetweenInclusive(0, PlaceCount - 1, place)];

    /// <summary>Takes the first free place in update order for a spawn.</summary>
    /// <returns>The place taken, numbered in update order, or null when every place is held.</returns>
    internal int? Take()
    {
        for (int place = 0; place < PlaceCount; place++)
        {
            if (_held[place])
                continue;
            _held[place] = true;
            HeldCount++;
            return place;
        }
        return null;
    }

    /// <summary>Frees <paramref name="place"/>, whose room object has left the room.</summary>
    /// <param name="place">A held place.</param>
    /// <exception cref="InvalidOperationException">The place holds no room object.</exception>
    internal void Free(int place)
    {
        if (!IsHeld(place))
            throw new InvalidOperationException($"Room-object place {place} holds no room object.");
        _held[place] = false;
        HeldCount--;
    }
}
