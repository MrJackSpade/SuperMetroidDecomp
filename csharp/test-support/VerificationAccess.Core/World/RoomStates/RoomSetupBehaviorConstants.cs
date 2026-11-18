namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Cartridge values and helpers of <see cref="RoomSetupBehavior"/> that only verification reads.</summary>
internal static class RoomSetupBehaviorConstants
{
    /// <summary>Every room-setup behaviour the room states author, <see cref="RoomSetupBehavior.None"/> first.</summary>
    public static IReadOnlyList<RoomSetupBehavior> All { get; } =
    [
        RoomSetupBehavior.None, RoomSetupBehavior.OpenRescuedAnimalsPassageAndQuake, RoomSetupBehavior.AutoDestroyWallDuringEscape, RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape,
        RoomSetupBehavior.LandSkyWithLandingSiteQuake, RoomSetupBehavior.SpawnScrollingSkyBands, RoomSetupBehavior.RunStatueUnlockingAnimations,
        RoomSetupBehavior.HoldPrePhantoonProjectilePlace, RoomSetupBehavior.SpawnShaktoolController, RoomSetupBehavior.SetCollectedMap,
        RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking, RoomSetupBehavior.SetLightHorizontalRoomShaking, RoomSetupBehavior.SetMediumHorizontalRoomShaking,
        RoomSetupBehavior.MediumHorizontalShakingWithRisingAcid, RoomSetupBehavior.SpawnCeresHaze,
    ];

    /// <summary>
    /// The setups more than one bank-$8F routine decodes to, with their routines' count (#2485): the land and the ocean
    /// skies' band setups, and the Ceres haze setups of the ordinary rooms, the elevator shaft after Ridley's escape and
    /// Ridley's arena. Every other acting setup is one routine.
    /// </summary>
    public static IReadOnlyDictionary<RoomSetupBehavior, int> SharedRoutineCounts { get; } = new Dictionary<RoomSetupBehavior, int>
    {
        [RoomSetupBehavior.SpawnScrollingSkyBands] = 2,
        [RoomSetupBehavior.SpawnCeresHaze] = 3,
    };
}
