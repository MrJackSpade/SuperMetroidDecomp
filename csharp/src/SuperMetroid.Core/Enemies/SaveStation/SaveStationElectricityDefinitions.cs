namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>Where a save station's electricity shows (#2156) and where its band sweeps (#2557).</summary>
internal static class SaveStationElectricityDefinitions
{
    /// <summary>How far below the electricity's origin its band first shows on a sweep: 32 pixels above it.</summary>
    internal const int SweepStartPixelsBelow = -32;

    /// <summary>How far the band drops on each update of its sweep: 8 pixels.</summary>
    internal const int SweepStepPixels = 8;

    /// <summary>How many places the band shows on one sweep, one update each: 8, the last 24 pixels below the origin.</summary>
    internal const int SweepPlaces = 8;

    /// <summary>Where the electricity's origin lies from the save station's trigger block: one block right and two above.</summary>
    internal static Offset16<RoomBlocks> BlocksFromTrigger { get; } = new(1, -2);
}
