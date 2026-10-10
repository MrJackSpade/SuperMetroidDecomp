using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The twenty bank-$84 draw lists selected by station animation programs.</summary>
internal enum StationDraw : ushort
{
    /// <summary>First map-station frame draw list at $84:9F25.</summary>
    MapFrame0 = 0x9f25,
    /// <summary>Second map-station frame draw list at $84:9F31.</summary>
    MapFrame1 = 0x9f31,
    /// <summary>Third map-station frame draw list at $84:9F3D.</summary>
    MapFrame2 = 0x9f3d,
    /// <summary>Retracted right-side map access draw at $84:9F49.</summary>
    MapRightRetracted = 0x9f49,
    /// <summary>Extended right-side map access draw at $84:9F55.</summary>
    MapRightExtended = 0x9f55,
    /// <summary>Retracted left-side map access draw at $84:9F5B.</summary>
    MapLeftRetracted = 0x9f5b,
    /// <summary>Extended left-side map access draw at $84:9F67.</summary>
    MapLeftExtended = 0x9f67,
    /// <summary>First energy-station frame draw list at $84:9F6D.</summary>
    EnergyFrame0 = 0x9f6d,
    /// <summary>Second energy-station frame draw list at $84:9F79.</summary>
    EnergyFrame1 = 0x9f79,
    /// <summary>Third energy-station frame draw list at $84:9F85.</summary>
    EnergyFrame2 = 0x9f85,
    /// <summary>First missile-station frame draw list at $84:9F91.</summary>
    MissileFrame0 = 0x9f91,
    /// <summary>Second missile-station frame draw list at $84:9F9D.</summary>
    MissileFrame1 = 0x9f9d,
    /// <summary>Third missile-station frame draw list at $84:9FA9.</summary>
    MissileFrame2 = 0x9fa9,
    /// <summary>Retracted right-side resource access draw at $84:9FB5.</summary>
    ResourceRightRetracted = 0x9fb5,
    /// <summary>Extended right-side resource access draw at $84:9FBB.</summary>
    ResourceRightExtended = 0x9fbb,
    /// <summary>Retracted left-side resource access draw at $84:9FC1.</summary>
    ResourceLeftRetracted = 0x9fc1,
    /// <summary>Extended left-side resource access draw at $84:9FC7.</summary>
    ResourceLeftExtended = 0x9fc7,
    /// <summary>Save-pod idle draw list at $84:9A3F.</summary>
    SaveIdle = 0x9a3f,
    /// <summary>First active save-pod draw list at $84:9A9F.</summary>
    SaveActive = 0x9a9f,
    /// <summary>Second active save-pod draw list at $84:9A6F.</summary>
    SaveAlternate = 0x9a6f,
}

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

    internal enum LayoutKind { Map, Energy, Missile, Save, MapAccess, ResourceAccess }

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
        internal int RunCount => Kind switch
        {
            LayoutKind.Save => 6,
            LayoutKind.MapAccess => Extended ? 1 : 2,
            LayoutKind.ResourceAccess => 1,
            LayoutKind.Map or LayoutKind.Energy or LayoutKind.Missile => 2,
            _ => throw new InvalidOperationException($"Undefined LayoutKind {Kind}."),
        };
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        internal int WordCount(int run) { CheckRun(run); return Kind == LayoutKind.Save ? 2 : 1; }
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            if (run != 0) return 0;
            return Kind == LayoutKind.Map ? (sbyte)-1 : Kind == LayoutKind.MapAccess && !Extended
                ? Left ? (sbyte)3 : (sbyte)-3 : (sbyte)0;
        }
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return Kind == LayoutKind.Save ? run < 5 ? (sbyte)(-run - 1) : (sbyte)0
                : Kind is LayoutKind.Energy or LayoutKind.Missile && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
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
    // Temporary artwork DTOs; runtime draws calculate cells directly.
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
