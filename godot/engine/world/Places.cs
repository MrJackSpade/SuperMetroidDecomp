namespace Engine.World;

/// <summary>
/// The places a room keeps for what its enemies launch and leave behind: their shots, the remains of the destroyed
/// and the pickups dropped. Each such thing holds one place for as long as it is in play, and they take their turns in
/// the order of their places.
/// </summary>
/// <remarks>
/// With no limit a thing takes the place after the last taken, none is ever refused, and turns go from the first
/// place on, to the things that were there as the turns began. With a limit, a thing takes the highest free place and
/// is refused when every place is held; turns go from the highest place down, each place asked as its turn comes, so
/// a thing that takes a lower place while the turns are being taken has its turn among them, and one that takes a
/// higher place waits for the next update's. A place is free again the moment what held it is taken out of play.
/// </remarks>
internal sealed class Places
{
    /// <summary>What holds each place; null where a place is free. It grows as places are taken when there is no limit.</summary>
    private readonly List<IRoomEntity?> _held = [];

    /// <summary>How many places there are, or zero for no limit.</summary>
    private readonly int _limit;

    /// <summary>The places of a room, <paramref name="limit"/> of them, or without limit when it is zero.</summary>
    public Places(int limit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(limit);
        _limit = limit;
        for (int place = 0; place < limit; place++)
            _held.Add(null);
    }

    /// <summary>Whether a thing asking for a place now would be given one.</summary>
    public bool HasFree => _limit == 0 || _held.Contains(null);

    /// <summary>Gives <paramref name="thing"/> a place. Returns whether there was one to give.</summary>
    public bool TryTake(IRoomEntity thing)
    {
        ArgumentNullException.ThrowIfNull(thing);
        if (_limit == 0)
        {
            _held.Add(thing);
            return true;
        }

        int free = _held.LastIndexOf(null);
        if (free < 0)
            return false;
        _held[free] = thing;
        return true;
    }

    /// <summary>The place <paramref name="thing"/> holds, counted from none; null when it holds none.</summary>
    public int? PlaceOf(IRoomEntity thing)
    {
        int place = _held.IndexOf(thing);
        return place < 0 ? null : place;
    }

    /// <summary>Gives the place <paramref name="leaving"/> holds to <paramref name="taking"/>, which holds it from now on.</summary>
    /// <exception cref="InvalidOperationException">The thing leaving holds no place.</exception>
    public void HandOver(IRoomEntity leaving, IRoomEntity taking)
    {
        ArgumentNullException.ThrowIfNull(taking);
        int place = _held.IndexOf(leaving);
        if (place < 0)
            throw new InvalidOperationException("A place was handed over by something that holds none.");
        _held[place] = taking;
    }

    /// <summary>Frees the place <paramref name="thing"/> holds, if it holds one.</summary>
    public void Free(IRoomEntity thing)
    {
        int place = _held.IndexOf(thing);
        if (place < 0)
            return;
        if (_limit == 0)
            _held.RemoveAt(place);
        else
            _held[place] = null;
    }

    /// <summary>Every thing that holds a place, in the order of their turns, as it stands now.</summary>
    public IReadOnlyList<IRoomEntity> Held()
    {
        var held = new List<IRoomEntity>();
        foreach (IRoomEntity? thing in _limit == 0 ? _held : Enumerable.Reverse(_held))
        {
            if (thing is not null)
                held.Add(thing);
        }

        return held;
    }

    /// <summary>
    /// Gives each thing that holds a place its turn, in the order of the places: with no limit, the things that hold
    /// places as the turns begin; with a limit, whatever holds each place as its turn comes.
    /// </summary>
    public void TakeTurns(Action<IRoomEntity> turn)
    {
        ArgumentNullException.ThrowIfNull(turn);
        if (_limit == 0)
        {
            foreach (IRoomEntity thing in Held())
                turn(thing);
            return;
        }

        for (int place = _limit - 1; place >= 0; place--)
        {
            if (_held[place] is { } thing)
                turn(thing);
        }
    }
}
