namespace SuperMetroid.Core.Rooms;

/// <summary>Typed dispatch identities for every room-main and room-setup behaviour retail room states use.</summary>
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
