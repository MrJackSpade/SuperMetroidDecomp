namespace SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomPlmSpeedBoosterVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSpeedBoosterVisualCatalogAccess
{
    /// <summary>Builds bomb-reveal speed-block art from the first physical draw word's visual bits.</summary>
    extension(RoomPlmSpeedBoosterVisualCatalog)
    {
        /// <summary>Builds the bomb-reveal visual from the first physical speed-block draw word.</summary>
        internal static RoomPlmSpeedBoosterVisualCatalog Stock() => new(
            [new RoomPlmSpeedBoosterVisualEntry(
                SpeedBoosterBlockPlmDrawDefinitions.BombRevealVisualId,
                [new RoomLevelWord(SpeedBoosterBlockPlmDrawDefinitions.BombReveal
                    .Runs.Span[0].LevelWords.Span[0]).VisualWord])]);
    }
}
