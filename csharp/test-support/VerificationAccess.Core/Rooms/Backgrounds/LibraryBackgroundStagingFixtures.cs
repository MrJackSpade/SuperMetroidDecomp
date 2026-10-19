using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Owned staging for library-background fixtures that run a program outside a runtime.</summary>
internal static class LibraryBackgroundStagingFixtures
{
    /// <summary>A fresh tilemap staging and fresh enemy staging surfaces.</summary>
    internal static LibraryBackgroundStaging Fresh() => new(new Bg2TilemapStaging(), new EnemyStagingSurfaces());
}
