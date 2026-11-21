namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects the elements of one packed run hold (#2609), distinct by identity, in the order the run first holds each:
/// an element's field holding one stores its place, and the state writes each object once, through the graph, with its
/// identity and its bound or definition path.
/// </summary>
internal sealed class PackedObjects
{
    private readonly List<object> objects = [];
    private readonly Dictionary<object, int> places = new(ReferenceEqualityComparer.Instance);

    /// <summary>Starts an empty table, filled as a run is written.</summary>
    internal PackedObjects()
    {
    }

    /// <summary>A table read back from a state.</summary>
    private PackedObjects(List<object?> read) => objects.AddRange(read!);

    /// <summary>How many objects the table holds.</summary>
    internal int Count => objects.Count;

    /// <summary>The place of <paramref name="value"/>, adding it when the run first holds it.</summary>
    internal int Place(object value)
    {
        if (places.TryGetValue(value, out int place))
            return place;
        place = objects.Count;
        objects.Add(value);
        places.Add(value, place);
        return place;
    }

    /// <summary>The object at <paramref name="place"/>.</summary>
    /// <exception cref="InvalidDataException">The table holds no such object.</exception>
    internal object? At(int place) => (uint)place < (uint)objects.Count
        ? objects[place]
        : throw new InvalidDataException($"A packed run names held object {place}, but its table holds {objects.Count}.");

    /// <summary>Writes every object through the graph, after their count.</summary>
    internal void Write(GraphWriter writer)
    {
        writer.Output.Write(objects.Count);
        foreach (object value in objects)
            writer.Write(value);
    }

    /// <summary>Reads a run's table back, inside struct storage, as a framed struct element's objects are read.</summary>
    internal static PackedObjects Read(GraphReader reader)
    {
        var read = new List<object?>();
        int count = reader.ReadLength("packed held object");
        reader.InStructElementsDo(() =>
        {
            for (int index = 0; index < count; index++)
                read.Add(reader.Read());
        });
        return new PackedObjects(read);
    }
}
