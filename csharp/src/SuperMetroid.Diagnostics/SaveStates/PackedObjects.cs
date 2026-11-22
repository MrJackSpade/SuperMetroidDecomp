namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects the elements of one packed run hold (#2609), distinct by identity, in the order the run first holds each:
/// an element's field holding one stores its place, and the state writes each object once, through the graph, with its
/// identity and its bound or definition path.
/// </summary>
internal sealed class PackedObjects
{
    private readonly List<object> objects = [];

    /// <summary>Starts an empty table, filled as a run is written.</summary>
    internal PackedObjects()
    {
    }

    /// <summary>A table read back from a state.</summary>
    private PackedObjects(List<object?> read) => objects.AddRange(read!);

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
