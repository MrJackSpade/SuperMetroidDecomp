namespace SuperMetroid.Core.Rooms;

/// <summary>Typed dispatch identities for every main/setup pointer used by retail room states.</summary>
public static class RoomCallbackDefinitions
{
    /// <summary>The setup callbacks that initialize the Ceres haze effect when their room loads.</summary>
    public static IReadOnlySet<RoomSetupCallback> CeresHazeSetups { get; } = new HashSet<RoomSetupCallback>
    {
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze,
        RoomSetupCallback.SpawnCeresHaze,
        RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze,
    };
}
