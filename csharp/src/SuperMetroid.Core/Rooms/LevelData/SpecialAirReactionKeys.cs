namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="SpecialAirReaction"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SpecialAirReactionKeys
{
    /// <summary>Each SpecialAirReaction member's installed key.</summary>
    internal static InstalledKeys<SpecialAirReaction> Installed { get; } = new(new Dictionary<SpecialAirReaction, string>
    {
        [SpecialAirReaction.PassesThrough] = "PassesThrough",
        [SpecialAirReaction.RightwardConveyorOnFoot] = "RightwardConveyorOnFoot",
        [SpecialAirReaction.LeftwardConveyorOnFoot] = "LeftwardConveyorOnFoot",
        [SpecialAirReaction.RightwardConveyor] = "RightwardConveyor",
        [SpecialAirReaction.LeftwardConveyor] = "LeftwardConveyor",
        [SpecialAirReaction.ScrollTrigger] = "ScrollTrigger",
        [SpecialAirReaction.FloorPlant] = "FloorPlant",
        [SpecialAirReaction.CeilingPlant] = "CeilingPlant",
        [SpecialAirReaction.QuicksandSurface] = "QuicksandSurface",
        [SpecialAirReaction.SubmergingQuicksand] = "SubmergingQuicksand",
        [SpecialAirReaction.SlowSandfall] = "SlowSandfall",
        [SpecialAirReaction.FastSandfall] = "FastSandfall",
        [SpecialAirReaction.SlowRespawningSpeedBlock] = "SlowRespawningSpeedBlock",
        [SpecialAirReaction.SlowPermanentSpeedBlock] = "SlowPermanentSpeedBlock",
        [SpecialAirReaction.DachoraRespawningSpeedBlock] = "DachoraRespawningSpeedBlock",
        [SpecialAirReaction.PermanentSpeedBlock] = "PermanentSpeedBlock",
        [SpecialAirReaction.WreckedShipChozoHand] = "WreckedShipChozoHand",
        [SpecialAirReaction.LowerNorfairChozoHand] = "LowerNorfairChozoHand",
    });

    /// <summary>The SpecialAirReaction members in the order the installation and a kind's reaction list name them (#2567), written out beside the keys so reordering the members never reorders them.</summary>
    internal static IReadOnlyList<SpecialAirReaction> ListedOrder { get; } =
    [
        SpecialAirReaction.PassesThrough,
        SpecialAirReaction.RightwardConveyorOnFoot,
        SpecialAirReaction.LeftwardConveyorOnFoot,
        SpecialAirReaction.RightwardConveyor,
        SpecialAirReaction.LeftwardConveyor,
        SpecialAirReaction.ScrollTrigger,
        SpecialAirReaction.FloorPlant,
        SpecialAirReaction.CeilingPlant,
        SpecialAirReaction.QuicksandSurface,
        SpecialAirReaction.SubmergingQuicksand,
        SpecialAirReaction.SlowSandfall,
        SpecialAirReaction.FastSandfall,
        SpecialAirReaction.SlowRespawningSpeedBlock,
        SpecialAirReaction.SlowPermanentSpeedBlock,
        SpecialAirReaction.DachoraRespawningSpeedBlock,
        SpecialAirReaction.PermanentSpeedBlock,
        SpecialAirReaction.WreckedShipChozoHand,
        SpecialAirReaction.LowerNorfairChozoHand,
    ];
}
