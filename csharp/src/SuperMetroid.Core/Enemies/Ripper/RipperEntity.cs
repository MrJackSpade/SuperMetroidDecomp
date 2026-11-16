namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (RipperPartDefinitions.Parts[Place.DefinitionId])
        {
            case RipperPart.GrappleRipper:
                RipperBehavior.InitializeGrappleRipper(Place);
                break;
            case RipperPart.JetRipper:
                RipperBehavior.InitializeJetRipper(Place);
                break;
            case RipperPart.Ripper:
                RipperBehavior.InitializeRipper(Place);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (RipperPartDefinitions.Parts[Place.DefinitionId])
        {
            case RipperPart.GrappleRipper:
                species.RunGrappleRipperMainAi(Place, frame);
                break;
            case RipperPart.JetRipper:
                species.RunJetRipperMainAi(Place, frame);
                break;
            case RipperPart.Ripper:
                species.RunRipperMainAi(Place, frame);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RipperBehavior.AdvanceRipperTypedAnimation(Place, context);

    /// <summary>The grapple Ripper and jet Ripper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? RipperBehavior.RipperShotReaction : base.ShotReaction;

    /// <summary>The grapple Ripper and jet Ripper's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? RipperBehavior.RipperBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The grapple Ripper and jet Ripper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
