namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Four native two-by-two physical ceiling draws. Each list has two
/// horizontal runs, calculated without a stored layout cache. Editable visible
/// block selections remain a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{
    /// <summary><c>$84:9413</c>: clear the ceiling after defeat or crumble.</summary>
    internal const ushort ClearPointer = 0x9413;
    /// <summary><c>$84:9423</c>: first crumble appearance.</summary>
    internal const ushort CrumbleFirstPointer = 0x9423;
    /// <summary><c>$84:9433</c>: second crumble appearance.</summary>
    internal const ushort CrumbleSecondPointer = 0x9433;
    /// <summary><c>$84:9443</c>: third crumble appearance.</summary>
    internal const ushort CrumbleThirdPointer = 0x9443;
    /// <summary><c>$84:9453</c>: first byte of the following Mother Brain draw region.</summary>
    internal const ushort EndExclusive = 0x9453;

    /// <summary>
    /// Clear fills with the blank tile; crumble advances through tiles $53..55
    /// at the native sixteen-byte record stride. Every cell uses the same air word.
    /// </summary>
    internal static bool TryGetWord(ushort pointer, out ushort word)
    {
        if (pointer == ClearPointer)
        {
            word = 0x00ff;
            return true;
        }
        int relative = pointer - CrumbleFirstPointer;
        if (relative >= 0 && relative <= CrumbleThirdPointer - CrumbleFirstPointer && relative % 16 == 0)
        {
            word = (ushort)(0x53 + relative / 16);
            return true;
        }
        word = 0;
        return false;
    }

    // Temporary DTOs serve artwork interfaces; runtime draws the scalar square directly.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int pointer = ClearPointer; pointer < EndExclusive; pointer += 16)
            {
                TryGet((ushort)pointer, out var list);
                yield return list;
            }
        }
    }

    internal static string VisualId(ushort pointer) => pointer switch
    {
        ClearPointer => "clear-ceiling",
        CrumbleFirstPointer => "crumble-frame-0",
        CrumbleSecondPointer => "crumble-frame-1",
        CrumbleThirdPointer => "crumble-frame-2",
        _ => throw new InvalidDataException(
            $"Spore Spawn ceiling draw ${pointer:X4} has no visual ID."),
    };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (!string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }

    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => CrumbleFirstPointer,
        1 => CrumbleSecondPointer,
        2 => CrumbleThirdPointer,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (TryGetWord(pointer, out ushort word))
        {
            list = Square(pointer, word);
            return true;
        }
        list = default;
        return false;
    }

    private static RoomPlmShotBlockDrawDefinitions.DrawList Square(
        ushort pointer, ushort physicalWord) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, 0, 1),
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, 0, 0),
        });
}
