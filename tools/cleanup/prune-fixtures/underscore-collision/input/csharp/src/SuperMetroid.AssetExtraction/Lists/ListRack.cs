namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The places a rack holds entries at.</summary>
internal sealed class RackPlaces
{
    /// <summary>Places for <paramref name="capacity"/> entries.</summary>
    /// <param name="capacity">How many entries the places hold at most.</param>
    internal RackPlaces(int capacity)
    {
        Capacity = capacity;
    }

    /// <summary>How many entries the places hold at most.</summary>
    internal int Capacity { get; }
}

/// <summary>A rack of list entries over its places.</summary>
internal sealed class ListRack
{
    /// <summary>The rack's places.</summary>
    private readonly RackPlaces _places;

    /// <summary>A rack over <paramref name="rackPlaces"/>.</summary>
    /// <param name="rackPlaces">The rack's places.</param>
    internal ListRack(RackPlaces rackPlaces)
    {
        _places = rackPlaces;
    }

    /// <summary>How many places the rack has, which production reads under the name its catalog gives it.</summary>
    internal int places => _places.Capacity;

    /// <summary>Twice the rack's places, which only checks read.</summary>
    internal int Doubled => this._places.Capacity * 2;
}
