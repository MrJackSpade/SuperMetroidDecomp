namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete bank-$84 draw lists selected by station animation programs. The native
/// level words are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomPlmStationDrawDefinitions
{
    /// <summary>First map-station frame draw list at $84:9F25.</summary>
    internal const ushort MapFirst = 0x9f25;
    /// <summary>First energy-station frame draw list at $84:9F6D.</summary>
    internal const ushort EnergyFirst = 0x9f6d;
    /// <summary>First missile-station frame draw list at $84:9F91.</summary>
    internal const ushort MissileFirst = 0x9f91;
    /// <summary>Save-pod idle draw list at $84:9A3F.</summary>
    internal const ushort SaveIdle = 0x9a3f;
    /// <summary>First active save-pod draw list at $84:9A9F.</summary>
    internal const ushort SaveActive = 0x9a9f;
    /// <summary>Second active save-pod draw list at $84:9A6F.</summary>
    internal const ushort SaveAlternate = 0x9a6f;
    /// <summary>Retracted right-side map access draw at $84:9F49.</summary>
    private const ushort MapRightRetracted = 0x9f49;
    /// <summary>Extended right-side map access draw at $84:9F55.</summary>
    private const ushort MapRightExtended = 0x9f55;
    /// <summary>Retracted left-side map access draw at $84:9F5B.</summary>
    private const ushort MapLeftRetracted = 0x9f5b;
    /// <summary>Extended left-side map access draw at $84:9F67.</summary>
    private const ushort MapLeftExtended = 0x9f67;
    /// <summary>Retracted right-side resource access draw at $84:9FB5.</summary>
    private const ushort ResourceRightRetracted = 0x9fb5;
    /// <summary>Extended right-side resource access draw at $84:9FBB.</summary>
    private const ushort ResourceRightExtended = 0x9fbb;
    /// <summary>Retracted left-side resource access draw at $84:9FC1.</summary>
    private const ushort ResourceLeftRetracted = 0x9fc1;
    /// <summary>Extended left-side resource access draw at $84:9FC7.</summary>
    private const ushort ResourceLeftExtended = 0x9fc7;

    /// <summary>Native geometry and tile-packing rule used to materialize a station draw list.</summary>
    internal enum LayoutKind
    {
        /// <summary>Two-cell map station frame whose second cell continues one tile to the left.</summary>
        Map,
        /// <summary>Two-cell energy station frame whose upper cell uses the preceding tile row.</summary>
        Energy,
        /// <summary>Two-cell missile station frame with the resource-station tile progression.</summary>
        Missile,
        /// <summary>Six-row save pod with paired floor/cap cells and a vertical shaft between them.</summary>
        Save,
        /// <summary>Retracted or extended map access panel, mirrored according to its side.</summary>
        MapAccess,
        /// <summary>Retracted or extended resource access panel, with its trigger replaced on extension.</summary>
        ResourceAccess
    }

    /// <summary>
    /// Native station runs are horizontal. Map displays write origin then left;
    /// resource displays write origin then above. Save pods have six two-cell rows
    /// from floor to cap. Access layouts select side and extension explicitly.
    /// All continuations are absolute origin-relative offsets, not accumulated.
    /// Map art advances 32 tiles per frame; resource art advances one and its upper
    /// cell is 32 tiles earlier. Resource upper cells are slopes. Save floor/cap
    /// are solid except the idle floor-left trigger; the shaft is air. Save art
    /// mirrors horizontally, its floor also vertically, and active-A advances one
    /// tile. Map access mirrors by side; resource access changes trigger to solid
    /// on extension. These rules cover only the twenty native owned draw pointers.
    /// </summary>
    /// <param name="Pointer">Native draw-list pointer selecting this station layout.</param>
    /// <param name="Kind">Physical layout rule used to generate its room words.</param>
    /// <param name="Frame">Zero-based animation frame for map, energy, or missile stations.</param>
    /// <param name="Left">True when an access layout is the left-facing variant.</param>
    /// <param name="Extended">True when an access panel is in its extended state.</param>
    internal readonly record struct Draw(ushort Pointer, LayoutKind Kind, int Frame, bool Left = false, bool Extended = false)
    {
        /// <summary>Number of origin-relative draw runs, including each save pod row.</summary>
        internal int RunCount => Kind switch
        {
            LayoutKind.Save => 6,
            LayoutKind.MapAccess => Extended ? 1 : 2,
            LayoutKind.ResourceAccess => 1,
            _ => 2,
        };
        /// <summary>Validates that a run index belongs to this layout.</summary>
        /// <param name="run">Zero-based run index to check.</param>
        /// <exception cref="IndexOutOfRangeException">The run is outside this layout's run count.</exception>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Returns the number of physical room words in a validated run.</summary>
        /// <param name="run">Zero-based run index.</param>
        internal int WordCount(int run) { CheckRun(run); return Kind == LayoutKind.Save ? 2 : 1; }
        /// <summary>Returns the X offset from the current run to the next run's origin.</summary>
        /// <param name="run">Zero-based run index whose continuation offset is requested.</param>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            if (run != 0) return 0;
            return Kind == LayoutKind.Map ? (sbyte)-1 : Kind == LayoutKind.MapAccess && !Extended
                ? Left ? (sbyte)3 : (sbyte)-3 : (sbyte)0;
        }
        /// <summary>Returns the Y offset from the current run to the next run's origin.</summary>
        /// <param name="run">Zero-based run index whose continuation offset is requested.</param>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return Kind == LayoutKind.Save ? run < 5 ? (sbyte)(-run - 1) : (sbyte)0
                : Kind is LayoutKind.Energy or LayoutKind.Missile && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Builds one native level word from the layout's tile, flip, and collision rules.</summary>
        /// <param name="run">Zero-based draw run containing the requested cell.</param>
        /// <param name="cell">Zero-based word position within the run.</param>
        /// <exception cref="IndexOutOfRangeException">The run or cell is outside this layout.</exception>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            int tile, collision, flip = 0;
            switch (Kind)
            {
                case LayoutKind.Map:
                    tile = 0x10c + Frame * 32 - run; collision = 8; break;
                case LayoutKind.Energy:
                case LayoutKind.Missile:
                    tile = (Kind == LayoutKind.Energy ? 0xc4 : 0xc7) + Frame - run * 32;
                    collision = run == 0 ? 8 : 1; break;
                case LayoutKind.Save:
                    bool shaft = run is > 0 and < 5;
                    tile = (shaft ? 0x5b : 0x59) + (Pointer == SaveActive ? 1 : 0);
                    flip = cell * 0x400 | (run == 0 ? 0x800 : 0);
                    collision = shaft ? 0 : Pointer == SaveIdle && run == 0 && cell == 0 ? 11 : 8;
                    break;
                case LayoutKind.MapAccess:
                    tile = Extended ? 0x129 : 0x128; collision = 8;
                    flip = (Left ^ (run != 0)) ? 0x400 : 0; break;
                default:
                    tile = Extended ? 0xc1 : 0xc3; collision = Extended ? 8 : 11;
                    flip = Left ? 0 : 0x400; break;
            }
            return (ushort)(collision << 12 | flip | tile);
        }
    }
    /// <summary>Maps a native station draw pointer to its compact physical layout description.</summary>
    /// <param name="pointer">Bank-$84 draw-list address to resolve.</param>
    /// <param name="draw">Receives the layout description when the pointer is recognized.</param>
    /// <returns>True when the pointer belongs to one of the supported station draw lists.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        for (int frame = 0; frame < 3; frame++)
        {
            if (pointer == MapFirst + frame * 12) { draw = new(pointer, LayoutKind.Map, frame); return true; }
            if (pointer == EnergyFirst + frame * 12) { draw = new(pointer, LayoutKind.Energy, frame); return true; }
            if (pointer == MissileFirst + frame * 12) { draw = new(pointer, LayoutKind.Missile, frame); return true; }
        }
        draw = pointer switch
        {
            SaveIdle or SaveActive or SaveAlternate => new(pointer, LayoutKind.Save, 0),
            MapRightRetracted => new(pointer, LayoutKind.MapAccess, 0),
            MapRightExtended => new(pointer, LayoutKind.MapAccess, 0, Extended: true),
            MapLeftRetracted => new(pointer, LayoutKind.MapAccess, 0, Left: true),
            MapLeftExtended => new(pointer, LayoutKind.MapAccess, 0, Left: true, Extended: true),
            ResourceRightRetracted => new(pointer, LayoutKind.ResourceAccess, 0),
            ResourceRightExtended => new(pointer, LayoutKind.ResourceAccess, 0, Extended: true),
            ResourceLeftRetracted => new(pointer, LayoutKind.ResourceAccess, 0, Left: true),
            ResourceLeftExtended => new(pointer, LayoutKind.ResourceAccess, 0, Left: true, Extended: true),
            _ => default,
        };
        return draw.Pointer != 0;
    }
    /// <summary>Yields the twenty supported station draw pointers in stable native grouping order.</summary>
    private static IEnumerable<ushort> Pointers()
    {
        for (int frame = 0; frame < 3; frame++)
        {
            yield return (ushort)(MapFirst + frame * 12);
            yield return (ushort)(EnergyFirst + frame * 12);
            yield return (ushort)(MissileFirst + frame * 12);
        }
        yield return SaveIdle;
        yield return SaveActive;
        yield return SaveAlternate;
        yield return MapRightRetracted;
        yield return MapRightExtended;
        yield return MapLeftRetracted;
        yield return MapLeftExtended;
        yield return ResourceRightRetracted;
        yield return ResourceRightExtended;
        yield return ResourceLeftRetracted;
        yield return ResourceLeftExtended;
    }
    /// <summary>Enumerates complete physical draw lists for every supported station state.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in Pointers())
            {
                TryGet(pointer, out var list);
                yield return list;
            }
        }
    }
    // Temporary artwork DTOs; runtime draws calculate cells directly.
    /// <summary>Materializes the physical room-word runs for one recognized station draw pointer.</summary>
    /// <param name="pointer">Bank-$84 draw-list address to resolve.</param>
    /// <param name="list">Receives the generated runs on success, or the default value on failure.</param>
    /// <returns>True when the pointer identifies a supported station list.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the physical words. Only complete native lists have IDs.
    /// </summary>
    internal static string VisualId(ushort pointer)
    {
        for (int frame = 0; frame < 3; frame++)
        {
            if (pointer == MapFirst + frame * 12) return $"map-frame-{frame}";
            if (pointer == EnergyFirst + frame * 12) return $"energy-frame-{frame}";
            if (pointer == MissileFirst + frame * 12) return $"missile-frame-{frame}";
        }
        return pointer switch
        {
            SaveIdle => "save-idle",
            SaveActive => "save-active-a",
            SaveAlternate => "save-active-b",
            MapRightRetracted => "map-right-retracted",
            MapRightExtended => "map-right-extended",
            MapLeftRetracted => "map-left-retracted",
            MapLeftExtended => "map-left-extended",
            ResourceRightRetracted => "resource-right-retracted",
            ResourceRightExtended => "resource-right-extended",
            ResourceLeftRetracted => "resource-left-retracted",
            ResourceLeftExtended => "resource-left-extended",
            _ => throw new InvalidDataException($"Station draw list ${pointer:X4} has no visual ID."),
        };
    }

    /// <summary>Finds a physical draw list by its stable artwork identifier.</summary>
    /// <param name="id">Ordinal visual identifier returned by <see cref="VisualId"/>.</param>
    /// <param name="list">Receives the matching draw list, or the default value if no identifier matches.</param>
    /// <returns>True when a supported visual identifier is found.</returns>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

}
