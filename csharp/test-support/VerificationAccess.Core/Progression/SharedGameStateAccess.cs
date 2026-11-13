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

    /// <summary>Reads and sets progress through the numbers room data and native fixtures give it.</summary>
    extension(SharedGameState self)
    {
        /// <summary>Whether the item location room data numbers <paramref name="number"/> has been collected.</summary>
        internal bool HasCollectedItemBit(int number) => self.HasCollectedItem(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(number));

        /// <summary>Records the item location room data numbers <paramref name="number"/> as collected.</summary>
        internal void SetCollectedItemBit(int number) => self.CollectItem(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(number));

        /// <summary>Whether the door room data numbers <paramref name="number"/> has been opened for good.</summary>
        internal bool HasOpenedDoorBit(int number) => self.HasOpenedDoor(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(number));

        /// <summary>Records the door room data numbers <paramref name="number"/> as opened for good.</summary>
        internal void SetOpenedDoorBit(int number) => self.OpenDoor(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(number));

        /// <summary>Records bosses as defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal void RecordBossesDefeated(int areaIndex, DefeatedBosses bosses) => self.RecordBossesDefeated(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), bosses);

        /// <summary>Forgets bosses as defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal void ForgetBossesDefeated(int areaIndex, DefeatedBosses bosses) => self.ForgetBossesDefeated(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), bosses);

        /// <summary>Whether any of the bosses is defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal bool HasDefeatedAnyOf(int areaIndex, DefeatedBosses bosses) => self.HasDefeatedAnyOf(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), bosses);

        /// <summary>The bosses defeated in the area with index <paramref name="areaIndex"/>.</summary>
        internal DefeatedBosses BossesDefeatedIn(int areaIndex) => self.BossesDefeatedIn(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex));

        /// <summary>Marks a map cell of the area with index <paramref name="areaIndex"/> explored.</summary>
        internal void MarkExploredMapTile(int areaIndex, Position16<MapTiles> cell) => self.MarkExploredMapTile(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), cell);

        /// <summary>Whether a map cell of the area with index <paramref name="areaIndex"/> is explored.</summary>
        internal bool IsMapTileExplored(int areaIndex, Position16<MapTiles> cell) => self.IsMapTileExplored(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), cell);

        /// <summary>Marks the station the area with index <paramref name="areaIndex"/> numbers <paramref name="station"/> used.</summary>
        internal void MarkSaveStationUsed(int areaIndex, int station) => self.MarkSaveStationUsed(SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(
            SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex), station, "A native fixture's used station"));

        /// <summary>Whether the area with index <paramref name="areaIndex"/> has its map.</summary>
        internal bool HasAreaMap(int areaIndex) => self.HasAreaMap(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex));

        /// <summary>Marks the map of the area with index <paramref name="areaIndex"/> acquired.</summary>
        internal void SetAreaMapAcquired(int areaIndex) => self.SetAreaMapAcquired(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(areaIndex));
    }
}
