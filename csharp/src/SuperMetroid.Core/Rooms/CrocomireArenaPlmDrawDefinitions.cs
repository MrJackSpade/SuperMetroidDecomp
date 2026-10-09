namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one word;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in the solid block bit. Native
/// continuation offsets are relative to the origin, not the preceding column.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    /// <summary><c>$84:9B5B</c>: ten cleared bridge blocks.</summary>
    internal const ushort ClearBridge = 0x9b5b;
    /// <summary><c>$84:9B73</c>: one crumbling bridge block.</summary>
    internal const ushort CrumbleBridgeBlock = 0x9b73;
    /// <summary><c>$84:9B79</c>: one cleared bridge block.</summary>
    internal const ushort ClearBridgeBlock = 0x9b79;
    /// <summary><c>$84:9B7F</c>: three columns of clear wall blocks.</summary>
    internal const ushort ClearInvisibleWall = 0x9b7f;
    /// <summary><c>$84:9BBB</c>: three columns of solid wall blocks.</summary>
    internal const ushort CreateInvisibleWall = 0x9bbb;

    /// <summary>Describes one native pointer's repeated bridge blocks or three-run arena wall.</summary>
    /// <param name="Pointer">Native bank-$84 pointer identifying this draw layout.</param>
    /// <param name="Wall">Whether the layout comprises three adjacent wall columns.</param>
    /// <param name="Solid">Whether emitted wall words set the solid-block bit.</param>
    /// <param name="WordsPerRun">Number of block words emitted in each run.</param>
    internal readonly record struct Draw(ushort Pointer, bool Wall, bool Solid, int WordsPerRun)
    {
        /// <summary>Number of horizontal runs: three for a wall and one for a bridge.</summary>
        internal int RunCount => Wall ? 3 : 1;

        /// <summary>Native direction/count word, with the high bit marking a wall run.</summary>
        internal ushort DirectionAndCount => (ushort)(WordsPerRun | (Wall ? 0x8000 : 0));

        /// <summary>Gets the continuation offset to the next run, or zero after the final run.</summary>
        /// <param name="run">Zero-based run being emitted.</param>
        /// <returns>The native next-X offset; invalid run indexes throw.</returns>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return run + 1 < RunCount ? (sbyte)(run + 1) : (sbyte)0;
        }
        /// <summary>Returns the native block word at a run and block position.</summary>
        /// <param name="run">Zero-based bridge or wall run.</param>
        /// <param name="block">Zero-based block within the run.</param>
        /// <returns>The tile and solidity bits for that cell; invalid indexes throw.</returns>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= WordsPerRun)
                throw new IndexOutOfRangeException();
            if (!Wall) return Pointer == CrumbleBridgeBlock ? (ushort)0x810b : (ushort)0x0080;
            int tile = block is 0 or >= 6 ? 0x80 :
                0x107 + run + (block == 5 ? 2 : (block - 1) % 2) * 0x20;
            return (ushort)(tile | (Solid ? 0x8000 : 0));
        }
    }

    /// <summary>Resolves a native Crocomire arena draw pointer to its bounded layout metadata.</summary>
    /// <param name="pointer">Bank-$84 pointer to inspect.</param>
    /// <param name="draw">Receives the matching draw layout, or the default value when unmatched.</param>
    /// <returns><see langword="true"/> when the pointer names a supported layout.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        draw = pointer switch
        {
            ClearBridge => new(pointer, false, false, 10),
            CrumbleBridgeBlock or ClearBridgeBlock => new(pointer, false, false, 1),
            ClearInvisibleWall => new(pointer, true, false, 8),
            CreateInvisibleWall => new(pointer, true, true, 8),
            _ => default,
        };
        return draw.Pointer != 0;
    }

    // Temporary DTOs serve the existing artwork interface; gameplay evaluates cells directly.
    /// <summary>Enumerates the five supported layouts as artwork-facing draw lists.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in EnumeratePointers())
            {
                TryGet(pointer, out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Yields the supported native draw pointers in catalog order.</summary>
    /// <returns>The bridge and invisible-wall pointer set.</returns>
    private static IEnumerable<ushort> EnumeratePointers()
    {
        yield return ClearBridge;
        yield return CrumbleBridgeBlock;
        yield return ClearBridgeBlock;
        yield return ClearInvisibleWall;
        yield return CreateInvisibleWall;
    }

    /// <summary>Builds the artwork-facing draw-list DTO for a supported native pointer.</summary>
    /// <param name="pointer">Bank-$84 draw pointer to translate.</param>
    /// <param name="draw">Receives the translated run and block words, or the default value when unmatched.</param>
    /// <returns><see langword="true"/> when the pointer has a known draw layout.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribe(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new(shape.DirectionAndCount, words, shape.NextX(run), 0);
        }
        draw = new(pointer, runs);
        return true;
    }

    /// <summary>Maps a supported native draw pointer to the stable artwork asset identifier.</summary>
    /// <param name="pointer">Native Crocomire arena draw pointer.</param>
    /// <returns>The corresponding visual asset identifier.</returns>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        ClearBridge => "clear-bridge",
        CrumbleBridgeBlock => "crumble-bridge-block",
        ClearBridgeBlock => "clear-bridge-block",
        ClearInvisibleWall => "clear-invisible-wall",
        CreateInvisibleWall => "create-invisible-wall",
        _ => throw new InvalidDataException(
            $"Crocomire arena draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Finds the draw-list DTO associated with a stable artwork asset identifier.</summary>
    /// <param name="id">Visual identifier to resolve.</param>
    /// <param name="draw">Receives the matching draw list, or the default value when unmatched.</param>
    /// <returns><see langword="true"/> when an identifier matches a supported layout.</returns>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
            {
                draw = candidate;
                return true;
            }
        }
        draw = default;
        return false;
    }

}
