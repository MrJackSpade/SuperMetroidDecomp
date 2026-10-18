namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The installed program frames each artwork schema generation holds, sorted by bank-$86 operand: the first
/// program-frame schema's frames and every generation's additions through that generation.
/// </summary>
internal static class EnemyProjectilePresentationFrameSets
{
    /// <summary>Each generation's additions, in publishing order; the sets below are built from it.</summary>
    private static readonly IReadOnlyList<EnemyProjectilePresentationFrameDefinition>[] Additions =
    [
        EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoEggAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreTorizoEffectsAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreGenericEnemyDeathAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreEnvironmentAndAttackAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreMotherBrainAndStatueAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreWorkRobotAdditions,
        EnemyProjectilePresentationFrameDefinitions.PrePolypRockAdditions,
        EnemyProjectilePresentationFrameDefinitions.CurrentAdditions,
    ];

    /// <summary>Base schema set before Golden Torizo projectile art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoFrames = Through(1);
    /// <summary>Schema set before Golden Torizo egg art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGoldenTorizoEggFrames = Through(2);
    /// <summary>Schema set before the broader Torizo effect family was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreTorizoEffectsFrames = Through(3);
    /// <summary>Schema set before generic enemy pickup and death art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreGenericEnemyDeathFrames = Through(4);
    /// <summary>Schema set before environmental and attack effect art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreEnvironmentAndAttackFrames = Through(5);
    /// <summary>Schema set before Mother Brain and statue projectile art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreMotherBrainAndStatueFrames = Through(6);
    /// <summary>Schema set before Work Robot laser art was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PreWorkRobotFrames = Through(7);
    /// <summary>Schema set before the Polyp Rock projectile frame was added.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] PrePolypRockFrames = Through(8);
    /// <summary>Current sorted program-frame set used by production catalog lookup.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] Frames = Through(9);

    /// <summary>Current projectile program frames sorted by bank-$86 operand.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> All => Frames;
    /// <summary>Schema set before the Polyp Rock projectile frame was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PrePolypRock => PrePolypRockFrames;
    /// <summary>Base schema set before Golden Torizo projectile art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizo => PreGoldenTorizoFrames;
    /// <summary>Schema set before Golden Torizo egg art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGoldenTorizoEgg => PreGoldenTorizoEggFrames;
    /// <summary>Schema set before the broader Torizo effect family was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreTorizoEffects => PreTorizoEffectsFrames;
    /// <summary>Schema set before generic enemy pickup and death art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreGenericEnemyDeath => PreGenericEnemyDeathFrames;
    /// <summary>Schema set before environmental and attack effect art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreEnvironmentAndAttack => PreEnvironmentAndAttackFrames;
    /// <summary>Schema set before Mother Brain and statue projectile art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreMotherBrainAndStatue => PreMotherBrainAndStatueFrames;
    /// <summary>Schema set before Work Robot laser art was added.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> PreWorkRobot => PreWorkRobotFrames;

    /// <summary>Checks whether a bank-$86 operand names a program frame of the current published artwork catalog.</summary>
    internal static bool Contains(ushort operandAddress) =>
        Array.BinarySearch(Frames, new EnemyProjectilePresentationFrameDefinition(operandAddress, string.Empty), AddressComparer.Instance) >= 0;

    /// <summary>The first program-frame schema's frames and the first <paramref name="generations"/> generations' additions, by operand.</summary>
    private static EnemyProjectilePresentationFrameDefinition[] Through(int generations) =>
        [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames.Concat(Additions.Take(generations).SelectMany(added => added))
            .OrderBy(frame => frame.OperandAddress)];

    /// <summary>Orders frame definitions by physical bank-$86 operand address for binary search.</summary>
    private sealed class AddressComparer : IComparer<EnemyProjectilePresentationFrameDefinition>
    {
        /// <summary>Shared stateless address comparer for current-catalog lookups.</summary>
        internal static readonly AddressComparer Instance = new();
        /// <summary>Compares only operand addresses; family names do not affect lookup identity.</summary>
        public int Compare(EnemyProjectilePresentationFrameDefinition left, EnemyProjectilePresentationFrameDefinition right) =>
            left.OperandAddress.CompareTo(right.OperandAddress);
    }
}
