namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One Ki-Hunter occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterEntity(KiHunterBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (KiHunterPartDefinitions.Parts[Slot.DefinitionId])
        {
            case KiHunterPart.KihunterGreen or KiHunterPart.KihunterYellow or KiHunterPart.KihunterRed:
                species.InitializeKiHunter(Slot);
                break;
            case KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterYellowWings or KiHunterPart.KihunterRedWings:
                species.InitializeKiHunterWings(Slot);
                break;
            default:
                throw new InvalidOperationException($"KiHunter entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceKiHunterTypedAnimation(Slot, context);

    /// <summary>The Ki-Hunter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.KiHunterShotReaction : base.ShotReaction;

    /// <summary>The Ki-Hunter's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.KiHunterBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Ki-Hunter's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
