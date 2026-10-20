using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete bank-$84 draw lists selected by station animation programs. The native
/// level words are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomPlmStationDrawDefinitions
{
    /// <summary>Byte stride between a station's consecutive frame draw lists.</summary>
    internal const int FrameStride = 12;

    /// <summary>Published native enumeration order: frames by station, then save and access states.</summary>
    private static readonly StationDraw[] NativeOrder =
    [
        StationDraw.MapFrame0, StationDraw.EnergyFrame0, StationDraw.MissileFrame0,
        StationDraw.MapFrame1, StationDraw.EnergyFrame1, StationDraw.MissileFrame1,
        StationDraw.MapFrame2, StationDraw.EnergyFrame2, StationDraw.MissileFrame2,
        StationDraw.SaveIdle, StationDraw.SaveActive, StationDraw.SaveAlternate,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];

    /// <summary>Station families with distinct native run geometry and collision words.</summary>
    internal enum LayoutKind
    {
        /// <summary>Two horizontal display cells, with tile identity advancing by 32 per frame.</summary>
        Map,
        /// <summary>Vertical refill display whose upper cell is a slope.</summary>
        Energy,
        /// <summary>Vertical ammunition display whose upper cell is a slope.</summary>
        Missile,
        /// <summary>Six two-cell rows from the save-pod floor through its cap.</summary>
        Save,
        /// <summary>Side-selectable map access cells with retracted and extended poses.</summary>
        MapAccess,
        /// <summary>Side-selectable resource access cell whose extension changes collision.</summary>
        ResourceAccess,
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
    internal readonly record struct Draw(StationDraw Pointer, LayoutKind Kind, int Frame, bool Left = false, bool Extended = false)
    {
        /// <summary>Number of origin-relative draw runs for this station layout and extension state.</summary>
        internal int RunCount => Kind switch
        {
            LayoutKind.Save => 6,
            LayoutKind.MapAccess => Extended ? 1 : 2,
            LayoutKind.ResourceAccess => 1,
            LayoutKind.Map or LayoutKind.Energy or LayoutKind.Missile => 2,
            _ => throw new InvalidOperationException($"Undefined LayoutKind {Kind}."),
        };
        /// <summary>Rejects a run outside this layout's native command count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Save rows write two adjacent cells; other station runs write one.</summary>
        internal int WordCount(int run) { CheckRun(run); return Kind == LayoutKind.Save ? 2 : 1; }
        /// <summary>Horizontal origin-relative offset for the next run, including map leftward and access-side spacing.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            if (run != 0) return 0;
            return Kind == LayoutKind.Map ? (sbyte)-1 : Kind == LayoutKind.MapAccess && !Extended
                ? Left ? (sbyte)3 : (sbyte)-3 : (sbyte)0;
        }
        /// <summary>Vertical origin-relative offset for save rows and resource upper cells.</summary>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return Kind == LayoutKind.Save ? run < 5 ? (sbyte)(-run - 1) : (sbyte)0
                : Kind is LayoutKind.Energy or LayoutKind.Missile && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Builds one native level word from the station's frame, tile, flip, and collision rules.</summary>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
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
                    tile = (shaft ? 0x5b : 0x59) + (Pointer == StationDraw.SaveActive ? 1 : 0);
                    flip = cell * 0x400 | (run == 0 ? 0x800 : 0);
                    collision = shaft ? 0 : Pointer == StationDraw.SaveIdle && run == 0 && cell == 0 ? 11 : 8;
                    break;
                case LayoutKind.MapAccess:
                    tile = Extended ? 0x129 : 0x128; collision = 8;
                    flip = (Left ^ (run != 0)) ? 0x400 : 0; break;
                case LayoutKind.ResourceAccess:
                    tile = Extended ? 0xc1 : 0xc3; collision = Extended ? 8 : 11;
                    flip = Left ? 0 : 0x400; break;
                default:
                    throw new InvalidOperationException($"Undefined LayoutKind {Kind}.");
            }
            return (ushort)(collision << 12 | flip | tile);
        }
    }
    /// <summary>
    /// Describes a draw pointer handed over by the shared PLM draw interpreter; pointers
    /// outside the twenty station lists belong to other families.
    /// </summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((StationDraw)pointer))
        {
            draw = default;
            return false;
        }
        var station = (StationDraw)pointer;
        draw = station switch
        {
            StationDraw.MapFrame0 => new(station, LayoutKind.Map, 0),
            StationDraw.MapFrame1 => new(station, LayoutKind.Map, 1),
            StationDraw.MapFrame2 => new(station, LayoutKind.Map, 2),
            StationDraw.EnergyFrame0 => new(station, LayoutKind.Energy, 0),
            StationDraw.EnergyFrame1 => new(station, LayoutKind.Energy, 1),
            StationDraw.EnergyFrame2 => new(station, LayoutKind.Energy, 2),
            StationDraw.MissileFrame0 => new(station, LayoutKind.Missile, 0),
            StationDraw.MissileFrame1 => new(station, LayoutKind.Missile, 1),
            StationDraw.MissileFrame2 => new(station, LayoutKind.Missile, 2),
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate => new(station, LayoutKind.Save, 0),
            StationDraw.MapRightRetracted => new(station, LayoutKind.MapAccess, 0),
            StationDraw.MapRightExtended => new(station, LayoutKind.MapAccess, 0, Extended: true),
            StationDraw.MapLeftRetracted => new(station, LayoutKind.MapAccess, 0, Left: true),
            StationDraw.MapLeftExtended => new(station, LayoutKind.MapAccess, 0, Left: true, Extended: true),
            StationDraw.ResourceRightRetracted => new(station, LayoutKind.ResourceAccess, 0),
            StationDraw.ResourceRightExtended => new(station, LayoutKind.ResourceAccess, 0, Extended: true),
            StationDraw.ResourceLeftRetracted => new(station, LayoutKind.ResourceAccess, 0, Left: true),
            StationDraw.ResourceLeftExtended => new(station, LayoutKind.ResourceAccess, 0, Left: true, Extended: true),
            _ => throw new InvalidOperationException($"Undefined station draw ${pointer:X4}."),
        };
        return true;
    }
    /// <summary>Calculates the twenty complete native station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (StationDraw pointer in NativeOrder)
            {
                TryGet((ushort)pointer, out var list);
                yield return list;
            }
        }
    }
    /// <summary>Constructs a temporary artwork draw-list DTO for a station pointer; runtime drawing calculates cells directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(pointer, runs);
        return true;
    }

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the physical words. Only complete native lists have IDs.
    /// </summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<StationDraw>(pointer, "station draw list") switch
        {
            StationDraw.MapFrame0 => "map-frame-0",
            StationDraw.MapFrame1 => "map-frame-1",
            StationDraw.MapFrame2 => "map-frame-2",
            StationDraw.EnergyFrame0 => "energy-frame-0",
            StationDraw.EnergyFrame1 => "energy-frame-1",
            StationDraw.EnergyFrame2 => "energy-frame-2",
            StationDraw.MissileFrame0 => "missile-frame-0",
            StationDraw.MissileFrame1 => "missile-frame-1",
            StationDraw.MissileFrame2 => "missile-frame-2",
            StationDraw.SaveIdle => "save-idle",
            StationDraw.SaveActive => "save-active-a",
            StationDraw.SaveAlternate => "save-active-b",
            StationDraw.MapRightRetracted => "map-right-retracted",
            StationDraw.MapRightExtended => "map-right-extended",
            StationDraw.MapLeftRetracted => "map-left-retracted",
            StationDraw.MapLeftExtended => "map-left-extended",
            StationDraw.ResourceRightRetracted => "resource-right-retracted",
            StationDraw.ResourceRightExtended => "resource-right-extended",
            StationDraw.ResourceLeftRetracted => "resource-left-retracted",
            StationDraw.ResourceLeftExtended => "resource-left-extended",
            _ => throw new InvalidOperationException($"Undefined station draw list ${pointer:X4}."),
        };

    /// <summary>Finds a station draw list by its stable artwork identity rather than native pointer.</summary>
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
