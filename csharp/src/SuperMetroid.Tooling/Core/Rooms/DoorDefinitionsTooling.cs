namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="DoorDefinitions"/>; never linked by player hosts.</summary>
internal static class DoorDefinitionsTooling
{
    /// <summary>Materializes an immutable caller-owned view of the calculated native BTS order.</summary>
    public static DoorListDefinition GetList(ushort doorListPointer)
    {
        DoorDefinitions.DoorListLayout layout = DoorDefinitions.ListLayout(doorListPointer);
        var pointers = new ushort[layout.Count];
        for (int index = 0; index < pointers.Length; index++)
            pointers[index] = DoorDefinitions.PointerAt(doorListPointer, layout, index);
        return new DoorListDefinition(doorListPointer, pointers);
    }
}
