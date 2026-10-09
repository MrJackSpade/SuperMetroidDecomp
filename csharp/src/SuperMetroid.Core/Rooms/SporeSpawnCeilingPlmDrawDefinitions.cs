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
    /// <summary>Enumerates the clear draw and all three crumble appearances in native pointer order.</summary>
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

    /// <summary>Maps a supported ceiling draw pointer to its stable editable-art identifier.</summary>
    /// <param name="pointer">Native bank-$84 draw-list pointer.</param>
    /// <returns>The identifier used to select the matching visual entry.</returns>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        ClearPointer => "clear-ceiling",
        CrumbleFirstPointer => "crumble-frame-0",
        CrumbleSecondPointer => "crumble-frame-1",
        CrumbleThirdPointer => "crumble-frame-2",
        _ => throw new InvalidDataException(
            $"Spore Spawn ceiling draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Finds a ceiling draw list by its exact, case-sensitive visual identifier.</summary>
    /// <param name="id">Stable identifier for the clear or one of the three crumble appearances.</param>
    /// <param name="list">Receives the matching physical draw list, or the default value when the identifier is unknown.</param>
    /// <returns><see langword="true"/> when the identifier corresponds to a supported appearance.</returns>
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

    /// <summary>Returns the native draw-list pointer for one of the three ordered crumble animation frames.</summary>
    /// <param name="frame">Zero-based crumble frame index from zero through two.</param>
    /// <returns>The bank-$84 pointer for the selected frame.</returns>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => CrumbleFirstPointer,
        1 => CrumbleSecondPointer,
        2 => CrumbleThirdPointer,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Resolves a supported draw-list pointer to its two-by-two physical block layout.</summary>
    /// <param name="pointer">Native pointer of the clear or crumble draw list.</param>
    /// <param name="list">Receives the physical draw definition, or the default value when the pointer is unsupported.</param>
    /// <returns><see langword="true"/> when the pointer identifies the clear list or a crumble frame.</returns>
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

    /// <summary>Builds two horizontal runs that cover a two-by-two square with one physical level word.</summary>
    /// <param name="pointer">Native draw-list address represented by the returned definition.</param>
    /// <param name="physicalWord">Block value repeated in all four cells.</param>
    /// <returns>The run-based draw description for the physical square.</returns>
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
