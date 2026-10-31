namespace SuperMetroid.Core.Progression;

/// <summary>Verification conveniences for building <see cref="PersistentProgress"/> fixtures.</summary>
internal static class PersistentProgressAccess
{
    /// <summary>Turns on every flag numbered in <paramref name="flags"/>.</summary>
    internal static void FillAll(NumberedFlags flags)
    {
        for (int number = 0; number < flags.Capacity; number++)
            flags.Add(number);
    }

    /// <summary>Fills whole sets of progress for fixtures.</summary>
    extension(PersistentProgress self)
    {
        /// <summary>Records every cell of every area's map as explored.</summary>
        internal void ExploreEveryCell()
        {
            for (int area = 0; area < AreaIds.RetailCount; area++)
            {
                for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
                {
                    for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                        self.SetExplored(AreaIds.At(area), new Position16<MapTiles>(checked((ushort)x), checked((ushort)y)), explored: true);
                }
            }
        }
    }
}
