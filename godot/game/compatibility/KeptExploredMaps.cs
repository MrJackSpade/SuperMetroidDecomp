using Engine.Actors;
using Engine.Core;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// The explored maps the cartridge keeps for play to read, apart from what the game shows as explored: one live map,
/// marked where Samus goes whatever area she is in, and a copy for each area. A door between areas stores the live
/// map as the copy of the area left and loads it from the copy of the area entered; a save stores it as the copy of
/// the area saved in. A file chosen sets the Zebes areas' copies from what it saved, clears Ceres's and loads the
/// live map from its own area's; loading a game sets the copies so again, and the landing on Zebes after Ceres
/// loads the live map from its area's. A new file clears the copies, a cleared file and the game started again
/// clear them and the live map, and a demo clears the Zebes areas' copies. Nothing else changes them, so the live
/// map carries what one game or demo explored into the next.
/// </summary>
/// <remarks>
/// The cartridge reads the copies only where a scroll check runs past a room's fifty scroll cells
/// (<see cref="KeptScrollCells"/>), which is how they decide where the view may go.
/// </remarks>
public sealed class KeptExploredMaps : ISessionService
{
    /// <summary>The folder of the areas' maps, which say which cells explore the cell above with them.</summary>
    private const string AreaFolder = "areas";

    /// <summary>The live map's explored cells.</summary>
    private readonly HashSet<(int Column, int Row)> _live = [];

    /// <summary>Each area's copy of the live map, by the area's name.</summary>
    private readonly Dictionary<string, HashSet<(int Column, int Row)>> _copies =
        Data.Areas.All.ToDictionary(area => area, _ => new HashSet<(int Column, int Row)>(), StringComparer.Ordinal);

    /// <summary>The areas' maps read so far, by the area's name.</summary>
    private readonly Dictionary<string, Area> _maps = new(StringComparer.Ordinal);

    /// <summary>The session's kept explored maps.</summary>
    public static KeptExploredMaps Of(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Service<KeptExploredMaps>();
    }

    /// <summary>Whether the copy kept for <paramref name="area"/> has the cell at <paramref name="column"/>, <paramref name="row"/> explored.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cartridge keeps no copy for the area.</exception>
    public bool IsExploredInCopy(string area, int column, int row) => CopyOf(area).Contains((column, row));

    /// <summary>Marks a cell explored on the live map, as whatever explores a cell of the map does.</summary>
    public void MarkLive(int column, int row) => _live.Add((column, row));

    /// <summary>A door that exchanges maps: the live map is stored as <paramref name="left"/>'s copy, then loaded from <paramref name="entered"/>'s.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cartridge keeps no copy for one of the areas.</exception>
    public void PassDoor(string left, string entered)
    {
        Store(left);
        LoadLive(entered);
    }

    /// <summary>The game is saved in <paramref name="area"/>: the live map is stored as its copy.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cartridge keeps no copy for the area.</exception>
    public void Store(string area)
    {
        HashSet<(int Column, int Row)> copy = CopyOf(area);
        copy.Clear();
        copy.UnionWith(_live);
    }

    /// <summary>The live map becomes what <paramref name="area"/>'s copy holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cartridge keeps no copy for the area.</exception>
    public void LoadLive(string area)
    {
        HashSet<(int Column, int Row)> copy = CopyOf(area);
        _live.Clear();
        _live.UnionWith(copy);
    }

    /// <summary>
    /// Sets the copies from the file <paramref name="save"/>, as loading it does: each Zebes area's holds what the
    /// file explored there, and Ceres's is cleared. The live map is not changed.
    /// </summary>
    public void LoadFile(GameSession session, SaveData save)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(save);
        Progress saved = save.Remembered();
        foreach ((string area, HashSet<(int Column, int Row)> copy) in _copies)
        {
            copy.Clear();
            if (area == Data.Areas.Ceres)
                continue;
            foreach ((_, int column, int row) in saved.Explored.Where(cell => cell.Area == area))
                copy.Add((column, row));
        }
    }

    /// <summary>Clears every area's copy, as a new file does. The live map is not changed.</summary>
    public void ClearCopies()
    {
        foreach (HashSet<(int Column, int Row)> copy in _copies.Values)
            copy.Clear();
    }

    /// <summary>Clears every area's copy and the live map, as a cleared file and the game started again do.</summary>
    public void ClearAll()
    {
        ClearCopies();
        _live.Clear();
    }

    /// <summary>Clears every Zebes area's copy, as a demo does as it is loaded. The live map is not changed.</summary>
    public void ClearZebesCopies()
    {
        foreach ((string area, HashSet<(int Column, int Row)> copy) in _copies)
        {
            if (area != Data.Areas.Ceres)
                copy.Clear();
        }
    }

    /// <summary>A death or a load clears nothing of it: a file loaded sets the copies itself, and the live map carries over.</summary>
    public void BeginAgain()
    {
    }

    /// <summary>
    /// Marks the live map where Samus is once the view has followed her, but while she dies. On an update the game is
    /// saved, the live map is then stored as the area's copy.
    /// </summary>
    public void Step(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Player.IsDying)
            MarkWhereSamusIs(session);
        if (session.SaveRequested is not null)
            Store(session.Player.Room.Area);
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;

    /// <summary>
    /// Marks the live map where Samus is: the cell of the room's map her screen is, and with it the cell above where
    /// that is the low floor of a sloped hallway. Nothing is marked while the room's map is hidden or while she
    /// stands outside the room.
    /// </summary>
    public void MarkWhereSamusIs(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Player samus = session.Player;
        Room room = samus.Room;
        if (room.Area.Length == 0 || !room.MapShown)
            return;
        int x = samus.Position.X.Whole, y = samus.Position.Y.Whole;
        if (x < 0 || y < 0 || x >= room.Terrain.WidthInPixels || y >= room.Terrain.HeightInPixels)
            return;

        // No room runs past the edges of its map, so a cell of the room is a cell of the map.
        int column = room.MapCell.X + x / room.MapCellSize.X;
        int row = room.MapCell.Y + y / room.MapCellSize.Y;
        MarkLive(column, row);
        if (row > 0 && MapOf(session, room.Area).CellAt(column, row) is { ExploresAbove: true })
            MarkLive(column, row - 1);
    }

    private Area MapOf(GameSession session, string area)
    {
        if (!_maps.TryGetValue(area, out Area? map))
            _maps[area] = map = Area.Read(session.Content.ReadData($"{AreaFolder}/{area}.json"));
        return map;
    }

    private HashSet<(int Column, int Row)> CopyOf(string area) =>
        _copies.TryGetValue(area, out HashSet<(int Column, int Row)>? copy)
            ? copy
            : throw new ArgumentOutOfRangeException(nameof(area), area, "The cartridge keeps no explored map for this area.");
}
