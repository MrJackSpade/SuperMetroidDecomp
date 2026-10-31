namespace SuperMetroid.Core.Progression;

/// <summary>
/// The progress a save keeps (#1475): the story events that have happened, each area's defeated bosses, the Chozo orbs
/// broken, the item locations collected, the doors opened for good, each area's used save stations and elevators, the
/// area maps acquired and each area's explored map cells. Each is a set keyed by its own identity. Lifetime: the
/// session's <see cref="SharedGameState"/> owns the live progress; every save slot and save snapshot owns a detached copy.
/// </summary>
public sealed class PersistentProgress
{
    /// <summary>The story events the game numbers, 0 through 63; <see cref="EventNumber"/> names the ones the retail game uses.</summary>
    public const int EventCount = 64;

    /// <summary>The Chozo orbs, item locations and doors room data numbers, 0 through 511 each.</summary>
    public const int RoomFlagCount = 512;

    /// <summary>The save stations and elevators each area numbers, 0 through 15.</summary>
    public const int StationsPerArea = 16;

    /// <summary>The cells of one area's 64-by-32 map.</summary>
    private const int MapCellsPerArea = AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles;

    /// <summary>Each area's defeated bosses, by area index.</summary>
    private readonly BossBits[] _defeatedBosses = new BossBits[AreaIds.RetailCount];

    /// <summary>Each area's used save stations and elevators, by area index.</summary>
    private readonly NumberedFlagSet[] _usedStations = Areas(StationsPerArea);

    /// <summary>Whether each area's map has been acquired, by area index.</summary>
    private readonly bool[] _acquiredMaps = new bool[AreaIds.RetailCount];

    /// <summary>Each area's explored map cells, by area index, each cell numbered in row-major order.</summary>
    private readonly NumberedFlagSet[] _explored = Areas(MapCellsPerArea);

    /// <summary>The story events that have happened, by event number.</summary>
    public NumberedFlagSet Events { get; } = new(EventCount);

    /// <summary>The Chozo orbs broken open, by room-data number; independent of whether the item they exposed was collected.</summary>
    public NumberedFlagSet ChozoOrbs { get; } = new(RoomFlagCount);

    /// <summary>The item locations whose pickup has been collected, by room-data number.</summary>
    public NumberedFlagSet CollectedItems { get; } = new(RoomFlagCount);

    /// <summary>The coloured doors opened for good, by room-data number.</summary>
    public NumberedFlagSet OpenedDoors { get; } = new(RoomFlagCount);

    /// <summary>Whether <paramref name="eventNumber"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public bool HasEvent(EventNumber eventNumber) => Events.Contains(eventNumber.ToEventNumber());

    /// <summary>Records that <paramref name="eventNumber"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public void SetEvent(EventNumber eventNumber) => Events.Add(eventNumber.ToEventNumber());

    /// <summary>Records that <paramref name="eventNumber"/> has not happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public void ClearEvent(EventNumber eventNumber) => Events.Remove(eventNumber.ToEventNumber());

    /// <summary>The bosses defeated in <paramref name="area"/>.</summary>
    public BossBits DefeatedBosses(AreaId area) => _defeatedBosses[AreaIds.ToIndex(area)];

    /// <summary>Replaces the bosses defeated in <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bosses"/> holds a flag the game does not name.</exception>
    public void SetDefeatedBosses(AreaId area, BossBits bosses)
    {
        ProvenBossBits.Validate(bosses, $"{area}'s defeated bosses");
        _defeatedBosses[AreaIds.ToIndex(area)] = bosses;
    }

    /// <summary>The save stations and elevators used in <paramref name="area"/>, by station number.</summary>
    public NumberedFlagSet UsedStations(AreaId area) => _usedStations[AreaIds.ToIndex(area)];

    /// <summary>Whether <paramref name="area"/>'s map has been acquired.</summary>
    public bool HasAreaMap(AreaId area) => _acquiredMaps[AreaIds.ToIndex(area)];

    /// <summary>Records whether <paramref name="area"/>'s map has been acquired.</summary>
    /// <param name="area">The area.</param>
    /// <param name="acquired">Whether its map has been acquired.</param>
    public void SetAreaMap(AreaId area, bool acquired) => _acquiredMaps[AreaIds.ToIndex(area)] = acquired;

    /// <summary>Whether <paramref name="cell"/> of <paramref name="area"/>'s map has been explored.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public bool IsExplored(AreaId area, Position16<MapTiles> cell) => _explored[AreaIds.ToIndex(area)].Contains(CellNumber(cell));

    /// <summary>Records whether <paramref name="cell"/> of <paramref name="area"/>'s map has been explored.</summary>
    /// <param name="area">The area.</param>
    /// <param name="cell">The map cell.</param>
    /// <param name="explored">Whether the cell has been explored.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public void SetExplored(AreaId area, Position16<MapTiles> cell, bool explored) =>
        _explored[AreaIds.ToIndex(area)].Set(CellNumber(cell), explored);

    /// <summary>The explored cells of <paramref name="area"/>'s map, in row-major order.</summary>
    public IEnumerable<Position16<MapTiles>> ExploredCells(AreaId area) =>
        _explored[AreaIds.ToIndex(area)].Members.Select(AreaMapLayout.Size.CellAtRowMajorIndex);

    /// <summary>
    /// Applies SpaceTime's progression overwrite: every story event, boss, orb, item, door, station and map it decides
    /// takes the state it gives, and everything else keeps its state.
    /// </summary>
    /// <exception cref="ArgumentException">The overwrite decides more flags than a range numbers.</exception>
    public void Overwrite(ProgressionOverwrite overwrite)
    {
        Ensure.NotNull(overwrite);
        Events.Overwrite(overwrite.Events);
        foreach (KeyValuePair<AreaId, BossBits> bosses in overwrite.AreaBosses)
            SetDefeatedBosses(bosses.Key, bosses.Value);
        ChozoOrbs.Overwrite(overwrite.ChozoOrbs);
        CollectedItems.Overwrite(overwrite.CollectedItems);
        OpenedDoors.Overwrite(overwrite.OpenedDoors);
        foreach (KeyValuePair<AreaId, FlagOverwrite> stations in overwrite.UsedStations)
            UsedStations(stations.Key).Overwrite(stations.Value);
        foreach (KeyValuePair<AreaId, bool> map in overwrite.AreaMaps)
            SetAreaMap(map.Key, map.Value);
    }

    /// <summary>Makes this progress match <paramref name="source"/>.</summary>
    public void CopyFrom(PersistentProgress source)
    {
        Ensure.NotNull(source);
        Events.CopyFrom(source.Events);
        source._defeatedBosses.CopyTo(_defeatedBosses, 0);
        ChozoOrbs.CopyFrom(source.ChozoOrbs);
        CollectedItems.CopyFrom(source.CollectedItems);
        OpenedDoors.CopyFrom(source.OpenedDoors);
        for (int area = 0; area < AreaIds.RetailCount; area++)
        {
            _usedStations[area].CopyFrom(source._usedStations[area]);
            _explored[area].CopyFrom(source._explored[area]);
        }
        source._acquiredMaps.CopyTo(_acquiredMaps, 0);
    }

    /// <summary>A detached copy of this progress.</summary>
    public PersistentProgress Copy()
    {
        var copy = new PersistentProgress();
        copy.CopyFrom(this);
        return copy;
    }

    /// <summary>One range of <paramref name="capacity"/> flags for each area.</summary>
    private static NumberedFlagSet[] Areas(int capacity) =>
        [.. Enumerable.Range(0, AreaIds.RetailCount).Select(_ => new NumberedFlagSet(capacity))];

    /// <summary>The number of a map cell, rejecting a cell outside the area map.</summary>
    private static int CellNumber(Position16<MapTiles> cell) => AreaMapLayout.Size.Contains(cell)
        ? AreaMapLayout.Size.RowMajorIndex(cell)
        : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The cell lies outside the 64-by-32 area map.");
}
