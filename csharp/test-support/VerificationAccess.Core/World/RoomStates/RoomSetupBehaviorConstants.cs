namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Cartridge values and helpers of <see cref="RoomSetupBehavior"/> that only verification reads.</summary>
internal static class RoomSetupBehaviorConstants
{
    /// <summary>Every room-setup behaviour the room states author, <see cref="RoomSetupBehavior.None"/> first.</summary>
    public static IReadOnlyList<RoomSetupBehavior> All { get; } =
    [
        RoomSetupBehavior.None, RoomSetupBehavior.OpenRescuedAnimalsPassageAndQuake, RoomSetupBehavior.AutoDestroyWallDuringEscape, RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape,
        RoomSetupBehavior.LandSkyWithLandingSiteQuake, RoomSetupBehavior.ScrollingSkyLand, RoomSetupBehavior.ScrollingSkyOcean, RoomSetupBehavior.RunStatueUnlockingAnimations,
        RoomSetupBehavior.HoldPrePhantoonProjectilePlace, RoomSetupBehavior.SpawnShaktoolController, RoomSetupBehavior.SetCollectedMap,
        RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking, RoomSetupBehavior.SetLightHorizontalRoomShaking, RoomSetupBehavior.SetMediumHorizontalRoomShaking,
        RoomSetupBehavior.MediumHorizontalShakingWithRisingAcid, RoomSetupBehavior.SealCeresDoorAndSpawnHaze, RoomSetupBehavior.SpawnCeresHaze, RoomSetupBehavior.SpawnCeresRidleyArenaHaze,
    ];
}
