namespace SuperMetroid.Core.Rooms;

/// <summary>Typed dispatch identities for every main/setup pointer used by retail room states.</summary>
public static class RoomCallbackDefinitions
{
    /// <summary>Determines whether the selected setup callback initializes the Ceres haze effect.</summary>
    /// <param name="callback">The room's resolved setup callback.</param>
    /// <returns>Whether room loading should enable Ceres haze for this callback.</returns>
    public static bool SpawnsCeresHaze(RoomSetupCallback callback) => callback is
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze or
        RoomSetupCallback.SpawnCeresHaze or
        RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
}
