namespace SuperMetroid.Core.Progression;

/// <summary>Verification conveniences for building <see cref="PersistentProgress"/> fixtures.</summary>
internal static class PersistentProgressAccess
{
    /// <summary>Turns on every flag numbered in <paramref name="flags"/>.</summary>
    internal static void FillAll(NumberedFlagSet flags)
    {
        for (int number = 0; number < flags.Capacity; number++)
            flags.Add(number);
    }

    /// <summary>Compares numbered flags.</summary>
    extension(NumberedFlagSet self)
    {
        /// <summary>Whether <paramref name="other"/> numbers the same range with the same flags on.</summary>
        internal bool SetEquals(NumberedFlagSet other) =>
            self.Capacity == other.Capacity && self.Members.SequenceEqual(other.Members);
    }

    /// <summary>Fills and compares whole sets of progress for fixtures.</summary>
    extension(PersistentProgress self)
    {
        /// <summary>Whether <paramref name="other"/> holds the same progress.</summary>
        internal bool SameAs(PersistentProgress other) =>
            self.Events.SetEquals(other.Events) && self.ChozoOrbs.SetEquals(other.ChozoOrbs) &&
            self.CollectedItems.SetEquals(other.CollectedItems) && self.OpenedDoors.SetEquals(other.OpenedDoors) &&
            Enum.GetValues<AreaId>().All(area => self.DefeatedBosses(area) == other.DefeatedBosses(area) &&
                self.UsedStations(area).SetEquals(other.UsedStations(area)) && self.HasAreaMap(area) == other.HasAreaMap(area) &&
                self.ExploredCells(area).SequenceEqual(other.ExploredCells(area)));

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
