using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The four bank-$84 Spore Spawn ceiling draw lists, valued by native address.</summary>
internal enum SporeSpawnCeilingDraw : ushort
{
    /// <summary><c>$84:9413</c>: clear the ceiling after defeat or crumble.</summary>
    Clear = 0x9413,
    /// <summary><c>$84:9423</c>: first crumble appearance.</summary>
    CrumbleFirst = 0x9423,
    /// <summary><c>$84:9433</c>: second crumble appearance.</summary>
    CrumbleSecond = 0x9433,
    /// <summary><c>$84:9443</c>: third crumble appearance.</summary>
    CrumbleThird = 0x9443,
}

/// <summary>
/// Four native two-by-two physical ceiling draws. Each list has two
/// horizontal runs, calculated without a stored layout cache. Editable visible
/// block selections remain a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{
    /// <summary><c>$84:9453</c>: first byte of the following Mother Brain draw region.</summary>
    internal const ushort EndExclusive = 0x9453;

    /// <summary>
    /// Clear fills with the blank tile; crumble advances through tiles $53..55
    /// in its three successive sixteen-byte records. Every cell uses the same air word.
    /// </summary>
    internal static bool TryGetWord(ushort pointer, out ushort word)
    {
        var list = (SporeSpawnCeilingDraw)pointer;
        if (!Enum.IsDefined(list))
        {
            word = 0;
            return false;
        }
        word = list switch
        {
            SporeSpawnCeilingDraw.Clear => 0x00ff,
            SporeSpawnCeilingDraw.CrumbleFirst => 0x53,
            SporeSpawnCeilingDraw.CrumbleSecond => 0x54,
            SporeSpawnCeilingDraw.CrumbleThird => 0x55,
            _ => throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {list}."),
        };
        return true;
    }

    // Temporary DTOs serve artwork interfaces; runtime draws the scalar square directly.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int pointer = (int)SporeSpawnCeilingDraw.Clear; pointer < EndExclusive; pointer += 16)
            {
                TryGet((ushort)pointer, out var list);
                yield return list;
            }
        }
    }

    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<SporeSpawnCeilingDraw>(pointer, "Spore Spawn ceiling draw with a visual ID"));

    internal static string VisualId(SporeSpawnCeilingDraw pointer) => pointer switch
    {
        SporeSpawnCeilingDraw.Clear => "clear-ceiling",
        SporeSpawnCeilingDraw.CrumbleFirst => "crumble-frame-0",
        SporeSpawnCeilingDraw.CrumbleSecond => "crumble-frame-1",
        SporeSpawnCeilingDraw.CrumbleThird => "crumble-frame-2",
        _ => throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {pointer}."),
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
        0 => (ushort)SporeSpawnCeilingDraw.CrumbleFirst,
        1 => (ushort)SporeSpawnCeilingDraw.CrumbleSecond,
        2 => (ushort)SporeSpawnCeilingDraw.CrumbleThird,
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
