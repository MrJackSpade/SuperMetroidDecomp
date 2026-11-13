namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Where a Yapping Maw holds Samus for each way it aims.</summary>
internal static class YappingMawHoldDefinitions
{
    /// <summary>
    /// Samus's offset from the maw's mouth for each aim: an equal-step diamond of sixteen pixels' Manhattan radius, each
    /// eighth of a turn exchanging eight pixels between the two components, opposite aims negating both.
    /// </summary>
    internal static readonly IReadOnlyDictionary<YappingMawAim, Offset16<RoomPixels>> HeldSamusOffsets =
        new Dictionary<YappingMawAim, Offset16<RoomPixels>>
        {
            [YappingMawAim.Up] = new(0, -16),
            [YappingMawAim.UpRight] = new(8, -8),
            [YappingMawAim.Right] = new(16, 0),
            [YappingMawAim.DownRight] = new(8, 8),
            [YappingMawAim.Down] = new(0, 16),
            [YappingMawAim.DownLeft] = new(-8, 8),
            [YappingMawAim.Left] = new(-16, 0),
            [YappingMawAim.UpLeft] = new(-8, -8),
        };
}
