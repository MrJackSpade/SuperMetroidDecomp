namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The places a list holds entries at.</summary>
internal sealed class ListPlaces
{
    /// <summary>Places for <paramref name="capacity"/> entries, none of them held.</summary>
    /// <param name="capacity">How many entries the places hold at most.</param>
    internal ListPlaces(int capacity)
    {
        Capacity = capacity;
    }

    /// <summary>How many entries the places hold at most.</summary>
    internal int Capacity { get; }

    /// <summary>How many places hold an entry.</summary>
    internal int Held { get; private set; }

    /// <summary>Takes the next free place.</summary>
    internal void Take() => Held++;
}

/// <summary>A list's slots over its places; <see cref="_places"/> says which hold an entry.</summary>
internal sealed class ListSlots
{
    /// <summary>The list's places.</summary>
    private readonly ListPlaces _places;

    /// <summary>Creates the slots over <paramref name="places"/>.</summary>
    /// <param name="places">The list's places.</param>
    internal ListSlots(ListPlaces places)
    {
        _places = places;
    }

    /// <summary>The name production reports the slots' places under.</summary>
    internal static string PlacesName => nameof(_places);

    /// <summary>How many slots hold an entry, which production reads.</summary>
    internal int Count => _places.Held;

    /// <summary>How many slots are free, which only checks read: <see cref="_places"/> less those held.</summary>
    internal int Free => this._places.Capacity - Count;

    /// <summary>How many slots <paramref name="slots"/> holds, which only checks read.</summary>
    /// <param name="slots">The slots, if any.</param>
    internal static int HeldBy(ListSlots? slots) => slots?._places.Held ?? 0;
}
