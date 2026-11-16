namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One Ki-Hunter occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterEntity(KiHunterBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (KiHunterPartDefinitions.Parts[Place.DefinitionId])
        {
            case KiHunterPart.KihunterGreen or KiHunterPart.KihunterYellow or KiHunterPart.KihunterRed:
                species.InitializeKiHunter(Place);
                break;
            case KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterYellowWings or KiHunterPart.KihunterRedWings:
                species.InitializeKiHunterWings(Place);
                break;
            default:
                throw new InvalidOperationException($"KiHunter entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceKiHunterTypedAnimation(Place, context);

    /// <summary>The Ki-Hunter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.KiHunterShotReaction : base.ShotReaction;

    /// <summary>The Ki-Hunter's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.KiHunterBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Ki-Hunter's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
