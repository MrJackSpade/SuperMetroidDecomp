using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>A room FX header: the records LoadFXHeader walks for the entry door, in native order.</summary>
/// <remarks>Door-specific records come first and the list ends at its default record or terminator. Mother Brain's
/// header keeps, after its default, the records her FX entries select by index.</remarks>
public sealed class RoomFxDefinition
{
    /// <summary>Holds a header's records in native order.</summary>
    /// <param name="name">The header's name, the disassembly's label for it without its prefix, or a fixture's description.</param>
    /// <param name="records">The header's records in native order.</param>
    public RoomFxDefinition(string name, RoomFxRecordDefinition[] records)
    {
        Name = Ensure.NotNull(name);
        Records = Array.AsReadOnly(Ensure.NotNull(records).ToArray());
    }

    /// <summary>The header's name.</summary>
    public string Name { get; }

    /// <summary>The header's records in native order.</summary>
    public IReadOnlyList<RoomFxRecordDefinition> Records { get; }

    /// <summary>Selects the first default or matching-door record, or none at the terminator.</summary>
    /// <remarks>Native $89:AB99..ABB7 treats the terminator record as unconditional, before comparing the
    /// requested door. A record without a door is the default.</remarks>
    public RoomFxRecordDefinition? Select(CartridgeDoorHeader? door)
    {
        foreach (RoomFxRecordDefinition record in Records)
        {
            if (record.Terminates)
                return null;
            if (record.Door is null || record.Door == door)
                return record;
        }
        throw new InvalidDataException($"Room FX {Name} has no default or terminator record.");
    }
}
