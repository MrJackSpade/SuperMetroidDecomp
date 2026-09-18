namespace SuperMetroid.Core.Game;

/// <summary>Fixed physical definitions for Draygon's six death/burial Evir actors.</summary>
internal static class DraygonBurialEvirDefinitions
{
    /// <summary>
    /// Combines the six XY subspeed pairs at <c>$A5:A1AF-$A5:A1C6</c>, the six initial
    /// XY positions at <c>$A5:A1C7-$A5:A1DE</c>, and the six angle records at
    /// <c>$A5:A1DF-$A5:A1F6</c>. Each native angle record has an unused zero word after
    /// its meaningful value; retaining the authored six-record domain prevents adjacent
    /// death-program bytes from becoming physical data.
    /// </summary>
    private static readonly DraygonBurialEvirDefinition[] Entries =
    [
        new(0xd4da, 0x8e39, 0xff59, 0x00e5, 0x0068),
        new(0x8e39, 0xd4da, 0xffe5, 0x0059, 0x0058),
        new(0x31f1, 0xfb13, 0x009c, 0x000d, 0x0048),
        new(0x31f1, 0xfb13, 0x0163, 0x000d, 0x0038),
        new(0x8e39, 0xd4da, 0x021a, 0x0059, 0x0028),
        new(0xd4da, 0x8e39, 0x02a6, 0x00e5, 0x0018),
    ];

    /// <summary>Returns the physical definition for one native zero-through-five entry.</summary>
    internal static DraygonBurialEvirDefinition ForEntry(int entry)
    {
        if ((uint)entry >= Entries.Length)
        {
            throw new InvalidDataException(
                $"Draygon burial Evir entry {entry} exceeds its six-record table.");
        }

        return Entries[entry];
    }
}

/// <summary>One compiled Draygon death/burial Evir position and movement record.</summary>
internal readonly record struct DraygonBurialEvirDefinition(
    ushort XSubspeed,
    ushort YSubspeed,
    ushort InitialX,
    ushort InitialY,
    ushort Angle);
