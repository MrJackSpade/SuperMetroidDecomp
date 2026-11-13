namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The coordinates the save station's trigger setup tests Samus against.</summary>
internal static class SaveStationTriggerGeometry
{
    /// <summary>The setup subtracts eight pixels from Samus X before comparing block columns.</summary>
    public const int HorizontalProbeOffset = 8;
}
