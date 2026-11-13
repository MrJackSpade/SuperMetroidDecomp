namespace SuperMetroid.Core.Progression;

/// <summary>Verification conveniences for building <see cref="PersistentProgress"/> fixtures.</summary>
internal static class PersistentProgressAccess
{
    /// <summary>Compares numbered flags.</summary>
    extension(NumberedFlagSet self)
    {
        /// <summary>Whether <paramref name="other"/> numbers the same range with the same flags on.</summary>
        internal bool SetEquals(NumberedFlagSet other) =>
            self.Capacity == other.Capacity && self.Members.SequenceEqual(other.Members);
    }

    /// <summary>Whether two progress sets hold the same members.</summary>
    extension<TMember>(ProgressSet<TMember> self)
        where TMember : notnull
    {
        /// <summary>Whether <paramref name="other"/> holds the same members, in the same order.</summary>
        internal bool SetEquals(ProgressSet<TMember> other) => self.Members.SequenceEqual(other.Members);
    }

    /// <summary>Fills and compares whole sets of progress for fixtures.</summary>
    extension(PersistentProgress self)
    {
        /// <summary>Whether <paramref name="other"/> holds the same progress.</summary>
        internal bool SameAs(PersistentProgress other) =>
            self.Events.Members.SequenceEqual(other.Events.Members) && self.ZebetitesDestroyed == other.ZebetitesDestroyed &&
            self.CollectedItems.Members.SequenceEqual(other.CollectedItems.Members) && self.OpenedDoors.Members.SequenceEqual(other.OpenedDoors.Members) &&
            self.UsedStations.Members.SequenceEqual(other.UsedStations.Members) &&
            AreaIds.All.All(area => self.BossesDefeatedIn(area) == other.BossesDefeatedIn(area) && self.HasAreaMap(area) == other.HasAreaMap(area) &&
                self.ExploredCells(area).SequenceEqual(other.ExploredCells(area)));

        /// <summary>Records every cell of every area's map as explored.</summary>
        internal void ExploreEveryCell()
        {
            foreach (AreaId area in AreaIds.All)
            {
                for (int y = 0; y < AreaMapSize.HeightInTiles; y++)
                {
                    for (int x = 0; x < AreaMapSize.WidthInTiles; x++)
                        self.SetExplored(area, new Position16<MapTiles>(checked((ushort)x), checked((ushort)y)), explored: true);
                }
            }
        }
    }
}
