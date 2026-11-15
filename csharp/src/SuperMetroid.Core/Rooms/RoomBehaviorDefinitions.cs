namespace SuperMetroid.Core.Rooms;

/// <summary>The room-main and room-setup behaviours the retail room states use, each by name.</summary>
public static class RoomBehaviorDefinitions
{
    /// <summary>The setup behaviours that initialize the Ceres haze effect when their room loads.</summary>
    public static IReadOnlySet<RoomSetupBehavior> CeresHazeSetups { get; } = new HashSet<RoomSetupBehavior>
    {
        RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze,
        RoomSetupBehavior.SpawnCeresHaze,
        RoomSetupBehavior.SpawnCeresRidleyArenaHaze,
    };
}
