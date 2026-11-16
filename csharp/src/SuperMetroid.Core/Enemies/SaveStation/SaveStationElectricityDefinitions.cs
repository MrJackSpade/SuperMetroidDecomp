namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>Where a save station's electricity shows (#2156).</summary>
internal static class SaveStationElectricityDefinitions
{
    /// <summary>Where the electricity's origin lies from the save station's trigger block: one block right and two above.</summary>
    internal static Offset16<RoomBlocks> BlocksFromTrigger { get; } = new(1, -2);
}
