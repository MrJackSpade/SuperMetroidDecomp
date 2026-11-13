using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One BombTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The BombTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BombTorizoEntity(BombTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization() => species.Movement.RunBombTorizoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => BombTorizoPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        BombTorizoPart.BombTorizo => species.RunBombTorizoMainAi,
        BombTorizoPart.GoldenTorizo => species.RunGoldenTorizoMainAi,
        _ => throw new InvalidOperationException($"BombTorizo entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceBombTorizoTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return TorizoCombat.FindTorizoHitboxReaction(Slot, probe, out reaction);
    }

    /// <summary>Both Torizos' contact and shots meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The Torizo shot reactions, which normal bombs dispatch like the common one; only Golden Torizo owns its own.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction is EnemyReaction.BombTorizoShot or EnemyReaction.TorizoStandUpSitDownShot ||
        Slot.DefinitionId == EnemyDefinitionId.GoldenTorizo && reaction == EnemyReaction.GoldenTorizoShot;
}
