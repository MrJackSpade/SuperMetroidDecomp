using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room FX header (#1546): the records its room's entry doors select, in the game's order, and the default record a room
/// entered through any other door loads, or none when the header loads no FX. The header ends at the end of its lists.
/// </summary>
public sealed class RoomFxHeader
{
    /// <summary>Holds a header's door records in the game's order and its default record.</summary>
    /// <param name="name">The header's name, or a fixture's description.</param>
    /// <param name="doorRecords">The records entry doors select, in the game's order.</param>
    /// <param name="defaultRecord">The record any other entry loads, or null when the header loads no FX for it.</param>
    public RoomFxHeader(string name, RoomFxDoorRecord[] doorRecords, RoomFxRecordDefinition? defaultRecord)
    {
        Name = Ensure.NotNull(name);
        DoorRecords = Array.AsReadOnly(Ensure.NotNull(doorRecords).ToArray());
        Default = defaultRecord;
        var records = new List<RoomFxRecordDefinition>(DoorRecords.Count + 1);
        records.AddRange(DoorRecords.Select(record => record.Record));
        if (defaultRecord is not null)
            records.Add(defaultRecord);
        Records = records.AsReadOnly();
    }

    /// <summary>The header's name.</summary>
    public string Name { get; }

    /// <summary>The records entry doors select, in the game's order: the first naming the entry door is loaded.</summary>
    public IReadOnlyList<RoomFxDoorRecord> DoorRecords { get; }

    /// <summary>The record a room entered through a door no door record names loads, or null for no FX.</summary>
    public RoomFxRecordDefinition? Default { get; }

    /// <summary>Every record the header can load: its door records' records in the game's order, then its default record.</summary>
    public IReadOnlyList<RoomFxRecordDefinition> Records { get; }

    /// <summary>Selects the first door record naming <paramref name="door"/>, or else the default record, or none.</summary>
    public RoomFxRecordDefinition? Select(DoorDefinition? door)
    {
        foreach (RoomFxDoorRecord record in DoorRecords)
        {
            if (record.Door == door)
                return record.Record;
        }
        return Default;
    }
}
