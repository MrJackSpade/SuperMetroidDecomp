using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

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
        if (!Enum.IsDefined((SporeSpawnCeilingDraw)pointer))
        {
            word = 0;
            return false;
        }
        var list = (SporeSpawnCeilingDraw)pointer;
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

    /// <summary>Temporary draw records for the artwork interface; runtime draws the scalar two-by-two square directly.</summary>
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

    /// <summary>Decodes a bank-$84 draw pointer before selecting its published artwork key.</summary>
    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<SporeSpawnCeilingDraw>(pointer, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw pointer) => pointer switch
    {
        SporeSpawnCeilingDraw.Clear => "clear-ceiling",
        SporeSpawnCeilingDraw.CrumbleFirst => "crumble-frame-0",
        SporeSpawnCeilingDraw.CrumbleSecond => "crumble-frame-1",
        SporeSpawnCeilingDraw.CrumbleThird => "crumble-frame-2",
        _ => throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {pointer}."),
    };

    /// <summary>Finds the native physical square associated with an editable ceiling-frame key.</summary>
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

    /// <summary>Maps zero-based crumble stage zero through two to its bank-$84 draw pointer.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => (ushort)SporeSpawnCeilingDraw.CrumbleFirst,
        1 => (ushort)SporeSpawnCeilingDraw.CrumbleSecond,
        2 => (ushort)SporeSpawnCeilingDraw.CrumbleThird,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Builds a two-run physical square for a supported clear or crumble pointer.</summary>
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

    /// <summary>Duplicates one physical tile across the two horizontal runs of the ceiling's two-by-two layout.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList Square(
        ushort pointer, ushort physicalWord) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, new(0, 1)),
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, new(0, 0)),
        });
}
