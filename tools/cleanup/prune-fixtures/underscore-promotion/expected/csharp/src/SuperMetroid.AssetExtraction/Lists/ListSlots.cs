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

/// <summary>A list's slots over its places; <see cref="places"/> says which hold an entry.</summary>
internal sealed class ListSlots
{
    /// <summary>The list's places.</summary>
    internal readonly ListPlaces places;

    /// <summary>Creates the slots over <paramref name="places"/>.</summary>
    /// <param name="places">The list's places.</param>
    internal ListSlots(ListPlaces places)
    {
        this.places = places;
    }

    /// <summary>The name production reports the slots' places under.</summary>
    internal static string PlacesName => nameof(places);

    /// <summary>How many slots hold an entry, which production reads.</summary>
    internal int Count => places.Held;
}
