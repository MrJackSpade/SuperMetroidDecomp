namespace SuperMetroid.Core.Progression;

/// <summary>
/// What the game's file and demo loads do to the explored maps it stores (<see cref="StoredExploredMaps{TArea}"/>,
/// #1500): the areas, in the order their maps are stored, and which of them a loaded file, a new file and a demo set.
/// </summary>
internal static class StoredExploredMapLoads
{
    /// <summary>The stored maps as power-on leaves them: every area's map unexplored, kept in the areas' order.</summary>
    internal static StoredExploredMaps<AreaId> PowerOn() =>
        new([.. AreaIds.All]);

    /// <summary>
    /// Loads a file's exploration: every Zebes area's stored map becomes the exploration the file keeps for it, and
    /// Ceres's is cleared. The live map is not changed.
    /// </summary>
    /// <param name="maps">The session's stored maps.</param>
    /// <param name="file">The progress the file holds.</param>
    internal static void LoadFile(StoredExploredMaps<AreaId> maps, PersistentProgress file)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(file);
        foreach (AreaId area in AreaIds.All)
        {
            if (AreaIds.IsZebes(area))
                maps.LoadStoredMap(area, file.ExploredCells(area));
            else
                maps.ClearStoredMap(area);
        }
    }

    /// <summary>Clears every area's stored map, as starting a new file does. The live map is not changed.</summary>
    /// <param name="maps">The session's stored maps.</param>
    internal static void ClearForNewFile(StoredExploredMaps<AreaId> maps)
    {
        Ensure.NotNull(maps);
        foreach (AreaId area in AreaIds.All)
            maps.ClearStoredMap(area);
    }

    /// <summary>
    /// Clears every area's stored map and the live map, as a soft reset does and as clearing a file at file select leaves
    /// them.
    /// </summary>
    /// <param name="maps">The session's stored maps.</param>
    internal static void ClearAll(StoredExploredMaps<AreaId> maps)
    {
        ClearForNewFile(maps);
        maps.ClearLiveMap();
    }

    /// <summary>Clears every Zebes area's stored map, as loading an attract demo does. The live map is not changed.</summary>
    /// <param name="maps">The session's stored maps.</param>
    internal static void ClearForDemo(StoredExploredMaps<AreaId> maps)
    {
        Ensure.NotNull(maps);
        foreach (AreaId area in AreaIds.Zebes)
            maps.ClearStoredMap(area);
    }
}
