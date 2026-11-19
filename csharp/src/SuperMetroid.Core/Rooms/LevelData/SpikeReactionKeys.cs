namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="SpikeReaction"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class SpikeReactionKeys
{
    /// <summary>Each SpikeReaction member's installed key.</summary>
    internal static InstalledKeys<SpikeReaction> Installed { get; } = new(new Dictionary<SpikeReaction, string>
    {
        [SpikeReaction.HeavySpike] = "HeavySpike",
        [SpikeReaction.LightSpike] = "LightSpike",
        [SpikeReaction.DamagingSpikeAir] = "DamagingSpikeAir",
        [SpikeReaction.TurretSpike] = "TurretSpike",
        [SpikeReaction.Harmless] = "Harmless",
        [SpikeReaction.XrayRevealed] = "XrayRevealed",
        [SpikeReaction.EnemyBreakable] = "EnemyBreakable",
        [SpikeReaction.HarmlessLetsGrappleThrough] = "HarmlessLetsGrappleThrough",
        [SpikeReaction.EnemyBreakableLetsGrappleThrough] = "EnemyBreakableLetsGrappleThrough",
    });

    /// <summary>The SpikeReaction members in the order the installation and a kind's reaction list name them (#2567), written out beside the keys so reordering the members never reorders them.</summary>
    internal static IReadOnlyList<SpikeReaction> ListedOrder { get; } =
    [
        SpikeReaction.HeavySpike,
        SpikeReaction.LightSpike,
        SpikeReaction.DamagingSpikeAir,
        SpikeReaction.TurretSpike,
        SpikeReaction.Harmless,
        SpikeReaction.XrayRevealed,
        SpikeReaction.EnemyBreakable,
        SpikeReaction.HarmlessLetsGrappleThrough,
        SpikeReaction.EnemyBreakableLetsGrappleThrough,
    ];
}
