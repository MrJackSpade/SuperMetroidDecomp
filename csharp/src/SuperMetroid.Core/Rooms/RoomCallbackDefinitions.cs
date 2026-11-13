namespace SuperMetroid.Core.Rooms;

/// <summary>Typed dispatch identities for every room-main and room-setup behaviour retail room states use.</summary>
public static class RoomCallbackDefinitions
{
    /// <summary>The setup behaviours that initialize the Ceres haze effect when their room loads.</summary>
    public static IReadOnlySet<RoomSetupCallback> CeresHazeSetups { get; } = new HashSet<RoomSetupCallback>
    {
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze,
        RoomSetupCallback.SpawnCeresHaze,
        RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze,
    };
}
