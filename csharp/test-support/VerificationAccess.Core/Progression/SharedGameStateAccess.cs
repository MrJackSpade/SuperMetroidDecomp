namespace SuperMetroid.Core.Progression;
/// <summary>Verification access to <see cref="SharedGameState"/> members production does not use.</summary>
internal static class SharedGameStateAccess
{
    /// <summary>Reproduces bank-$80's unsigned 16-by-16 multiply with a full 32-bit product.</summary>
    extension(SharedGameState)
    {
        /// <summary>
        /// Unsigned <c>16 x 16 -> 32</c> multiplication from <c>$80:82D6</c>.
        /// The widening casts must happen before multiplication or C# would discard the high
        /// word before returning it.
        /// </summary>
        internal static uint Multiply16By16(ushort left, ushort right) => (uint)left * right;
    }

    /// <summary>Reads and sets the broken Chozo orbs, which production carries only through saves.</summary>
    extension(SharedGameState self)
    {
        /// <summary>Whether the Chozo orb room data numbers <paramref name="orb"/> has been broken.</summary>
        internal bool HasRoomChozoBit(int orb) => self.Progress.ChozoOrbs.Contains(orb);

        /// <summary>Records the Chozo orb room data numbers <paramref name="orb"/> as broken.</summary>
        internal void SetRoomChozoBit(int orb) => self.Progress.ChozoOrbs.Add(orb);

        /// <summary>Records bosses as defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal void SetBossBits(int areaIndex, BossBits bits) => self.SetBossBits(AreaIds.At(areaIndex), bits);

        /// <summary>Forgets bosses as defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal void ClearBossBits(int areaIndex, BossBits bits) => self.ClearBossBits(AreaIds.At(areaIndex), bits);

        /// <summary>Whether any of the bosses is defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal bool HasAnyBossBits(int areaIndex, BossBits bits) => self.HasAnyBossBits(AreaIds.At(areaIndex), bits);

        /// <summary>The bosses defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal BossBits GetBossBits(int areaIndex) => self.GetBossBits(AreaIds.At(areaIndex));

        /// <summary>Marks a map cell of the area with index <paramref name="areaIndex"/> explored.</summary>
        internal void MarkExploredMapTile(int areaIndex, Position16<MapTiles> cell) => self.MarkExploredMapTile(AreaIds.At(areaIndex), cell);

        /// <summary>Whether a map cell of the area with index <paramref name="areaIndex"/> is explored.</summary>
        internal bool IsMapTileExplored(int areaIndex, Position16<MapTiles> cell) => self.IsMapTileExplored(AreaIds.At(areaIndex), cell);

        /// <summary>Marks a save station of the area with index <paramref name="areaIndex"/> used.</summary>
        internal void MarkSaveStationUsed(int areaIndex, int station) => self.MarkSaveStationUsed(AreaIds.At(areaIndex), station);

        /// <summary>Whether the area with index <paramref name="areaIndex"/> has its map.</summary>
        internal bool HasAreaMap(int areaIndex) => self.HasAreaMap(AreaIds.At(areaIndex));

        /// <summary>Marks the map of the area with index <paramref name="areaIndex"/> acquired.</summary>
        internal void SetAreaMapAcquired(int areaIndex) => self.SetAreaMapAcquired(AreaIds.At(areaIndex));
    }
}
